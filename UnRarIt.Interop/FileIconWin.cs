using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace UnRarIt.Interop
{
  internal sealed class FileIconWin : IFileIcon
  {
    private readonly IntPtr[] ImageLists;


    public FileIconWin()
    {
      ImageLists = new IntPtr[4];
      var iidImageList = new Guid("46EB5926-582E-4017-9FDF-E8998DAA0950");
      for (var i = 0; i < 4; ++i) {
        SafeNativeMethods.SHGetImageList(i, ref iidImageList, ref ImageLists[i]);
      }
    }


    [MethodImpl(MethodImplOptions.Synchronized)]
    public Image GetIcon(string aPath, FileIconSize aSize)
    {
      var info = new SafeNativeMethods.SHFILEINFO();
      var sInfo = Marshal.SizeOf(info);

      var flags = SafeNativeMethods.SHGFI.Icon | SafeNativeMethods.SHGFI.SysIconIndex;
      if (SafeNativeMethods.SHGetFileInfo(aPath, 0, out info, (uint)sInfo, flags) == 0) {
        flags = SafeNativeMethods.SHGFI.Icon | SafeNativeMethods.SHGFI.SmallIcon | SafeNativeMethods.SHGFI.UseFileAttributes;
        if (SafeNativeMethods.SHGetFileInfo(aPath, 0x80, out info, (uint)sInfo, flags) == 0) {
          throw new ArgumentException("Cannot get the Icon Index");
        }
      }
      SafeNativeMethods.DestroyIcon(info.hIcon);

      var hIcon = SafeNativeMethods.ImageList_GetIcon(ImageLists[(int)aSize], info.iIcon, 0);
      if (hIcon == IntPtr.Zero) {
        throw new ArgumentException("Cannot get the Icon");
      }

      var bmp = Icon.FromHandle(hIcon).ToBitmap();
      SafeNativeMethods.DestroyIcon(hIcon);
      return bmp;
    }
  }
}
