using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using UnRarIt.Interop;

namespace UnRarIt
{

    internal class ArchiveItem : ListViewItem, IEnumerable<FileInfo>
    {
        internal static ImageList Icons = new ImageList();
        private static IFileIcon FileIcon = new FileIconWin();
        private static Properties.Settings Config = Properties.Settings.Default;

        Dictionary<string, FileInfo> parts = new Dictionary<string, FileInfo>();
        FileInfo file;
        Guid format;
        bool nested = false;

        public bool IsNested
        {
            get { return nested; }
        }

        public ArchiveItem(string aFileName, Guid aFormat, bool aNested)
        {
            format = aFormat;
            nested = aNested;

            SubItems.Add(string.Empty);
            SubItems.Add("Ready...");
            file = new FileInfo(aFileName);
            parts.Add(aFileName, file);
            if (!Icons.Images.ContainsKey(file.Extension))
            {
                Icons.Images.Add(file.Extension, FileIcon.GetIcon(file.FullName, ExtractIconSize.Small));
            }
            ImageKey = file.Extension;
            StateImageIndex = 0;

            Invalidate(); 
        }

        internal Guid Format
        {
            get { return format; }
        }

        protected void Invalidate()
        {
            Text = file.Name + (parts.Count > 1 ? String.Format(" +{0} parts", parts.Count - 1) : "");

            if (file.Exists)
            {
                ulong size = 0;
                foreach (FileInfo part in parts.Values)
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

        internal bool AddPart(string Part)
        {
            FileInfo pf = new FileInfo(Part);
            if (!pf.Exists)
            {
                return false;
            }
            if (!parts.ContainsKey(pf.FullName.ToLower()))
            {
                parts[pf.FullName.ToLower()] = pf;
                Invalidate();
                return true;
            }
            return false;
        }

        internal void ExcuteSuccessAction()
        {
            if (IsNested)
            {
                DeleteFiles();
                Remove();
            }
            else
            {
                switch (Config.SuccessAction)
                {
                    case 1:
                        {
                            Dictionary<string, FileInfo> oldParts = parts;
                            parts = new Dictionary<string, FileInfo>();
                            foreach (FileInfo part in oldParts.Values)
                            {
                                FileInfo newPart = Rename(part);
                                parts[newPart.FullName.ToLower()] = newPart;
                            }
                        }
                        break;
                    case 2:
                        DeleteFiles();
                        break;
                }
            }
            Invalidate();
        }

        internal void DeleteFiles()
        {
            if (file.Exists)
            {
                file.Delete();
            }
            foreach (FileInfo part in parts.Values)
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

        #region IEnumerable<FileInfo> Members

        public IEnumerator<FileInfo> GetEnumerator()
        {
            return parts.Values.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return parts.Values.GetEnumerator();
        }

        #endregion
    }
}
