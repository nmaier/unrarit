using System;
using System.Runtime.Serialization;

namespace UnRarIt.Archive
{
  [Serializable]
  public class SevenZipException : Exception
  {
    protected SevenZipException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }


    public SevenZipException()
    {
    }
    public SevenZipException(string msg)
      : base(msg)
    {
    }
    public SevenZipException(string message, Exception innerException)
      : base(message, innerException)
    {
    }
  }
}
