using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Gleee.Hook
{
    public class Hook
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);
        // 卸载钩子
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        static extern bool UnhookWindowsHookEx(int idHook);
        // 继续下一个钩子
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        static extern int CallNextHookEx(int idHook, int nCode, Int32 wParam, IntPtr lParam);
        public delegate int HookProc(int nCode, IntPtr wParam, IntPtr lParam);
        private int hHook = 0;
        HookProc HookProcedure;
//        private int KeyboardHookProc(int nCode, Int32 wParam, IntPtr IParam)
//        {
//            if (nCode >= 0)
//            {
//#if DEBUG
//                Debug.Print($"接收到消息, nCode={nCode}");
//#endif
//                return 1;
//            }
//            return CallNextHookEx(hKeyboardHook, nCode, wParam, IParam);
//        }
//        public void HookStart()
//        {
//            if (hKeyboardHook == 0)//如果hKeyboardHook==0,钩子安装失败
//            {
//                //创建HookProc实例
//                KeyboardHookProcedure = new HookProc(KeyboardHookProc);
//                //设置线程钩子
//                hKeyboardHook = SetWindowsHookEx(2, KeyboardHookProc, IntPtr.Zero, GetCurrentThreadId());
//                if (hKeyboardHook == 0)
//                {
//                    //终止钩子
//                    throw new Exception("安装钩子失败");
//                }
//            }
//        }
    }
    enum IdHook
    {
        KeyboardThread = 2,
        MouseThread = 7,
        KeyboardGeneral = 13,
        MouseGeneral = 14,
    }
}
