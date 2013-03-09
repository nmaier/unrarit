using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using UnRarIt.Archive;
using UnRarIt.Interop;
using UnRarIt.Updater;
using UnRarIt.Utilities;

namespace UnRarIt
{
  public partial class Main : Form, IUpdateTarget
  {
    private bool aborted = false;

    private OverwriteAction actionRemembered = OverwriteAction.Unspecified;

    private readonly static Properties.Settings Config = Properties.Settings.Default;

    private static Mutex overwritePromptMutex = new Mutex();

    private static PasswordList passwords = new PasswordList();

    private readonly List<string> silentFiles = new List<string>();

    private readonly static Regex silentSkip = new Regex(@":(?:encryptable)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly static Regex skipper = new Regex(@"^\._|\bthumbs.db$|\b__MACOSX\b|\bds_store\b|\bdxva_sig$|rapidpoint|\.(?:ion|pif|jbf|url|lnk|nfo)|^file_id.diz$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private readonly static Regex trimmer = new Regex(@"^[\s_-]+|^unrarit_|(?:\.part\d+)?\.(?:[r|z].{2}|7z)$|[\s_-]+$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private bool auto;

    private bool browsedBefore = false;

    private bool running = false;

    private bool stopped = false;

    private UpdateChecker updater;


    public Main(bool autoProcess, string dir, string[] args)
    {
      auto = autoProcess;

      InitializeComponent();

      Files.SmallImageList = Files.LargeImageList = ArchiveItem.Icons;
      StateIcons.Images.Add(Properties.Resources.idle);
      StateIcons.Images.Add(Properties.Resources.done);
      StateIcons.Images.Add(Properties.Resources.error);
      StateIcons.Images.Add(Properties.Resources.run);
      StateIcons.Images.Add(Properties.Resources.warning);

      Text = String.Format(CultureInfo.CurrentCulture, "{0} - {1}bit", Text, CpuInfo.IsX64 ? 64 : 32);

      About.Image = Icon.ToBitmap();

      RefreshPasswordCount();
      BrowseDestDialog.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
      CtxOpenDirectory.Image = FileIcon.GetIcon(BrowseDestDialog.SelectedPath, FileIconSize.Small);

      Dests.AddRange(Config.Destinations.ToArray());
      DestinationsChanged();

      if (!string.IsNullOrEmpty(dir)) {
        Dests.Add(dir);
      }

      if (args != null && args.Length != 0) {
        AddFiles(args);
      }
      else {
        AdjustHeaders();
      }
    }


    private delegate void OverwriteExecuteDelegate(OverwritePromptInfo aInfo);
    private delegate void SetStatus(string NewStatus);


    private delegate void InvokeVoidDelegate();


    public Version AppVersion
    {
      get {
        return Assembly.GetExecutingAssembly().GetName().Version;
      }
    }
    public Uri UpdateUri
    {
      get {
        return new Uri("https://tn123.org/UnRarIt/version");
      }
    }


    private void Abort_Click(object sender, EventArgs e)
    {
      UnrarIt.Enabled = false;
      aborted = true;
    }

    private void About_Click(object sender, EventArgs e)
    {
      using (var a = new AboutBox())
        a.ShowDialog();
    }

    private bool AddFiles(string[] aFiles)
    {
      return AddFiles(aFiles, false);
    }

    private bool AddFiles(string[] aFiles, bool nested)
    {
      var seen = new Dictionary<string, ArchiveItem>();

      Files.BeginUpdate();
      foreach (ArchiveItem i in Files.Items) {
        seen[i.FileName.ToUpperInvariant()] = i;
      }

      var consumer = new PartFileConsumer();

      var added = AddFiles(silentFiles, nested, seen, consumer);
      added |= AddFiles(aFiles, nested, seen, consumer);
      foreach (var p in consumer.Parts) {
        var parts = p.Value;
        var files = parts.Files;
        if (!parts.HasPart(p.Key)) {
          foreach (var i in files) {
            if (!seen.ContainsKey(i.FullName.ToUpperInvariant())) {
              silentFiles.Add(i.FullName);
            }
          }
          continue;
        }

        var file = files[0];
        files.RemoveAt(0);
        ArchiveItem item;
        if (!seen.TryGetValue(file.FullName.ToUpperInvariant(), out item)) {
          item = CreateItem(nested, file);
          Files.Items.Add(item);
        }
        foreach (var i in files) {
          item.AddPart(i);
        }
        if (!parts.IsComplete) {
          item.StateImageIndex = 3;
          item.SubStatus = "Missing parts! Re-queue at your own risk!";
          item.StateImageIndex = 4;
        }
        else if (item.StateImageIndex == 4) {
          item.StateImageIndex = 3;
          item.SubStatus = "Ready...";
          item.StateImageIndex = 0;
        }
        added = true;
      }
      Files.EndUpdate();
      AdjustHeaders();
      return added;
    }

    private bool AddFiles(string[] aFiles, bool nested, Dictionary<string, ArchiveItem> seen, PartFileConsumer consumer)
    {
      var files = new List<string>(aFiles);
      return AddFiles(files, nested, seen, consumer);
    }

    private bool AddFiles(List<string> aFiles, bool nested, Dictionary<string, ArchiveItem> seen, PartFileConsumer consumer)
    {
      var added = false;
      var files = aFiles.ToArray();
      foreach (string file in files) {
        try {
          var info = new FileInfo(file);
          if (!info.Exists) {
            continue;
          }
          if ((info.Attributes & FileAttributes.Directory) == FileAttributes.Directory) {
            continue;
          }

          if (consumer.Consume(info)) {
            aFiles.Remove(file);
            continue;
          }

          var ext = info.Extension.ToUpperInvariant();
          if (seen.ContainsKey(info.FullName.ToUpperInvariant())) {
            continue;
          }

          ArchiveItem item = CreateItem(nested, info, ext);
          if (item == null) {
            continue;
          }
          seen[info.FullName.ToUpperInvariant()] = item;
          added = true;
          Files.Items.Add(item);
        }
        catch (Exception ex) {
          Console.Error.WriteLine(ex);
        }
      }
      return added;
    }
    private ArchiveItem CreateItem(bool nested, FileInfo info)
    {
      return CreateItem(nested, info, info.Extension.ToUpperInvariant());
    }

    private ArchiveItem CreateItem(bool nested, FileInfo info, string ext)
    {
      ArchiveItem item = null;
      if (ext == ".ZIP" || ext == ".JAR" || ext == ".XPI") {
        item = new ArchiveItem(info.FullName, ArchiveFile.FormatZip, nested);
        item.Group = Files.Groups["GroupZip"];
      }
      else
        if (ext == ".7Z") {
          item = new ArchiveItem(info.FullName, ArchiveFile.FormatSevenZip, nested);
          item.Group = Files.Groups["GroupSevenZip"];
        }
        else
          if (ext == ".RAR") {
            item = new ArchiveItem(info.FullName, ArchiveFile.FormatRar, nested);
            item.Group = Files.Groups["GroupRar"];
          }
          else
            if (ext == ".001") {
              item = new ArchiveItem(info.FullName, ArchiveFile.FormatSplit, nested);
              item.Group = Files.Groups["GroupSplit"];
            }
      return item;
    }

    private void AddPassword_Click(object sender, EventArgs e)
    {
      using (var apf = new AddPasswordForm()) {
        var dr = apf.ShowDialog();
        switch (dr) {
          case DialogResult.OK:
            passwords.SetGood(apf.Password.Text.Trim());
            break;
          case DialogResult.Yes:
            passwords.AddFromFile(apf.Password.Text);
            break;
          default:
            break;
        }
      }
      RefreshPasswordCount();
    }

    private void AdjustHeaders()
    {
      var style = ColumnHeaderAutoResizeStyle.ColumnContent;
      if (Files.Items.Count == 0) {
        style = ColumnHeaderAutoResizeStyle.HeaderSize;
      }
      foreach (ColumnHeader h in new ColumnHeader[] { columnFile, columnSize }) {
        h.AutoResize(style);
      }
    }

    private void BrowseDest_Click(object sender, EventArgs e)
    {
      if (!string.IsNullOrEmpty(Dests.Text) && !browsedBefore) {
        BrowseDestDialog.SelectedPath = Dests.Text;
        browsedBefore = true;
      }
      if (BrowseDestDialog.ShowDialog() == DialogResult.OK) {
        Dests.Add(BrowseDestDialog.SelectedPath);
        DestinationsChanged();
      }
    }

    private void checkForUpdatesToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (updater != null) {
        updater.Check(UpdateCheckType.Forced);
      }
    }

    private void ClearAllPasswords_Click(object sender, EventArgs e)
    {
      if (MessageBox.Show("Do you really want to clear all passwords?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes) {
        passwords.Clear();
        RefreshPasswordCount();
      }
    }

    private void clearDestsToolStripMenuItem_Click(object sender, EventArgs e)
    {
      Dests.Items.Clear();
      DestinationsChanged();
    }

    private static string CommonDirectoryPrefix(List<string> paths)
    {
      var work = new List<string>(paths);
      string rv = null;
      for (; ; ) {
        string dn = null;
        foreach (string p in work) {
          var cd = Path.GetDirectoryName(p);
          if (string.IsNullOrEmpty(cd)) {
            dn = null;
            break;
          }
          for (; ; ) {
            var pd = Path.GetDirectoryName(cd);
            if (string.IsNullOrEmpty(pd)) {
              break;
            }
            cd = pd;
          }
          if (dn == null) {
            dn = cd;
          }
          else {
            if (dn != cd) {
              dn = null;
              break;
            }
          }
        }
        if (dn == null) {
          break;
        }
        if (!string.IsNullOrEmpty(rv)) {
          rv += "\\";
        }
        rv += dn;
        work.Clear();
        foreach (string p in paths) {
          work.Add(p.Substring(rv.Length + 1));
        }
        dn = null;
      }
      return rv;
    }

    private void CtxClearList_Click(object sender, EventArgs e)
    {
      Files.Items.Clear();
      AdjustHeaders();
    }

    private void CtxClearSelected_Click(object sender, EventArgs e)
    {
      Files.BeginUpdate();
      foreach (ArchiveItem item in Files.SelectedItems) {
        item.Remove();
      }
      Files.EndUpdate();
      AdjustHeaders();
    }

    private void CtxDeleteFiles_Click(object sender, EventArgs e)
    {
      Files.BeginUpdate();
      foreach (ArchiveItem item in Files.SelectedItems) {
        item.DeleteFiles();
      }
      Files.EndUpdate();
      AdjustHeaders();
    }

    private void CtxOpenDirectory_Click(object sender, EventArgs e)
    {
      if (Files.SelectedItems.Count == 0) {
        return;
      }
      var item = Files.SelectedItems[0] as ArchiveItem;
      if (item == null) {
        return;
      }
      Process.Start(item.BaseDirectory.FullName);
    }

    private void DecompressDirectory_Click(object sender, EventArgs e)
    {
      AddFiles((from s in new DirectoryInfo(Dests.Text).GetFiles()
                                    select s.FullName).ToArray());
      UnRarIt_Click(sender, e);
    }

    private void DestinationsChanged()
    {
      if (!(UnrarIt.Enabled = Dests.Items.Count > 0)) {
        GroupDest.ForeColor = Color.Red;
      }
      else {
        GroupDest.ForeColor = SystemColors.ControlText;
      }
    }

    private void exitToolStripMenuItem_Click(object sender, EventArgs e)
    {
      Close();
    }

    private void ExportPasswords_Click(object sender, EventArgs e)
    {
      if (ExportDialog.ShowDialog() == DialogResult.OK) {
        passwords.SaveToFile(ExportDialog.FileName);
      }
    }

    private void Files_DragDrop(object sender, DragEventArgs e)
    {
      if (!e.Data.GetDataPresent(DataFormats.FileDrop)) {
        return;
      }

      var dropped = e.Data.GetData(DataFormats.FileDrop) as string[];
      if (dropped.Length == 0) {
        return;
      }
      AddFiles(dropped);
    }

    private void Files_DragEnter(object sender, DragEventArgs e)
    {
      e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
    }

    private void Files_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.Modifiers == Keys.Control) {
        switch (e.KeyCode) {
          case Keys.A:
            Files.BeginUpdate();
            foreach (ListViewItem i in Files.Items) {
              i.Selected = true;
            }
            Files.EndUpdate();
            e.Handled = true;
            break;
          default:
            break;
        }
      }
    }

    private void FilesCtx_Opening(object sender, System.ComponentModel.CancelEventArgs e)
    {
      var hasItems = Files.SelectedItems.Count != 0;
      CtxOpenDirectory.Enabled = hasItems;
      if (!hasItems) {
        return;
      }
      var item = Files.SelectedItems[0] as ArchiveItem;
      if (item == null) {
        CtxOpenDirectory.Enabled = false;
        return;
      }
      CtxOpenDirectory.Image = FileIcon.GetIcon(item.BaseDirectory.FullName, FileIconSize.Small);
    }

    private void HandleFile(object o)
    {
      var task = o as Task;
      string DestsText = GetDest();
      try {
        Invoke(
          new SetStatus(status =>
        {
          task.Item.SubStatus = status;
        }),
          String.Format(CultureInfo.CurrentCulture, "Opening archive and cracking password: {0}...", task.File.Archive.Name)
        );
        task.File.Open((passwords as IEnumerable<string>).GetEnumerator());
        Invoke(
          new SetStatus(status =>
        {
          task.Item.SubStatus = status;
        }),
          String.Format(CultureInfo.CurrentCulture, "Extracting: {0}...", task.File.Archive.Name)
        );


        var paths = new List<string>();
        foreach (IArchiveEntry info in task.File) {
          if (skipper.IsMatch(info.Name)) {
            continue;
          }
          paths.Add(info.Name);
        }
        var minPath = CommonDirectoryPrefix(paths);
        if (minPath == null) {
          minPath = string.Empty;
        }
        var basePath = FindBasePath(task, paths.Count, minPath);

        var shouldExtract = false;
        foreach (IArchiveEntry info in task.File) {
          if (skipper.IsMatch(info.Name)) {
            continue;
          }

          var name = info.Name;
          if (!string.IsNullOrEmpty(minPath)) {
            name = name.Substring(minPath.Length + 1);
          }
          var rootPath = DestsText;
          if (task.Item.IsNested) {
            rootPath = task.File.Archive.Directory.FullName;
          }
          var baseDirectory = new DirectoryInfo(Replacements.CombinePath(rootPath, basePath));
          if (!string.IsNullOrEmpty(basePath) && (OverwriteAction)Config.OverwriteAction == OverwriteAction.RenameDirectory) {
            baseDirectory = MakeUnique(baseDirectory);
          }
          task.Item.BaseDirectory = baseDirectory;

          FileInfo dest;
          var destPath = Replacements.CombinePath(baseDirectory.FullName, name);
          try {
            dest = new FileInfo(destPath);
          }
          catch (NotSupportedException) {
            if (!silentSkip.IsMatch(destPath)) {
              Invoke(new InvokeVoidDelegate(() =>
              {
                MessageBox.Show(this, String.Format(CultureInfo.CurrentCulture, "Invalid file path: {0}\nSkipping file", destPath));
              }));
            }
            continue;
          }
          shouldExtract = SetupExtractDest(task, info, dest);
        }
        if (shouldExtract) {
          task.File.Extract();
        }
      }
      catch (ArchiveException ex) {
        task.Result = ex.Message;
      }
      catch (Exception ex) {
        task.Result = String.Format(CultureInfo.CurrentCulture, "Unexpected: {0} ({1})", ex.Message, typeof(Exception));
      }
      task.Signal.Set();
    }

    private bool SetupExtractDest(Task task, IArchiveEntry info, FileInfo dest)
    {
      bool rv = false;
      if (dest.Exists) {
        switch ((OverwriteAction)Config.OverwriteAction) {
          case OverwriteAction.Overwrite:
            info.Destination = dest;
            rv = true;
            break;
          default:
            switch (OverwritePrompt(task, dest, info)) {
              case OverwriteAction.Overwrite:
                info.Destination = dest;
                rv = true;
                break;
              case OverwriteAction.RenameDirectory:
              case OverwriteAction.Rename:
                info.Destination = MakeUnique(dest);
                rv = true;
                break;
              default:
                break;
            }
            break;
          case OverwriteAction.Rename:
            info.Destination = MakeUnique(dest);
            rv = true;
            break;
        }
      }
      else {
        rv = true;
        info.Destination = dest;
      }
      return rv;
    }

    private string GetDest()
    {
      string DestsText = null;
      Invoke(new InvokeVoidDelegate(() =>
      {
        DestsText = Dests.Text;
      }));
      return DestsText;
    }

    private static string FindBasePath(Task task, int items, string minPath)
    {
      var basePath = string.Empty;
      if (items >= Config.OwnDirectoryLimit) {
        if (!string.IsNullOrEmpty(minPath)) {
          basePath = minPath;
          for (; ; ) {
            var p = Path.GetDirectoryName(basePath);
            if (string.IsNullOrEmpty(p)) {
              break;
            }
            basePath = p;
          }
        }
        if (string.IsNullOrEmpty(minPath)) {
          basePath = trimmer.Replace(task.File.Archive.Name, string.Empty);
          string tmpPath;
          while ((tmpPath = trimmer.Replace(basePath, string.Empty)) != basePath) {
            basePath = tmpPath;
          }
        }
      }
      basePath = Tools.CleanName(basePath);
      return basePath;
    }

    private void HasUpdate(object sender, UpdateEventArgs e)
    {
      if (MessageBox.Show(
          this,
          String.Format(CultureInfo.CurrentCulture, "New version {0} is available!\n\nVisit {1}?", e.NewVersion, e.InfoUri),
          "Update available",
          MessageBoxButtons.YesNo,
          MessageBoxIcon.Information
          ) == DialogResult.Yes) {
        Process.Start(e.InfoUri.ToString());
      }
    }

    private void Homepage_Click(object sender, EventArgs e)
    {
      Process.Start("https://tn123.org/UnRarIt/");
    }

    private void License_Click(object sender, EventArgs e)
    {
      var license = Replacements.CombinePath(Path.GetDirectoryName(Application.ExecutablePath), "license.rtf");
      if (!File.Exists(license)) {
        MessageBox.Show("License file not found", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
      else {
        Process.Start(license);
      }
    }

    private void Main_FormClosed(object sender, FormClosedEventArgs e)
    {
      Config.Destinations.Clear();
      foreach (string s in Dests.Items) {
        Config.Destinations.Add(s);
      }
      Config.Save();
      passwords.Save();
    }

    private void Main_FormClosing(object sender, FormClosingEventArgs e)
    {
      if ((e.Cancel = running)) {
        MessageBox.Show("Wait for the operation to complete", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
      }
    }

    private void Main_Shown(object sender, EventArgs e)
    {
      if (auto) {
        auto = false;
        Run();
        Close();
        return;
      }

      updater = new UpdateChecker(this);
      updater.OnHasUpdate += HasUpdate;
      updater.Check();
    }

    private static DirectoryInfo MakeUnique(DirectoryInfo info)
    {
      if (!info.Exists) {
        return info;
      }
      var baseName = info.Name;
      for (var i = 1; info.Exists; ++i) {
        info = new DirectoryInfo(Replacements.CombinePath(info.Parent.FullName, String.Format(CultureInfo.InvariantCulture, "{0}_{1}", baseName, i)));
      }

      return info;
    }

    private static FileInfo MakeUnique(FileInfo info)
    {
      if (!info.Exists) {
        return info;
      }
      var ext = info.Extension;
      var baseName = info.Name.Substring(0, info.Name.Length - info.Extension.Length);

      for (var i = 1; info.Exists; ++i) {
        info = new FileInfo(Replacements.CombinePath(info.DirectoryName, String.Format(CultureInfo.CurrentCulture, "{0}_{1}{2}", baseName, i, ext)));
      }

      return info;
    }

    private void OpenSettings_Click(object sender, EventArgs e)
    {
      using (var sf = new SettingsForm())
        sf.ShowDialog();
    }

    private void OverwriteExecute(OverwritePromptInfo aInfo)
    {
      DialogResult dr;
      using (var form = new OverwriteForm(
          aInfo.dest.FullName,
          aInfo.dest.Length.ToFormattedSize(),
          aInfo.entry.Name,
          aInfo.entry.Size.ToFormattedSize()
          )) {
        form.Owner = this;
        dr = form.ShowDialog();
        aInfo.Action = form.Action;
      }

      switch (dr) {
        case DialogResult.Retry:
          aInfo.task.Action = aInfo.Action;
          break;
        case DialogResult.Abort:
          actionRemembered = aInfo.Action;
          break;
        default:
          break;
      }
    }

    private OverwriteAction OverwritePrompt(Task task, FileInfo Dest, IArchiveEntry Entry)
    {
      overwritePromptMutex.WaitOne();
      try {
        if (task.Action != OverwriteAction.Unspecified) {
          return task.Action;
        }
        if (actionRemembered != OverwriteAction.Unspecified) {
          return actionRemembered;
        }
        using (var oi = new OverwritePromptInfo(task, Dest, Entry)) {
          Invoke(new OverwriteExecuteDelegate(OverwriteExecute), oi);
          return oi.Action;
        }
      }
      finally {
        overwritePromptMutex.ReleaseMutex();
      }
    }

    private void RefreshPasswordCount()
    {
      StatusPasswords.Text = String.Format(CultureInfo.CurrentCulture, "{0} passwords...", passwords.Length);
    }

    private void requeueFailedToolStripMenuItem_Click(object sender, EventArgs e)
    {
      foreach (ListViewItem item in Files.Items) {
        if (item.StateImageIndex == 2) {
          item.StateImageIndex = 0;
        }
      }
    }

    private void requeueToolStripMenuItem_Click(object sender, EventArgs e)
    {
      Files.BeginUpdate();
      foreach (ArchiveItem item in Files.SelectedItems) {
        item.StateImageIndex = 0;
      }
      Files.EndUpdate();
    }

    private void Run()
    {
      actionRemembered = OverwriteAction.Unspecified;

      running = true;
      aborted = false;
      stopped = false;
      DecompressDirectory.Enabled =
        BrowseDest.Enabled =
        Exit.Enabled =
        OpenSettings.Enabled =
        AddPassword.Enabled =
        false;

      UnrarIt.Click += Stop_Click;
      UnrarIt.Click -= UnRarIt_Click;
      UnrarIt.Image = UnRarIt.Properties.Resources.process_stop;
      UnrarIt.Text = "Stop";

      Progress.Visible = true;
      var threads = UnRarIt.Properties.Settings.Default.Threads;

      Progress.Value = 0;
      Progress.Maximum = 0;

      RunTasks(threads);

      Details.Text = string.Empty;
      Progress.Value = 0;
      Progress.Visible = false;
      DecompressDirectory.Enabled =
        BrowseDest.Enabled =
        Exit.Enabled =
        OpenSettings.Enabled =
        UnrarIt.Enabled =
        AddPassword.Enabled =
        true;
      running = false;
      UnrarIt.Text = "Extract Files";
      UnrarIt.Image = UnRarIt.Properties.Resources.extract;
      UnrarIt.Click += UnRarIt_Click;
      UnrarIt.Click -= Stop_Click;
      UnrarIt.Click -= Abort_Click;
    }

    private void RunTasks(int threads)
    {
      for (var rerun = true; rerun && !stopped && !aborted; ) {
        rerun = false;

        var tasks = CollectTasks();
        var taskEnumerator = tasks.GetEnumerator();
        var runningTasks = new Dictionary<AutoResetEvent, Task>();

        Progress.Maximum += tasks.Count;

        for (; ; ) {
          while (!aborted && !stopped && runningTasks.Count < threads && taskEnumerator.MoveNext()) {
            var task = taskEnumerator.Current;
            task.Item.StateImageIndex = 3;
            task.Item.Status = "Processing...";
            var thread = new Thread(HandleFile) { Priority = ThreadPriority.Lowest };
            thread.Start(task);
            runningTasks[task.Signal] = task;
          }
          if (runningTasks.Count == 0) {
            break;
          }
          var handles = new List<AutoResetEvent>(runningTasks.Keys).ToArray();
          var idx = WaitHandle.WaitAny(handles, 100);
          if (idx != WaitHandle.WaitTimeout) {
            var evt = handles[idx];
            var task = runningTasks[evt];
            runningTasks.Remove(evt);
            if (aborted) {
              task.Item.Status = "Aborted";
              task.Item.StateImageIndex = 2;

              continue;
            }
            if (string.IsNullOrEmpty(task.Result)) {
              if (!string.IsNullOrEmpty(task.File.Password)) {
                passwords.SetGood(task.File.Password);
              }
              try {
                task.Item.ExcuteSuccessAction();
              }
              catch (Exception ex) {
                MessageBox.Show("Failed to rename/delete file:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
              }
              task.Item.Checked = true;
              task.Item.Status = String.Format(
                CultureInfo.CurrentCulture,
                "Done, {0} files, {1}{2}",
                task.ExtractedFiles,
                task.UnpackedSize.ToFormattedSize(),
                string.IsNullOrEmpty(task.File.Password)
                  ? string.Empty
                  : string.Format(CultureInfo.CurrentCulture, ", {0}", task.File.Password)
                 );
              task.Item.StateImageIndex = 1;
              AdjustHeaders();
            }
            else {
              task.Item.Status = String.Format(
                CultureInfo.CurrentCulture,
                "Error, {0}{1}",
                task.Result,
                string.IsNullOrEmpty(task.File.Password)
                  ? string.Empty
                  : string.Format(CultureInfo.CurrentCulture, ", {0}", task.File.Password)
                 );
              task.Item.StateImageIndex = 2;
              AdjustHeaders();
            }
            if (Config.Nesting) {
              rerun = AddFiles(task.Files.ToArray(), true) || rerun;
            }
            task.Dispose();
            Progress.Increment(1);
          }
          Application.DoEvents();
        }

        if (!aborted) {
          Files.BeginUpdate();
          switch (Config.EmptyListWhenDone) {
            case 1:
              Files.Clear();
              break;
            case 2:
              var idx = new List<int>();
              foreach (ArchiveItem i in Files.Items) {
                if (i.StateImageIndex == 1) {
                  idx.Add(i.Index);
                }
              }
              idx.Reverse();
              foreach (int i in idx) {
                Files.Items.RemoveAt(i);
              }
              break;
            default:
              break;
          }
          Files.EndUpdate();
        }
      }
    }

    private List<Task> CollectTasks()
    {
      var tasks = new List<Task>();

      foreach (ArchiveItem i in Files.Items) {
        if (aborted || stopped) {
          break;
        }
        if (i.StateImageIndex != 0) {
          continue;
        }
        if (!File.Exists(i.FileName)) {
          i.Status = "Error, File not found";
          i.StateImageIndex = 2;
          continue;
        }
        tasks.Add(new Task(this, i, i.File, i.Format, Properties.Settings.Default.Priority));
      }
      return tasks;
    }

    private void Stop_Click(object sender, EventArgs e)
    {
      stopped = true;
      UnrarIt.Click -= Stop_Click;
      UnrarIt.Click += Abort_Click;
      UnrarIt.Image = UnRarIt.Properties.Resources.abort;
      UnrarIt.Text = "Abort";
    }

    private void UnRarIt_Click(object sender, EventArgs e)
    {
      BeginInvoke(new InvokeVoidDelegate(Run));
    }


    private class OverwritePromptInfo : IDisposable
    {
      public OverwriteAction Action = OverwriteAction.Skip;
      public FileInfo dest;
      public IArchiveEntry entry;
      public Task task;


      public OverwritePromptInfo(Task aTask, FileInfo aDest, IArchiveEntry aEntry)
      {
        task = aTask;
        dest = aDest;
        entry = aEntry;
      }

      public void Dispose()
      {
      }
    }

    private sealed class Task : IDisposable
    {
      private ulong extractedFiles = 0;

      private readonly IArchiveFile file;

      private readonly ArchiveItem item;

      private readonly Main owner;

      private ulong unpackedSize = 0;


      public OverwriteAction Action = OverwriteAction.Unspecified;
      private readonly List<string> files = new List<string>();
      public string Result = string.Empty;
      private readonly AutoResetEvent signal = new AutoResetEvent(false);


      public Task(Main aOwner, ArchiveItem aItem, FileInfo aFile, Guid aFormat, ThreadIOPriority aPriority)
      {
        file = new ArchiveFile(aFile, aFormat, aPriority);
        item = aItem;
        owner = aOwner;
        file.ExtractFile += OnExtractFile;
        file.ExtractProgress += OnExtractProgress;
        file.PasswordAttempt += OnPasswordAttempt;
      }


      public ulong ExtractedFiles
      {
        get {
          return extractedFiles;
        }
      }
      public IArchiveFile File
      {
        get {
          return file;
        }
      }
      public List<string> Files
      {
        get {
          return files;
        }
      }
      public ArchiveItem Item
      {
        get {
          return item;
        }
      }
      public AutoResetEvent Signal
      {
        get {
          return signal;
        }
      }
      public ulong UnpackedSize
      {
        get {
          return unpackedSize;
        }
      }


      private void OnExtractFile(object sender, ExtractFileEventArgs e)
      {
        files.Add(e.Item.Destination.FullName);
        var fn = e.Item.Name;
        owner.BeginInvoke
        (new SetStatus(status =>
        {
          Item.SubStatus = status;
        }),
          String.Format(CultureInfo.CurrentCulture, "Extracting - {0}", fn)
        );
        if (e.Stage == ExtractionStage.Done) {
          unpackedSize += e.Item.Size;
          extractedFiles++;
        }
        e.ContinueOperation = !owner.aborted;
      }

      private void OnExtractProgress(object sender, ExtractProgressEventArgs e)
      {
        var progress = (float)((double)e.Written / e.Total);
        var fn = e.File.Name;
        owner.BeginInvoke(
          new SetStatus(status =>
        {
          Item.SubStatus = status;
        }),
          String.Format(CultureInfo.CurrentCulture, "{0:0%} - {1}", progress, fn)
        );
        e.ContinueOperation = !owner.aborted;
      }

      private void OnPasswordAttempt(object sender, PasswordEventArgs e)
      {
        owner.BeginInvoke(
          new SetStatus(status =>
        {
          Item.SubStatus = status;
        }),
          String.Format(CultureInfo.CurrentCulture, "Password: {0}", e.Password)
        );
        e.ContinueOperation = !owner.aborted && !owner.stopped;
      }


      public void Dispose()
      {
        files.Clear();
        file.Dispose();
        signal.Dispose();
      }
    }
  }
}
