using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using UnRarIt.Interop;

namespace UnRarIt.Utils
{
    public class PartFileConsumer
    {
        private interface Consumer
        {
            bool Consume(FileInfo info, out string key);
        }
        private class OldSyntaxConsumer : Consumer
        {
            static Regex r = new Regex(
                @"\.(r|z)\d{2,}$",
                RegexOptions.IgnoreCase | RegexOptions.Compiled
                );
            
            public bool Consume(FileInfo info, out string key)
            {
                key = string.Empty;
                Match m = r.Match(info.Name);
                if (!m.Success)
                {
                    return false;
                }
                string format;
                switch (m.Groups[1].Value[0])
                {
                    case 'r':
                        format = ".rar";
                        break;
                    case 'z':
                        format = ".zip";
                        break;
                    default:
                        throw new NotImplementedException();
                }
                key = Reimplement.CombinePath(info.DirectoryName, Path.GetFileNameWithoutExtension(info.Name).ToLower() + format).ToLower();
                return true;
            }
        }
        private class SplitConsumer : Consumer
        {
            Regex r = new Regex(@"\.(\d+)$", RegexOptions.Compiled);
            public bool Consume(FileInfo info, out string key)
            {
                key = string.Empty; 
                Match m = r.Match(info.Name);
                if (!m.Success)
                {
                    return false;
                }
                string val = m.Groups[1].Value;
                uint i = 0;
                if (!uint.TryParse(val, out i) || i == 1)
                {
                    return false;
                }
                key = Reimplement.CombinePath(info.DirectoryName, String.Format("{0}.{1}1", Path.GetFileNameWithoutExtension(info.Name), new string('0', m.Groups[1].Length - 1))).ToLower();
                return true;
            }
        }

        private class PartConsumer : Consumer
        {
            static Regex r = new Regex(@"\.(part(\d+))\.(?:rar|zip)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            public bool Consume(FileInfo info, out string key)
            {
                key = string.Empty;
                Match m = r.Match(info.Name);
                if (!m.Success)
                {
                    return false;
                }
                string val = m.Groups[2].Value;
                uint i = 0;
                if (!uint.TryParse(val, out i) || i == 1)
                {
                    return false;
                }
                // new format
                key = Reimplement.CombinePath(info.DirectoryName, info.Name.Replace(m.Groups[1].Value, String.Format(
                    "part{0}",
                    1.ToString(new string('0', val.Length))
                ))).ToLower();
                return true;
            }
        }

        static private Consumer[] consumers = new Consumer[] { new SplitConsumer(), new PartConsumer(), new OldSyntaxConsumer() };

        List<KeyValuePair<string, string>> parts = new List<KeyValuePair<string, string>>();

        public List<KeyValuePair<string, string>> Parts
        {
            get { return parts; }
        }

        public bool Consume(FileInfo info)
        {
            foreach (Consumer c in consumers)
            {
                string key;
                if (c.Consume(info, out key))
                {
                    parts.Add(new KeyValuePair<string, string>(key, info.FullName));
                    return true;
                }
            }
            return false;
        }

    }
}
