using System;
namespace UnRarIt
{
    public interface IArchiveEntry
    {
        ulong CompressedSize { get; }
        uint Crc { get; }
        DateTime DateTime { get; }
        System.IO.FileInfo Destination { get; set; }
        bool IsCrypted { get; }
        string Name { get; }
        ulong Size { get; }
        uint Version { get; }
    }
}
