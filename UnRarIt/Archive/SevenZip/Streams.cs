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
        protected FileStream stream;
        public SevenZipFileStream(FileInfo file, FileMode mode)
        {
            Init(file, mode, FileAccess.Read, 4194304);
        }
        public SevenZipFileStream(FileInfo file, FileMode mode, FileAccess access)
        {
            Init(file, mode, access, 4194304);
        }
        public SevenZipFileStream(FileInfo file, FileMode mode, FileAccess access, long buffering)
        {
            Init(file, mode, access, (int)Math.Min(buffering, 4194304));
        }
        private void Init(FileInfo file, FileMode mode, FileAccess access, int buffering)
        {
            if (!file.Directory.Exists && access != FileAccess.Read)
            {
                file.Directory.Create();
            }
            stream = new FileStream(file.FullName, mode, access, FileShare.Read, buffering);
        }

        public ulong Seek(long offset, uint seekOrigin)
        {
            try
            {
                return (ulong)stream.Seek(offset, (SeekOrigin)seekOrigin);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public uint Read(byte[] data, uint size)
        {
            try
            {
                return (uint)stream.Read(data, 0, (int)size);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public uint Write(byte[] data, uint size)
        {
            try
            {
                stream.Write(data, 0, (int)size);
                return size;
            }
            catch (Exception ex)
            {
                throw ex;
            }
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
    internal class SevenZipOutFileStream : SevenZipFileStream
    {
        public SevenZipOutFileStream(FileInfo file, long size)
            : base(file, FileMode.Create, FileAccess.ReadWrite, size)
        {
            if (size > 0)
            {
                stream.SetLength(size);
                stream.Flush();
            }
        }
    }
}
