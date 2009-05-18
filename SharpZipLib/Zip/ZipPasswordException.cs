using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ICSharpCode.SharpZipLib.Zip
{
    public class ZipPasswordException : ZipException
    {
        public ZipPasswordException(string msg) : base(msg) { }
    }
}
