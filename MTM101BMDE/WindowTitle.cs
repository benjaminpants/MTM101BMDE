using System;
using System.Runtime.InteropServices;

namespace MTM101BaldAPI
{
    public static class WindowTitle
    {
        private delegate bool EnumThreadDelegate(IntPtr hwnd, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool EnumThreadWindows(int dwThreadId, EnumThreadDelegate lpfn, IntPtr lParam);

        [DllImport("Kernel32.dll")]
        private static extern int GetCurrentThreadId();

        private static IntPtr GetWindowHandle()
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
        private static extern bool SetWindowText(IntPtr hwnd, string lpString);

        private static void SetTextInternal(string text)
        {
            IntPtr handle = GetWindowHandle();
            SetWindowText(handle, text);
        }

        private static bool osChecked = false;
        private static bool enabled;

        //SET FUNCTION
        public static void SetText(string text)
        {
            if (!osChecked)
            {
                osChecked = true;
                enabled = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            }
            if (enabled)
                SetTextInternal(text);
        }
    }
}
