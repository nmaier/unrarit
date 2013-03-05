using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnRarIt.Interop;
using System.Globalization;

namespace UnRarIt.Utilities
{
  public class PartFileConsumer
  {
    private readonly static IPartConsumer[] consumers = new IPartConsumer[] { new SplitConsumer(), new PartConsumer(), new OldSyntaxConsumer() };

    private readonly Dictionary<string, PartDictionary> parts = new Dictionary<string, PartDictionary>();


    private interface IPartConsumer
    {
      bool Consume(FileInfo info, out string key, out uint number);
    }


    public Dictionary<string, PartDictionary> Parts
    {
      get
      {
        return parts;
      }
    }


    public bool Consume(FileInfo info)
    {
      foreach (IPartConsumer c in consumers) {
        string key;
        uint number;
        if (c.Consume(info, out key, out number)) {
          PartDictionary pi;
          if (!Parts.TryGetValue(key, out pi)) {
            pi = new PartDictionary();
            Parts.Add(key, pi);
          }
          pi[number] = info;
          return true;
        }
      }
      return false;
    }


    private class OldSyntaxConsumer : IPartConsumer
    {
      private readonly static Regex r = new Regex(@"\.(R|Z)\d{2,}$", RegexOptions.IgnoreCase | RegexOptions.Compiled);


      public bool Consume(FileInfo info, out string key, out uint number)
      {
        key = string.Empty;
        number = 0;
        var ext = info.Extension.ToUpperInvariant();
        if (ext == ".ZIP" || ext == ".RAR") {
          key = info.FullName.ToUpperInvariant();
          return true;
        }
        var m = r.Match(info.Name);
        if (!m.Success) {
          return false;
        }
        string format;
        switch (m.Groups[1].Value[0]) {
          case 'r':
            format = ".rar";
            break;
          case 'z':
            format = ".zip";
            break;
          default:
            throw new NotImplementedException();
        }
        key = Replacements.CombinePath(info.DirectoryName, Path.GetFileNameWithoutExtension(info.Name).ToUpperInvariant() + format).ToUpperInvariant();
        return true;
      }
    }

    private class PartConsumer : IPartConsumer
    {
      private readonly static Regex r = new Regex(@"\.(PART(\d+))\.(?:RAR|ZIP)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);


      public bool Consume(FileInfo info, out string key, out uint number)
      {
        key = string.Empty;
        number = 0;
        var m = r.Match(info.Name);
        if (!m.Success) {
          return false;
        }
        var val = m.Groups[2].Value;
        if (!uint.TryParse(val, out number)) {
          return false;
        }
        key = Replacements.CombinePath(info.DirectoryName, info.Name.Replace(m.Groups[1].Value, String.Format(
          CultureInfo.InvariantCulture,
          "part{0}",
          1.ToString(new string('0', val.Length), CultureInfo.InvariantCulture)
          ))).ToUpperInvariant();

        return true;
      }
    }

    private class SplitConsumer : IPartConsumer
    {
      private readonly Regex r = new Regex(@"\.(\d+)$", RegexOptions.Compiled);


      public bool Consume(FileInfo info, out string key, out uint number)
      {
        key = string.Empty;
        number = 0;
        var m = r.Match(info.Name);
        if (!m.Success) {
          return false;
        }
        var val = m.Groups[1].Value;
        if (!uint.TryParse(val, out number)) {
          return false;
        }
        key = Replacements.CombinePath(info.DirectoryName, String.Format(
          CultureInfo.InvariantCulture,
          "{0}.{1}1", Path.GetFileNameWithoutExtension(info.Name),
          new string('0', m.Groups[1].Length - 1)
          )).ToUpperInvariant();

        return true;
      }
    }
  }
}
