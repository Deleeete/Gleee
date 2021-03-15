using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Gleee.Assembly;
using Gleee.Win32;

namespace Gleee.Shellcode
{
    /// <summary>
    /// 一些现成的shellcode
    /// 命名规则：
    /// A_To_B代表：这段shellcode执行结束后，A的指针将被放进B中；
    /// With_X_In_Y代表：这段shellcode执行之前，Y里需要事先存好X的地址；
    /// Full代表：这段shellcode可以独立完成自身功能
    /// </summary>
    public static class ShellCode
    {
        /// <summary>
        /// 执行基本准备工作的shellcode
        /// </summary>
        public static class Init
        {
            /// <summary>
            /// 执行后ebx将指向Kernel32.dll的基地址。先决条件：无
            /// </summary>
            public readonly static byte[] Kernel32_To_EBX_Full =
            {
                    0x53,0x56,0x33,0xc9,0x64,0x8b,0x41,0x30,0x8b,0x40,0x0c,
                    0x8b,0x40,0x14,0xad,0x96,0xad,0x8b,0x58,0x10
                };
            /// <summary>
            /// 执行后edx将指向GetProcAddress的地址。先决条件：ebx中已经保存了Kernel32.dll的地址
            /// </summary>
            public readonly static byte[] GetProcAddress_To_EDX_With_Kernel32_In_EBX =
            {
                    0x8b,0x53,0x3c,0x03,0xd3,0x8b,0x52,0x78,0x03,0xd3,0x8b,0x72,
                    0x20,0x03,0xf3,0x33,0xc9,0x41,0xad,0x03,0xc3,0x81,0x38,0x47,
                    0x65,0x74,0x50,0x75,0xf4,0x81,0x78,0x04,0x72,0x6f,0x63,0x41,
                    0x75,0xeb,0x81,0x78,0x08,0x64,0x64,0x72,0x65,0x75,0xe2,0x8b,
                    0x72,0x24,0x03,0xf3,0x49,0x66,0x8b,0x0c,0x4e,0x8b,0x72,0x1c,
                    0x03,0xf3,0x8b,0x14,0x8e,0x03,0xd3
            };
            /// <summary>
            /// 执行后eax将指向LoadLibraryA的地址。先决条件：ebx中已经保存了Kernel32.dll的地址，edx中已经保存了GetProcAddress的地址
            /// </summary>
            public readonly static byte[] LoadLibraryA_To_EAX_With_Kernel32_In_EBX_And_GetProcAddress_In_EDX =
            {
                    0x33,0xc9,0x51,0x68,0x61,0x72,0x79,0x41,0x68,0x4c,0x69,0x62,0x72,
                    0x68,0x4c,0x6f,0x61,0x64,0x54,0x53,0xff,0xd2,0x83,0xc4,0x0c,0x59,
            };
            /// <summary>
            /// 执行后ebx=kernel32.BaseAddress, ecx=LoadLibraryA, edx=GetProcAddress
            /// </summary>
            public readonly static byte[] BasicInit =
            {
                    0x33,0xc9,0x64,0x8b,0x41,0x30,0x8b,0x40,0x0c,0x8b,0x70,
                    0x14,0xad,0x96,0xad,0x8b,0x58,0x10,0x8b,0x53,0x3c,0x03,
                    0xd3,0x8b,0x52,0x78,0x03,0xd3,0x8b,0x72,0x20,0x03,0xf3,
                    0x33,0xc9,0x41,0xad,0x03,0xc3,0x81,0x38,0x47,0x65,0x74,
                    0x50,0x75,0xf4,0x81,0x78,0x04,0x72,0x6f,0x63,0x41,0x75,
                    0xeb,0x81,0x78,0x08,0x64,0x64,0x72,0x65,0x75,0xe2,0x8b,
                    0x72,0x24,0x03,0xf3,0x49,0x66,0x8b,0x0c,0x4e,0x8b,0x72,
                    0x1c,0x03,0xf3,0x8b,0x14,0x8e,0x03,0xd3,0x33,0xc9,0x53,
                    0x52,0x51,0x68,0x61,0x72,0x79,0x41,0x68,0x4c,0x69,0x62,
                    0x72,0x68,0x4c,0x6f,0x61,0x64,0x54,0x53,0xff,0xd2,0x8b,
                    0xc8,0x83,0xc4,0x10,0x5a,0x5b,
            };
        }
        /// <summary>
        /// 备份、恢复各种寄存器
        /// </summary>
        public static class Backup
        {
            /// <summary>
            /// 备份所有通用寄存器到栈上，顺序：eax->ebx->ecx->edx
            /// </summary>
            public static byte[] BackupCommon = new byte[] { 0x50, 0x53, 0x51, 0x52 };
            /// <summary>
            /// 从栈上还原所有值到通用寄存器，顺序：edx->ecx->ebx->eax
            /// </summary>
            public static byte[] RestoreCommon = new byte[] { 0x5a, 0x59, 0x5b, 0x58 };
            /// <summary>
            /// 备份除了eax之外的通用寄存器到栈上，顺序：ebx->ecx->edx
            /// </summary>
            public static byte[] BackupCommon_NoEAX = new byte[] { 0x53, 0x51, 0x52 };
            /// <summary>
            /// 从栈上还原所有值到除了eax之外的通用寄存器，顺序：edx->ecx->ebx
            /// </summary>
            public static byte[] RestoreCommon_NoEAX = new byte[] { 0x5a, 0x59, 0x5b };
            /// <summary>
            /// 备份所有寄存器，顺序：esp -> ebp -> eax -> ebx -> ecx -> edx -> esi -> edi
            /// </summary>
            public static byte[] BackupAll = new byte[] { 0x54, 0x55, 0x50, 0x53, 0x51, 0x52, 0x56, 0x57 };
            /// <summary>
            /// 从栈上恢复所有寄存器，顺序：edi -> esi -> edx -> ecx -> ebx -> eax -> ebp -> esp
            /// </summary>
            /// <returns></returns>
            public static byte[] RestoreAll = new byte[] { 0x5f, 0x5e, 0x5a, 0x59, 0x5b, 0x58, 0x5d, 0x5c };
            /// <summary>
            /// 备份除eax之外的所有寄存器，顺序：esp -> ebp -> ebx -> ecx -> edx -> esi -> edi
            /// </summary>
            public static byte[] BackupAll_NoEAX = new byte[] { 0x54, 0x55, 0x53, 0x51, 0x52, 0x56, 0x57 };
            /// <summary>
            /// 从栈上恢复除eax之外的所有寄存器，顺序：edi -> esi -> edx -> ecx -> ebx -> ebp -> esp
            /// </summary>
            /// <returns></returns>
            public static byte[] RestoreAll_NoEAX = new byte[] { 0x5f, 0x5e, 0x5a, 0x59, 0x5b, 0x5d, 0x5c };
        }
        /// <summary>
        /// 栈操作
        /// </summary>
        public static class Stack
        {
            //直接版本
            /// <summary>
            /// 将x8立即数压入栈中
            /// </summary>
            /// <param name="x8">要压入的16位整数</param>
            /// <returns>相应的shellcode</returns>
            public static byte[] PushX8(byte x8) => Asm.Assemble($"push 0x{x8:x2}");
            /// <summary>
            /// 将x16立即数压入栈中
            /// </summary>
            /// <param name="x16">要压入的16位整数</param>
            /// <returns>相应的shellcode</returns>
            public static byte[] PushX16(short x16) => Asm.Assemble($"push 0x{x16:x4}");
            /// <summary>
            /// 将x32立即数压入栈中
            /// </summary>
            /// <param name="x32">要压入的32位整数</param>
            /// <returns>相应的shellcode</returns>
            public static byte[] PushX32(int x32) => Asm.Assemble($"push 0x{x32:x8}");
            /// <summary>
            /// 将ASCII字符串压入栈中
            /// </summary>
            /// <param name="str">准备压入栈的字符串</param>
            /// <returns></returns>
            public static byte[] PushStringA(string str)
            {
                List<byte> shellcode = new List<byte>();
                List<byte> strb = Encoding.ASCII.GetBytes(str).ToList<byte>();
                if (strb.Count % 4 == 0) strb.AddRange(new byte[] { 0x00, 0x00, 0x00, 0x00} );
                while (strb.Count % 4 != 0)
                {
                    strb.Add(0x00);     //0a 0b 0c 00
                }
                strb.Reverse();         //00 0c 0b 0a

                for (int i = 0; i < strb.Count / 4; i++ )
                {
                    int j = i * 4;
                    shellcode.AddRange(new byte[] { 0x68, strb[j+3], strb[j+2], strb[j+1], strb[j], }); //push 0x0a0b0c00
                }
                return shellcode.ToArray();
            }
            /// <summary>
            /// 将Unicode（wchar_t）字符串压入栈中
            /// </summary>
            /// <param name="str">准备压入栈的Unicode字符串</param>
            /// <returns></returns>
            public static byte[] PushStringW(string str)
            {
                List<byte> shellcode = new List<byte>();
                List<byte> strb = Encoding.Unicode.GetBytes(str).ToList();  //0a 00 0b 00 0c 00
                if (strb.Count % 4 == 0) strb.AddRange(new byte[] { 0x00, 0x00, 0x00, 0x00 });
                while (strb.Count % 4 != 0)
                {
                    strb.Add(0x00);     //0a 00 0b 00 0c 00 00 00
                }
                strb.Reverse();         //00 00 00 0c 00 0b 00 0a

                for (int i = 0; i < strb.Count / 4; i++)
                {
                    int j = i * 4;
                    shellcode.AddRange(new byte[] { 0x68, strb[j + 3], strb[j + 2], strb[j + 1], strb[j + 0], }); //push 0x0000000c; push 0x000b000a
                }
                return shellcode.ToArray();
            }
            /// <summary>
            /// 将即将调用的函数的参数序列压入栈中，其中的字符串以ASCII编码压入
            /// 注意：如果指针参数不带有out标记，则必须转换为int类型传入；反之，若指针带有out标记，则必须以IntPtr类型传入
            /// </summary>
            /// <param name="parameters">函数参数序列</param>
            /// <returns>相应的shellcode</returns>
            public static byte[] PushParametersA(params object[] parameters)
            {
                List<byte> shellcode = new List<byte>();
                //把所有字符串预压进栈，顺序为左到右
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (parameters[i] is string str) shellcode.AddRange(PushStringA(str));
                }
                //反转
                parameters = parameters.Reverse().ToArray();
                //esp_offset用来跟踪栈的生长。[esp+esp_offset]恒等于栈字符串起始地址
                int esp_offset = 0;
                //str_offset用来指向等待压入的下一个字符串指针到栈字符串的偏移量。[str+str_offset]指向下一个要压的字符串的地址
                int str_offset = 0;
                //从右往左遇到的字符串参数索引
                for (int i = 0; i < parameters.Length; i++)
                {
                    object p = parameters[i];
                    //如果是字符串需要特别计算地址
                    if (p is string str)
                    {
                        int total_offset = esp_offset + str_offset;
                        shellcode.AddRange(Asm.Assemble($"mov ebx,esp"));
                        shellcode.AddRange(Asm.Assemble($"sub ebx,0x{-total_offset:x8}"));
                        shellcode.AddRange(Asm.Assemble($"push ebx"));
                        //shellcode.AddRange(Asm.Assemble($"sub esp,0x{total_offset:x2}"));   //再减回来
                        str_offset += 4 * (str.Length / 4) + 4;  //向上取最接近4的倍数
                        esp_offset += 4;  //刚刚压进去一个指针，esp生长4字节
                    }
                    else if (p is Ptr)
                    {
                        int total_offset = esp_offset + str_offset;
                        shellcode.AddRange(Asm.Assemble($"mov ebx,esp"));
                        shellcode.AddRange(Asm.Assemble($"sub ebx,0x{-total_offset:x8}"));
                        shellcode.AddRange(Asm.Assemble($"push ebx"));
                        str_offset += 4;    //指针在逻辑上可以当成一个四字节字符串来处理
                        esp_offset += 4;
                    }
                    else if (p is bool bo)
                    {
                        if (bo) shellcode.AddRange(Asm.Assemble("push 0x01"));     //push x32
                        else shellcode.AddRange(Asm.Assemble("push 0x00"));
                        esp_offset += 1;
                    }
                    else if (p is byte x8)
                    {
                        shellcode.AddRange(PushX8(x8));
                        esp_offset += 1;
                    }
                    else if (p is short x16)
                    {
                        shellcode.AddRange(PushX16(x16));
                        esp_offset += 2;
                    }
                    else if (p is int x32)
                    {
                        shellcode.AddRange(PushX32(x32));
                        esp_offset += 4;
                    }
                    else if (p is IntPtr ptr)
                    {
                        shellcode.AddRange(PushX32(ptr.ToInt32()));
                        esp_offset += 4;
                    }
                    else throw new Exception($"'遇到不支持的参数类型{p.GetType()}'");
                }
                return shellcode.ToArray();
            }
            /// <summary>
            /// 将即将调用的函数的参数序列压入栈中，其中的字符串以Unicode编码压入. 
            /// 注意：如果指针参数不带有out标记，则必须转换为int类型传入；反之，若指针带有out标记，则必须以IntPtr类型传入
            /// </summary>
            /// <param name="parameters">函数参数序列</param>
            /// <returns>相应的shellcode</returns>
            public static byte[] PushParametersW(params object[] parameters)
            {
                List<byte> shellcode = new List<byte>();
                //把所有字符串预压进栈，顺序为左到右
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (parameters[i] is string str) shellcode.AddRange(PushStringW(str));
                }
                //反转
                parameters = parameters.Reverse().ToArray();
                //esp_offset用来跟踪栈的生长。[esp+esp_offset]恒等于栈字符串起始地址
                int esp_offset = 0;
                //str_offset用来指向等待压入的下一个字符串指针到栈字符串的偏移量。[str+str_offset]指向下一个要压的字符串的地址
                int str_offset = 0;
                //从右往左遇到的字符串参数索引
                for (int i = 0; i < parameters.Length; i++)
                {
                    object p = parameters[i];
                    //如果是字符串需要特别计算地址
                    if (p is string str)
                    {
                        int total_offset = esp_offset + str_offset;
                        shellcode.AddRange(Asm.Assemble($"mov ebx,esp"));
                        shellcode.AddRange(Asm.Assemble($"sub ebx,0x{-total_offset:x8}"));
                        shellcode.AddRange(Asm.Assemble($"push ebx"));
                        //shellcode.AddRange(Asm.Assemble($"sub esp,0x{total_offset:x2}"));   //再减回来
                        str_offset += 4 * (str.Length / 2) + 4;  //向上取最接近4的倍数
                        esp_offset += 4;  //刚刚压进去一个指针，esp生长4字节
                    }
                    else if (p is Ptr)
                    {
                        int total_offset = esp_offset + str_offset;
                        shellcode.AddRange(Asm.Assemble($"mov ebx,esp"));
                        shellcode.AddRange(Asm.Assemble($"sub ebx,0x{-total_offset:x8}"));
                        shellcode.AddRange(Asm.Assemble($"push ebx"));
                        str_offset += 4;    //指针在逻辑上可以当成一个四字节字符串来处理
                        esp_offset += 4;
                    }
                    else if (p is bool bo)
                    {
                        if (bo) shellcode.AddRange(Asm.Assemble("push 0x01"));
                        else shellcode.AddRange(Asm.AssembleAllText("push 0x00"));
                        esp_offset += 1;
                    }
                    else if (p is byte x8)
                    {
                        shellcode.AddRange(PushX8(x8));
                        esp_offset += 1;
                    }
                    else if (p is short x16)
                    {
                        shellcode.AddRange(PushX16(x16));
                        esp_offset += 2;
                    }
                    else if (p is int x32)
                    {
                        shellcode.AddRange(PushX32(x32));
                        esp_offset += 4;
                    }
                    else if (p is IntPtr ptr)
                    {
                        shellcode.AddRange(PushX32(ptr.ToInt32()));
                        esp_offset += 4;
                    }
                    else if (p is Win32_GUID guid)
                    {
                        //最简单的记法：越靠近末尾的字段地址应该越高
                        //所以Data4[7]地址总体最高
                        //所以是0x07060504, 0x03020100
                        shellcode.AddRange(new byte[] 
                        { 
                            0x68, 
                            guid.Data4_4,
                            guid.Data4_5,
                            guid.Data4_6,
                            guid.Data4_7
                        });
                        shellcode.AddRange(new byte[]
                        {
                            0x68,
                            guid.Data4_0,
                            guid.Data4_1,
                            guid.Data4_2,
                            guid.Data4_3
                        });
                        //同理，Data3高于Data2，且每个数高位高于低位（即原顺序）
                        shellcode.AddRange(Asm.Assemble($"push 0x{guid.Data3:x4}{guid.Data2:x4}"));
                        shellcode.AddRange(Asm.Assemble($"push 0x{guid.Data1:x8}"));
                        esp_offset += 16;
                    }
                    else throw new Exception($"'遇到不支持的参数类型{p.GetType()}'");
                }
                return shellcode.ToArray();
            }
            /// <summary>
            /// 清理栈顶上一定数量的字节
            /// </summary>
            /// <param name="bytes_count">清理的字节数</param>
            /// <returns>相应的shellcode</returns>
            public static byte[] ClearStack(int bytes_count) => ClearStack_NoNull(bytes_count);
            /// <summary>
            /// 清理使用PushStringToStackA方法压入的ASCII字符串
            /// </summary>
            /// <param name="str">先前压入的ASCII字符串（或长度相同的任意字符串）</param>
            /// <returns>相应的shellcode</returns>
            public static byte[] ClearStringA(string str) => ClearStringA_NoNull(str);
            /// <summary>
            /// 清理使用PushStringToStackW方法压入的Unicode(wchar_t)字符串
            /// </summary>
            /// <param name="str">先前压入的Unicode字符串（或长度相同的任意字符串）</param>
            /// <returns>相应的shellcode</returns>
            public static byte[] ClearStringW(string str) => ClearStringW_NoNull(str);
            /// <summary>
            /// 清理使用PushParametersA方法压入的参数
            /// </summary>
            /// <param name="parameters">参数序列</param>
            /// <returns>相应的shellcode</returns>
            public static byte[] ClearParametersA(params object[] parameters)
            {
                List<byte> shellcode = new List<byte>();
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (parameters[i] is string str) shellcode.AddRange(ClearStringA(str));
                }
                return shellcode.ToArray();
            }
            /// <summary>
            /// 清理使用PushParametersW方法压入的参数
            /// </summary>
            /// <param name="parameters">参数序列</param>
            /// <returns>相应的shellcode</returns>
            public static byte[] ClearParametersW(params object[] parameters)
            {
                List<byte> shellcode = new List<byte>();
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (parameters[i] is string str) shellcode.AddRange(ClearStringW(str));
                }
                return shellcode.ToArray();
            }

            //无0x00版本
            /// <summary>
            /// 将x8立即数压入栈中
            /// </summary>
            /// <param name="x8">要压入的16位整数</param>
            /// <returns>相应的shellcode</returns>
            public static byte[] PushX8_NoNull(byte x8)
            {
                List<byte> shellcode = new List<byte>(10);
                if (x8 == 0) shellcode.AddRange(Asm.AssembleAllText("push 0x01\nsub [esp],0x01"));
                else shellcode.AddRange(Asm.Assemble($"push 0x{x8:x2}"));
                return shellcode.ToArray();
            }
            /// <summary>
            /// 将x16立即数压入栈中
            /// </summary>
            /// <param name="x16">要压入的16位整数</param>
            /// <returns>相应的shellcode</returns>
            public static byte[] PushX16_NoNull(short x16)
            {
                List<byte> shellcode = new List<byte>(10);
                if (x16 == 0)
                {
                    shellcode.AddRange(Asm.Assemble("xor si,si"));
                    shellcode.AddRange(Asm.Assemble("push si"));
                }
                else
                {
                    byte[] bin = BitConverter.GetBytes(x16);    //00 09 09 b2
                    List<int> nulls = new List<int>();
                    byte[] safe = new byte[2];
                    for (int n = 0; n < bin.Length; n++)
                    {
                        if (bin[n] == 0)
                        {
                            nulls.Add(n);
                            safe[n] = 0x61;
                        }
                        else safe[n] = bin[n];
                    }
                    if (nulls.Count != 0)
                    {
                        shellcode.AddRange(Asm.Assemble($"push 0x{safe[2]:x2}{safe[1]:x2}"));
                        foreach (int index in nulls)
                        {
                            if (index == 0) shellcode.AddRange(Asm.Assemble($"sub [esp],0xff"));
                            else shellcode.AddRange(Asm.Assemble($"sub [esp+0x{index:x2}],0xff"));
                        }
                    }
                    else shellcode.AddRange(Asm.Assemble($"push 0x{x16:x4}"));
                }
                return shellcode.ToArray();
            }
            /// <summary>
            /// 将x32立即数压入栈中
            /// </summary>
            /// <param name="x32">要压入的32位整数</param>
            /// <returns>相应的shellcode</returns>
            public static byte[] PushX32_NoNull(int x32)
            {
                List<byte> shellcode = new List<byte>(10);
                if (x32 == 0) shellcode.AddRange(new byte[] { 0x33, 0xf6, 0x56 });  //xor esi,esi; push esi
                else
                {
                    byte[] bin = BitConverter.GetBytes(x32);    //00 09 09 b2
                    List<int> nulls = new List<int>();
                    byte[] safe = new byte[4];
                    for (int n = 0; n < bin.Length; n++)
                    {
                        if (bin[n] == 0)
                        {
                            nulls.Add(n);
                            safe[n] = 0x61;
                        }
                        else safe[n] = bin[n];
                    }
                    if (nulls.Count != 0)
                    {
                        shellcode.AddRange(new byte[] { 0x68, safe[0], safe[1], safe[2], safe[3] });    //push 0xs3s2s1s0
                        //shellcode.AddRange(Asm.Assemble($"push 0x{safe[3]:x2}{safe[2]:x2}{safe[1]:x2}{safe[0]:x2}"));
                        foreach (int index in nulls)
                        {
                            if (index == 0) shellcode.AddRange(new byte[] { 0x80, 0x2c, 0x24, 0x61 });  //sub [esp],0x61
                            else shellcode.AddRange(new byte[] { 0x83, 0x6c, 0x24, (byte)index, 0x61 });     // sub [esp+index],0x61"
                        }
                    }
                    else shellcode.AddRange(Asm.Assemble($"push 0x{x32:x8}"));
                }
                return shellcode.ToArray();
            }
            /// <summary>
            /// 将ASCII字符串压入栈中
            /// </summary>
            /// <param name="str">准备压入栈的字符串</param>
            /// <returns></returns>
            public static byte[] PushStringA_NoNull(string str)
            {
                List<byte> shellcode = new List<byte>();
                int length = str.Length;
                //残余字符串的长度
                int remainder = length % 4;
                //完整切片的个数
                int slice_count = length / 4 + 1;
                //在字符串末尾补a至长度为4的倍数
                str = str.PadRight(slice_count * 4, 'a');
                //切片
                List<string> slices = new List<string>();
                for (int n = 0; n < slice_count; n++)
                {
                    slices.Add(str.Substring(4 * n, 4));
                }
                //反转切片顺序
                slices.Reverse();
                //单独处理字符串尾
                shellcode.AddRange(Asm.Assemble($"push {Asm.StrToX32A(slices[0])}"));  //压入第0个元素
                for (int i = 0; i < 4 - remainder; i++) //补了几个a就需要减几次
                {
                    int offset = 0x03 - i;
                    if (offset == 0) shellcode.AddRange(Asm.Assemble($"sub [esp],0x61"));
                    else shellcode.AddRange(Asm.Assemble($"sub [esp+0x{offset:x2}],0x61"));
                }
                //然后才是剩下的
                for (int n = 1; n < slices.Count; n++)
                {
                    shellcode.AddRange(Asm.Assemble($"push {Asm.StrToX32A(slices[n])}"));
                }
                return shellcode.ToArray();
            }
            /// <summary>
            /// 将Unicode（wchar_t）字符串压入栈中
            /// </summary>
            /// <param name="str">准备压入栈的Unicode字符串</param>
            /// <returns></returns>
            public static byte[] PushStringW_NoNull(string str)
            {
                List<byte> shellcode = new List<byte>();
                int length = str.Length;

                int remainder = length % 2;
                int slice_count = length / 2 + 1;
                //补齐偶数个字
                str = str.PadRight(slice_count * 2, 'a');
                //切片
                List<string> slices = new List<string>();
                for (int n = 0; n < slice_count; n++)
                {
                    slices.Add(str.Substring(2 * n, 2));
                }
                //反转切片顺序
                slices.Reverse();
                shellcode.AddRange(PushX32_NoNull(Asm.StrToIntW(slices[0])));  //使用无0x00方法压进去

                //处理补进去的a字符串
                //至少补了一个，必然要删除[esp+0x02]
                shellcode.AddRange(new byte[] { 0x83, 0x6c, 0x24, 0x02, 0x61 });   //sub [esp+0x02],0x61
                //这意味着补了两个，还要多删除一个[esp]
                if (remainder != 1) shellcode.AddRange(new byte[] { 0x80, 0x2c, 0x24, 0x61 });    //sub [esp],0x61

                //剩下的按正常no null int压就好
                for (int i = 1; i < slice_count; i++)
                {
                    shellcode.AddRange(PushX32_NoNull(Asm.StrToIntW(slices[i])));
                }
                return shellcode.ToArray();
            }
            /// <summary>
            /// 将即将调用的函数的参数序列压入栈中，其中的字符串以ASCII编码压入
            /// </summary>
            /// <param name="parameters">函数参数序列</param>
            /// <returns>相应的shellcode</returns>
            public static byte[] PushParametersA_NoNull(params object[] parameters)
            {
                List<byte> shellcode = new List<byte>();
                //把所有字符串预压进栈，顺序为左到右
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (parameters[i] is string str) shellcode.AddRange(Stack.PushStringA_NoNull(str));
                }
                //反转
                parameters = parameters.Reverse().ToArray();
                //esp_offset用来跟踪栈的生长。[esp+esp_offset]恒等于栈字符串起始地址
                int esp_offset = 0;
                //str_offset用来指向等待压入的下一个字符串指针到栈字符串的偏移量。[str+str_offset]指向下一个要压的字符串的地址
                int str_offset = 0;
                //从右往左遇到的字符串参数索引
                for (int i = 0; i < parameters.Length; i++)
                {
                    object p = parameters[i];
                    //如果是字符串需要特别计算地址
                    if (p is string str)
                    {
                        int total_offset = esp_offset + str_offset;
                        shellcode.AddRange(Asm.Assemble($"mov ebx,esp"));
                        shellcode.AddRange(Asm.Assemble($"sub ebx,0x{-total_offset:x8}"));
                        shellcode.AddRange(Asm.Assemble($"push ebx"));
                        //shellcode.AddRange(Asm.Assemble($"sub esp,0x{total_offset:x2}"));   //再减回来
                        str_offset += 4 * (str.Length / 4) + 4;  //向上取最接近4的倍数
                        esp_offset += 4;  //刚刚压进去一个指针，esp生长4字节
                    }
                    else if (p is bool bo)
                    {
                        if (bo) shellcode.AddRange(Asm.Assemble("push 0x01"));
                        else shellcode.AddRange(Asm.AssembleAllText("push 0x01\nsub [esp],0x01"));
                        esp_offset += 1;
                    }
                    else if (p is byte x8)
                    {
                        shellcode.AddRange(Stack.PushX8_NoNull(x8));
                        esp_offset += 1;
                    }
                    else if (p is short x16)
                    {
                        shellcode.AddRange(Stack.PushX16_NoNull(x16));
                        esp_offset += 2;
                    }
                    else if (p is int x32)
                    {
                        shellcode.AddRange(Stack.PushX32_NoNull(x32));
                        esp_offset += 4;
                    }
                    else if (p is IntPtr ptr)
                    {
                        int intg = ptr.ToInt32();
                        shellcode.AddRange(Stack.PushX32_NoNull(intg));
                        esp_offset += 4;
                    }
                    else throw new Exception($"'遇到不支持的参数类型{p.GetType()}'");
                }
                return shellcode.ToArray();
            }
            /// <summary>
            /// 将即将调用的函数的参数序列压入栈中，其中的字符串以Unicode编码压入
            /// </summary>
            /// <param name="parameters">函数参数序列</param>
            /// <returns>相应的shellcode</returns>
            public static byte[] PushParametersW_NoNull(params object[] parameters)
            {
                List<byte> shellcode = new List<byte>();
                //把所有字符串预压进栈，顺序为左到右
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (parameters[i] is string str) shellcode.AddRange(Stack.PushStringW_NoNull(str));
                }
                //反转
                parameters = parameters.Reverse().ToArray();
                //esp_offset用来跟踪栈的生长。[esp+esp_offset]恒等于栈字符串起始地址
                int esp_offset = 0;
                //str_offset用来指向等待压入的下一个字符串指针到栈字符串的偏移量。[str+str_offset]指向下一个要压的字符串的地址
                int str_offset = 0;
                //从右往左遇到的字符串参数索引
                for (int i = 0; i < parameters.Length; i++)
                {
                    object p = parameters[i];
                    //如果是字符串需要特别计算地址
                    if (p is string str)
                    {
                        int total_offset = esp_offset + str_offset;
                        shellcode.AddRange(Asm.Assemble($"mov ebx,esp"));
                        shellcode.AddRange(Asm.Assemble($"sub ebx,0x{-total_offset:x8}"));
                        shellcode.AddRange(Asm.Assemble($"push ebx"));
                        //shellcode.AddRange(Asm.Assemble($"sub esp,0x{total_offset:x2}"));   //再减回来
                        str_offset += 4 * (str.Length / 2) + 4;  //向上取最接近4的倍数
                        esp_offset += 4;  //刚刚压进去一个指针，esp生长4字节
                    }
                    else if (p is bool bo)
                    {
                        if (bo) shellcode.AddRange(Asm.Assemble("push 0x01"));
                        else shellcode.AddRange(Asm.AssembleAllText("push 0x01\nsub [esp],0x01"));
                        esp_offset += 1;
                    }
                    else if (p is byte x8)
                    {
                        shellcode.AddRange(Stack.PushX8_NoNull(x8));
                        esp_offset += 1;
                    }
                    else if (p is short x16)
                    {
                        shellcode.AddRange(Stack.PushX16_NoNull(x16));
                        esp_offset += 2;
                    }
                    else if (p is int x32)
                    {
                        shellcode.AddRange(Stack.PushX32_NoNull(x32));
                        esp_offset += 4;
                    }
                    else if (p is IntPtr ptr)
                    {
                        int intg = ptr.ToInt32();
                        shellcode.AddRange(Stack.PushX32_NoNull(intg));
                        esp_offset += 4;
                    }
                    else throw new Exception($"'遇到不支持的参数类型{p.GetType()}'");
                }
                return shellcode.ToArray();
            }
            /// <summary>
            /// 清理栈顶上一定数量的字节
            /// </summary>
            /// <param name="bytes_count">清理的字节数</param>
            /// <returns>相应的shellcode</returns>
            public static byte[] ClearStack_NoNull(int bytes_count)
            {
                if (bytes_count > byte.MaxValue) throw new Exception("待清理的栈长度超过了八位偏移量最大值");
                return new byte[] { 0x83, 0xc4, (byte)bytes_count };    //add esp,0x{bytes_count:x2}
            }
            /// <summary>
            /// 清理使用PushStringToStackA方法压入的ASCII字符串
            /// </summary>
            /// <param name="str">先前压入的ASCII字符串（或长度相同的任意字符串）</param>
            /// <returns>相应的shellcode</returns>
            public static byte[] ClearStringA_NoNull(string str)
            {
                int length = str.Length;
                //完整切片的个数
                int slice_count = length / 4;
                return ClearStack_NoNull(slice_count * 4 + 4);
            }
            /// <summary>
            /// 清理使用PushStringToStackW方法压入的Unicode(wchar_t)字符串
            /// </summary>
            /// <param name="str">先前压入的Unicode字符串（或长度相同的任意字符串）</param>
            /// <returns>相应的shellcode</returns>
            public static byte[] ClearStringW_NoNull(string str)
            {
                int length = str.Length * 2;
                //完整切片的个数
                int slice_count = length / 4;
                return ClearStack_NoNull(slice_count * 4 + 4);
            }
            /// <summary>
            /// 清理使用PushParametersA方法压入的参数
            /// </summary>
            /// <param name="parameters">参数序列</param>
            /// <returns>相应的shellcode</returns>
            public static byte[] ClearParametersA_NoNull(params object[] parameters)
            {
                List<byte> shellcode = new List<byte>();
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (parameters[i] is string str) shellcode.AddRange(ClearStringA_NoNull(str));
                }
                return shellcode.ToArray();
            }
            /// <summary>
            /// 清理使用PushParametersW方法压入的参数
            /// </summary>
            /// <param name="parameters">参数序列</param>
            /// <returns>相应的shellcode</returns>
            public static byte[] ClearParametersW_NoNull(params object[] parameters)
            {
                List<byte> shellcode = new List<byte>();
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (parameters[i] is string str) shellcode.AddRange(ClearStringW_NoNull(str));
                }
                return shellcode.ToArray();
            }
        }
        /// <summary>
        /// 可完成某些特定或通用的完整功能
        /// </summary>
        public static class Function
        {
            /// <summary>
            /// 跳转至绝对地址（mov eax,lpDst; jmp eax）
            /// </summary>
            /// <param name="lpDst">目标地址</param>
            /// <returns>相应的shellcode</returns>
            public static byte[] LongJump(IntPtr lpDst)
            {
                byte[] bin = BitConverter.GetBytes(lpDst.ToInt32());
                return new byte[] { 0xb8, bin[0], bin[1], bin[2], bin[3], 0xff, 0xe0 };     //mov eax,x32; jmp eax
            }
            /// <summary>
            /// 查找任意函数指针。先决条件：已经进行过标准初始化
            /// </summary>
            /// <param name="dll_name">DLL名称（ANSI）</param>
            /// <param name="func_name">函数名称（ANSI）</param>
            /// <returns></returns>
            public static byte[] GetProcAddress(string dll_name, string func_name)
            {
                List<byte> shellcode = new List<byte>();
                shellcode.AddRange(LoadLibraryA(dll_name));
                //此时eax=hModule
                shellcode.AddRange(Stack.PushStringA(func_name));   //push func_name
                shellcode.AddRange(Asm.Assemble("push esp"));               //push lp_func_name
                shellcode.AddRange(Asm.Assemble("push eax"));               //push hModule
                shellcode.AddRange(Asm.Assemble("call ecx"));
                return shellcode.ToArray();
            }
            /// <summary>
            /// 执行LoadLibraryA()。先决条件：已经进行过标准初始化
            /// </summary>
            /// <param name="lib_name">DLL文件的绝对路径(ANSI)</param>
            /// <returns>执行LoadLibraryA(lib_name)的shellcode</returns>
            public static byte[] LoadLibraryA(string lib_name)
            {
                List<byte> shellcode = new List<byte>();
                shellcode.AddRange(Stack.PushStringA(lib_name));
                shellcode.AddRange(Asm.Assemble("push esp"));              
                shellcode.AddRange(Asm.Assemble("call ecx"));           //调用LoadLibraryA(lib_name)
                shellcode.AddRange(Stack.ClearStringA(lib_name));    //调用完毕，清理栈字符串，ClearStack和PushStringAsParameter成对使用可保证栈平衡
                return shellcode.ToArray();
            }
            /// <summary>
            /// 执行LoadLibraryA。先决条件：无
            /// </summary>
            /// <param name="dll_name">DLL文件的绝对路径(ANSI)</param>
            /// <returns>执行LoadLibraryA(lib_name)的shellcode</returns>
            public static byte[] LoadLibraryA_Sharp(string dll_name)
            {
                List<byte> shellcode = new List<byte>();
                IntPtr hKernel = Kernel32.LoadLibraryA("kernel32.dll");
                if (hKernel == IntPtr.Zero) throw new Exception($"模块句柄获取失败：{new Win32Exception().Message}");
                IntPtr lpLoad = Kernel32.GetProcAddress(hKernel, "LoadLibraryA");
                if (lpLoad == IntPtr.Zero) throw new Exception($"函数指针获取失败：{new Win32Exception().Message}");
                shellcode.AddRange(Asm.Assemble($"mov ebx,{hKernel.ToHexString32()}"));
                shellcode.AddRange(Asm.Assemble($"mov edx,{lpLoad.ToHexString32()}"));
                shellcode.AddRange(Stack.PushStringA(dll_name));
                shellcode.AddRange(Asm.Assemble("push esp"));
                shellcode.AddRange(Asm.Assemble("call edx"));           //调用LoadLibraryA(lib_name)
                shellcode.AddRange(Stack.ClearStringA(dll_name));    //调用完毕，清理栈字符串，ClearStack和PushStringAsParameter成对使用可保证栈平衡
                return shellcode.ToArray();
            }

            /// <summary>
            /// 调用任意函数。先决条件：已经进行过标准初始化
            /// </summary>
            /// <param name="dll_name">DLL名称（ANSI）</param>
            /// <param name="func_name">函数名称（ANSI）</param>
            /// <param name="parameters">参数字节数组，顺序为左到右</param>
            /// <returns></returns>
            public static byte[] CallFunctionA(string dll_name, string func_name, params object[] parameters)
            {
                List<byte> shellcode = new List<byte>();
                shellcode.AddRange(LoadLibraryA(dll_name));
                //此时eax=hModule
                shellcode.AddRange(Stack.PushStringA(func_name));   //push func_name
                shellcode.AddRange(Asm.Assemble("push esp"));       //push lp_func_name
                shellcode.AddRange(Asm.Assemble("push eax"));       //push hModule
                shellcode.AddRange(Asm.Assemble("call edx"));       //GetProcAddress(hModule, &func_name)
                shellcode.AddRange(Stack.ClearStringA(func_name));
                //此时eax=func，栈已重新平衡
                shellcode.AddRange(Stack.PushParametersA(parameters));  //压入参数
                shellcode.AddRange(Asm.Assemble("call eax"));           //调用
                //堆栈平衡
                shellcode.AddRange(Stack.ClearParametersA(parameters));
                return shellcode.ToArray();
            }
            /// <summary>
            /// 调用任意函数，但是不预先加载DLL。先决条件：已经进行过标准初始化，且已经加载过相关DLL文件
            /// </summary>
            /// <param name="func_name">函数名称（ANSI）</param>
            /// <param name="parameters">参数字节数组，顺序为左到右</param>
            /// <returns></returns>
            public static byte[] CallFunctionALite(string func_name, params object[] parameters)
            {
                List<byte> shellcode = new List<byte>();
                //此时eax=hModule
                shellcode.AddRange(Stack.PushStringA(func_name));   //push func_name
                shellcode.AddRange(Asm.Assemble("push esp"));               //push lp_func_name
                shellcode.AddRange(Asm.Assemble("push eax"));               //push hModule
                shellcode.AddRange(Asm.Assemble("call edx"));               //GetProcAddress(hModule, &func_name)
                shellcode.AddRange(Stack.ClearStringA(func_name));
                //此时eax=func，栈已重新平衡
                shellcode.AddRange(Stack.PushParametersA(parameters));  //压入参数
                shellcode.AddRange(Asm.Assemble("call eax"));           //调用
                //堆栈平衡
                shellcode.AddRange(Stack.ClearParametersA(parameters));
                return shellcode.ToArray();
            }
            /// <summary>
            /// 直接利用C#得到DLL和函数地址，生成shellcode调用任意函数。
            /// </summary>
            /// <param name="dll_name">DLL名称</param>
            /// <param name="fn_name">函数名称</param>
            /// <param name="parameters">函数的参数</param>
            /// <returns>相应的shellcode</returns>
            public static byte[] CallFunctionA_Sharp(string dll_name, string fn_name, params object[] parameters)
            {
                List<byte> shellcode = new List<byte>();
                //确保目标程序已加载DLL
                shellcode.AddRange(LoadLibraryA_Sharp(dll_name));

                IntPtr lpModule = Kernel32.LoadLibraryA(dll_name);
                if (lpModule == IntPtr.Zero) throw new Exception($"模块句柄获取失败：{new Win32Exception().Message}");
                IntPtr lpFn = Kernel32.GetProcAddress(lpModule, fn_name);
                if (lpModule == IntPtr.Zero) throw new Exception($"函数指针获取失败：{new Win32Exception().Message}");

                shellcode.AddRange(Asm.Assemble($"mov eax,0x{lpFn.ToInt32():x8}"));  //eax=lpFn
                //压参数
                shellcode.AddRange(Stack.PushParametersA(parameters));
                shellcode.AddRange(Asm.Assemble("call eax"));
                //堆栈平衡
                shellcode.AddRange(Stack.ClearParametersA(parameters));
                return shellcode.ToArray();
            }
            /// <summary>
            /// 直接利用C#得到函数地址，但是不预先加载DLL
            /// </summary>
            /// <param name="dll_name"></param>
            /// <param name="fn_name">函数名称</param>
            /// <param name="parameters">函数的参数</param>
            /// <returns>相应的shellcode</returns>
            public static byte[] CallFunctionALite_Sharp(string dll_name, string fn_name, params object[] parameters)
            {
                List<byte> shellcode = new List<byte>();
                IntPtr lpModule = Kernel32.LoadLibraryA(dll_name);
                if (lpModule == IntPtr.Zero) throw new Exception($"模块句柄获取失败：{new Win32Exception().Message}");
                IntPtr lpFn = Kernel32.GetProcAddress(lpModule, fn_name);
                if (lpModule == IntPtr.Zero) throw new Exception($"函数指针获取失败：{new Win32Exception().Message}");

                shellcode.AddRange(Asm.Assemble($"mov eax,0x{lpFn.ToInt32():x8}"));  //eax=lpFn
                //压参数
                shellcode.AddRange(Stack.PushParametersA(parameters));
                shellcode.AddRange(Asm.Assemble("call eax"));
                //堆栈平衡
                shellcode.AddRange(Stack.ClearParametersA(parameters));
                return shellcode.ToArray();
            }
            /// <summary>
            /// 调用任意函数。先决条件：已经进行过标准初始化
            /// </summary>
            /// <param name="dll_name">DLL名称（ANSI）</param>
            /// <param name="func_name">函数名称（ANSI）</param>
            /// <param name="parameters">参数字节数组，顺序为左到右</param>
            /// <returns></returns>
            public static byte[] CallFunctionW(string dll_name, string func_name, params object[] parameters)
            {
                List<byte> shellcode = new List<byte>();
                shellcode.AddRange(LoadLibraryA(dll_name));
                //此时eax=hModule
                shellcode.AddRange(Stack.PushStringA(func_name));   //push func_name
                shellcode.AddRange(Asm.Assemble("push esp"));               //push lp_func_name
                shellcode.AddRange(Asm.Assemble("push eax"));            //push eax
                shellcode.AddRange(Asm.Assemble("call edx"));
                shellcode.AddRange(Stack.ClearStringA(func_name));
                //此时eax=func，栈已重新平衡
                shellcode.AddRange(Stack.PushParametersW(parameters));
                shellcode.AddRange(Asm.Assemble("call eax"));
                //堆栈平衡
                shellcode.AddRange(Stack.ClearParametersW(parameters));
                return shellcode.ToArray();
            }
            /// <summary>
            /// 调用任意函数，但是不预先加载DLL。先决条件：已经进行过标准初始化，且已经加载过相关DLL文件
            /// </summary>
            /// <param name="func_name">函数名称（ANSI）</param>
            /// <param name="parameters">参数字节数组，顺序为左到右</param>
            /// <returns></returns>
            public static byte[] CallFunctionWLite(string func_name, params object[] parameters)
            {
                List<byte> shellcode = new List<byte>();
                //此时eax=hModule
                shellcode.AddRange(Stack.PushStringA(func_name));   //push func_name
                shellcode.AddRange(Asm.Assemble("push esp"));               //push lp_func_name
                shellcode.AddRange(Asm.Assemble("push eax"));            //push eax
                shellcode.AddRange(Asm.Assemble("call edx"));
                shellcode.AddRange(Stack.ClearStringA(func_name));
                //此时eax=func，栈已重新平衡
                shellcode.AddRange(Stack.PushParametersW(parameters));
                shellcode.AddRange(Asm.Assemble("call eax"));
                //堆栈平衡
                shellcode.AddRange(Stack.ClearParametersW(parameters));
                return shellcode.ToArray();
            }
            /// <summary>
            /// 直接利用C#得到DLL和函数地址，生成shellcode调用任意函数。
            /// </summary>
            /// <param name="dll_name">DLL名称</param>
            /// <param name="fn_name">函数名称</param>
            /// <param name="parameters">函数的参数</param>
            /// <returns>相应的shellcode</returns>
            public static byte[] CallFunctionW_Sharp(string dll_name, string fn_name, params object[] parameters)
            {
                List<byte> shellcode = new List<byte>();
                //确保目标程序已加载DLL
                shellcode.AddRange(LoadLibraryA_Sharp(dll_name));

                IntPtr lpModule = Kernel32.LoadLibraryA(dll_name);
                if (lpModule == IntPtr.Zero) throw new Exception($"模块句柄获取失败：{new Win32Exception().Message}");
                IntPtr lpFn = Kernel32.GetProcAddress(lpModule, fn_name);
                if (lpModule == IntPtr.Zero) throw new Exception($"函数指针获取失败：{new Win32Exception().Message}");

                shellcode.AddRange(Asm.Assemble($"mov eax,0x{lpFn.ToInt32():x8}"));  //eax=lpFn
                //压参数
                shellcode.AddRange(Stack.PushParametersW(parameters));
                shellcode.AddRange(Asm.Assemble("call eax"));
                //堆栈平衡
                shellcode.AddRange(Stack.ClearParametersW(parameters));
                return shellcode.ToArray();
            }
            /// <summary>
            /// 直接利用C#得到函数地址，但是不预先加载DLL
            /// </summary>
            /// <param name="dll_name"></param>
            /// <param name="fn_name">函数名称</param>
            /// <param name="parameters">函数的参数</param>
            /// <returns>相应的shellcode</returns>
            public static byte[] CallFunctionWLite_Sharp(string dll_name, string fn_name, params object[] parameters)
            {
                List<byte> shellcode = new List<byte>();
                IntPtr lpModule = Kernel32.LoadLibraryA(dll_name);
                if (lpModule == IntPtr.Zero) throw new Exception($"模块句柄获取失败：{new Win32Exception().Message}");
                IntPtr lpFn = Kernel32.GetProcAddress(lpModule, fn_name);
                if (lpModule == IntPtr.Zero) throw new Exception($"函数指针获取失败：{new Win32Exception().Message}");

                shellcode.AddRange(Asm.Assemble($"mov eax,0x{lpFn.ToInt32():x8}"));  //eax=lpFn
                //压参数
                shellcode.AddRange(Stack.PushParametersW(parameters));
                shellcode.AddRange(Asm.Assemble("call eax"));
                //堆栈平衡
                shellcode.AddRange(Stack.ClearParametersW(parameters));
                return shellcode.ToArray();
            }
            /// <summary>
            /// 加载CLR
            /// </summary>
            /// <returns></returns>
            public static byte[] LoadCLR()
            {
                List<byte> shellcode = new List<byte>();
                shellcode.AddRange(Init.BasicInit);
                shellcode.AddRange(Backup.BackupCommon_NoEAX);
                shellcode.AddRange(Function.LoadLibraryA("mscoree.dll"));
                //eax = hMsCorEE
                shellcode.AddRange(Backup.RestoreCommon_NoEAX);
                shellcode.AddRange(Stack.PushParametersA("CLRCreateInstance"));
                shellcode.AddRange(Asm.Assemble($"push eax"));
                shellcode.AddRange(Asm.Assemble($"call edx"));
                shellcode.AddRange(Stack.PushParametersW(Win32_GUID.CLSID_CLRMetaHost, Win32_GUID.IID_ICLRMetaHost, new Ptr(0)));
                shellcode.AddRange(Asm.Assemble($"call eax"));
                return shellcode.ToArray();
            }
            private static byte[] 脑瘫的最后一个参数是指针的那种函数的EAX调用(params object[] parameters)
            {
                List<byte> shellcode = new List<byte>();

                return null;
            }


            /// <summary>
            /// 查找任意函数指针。先决条件：已经进行过标准初始化
            /// </summary>
            /// <param name="dll_name">DLL名称（ANSI）</param>
            /// <param name="func_name">函数名称（ANSI）</param>
            /// <returns></returns>
            public static byte[] GetProcAddress_NoNull(string dll_name, string func_name)
            {
                List<byte> shellcode = new List<byte>();
                shellcode.AddRange(Backup.BackupCommon_NoEAX);
                shellcode.AddRange(LoadLibraryA_NoNull(dll_name));
                //此时eax=hModule
                shellcode.AddRange(Stack.PushStringA_NoNull(func_name));   //push func_name
                shellcode.AddRange(Asm.Assemble("push esp"));               //push lp_func_name
                shellcode.AddRange(Asm.Assemble("push eax"));               //push hModule
                shellcode.AddRange(Asm.Assemble("call ecx"));
                shellcode.AddRange(Backup.RestoreCommon_NoEAX);                     //恢复备份，以保证标准状态不被调用的函数破坏
                return shellcode.ToArray();
            }
            /// <summary>
            /// 执行LoadLibraryA()。先决条件：已经进行过标准初始化
            /// </summary>
            /// <param name="lib_name">DLL文件的绝对路径(ANSI)</param>
            /// <returns>执行LoadLibraryA(lib_name)的shellcode</returns>
            public static byte[] LoadLibraryA_NoNull(string lib_name)
            {
                List<byte> shellcode = new List<byte>();
                shellcode.AddRange(Backup.BackupCommon_NoEAX);
                shellcode.AddRange(Stack.PushStringA_NoNull(lib_name));
                shellcode.AddRange(Asm.Assemble("push esp"));           
                shellcode.AddRange(Asm.Assemble("call ecx"));           //调用LoadLibraryA(lib_name)
                shellcode.AddRange(Stack.ClearStringA_NoNull(lib_name));    //调用完毕，清理栈字符串，ClearStack和PushStringAsParameter成对使用可保证栈平衡
                shellcode.AddRange(Backup.RestoreCommon_NoEAX);          //恢复备份，以保证标准状态不被调用的函数破坏
                return shellcode.ToArray();
            }
            /// <summary>
            /// 调用任意函数。先决条件：已经进行过标准初始化
            /// </summary>
            /// <param name="dll_name">DLL名称（ANSI）</param>
            /// <param name="func_name">函数名称（ANSI）</param>
            /// <param name="parameters">参数字节数组，顺序为左到右</param>
            /// <returns></returns>
            public static byte[] CallFunctionA_NoNull(string dll_name, string func_name, params object[] parameters)
            {
                List<byte> shellcode = new List<byte>();
                shellcode.AddRange(Backup.BackupCommon_NoEAX);
                shellcode.AddRange(LoadLibraryA_NoNull(dll_name));
                //此时eax=hModule
                shellcode.AddRange(Stack.PushStringA_NoNull(func_name));   //push func_name
                shellcode.AddRange(Asm.Assemble("push esp"));               //push lp_func_name
                shellcode.AddRange(Asm.Assemble("push eax"));               //push hModule
                shellcode.AddRange(Asm.Assemble("call edx"));               //GetProcAddress(hModule, &func_name)
                shellcode.AddRange(Stack.ClearStringA_NoNull(func_name));
                //此时eax=func，栈已重新平衡
                shellcode.AddRange(Stack.PushParametersA_NoNull(parameters));
                shellcode.AddRange(Asm.Assemble("call eax"));
                //堆栈平衡
                shellcode.AddRange(Stack.ClearParametersA_NoNull(parameters));
                shellcode.AddRange(Backup.RestoreCommon_NoEAX);                     //恢复备份，以保证标准状态不被调用的函数破坏
                return shellcode.ToArray();
            }
            /// <summary>
            /// 调用任意函数。先决条件：已经进行过标准初始化
            /// </summary>
            /// <param name="dll_name">DLL名称（ANSI）</param>
            /// <param name="func_name">函数名称（ANSI）</param>
            /// <param name="parameters">参数字节数组，顺序为左到右</param>
            /// <returns></returns>
            public static byte[] CallFunctionW_NoNull(string dll_name, string func_name, params object[] parameters)
            {
                List<byte> shellcode = new List<byte>();
                shellcode.AddRange(Backup.BackupCommon_NoEAX);
                shellcode.AddRange(LoadLibraryA_NoNull(dll_name));
                //此时eax=hModule
                shellcode.AddRange(Stack.PushStringA_NoNull(func_name));   //push func_name
                shellcode.AddRange(Asm.Assemble("push esp"));               //push lp_func_name
                shellcode.AddRange(Asm.Assemble("push eax"));               //push hModule
                shellcode.AddRange(Asm.Assemble("call edx"));               //GetProcAddress(hModule, &func_name)
                shellcode.AddRange(Stack.ClearStringA_NoNull(func_name));
                //此时eax=func，栈已重新平衡
                shellcode.AddRange(Stack.PushParametersW_NoNull(parameters));
                shellcode.AddRange(Asm.Assemble("call eax"));
                //堆栈平衡
                shellcode.AddRange(Stack.ClearParametersW_NoNull(parameters));
                shellcode.AddRange(Backup.RestoreCommon_NoEAX);                     //恢复备份，以保证标准状态不被调用的函数破坏
                return shellcode.ToArray();
            }
        }

        /// <summary>
        /// 检查shellcode中是否含0x00
        /// </summary>
        /// <param name="shellcode">待检查的shellcode</param>
        /// <returns>所有0x00的索引列表</returns>
        public static bool HasNull(byte[] shellcode, out int[] indexes)
        {
            List<int> nulls = new List<int>(shellcode.Length / 2);
            for (int i = 0; i < shellcode.Length; i++)
            {
                if (shellcode[i] == 0x00) nulls.Add(i);
            }
            indexes = nulls.ToArray();
            return nulls.Count != 0;
        }
        /// <summary>
        /// 获取参数类型在x86机器的字节长度
        /// </summary>
        /// <param name="obj">参数</param>
        /// <returns>参数类型在x86机器的字节长度</returns>
        private static int SizeOf(object obj)
        {
            if (obj is bool) return 1;
            else if (obj is byte) return 1;
            else if (obj is char) return 2;
            else if (obj is short || obj is ushort) return 2;
            else if (obj is int || obj is uint) return 4;
            else if (obj is long || obj is ulong) return 8;
            else if (obj is string) return ((string)obj).Length;
            else throw new Exception("未知的参数类型");
        }
    }

    /// <summary>
    /// 负责栈数据的索引及管理，字符串编码方式为ASCII
    /// </summary>
    public class StackDataA
    {
        private Dictionary<object, int> dict = new Dictionary<object, int>();
        private List<object> index = new List<object>();
        private int data_size = 0;
        private int SizeOf(object data)
        {
            if (data is byte) return 1;
            else if (data is short || data is ushort) return 2;
            else if (data is int || data is uint || data is IntPtr || data is UIntPtr) return 4;
            else if (data is Guid || data is Win32_GUID) return 16;
            else if (data is string str) return (str.Length / 4 + 1) * 4;
            else throw new Exception("遇到不支持的数据类型");
        }

        /// <summary>
        /// 使用数据列表生成一个栈数据段，该数据段连接到指定的Shellcoder对象
        /// </summary>
        /// <param name="shellcoder">要绑定的Shellcoder对象</param>
        /// <param name="data">要保存到栈上的数据</param>
        public StackDataA(params object[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                if (!dict.ContainsKey(data[i]))
                {
                    data_size += SizeOf(data[i]);
                    dict.Add(data[i], data_size);
                    index.Add(data[i]);
                }
            }
        }
        /// <summary>
        /// 在目标Shellcoder中生成压入数据栈的代码
        /// </summary>
        public void Push(ShellcoderA sc)
        {
            sc.Assemble("push ebp;mov ebp,esp");
            for (int i = 0; i < dict.Count; i++)
            {
                object p = dict[index[i]];
                if (p is byte b)
                    sc.Assemble($"push 0x{b:x2}");
                else if (p is short s)
                    sc.Assemble($"push 0x{s:x4}");
                else if (p is ushort us)
                    sc.Assemble($"push 0x{us:x4}");
                else if (p is int x)
                    sc.Assemble($"push 0x{x:x4}");
                else if (p is uint ux)
                    sc.Assemble($"push 0x{ux:x4}");
                else if (p is string str)
                {
                    sc.Add(ShellCode.Stack.PushStringA(str));
                }
                else throw new Exception("遇到不支持的数据类型");
            }
        }
        /// <summary>
        /// 释放整个StackData数据段
        /// </summary>
        /// <param name="sc">目标Shellcoder</param>
        public void Free(ShellcoderA sc)
        {
            sc.Assemble("mov esp,ebp;");
            sc.Assemble("pop ebp");
        }
        /// <summary>
        /// 返回该对象在汇编中的_ebp_a_x8_表示（若存在）
        /// </summary>
        /// <param name="obj">目标对象</param>
        /// <returns></returns>
        public string this[object obj]
        {
            get
            {
                if (!dict.ContainsKey(obj)) throw new Exception("该对象不在数据段中");
                return $"[ebp+0x{data_size:x2}]"; 
            }
        }
    }
    /// <summary>
    /// 负责栈数据的索引及管理，字符串编码方式为Unicode
    /// </summary>
    public class StackDataW
    {
        private Dictionary<object, int> dict = new Dictionary<object, int>();
        private List<object> index = new List<object>();
        private int offset = 0;
        private int SizeOf(object data)
        {
            if (data is byte) return 1;
            else if (data is short || data is ushort) return 2;
            else if (data is int || data is uint || data is Ptr) return 4;
            else if (data is Guid || data is Win32_GUID) return 16;
            else if (data is string str) return (str.Length / 2 + 1) * 4;
            else throw new Exception("遇到不支持的数据类型");
        }

        /// <summary>
        /// 使用数据列表生成一个栈数据段，该数据段连接到指定的Shellcoder对象
        /// </summary>
        /// <param name="data">要保存到栈上的数据</param>
        public StackDataW(params object[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                if (!dict.ContainsKey(data[i]))
                {
                    offset -= Convert.ToInt16(SizeOf(data[i]));
                    dict.Add(data[i], offset);
                    index.Add(data[i]);
                }
            }
        }
        /// <summary>
        /// 在目标Shellcoder中生成压入数据栈的代码
        /// </summary>
        public void Push(ShellcoderW sc)
        {
            sc.Assemble("push ebp;mov ebp,esp");
            for (int i = 0; i < dict.Count; i++)
            {
                object p = index[i];
                if (p is byte b)
                    sc.Assemble($"push 0x{b:x2}");
                else if (p is short s)
                    sc.Assemble($"push 0x{s:x4}");
                else if (p is ushort us)
                    sc.Assemble($"push 0x{us:x4}");
                else if (p is int x)
                    sc.Assemble($"push 0x{x:x8}");
                else if (p is uint ux)
                    sc.Assemble($"push 0x{ux:x8}");
                else if (p is Ptr ptr)
                    sc.Assemble($"push 0x{ptr.Value:x8}");
                else if (p is string str)
                    sc.Add(ShellCode.Stack.PushStringW(str));
                else if (p is Guid guid)
                {
                    var bin = guid.ToByteArray();
                    for (int n = 3; n >= 0; n--)
                    {
                        int x32 = BitConverter.ToInt32(bin, n * 4);
                        sc.Assemble($"push 0x{x32:x8}");
                    }
                }
                else if (p is Win32_GUID g)
                {
                    var bin = g.ToByteArray();
                    for (int n = 3; n >= 0; n--)
                    {
                        int x32 = BitConverter.ToInt32(bin, n * 4);
                        sc.Assemble($"push 0x{x32:x8}");
                    }
                }
                else if (p is IntPtr || p is UIntPtr) throw new Exception("IntPtr是值类型。如果要在栈上保存可修改的指针，请使用Win32.Ptr类");
                else throw new Exception("遇到不支持的数据类型");
            }
        }
        /// <summary>
        /// 重置整个数据段
        /// </summary>
        /// <param name="sc"></param>
        public void Free(ShellcoderW sc)
        {
            sc.Assemble("mov esp,ebp");
            sc.Assemble("pop ebp");
        }
        /// <summary>
        /// 返回该对象在汇编中的表示（若存在）
        /// </summary>
        /// <param name="obj">目标对象</param>
        /// <returns></returns>
        public string this[object obj]
        {
            get
            {
                int offset = GetOffset(obj);
                if (sbyte.MinValue < offset) return $"[ebp+0x{(sbyte)offset:x2}]";
                else return $"[ebp+0x{(offset):x8}]";
            } 
        }
        /// <summary>
        /// 获取该数据相对ebp的偏移量
        /// </summary>
        /// <param name="obj">要查询的对象（的引用）</param>
        /// <returns>该数据相对ebp的偏移量</returns>
        public int GetOffset(object obj)
        {
            if (!dict.ContainsKey(obj)) throw new Exception("该对象不在数据段中");
            return dict[obj];
        }
    }

    /// <summary>
    /// 按照一定架构生成shellcode
    /// </summary>
    public class ShellcoderA
    {
        private List<byte> s = new List<byte>();
        /// <summary>
        /// 栈数据段管理器
        /// </summary>
        public StackDataA Data { get; private set; }
        /// <summary>
        /// 实例化Shellcoder对象
        /// </summary>
        public ShellcoderA()
        {
            
        }
        /// <summary>
        /// 在当前栈顶新建栈数据段。此操作将重置ebp到栈顶
        /// </summary>
        public void ImportStackData(params object[] stack_data)
        {
            Data = new StackDataA(stack_data);
            Data.Push(this);
        }
        /// <summary>
        /// 获取与该对象相应的shellcode字节数组
        /// </summary>
        public byte[] GetBytes() => s.ToArray();
        /// <summary>
        /// 向对象中添加汇编
        /// </summary>
        /// <param name="asm">汇编文本，多行指令可用";"分隔</param>
        public void Assemble(string asm) => s.AddRange(Asm.AssembleAllLines(asm.Split(';')));
        /// <summary>
        /// 追加shellcode到此对象末尾
        /// </summary>
        /// <param name="shellcode">要追加的shellcode</param>
        public void Add(byte[] shellcode) => s.AddRange(shellcode);
        /// <summary>
        /// 执行基本初始化
        /// </summary>
        public void BasicInit() => s.AddRange(ShellCode.Init.BasicInit);
        /// <summary>
        /// 加载任意DLL
        /// </summary>
        /// <param name="dll_name">DLL名称</param>
        public void LoadLibrary(string dll_name) => s.AddRange(ShellCode.Function.LoadLibraryA(dll_name));
        /// <summary>
        /// 调用任意函数
        /// </summary>
        /// <param name="dll_name">DLL文件名</param>
        /// <param name="fn_name">函数名</param>
        /// <param name="parameters">参数</param>
        public void CallFunction(string dll_name, string fn_name, params object[] parameters)
            => s.AddRange(ShellCode.Function.CallFunctionA_Sharp(dll_name,fn_name,parameters));
    }
    /// <summary>
    /// 按照一定架构生成shellcode
    /// </summary>
    public class ShellcoderW
    {
        private List<byte> s = new List<byte>();
        /// <summary>
        /// 栈数据段管理器
        /// </summary>
        public StackDataW Data { get; private set; }
        /// <summary>
        /// 在当前栈顶新建栈数据段。此操作将重置ebp到栈顶
        /// </summary>
        public void ImportStackData(params object[] stack_data)
        {
            Data = new StackDataW(stack_data);
            Data.Push(this);
        }
        /// <summary>
        /// 释放整个栈数据段
        /// </summary>
        public void ReleaseStackData()
        {
            Data.Free(this);
        }
        /// <summary>
        /// 获取与该对象相应的shellcode字节数组
        /// </summary>
        public byte[] GetBytes() => s.ToArray();
        /// <summary>
        /// 向对象中添加汇编
        /// </summary>
        /// <param name="asm">汇编文本，多行指令可用";"分隔</param>
        public void Assemble(string asm) => s.AddRange(Asm.AssembleAllLines(asm.Split(';')));
        /// <summary>
        /// 追加shellcode到此对象末尾
        /// </summary>
        /// <param name="shellcode">要追加的shellcode</param>
        public void Add(byte[] shellcode) => s.AddRange(shellcode);
        /// <summary>
        /// 执行基本初始化
        /// </summary>
        public void BasicInit() => s.AddRange(ShellCode.Init.BasicInit);
        /// <summary>
        /// 加载任意DLL
        /// </summary>
        /// <param name="dll_name">DLL名称</param>
        public void LoadLibraryA(string dll_name) => s.AddRange(ShellCode.Function.LoadLibraryA(dll_name));
        /// <summary>
        /// 加载任意DLL并将地址保存在ebx。这个版本用C#直接获得相关地址，以最小化指令数
        /// </summary>
        /// <param name="dll_name">DLL名称</param>
        public void LoadLibraryA_Lite(string dll_name)
        {
            IntPtr hKernel32 = Kernel32.LoadLibraryA("kernel32.dll");
            IntPtr lpLoadLibraryA = Kernel32.GetProcAddress(hKernel32, "LoadLibraryA");
            IntPtr hModule = Kernel32.LoadLibraryA(dll_name);
            if (hModule == IntPtr.Zero) throw new Exception($"获取库地址失败：{new Win32Exception().Message}");
            Add(ShellCode.Stack.PushStringA(dll_name));
            Assemble($"push esp;mov eax,{lpLoadLibraryA.ToHexString32()};call eax");
            Assemble($"mov ebx,eax");
            Add(ShellCode.Stack.ClearStringA(dll_name));
        }
        /// <summary>
        /// 加载DLL并将并将地址保存在ebx，函数地址放在eax寄存器中。这个版本用C#直接获得相关地址，以最小化指令数
        /// </summary>
        /// <param name="dll_name">DLL名称</param>
        /// <param name="fn_name">函数名</param>
        public void LoadLibraryA_GetFunction_Lite(string dll_name, string fn_name)
        {
            LoadLibraryA_Lite(dll_name);
            IntPtr hModule = Kernel32.LoadLibraryA(dll_name);
            if (hModule == IntPtr.Zero) throw new Exception($"获取库地址失败：{new Win32Exception().Message}");
            IntPtr lpFn = Kernel32.GetProcAddress(hModule, fn_name);
            if (lpFn == IntPtr.Zero) throw new Exception($"获取函数地址失败：{new Win32Exception().Message}");
            Assemble($"mov eax,{lpFn.ToHexString32()}");
        }
        /// <summary>
        /// 调用任意函数
        /// </summary>
        /// <param name="dll_name">DLL文件名</param>
        /// <param name="fn_name">函数名</param>
        /// <param name="parameters">参数</param>
        public void CallFunction_Lite(string dll_name, string fn_name, params object[] parameters)
            => s.AddRange(ShellCode.Function.CallFunctionW_Sharp(dll_name, fn_name, parameters));
        /// <summary>
        /// 调用任意函数
        /// </summary>
        /// <param name="dll_name">DLL文件名</param>
        /// <param name="fn_name">函数名</param>
        /// <param name="parameters">参数</param>
        public void CallFunction(string dll_name, string fn_name, params object[] parameters)
            => s.AddRange(ShellCode.Function.CallFunctionW_Sharp(dll_name, fn_name, parameters));
        /// <summary>
        /// 加载CLR。执行后ebx = pMetaHost, ecx = pRuntimeInfo, edx = pRuntimeHost
        /// </summary>
        public void LoadAndRunCLR()
        {
            LoadLibraryA_GetFunction_Lite("mscoree.dll", "CLRCreateInstance");
            Assemble("mov ebx,eax");
            //ebx=CLRCreateInstance
            string version = "v4.0.30319";
            Ptr pMetaHost = new Ptr(0);
            Ptr pRuntimeInfo = new Ptr(0);
            Ptr pRuntimeHost = new Ptr(0);
            ImportStackData(
                version,
                pMetaHost,
                pRuntimeInfo,
                pRuntimeHost,
                Win32Const.IID_ICLRMetaHost,
                Win32Const.CLSID_CLRMetaHost,
                Win32Const.IID_ICLRRuntimeInfo,
                Win32Const.IID_ICLRRuntimeHost,
                Win32Const.CLSID_CLRRuntimeHost
                );
            //eax = CLRCreateInstance
            Assemble($"lea eax,{Data[pMetaHost]};push eax");
            Assemble($"lea eax,{Data[Win32Const.IID_ICLRMetaHost]};push eax");
            Assemble($"lea eax,{Data[Win32Const.CLSID_CLRMetaHost]};push eax");
            Assemble($"call ebx");
            //pMetaHost=pGetRuntime
            Assemble($"lea eax,{Data[pRuntimeInfo]};push eax");
            Assemble($"lea eax,{Data[Win32Const.IID_ICLRRuntimeInfo]};push eax");
            Assemble($"lea eax,{Data[version]};push eax");
            Assemble($"push {Data[pMetaHost]}");    //隐藏的第一个参数，MetaHost*
            Assemble($"mov eax,{Data[pMetaHost]}"); //eax = pMetaHost
            Assemble($"mov eax,[eax]");             //eax = *vtable
            Assemble($"mov eax,[eax+0x0c]");        //eax = vtable[0x0c] = pGetRuntime
            Assemble($"call eax");
            //pRuntimeInfo + 0x18 = pGetInterface
            Assemble($"lea eax,{Data[pRuntimeHost]};push eax");
            Assemble($"lea eax,{Data[Win32Const.IID_ICLRRuntimeHost]};push eax");
            Assemble($"lea eax,{Data[Win32Const.CLSID_CLRRuntimeHost]};push eax");
            Assemble($"push {Data[pRuntimeInfo]}");    //隐藏的第一个参数，RuntimeInfo*
            Assemble($"mov eax,{Data[pRuntimeInfo]}"); //eax = pRuntimeInfo
            Assemble($"mov eax,[eax]");             //eax = *vtable
            Assemble($"mov eax,[eax+0x24]");        //eax = vtable[0x0c+0x18] = pGetRuntime
            Assemble($"call eax");
            //pStart = pRuntimeHost  
            //pStop = pRuntimeHost + 0x04
            //pExecuteApplication = pRuntimeHost + 0x1c
            Assemble($"mov eax,{Data[pRuntimeHost]}");  //eax = ppRuntimeHost
            Assemble($"mov ecx,[eax]");                 //eax = RuntimeHost[0] = pStart
            Assemble($"mov edx,{Data[pRuntimeHost]}");
            Assemble($"push edx");                      //隐藏的第一个参数，RuntimeHost*
            Assemble($"mov eax,[ecx+0x0c]");
            Assemble($"call eax");

            //保存
            Assemble($"mov ebx,{Data[pMetaHost]}");
            Assemble($"mov ecx,{Data[pRuntimeInfo]}");
            Assemble($"mov edx,{Data[pRuntimeHost]}");
            ReleaseStackData();
        }
        /// <summary>
        /// 执行CLR方法
        /// </summary>
        /// <param name="assembly_name">CLR可执行文件名</param>
        /// <param name="namespace_name">命名空间名</param>
        /// <param name="class_name">类名</param>
        /// <param name="method_name">方法名</param>
        /// <param name="parameter">参数，必须为字符串</param>
        public void RunMethodInDefaultAppDomain_Full(string assembly_name, string namespace_name, string class_name, string method_name, string parameter)
        {
            LoadAndRunCLR();
            RunMethodInDefaultAppDomain(assembly_name, namespace_name, class_name, method_name, parameter);
        }
        /// <summary>
        /// 执行CLR方法。先决条件：已经加载并运行CLR
        /// </summary>
        /// <param name="assembly_name">CLR可执行文件名</param>
        /// <param name="namespace_name">命名空间名</param>
        /// <param name="class_name">类名</param>
        /// <param name="method_name">方法名</param>
        /// <param name="parameter">参数，必须为字符串</param>
        public void RunMethodInDefaultAppDomain(string assembly_name, string namespace_name, string class_name, string method_name, string parameter)
        {
            //ebx = pMetaHost, ecx = pRuntimeInfo, edx = pRuntimeHost
            //RunMethodInDefaultAppDomain函数签名：可执行文件名字、命名空间.类、方法名、字符串参数、返回值指针
            Ptr dwRet = new Ptr(0);
            string asm_str = assembly_name;
            Debug.Print($"asm_str: {asm_str}");
            string class_str = $"{namespace_name}.{class_name}";
            Debug.Print($"class_str: {class_str}");
            string method_str = method_name;
            Debug.Print($"class_str: {method_str}");
            string param_str = parameter;
            ImportStackData(
                asm_str,
                class_str,
                method_str,
                param_str,
                dwRet
                );
            //pExecuteInDefaultAppDomain = pRuntimeHost + 0x20
            Assemble($"lea eax,{Data[dwRet]};push eax");
            Assemble($"lea eax,{Data[param_str]};push eax");
            Assemble($"lea eax,{Data[method_str]};push eax");
            Assemble($"lea eax,{Data[class_str]};push eax");
            Assemble($"lea eax,{Data[asm_str]};push eax");
            Assemble($"mov ecx,edx");       //ecx=pRuntimeHost
            Assemble($"mov edx,[ecx]");     //edx=RuntimeHost
            Assemble($"mov eax,edx");       //eax=RuntimeHost
            Assemble($"push edx");          
            Assemble($"mov ecx,[edx+0x2c]");
            Assemble($"call ecx");
            ReleaseStackData();
        }
    }
}
