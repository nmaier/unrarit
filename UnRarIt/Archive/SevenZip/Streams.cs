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

    internal class SevenZipVolumeStream : SevenZipStream
    {
        class StreamInfo
        {
            public long Length;
            public long Offset;
            public long OffsetMax;
            public Stream Stream;
            public StreamInfo(Stream aStream, long aOffset)
            {
                Stream = aStream;
                Length = Stream.Length;
                Offset = aOffset;
                OffsetMax = Offset + Length;
            }
        }
        List<StreamInfo> streams = new List<StreamInfo>();
        long position = 0;
        long length = 0;

        public SevenZipVolumeStream(List<FileInfo> files, FileMode mode, FileAccess access)
        {
            if (FileAccess.Read != access)
            {
                throw new IOException("Cannot open a VolumeStream in " + access.ToString());
            }
            foreach (FileInfo fi in files)
            {
                AddStream(new FileStream(fi.FullName, mode, access));
            }
        }


        void AddStream(Stream aToAdd)
        {
            streams.Add(new StreamInfo(aToAdd, length));
            length += aToAdd.Length;
        }

        public ulong Seek(long offset, uint seekOrigin)
        {
            switch ((SeekOrigin)seekOrigin)
            {
                case SeekOrigin.Begin:
                    position = offset;
                    break;
                case SeekOrigin.Current:
                    position += offset;
                    break;
                case SeekOrigin.End:
                    position = (length - 1) + offset;
                    break;
            }
            if (position < 0 || position >= length)
            {
                throw new IOException("Tried to seek over stream bounds");
            }
            return (ulong)position;
        }
        public uint Read(byte[] data, uint size)
        {
            int max = (int)Math.Max(Math.Min(0, length - position - 1), size);
            int read = 0;
            while (max > 0)
            {
                foreach (StreamInfo i in streams)
                {
                    if (i.OffsetMax > position && i.Offset <= position)
                    {
                        long sp = position - i.Offset;
                        if (i.Stream.Position != sp)
                        {
                            try
                            {
                                i.Stream.Seek(sp, SeekOrigin.Begin);
                            }
                            catch (IOException ex)
                            {
                                throw ex;
                            }
                        }
                        int cr = 0;
                        try
                        {
                            cr = i.Stream.Read(data, read, max);
                        }
                        catch (IOException ex)
                        {
                            throw ex;
                        }
                        read += cr;
                        max -= cr;
                        position += cr;
                        break;
                    }
                    Console.WriteLine(max);
                }
            }
            return (uint)read;
        }
        public uint Write(byte[] data, uint size)
        {
            throw new IOException("Cannot write");
        }
        public void Dispose()
        {
            foreach (StreamInfo i in streams)
            {
                i.Stream.Dispose();
            }
        }
    }
}
