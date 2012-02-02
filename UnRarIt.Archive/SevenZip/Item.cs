using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnRarIt.Interop;
using System.Text.RegularExpressions;

namespace UnRarIt.Archive.SevenZip
{
    internal class SevenZipItemInfo : IArchiveEntry
    {
        public ulong CompressedSize
        {
            get { return compressedSize; }
        }

        public uint Crc
        {
            get { return crc; }
        }

        public DateTime DateTime
        {
            get { throw new NotImplementedException(); }
        }

        private FileInfo destination = null;

        public FileInfo Destination
        {
            get { return destination; }
            set { destination = value; }
        }

        public bool IsCrypted
        {
            get { return isCrypted; }
        }

        public string Name
        {
            get { return name; }
        }

        public ulong Size
        {
            get { return size; }
        }

        public uint Version
        {
            get { throw new NotImplementedException(); }
        }

        private string name;
        private uint crc;
        private bool isCrypted;
        private ulong size;
        private ulong compressedSize;
        static private Regex regPreClean = new Regex(@"^((?:\\|/)?(?:images|bilder|DH|set|cd(?:a|b|\d+))(?:\\|/))", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        internal SevenZipItemInfo(string aName, uint aCrc, bool aIsCrypted, ulong aSize, ulong aCompressedSize)
        {
            name = aName;
            while (true)
            {
                Match m = regPreClean.Match(name);
                if (!m.Success)
                {
                    break;
                }
                name = name.Substring(m.Value.Length);
            }
            while (name.Length != 0 && name[0] == Path.DirectorySeparatorChar || name[0] == Path.AltDirectorySeparatorChar)
            {
                name = name.Substring(1);
            }
            name = Reimplement.CleanFileName(name);
            crc = aCrc;
            isCrypted = aIsCrypted;
            size = aSize;
            compressedSize = aCompressedSize;
        }
    }
}
