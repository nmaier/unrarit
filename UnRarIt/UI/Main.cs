using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using UnRarIt.Archive;
using UnRarIt.Archive.Rar;
using UnRarIt.Archive.Zip;
using UnRarIt.Interop;

namespace UnRarIt
{
    public partial class Main : Form
    {
        private static Properties.Settings Config = Properties.Settings.Default;
        internal static string ToFormatedSize(long aSize)
        {
            return ToFormatedSize((ulong)aSize);
        }
        private static string ToFormatedSize(ulong aSize)
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


        private bool running = false;
        private bool aborted = false;
        private bool auto;
        private IFileIcon FileIcon = new FileIconWin();
        private PasswordList passwords = new PasswordList();

        public Main(bool aAuto, string dir, string[] args)
        {
            auto = aAuto;
            if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
            {
                Config.Dest = dir;
            }

            InitializeComponent();
            StateIcons.Images.Add(Properties.Resources.idle);
            StateIcons.Images.Add(Properties.Resources.done);
            StateIcons.Images.Add(Properties.Resources.error);
            StateIcons.Images.Add(Properties.Resources.run);

            if (!(UnrarIt.Enabled = !string.IsNullOrEmpty(Dest.Text)))
            {
                GroupDest.ForeColor = Color.Red;
            }

            About.Image = Icon.ToBitmap();

            RefreshPasswordCount();
            if (args != null && args.Length != 0)
            {
                AddFiles(args);
            }
            else
            {
                AdjustHeaders();
            }
            Status.Text = "Ready...";
            BrowseDestDialog.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

        }

        private void AdjustHeaders()
        {
            ColumnHeaderAutoResizeStyle style = ColumnHeaderAutoResizeStyle.ColumnContent;
            if (Files.Items.Count == 0)
            {
                style = ColumnHeaderAutoResizeStyle.HeaderSize;
            }
            foreach (ColumnHeader h in Files.Columns)
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

        private static Regex[] partFiles = new Regex[] {
            new Regex(@"(part0*[2-9]\d*)\.(?:rar|zip)$", RegexOptions.IgnoreCase),
            new Regex(@"\.(r|z)\d{2,}$", RegexOptions.IgnoreCase)
            };

        private void AddFiles(string[] aFiles)
        {
            Dictionary<string, ArchiveItem> seen = new Dictionary<string, ArchiveItem>();
            List<KeyValuePair<string, string>> parts = new List<KeyValuePair<string,string>>();
            
            Files.BeginUpdate();
            foreach (ArchiveItem i in Files.Items)
            {
                seen[i.FileName.ToLower()] = i;
            }
            foreach (string file in aFiles)
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

                    if (ConsumePartFile(parts, info))
                    {
                        continue;
                    }

                    string ext = info.Extension.ToLower();
                    if (ext != ".rar" && ext != ".zip")
                    {
                        continue;
                    }
                    if (seen.ContainsKey(info.FullName))
                    {
                        continue;
                    }

                    ArchiveItem item = new ArchiveItem(info.FullName);
                    seen[info.FullName.ToLower()] = item;
                    if (ext == ".zip")
                    {
                        item.Group = Files.Groups["GroupZip"];
                    }
                    else
                    {
                        item.Group = Files.Groups["GroupRar"];
                    }
                    if (!Icons.Images.ContainsKey(ext))
                    {
                        Icons.Images.Add(ext, FileIcon.GetIcon(info.FullName, ExtractIconSize.Small));
                    }
                    item.ImageKey = ext;
                    item.StateImageIndex = 0;
                    Files.Items.Add(item);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                }
            }
            foreach (KeyValuePair<string,string> part in parts) {
                if (!seen.ContainsKey(part.Key))
                {
                    continue;
                }
                seen[part.Key].AddPart(part.Value);
            }
            Files.EndUpdate();
            AdjustHeaders();
        }

        private static bool ConsumePartFile(List<KeyValuePair<string, string>> parts, FileInfo info)
        {
            foreach (Regex partFile in partFiles)
            {
                Match m = partFile.Match(info.FullName);
                if (!m.Success)
                {
                    continue;
                }
                string t = m.Groups[1].Value.ToLower();
                string f = info.FullName.ToLower();
                if (t.Length == 1)
                {
                    // old format
                    parts.Add(new KeyValuePair<string, string>(
                        Reimplement.CombinePath(info.DirectoryName.ToLower(), Path.GetFileNameWithoutExtension(f) + (t[0] == 'r' ? ".rar" : ".zip")),
                        info.FullName.ToLower()
                        ));
                }
                else
                {
                    // new format
                    parts.Add(new KeyValuePair<string, string>(
                        f.Replace(t, "part1"),
                        info.FullName.ToLower()
                        ));
                }
                return true;
            }
            return false;
        }

        private void Files_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        }

        private void Files_SelectedIndexChanged(object sender, EventArgs e)
        {

        }


        private void UnRarIt_Click(object sender, EventArgs e)
        {
            Run();
        }
        private void Abort_Click(object sender, EventArgs e)
        {
            UnrarIt.Enabled = false;
            aborted = true;
        }

        class Task
        {
            public AutoResetEvent Event = new AutoResetEvent(false);
            private Main Owner;
            public ArchiveItem Item;
            public IArchiveFile File;
            public string Result = string.Empty;
            public OverwriteAction Action = OverwriteAction.Unspecified;

            public Task(Main aOwner, ArchiveItem aItem, IArchiveFile aFile)
            {
                File = aFile;
                Item = aItem;
                Owner = aOwner;
                File.ExtractFile += OnExtractFile;
                File.PasswordAttempt += OnPasswordAttempt;
            }

            public ulong UnpackedSize = 0;
            public ulong ExtractedFiles = 0;

            private void OnExtractFile(object sender, ExtractFileEventArgs e)
            {
                Owner.BeginInvoke(
                    new SetStatus(delegate(string status) { Owner.Details.Text = status; }),
                    e.Item.Name
                    );
                UnpackedSize += e.Item.Size;
                ExtractedFiles++;
                e.ContinueOperation = !Owner.aborted;
            }
            private void OnPasswordAttempt(object sender, PasswordEventArgs e)
            {
                Owner.BeginInvoke(
                    new SetStatus(delegate(string status) { Owner.Details.Text = status; }),
                    String.Format("Password: {0}", e.Password)
                    );
                e.ContinueOperation = !Owner.aborted;
            }
        }

        private void Run()
        {
            actionRemembered = OverwriteAction.Unspecified;

            running = true;
            aborted = false;
            BrowseDest.Enabled = Exit.Enabled = OpenSettings.Enabled = AddPassword.Enabled = false;
            UnrarIt.Text = "Abort";

            UnrarIt.Click += Abort_Click;
            UnrarIt.Click -= UnRarIt_Click;

            Progress.Maximum = Files.Items.Count;
            Progress.Value = 0;

            List<Task> tasks = new List<Task>();

            foreach (ArchiveItem i in Files.Items)
            {
                if (aborted)
                {
                    break;
                }
                if (i.StateImageIndex == 1)
                {
                    continue;
                }
                if (!File.Exists(i.FileName))
                {
                    i.Status = "Error, File not found";
                    i.StateImageIndex = 2;
                    continue;
                }


                if (i.Group.Name == "GroupRar")
                {
                    tasks.Add(new Task(this, i, new RarArchiveFile(i.FileName)));
                }
                else
                {
                    tasks.Add(new Task(this, i, new ZipArchiveFile(i.FileName)));
                }
            }

            IEnumerator<Task> taskEnumerator = tasks.GetEnumerator();
            Dictionary<AutoResetEvent, Task> runningTasks = new Dictionary<AutoResetEvent, Task>();

            int threads = Math.Min(Environment.ProcessorCount, 4);

            for (; !aborted; )
            {
                while (runningTasks.Count < threads)
                {
                    if (!taskEnumerator.MoveNext())
                    {
                        break;
                    }
                    Task task = taskEnumerator.Current;
                    task.Item.StateImageIndex = 3;
                    task.Item.Status = "Processing...";
                    Thread thread = new Thread(HandleFile);
                    thread.Priority = ThreadPriority.BelowNormal;
                    thread.Start(task);
                    runningTasks[task.Event] = task;
                }
                Files.Columns[2].AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
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
                        Files.Columns[2].AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);

                        break;
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
                    }
                    else
                    {
                        task.Item.Status = String.Format("Error, {0}", task.Result.ToString());
                        task.Item.StateImageIndex = 2;
                    }
                    Files.Columns[2].AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
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

            Details.Text = "";
            Status.Text = "Ready...";
            Progress.Value = 0;
            BrowseDest.Enabled = Exit.Enabled = OpenSettings.Enabled = UnrarIt.Enabled = AddPassword.Enabled = true;
            running = false;
            UnrarIt.Text = "Unrar!";
            UnrarIt.Click += UnRarIt_Click;
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
            passwords.Save();
        }
        delegate void SetStatus(string NewStatus);

        private void HandleFile(object o)
        {
            Task task = o as Task;
            try
            {
                Invoke(
                    new SetStatus(delegate(string status) { Status.Text = status; }),
                    String.Format("Opening archive and cracking password: {0}...", task.File.Archive.Name)
                    );
                task.File.Open((passwords as IEnumerable<string>).GetEnumerator());
                Invoke(
                    new SetStatus(delegate(string status) { Status.Text = status; }),
                    String.Format("Extracting: {0}...", task.File.Archive.Name)
                );
                Regex skip = new Regex(@"\bthumbs.db$|\b__MACOSX\b|\bds_store\b|\bdxva_sig$|rapidpoint|\.(?:ion|pif|jbf)$", RegexOptions.IgnoreCase);
                List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();

                string minPath = null;
                uint items = 0;
                foreach (IArchiveEntry info in task.File)
                {
                    if (skip.IsMatch(info.Name))
                    {
                        continue;
                    }
                    ++items;
                    string path = Path.GetDirectoryName(info.Name);
                    if (minPath == null || minPath.Length > path.Length)
                    {
                        minPath = path;
                    }
                }
                if (minPath == null)
                {
                    minPath = string.Empty;
                }
                string basePath = string.Empty;
                if (items >= Config.OwnDirectoryLimit && string.IsNullOrEmpty(minPath))
                {
                    basePath = new Regex(@"(?:\.part\d+)?\.[r|z].{2}$", RegexOptions.IgnoreCase).Replace(task.File.Archive.Name, "");
                }
                foreach (IArchiveEntry info in task.File)
                {
                    if (skip.IsMatch(info.Name))
                    {
                        continue;
                    }
                    FileInfo dest = new FileInfo(Reimplement.CombinePath(Reimplement.CombinePath(Config.Dest, basePath), info.Name));
                    if (dest.Exists)
                    {
                        switch (Config.OverwriteAction)
                        {
                            case 1:
                                info.Destination = dest;
                                break;
                            case 2:
                                switch (OverwritePrompt(task, dest, info))
                                {
                                    case OverwriteAction.Overwrite:
                                        info.Destination = dest;
                                        break;
                                    case OverwriteAction.Rename:
                                        info.Destination = MakeUnique(dest);
                                        break;
                                }
                                break;
                            case 3:
                                info.Destination = MakeUnique(dest);
                                break;
                        }
                    }
                    else
                    {
                        info.Destination = dest;
                    }

                }
                task.File.Extract();
            }
            catch (RarException ex)
            {
                task.Result = ex.Result.ToString();
            }
            catch (Exception ex)
            {
                task.Result = ex.Message;
            }
            task.Event.Set();
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
        Mutex overwritePromptMutex = new Mutex();
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

        private void BrowseDest_Click(object sender, EventArgs e)
        {
            if (BrowseDestDialog.ShowDialog() == DialogResult.OK)
            {
                Config.Dest = BrowseDestDialog.SelectedPath;
                Config.Save();
                GroupDest.ForeColor = SystemColors.ControlText;
            }
        }

        private void Dest_TextChanged(object sender, EventArgs e)
        {
            UnrarIt.Enabled = !string.IsNullOrEmpty(Dest.Text);
        }

        private void Homepage_Click(object sender, EventArgs e)
        {
            Process.Start("http://tn123.ath.cx/UnRarIt/");
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
    }
}
