using System.IO;

namespace UnRarIt.Archive
{
  internal class SevenZipFileStream : ISevenZipStream
  {
    protected const int BUFFER_SIZE = 8192;


    protected FileInfo file;

    protected FileStream stream;


    protected SevenZipFileStream(FileInfo file, FileMode mode, FileAccess access, int buffer_size)
    {
      Init(file, mode, access, buffer_size);
    }


    public SevenZipFileStream(FileInfo file, FileMode mode, FileAccess access)
    {
      Init(file, mode, access, BUFFER_SIZE);
    }


    private void Init(FileInfo aFile, FileMode mode, FileAccess access, int buffering)
    {
      var opts = FileOptions.SequentialScan;
      if (access != FileAccess.Read) {
        opts |= FileOptions.WriteThrough;
      }
      if (!aFile.Directory.Exists && access != FileAccess.Read) {
        aFile.Directory.Create();
      }
      stream = new FileStream(aFile.FullName, mode, access, FileShare.Read, buffering, opts);
      file = aFile;
    }


    public virtual void Dispose()
    {
      if (stream != null) {
        stream.Close();
        stream.Dispose();
        stream = null;
      }
    }

    public uint Read(byte[] data, uint size)
    {
      return (uint)stream.Read(data, 0, (int)size);
    }

    public ulong Seek(long offset, uint seekOrigin)
    {
      return (ulong)stream.Seek(offset, (SeekOrigin)seekOrigin);
    }

    public virtual void SetOK()
    {
    }

    public virtual uint Write(byte[] data, uint size)
    {
      stream.Write(data, 0, (int)size);
      return size;
    }
  }
}
