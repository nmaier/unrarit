using System;
using System.IO;

namespace UnRarIt.Archive
{
  public class ExtractFileEventArgs : EventArgs
  {
    internal ExtractFileEventArgs(FileInfo aArchive, IArchiveEntry aItem, ExtractionStage aStage)
    {
      ContinueOperation = true;
      Archive = aArchive;
      Item = aItem;
      Stage = aStage;
    }


    public FileInfo Archive { get; private set; }
    public bool ContinueOperation { get; set; }
    [CLSCompliant(false)]
    public IArchiveEntry Item { get; private set; }
    public ExtractionStage Stage { get; private set; }
  }
}
