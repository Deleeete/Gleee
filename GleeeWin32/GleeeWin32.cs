using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using static Gleee.Win32.Struct;

namespace Gleee.Win32
{
    public static class CustomFunction
    {
        public static Rectangle GetClientRectangle(IntPtr handle)
        {
            return User32.Window.ClientToScreen(handle, out var point) && User32.Window.GetClientRect(handle, out var rect)
                ? new Rectangle(point.X, point.Y, rect.Right - rect.Left, rect.Bottom - rect.Top)
                : default;
        }
        public static string ReadMemString(IntPtr hProcess, IntPtr BaseAddress, int bytesToRead)
        {
            var size = bytesToRead;
            byte[] buffer = new byte[bytesToRead];
            Kernel32.ReadProcessMemory(hProcess, BaseAddress, buffer, size, out var lpNumberOfBytesRead);
            if (lpNumberOfBytesRead == size) return Encoding.ASCII.GetString(buffer);
            else return null;
        }
    }
    public static class User32
    {
        public static class Mouse
        {
            [DllImport("user32.dll")]
            static extern bool GetCursorPos(ref Point lpPoint);
            [DllImport("user32.dll", EntryPoint = "SetCursorPos")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool SetCursorPos(int x, int y);
            [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
            public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);
        }
        public static class Window
        {
            [DllImport("user32.dll")]
            public static extern UInt32 RegisterHotKey(IntPtr hWnd, UInt32 id, UInt32 fsModifiers, UInt32 vk);
            [DllImport("user32.dll", EntryPoint = "GetDC")]
            public static extern IntPtr GetDC(IntPtr hwnd);
            [DllImport("user32.dll", SetLastError = true)]
            public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
            [DllImport("user32.dll", SetLastError = true)]
            public static extern IntPtr GetForegroundWindow();
            [DllImport("user32.dll", EntryPoint = "ChildWindowFromPoint")]
            public static extern IntPtr ChildWindowFromPoint(IntPtr hwnd, Point pt);
            [DllImport("user32.dll")]
            public static extern IntPtr WindowFromPoint(Point pt);
            [DllImport("user32.dll", EntryPoint = "GetParent")]
            public static extern IntPtr GetParent(IntPtr hwnd);
            [DllImport("user32.dll", SetLastError = true)]
            public static extern bool ClientToScreen(IntPtr hWnd, out Point lpPoint);
            [DllImport("user32.dll", SetLastError = true)]
            public static extern bool GetClientRect(IntPtr hWnd, out Struct.Rect lpRect);
            [DllImport("user32.dll", EntryPoint = "ScreenToClient")]
            public static extern bool ScreenToClient(IntPtr hwnd, ref Point pt);
            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool EnableWindow(IntPtr hWnd, bool bEnable);
            [DllImport("user32.dll", EntryPoint = "SendMessage")]
            private static extern int SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);
            [DllImport("user32.dll", EntryPoint = "PostMessage")]
            private static extern int PostMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);
            [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
            private static extern IntPtr GetWindowLongPtr32(IntPtr hWnd, int nIndex);
            [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
            private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);
            public static IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex)
            {
                if (IntPtr.Size == 8)
                    return GetWindowLongPtr64(hWnd, nIndex);
                else
                    return GetWindowLongPtr32(hWnd, nIndex);
            }
            [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
            public static extern int SetWindowLongPtr32(IntPtr hWnd, int nIndex, int dwNewLong);
            [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
            static extern int SetWindowLongPtr64(IntPtr hWnd, int nIndex, int dwNewLong);
            public static int SetWindowLongPtr(IntPtr hWnd, int nIndex, int dwNewLong)
            {
                if (IntPtr.Size == 8) return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
                else return SetWindowLongPtr32(hWnd, nIndex, dwNewLong);
            }
            [DllImport("user32.dll")]
            public static extern bool MoveWindow(IntPtr hWnd, int x, int y, int width, int height, bool repaint);
            [DllImport("user32.dll")]
            public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);
            [DllImport("user32.dll")]
            public static extern bool GetWindowRect(IntPtr hWnd, IntPtr lpRect);
            [DllImport("user32.dll", CharSet = CharSet.Auto)]
            public static extern bool MessageBox(IntPtr hWnd, string lpText, string lpTitle, uint uType);
        }
    }
    public static class Psapi
    {
        [DllImport("psapi.dll", SetLastError = true, EntryPoint = "GetModuleInformation")]
        public static extern bool GetModuleInformation(IntPtr hProcess, IntPtr hDllHandle, ref ModuleInfo Mdl_Info, int dwSizeOfModuleInfo);
    }
    public static class Kernel32
    {
        [DllImport("kernel32.dll", SetLastError = true, EntryPoint = "GetLastError")]
        public static extern int GetLastError();
        [DllImport("kernel32.dll", SetLastError = true, EntryPoint = "K32GetModuleInformation")]
        public static extern bool GetModuleInformation(IntPtr hProcess, IntPtr hDllHandle, ref ModuleInfo Mdl_Info, int dwSizeOfModuleInfo);
        [DllImport("kernel32.dll", SetLastError = true, EntryPoint ="LoadLibrary")]
        public static extern IntPtr LoadLibrary(string lpLibName);
        [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true, EntryPoint = "GetProcAddress")]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);
        [DllImport("kernel32", CharSet = CharSet.Ansi, SetLastError = true, EntryPoint = "GetProcAddress")]
        public static extern IntPtr GetProcAddressOrdinal(IntPtr hModule, IntPtr procName);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out, MarshalAs(UnmanagedType.AsAny)] object lpBuffer, int dwSize, out int lpNumberOfBytesRead);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, out int lpNumberOfBytesWritten);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr VirtualAlloc(IntPtr lpAddress, IntPtr dwSize, int flAllocationType, int flProtect);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, int dwsize, int flAllocationType, int flProtect);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool VirtualProtect(IntPtr lpAddress, int dwSize, int flNewProtect, ref int lpflOldProtect);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr CreateEvent(IntPtr lpEventAttributes, bool bManualReset, bool bInitialState, IntPtr lpName);
    }
    public static class NtosKrnl
    {
        [DllImport("ntoskrnl.dll")]
        public static extern void RtlCopyMemory(IntPtr Destination, IntPtr Source, int Length);
    }
    public static class Gdi32
    {
        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int BitBlt(IntPtr hDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, int dwRop);
        [DllImport("gdi32.dll")]
        public static extern bool Rectangle(IntPtr hdc, IntPtr lpRect);
    }
    public static class Macro
    {
        public const int PAGE_EXECUTE = 0x10;
        public const int PAGE_EXECUTE_READ = 0X20;
        public const int PAGE_EXECUTE_READWRITE = 0X40;
        public const int PAGE_EXECUTE_WRITECOPY = 0x80;
        public const int PAGE_NOACCESS = 0X01;
        public const int PAGE_READONLY = 0x02;
        public const int PAGE_READWRITE = 0X04;
        public const int PAGE_WRITECOPY = 0X08;
        public const int PAGE_TARGETS_INVALID = 0x40000000;
        public const int PAGE_TARGETS_NO_UPDATE = 0x40000000;
        public const int PAGE_GUARD = 0x100;
        public const int PAGE_NOCACHE = 0x200;
        public const int PAGE_WRITECOMBINE = 0X400;
        public const int MEM_COMMIT = 0x1000;
        public const int MEM_RESERVE = 0x2000;
        public const int MEM_RESET = 0x8000;
        public const int MEM_RESET_UNDO = 0x1000000;
        public const int MEM_LARGE_PAGES = 0x20000000;
        public const int MEM_PHYSICAL = 0x00400000;
        public const int MEM_WRITE_WATCH = 0x00200000;
        public const int MEM_TOP_DOWN = 0x00100000;
        public const int WM_ERASEBKGND = 0x0014;
        public const int WM_NCPAINT = 0x85;
        public const int WM_ACTIVATE = 0x001C;
        public const int WM_LBUTTONDOWN = 0x0201;
        public const int WM_LBUTTONUP = 0x0202;
        public const int WM_ENABLE = 0X000A;
        public const int WM_COMMAND = 0X0111;
        public const int WM_PAINT = 0X0F;
        public const int WM_CANCELMODE = 0x001F;
        public const int MOUSEEVENTF_LEFTDOWN = 0x02;
        public const int MOUSEEVENTF_LEFTUP = 0x04;
        public const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        public const int MOUSEEVENTF_RIGHTUP = 0x10;
        public const int WS_DISABLED = 0x8000000;
        public const int WS_OVERLAPPED = 0;
        public const int WS_CHILD = 0x40000000;
        public const int WS_MINIMIZE = 0x20000000;
        public const int WS_DISABLE = 0x08000000;
        public const int WS_VISIBLE = 0x10000000;
        public const int WS_CLIPSIBLINGS = 0x4000000;
        public const int WS_CLIPCHILDREN = 0x2000000;
        public const int WS_MAXIMIZE = 0x1000000;
        public const int WS_CAPTION = 0xC00000;
        public const int WS_BORDER = 0x800000;
        public const int WS_DLGFRAME = 0x400000;
        public const int WS_VSCROLL = 0x200000;
        public const int WS_HSCROLL = 0x100000;
        public const int WS_SYSMENU = 0x80000;
        public const int WS_THICKFRAME = 0x40000;
        public const int WS_GROUP = 0x20000;
        public const int WS_TABSTOP = 0x10000;
        public const int WS_MINIMIZEBOX = 0x20000;
        public const int WS_MAXIMIZEBOX = 0x10000;
        public const int WS_TILED = WS_OVERLAPPED;
        public const int WS_ICONIC = WS_MINIMIZE;
        public const int WS_SIZEBOX = WS_THICKFRAME;
        public const int WS_EX_DLGMODALFRAME = 0x0001;
        public const int WS_EX_NOPARENTNOTIFY = 0x0004;
        public const int WS_EX_TOPMOST = 0x0008;
        public const int WS_EX_ACCEPTFILES = 0x0010;
        public const int WS_EX_TRANSPARENT = 0x0020;
        public const int WS_EX_MDICHILD = 0x0040;
        public const int WS_EX_TOOLWINDOW = 0x0080;
        public const int WS_EX_WINDOWEDGE = 0x0100;
        public const int WS_EX_CLIENTEDGE = 0x0200;
        public const int WS_EX_CONTEXTHELP = 0x0400;
        public const int WS_EX_RIGHT = 0x1000;
        public const int WS_EX_LEFT = 0x0000;
        public const int WS_EX_RTLREADING = 0x2000;
        public const int WS_EX_LTRREADING = 0x0000;
        public const int WS_EX_LEFTSCROLLBAR = 0x4000;
        public const int WS_EX_RIGHTSCROLLBAR = 0x0000;
        public const int WS_EX_CONTROLPARENT = 0x10000;
        public const int WS_EX_STATICEDGE = 0x20000;
        public const int WS_EX_APPWINDOW = 0x40000;
        public const int WS_EX_OVERLAPPEDWINDOW = (WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE);
        public const int WS_EX_PALETTEWINDOW = (WS_EX_WINDOWEDGE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST);
        public const int WS_EX_LAYERED = 0x00080000;
        public const int WS_EX_NOINHERITLAYOUT = 0x00100000;
        public const int WS_EX_LAYOUTRTL = 0x00400000;
        public const int WS_EX_COMPOSITED = 0x02000000;
        public const int WS_EX_NOACTIVATE = 0x08000000;
        public const int DOT_NET = 0X202B;
        public const int SWP_NOMOVE = 0x0002; //不调整窗体位置
        public const int SWP_NOSIZE = 0x0001; //不调整窗体大小
        public static IntPtr HWND_TOPMOST = new IntPtr(-1);
        public static IntPtr HWND_NOTOPMOST = new IntPtr(-2);
    }
    public static class Struct
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct Rect
        {
            public int Left, Top, Right, Bottom;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct ModuleInfo
        {
            public IntPtr lpBaseOfDll;
            public int SizeOfImage;
            public IntPtr EntryPoint;
        }
    }
}
