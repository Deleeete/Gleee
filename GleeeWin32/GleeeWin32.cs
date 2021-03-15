using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace Gleee.Win32
{
    public static class Win32Wrapper
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
            public static extern bool GetClientRect(IntPtr hWnd, out Rect lpRect);
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
        [DllImport("psapi.dll", SetLastError = true)]
        public static extern int EnumProcessModules(IntPtr hProcess, [Out] IntPtr lphModule, uint cb, out uint lpcbNeeded);
        [DllImport("psapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern uint GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, [Out] StringBuilder lpBaseName, uint nSize);
        [DllImport("psapi.dll", SetLastError = true)]
        static extern uint GetProcessImageFileName(IntPtr hProcess, [Out] StringBuilder lpImageFileName, [In][MarshalAs(UnmanagedType.U4)] int nSize);
    }
    public static class Kernel32
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern void GetSystemInfo(ref SystemInfo Info);
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto, EntryPoint = "K32EnumProcessModules")]
        public static extern bool EnumProcessModules(IntPtr hProcess, [Out] IntPtr lphModule, uint cb, out uint lpcbNeeded);
        [DllImport("kernel32.dll", SetLastError = true)]
        [PreserveSig]
        public static extern uint GetModuleFileName([In] IntPtr hModule, [Out] StringBuilder lpFilename, [In][MarshalAs(UnmanagedType.U4)] int nSize);
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi, EntryPoint = "K32EnumProcessModulesA")]
        public static extern uint GetModuleFileNameExA(IntPtr hProcess, IntPtr hModule, [Out] StringBuilder lpBaseName, uint nSize);
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "K32EnumProcessModulesW")]
        public static extern uint GetModuleFileNameExW(IntPtr hProcess, IntPtr hModule, [Out] StringBuilder lpBaseName, uint nSize);
        [DllImport("kernel32.dll", SetLastError = true, EntryPoint = "K32GetModuleInformation")]
        public static extern bool GetModuleInformation(IntPtr hProcess, IntPtr hDllHandle, ref ModuleInfo Mdl_Info, int dwSizeOfModuleInfo);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr CreateEvent(IntPtr lpEventAttributes, bool bManualReset, bool bInitialState, IntPtr lpName);
        [DllImport("kernel32.dll", SetLastError = true, EntryPoint = "GetLastError")]
        public static extern int GetLastError();
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern void GetSystemInfo(IntPtr lpSystemInfo);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int SuspendThread(IntPtr hThread);

        [DllImport("kernel32.dll", SetLastError = true, EntryPoint = "LoadLibraryA", CharSet = CharSet.Ansi)]
        public static extern IntPtr LoadLibraryA(string lpLibName);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hReservedNull, LoadLibraryFlags dwFlags);
        [DllImport("kernel32.dll", SetLastError = true, EntryPoint = "LoadLibraryW", CharSet = CharSet.Unicode)]
        public static extern IntPtr LoadLibraryW(string lpLibName);

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, EntryPoint = "GetProcAddress")]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);
        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Ansi, EntryPoint = "GetProcAddress")]
        public static extern IntPtr GetProcAddressOrdinal(IntPtr hModule, IntPtr procName);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out, MarshalAs(UnmanagedType.AsAny)] object lpBuffer, int dwSize, out int lpNumberOfBytesRead);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, out int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        public static extern bool VirtualQuery(IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, uint dwLength);
        [DllImport("kernel32.dll")]
        public static extern bool VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, uint dwLength);
        [DllImport("kernel32.dll")]
        public static extern bool VirtualQuery(IntPtr lpAddress, out MemoryBasicInfomation lpBuffer, uint dwLength);
        [DllImport("kernel32.dll")]
        public static extern bool VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MemoryBasicInfomation lpBuffer, uint dwLength);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr VirtualAlloc(IntPtr lpAddress, int dwSize, AllocationType flAllocationType, PageProtection flProtect);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, int dwsize, AllocationType flAllocationType, PageProtection flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool VirtualProtect(IntPtr lpAddress, int dwSize, PageProtection flNewProtect, ref int lpflOldProtect);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress, int dwSize, PageProtection flNewProtect, ref int lpflOldProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenProcess(ProcessAccessFlags processAccess, bool bInheritHandle, int processId);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetCurrentProcess();
        [DllImport("kernel32.dll", SetLastError = true, EntryPoint = "K32GetProcessImageFileNameA", CharSet = CharSet.Ansi)]
        public static extern bool GetProcessImageFileNameA(IntPtr hProcess, [Out] StringBuilder lpImageFileName, [In][MarshalAs(UnmanagedType.U4)] int nSize);
        [DllImport("kernel32.dll", SetLastError = true, EntryPoint = "K32GetProcessImageFileNameW", CharSet = CharSet.Unicode)]
        public static extern bool GetProcessImageFileNameW(IntPtr hProcess, [Out] StringBuilder lpImageFileName, [In][MarshalAs(UnmanagedType.U4)] int nSize);
    }
    public static class MsCorEE
    {
        [DllImport("mscoree.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr CorBindToRuntimeEx(string pwszVersion, string pwszBuildFlavor, StartupFlags startupFlags, Guid rclsid, Guid riid, out IntPtr pClrHost);
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
    public static class Win32Const
    {
        public static Guid CLSID_CLRRuntimeHost = new Guid(0x90F1A06E, 0x7712, 0x4762, 0x86, 0xB5, 0x7A, 0x5E, 0xBA, 0x6B, 0xDB, 0x02);
        public static Guid IID_ICLRRuntimeHost = new Guid(0x90F1A06C, 0x7712, 0x4762, 0x86, 0xB5, 0x7A, 0x5E, 0xBA, 0x6B, 0xDB, 0x02);
        public static Guid CLSID_CLRStrongName = new Guid(0xB79B0ACD, 0xF5CD, 0x409b, 0xB5, 0xA5, 0xA1, 0x62, 0x44, 0x61, 0x0B, 0x92);
        public static Guid IID_ICLRMetaHost = new Guid(0xD332DB9E, 0xB9B3, 0x4125, 0x82, 0x07, 0xA1, 0x48, 0x84, 0xF5, 0x32, 0x16);                 
        public static Guid CLSID_CLRMetaHost = new Guid(0x9280188d, 0xe8e, 0x4867, 0xb3, 0xc, 0x7f, 0xa8, 0x38, 0x84, 0xe8, 0xde);                  
        public static Guid IID_ICLRMetaHostPolicy = new Guid(0xE2190695, 0x77B2, 0x492e, 0x8E, 0x14, 0xC4, 0xB3, 0xA7, 0xFD, 0xD5, 0x93);           
        public static Guid CLSID_CLRMetaHostPolicy = new Guid(0x2ebcd49a, 0x1b47, 0x4a61, 0xb1, 0x3a, 0x4a, 0x3, 0x70, 0x1e, 0x59, 0x4b);           
        public static Guid IID_ICLRDebugging = new Guid(0xd28f3c5a, 0x9634, 0x4206, 0xa5, 0x9, 0x47, 0x75, 0x52, 0xee, 0xfb, 0x10);                 
        public static Guid CLSID_CLRDebugging = new Guid(0xbacc578d, 0xfbdd, 0x48a4, 0x96, 0x9f, 0x2, 0xd9, 0x32, 0xb7, 0x46, 0x34);                
        public static Guid IID_ICLRRuntimeInfo = new Guid(0xBD39D1D2, 0xBA2F, 0x486a, 0x89, 0xB0, 0xB4, 0xB0, 0xCB, 0x46, 0x68, 0x91);              
        public static Guid IID_ICLRStrongName = new Guid(0x9FD93CCF, 0x3280, 0x4391, 0xB3, 0xA9, 0x96, 0xE1, 0xCD, 0xE7, 0x7C, 0x8D);               
        public static Guid IID_ICLRStrongName2 = new Guid(0xC22ED5C5, 0x4B59, 0x4975, 0x90, 0xEB, 0x85, 0xEA, 0x55, 0xC0, 0x06, 0x9B);              
        public static Guid IID_ICLRStrongName3 = new Guid(0x22c7089b, 0xbbd3, 0x414a, 0xb6, 0x98, 0x21, 0x0f, 0x26, 0x3f, 0x1f, 0xed);              
        public static Guid CLSID_CLRDebuggingLegacy = new Guid(0xDF8395B5, 0xA4BA, 0x450b, 0xA7, 0x7C, 0xA9, 0xA4, 0x77, 0x62, 0xC5, 0x20);         
        public static Guid CLSID_CLRProfiling = new Guid(0xbd097ed8, 0x733e, 0x43fe, 0x8e, 0xd7, 0xa9, 0x5f, 0xf9, 0xa8, 0x44, 0x8c);               
        public static Guid IID_ICLRProfiling = new Guid(0xb349abe3, 0xb56f, 0x4689, 0xbf, 0xcd, 0x76, 0xbf, 0x39, 0xd8, 0x88, 0xea);                
        public static Guid IID_ICLRDebuggingLibraryProvider = new Guid(0x3151c08d, 0x4d09, 0x4f9b, 0x88, 0x38, 0x28, 0x80, 0xbf, 0x18, 0xfe, 0x51);

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

    public static class Win32ExtensionMethods
    {
        public static string ToHexString32(this IntPtr ptr) => "0x" + ptr.ToString("x8");
    }
}
