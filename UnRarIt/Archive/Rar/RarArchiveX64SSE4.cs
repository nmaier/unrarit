using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace UnRarIt.Archive.Rar
{
    internal class RarArchiveX64SSE4 : RarArchive
    {
        [DllImportAttribute("unrar_x64_SSE4.dll", EntryPoint = "RARCloseArchive", CallingConvention = CallingConvention.StdCall)]
        private static extern RarErrors CloseArchiveImpl(IntPtr hArcData);

        [DllImportAttribute("unrar_x64_SSE4.dll", EntryPoint = "RAROpenArchiveEx", CallingConvention = CallingConvention.StdCall)]
        protected static extern IntPtr OpenArchiveImpl(ref RAROpenArchiveDataEx ArchiveData);

        [DllImportAttribute("unrar_x64_SSE4.dll", EntryPoint = "RARReadHeaderEx", CallingConvention = CallingConvention.StdCall)]
        protected static extern RarErrors GetHeaderImpl(IntPtr hArcData, ref Header HeaderData);

        [DllImportAttribute("unrar_x64_SSE4.dll", EntryPoint = "RARProcessFileW", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        protected static extern RarErrors ProcessFileImpl(IntPtr hArcData, RarOperation Operation, string DestPath, string DestName);

        [DllImportAttribute("unrar_x64_SSE4.dll", EntryPoint = "RARGetDllVersion", CallingConvention = CallingConvention.StdCall)]
        public static extern RarErrors GetVersionImpl();

        [DllImportAttribute("unrar_x64_SSE4.dll", EntryPoint = "RARSetPassword", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        protected static extern RarErrors SetPasswordImpl(IntPtr hArcData, string Password);

        [DllImportAttribute("unrar_x64_SSE4.dll", EntryPoint = "RARSetCallback", CallingConvention = CallingConvention.StdCall)]
        protected static extern void SetCallbackImpl(IntPtr hArcData, UnRarCallback callback, int UserData);


        protected override RarErrors CloseArchive(IntPtr hArcData)
        {
            return CloseArchiveImpl(hArcData);
        }

        protected override IntPtr OpenArchive(ref RarArchive.RAROpenArchiveDataEx ArchiveData)
        {
            return OpenArchiveImpl(ref ArchiveData);
        }

        protected override RarErrors GetHeader(IntPtr hArcData, ref RarArchive.Header HeaderData)
        {
            return GetHeaderImpl(hArcData, ref HeaderData);
        }

        protected override RarErrors ProcessFile(IntPtr hArcData, RarArchive.RarOperation Operation, string DestPath, string DestName)
        {
            return ProcessFileImpl(hArcData, Operation, DestPath, DestName);
        }

        public override RarErrors GetVersion()
        {
            return GetVersionImpl();
        }

        protected override RarErrors SetPassword(IntPtr hArcData, string Password)
        {
            return SetPasswordImpl(hArcData, Password);
        }

        protected override void SetCallback(IntPtr hArcData, UnRarCallback callback, int UserData)
        {
            SetCallbackImpl(hArcData, callback, UserData);
        }

        public RarArchiveX64SSE4(string FileName, RarOpenMode Mode, UnRarCallback callback) : base(FileName, Mode, callback) { }
    }
}
