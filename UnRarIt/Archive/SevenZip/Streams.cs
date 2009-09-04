using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace UnRarIt.Archive.SevenZip
{
    internal interface SevenZipStream : IInStream, ISequentialInStream, ISequentialOutStream, IDisposable
    {
    }

    internal class SevenZipNullStream : SevenZipStream
    {
        public uint Read(byte[] data, uint size)
        {
            return 0;
        }

        public ulong Seek(long offset, uint seekOrigin)
        {
            return 0;
        }

        public uint Write(byte[] data, uint size)
        {
            return size;
        }

        public void Dispose()
        {
        }
    }

    internal class SevenZipFileStream : SevenZipStream
    {
        FileStream stream;
        public SevenZipFileStream(FileInfo file, FileMode mode, FileAccess access)
        {
            if (!file.Directory.Exists && access != FileAccess.Read)
            {
                file.Directory.Create();
            }
            stream = new FileStream(file.FullName, mode, access);
        }

        public ulong Seek(long offset, uint seekOrigin)
        {
            return (ulong)stream.Seek(offset, (SeekOrigin)seekOrigin);
        }
        public uint Read(byte[] data, uint size)
        {
            return (uint)stream.Read(data, 0, (int)size);
        }
        public uint Write(byte[] data, uint size)
        {
            stream.Write(data, 0, (int)size);
            return size;
        }
        public void Dispose()
        {
            if (stream != null)
            {
                stream.Dispose();
                stream = null;
            }
        }
    }
}
