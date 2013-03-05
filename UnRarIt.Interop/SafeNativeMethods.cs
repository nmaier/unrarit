using System;
using System.Runtime.InteropServices;

namespace UnRarIt.Interop
{
  internal static class SafeNativeMethods
  {
    internal enum SHGFI : uint
    {
      DisplayName = 0x00000200,
      Icon = 0x00000100,
      LargeIcon = 0x00000000,
      SmallIcon = 0x00000001,
      SysIconIndex = 0x00004000,
      Typename = 0x00000400,
      UseFileAttributes = 0x00000010
    }


    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal extern static bool DestroyIcon(IntPtr hIcon);

    [DllImport("comctl32")]
    internal extern static IntPtr ImageList_GetIcon(IntPtr hImgList, int i, uint flags);

    [DllImport("Shell32.dll", CharSet=CharSet.Unicode, EntryPoint="SHGetFileInfoW")]
    internal static extern int SHGetFileInfo(string pszPath, uint dwFileAttributes, out SHFILEINFO psfi, uint cbfileInfo, SHGFI uFlags);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode"), DllImport("Shell32.dll")]
    internal static extern int SHGetImageList(int iImageList, ref Guid IID, ref IntPtr p);


    [StructLayout(LayoutKind.Sequential)]
    internal struct SHFILEINFO
    {
      public IntPtr hIcon;
      public int iIcon;
      public uint dwAttributes;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 520)]
      public byte[] szDisplayName;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 160)]
      public byte[] szTypeName;
    }
  }
}
