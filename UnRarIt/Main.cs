using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace UnRarIt
{
    public partial class Main : Form
    {
        private static PasswordList passwords = new PasswordList("passwords.txt");

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

        public Main()
        {
            InitializeComponent();
            StatusPasswords.Text = String.Format("{0} passwords...", passwords.Length);
            AddFiles(new string[] { @"E:\Beispielmusik.rar", @"G:\Stuff\Unsorted\BNC160.rar" });
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
            foreach (ListViewItem i in Files.Groups["GroupRar"].Items)
            {
                try
                {
                    using (RarFile rf = new RarFile(i.Text, (passwords as IEnumerable<string>).GetEnumerator()))
                    {
                        if (!string.IsNullOrEmpty(rf.Password))
                        {
                            passwords.SetGood(rf.Password);
                        }
                        StringWriter w = new StringWriter();
                        w.WriteLine("{0}: {1}", rf.FileName, rf.Password);
                        foreach (RarItemInfo info in rf)
                        {
                            w.WriteLine("{0}: {1} {2}", info.FileName, info.UnpackedSize, info.IsEncrypted ? "*" : "");
                        }
                        MessageBox.Show(w.ToString());
                    }
                }
                catch (RarException ex) {
                    MessageBox.Show(
                        String.Format("Rar Error processing: {0}\n{1}", i.Text, ex.Result.ToString()),
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                        );
                }
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "Written 2009 by Nils Maier - Copyrights are disclaimed - Public domain\n\nUnRar and #ZipLib are copyrighted and available under free-to-use or completely free licenses",
                "About",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
                );
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void Main_FormClosed(object sender, FormClosedEventArgs e)
        {
            passwords.Save();
        }
    }
}
