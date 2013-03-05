using System;
using System.Collections.Generic;

[assembly:CLSCompliant(true)]
namespace UnRarIt.Interop
{
  public static class CpuInfo
  {
    public static readonly bool IsX64 = IntPtr.Size == 8;
  }
}
