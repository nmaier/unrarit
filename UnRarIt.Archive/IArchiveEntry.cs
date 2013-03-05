using System;

namespace UnRarIt.Archive
{
  [CLSCompliant(false)]
  public interface IArchiveEntry
  {
    [CLSCompliant(false)]
    ulong CompressedSize { get; }
    [CLSCompliant(false)]
    uint Checksum { get; }
    DateTime DateTime { get; }
    System.IO.FileInfo Destination { get; set; }
    bool IsEncrypted { get; }
    string Name { get; }
    [CLSCompliant(false)]
    ulong Size { get; }
    [CLSCompliant(false)]
    uint Version { get; }
  }
}
