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
        virtual public uint Write(byte[] data, uint size)
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
        const uint PROGRESS_SIZE = 25 * 1024 * 1024;
        const uint FLUSH_SIZE = 250 * 1024 * 1024;

        long written = 0;
        long fileSize;
        uint flush = FLUSH_SIZE;
        uint progress = PROGRESS_SIZE;
        FileInfo file;
        public SevenZipOutFileStream(FileInfo aFile, long aSize)
            : base(aFile, FileMode.Create, FileAccess.ReadWrite)
        {
            file = aFile;
            fileSize = aSize;
            if (fileSize > 0)
            {
                stream.SetLength(fileSize);
                stream.Flush();
            }
        }
        override public uint Write(byte[] data, uint size)
        {
            uint rv = base.Write(data, size);
            flush -= Math.Min(rv, flush);
            if (flush == 0)
            {
                stream.Flush();
                flush = FLUSH_SIZE;
            }
            written += rv;
            
            if (ProgressHandler != null)
            {
                progress -= Math.Min(rv, progress);
                if (progress == 0)
                {
                    ProgressHandler(this, file, written, fileSize);
                    progress = PROGRESS_SIZE;
                }
            }

            return rv;
        }
        public long Written { get { return written; } }
        public event ExtractProgressHandler ProgressHandler;
    }
}
