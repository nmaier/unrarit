using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Diagnostics;

namespace UnRarIt
{
    public partial class Main : Form
    {
        private static PasswordList passwords = new PasswordList("passwords.txt");
        private static Properties.Settings Config = Properties.Settings.Default;
        private static string ToFormatedSize(long aSize)
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

        public Main()
        {
            InitializeComponent();
            RefreshPasswordCount();
            AddFiles(new string[] { @"E:\Beispielmusik.rar", @"G:\Stuff\Unsorted\BNC160.rar" });
            Status.Text = "Ready...";
            BrowseDestDialog.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            UnrarIt.Enabled = !string.IsNullOrEmpty(Dest.Text);
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
                        Icons.Images.Add(ext, Icon.ExtractAssociatedIcon(info.FullName));
                    }
                    item.ImageKey = ext;
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

        private void button1_Click(object sender, EventArgs e)
        {
            running = true;
            UnrarIt.Enabled = false;
            Progress.Maximum = Files.Items.Count;
            Progress.Value = 0;
            foreach (ListViewItem i in Files.Groups["GroupRar"].Items)
            {
                using (RarFile rf = new RarFile(i.Text, (passwords as IEnumerable<string>).GetEnumerator()))
                {
                    rf.ExtractFile += OnExtractFile;
                    Thread thread = new Thread(HandleFile);
                    thread.Start(rf);
                    while (!thread.Join(200))
                    {
                        Application.DoEvents();
                    }
                    if (!string.IsNullOrEmpty(rf.Password))
                    {
                        passwords.SetGood(rf.Password);
                    }
                }
                Progress.Increment(1);
            }

            Status.Text = "Ready...";
            Progress.Value = 0;
            UnrarIt.Enabled = true;
            running = false;
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
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
        private void OnExtractFile(object sender, ExtractFileEventArgs e)
        {
            Invoke(
                new SetStatus(delegate(string status) { Status.Text = status; }),
                String.Format("Extracting {0}::{1}...", e.Archive, e.FileName)
                );
        }
        private void HandleFile(object o)
        {
            RarFile file = o as RarFile;
            try
            {
                Invoke(
                    new SetStatus(delegate(string status) { Status.Text = status; }),
                    String.Format("Opening archive and cracking password: {0}...", file.FileName.Name)
                    );
                file.Open();
                foreach (RarItemInfo info in file)
                {
                    info.Destination = new FileInfo(Path.Combine(Path.GetTempPath(), info.FileName));
                }
                file.Extract();
            }
            catch (RarException ex)
            {
                MessageBox.Show(
                    String.Format("Rar Error processing: {0}\n{1}", file.FileName.Name, ex.Result.ToString()),
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                    );
            }
        }

        private void Status_Click(object sender, EventArgs e)
        {

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

        private void homepageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("http://tn123.ath.cx/UnRarIt/");
        }
    }
}
