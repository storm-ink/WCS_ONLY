using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace WCS.APP
{
    internal static class NativeMethods
    {

        public const int SW_SHOWNORMAL = 1;
        public const int SW_RESTORE = 9;

        //该函数设置由不同线程产生的窗口的显示状态;
        //如果函数原来可见，返回值为非零；如果函数原来被隐藏，返回值为零。
        [DllImport("user32.dll")]
        public static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        //该函数将创建指定窗口的线程设置到前台，并且激活该窗口。键盘输入转向该窗口，并为用户改各种可视的记号。系统给创建前台窗口的线程分配的权限稍高于其他线程。 
        //如果窗口设入了前台，返回值为非零；如果窗口未被设入前台，返回值为零。
        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        // IsIconic ------ 分别判断窗口是否已最小化
        [DllImport("user32.dll")]
        public static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll", EntryPoint = "ShowWindow", SetLastError = true)]
        public static extern bool ShowWindow(IntPtr hWnd, uint nCmdShow);

        [DllImport("User32.dll ", EntryPoint = "FindWindow")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        /// <summary>
        /// 启动控制台
        /// </summary>
        /// <returns></returns>
        [DllImport("kernel32.dll")]
        public static extern bool AllocConsole();
        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", ExactSpelling = true)]
        public static extern int SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        //1.系统消息使用的是0x0100(WM_KEYDOWN)；0x0000--0x0400是系统自定义的消息，其中0x0000为WM_NULL,0x0400为WM_USER。0x0400以后的数值我们可以作为自定义的消息值。（见附录）
        [DllImport("user32.dll")]
        public static extern void PostMessage(IntPtr hWnd, int msg, int wParam, IntPtr lParam);

        public const int GWL_EXSTYLE = -20;
        public const int WS_EX_CLIENTEDGE = 0x200;
        public const uint SWP_NOSIZE = 0x0001;
        public const uint SWP_NOMOVE = 0x0002;
        public const uint SWP_NOZORDER = 0x0004;
        public const uint SWP_NOREDRAW = 0x0008;
        public const uint SWP_NOACTIVATE = 0x0010;
        public const uint SWP_FRAMECHANGED = 0x0020;
        public const uint SWP_SHOWWINDOW = 0x0040;
        public const uint SWP_HIDEWINDOW = 0x0080;
        public const uint SWP_NOCOPYBITS = 0x0100;
        public const uint SWP_NOOWNERZORDER = 0x0200;
        public const uint SWP_NOSENDCHANGING = 0x0400;

        public const int WM_USER = 0x0400;

        private static Hashtable processWnd = new Hashtable();

        public delegate bool WNDENUMPROC(IntPtr hwnd, uint lParam);

        [DllImport("user32.dll", EntryPoint = "EnumWindows", SetLastError = true)]
        public static extern bool EnumWindows(WNDENUMPROC lpEnumFunc, uint lParam);

        [DllImport("user32.dll", EntryPoint = "GetParent", SetLastError = true)]
        public static extern IntPtr GetParent(IntPtr hWnd);

        [DllImport("user32.dll", EntryPoint = "GetWindowThreadProcessId")]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, ref uint lpdwProcessId);

        [DllImport("user32.dll", EntryPoint = "IsWindow")]
        public static extern bool IsWindow(IntPtr hWnd);

        [DllImport("kernel32.dll", EntryPoint = "SetLastError")]
        public static extern void SetLastError(uint dwErrCode);


        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetDesktopWindow();
        [DllImport("user32.dll", EntryPoint = "GetDCEx", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetDCEx(IntPtr hWnd, IntPtr hrgnClip, int flags);

        [DllImport("User32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr hwnd);

        [DllImport("gdi32.dll")]
        static extern int GetPixel(IntPtr hDC, int x, int y);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetForegroundWindow();
        
        [DllImport("User32.dll")]
        public static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);

        public static System.Drawing.Color GetPixelColor(IntPtr dc, int x, int y)
        {
            System.Drawing.Color color = System.Drawing.Color.Empty;

            int colorRef = GetPixel(dc, x, y);
            color = System.Drawing.Color.FromArgb(
                (int)(colorRef & 0x000000FF),
                (int)(colorRef & 0x0000FF00) >> 8,
                (int)(colorRef & 0x00FF0000) >> 16);

            return color;
        }

        public static IntPtr GetMainWindowHandle(uint processId)
        {
            IntPtr ptrWnd = IntPtr.Zero;
            uint uiPid = processId;
            object objWnd = processWnd[uiPid];

            if (objWnd != null)
            {
                ptrWnd = (IntPtr)objWnd;
                if (ptrWnd != IntPtr.Zero && IsWindow(ptrWnd))  // 从缓存中获取句柄
                {
                    return ptrWnd;
                }
                else
                {
                    ptrWnd = IntPtr.Zero;
                }
            }

            bool bResult = EnumWindows(new WNDENUMPROC(EnumWindowsProc), uiPid);
            // 枚举窗口返回 false 并且没有错误号时表明获取成功
            if (!bResult && Marshal.GetLastWin32Error() == 0)
            {
                objWnd = processWnd[uiPid];
                if (objWnd != null)
                {
                    ptrWnd = (IntPtr)objWnd;
                }
            }

            return ptrWnd;
        }

        private static bool EnumWindowsProc(IntPtr hwnd, uint lParam)
        {
            uint uiPid = 0;

            if (GetParent(hwnd) == IntPtr.Zero)
            {
                GetWindowThreadProcessId(hwnd, ref uiPid);
                if (uiPid == lParam)    // 找到进程对应的主窗口句柄
                {
                    processWnd.Add(uiPid, hwnd);   // 把句柄缓存起来
                    SetLastError(0);    // 设置无错误
                    return false;   // 返回 false 以终止枚举窗口
                }
            }

            return true;
        }

    }
}
