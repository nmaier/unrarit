using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Emmy.Interop
{
    public enum ExtractIconSize
    {
        Small = 0x1,
        Large = 0x0,
        ExtraLarge = 0x2
    };

    internal interface IFileIcon
    {
        Image GetIcon(string aPath, ExtractIconSize aSize);
    }
}
