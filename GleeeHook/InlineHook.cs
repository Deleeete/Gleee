using Gleee.Shellcode;
using Gleee.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Gleee.Hook
{
    public class InlineHook
    {
        private IntPtr TargetHandle { get; set; }
        private string target_fn_name;
        /// <summary>
        /// 注入的目标程序
        /// </summary>
        public Process TargetProcess { get; set; }
        /// <summary>
        /// 注入的目标模块
        /// </summary>
        public ProcessModule TargetModule { get; set; }
        /// <summary>
        /// 钩取的目标函数名称
        /// </summary>
        public string TargetFunctionName 
        {
            get => target_fn_name;
            set
            {
                var lp = Kernel32.GetProcAddress(Kernel32.LoadLibraryA(TargetModule.FileName), value);
                if (lp == IntPtr.Zero) throw new Exception($"查找函数指针失败，找不到函数指针：{new Win32Exception().Message}");
                target_fn_name = value;
                TargetFunctionPointer = lp;
            }
        }
        /// <summary>
        /// 目标函数的指针
        /// </summary>
        public IntPtr TargetFunctionPointer { get; private set; }
        /// <summary>
        /// 目标函数的参数类型
        /// </summary>
        public Type[] ParameterTypes { get; set; } = new Type[] { };
        /// <summary>
        /// 自定义函数
        /// </summary>
        public Func<String, int> CustomFunction;

        /// <summary>
        /// 初始化一个InlineHook对象
        /// </summary>
        /// <param name="process">目标进程的Process对象</param>
        /// <param name="module">目标模块的ProcessModule对象</param>
        /// <param name="func_name">目标函数的名称</param>
        /// <param name="custom_func">自定义函数</param>
        public InlineHook(Process process, ProcessModule module, string func_name, Func<String, int> custom_func)
        {
            TargetProcess = process;
            TargetModule = module;
            TargetFunctionName = func_name;
            CustomFunction = custom_func;
        }
        /// <summary>
        /// 初始化一个InlineHook对象
        /// </summary>
        /// <param name="process_name">目标进程的名称</param>
        /// <param name="module_name">目标模块的名称</param>
        /// <param name="func_name">目标函数的名称</param>
        /// <param name="custom_func">自定义函数</param>
        public InlineHook(string process_name, string module_name, string func_name, Func<String, int> custom_func)
        {
            if (process_name.EndsWith(".exe")) process_name = process_name.Substring(0, process_name.Length - 4);   //去除后缀
            if (!module_name.EndsWith(".dll")) module_name += ".dll";   //添加后缀
            Process[] aProcess = Process.GetProcessesByName(process_name);
            if (aProcess.Length == 0) throw new Exception($"无法创建InlineHook对象：无法找到进程'{process_name}'");
            for (int i = 0; i < aProcess.Length; i++)
            {
                if (aProcess[i].HasExited) continue;
                else
                {
                    TargetProcess = aProcess[i];
                    break;
                }
            }
            if (TargetProcess == null) throw new Exception($"无法创建InlineHook对象：确实曾存在进程'{process_name}.exe'，但该程序已经退出");
            var modules = TargetProcess.Modules;
            bool module_found = false;
            foreach (ProcessModule module in modules)
            {
                if (module.ModuleName.ToLower() == module_name.ToLower())
                {
                    TargetModule = module;
                    module_found = true;
                    break;
                }
            }
            if (!module_found) throw new Exception($"无法创建InlineHook对象：该进程未加载模块'{module_name}'");
            TargetFunctionName = func_name;
            Process.EnterDebugMode();
            TargetHandle = Kernel32.OpenProcess(ProcessAccessFlags.All, false, TargetProcess.Id);
            Debug.Print($"打开的句柄为{TargetHandle.ToHexString32()}");
            CustomFunction = custom_func;
        }

        /// <summary>
        /// 为目标程序申请一块内存空间
        /// </summary>
        /// <param name="nSize"></param>
        /// <returns>指向该块内存起始点的指针</returns>
        public IntPtr AllocateMemory(int nSize)
        {
            IntPtr lpCustomMem = Kernel32.VirtualAllocEx(TargetProcess.Handle, IntPtr.Zero, nSize, AllocationType.MEM_COMMIT, PageProtection.PAGE_EXECUTE_READWRITE);
            if (lpCustomMem == IntPtr.Zero)
            {
                var ex = new Win32Exception();
                throw new Exception($"申请内存失败：{ex.Message}");
            }
            return lpCustomMem;
        }
        /// <summary>
        /// 读取目标程序的内存，将缓冲区装满
        /// </summary>
        /// <param name="lpMemory">指向读取区域的指针</param>
        /// <param name="buffer">缓冲区</param>
        /// <returns>读入内存后的缓冲区</returns>
        public byte[] ReadMemory(IntPtr lpMemory, byte[] buffer)
        {
            if (!Kernel32.ReadProcessMemory(TargetProcess.Handle, lpMemory, buffer, buffer.Length, out _))
            {
                var ex = new Win32Exception();
                throw new Exception($"读取内存失败：{ex.Message}");
            }
            return buffer;
        }
        /// <summary>
        /// 读取目标程序的内存
        /// </summary>
        /// <param name="lpMemory">指向读取区域的指针</param>
        /// <param name="buffer">缓冲区</param>
        /// <param name="nSize">读取的字节数</param>
        /// <returns>读入内存后的缓冲区</returns>
        public byte[] ReadMemory(IntPtr lpMemory, byte[] buffer, int nSize)
        {
            if (!Kernel32.ReadProcessMemory(TargetProcess.Handle, lpMemory, buffer, nSize, out _))
            {
                var ex = new Win32Exception();
                throw new Exception($"读取内存失败：{ex.Message}");
            }
            return buffer;
        }
        /// <summary>
        /// 向目标程序内存中的特定地址写入字节数组。不会修改保护级别
        /// </summary>
        /// <param name="lpMemory">写入的地址</param>
        /// <param name="bin">要写入的字节数组</param>
        /// <returns>成功写入的字节数</returns>
        public int WriteMemory(IntPtr lpMemory, byte[] bin)
        {
            if (!Kernel32.WriteProcessMemory(TargetProcess.Handle, lpMemory, bin, bin.Length, out int nBytesWritten))
            {
                var ex = new Win32Exception();
                throw new Exception($"写入内存失败：{ex.Message}");
            }
            return nBytesWritten;
        }
        /// <summary>
        /// 向目标程序内存中的特定地址写入字节数组，将预先修改内存空间的保护级别
        /// </summary>
        /// <param name="lpMemory">写入的地址</param>
        /// <param name="bin">要写入的字节数组</param>
        /// <returns>成功写入的字节数</returns>
        public int WriteProtectedMemory(IntPtr lpMemory, byte[] bin)
        {
            int old_protect = 0;
            if (!Kernel32.VirtualProtect(lpMemory, bin.Length, PageProtection.PAGE_EXECUTE_READWRITE, ref old_protect))
            {
                var ex = new Win32Exception();
                throw new Exception($"设置保护级别失败：{ex.Message}");
            }
            if (!Kernel32.WriteProcessMemory(TargetProcess.Handle, lpMemory, bin, bin.Length, out int nBytesWritten))
            {
                var ex = new Win32Exception();
                throw new Exception($"写入内存失败：{ex.Message}");
            }
            if (!Kernel32.VirtualProtect(lpMemory, bin.Length, (PageProtection)old_protect, ref old_protect))
            {
                var ex = new Win32Exception();
                throw new Exception($"恢复保护级别失败：{ex.Message}");
            }
            return nBytesWritten;
        }
        /// <summary>
        /// 篡改目标函数的头部，使其跳转到指定地址
        /// </summary>
        /// <param name="lpMyMem">目标地址</param>
        /// <returns>写入的字节数</returns>
        public int JmpHead(IntPtr lpMyMem)
        {
            byte[] jmp_to_my_mem = ShellCode.Function.LongJump(lpMyMem);
            return WriteProtectedMemory(TargetFunctionPointer, jmp_to_my_mem);
        }
        /// <summary>
        /// 自动申请并写入内存，篡改目标函数的头部，使其跳转到内存起始处
        /// </summary>
        /// <param name="shellcode">要注入的shellcode</param>
        /// <returns>写入的字节数</returns>
        public IntPtr HookHead(byte[] shellcode)
        {
            //备份
            byte[] backup = new byte[7];
            ReadMemory(TargetFunctionPointer, backup);
            List<byte> sc = new List<byte>();
            sc.AddRange(shellcode);             //结束时要保证寄存器都恢复成初始调用的状态
            sc.AddRange(backup);
            sc.AddRange(ShellCode.Function.LongJump(TargetFunctionPointer + 7));    //跳回函数
            IntPtr lpMyMem = AllocateMemory(shellcode.Length + 14);
            JmpHead(lpMyMem);
            WriteMemory(lpMyMem, sc.ToArray());
            return lpMyMem;
        }
        /// <summary>
        /// 篡改目标函数距离头部offset字节的代码，使其跳转到指定地址
        /// </summary>
        /// <param name="lpMyMem">目标地址</param>
        /// <param name="offset">偏移量</param>
        /// <returns>写入的字节数</returns>
        public int HookOffset(IntPtr lpMyMem, int offset)
        {
            byte[] jmp_to_my_mem = ShellCode.Function.LongJump(lpMyMem);
            return WriteProtectedMemory(TargetFunctionPointer+offset, jmp_to_my_mem);
        }
        /// <summary>
        /// 篡改目标函数头部，使其跳转到自定义函数中
        /// </summary>
        /// <returns>写入的字节数</returns>
        public IntPtr HookHeadToCustomFunction()
        {
            //获取自定义函数的相关信息
            var info = CustomFunction.GetMethodInfo();
            string asm_name = info.Module.FullyQualifiedName;
            string namespace_name = info.ReflectedType.Namespace;
            string class_name = info.ReflectedType.Name;
            string fn_name = info.Name;
            //生成shellcode
            ShellcoderW sc = new ShellcoderW();
            sc.Add(ShellCode.Backup.BackupAll);    //备份寄存器
            sc.LoadAndRunCLR();                                                     //跑CLR
            sc.RunMethodInDefaultAppDomain(asm_name, namespace_name, class_name, fn_name, "#");//调用函数
            sc.Add(ShellCode.Backup.RestoreAll);
            return HookHead(sc.GetBytes());
        }
        /// <summary>
        /// 篡改目标函数头部，使其跳转到自定义函数中
        /// </summary>
        /// <returns>写入的字节数</returns>
        public IntPtr HookHeadToCustomFunction(string asm_name, string namespace_name, string class_name, string method_name, string parameter)
        {
            //生成shellcode
            ShellcoderW sc = new ShellcoderW();
            sc.Add(ShellCode.Backup.BackupAll);    //备份寄存器
            sc.LoadAndRunCLR();                                                     //跑CLR
            sc.RunMethodInDefaultAppDomain(asm_name, namespace_name, class_name, method_name, parameter);//调用函数
            sc.Add(ShellCode.Backup.RestoreAll);
            return HookHead(sc.GetBytes());
        }
    }
}
