using System;
using System.Runtime.Serialization;

namespace UnRarIt.Archive
{
  [Serializable]
  public class ArchiveException : Exception
  {
    protected ArchiveException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }


    public ArchiveException()
      : base("Archive cannot be opened")
    {
    }
    public ArchiveException(string message)
      : base(message)
    {
    }
    public ArchiveException(string message, Exception innerException)
      : base(message, innerException)
    {
    }
  }
}
