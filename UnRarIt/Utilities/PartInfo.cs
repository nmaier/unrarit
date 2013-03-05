using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

namespace UnRarIt.Utilities
{
  [Serializable]
  public sealed class PartDictionary : Dictionary<uint, FileInfo>
  {
    private PartDictionary(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }


    public PartDictionary()
    {
    }


    public IList<FileInfo> Files
    {
      get
      {
        return (from i in this
                orderby i.Key
                select i.Value).ToList();
      }
    }
    public bool IsComplete
    {
      get
      {
        var keys = (from k in Keys
                    orderby k
                    select k).ToList();
        if (keys.Count == 1 && keys.First() == 0) {
          return true;
        }
        if (this[keys.Last()].Length == this[keys.First()].Length) {
          return false;
        }
        var first = keys.First();
        for (var i = 0; i < keys.Count; ++i) {
          if (keys[i] - first != i) {
            return false;
          }
        }
        return true;
      }
    }


    public bool HasPart(string file)
    {
      if (string.IsNullOrWhiteSpace(file)) {
        return false;
      }
      return (from i in Values
              select i.FullName.ToUpperInvariant()).Contains(file.ToUpperInvariant());
    }
  }
}
