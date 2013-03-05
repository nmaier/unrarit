using System;
using System.IO;
using UnRarIt.Interop;

namespace UnRarIt.Archive
{
  internal class SevenZipOutFileStream : SevenZipFileStream
  {
    protected const uint FLUSH_SIZE = 1 << 24;

    protected const int WRITE_BUFFER_SIZE = 1 << 20;


    private readonly long fileSize;

    private uint flush = FLUSH_SIZE;

    private IOPriority lp = null;

    private bool ok = false;

    private uint progress = BUFFER_SIZE;


    public SevenZipOutFileStream(FileInfo aFile, long aSize, ThreadIOPriority aPriority)
      : base(aFile, FileMode.Create, FileAccess.ReadWrite, WRITE_BUFFER_SIZE)
    {
      file = aFile;
      fileSize = aSize;
      if (fileSize > 0) {
        stream.SetLength(fileSize);
        stream.Flush();
      }
      if (fileSize > BUFFER_SIZE) {
        lp = new IOPriority(aPriority);
      }
    }


    public event EventHandler<ExtractProgressEventArgs> ProgressHandler;


    public long Written { get; private set; }


    private void Flush(uint written)
    {
      flush -= Math.Min(written, flush);
      if (flush == 0) {
        stream.Flush();
        flush = FLUSH_SIZE;
      }
    }


    public override void Dispose()
    {
      base.Dispose();
      if (lp != null) {
        lp.Dispose();
        lp = null;
      }
      if (!ok) {
        try {
          file.Delete();
        }
        catch (Exception) {
        }
      }
    }

    public override void SetOK()
    {
      ok = true;
    }

    public override uint Write(byte[] data, uint size)
    {
      var rv = base.Write(data, size);
      Flush(rv);
      Written += rv;

      if (ProgressHandler != null) {
        progress -= Math.Min(rv, progress);
        if (progress == 0) {
          if (ProgressHandler != null) {
            var eventArgs = new ExtractProgressEventArgs(file, Written, fileSize);
            ProgressHandler(this, eventArgs);
            if (!eventArgs.ContinueOperation) {
              throw new IOException("User canceled");
            }
          }
          progress = BUFFER_SIZE;
        }
      }
      return rv;
    }
  }
}
