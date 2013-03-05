using System;
using System.Collections.Generic;
using System.Drawing;

namespace UnRarIt.Interop
{
  internal interface IFileIcon
  {
    Image GetIcon(string aPath, FileIconSize aSize);
  }
}
