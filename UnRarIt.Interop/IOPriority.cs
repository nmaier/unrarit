using System.Runtime.InteropServices;
using System;

namespace UnRarIt.Interop
{
  public sealed class IOPriority : IDisposable
  {
    private enum Win32Priority : int
    {
      BACKGROUND_BEGIN = 0x00010000,
      BACKGROUND_END = 0x00020000,
      HIGHEST = 2,
      IDLE = -15,
      NORMAL = 0
    }

    private static class NativeMethods
    {
      [DllImport("Kernel32.dll", ExactSpelling = true)]
      internal static extern IntPtr GetCurrentThread();

      [DllImport("Kernel32.dll", ExactSpelling = true)]
      [return: MarshalAs(UnmanagedType.Bool)]
      internal static extern bool SetThreadPriority(IntPtr hThread, int nPriority);
    }

    private readonly static bool supported = IsAtLeastVista();
    private static bool IsAtLeastVista()
    {
      var os = Environment.OSVersion;
      return os.Platform == PlatformID.Win32NT && os.Version >= new Version(6, 0);
    }

    private void BeginThreadPriority()
    {
      if (priority == ThreadIOPriority.Normal) {
        return;
      }
      var hThread = NativeMethods.GetCurrentThread();
      if (hThread != IntPtr.Zero) {
        if (priority == ThreadIOPriority.High) {
          NativeMethods.SetThreadPriority(hThread, (int)Win32Priority.HIGHEST);
        }
        else {
          if (supported) {
            NativeMethods.SetThreadPriority(hThread, (int)Win32Priority.BACKGROUND_BEGIN);
          }
          else {
            NativeMethods.SetThreadPriority(hThread, (int)Win32Priority.IDLE);
          }
        }
      }
    }
    private void EndThreadPriority()
    {
      if (priority == ThreadIOPriority.Normal) {
        return;
      }
      var hThread = NativeMethods.GetCurrentThread();
      if (hThread != IntPtr.Zero) {
        if (priority == ThreadIOPriority.High || !supported) {
          NativeMethods.SetThreadPriority(hThread, (int)Win32Priority.NORMAL);
        }
        else {
          NativeMethods.SetThreadPriority(hThread, (int)Win32Priority.BACKGROUND_END);
        }
      }
    }
    private readonly ThreadIOPriority priority;

    public IOPriority(ThreadIOPriority priority)
    {
      this.priority = priority;
      BeginThreadPriority();
    }
    public IOPriority()
      : this(ThreadIOPriority.Low)
    {
    }

    public void Dispose()
    {
      EndThreadPriority();
    }
  }
}
