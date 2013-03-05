using System;
using System.Globalization;

namespace UnRarIt.Utilities
{
  public static class Extensions
  {
    [CLSCompliant(false)]
    public static string ToFormattedSize(this ulong size)
    {
      if (size <= 900) {
        return String.Format(CultureInfo.CurrentCulture, "{0} Byte", size);
      }
      var fmts = new string[] { "KB", "MB", "GB", "TB" };
      var fmt = fmts[0];
      double ds = size;
      for (var i = 0; i < fmts.Length && ds > 900; ++i) {
        ds /= 1024;
        fmt = fmts[i];
      }
      return String.Format(CultureInfo.CurrentCulture, "{0:F2} {1}", ds, fmt);
    }

    public static string ToFormattedSize(this long size)
    {
      return ((ulong)size).ToFormattedSize();
    }
  }
}
