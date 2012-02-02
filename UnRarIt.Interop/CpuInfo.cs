using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace UnRarIt.Interop
{
    public class CpuInfo
    {
        [DllImportAttribute("CPUFeatures_Win32.dll", EntryPoint = "hasSSE3", CallingConvention = CallingConvention.StdCall)]
        private static extern uint HasSSE3_32();
        [DllImportAttribute("CPUFeatures_x64.dll", EntryPoint = "hasSSE3", CallingConvention = CallingConvention.StdCall)]
        private static extern uint HasSSE3_64();

        public static bool isX64 = IntPtr.Size == 8;
        public static bool hasSSE3 = false;

        static CpuInfo()
        {
            if (isX64)
            {
                hasSSE3 = HasSSE3_64() != 0;
            }
            else
            {
                hasSSE3 = HasSSE3_32() != 0;
            }
        }
    }
}
