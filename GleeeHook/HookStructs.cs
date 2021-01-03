using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Gleee.Hook
{
    [StructLayout(LayoutKind.Sequential)]
    public struct KeyboardHookStruct
    {
        public int vkCode;
        public int scanCode;
        public int flags;
        public int time;
        public int dwExtraInfo;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct MouseHookStruct
    {
        public Point p;
        public IntPtr HWnd;
        public uint wHitTestCode;
        public int dwExtraInfo;
    }
    //struct HookInfomation
    //{
    //    byte* HOOK_CODE;//指令数组指针
    //    DWORD HOOK_CODE_LENGTH;//指令占的字节数
    //    void* HOOK_PATH;//将要hook的目标函数地址
    //    void* ASM_PATH;//被跳转的函数地址，即申请的内存空间地址
    //}
}
