using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

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
        internal SevenZipItemInfo(string aName, uint aCrc, bool aIsCrypted, ulong aSize, ulong aCompressedSize)
        {
            name = aName;
            crc = aCrc;
            isCrypted = aIsCrypted;
            size = aSize;
            compressedSize = aCompressedSize;
        }
    }
}
