using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using Emmy.Interop;

namespace UnRarIt
{
    public partial class Main : Form
    {
        private static Properties.Settings Config = Properties.Settings.Default;
        private static string ToFormatedSize(long aSize)
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

        private bool running = false;
        private bool aborted = false;
        private bool auto;
        private IFileIcon FileIcon = new FileIconWin();
        private PasswordList passwords = new PasswordList();

        public Main(bool aAuto, string[] args)
        {
            auto = aAuto;

            InitializeComponent();
            StateIcons.Images.Add(Properties.Resources.idle);
            StateIcons.Images.Add(Properties.Resources.done);
            StateIcons.Images.Add(Properties.Resources.error);

            UnrarIt.Enabled = !string.IsNullOrEmpty(Dest.Text);
            if (!UnrarIt.Enabled)
            {
                Dest.ForeColor = Color.Red;
            }

            About.Image = Icon.ToBitmap();

            RefreshPasswordCount();
            if (args != null && args.Length != 0)
            {
                AddFiles(args);
            }
            Status.Text = "Ready...";
            BrowseDestDialog.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
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

        private void AddFiles(string[] aFiles)
        {
            Files.BeginUpdate();
            Dictionary<string, bool> seen = new Dictionary<string, bool>();
            foreach (ListViewItem i in Files.Items)
            {
                seen[i.Text] = true;
            }
            foreach (string file in aFiles)
            {
                try
                {
                    FileInfo info = new FileInfo(file);

                    string ext = info.Extension.ToLower();
                    if (ext != ".rar" && ext != ".zip")
                    {
                        continue;
                    }
                    if (seen.ContainsKey(info.FullName))
                    {
                        continue;
                    }
                    seen[info.FullName] = true;

                    if ((info.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        continue;
                    }
                    ListViewItem item = new ListViewItem();
                    item.Text = info.FullName;
                    item.SubItems.Add(ToFormatedSize(info.Length));
                    item.SubItems.Add("Ready");
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
            Files.EndUpdate();
            foreach (ColumnHeader h in Files.Columns)
            {
                h.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
            }
        }

        private void Files_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        }

        private void Files_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        class Task
        {
            public IArchiveFile File;
            public string Result = string.Empty;
            public Task(IArchiveFile aFile)
            {
                File = aFile;
            }
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

        private void Run()
        {
            running = true;
            aborted = false;
            BrowseDest.Enabled = Exit.Enabled = OpenSettings.Enabled = AddPassword.Enabled = false;
            UnrarIt.Text = "Abort";

            UnrarIt.Click += Abort_Click;
            UnrarIt.Click -= UnRarIt_Click;

            Progress.Maximum = Files.Items.Count;
            Progress.Value = 0;

            foreach (ListViewItem i in Files.Groups["GroupRar"].Items)
            {
                if (aborted)
                {
                    break;
                }
                if (i.StateImageIndex == 1)
                {
                    continue;
                }
                using (IArchiveFile rf = new RarFile(i.Text))
                {
                    HandleItem(i, rf);
                }
                Progress.Increment(1);
            }
            foreach (ListViewItem i in Files.Groups["GroupZip"].Items)
            {
                if (aborted)
                {
                    break;
                }
                if (i.StateImageIndex == 1)
                {
                    continue;
                }
                using (IArchiveFile rf = new ZipArchiveFile(i.Text))
                {
                    HandleItem(i, rf);
                }
                Progress.Increment(1);
            }

            Details.Text = "";
            Status.Text = "Ready...";
            Progress.Value = 0;
            BrowseDest.Enabled = Exit.Enabled = OpenSettings.Enabled = UnrarIt.Enabled = AddPassword.Enabled = true;
            running = false;
            UnrarIt.Text = "Unrar!";
            UnrarIt.Click += UnRarIt_Click;
            UnrarIt.Click -= Abort_Click;

            Files.BeginUpdate();
            switch (Config.EmptyListWhenDone)
            {
                case 1:
                    foreach (ListViewItem i in Files.CheckedItems)
                    {
                        Files.Items.Remove(i);
                    }
                    break;
                case 2:
                    Files.Clear();
                    break;
            }
            Files.EndUpdate();
        }

        private void HandleItem(ListViewItem i, IArchiveFile rf)
        {
            unpackedSize = 0;
            files = 0;

            rf.ExtractFile += OnExtractFile;
            rf.PasswordAttempt += OnPasswordAttempt;

            Thread thread = new Thread(HandleFile);
            Task task = new Task(rf);
            thread.Start(task);
            while (!thread.Join(200))
            {
                Application.DoEvents();
            }
            if (!string.IsNullOrEmpty(rf.Password))
            {
                passwords.SetGood(rf.Password);
            }
            if (!aborted)
            {
                switch (Config.SuccessAction)
                {
                    case 1:
                        rf.Archive.MoveTo(Path.Combine(rf.Archive.Directory.FullName, String.Format("unrarit_{0}", rf.Archive.Name)));
                        break;
                    case 2:
                        rf.Archive.Delete();
                        break;
                }
                if (string.IsNullOrEmpty(task.Result))
                {
                    i.Checked = true;
                    i.SubItems[2].Text = String.Format("Done, {0} files, {1}", files, ToFormatedSize(unpackedSize));
                    i.StateImageIndex = 1;
                }
                else
                {
                    i.SubItems[2].Text = String.Format("Error, {0}", task.Result.ToString());
                    i.StateImageIndex = 2;
                }
            }
            else
            {
                i.SubItems[2].Text = String.Format("Aborted");
                i.StateImageIndex = 2;
            }

            Files.Columns[2].AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
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

        ulong unpackedSize = 0;
        ulong files = 0;

        private void OnExtractFile(object sender, ExtractFileEventArgs e)
        {
            Invoke(
                new SetStatus(delegate(string status) { Details.Text = status; }),
                e.Item.Name
                );
            unpackedSize += e.Item.Size;
            files++;
            e.ContinueOperation = !aborted;
        }
        private void OnPasswordAttempt(object sender, PasswordEventArgs e)
        {
            Invoke(
                new SetStatus(delegate(string status) { Details.Text = status; }),
                String.Format("Password: {0}", e.Password)
                );
            e.ContinueOperation = !aborted;
        }
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
                Regex skip = new Regex(@"\bthumbs.db$|\b_macosx\b|\b\.ds_store\b|\bdxva_sig$|rapidpoint|\.(?:ion|pif|jbf)$", RegexOptions.IgnoreCase);
                List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();

                string maxPath = null;
                uint items = 0;
                foreach (IArchiveEntry info in task.File)
                {
                    if (skip.IsMatch(info.Name))
                    {
                        continue;
                    }
                    ++items;
                    int idx = info.Name.LastIndexOfAny(new char[] { '\\', '/' });
                    if (idx != -1)
                    {
                        string path = info.Name.Substring(0, idx);
                        if (maxPath == null || maxPath.Length > path.Length)
                        {
                            maxPath = path;
                        }
                    }
                }
                if (maxPath == null)
                {
                    maxPath = string.Empty;
                }
                string basePath = string.Empty;
                if (items >= Config.OwnDirectoryLimit && string.IsNullOrEmpty(maxPath))
                {
                    basePath = new Regex(@"(?:\.part\d+)?\.[r|z].{2}$", RegexOptions.IgnoreCase).Replace(task.File.Archive.Name, "");
                }
                foreach (IArchiveEntry info in task.File)
                {
                    if (skip.IsMatch(info.Name))
                    {
                        continue;
                    }
                    FileInfo dest = new FileInfo(Path.Combine(Path.Combine(Config.Dest, basePath), info.Name));
                    if (dest.Exists)
                    {
                        switch (Config.OverwriteAction)
                        {
                            case 1:
                                info.Destination = dest;
                                break;
                            case 2:
                                switch (MessageBox.Show(
                                    String.Format(
                                    "{0} already exists.\nOverwrite?\n\nCurrent file:\t{1} bytes\nArchive File:\t{2} bytes",
                                    dest.FullName,
                                    dest.Length,
                                    info.Size
                                    ),
                                    "Confirm overwrite",
                                    MessageBoxButtons.YesNoCancel,
                                    MessageBoxIcon.Question
                                    ))
                                {
                                    case DialogResult.OK:
                                        info.Destination = dest;
                                        break;
                                    case DialogResult.Cancel:
                                        return;
                                }
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
    }
}
