using System;

namespace UnRarIt.Archive
{
  internal class SevenZipNullStream : ISevenZipStream
  {
    public void Dispose()
    {
    }

    public uint Read(byte[] data, uint size)
    {
      return 0;
    }

    public ulong Seek(long offset, uint seekOrigin)
    {
      return 0;
    }

    public void SetOK()
    {
    }

    public uint Write(byte[] data, uint size)
    {
      return size;
    }
  }
}
