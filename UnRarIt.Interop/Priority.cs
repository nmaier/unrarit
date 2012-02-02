
using System.Runtime.InteropServices;
using System;
namespace UnRarIt.Interop
{
    [Serializable]
    public enum ThreadIOPriority
    {
        LOW,
        NORMAL,
        HIGH
    }

    public class IOPriority : IDisposable
    {
        private enum Win32Priority : int
        {
            BACKGROUND_BEGIN = 0x00010000,
            BACKGROUND_END = 0x00020000,
            IDLE = -15,
            NORMAL = 0,
            HIGHEST = 2
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

        private void BeginThreadPriority()
        {
            if (priority == ThreadIOPriority.NORMAL)
            {
                return;
            }
            IntPtr hThread = NativeMethods.GetCurrentThread();
            if (hThread != IntPtr.Zero)
            {
                if (priority == ThreadIOPriority.HIGH)
                {
                    NativeMethods.SetThreadPriority(hThread, (int)Win32Priority.HIGHEST);
                }
                else if (supported)
                {
                    NativeMethods.SetThreadPriority(hThread, (int)Win32Priority.BACKGROUND_BEGIN);
                }
                else
                {
                    NativeMethods.SetThreadPriority(hThread, (int)Win32Priority.IDLE);
                }
            }
        }
        private void EndThreadPriority()
        {
            if (priority == ThreadIOPriority.NORMAL)
            {
                return;
            }
            IntPtr hThread = NativeMethods.GetCurrentThread();
            if (hThread != IntPtr.Zero)
            {
                if (priority == ThreadIOPriority.HIGH || !supported)
                {
                    NativeMethods.SetThreadPriority(hThread, (int)Win32Priority.NORMAL);
                }
                else
                {
                    NativeMethods.SetThreadPriority(hThread, (int)Win32Priority.BACKGROUND_END);
                }
            }
        }
        private ThreadIOPriority priority = ThreadIOPriority.NORMAL;

        public IOPriority(ThreadIOPriority aPriority)
        {
            BeginThreadPriority();
        }
        public IOPriority()
            : this(ThreadIOPriority.LOW)
        {
        }

        #region IDisposable Members

        public void Dispose()
        {
            EndThreadPriority();
        }

        #endregion
    }
}