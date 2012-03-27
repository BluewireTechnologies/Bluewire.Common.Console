using System;
using System.Runtime.InteropServices;

namespace Bluewire.Common.Console
{
    static class NativeMethods
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle(int nStdHandle);
        const int STD_OUTPUT_HANDLE = -11;

        public static bool IsRunningAsService()
        {
            var iStdOut = GetStdHandle(STD_OUTPUT_HANDLE);

            return iStdOut == IntPtr.Zero;
        }
    }
}