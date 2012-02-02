using System;
using System.Collections.Generic;
using System.Text;

namespace UnRarIt.Archive
{
    public class ArchiveException : Exception
    {
        public ArchiveException()
            : base("Archive cannot be opened")
        { }

        public ArchiveException(string message)
            : base(message)
        { }
    }
}
