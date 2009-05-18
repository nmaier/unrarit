using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Emmy.Interop
{
    internal sealed class FileIconWin: IFileIcon
    {
        [DllImport("Shell32.dll")]
        private static extern int SHGetFileInfo(string pszPath, uint dwFileAttributes, out SHFILEINFO psfi, uint cbfileInfo, SHGFI uFlags);
        [DllImport("Shell32.dll")]
        private static extern int SHGetImageList(int iImageList, ref Guid IID, ref IntPtr p);
        [DllImport("comctl32")]
        private extern static IntPtr ImageList_GetIcon(IntPtr hImgList, int i, int flags);
        [DllImport("user32.dll")]
        extern static bool DestroyIcon(IntPtr hIcon);


        [StructLayout(LayoutKind.Sequential)]
        private struct SHFILEINFO
        {
            public SHFILEINFO(bool b)
            {
                hIcon = IntPtr.Zero; iIcon = 0; dwAttributes = 0; szDisplayName = ""; szTypeName = "";
            }
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;

            [MarshalAs(UnmanagedType.LPStr, SizeConst = 260)]
            public string szDisplayName;

            [MarshalAs(UnmanagedType.LPStr, SizeConst = 80)]
            public string szTypeName;
        };

        private enum SHGFI
        {
            SmallIcon = 0x00000001,
            LargeIcon = 0x00000000,
            Icon = 0x00000100,
            DisplayName = 0x00000200,
            Typename = 0x00000400,
            SysIconIndex = 0x00004000,
            UseFileAttributes = 0x00000010
        }

        IntPtr[] ImageLists;


        public FileIconWin()
        {
            ImageLists = new IntPtr[4];
            Guid iidImageList = new Guid("46EB5926-582E-4017-9FDF-E8998DAA0950");
            for (int i = 0; i < 4; ++i)
            {
                SHGetImageList(i, ref iidImageList, ref ImageLists[i]);
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public Image GetIcon(string aPath, ExtractIconSize aSize)
        {
            SHFILEINFO info = new SHFILEINFO(true);
            int sInfo = Marshal.SizeOf(info);

            SHGFI flags = SHGFI.Icon | SHGFI.SysIconIndex;
            if (SHGetFileInfo(aPath, 0, out info, (uint)sInfo, flags) == 0)
            {
                flags = SHGFI.Icon | SHGFI.SmallIcon | SHGFI.UseFileAttributes;
                if (SHGetFileInfo(aPath, 0x80, out info, (uint)sInfo, flags) == 0)
                {
                    throw new ArgumentException("Cannot get the Icon Index");
                }
            }
            DestroyIcon(info.hIcon);

            IntPtr hIcon = ImageList_GetIcon(ImageLists[(int)aSize], info.iIcon, 0);
            if (hIcon == IntPtr.Zero)
            {
                throw new ArgumentException("Cannot get the Icon");
            }

            using (Icon rv = Icon.FromHandle(hIcon).Clone() as Icon)
            {
                DestroyIcon(hIcon);
                return rv.ToBitmap();
            }
        }
    }
}
