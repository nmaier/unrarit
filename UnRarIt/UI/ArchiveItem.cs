using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using UnRarIt.Interop;

namespace UnRarIt
{
    internal class ArchiveItem : ListViewItem
    {
        internal static ImageList Icons = new ImageList();
        private static IFileIcon FileIcon = new FileIconWin();

        private static Properties.Settings Config = Properties.Settings.Default;

        Dictionary<FileInfo, bool> parts = new Dictionary<FileInfo, bool>();
        FileInfo file = null;
        public ArchiveItem(string aFileName)
        {
            SubItems.Add(string.Empty);
            SubItems.Add("Ready...");
            file = new FileInfo(aFileName);
            if (!Icons.Images.ContainsKey(file.Extension))
            {
                Icons.Images.Add(file.Extension, FileIcon.GetIcon(file.FullName, ExtractIconSize.Small));
            }
            ImageKey = file.Extension;
            StateImageIndex = 0;

            Invalidate();
        }

        protected void Invalidate()
        {
            Text = file.Name + (parts.Count != 0 ? String.Format(" +{0} parts", parts.Count) : "");

            if (file.Exists)
            {
                ulong size = 0;
                size += (ulong)file.Length;
                foreach (FileInfo part in parts.Keys)
                {
                    if (part.Exists)
                    {
                        size += (ulong)part.Length;
                    }
                }
                FileSize = Main.ToFormatedSize(size);
            }
            else
            {
                FileSize = "missing";
            }
        }
        private new string Text
        {
            get { return base.Text; }
            set { base.Text = value; }
        }

        public string FileName
        {
            get { return file.FullName; }
            set { file = new FileInfo(value); Invalidate(); }
        }
        public string FileSize
        {
            get { return SubItems[1].Text; }
            private set { SubItems[1].Text = value; }
        }
        public string Status
        {
            get { return SubItems[2].Text; }
            set { SubItems[2].Text = value; }
        }
        public string SubStatus
        {
            set
            {
                if (StateImageIndex != 3)
                {
                    return;
                }
                SubItems[2].Text = value;
            }
        }
        public FileInfo File
        {
            get { return file; }
        }

        internal void AddPart(string Part)
        {
            FileInfo pf = new FileInfo(Part);
            if (!pf.Exists)
            {
                return;
            }
            parts[pf] = true;
            Invalidate();
        }

        internal void ExcuteSuccessAction()
        {
            switch (Config.SuccessAction)
            {
                case 1:
                    file = Rename(file);
                    {
                        Dictionary<FileInfo, bool> oldParts = parts;
                        parts = new Dictionary<FileInfo, bool>();
                        foreach (FileInfo part in oldParts.Keys)
                        {
                            parts[Rename(part)] = true;
                        }
                    }
                    break;
                case 2:
                    DeleteFiles();
                    break;
            }
            Invalidate();
        }

        internal void DeleteFiles()
        {
            if (file.Exists)
            {
                file.Delete();
            }
            foreach (FileInfo part in parts.Keys)
            {
                if (part.Exists)
                {
                    part.Delete();
                }
            }
            parts.Clear();
        }

        private static Regex renamer = new Regex("^unrarit_", RegexOptions.Compiled);
        internal static FileInfo Rename(FileInfo aFile)
        {
            if (renamer.IsMatch(aFile.Name))
            {
                return aFile;
            }
            aFile.MoveTo(Reimplement.CombinePath(aFile.Directory.FullName, String.Format("unrarit_{0}", aFile.Name)));
            return aFile;
        }

    }
}
