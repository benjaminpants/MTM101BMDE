using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace MTM101BaldAPI
{
    public class WindowTitle
    {
        private delegate bool EnumThreadDelegate(IntPtr hwnd, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern bool EnumThreadWindows(int dwThreadId, EnumThreadDelegate lpfn, IntPtr lParam);

        [DllImport("Kernel32.dll")]
        static extern int GetCurrentThreadId();

        static IntPtr GetWindowHandle()
        {
            IntPtr returnHwnd = IntPtr.Zero;
            var threadId = GetCurrentThreadId();
            EnumThreadWindows(threadId,
                (hWnd, lParam) => {
                    if (returnHwnd == IntPtr.Zero) returnHwnd = hWnd;
                    return true;
                }, IntPtr.Zero);
            return returnHwnd;
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowText")]
        private static extern bool SetWindowText(System.IntPtr hwnd, System.String lpString);

        //SET FUNCTION
        public static void SetText(string text)
        {
            System.IntPtr handle = GetWindowHandle();
            SetWindowText(handle, text);
        }
    }
}
