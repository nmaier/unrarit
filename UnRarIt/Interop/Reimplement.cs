using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace UnRarIt.Interop
{
    public class Reimplement
    {
        private Reimplement() {}
        public static string CleanFileName(string fileName)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException("fileName");
            }
            StringBuilder result = new StringBuilder();
            for (int i = 0, e = fileName.Length; i < e; ++i)
            {
                char ch = fileName[i];
                if (ch < 32 || ch == 34 || ch == 60 || ch == 62 || ch == 63 || ch == 127 || ch == 0x2a || ch == 0x3f)
                {
                    if (i == e - 1)
                    {
                        break;
                    }
                    result.Append('_');
                    continue;
                }
                result.Append(ch);
            }
            fileName = result.ToString().Trim();
            return fileName.Replace('/', '\\');
        }

            
        
        public static string CombinePath(string path1, string path2)
        {
            // Note: Reimplementation of Reimplement.CombinePath with subtle differences:
            // * Invalid chars will be replaced by underscores!
            // * Directory Seperators will be normalized

            if (path1 == null)
            {
                throw new ArgumentNullException("path1");
            }
            if (path2 == null)
            {
                throw new ArgumentNullException("path2");
            }
            if (string.IsNullOrEmpty(path1) && string.IsNullOrEmpty(path2))
            {
                throw new ArgumentException("path1 and path2 are empty!");
            }
            path1 = CleanFileName(path1);
            path2 = CleanFileName(path2);
            while ((path2.Length >= 1 && path2[0] == Path.DirectorySeparatorChar) || (path2.Length >= 2 && path2[1] == Path.VolumeSeparatorChar))
            {
                path2 = path2.Substring(1);
            }
            if (string.IsNullOrEmpty(path1))
            {
                return path2;
            }
            if (string.IsNullOrEmpty(path2))
            {
                return path1;
            }

            char ch = path1[path1.Length - 1];
            if (ch == Path.DirectorySeparatorChar) {
                return path1 + path2;
            }
            return path1 + Path.DirectorySeparatorChar + path2;
        }
        public static string GetFileName(string file)
        {
            int idx = file.LastIndexOfAny(new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar, Path.VolumeSeparatorChar });
            if (idx >= 0)
            {
                return file.Substring(idx);
            }
            return file;
        }
    }
}
