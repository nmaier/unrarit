using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using UnRarIt.Archive;
using UnRarIt.Archive.SevenZip;
using UnRarIt.Interop;
using UnRarIt.Utils;
using UnRarIt.Updater;
using System.Reflection;

namespace UnRarIt
{
    public partial class Main : Form, UpdateTarget
    {
        private static Properties.Settings Config = Properties.Settings.Default;
        private static PasswordList passwords = new PasswordList();
        private static Mutex overwritePromptMutex = new Mutex();

        private static Regex trimmer = new Regex(
            @"^[\s_-]+|^unrarit_|(?:\.part\d+)?\.(?:[r|z].{2}|7z)$|[\s_-]+$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled
            );
        private static Regex skipper = new Regex
            (@"^\._|\bthumbs.db$|\b__MACOSX\b|\bds_store\b|\bdxva_sig$|rapidpoint|\.(?:ion|pif|jbf|url|lnk|nfo)|^file_id.diz$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled
            );

        internal static string ToFormatedSize(long aSize)
        {
            return ToFormatedSize((ulong)aSize);
        }
        internal static string ToFormatedSize(ulong aSize)
        {
            if (aSize <= 900)
            {
                return String.Format("{0} Byte", aSize);
            }
            string[] fmts = { "KB", "MB", "GB", "TB" };
            string fmt = fmts[0];
            double size = aSize;

            for (int i = 0; i < fmts.Length && size > 900; ++i)
            {
                size /= 1024;
                fmt = fmts[i];
            }
            return String.Format("{0:F2} {1}", size, fmt);
        }
        private static FileInfo MakeUnique(FileInfo info)
        {
            if (!info.Exists)
            {
                return info;
            }
            string ext = info.Extension;
            string baseName = info.Name.Substring(0, info.Name.Length - info.Extension.Length);

            for (uint i = 1; info.Exists; ++i)
            {
                info = new FileInfo(Reimplement.CombinePath(info.DirectoryName, String.Format("{0}_{1}{2}", baseName, i, ext)));
            }

            return info;
        }
        private static DirectoryInfo MakeUnique(DirectoryInfo info)
        {
            if (!info.Exists)
            {
                return info;
            }
            string baseName = info.Name;
            for (uint i = 1; info.Exists; ++i)
            {
                info = new DirectoryInfo(Reimplement.CombinePath(info.Parent.FullName, String.Format("{0}_{1}", baseName, i)));
            }

            return info;
        }

        private bool running = false;
        private bool aborted = false;
        private bool stopped = false;
        private bool auto;

        public Version AppVersion { get { return Assembly.GetExecutingAssembly().GetName().Version; } }
        public Uri UpdateURI { get { return new Uri("https://tn123.org/UnRarIt/version"); } }

        private Updater.Updater updater;

        public Main(bool aAuto, string dir, string[] args)
        {
            auto = aAuto;

            InitializeComponent();

            Files.SmallImageList = Files.LargeImageList = ArchiveItem.Icons;
            StateIcons.Images.Add(Properties.Resources.idle);
            StateIcons.Images.Add(Properties.Resources.done);
            StateIcons.Images.Add(Properties.Resources.error);
            StateIcons.Images.Add(Properties.Resources.run);

#if ENABLE_SSE3
            Text = String.Format("{0} - {1}bit - {2}", Text, CpuInfo.isX64 ? 64 : 32, CpuInfo.hasSSE3 ? "SSE3/4" : "Generic");
#else
            Text = String.Format("{0} - {1}bit", Text, CpuInfo.isX64 ? 64 : 32);
#endif

            About.Image = Icon.ToBitmap();

            RefreshPasswordCount();
            BrowseDestDialog.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            CtxOpenDirectory.Image = FileIcon.GetIcon(BrowseDestDialog.SelectedPath, FileIconSize.Small);

            Dests.AddRange(Config.Destinations.ToArray());
            DestinationsChanged();

            if (!string.IsNullOrEmpty(dir))
            {
                Dests.Add(dir);
            }

            if (args != null && args.Length != 0)
            {
                AddFiles(args);
            }
            else
            {
                AdjustHeaders();
            }
        }

        private void DestinationsChanged()
        {
            if (!(UnrarIt.Enabled = Dests.Items.Count > 0))
            {
                GroupDest.ForeColor = Color.Red;
            }
            else
            {
                GroupDest.ForeColor = SystemColors.ControlText;
            }
        }

        private void AdjustHeaders()
        {
            ColumnHeaderAutoResizeStyle style = ColumnHeaderAutoResizeStyle.ColumnContent;
            if (Files.Items.Count == 0)
            {
                style = ColumnHeaderAutoResizeStyle.HeaderSize;
            }
            foreach (ColumnHeader h in new ColumnHeader[] { columnFile, columnSize })
            {
                h.AutoResize(style);
            }
        }

        private void RefreshPasswordCount()
        {
            StatusPasswords.Text = String.Format("{0} passwords...", passwords.Length);
        }

        private void Files_DragDrop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                return;
            }

            string[] dropped = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (dropped.Length == 0)
            {
                return;
            }
            AddFiles(dropped);

        }
        private bool AddFiles(string[] aFiles)
        {
            return AddFiles(aFiles, false);
        }
        private List<string> silentFiles = new List<string>();
        private bool AddFiles(string[] aFiles, bool nested)
        {
            Dictionary<string, ArchiveItem> seen = new Dictionary<string, ArchiveItem>();

            Files.BeginUpdate();
            foreach (ArchiveItem i in Files.Items)
            {
                seen[i.FileName.ToLower()] = i;
            }

            PartFileConsumer consumer = new PartFileConsumer();

            bool added = AddFiles(silentFiles, nested, seen, consumer);
            added |= AddFiles(aFiles, nested, seen, consumer);
            foreach (KeyValuePair<string, string> part in consumer.Parts)
            {
                if (!seen.ContainsKey(part.Key))
                {
                    silentFiles.Add(part.Value);
                    continue;
                }
                added = seen[part.Key].AddPart(part.Value) || added;
            }
            Files.EndUpdate();
            AdjustHeaders();
            return added;
        }
        private bool AddFiles(string[] aFiles, bool nested, Dictionary<string, ArchiveItem> seen, PartFileConsumer consumer)
        {
            List<string> files = new List<string>(aFiles);
            return AddFiles(files, nested, seen, consumer);
        }
        private bool AddFiles(List<string> aFiles, bool nested, Dictionary<string, ArchiveItem> seen, PartFileConsumer consumer)
        {
            bool added = false;
            string[] files = aFiles.ToArray();
            foreach (string file in files)
            {
                try
                {
                    FileInfo info = new FileInfo(file);
                    if (!info.Exists)
                    {
                        continue;
                    }
                    if ((info.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        continue;
                    }

                    if (consumer.Consume(info))
                    {
                        aFiles.Remove(file);
                        continue;
                    }

                    string ext = info.Extension.ToLower();
                    if (seen.ContainsKey(info.FullName.ToLower()))
                    {
                        continue;
                    }

                    ArchiveItem item;
                    if (ext == ".zip" || ext == ".jar" || ext == ".xpi")
                    {
                        item = new ArchiveItem(info.FullName, SevenZipArchiveFile.FormatZip, nested);
                        item.Group = Files.Groups["GroupZip"];
                    }
                    else if (ext == ".7z")
                    {
                        item = new ArchiveItem(info.FullName, SevenZipArchiveFile.FormatSevenZip, nested);
                        item.Group = Files.Groups["GroupSevenZip"];
                    }
                    else if (ext == ".rar")
                    {
                        item = new ArchiveItem(info.FullName, SevenZipArchiveFile.FormatRar, nested);
                        item.Group = Files.Groups["GroupRar"];
                    }
                    else if (ext == ".001")
                    {
                        item = new ArchiveItem(info.FullName, SevenZipArchiveFile.FormatSplit, nested);
                        item.Group = Files.Groups["GroupSplit"];
                    }
                    else
                    {
                        continue;
                    }
                    seen[info.FullName.ToLower()] = item;
                    added = true;
                    Files.Items.Add(item);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                }
            }
            return added;
        }

        private void Files_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        }

        public delegate void InvokeVoidDelegate();
        private void UnRarIt_Click(object sender, EventArgs e)
        {
            BeginInvoke(new InvokeVoidDelegate(Run));
        }
        private void Stop_Click(object sender, EventArgs e)
        {
            stopped = true;
            UnrarIt.Click -= Stop_Click;
            UnrarIt.Click += Abort_Click;
            UnrarIt.Image = UnRarIt.Properties.Resources.abort;
            UnrarIt.Text = "Abort";
        }
        private void Abort_Click(object sender, EventArgs e)
        {
            UnrarIt.Enabled = false;
            aborted = true;
        }

        private class Task : IDisposable
        {
            private Main owner;

            private ulong unpackedSize = 0;
            public ulong UnpackedSize
            {
                get { return unpackedSize; }
            }

            private ulong extractedFiles = 0;
            public ulong ExtractedFiles
            {
                get { return extractedFiles; }
            }

            private AutoResetEvent signal = new AutoResetEvent(false);
            public AutoResetEvent Signal
            {
                get { return signal; }
            }

            private ArchiveItem item;
            public ArchiveItem Item
            {
                get { return item; }
            }

            private IArchiveFile file;
            public IArchiveFile File
            {
                get { return file; }
            }

            public string Result = string.Empty;
            public OverwriteAction Action = OverwriteAction.Unspecified;

            private List<string> files = new List<string>();

            public List<string> Files
            {
                get { return files; }
            }

            public Task(Main aOwner, ArchiveItem aItem, IArchiveFile aFile)
            {
                file = aFile;
                item = aItem;
                owner = aOwner;
                file.ExtractFile += OnExtractFile;
                file.ExtractProgress += OnExtractProgress;
                file.PasswordAttempt += OnPasswordAttempt;
            }


            private void OnExtractFile(object sender, ExtractFileEventArgs e)
            {
                files.Add(e.Item.Destination.FullName);
                string fn = e.Item.Name;
                owner.BeginInvoke(
                    new SetStatus(delegate(string status) { Item.SubStatus = status; }),
                    String.Format("Extracting - {0}", fn)
                    );
                if (e.Stage == ExtractFileEventArgs.ExtractionStage.Done)
                {
                    unpackedSize += e.Item.Size;
                    extractedFiles++;
                }
                e.ContinueOperation = !owner.aborted;
            }
            private void OnExtractProgress(object sender, ExtractProgressEventArgs e)
            {
                float progress = (float)((double)e.Written / e.Total);
                string fn = e.File.Name;
                owner.BeginInvoke(
                    new SetStatus(delegate(string status) { Item.SubStatus = status; }),
                    String.Format("{0:0%} - {1}", progress, fn)
                    );
                e.ContinueOperation = !owner.aborted;
            }
            private void OnPasswordAttempt(object sender, PasswordEventArgs e)
            {
                owner.BeginInvoke(
                    new SetStatus(delegate(string status) { Item.SubStatus = status; }),
                    String.Format("Password: {0}", e.Password)
                    );
                e.ContinueOperation = !owner.aborted && !owner.stopped;
            }

            #region IDisposable Members

            public void Dispose()
            {
                files.Clear();
                file.Dispose();
            }

            #endregion
        }

        private void Run()
        {
            actionRemembered = OverwriteAction.Unspecified;

            running = true;
            aborted = false;
            stopped = false;
            BrowseDest.Enabled = Exit.Enabled = OpenSettings.Enabled = AddPassword.Enabled = false;

            UnrarIt.Click += Stop_Click;
            UnrarIt.Click -= UnRarIt_Click;
            UnrarIt.Image = UnRarIt.Properties.Resources.process_stop;
            UnrarIt.Text = "Stop";

            Progress.Visible = true;
            int threads = UnRarIt.Properties.Settings.Default.Threads;

            Progress.Value = 0;
            Progress.Maximum = 0;

            for (bool rerun = true; rerun && !stopped && !aborted; )
            {
                rerun = false;

                List<Task> tasks = new List<Task>();

                foreach (ArchiveItem i in Files.Items)
                {
                    if (aborted || stopped)
                    {
                        break;
                    }
                    if (i.StateImageIndex != 0)
                    {
                        continue;
                    }
                    if (!File.Exists(i.FileName))
                    {
                        i.Status = "Error, File not found";
                        i.StateImageIndex = 2;
                        continue;
                    }
                    tasks.Add(new Task(this, i, new SevenZipArchiveFile(i.File, i.Format, Properties.Settings.Default.Priority)));
                }
                IEnumerator<Task> taskEnumerator = tasks.GetEnumerator();
                Dictionary<AutoResetEvent, Task> runningTasks = new Dictionary<AutoResetEvent, Task>();

                Progress.Maximum += tasks.Count;

                for (; ; )
                {
                    while (!aborted && !stopped && runningTasks.Count < threads && taskEnumerator.MoveNext())
                    {
                        Task task = taskEnumerator.Current;
                        task.Item.StateImageIndex = 3;
                        task.Item.Status = "Processing...";
                        Thread thread = new Thread(HandleFile);
                        thread.Priority = ThreadPriority.Lowest;
                        thread.Start(task);
                        runningTasks[task.Signal] = task;
                    }
                    if (runningTasks.Count == 0)
                    {
                        break;
                    }
                    AutoResetEvent[] handles = new List<AutoResetEvent>(runningTasks.Keys).ToArray();
                    int idx = WaitHandle.WaitAny(handles, 100);
                    if (idx != WaitHandle.WaitTimeout)
                    {
                        AutoResetEvent evt = handles[idx];
                        Task task = runningTasks[evt];
                        runningTasks.Remove(evt);
                        if (aborted)
                        {
                            task.Item.Status = String.Format("Aborted");
                            task.Item.StateImageIndex = 2;

                            continue;
                        }
                        if (string.IsNullOrEmpty(task.Result))
                        {
                            if (!string.IsNullOrEmpty(task.File.Password))
                            {
                                passwords.SetGood(task.File.Password);
                            }
                            try
                            {
                                task.Item.ExcuteSuccessAction();
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Failed to rename/delete file:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                            task.Item.Checked = true;
                            task.Item.Status = String.Format("Done, {0} files, {1}{2}",
                                task.ExtractedFiles,
                                ToFormatedSize(task.UnpackedSize),
                                string.IsNullOrEmpty(task.File.Password) ? "" : String.Format(", {0}", task.File.Password)
                                );
                            task.Item.StateImageIndex = 1;
                            AdjustHeaders();
                        }
                        else
                        {
                            task.Item.Status = String.Format(
                                "Error, {0}{1}",
                                task.Result.ToString(),
                                string.IsNullOrEmpty(task.File.Password) ? "" : String.Format(", {0}", task.File.Password)
                                );
                            task.Item.StateImageIndex = 2;
                            AdjustHeaders();
                        }
                        if (Config.Nesting)
                        {
                            rerun = AddFiles(task.Files.ToArray(), true) || rerun;
                        }
                        task.Dispose();
                        Progress.Increment(1);
                    }
                    Application.DoEvents();
                }

                if (!aborted)
                {
                    Files.BeginUpdate();
                    switch (Config.EmptyListWhenDone)
                    {
                        case 1:
                            Files.Clear();
                            break;
                        case 2:
                            {
                                List<int> idx = new List<int>();
                                foreach (ArchiveItem i in Files.Items)
                                {
                                    if (i.StateImageIndex == 1)
                                    {
                                        idx.Add(i.Index);
                                    }
                                }
                                idx.Reverse();
                                foreach (int i in idx)
                                {
                                    Files.Items.RemoveAt(i);
                                }
                            }
                            break;

                    }
                    Files.EndUpdate();
                }
            }

            Details.Text = "";
            Progress.Value = 0;
            Progress.Visible = false;
            BrowseDest.Enabled = Exit.Enabled = OpenSettings.Enabled = UnrarIt.Enabled = AddPassword.Enabled = true;
            running = false;
            UnrarIt.Text = "Extract Files";
            UnrarIt.Image = UnRarIt.Properties.Resources.extract;
            UnrarIt.Click += UnRarIt_Click;
            UnrarIt.Click -= Stop_Click;
            UnrarIt.Click -= Abort_Click;
        }


        private void About_Click(object sender, EventArgs e)
        {
            new AboutBox().ShowDialog();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void Main_FormClosed(object sender, FormClosedEventArgs e)
        {
            Config.Destinations.Clear();
            foreach (string s in Dests.Items)
            {
                Config.Destinations.Add(s);
            }
            Config.Save();
            passwords.Save();
        }
        delegate void SetStatus(string NewStatus);

        static string CommonDirectoryPrefix(List<string> paths)
        {
            List<string> work = new List<string>(paths);
            string rv = null;
            for (; ; )
            {
                string dn = null;
                foreach (string p in work)
                {
                    string cd = Path.GetDirectoryName(p);
                    if (string.IsNullOrEmpty(cd))
                    {
                        dn = null;
                        break;
                    }
                    for (; ; )
                    {
                        string pd = Path.GetDirectoryName(cd);
                        if (string.IsNullOrEmpty(pd))
                        {
                            break;
                        }
                        cd = pd;
                    }
                    if (dn == null)
                    {
                        dn = cd;
                    }
                    else if (dn != cd)
                    {
                        dn = null;
                        break;
                    }
                }
                if (dn == null)
                {
                    break;
                }
                if (!string.IsNullOrEmpty(rv))
                {
                    rv += "\\";
                }
                rv += dn;
                work.Clear();
                foreach (string p in paths)
                {
                    work.Add(p.Substring(rv.Length + 1));
                }
                dn = null;
            }
            return rv;
        }

        static Regex silentSkip = new Regex(@":(?:encryptable)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private void HandleFile(object o)
        {
            Task task = o as Task;
            string DestsText = null;
            Invoke(new InvokeVoidDelegate(delegate() { DestsText = Dests.Text; }));
            try
            {
                Invoke(
                    new SetStatus(delegate(string status) { task.Item.SubStatus = status; }),
                    String.Format("Opening archive and cracking password: {0}...", task.File.Archive.Name)
                    );
                task.File.Open((passwords as IEnumerable<string>).GetEnumerator());
                Invoke(
                    new SetStatus(delegate(string status) { task.Item.SubStatus = status; }),
                    String.Format("Extracting: {0}...", task.File.Archive.Name)
                );
                List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();


                uint items = 0;
                List<string> paths = new List<string>();
                foreach (IArchiveEntry info in task.File)
                {
                    if (skipper.IsMatch(info.Name))
                    {
                        continue;
                    }
                    ++items;
                    paths.Add(info.Name);
                }
                string minPath = CommonDirectoryPrefix(paths);
                if (minPath == null)
                {
                    minPath = string.Empty;
                }
                string basePath = string.Empty;
                if (items >= Config.OwnDirectoryLimit)
                {
                    if (!string.IsNullOrEmpty(minPath))
                    {
                        basePath = minPath;
                        for (; ; )
                        {
                            string p = Path.GetDirectoryName(basePath);
                            if (string.IsNullOrEmpty(p))
                            {
                                break;
                            }
                            basePath = p;
                        }
                    }
                    if (string.IsNullOrEmpty(minPath))
                    {
                        basePath = trimmer.Replace(task.File.Archive.Name, "");
                        string tmpPath;
                        while ((tmpPath = trimmer.Replace(basePath, "")) != basePath)
                        {
                            basePath = tmpPath;
                        }
                    }
                }
                basePath = Utils.Utils.CleanName(basePath);

                bool shouldExtract = false;
                foreach (IArchiveEntry info in task.File)
                {
                    if (skipper.IsMatch(info.Name))
                    {
                        continue;
                    }

                    string name = info.Name;
                    if (!string.IsNullOrEmpty(minPath))
                    {
                        name = name.Substring(minPath.Length + 1);
                    }
                    string rootPath = DestsText;
                    if (task.Item.IsNested)
                    {
                        rootPath = task.File.Archive.Directory.FullName;
                    }
                    DirectoryInfo baseDirectory = new DirectoryInfo(Reimplement.CombinePath(rootPath, basePath));
                    if (!string.IsNullOrEmpty(basePath) && (OverwriteAction)Config.OverwriteAction == OverwriteAction.RenameDirectory)
                    {
                        baseDirectory = MakeUnique(baseDirectory);
                    }
                    task.Item.BaseDirectory = baseDirectory;

                    FileInfo dest;
                    string destPath = Reimplement.CombinePath(baseDirectory.FullName, name);
                    try
                    {
                        dest = new FileInfo(destPath);
                    }
                    catch (NotSupportedException)
                    {
                        if (!silentSkip.IsMatch(destPath))
                        {
                            Invoke(new InvokeVoidDelegate(delegate()
                            {
                                MessageBox.Show(this, String.Format("Invalid file path: {0}\nSkipping file", destPath));
                            }));
                        }
                        continue;
                    }
                    if (dest.Exists)
                    {
                        switch ((OverwriteAction)Config.OverwriteAction)
                        {
                            case OverwriteAction.Overwrite:
                                info.Destination = dest;
                                shouldExtract = true;
                                break;
                            default:
                                switch (OverwritePrompt(task, dest, info))
                                {
                                    case OverwriteAction.Overwrite:
                                        info.Destination = dest;
                                        shouldExtract = true;
                                        break;
                                    case OverwriteAction.RenameDirectory:
                                    case OverwriteAction.Rename:
                                        info.Destination = MakeUnique(dest);
                                        shouldExtract = true;
                                        break;
                                }
                                break;
                            case OverwriteAction.Rename:
                                info.Destination = MakeUnique(dest);
                                shouldExtract = true;
                                break;
                        }
                    }
                    else
                    {
                        shouldExtract = true;
                        info.Destination = dest;
                    }
                }
                if (shouldExtract)
                {
                    task.File.Extract();
                }
            }
            catch (ArchiveException ex)
            {
                task.Result = ex.Message;
            }
            catch (Exception ex)
            {
                task.Result = String.Format("Unexpected: {0} ({1})", ex.Message, typeof(Exception).ToString());
            }
            task.Signal.Set();
        }

        OverwriteAction actionRemembered = OverwriteAction.Unspecified;

        class OverwritePromptInfo
        {
            public Task task;
            public FileInfo dest;
            public IArchiveEntry entry;
            public OverwriteAction Action = OverwriteAction.Skip;
            public OverwritePromptInfo(Task aTask, FileInfo aDest, IArchiveEntry aEntry)
            {
                task = aTask;
                dest = aDest;
                entry = aEntry;
            }
        }

        delegate void OverwriteExecuteDelegate(OverwritePromptInfo aInfo);
        void OverwriteExecute(OverwritePromptInfo aInfo)
        {
            OverwriteForm form = new OverwriteForm(
                aInfo.dest.FullName,
                ToFormatedSize(aInfo.dest.Length),
                aInfo.entry.Name,
                ToFormatedSize(aInfo.entry.Size)
                );
            form.Owner = this;
            DialogResult dr = form.ShowDialog();
            aInfo.Action = form.Action;
            form.Dispose();

            switch (dr)
            {
                case DialogResult.Retry:
                    aInfo.task.Action = aInfo.Action;
                    break;
                case DialogResult.Abort:
                    actionRemembered = aInfo.Action;
                    break;
            }
        }

        private OverwriteAction OverwritePrompt(Task task, FileInfo Dest, IArchiveEntry Entry)
        {
            overwritePromptMutex.WaitOne();
            OverwriteAction rv = OverwriteAction.Unspecified;
            try
            {
                if (task.Action != OverwriteAction.Unspecified)
                {
                    return task.Action;
                }
                if (actionRemembered != OverwriteAction.Unspecified)
                {
                    return actionRemembered;
                }
                OverwritePromptInfo oi = new OverwritePromptInfo(task, Dest, Entry);
                Invoke(new OverwriteExecuteDelegate(OverwriteExecute), oi);
                rv = oi.Action;
            }
            finally
            {
                overwritePromptMutex.ReleaseMutex();
            }
            return rv;

        }

        private void AddPassword_Click(object sender, EventArgs e)
        {
            AddPasswordForm apf = new AddPasswordForm();
            DialogResult dr = apf.ShowDialog();
            switch (dr)
            {
                case DialogResult.OK:
                    passwords.SetGood(apf.Password.Text.Trim());
                    break;
                case DialogResult.Yes:
                    passwords.AddFromFile(apf.Password.Text);
                    break;
            }
            RefreshPasswordCount();
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            if ((e.Cancel = running))
            {
                MessageBox.Show("Wait for the operation to complete", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private bool browsedBefore = false;
        private void BrowseDest_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(Dests.Text) && !browsedBefore)
            {
                BrowseDestDialog.SelectedPath = Dests.Text;
                browsedBefore = true;
            }
            if (BrowseDestDialog.ShowDialog() == DialogResult.OK)
            {
                Dests.Add(BrowseDestDialog.SelectedPath);
                DestinationsChanged();
            }
        }

        private void Dest_TextChanged(object sender, EventArgs e)
        {
            UnrarIt.Enabled = !string.IsNullOrEmpty(Dests.Text);
        }

        private void Homepage_Click(object sender, EventArgs e)
        {
            Process.Start("https://tn123.org/UnRarIt/");
        }

        private void OpenSettings_Click(object sender, EventArgs e)
        {
            new SettingsForm().ShowDialog();
        }

        private void Main_Shown(object sender, EventArgs e)
        {
            if (auto)
            {
                auto = false;
                Run();
                Close();
                return;
            }

            updater = new Updater.Updater(this);
            updater.OnHasUpdate += HasUpdate;
            updater.Check();
        }
        private void HasUpdate(Version NewVersion, Uri InfoURI)
        {
            if (MessageBox.Show(
                this,
                String.Format("New version {0} is available!\n\nVisit {1}?", NewVersion, InfoURI),
                "Update available",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Information
                ) == DialogResult.Yes)
            {
                Process.Start(InfoURI.ToString());
            }
        }

        private void ExportPasswords_Click(object sender, EventArgs e)
        {
            if (ExportDialog.ShowDialog() == DialogResult.OK)
            {
                passwords.SaveToFile(ExportDialog.FileName);
            }
        }

        private void ClearAllPasswords_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Do you really want to clear all passwords?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
            {
                passwords.Clear();
                RefreshPasswordCount();
            }
        }

        private void License_Click(object sender, EventArgs e)
        {
            string license = Reimplement.CombinePath(Path.GetDirectoryName(Application.ExecutablePath), "license.rtf");
            if (!File.Exists(license))
            {
                MessageBox.Show("License file not found", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                Process.Start(license);
            }
        }

        private void CtxClearList_Click(object sender, EventArgs e)
        {
            Files.Items.Clear();
            AdjustHeaders();
        }

        private void CtxClearSelected_Click(object sender, EventArgs e)
        {
            Files.BeginUpdate();
            foreach (ArchiveItem item in Files.SelectedItems)
            {
                item.Remove();
            }
            Files.EndUpdate();
            AdjustHeaders();
        }

        private void requeueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Files.BeginUpdate();
            foreach (ArchiveItem item in Files.SelectedItems)
            {
                item.StateImageIndex = 0;
            }
            Files.EndUpdate();
        }

        private void CtxDeleteFiles_Click(object sender, EventArgs e)
        {
            Files.BeginUpdate();
            foreach (ArchiveItem item in Files.SelectedItems)
            {
                item.DeleteFiles();
            }
            Files.EndUpdate();
            AdjustHeaders();
        }

        private void Files_KeyPress(object sender, KeyPressEventArgs e)
        {
        }

        private void Files_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Control)
            {
                switch (e.KeyCode)
                {
                    case Keys.A:
                        Files.BeginUpdate();
                        foreach (ListViewItem i in Files.Items)
                        {
                            i.Selected = true;
                        }
                        Files.EndUpdate();
                        e.Handled = true;
                        break;
                }
            }
        }

        private void requeueFailedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in Files.Items)
            {
                if (item.StateImageIndex == 2)
                {
                    item.StateImageIndex = 0;
                }
            }
        }

        private void CtxOpenDirectory_Click(object sender, EventArgs e)
        {
            if (Files.SelectedItems.Count == 0)
            {
                return;
            }
            ArchiveItem item = Files.SelectedItems[0] as ArchiveItem;
            if (item == null)
            {
                return;
            }
            Process.Start(item.BaseDirectory.FullName);
        }

        private void FilesCtx_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            bool hasItems = Files.SelectedItems.Count != 0;
            CtxOpenDirectory.Enabled = hasItems;
            if (!hasItems)
            {
                return;
            }
            ArchiveItem item = Files.SelectedItems[0] as ArchiveItem;
            if (item == null)
            {
                CtxOpenDirectory.Enabled = false;
                return;
            }
            CtxOpenDirectory.Image = FileIcon.GetIcon(item.BaseDirectory.FullName, FileIconSize.Small);
        }

        private void clearDestsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Dests.Items.Clear();
            DestinationsChanged();
        }

        private void checkForUpdatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (updater != null)
                updater.Check(UpdateCheckType.Forced);
        }
    }
}
