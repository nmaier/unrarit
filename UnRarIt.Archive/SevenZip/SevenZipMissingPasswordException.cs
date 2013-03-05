using System;
using System.Runtime.Serialization;

namespace UnRarIt.Archive
{
  [Serializable]
  public class SevenZipMissingPasswordException : SevenZipException
  {
    protected SevenZipMissingPasswordException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }


    public SevenZipMissingPasswordException()
      : base("No password available")
    {
    }
    public SevenZipMissingPasswordException(string msg)
      : base(msg)
    {
    }
    public SevenZipMissingPasswordException(string message, Exception innerException)
      : base(message, innerException)
    {
    }
  }
}
