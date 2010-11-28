
using System.Runtime.InteropServices;
using System;
namespace UnRarIt.Interop
{
    public class LowPriority : IDisposable
    {
        private sealed class Win32
        {
            public const int THREAD_MODE_BACKGROUND_BEGIN = 0x00010000;
            public const int THREAD_MODE_BACKGROUND_END = 0x00020000;
        }

        private sealed class NativeMethods
        {
            [DllImport("Kernel32.dll", ExactSpelling = true)]
            public static extern IntPtr GetCurrentThread();

            [DllImport("Kernel32.dll", ExactSpelling = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool SetThreadPriority(IntPtr hThread, int nPriority);
        }

        private static bool supported = IsAtLeastVista();
        private static bool IsAtLeastVista()
        {
            OperatingSystem os = Environment.OSVersion;
            return os.Platform == PlatformID.Win32NT && os.Version >= new Version(6, 0);
        }

        public static void BeginThreadBackground()
        {
            if (!supported)
            {
                return;
            }
            IntPtr hThread = NativeMethods.GetCurrentThread();
            if (hThread != IntPtr.Zero)
            {
                NativeMethods.SetThreadPriority(hThread, Win32.THREAD_MODE_BACKGROUND_BEGIN);
            }

        }
        public static void EndThreadBackground()
        {
            if (!supported)
            {
                return;
            }
            IntPtr hThread = NativeMethods.GetCurrentThread();
            if (hThread != IntPtr.Zero)
            {
                NativeMethods.SetThreadPriority(hThread, Win32.THREAD_MODE_BACKGROUND_END);
            }
        }

        public LowPriority()
        {
            BeginThreadBackground();
        }

        #region IDisposable Members

        public void Dispose()
        {
            EndThreadBackground();
        }

        #endregion
    }
}