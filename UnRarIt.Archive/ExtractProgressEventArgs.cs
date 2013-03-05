using System;
using System.IO;

namespace UnRarIt.Archive
{
  public class ExtractProgressEventArgs : EventArgs
  {
    internal ExtractProgressEventArgs(FileInfo aFile, long aWritten, long aTotal)
    {
      ContinueOperation = true;
      File = aFile;
      Written = aWritten;
      Total = aTotal;
    }


    public bool ContinueOperation { get; set; }
    public FileInfo File { get; private set; }
    public long Total { get; private set; }
    public long Written { get; private set; }
  }
}
