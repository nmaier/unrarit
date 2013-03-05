using System;
using System.IO;
using System.Text.RegularExpressions;
using UnRarIt.Interop;

namespace UnRarIt.Archive
{
  internal class SevenZipItemInfo : IArchiveEntry
  {
    private readonly ulong compressedSize;

    private readonly uint crc;

    private FileInfo destination = null;

    private readonly bool isCrypted;

    private readonly string name;

    private readonly static Regex regPreClean = new Regex(@"^((?:\\|/)?(?:images|bilder|DH|set|cd(?:a|b|\d+))(?:\\|/))", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly ulong size;


    internal SevenZipItemInfo(string aName, uint aCrc, bool aIsCrypted, ulong aSize, ulong aCompressedSize)
    {
      name = aName;
      while (true) {
        var m = regPreClean.Match(name);
        if (!m.Success) {
          break;
        }
        name = name.Substring(m.Value.Length);
      }
      while (name.Length != 0 && name[0] == Path.DirectorySeparatorChar || name[0] == Path.AltDirectorySeparatorChar) {
        name = name.Substring(1);
      }
      name = Replacements.CleanFileName(name);
      crc = aCrc;
      isCrypted = aIsCrypted;
      size = aSize;
      compressedSize = aCompressedSize;
    }


    public ulong CompressedSize
    {
      get {
        return compressedSize;
      }
    }
    public uint Checksum
    {
      get {
        return crc;
      }
    }
    public DateTime DateTime
    {
      get {
        throw new NotImplementedException();
      }
    }
    public FileInfo Destination
    {
      get {
        return destination;
      }
      set {
        destination = value;
      }
    }
    public bool IsEncrypted
    {
      get {
        return isCrypted;
      }
    }
    public string Name
    {
      get {
        return name;
      }
    }
    public ulong Size
    {
      get {
        return size;
      }
    }
    public uint Version
    {
      get {
        throw new NotImplementedException();
      }
    }
  }
}
