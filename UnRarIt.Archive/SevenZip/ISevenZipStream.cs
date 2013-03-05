using System;

namespace UnRarIt.Archive
{
  internal interface ISevenZipStream : IInStream, ISequentialInStream, ISequentialOutStream, ISevenZipCleanupStream, IDisposable
  {
  }
}
