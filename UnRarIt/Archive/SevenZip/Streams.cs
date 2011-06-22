using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnRarIt.Interop;

namespace UnRarIt.Archive.SevenZip
{
    internal interface ISevenZipCleanupStream
    {
        void SetOK();
    }

    internal interface ISevenZipStream : IInStream, ISequentialInStream, ISequentialOutStream, ISevenZipCleanupStream, IDisposable
    {
    }

    internal class SevenZipNullStream : ISevenZipStream
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
        public void SetOK()
        {
        }
        public void Dispose()
        {
        }
    }

    internal class SevenZipFileStream : ISevenZipStream
    {
        protected const int BUFFER_SIZE = 1 << 20;

        protected FileInfo file;
        public FileInfo File
        {
            get { return file; }
        }
        protected FileStream stream;
        public SevenZipFileStream(FileInfo file, FileMode mode)
        {
            Init(file, mode, FileAccess.Read, BUFFER_SIZE);
        }
        public SevenZipFileStream(FileInfo file, FileMode mode, FileAccess access)
        {
            Init(file, mode, access, BUFFER_SIZE);
        }
        protected SevenZipFileStream(FileInfo file, FileMode mode, FileAccess access, int buffer_size)
        {
            Init(file, mode, access, buffer_size);
        }
        private void Init(FileInfo aFile, FileMode mode, FileAccess access, int buffering)
        {
            if (!aFile.Directory.Exists && access != FileAccess.Read)
            {
                aFile.Directory.Create();
            }
            stream = new FileStream(aFile.FullName, mode, access, FileShare.Read, buffering);
            file = aFile;
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
        virtual public void SetOK()
        {
        }
        public virtual void Dispose()
        {
            if (stream != null)
            {
                stream.Close();
                stream.Dispose();
                stream = null;
            }
        }
    }
    internal class SevenZipOutFileStream : SevenZipFileStream
    {
        protected const int WRITE_BUFFER_SIZE = 1 << 23;
        protected const uint FLUSH_SIZE = 1 << 27;

        private LowPriority lp = null;

        long written = 0;
        long fileSize;
        uint flush = FLUSH_SIZE;
        uint progress = BUFFER_SIZE;
        private bool ok = false;

        public SevenZipOutFileStream(FileInfo aFile, long aSize)
            : base(aFile, FileMode.Create, FileAccess.ReadWrite, WRITE_BUFFER_SIZE)
        {
            file = aFile;
            fileSize = aSize;
            if (fileSize > 0)
            {
                stream.SetLength(fileSize);
                stream.Flush();
            }
            if (fileSize > BUFFER_SIZE)
            {
                lp = new LowPriority();
            }
        }
        override public uint Write(byte[] data, uint size)
        {
            uint rv = base.Write(data, size);
            Flush(rv);
            written += rv;

            if (ProgressHandler != null)
            {
                progress -= Math.Min(rv, progress);
                if (progress == 0)
                {
                    if (ProgressHandler != null)
                    {
                        ExtractProgressEventArgs eventArgs = new ExtractProgressEventArgs(file, written, fileSize);
                        ProgressHandler(this, eventArgs);
                        if (!eventArgs.ContinueOperation)
                        {
                            throw new IOException("User canceled");
                        }
                    }
                    progress = BUFFER_SIZE;
                }
            }
            return rv;
        }

        private void Flush(uint written)
        {
            flush -= Math.Min(written, flush);
            if (flush == 0)
            {
                stream.Flush();
                flush = FLUSH_SIZE;
                GC.Collect(1, GCCollectionMode.Optimized);
            }
        }
        public long Written { get { return written; } }
        public event ExtractProgressHandler ProgressHandler;

        public override void Dispose()
        {
            base.Dispose();
            if (lp != null)
            {
                lp.Dispose();
                lp = null;
            }
            if (!ok)
            {
                try
                {
                    file.Delete();
                }
                catch (Exception)
                {
                    // no op
                }
            }
        }
        public override void SetOK()
        {
            ok = true;
        }
    }
}
