using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Gleee.Assembly
{
    /// <summary>
    /// 汇编类
    /// </summary>
    public static class Asm
    {
        /// <summary>
        /// 返回字节数组的十六进制字符串
        /// </summary>
        /// <param name="bin"></param>
        /// <returns></returns>
        public static string ToHexString(this byte[] bin)
        {
            string re = "";
            for (int i = 0; i < bin.Length; i++)
            {
                re += $"{bin[i]:X2} ";
            }
            return re;
        }
        /// <summary>
        /// 返回字节数组的C/C++定义字符串
        /// </summary>
        /// <param name="shell_code">待转换的字节数组</param>
        /// <returns></returns>
        public static string ToCppCharArrayString(this byte[] shell_code)
        {
            string re = "\"";
            for (int i = 0; i < shell_code.Length; i++)
            {
                re += $"\\x{shell_code[i]:x2}";
            }
            re += "\"";
            return re;
        }
        /// <summary>
        /// 返回字节数组的C#定义字符串
        /// </summary>
        /// <param name="shell_code">待转换的字节数组</param>
        /// <returns></returns>
        public static string ToCSharpByteArrayString(this byte[] shell_code)
        {
            string re = "{\n\t";
            for (int i = 0; i < shell_code.Length; i++)
            {
                re += $"0x{shell_code[i]:x2},";
                if (i % 11 == 10) re += "\n\t";
            }
            return re + "\n};";
        }
        private static readonly Regex x8_pat = new Regex(@"\b(0x[a-fA-F0-9]{2})\b");
        private static readonly Regex x16_pat = new Regex(@"\b(0x[a-fA-F0-9]{4})\b");
        private static readonly Regex x32_pat = new Regex(@"\b(0x[a-fA-F0-9]{8})\b");
        private static readonly Regex labelC_pat = new Regex(@"^([a-zA-Z0-9_]+)\:$");
        private static readonly Regex label_pat = new Regex(@"^([a-zA-Z0-9_]+)$");

        /// <summary>
        /// 将单行文本汇编为机器码
        /// </summary>
        /// <param name="line">单行文本</param>
        /// <returns></returns>
        public static byte[] Assemble(string line)
        {
            //Console.Write($"汇编‘{line}’...");
            Instruction ins = ParseLine(line);
            if (ins.Operator == Operator.jne || ins.Operator == Operator.jnz) throw new Exception("对单行代码汇编时无法处理跳转指令");
            byte[] code = AssembleInstruction(ins);
            //Console.WriteLine(code.ToHexString());
            if (code.ToList<byte>().Contains(0x00)) Console.WriteLine($"汇编'{line}'时遇到0x00：{code.ToHexString()}");
            return code;
        }
        /// <summary>
        /// 将文本汇编为机器码
        /// </summary>
        /// <param name="asm">待汇编文本</param>
        /// <returns>机器码序列</returns>
        public static byte[] AssembleAllText(string asm)
        {
            string[] lines = asm.Split('\n');
            return AssembleAllLines(lines);
        }
        /// <summary>
        /// 将多行文本汇编为机器码
        /// </summary>
        /// <param name="asm">待汇编文本</param>
        /// <returns>机器码序列</returns>
        public static byte[] AssembleAllLines(string[] asm)
        {
            Dictionary<string, int> labels = new Dictionary<string, int>();
            Dictionary<int, string> jnzs = new Dictionary<int, string>();
            List<byte> bin = new List<byte>();
            for (int ip = 0; ip < asm.Length; ip++)
            {
                string line = asm[ip].Trim();
                if (line.StartsWith(";") || line.Length == 0) continue;
                Instruction ins = ParseLine(line);
                if (ins.Operator == Operator.label)
                {
                    labels.Add((string)ins.ValueA, bin.Count);
                }
                else
                {
                    if (ins.Operator == Operator.jne || ins.Operator == Operator.jnz)
                    {
                        //跳转命令保存到跳转表里
                        jnzs.Add(bin.Count, (string)ins.ValueA);
                    }
                    bin.AddRange(AssembleInstruction(ins));
                } 
            }
            foreach (var kvp in jnzs)
            {
                int jnz_addr = kvp.Key;    //跳转命令的地址
                //Debug.Print($"跳转命令的地址为0x{jnz_addr:x8}，标签为{kvp.Value}");
                string label = kvp.Value;   //目标标签
                //Debug.Print($"标签的地址为0x{jnz_addr:x8}");
                if (!labels.ContainsKey(label)) throw new Exception($"找不到标签{label}");
                int lbl_addr = labels[label];   //标签的地址
                byte offset = (byte)(lbl_addr - jnz_addr - 0x2);
                //Debug.Print($"计算出的跳转值为0x{offset:x2}");
                bin[kvp.Key + 1] = offset;
            }
            return bin.ToArray();
        }
        /// <summary>
        /// 汇编代码并生成C# shellcode byte[]文本
        /// </summary>
        /// <param name="asm">待汇编文本</param>
        /// <returns>C# shellcode byte[]文本</returns>
        public static string GetShellCodeByteArrayString(string asm)
        {
            byte[] shell_code = AssembleAllText(asm);
            string re = "{\n\t";
            for (int i = 0; i < shell_code.Length; i++)
            {
                re += $"0x{shell_code[i]:x2},";
                if (i % 11 == 10) re += "\n\t";
            }
            return re + "\n};";
        }
        /// <summary>
        /// 将ASCII字符串转换为倒序字节序列
        /// </summary>
        /// <param name="str">待转换的ASCII字符串</param>
        /// <returns>相应的字节序列</returns>
        public static byte[] StrToAscii(string str)
        {
            return Encoding.ASCII.GetBytes(str).Reverse().ToArray();
        }
        /// <summary>
        /// 将Unicode字符串转换为倒序字节序列
        /// </summary>
        /// <param name="str">待转换的Unicode字符串</param>
        /// <returns>相应的字节序列</returns>
        public static byte[] StrToUnicode(string str)
        {
            return Encoding.Unicode.GetBytes(str).Reverse().ToArray();
        }
        /// <summary>
        /// 将双字节数组转换为16位立即数
        /// </summary>
        /// <param name="bin">待转换数组</param>
        /// <returns></returns>
        public static string BytesToX16(byte[] bin)
        {
            if (bin.Length != 2) throw new Exception("字节数组的长度错误，必须为2");
            return $"0x{string.Concat(bin.Select(x => x.ToString("x2")).ToArray())}";
        }
        /// <summary>
        /// 将四字节数组转换为32位立即数字符串(0x1a2b3c4d形式)
        /// </summary>
        /// <param name="bin">待转换数组</param>
        /// <returns></returns>
        public static string BytesToX32(byte[] bin)
        {
            if (bin.Length != 4) throw new Exception("字节数组的长度错误，必须为4");
            return $"0x{string.Concat(bin.Select(x=>x.ToString("x2")).ToArray())}";
        }
        /// <summary>
        /// 长度为4的ASCII字符串转换为32位立即数字符串(0x1a2b3c4d形式)
        /// </summary>
        /// <param name="str">待转换的ASCII字符串</param>
        /// <returns></returns>
        public static string StrToX32A(string str)
        {
            if (str.Length != 4) throw new Exception("字符串的长度错误，必须为4");
            return BytesToX32(StrToAscii(str));
        }
        /// <summary>
        /// 长度为2的Unicode字符串转换为32位立即数
        /// </summary>
        /// <param name="str">待转换的Unicode字符串</param>
        /// <returns></returns>
        public static string StrToX32W(string str)
        {
            if (str.Length != 2) throw new Exception("字符串的长度错误，必须为2");
            return BytesToX32(StrToUnicode(str));
        }
        /// <summary>
        /// 长度为4的ASCII字符串转换为32位整数
        /// </summary>
        /// <param name="str">待转换的Unicode字符串</param>
        /// <returns></returns>
        public static int StrToIntA(string str)
        {
            if (str.Length != 4) throw new Exception("字符串的长度错误，必须为4");
            return BitConverter.ToInt32(Encoding.ASCII.GetBytes(str).Reverse().ToArray(), 0);
        }
        /// <summary>
        /// 长度为2的Unicode字符串转换为32位整数
        /// </summary>
        /// <param name="str">待转换的Unicode字符串</param>
        /// <returns>对应的32位整数</returns>
        public static int StrToIntW(string str)
        {
            if (str.Length != 2) throw new Exception("字符串的长度错误，必须为2");
            return BitConverter.ToInt32(Encoding.Unicode.GetBytes(str).ToArray(), 0);
        }

        //解析单行文本生成Instruction对象
        private static Instruction ParseLine(string line)
        {
            string[] strs = line.Split(' ');
            string opt_str = strs[0];
            Operator opt;
            Operand opra = Operand.none;
            object valuea = 0, valueb = 0;
            Operand oprb = Operand.none;
            if (labelC_pat.IsMatch(opt_str))
            {
                string label = labelC_pat.Match(opt_str).Groups[1].Value;
                Instruction lbl = new Instruction
                {
                    Operator = Operator.label,
                    OperandA = opra,
                    OperandB = oprb,
                    ValueA = label,
                    ValueB = valueb
                };
                return lbl;
            }
            if (!Enum.TryParse(opt_str, out opt)) throw new Exception($"未知指令：{opt_str}");

            //解析第一个操作数（若存在）
            else if (strs.Length > 1 && !strs[1].StartsWith(";") && strs[1].Trim().Length != 0)
            {
                string[] oprs = strs[1].Split(',');
                string opra_str = oprs[0].Split(';')[0];
                string opra_pre_str = OperandSignature(opra_str);
                //获取操作数类型
                if (!Enum.TryParse(opra_pre_str, out opra))
                {
                    if (label_pat.IsMatch(opra_str)) opra = Operand.label;          //跟其它关键字对不上，又符合标签规则，一律当标签
                    else throw new Exception($"无法识别的操作数模式：{opra_str}");     //连标签都不配当，说明写错了
                } 
                //获取与操作数A相关的值
                valuea = ExtractValue(opra_str);

                //解析第二个操作数（若存在）
                if (oprs.Length == 2 && !oprs[1].StartsWith(";") && oprs[1].Trim().Length!=0)   //存在，非注释，非空白字符串
                {
                    string oprb_str = oprs[1].Split(';')[0];
                    string oprb_pre_str = OperandSignature(oprb_str);
                    //解析操作数类型
                    if (!Enum.TryParse(oprb_pre_str, out oprb))
                    {
                        if (label_pat.IsMatch(oprb_str)) oprb = Operand.label;          //跟其它关键字对不上，又符合标签规则，一律当标签
                        else throw new Exception($"无法识别的操作数模式：{oprb_str}");     //连标签都不配当，说明写错了
                    }
                    //获取与操作数B相关的值
                    valueb = ExtractValue(oprb_str);
                }
            }
            Instruction ins = new Instruction
            {
                Operator = opt,
                OperandA = opra,
                OperandB = oprb,
                ValueA = valuea,
                ValueB = valueb,
            };
            return ins;
        }
        //将Instruction对象转换为机器码
        private static byte[] AssembleInstruction(Instruction ins)
        {
            switch (ins.Operator)
            {
                case Operator.xor:
                    return Xor(ins);
                case Operator.inc:
                    return Inc(ins);
                case Operator.dec:
                    return Dec(ins);
                case Operator.lodsd:
                    return Lodsd(ins);
                case Operator.ret:
                    return Ret(ins);
                case Operator.push:
                    return Push(ins);
                case Operator.pop:
                    return Pop(ins);
                case Operator.add:
                    return Add(ins);
                case Operator.sub:
                    return Sub(ins);
                case Operator.cmp:
                    return Cmp(ins);
                case Operator.jmp:
                    return Jmp(ins);
                case Operator.jne:
                    return Jne(ins);
                case Operator.jnz:
                    return Jne(ins);
                case Operator.mov:
                    return Mov(ins);
                case Operator.call:
                    return Call(ins);
                case Operator.xchg:
                    return Xchg(ins);
                case Operator.lea:
                    return Lea(ins);
            }
            throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA} {ins.OperandB}");
        }
        //生成操作数模式签名
        private static string OperandSignature(string raw)
        {
            raw = ReplaceX(raw, x8_pat, "x8");
            raw = ReplaceX(raw, x16_pat, "x16");
            raw = ReplaceX(raw, x32_pat, "x32");
            raw = raw.Replace("[", "_").Replace("]", "_");
            raw = raw.Replace("+", "_a_").Replace("-", "_s_").Replace("*", "_m_").Replace("/", "_d_");
            raw = raw.Replace(":", "_c_");
            return raw;
        }
        //取出相关值
        private static object ExtractValue(string raw)
        {
            if (x8_pat.IsMatch(raw))
            {
                var matches = x8_pat.Matches(raw);
                if (matches.Count != 1) throw new Exception("操作数中匹配到多个8位立即数");
                return Convert.ToByte(matches[0].Groups[1].Value, 16);
            }
            else if (x16_pat.IsMatch(raw))
            {
                var matches = x16_pat.Matches(raw);
                if (matches.Count != 1) throw new Exception("操作数中匹配到多个16位立即数");
                return Convert.ToInt16(matches[0].Groups[1].Value, 16);
            }
            else if (x32_pat.IsMatch(raw))
            {
                var matches = x32_pat.Matches(raw);
                if (matches.Count != 1) throw new Exception("操作数中匹配到多个32位立即数");
                return Convert.ToInt32(matches[0].Groups[1].Value, 16);
            }
            else if (label_pat.IsMatch(raw))
            {
                var matches = label_pat.Matches(raw);
                if (matches[0].Groups[1].Value != raw) throw new Exception("非法标签");
                return raw;
            }
            else return 0;
        }
        //把立即数替换成x8 x16 x32之类的
        private static string ReplaceX(string raw, Regex regex, string new_name)
        {
            var matches = regex.Matches(raw);
            foreach (Match match in matches)
            {
                string str = match.Groups[1].Value;
                int index = match.Groups[1].Index;
                raw = raw.Substring(0, index) + new_name + raw.Substring(index + str.Length, raw.Length - index - str.Length);
            }
            return raw;
        }
        //汇编函数
        private static byte[] Add(Instruction ins)
        {
            if (ins.IsBinary)
            {
                switch (ins.OperandA)
                {
                    case Operand.eax:
                        switch (ins.OperandB)
                        {
                            case Operand.ebx:
                                return new byte[] { 0x03, 0xc3 };
                            case Operand.x8:
                                byte bx8 = (byte)ins.ValueB;
                                return new byte[] { 0x83, 0xc0, bx8 };
                            case Operand.x32:
                                byte[] bx32 = BitConverter.GetBytes((int)ins.ValueB);
                                return new byte[] { 0x05, bx32[0], bx32[1], bx32[2], bx32[3] };
                        }
                        break;
                    case Operand.ebx:
                        switch (ins.OperandB)
                        {
                            case Operand.ebx:
                                throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
                            case Operand.x8:
                                byte x8 = (byte)ins.ValueB;
                                return new byte[] { 0x83, 0xc4, x8 };
                            case Operand.x32:
                                throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
                        }
                        break;
                    case Operand.ecx:
                        throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
                    case Operand.edx:
                        switch (ins.OperandB)
                        {
                            case Operand.ebx:
                                return new byte[] { 0x03, 0xd3 };
                        }
                        break;
                    case Operand.esi:
                        switch (ins.OperandB)
                        {
                            case Operand.ebx:
                                return new byte[] { 0x03, 0xf3 };
                        }
                        break;
                    case Operand.esp:
                        switch (ins.OperandB)
                        {
                            case Operand.x8:
                                byte x8 = (byte)ins.ValueB;
                                return new byte[] { 0x83, 0xc4, x8 };
                        }
                        break;
                    case Operand._esp_:
                        switch (ins.OperandB)
                        {
                            case Operand.x8:
                                byte x8 = (byte)ins.ValueB;
                                return new byte[] { 0x80, 0x04, 0x24, x8 };
                        }
                        break;
                    case Operand._esp_a_x8_:
                        switch (ins.OperandB)
                        {
                            case Operand.x8:
                                byte ax8 = (byte)ins.ValueA;
                                byte bx8 = (byte)ins.ValueB;
                                return new byte[] { 0x80, 0x44,0x24, ax8, bx8 };
                            case Operand.x32:
                                ax8 = (byte)ins.ValueA;
                                byte[] bx32 = BitConverter.GetBytes((int)ins.ValueB);
                                return new byte[] { 0x80, 0x44, 0x24, ax8, bx32[0], bx32[1], bx32[2], bx32[3] };
                        }
                        break;
                    case Operand.x32:
                        throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
                }
            }
            throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
        }
        private static byte[] Call(Instruction ins)
        {
            if (ins.IsUnary)
            {
                if (ins.OperandA.IsSimpleReg())
                {
                    int regA_n = (int)ins.OperandA - 4;  //寄存器编号
                    byte @base = 0xd0;
                    byte src = Convert.ToByte(@base + regA_n);
                    return new byte[] { 0xff, src };
                }
                else
                {
                    switch (ins.OperandA)
                    {
                        case Operand._ebp_a_x8_:
                            byte bx8 = (byte)ins.ValueA;
                            return new byte[] { 0xff, 0x55, bx8 };
                        case Operand.x32:
                            throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
                    }
                }
            }
            throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
        }
        private static byte[] Cmp(Instruction ins)
        {
            if (ins.IsBinary)
            {
                switch (ins.OperandA)
                {
                    case Operand.eax:
                        throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
                    case Operand.ebx:
                        throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
                    case Operand.ecx:
                        throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
                    case Operand.edx:
                        throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
                    case Operand.esi:
                        throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
                    case Operand.esp:
                        throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
                    case Operand.x32:
                        throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
                    case Operand._eax_:
                        switch (ins.OperandB)
                        {
                            case Operand.x32:
                                byte[] bx32 = BitConverter.GetBytes((int)ins.ValueB);
                                return new byte[] { 0x81, 0x38, bx32[0], bx32[1], bx32[2], bx32[3] };
                        }
                        break;
                    case Operand._eax_a_x8_:
                        switch (ins.OperandB)
                        {
                            case Operand.x32:
                                byte ax8 = (byte)ins.ValueA;
                                byte[] bx32 = BitConverter.GetBytes((int)ins.ValueB);
                                return new byte[] { 0x81, 0x78, ax8, bx32[0], bx32[1], bx32[2], bx32[3] };
                        }
                        break;
                }
            }
            throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
        }
        private static byte[] Dec(Instruction ins)
        {
            if (ins.IsUnary)
            {
                switch (ins.OperandA)
                {
                    case Operand.ecx:
                        return new byte[] { 0x49 };
                }
            }
            throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA} {ins.OperandB}");
        }
        private static byte[] Inc(Instruction ins)
        {
            if (ins.IsUnary)
            {
                switch (ins.OperandA)
                {
                    case Operand.ecx:
                        return new byte[] { 0x41 };
                }
            }
            throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA} {ins.OperandB}");
        }
        private static byte[] Jmp(Instruction ins)
        {
            if (ins.IsUnary)
            {
                switch (ins.OperandA)
                {
                    case Operand.eax:
                        return new byte[] { 0xff, 0xe0 };
                    case Operand.ebx:
                        throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
                    case Operand.ecx:
                        throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
                    case Operand.edx:
                        throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
                    case Operand.esp:
                        throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
                    case Operand.label:
                        return new byte[] { 0x75, 0xff };   //0xff只是垃圾占位符
                }
            }
            throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
        }
        private static byte[] Jne(Instruction ins)
        {
            if (ins.IsUnary)
            {
                switch (ins.OperandA)
                {
                    case Operand.eax:
                        throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
                    case Operand.ebx:
                        throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
                    case Operand.ecx:
                        throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
                    case Operand.edx:
                        throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
                    case Operand.esp:
                        throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
                    case Operand.label:
                        return new byte[] { 0x75, 0xff };//0xff只是垃圾占位符
                }
            }
            throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
        }
        private static byte[] Lea(Instruction ins)
        {
            if (ins.IsBinary)
            {
                if (ins.OperandA.IsSimpleReg() && ins.OperandB.IsSimpleReg()) throw new NotImplementedException();
                else if (ins.OperandA.IsSimpleReg() && ins.OperandB == Operand._ebp_a_x8_)
                {
                    int regA_n = (int)ins.OperandA - 4;  //寄存器编号
                    byte @base = Convert.ToByte(0x45 + regA_n * 8);
                    return new byte[] { 0x8d, @base, (byte)ins.ValueB };
                }
                else if (ins.OperandA.IsSimpleReg() && ins.OperandB == Operand._ebp_a_x16_)
                {
                    int regA_n = (int)ins.OperandA - 4;  //寄存器编号
                    byte @base = Convert.ToByte(0x85 + regA_n * 8);
                    byte[] bx16 = BitConverter.GetBytes((short)ins.ValueB);
                    return new byte[] { 0x8d, @base, bx16[0], bx16[1], 0x00, 0x00 };
                }
                else if (ins.OperandA.IsSimpleReg() && ins.OperandB == Operand._ebp_a_x32_)
                {
                    int regA_n = (int)ins.OperandA - 4;  //寄存器编号
                    byte @base = Convert.ToByte(0x85 + regA_n * 8);
                    byte[] bx32 = BitConverter.GetBytes((int)ins.ValueB);
                    return new byte[] { 0x8d, @base, bx32[0], bx32[1], bx32[2], bx32[3] };
                }
                else
                {
                    switch (ins.OperandA)
                    {
                        case Operand.eax:
                            switch (ins.OperandB)
                            {
                                case Operand.esp:
                                    throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
                                case Operand._eax_:
                                    throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
                                case Operand._eax_a_x8_:
                                    byte bx8 = (byte)ins.ValueB;
                                    throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
                                case Operand._ebp_a_x8_:
                                    bx8 = (byte)ins.ValueB;
                                    return new byte[] { 0x8d, 0x45, bx8 };
                                case Operand.fs_c__ecx_a_x8_:
                                    bx8 = (byte)ins.ValueB;
                                    throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
                                case Operand.x32:
                                    byte[] bx32 = BitConverter.GetBytes((int)ins.ValueB);
                                    throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
                            }
                            break;
                        case Operand.ebx:
                            switch (ins.OperandB)
                            {
                                case Operand.esp:
                                    throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
                                case Operand._eax_:
                                    throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
                                case Operand._eax_a_x8_:
                                    byte bx8 = (byte)ins.ValueB;
                                    throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
                                case Operand.fs_c__ecx_a_x8_:
                                    throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
                            }
                            break;
                        case Operand.ecx:
                            switch (ins.OperandB)
                            {
                                case Operand.eax:
                                    throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
                                case Operand.x32:
                                    byte[] bx32 = BitConverter.GetBytes((int)ins.ValueB);
                                    throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
                            }
                            break;
                        case Operand.edx:
                            switch (ins.OperandB)
                            {
                                case Operand._edx_a_x8_:
                                    byte bx8 = (byte)ins.ValueB;
                                    throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
                                case Operand._ebx_a_x8_:
                                    bx8 = (byte)ins.ValueB;
                                    throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
                                case Operand._esp_a_x8_:
                                    bx8 = (byte)ins.ValueB;
                                    throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
                                case Operand._esi_a_ecx_m_4_:
                                    throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
                            }
                            break;
                        case Operand.esi:
                            switch (ins.OperandB)
                            {
                                case Operand._eax_a_x8_:
                                    byte bx8 = (byte)ins.ValueB;
                                    throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
                                case Operand._edx_a_x8_:
                                    bx8 = (byte)ins.ValueB;
                                    throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
                            }
                            break;
                        case Operand.esp:
                            throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
                        case Operand.cx:
                            switch (ins.OperandB)
                            {
                                case Operand._esi_a_ecx_m_2_:
                                    throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
                                case Operand.x16:
                                    byte[] bx16 = BitConverter.GetBytes((short)ins.ValueB);
                                    throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
                            }
                            break;
                        case Operand.x32:
                            throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
                    }
                }
            }
            throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
        }
        private static byte[] Lodsd(Instruction ins)
        {
            if (ins.IsNone) return new byte[] { 0xad };
            throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA} {ins.OperandB}");
        }
        private static byte[] Mov(Instruction ins)
        {
            if (ins.IsBinary)
            {
                if (ins.OperandA.IsSimpleReg() && ins.OperandB.IsSimpleReg())
                {
                    int regA_n = (int)ins.OperandA - 4;  //寄存器编号
                    int regB_n = (int)ins.OperandB - 4;
                    byte @base = Convert.ToByte(0xc0 + regA_n * 8);
                    byte src = Convert.ToByte(@base + regB_n);
                    return new byte[] { 0x8b, src };
                }
                else if (ins.OperandA.IsSimpleReg() && ins.OperandB.Is_Reg_())
                {
                    int regA_n = (int)ins.OperandA - 4;  //寄存器编号
                    int regB_n = (int)ins.OperandB - 12;
                    byte @base = Convert.ToByte(regA_n * 8);
                    byte src = Convert.ToByte(@base + regB_n);
                    return new byte[] { 0x8b, src };
                }
                else if (ins.OperandA.IsSimpleReg() && ins.OperandB == Operand._ebp_a_x8_)
                {
                    int regA_n = (int)ins.OperandA - 4;  //寄存器编号
                    byte @base = Convert.ToByte(0x45 + regA_n * 8);
                    return new byte[] { 0x8b, @base, (byte)ins.ValueB };
                }
                else if (ins.OperandA.IsSimpleReg() && ins.OperandB == Operand._ebp_a_x32_)
                {
                    int regA_n = (int)ins.OperandA - 4;  //寄存器编号
                    byte @base = Convert.ToByte(0x85 + regA_n * 8);
                    byte[] bx32 = BitConverter.GetBytes((int)ins.ValueB);
                    return new byte[] { 0x8b, @base, bx32[0], bx32[1], bx32[2], bx32[3] };
                }
                else if (ins.OperandA.IsSimpleReg() && ins.OperandB == Operand.x32)
                {
                    int regA_n = (int)ins.OperandA - 4;  //寄存器编号
                    byte[] bx32 = BitConverter.GetBytes((int)ins.ValueB);
                    byte @base = Convert.ToByte(0xb8 + regA_n);
                    return new byte[] { @base, bx32[0], bx32[1], bx32[2], bx32[3] };
                }
                else
                {
                    switch (ins.OperandA)
                    {
                        case Operand.eax:
                            switch (ins.OperandB)
                            {
                                case Operand._eax_:
                                    return new byte[] { 0x8b, 0x00 };
                                case Operand._eax_a_x8_:
                                    byte bx8 = (byte)ins.ValueB;
                                    return new byte[] { 0x8b, 0x40, bx8 };
                                case Operand._ecx_a_x8_:
                                    bx8 = (byte)ins.ValueB;
                                    return new byte[] { 0x8b, 0x41, bx8 };
                                case Operand._ebp_a_x8_:
                                    bx8 = (byte)ins.ValueB;
                                    return new byte[] { 0x8b, 0x45, bx8 };
                                case Operand.fs_c__ecx_a_x8_:
                                    bx8 = (byte)ins.ValueB;
                                    return new byte[] { 0x64, 0x8b, 0x41, bx8 };
                                case Operand.x32:
                                    byte[] bx32 = BitConverter.GetBytes((int)ins.ValueB);
                                    return new byte[] { 0xb8, bx32[0], bx32[1], bx32[2], bx32[3] };
                            }
                            break;
                        case Operand.ecx:
                            switch (ins.OperandB)
                            {
                                case Operand.eax:
                                    return new byte[] { 0x8b, 0xc8 };
                                case Operand._edx_a_x8_:
                                    byte bx8 = (byte)ins.ValueB;
                                    return new byte[] { 0x8b, 0x4a, bx8 };
                                case Operand.x32:
                                    byte[] bx32 = BitConverter.GetBytes((int)ins.ValueB);
                                    return new byte[] { 0xb9, bx32[0], bx32[1], bx32[2], bx32[3] };
                            }
                            break;
                        case Operand.ebx:
                            switch (ins.OperandB)
                            {
                                case Operand.esp:
                                    return new byte[] { 0x8b, 0xdc };
                                case Operand._eax_:
                                    throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
                                case Operand._eax_a_x8_:
                                    byte bx8 = (byte)ins.ValueB;
                                    return new byte[] { 0x8b, 0x58, bx8 };
                                case Operand.fs_c__ecx_a_x8_:
                                    throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
                            }
                            break;
                        case Operand.edx:
                            switch (ins.OperandB)
                            {
                                case Operand._edx_a_x8_:
                                    byte bx8 = (byte)ins.ValueB;
                                    return new byte[] { 0x8b, 0x52, bx8 };
                                case Operand._ebx_a_x8_:
                                    bx8 = (byte)ins.ValueB;
                                    return new byte[] { 0x8b, 0x53, bx8 };
                                case Operand._esp_a_x8_:
                                    bx8 = (byte)ins.ValueB;
                                    return new byte[] { 0x8b, 0x54, 0x24, bx8 };
                                case Operand._esi_a_ecx_m_4_:
                                    return new byte[] { 0x8b, 0x14, 0x8e };
                            }
                            break;
                        case Operand.esi:
                            switch (ins.OperandB)
                            {
                                case Operand._eax_a_x8_:
                                    byte bx8 = (byte)ins.ValueB;
                                    return new byte[] { 0x8b, 0x70, bx8 };
                                case Operand._edx_a_x8_:
                                    bx8 = (byte)ins.ValueB;
                                    return new byte[] { 0x8b, 0x72, bx8 };
                            }
                            break;
                        case Operand.esp:
                            throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
                        case Operand.ebp:
                            switch (ins.OperandB)
                            {
                                case Operand.esp:
                                    byte bx8 = (byte)ins.ValueB;
                                    return new byte[] { 0x8b, 0xec, bx8 };
                            }
                            break;
                        case Operand.cx:
                            switch (ins.OperandB)
                            {
                                case Operand._esi_a_ecx_m_2_:
                                    return new byte[] { 0x66, 0x8b, 0x0c, 0x4e };
                                case Operand.x16:
                                    byte[] bx16 = BitConverter.GetBytes((short)ins.ValueB);
                                    return new byte[] { 0x66, 0xb9, bx16[0], bx16[1] };
                            }
                            break;
                        case Operand.x32:
                            throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
                    }
                }
            }
            throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
        }
        private static byte[] Pop(Instruction ins)
        {
            if (ins.IsUnary)
            {
                byte mgc = 0x54;
                if (ins.OperandA.IsSimpleReg()) return new byte[] { (byte)(mgc + ins.OperandA) };
                else
                {
                    switch (ins.OperandA)
                    {
                        case Operand.x32:
                            throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
                    }
                }
            }
            throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
        }
        private static byte[] Push(Instruction ins)
        {
            if (ins.IsUnary)
            {
                const byte mgc = 0x4c;
                if (ins.OperandA.IsSimpleReg()) return new byte[] { (byte)(mgc + ins.OperandA) };
                else
                {
                    switch (ins.OperandA)
                    {
                        case Operand._esp_a_x8_:
                            byte bx8 = (byte)ins.ValueA;
                            return new byte[] { 0xff, 0x74, 0x24, bx8 };
                        case Operand._ebp_a_x8_:
                            bx8 = (byte)ins.ValueA;
                            return new byte[] { 0xff, 0x75, bx8 };
                        case Operand._ebp_a_x32_:
                            byte[] bx32 = BitConverter.GetBytes((int)ins.ValueA);
                            return new byte[] { 0xff, 0xb5, bx32[0], bx32[1], bx32[2], bx32[3] };
                        case Operand.x8:
                            bx8 = (byte)ins.ValueA;
                            return new byte[] { 0x6a, bx8 };
                        case Operand.x16:
                            byte[] bx16 = BitConverter.GetBytes((short)ins.ValueA);
                            return new byte[] { 0x68, 0x00, 0x00, bx16[1], bx16[0],   };
                        case Operand.x32:
                            bx32 = BitConverter.GetBytes((int)ins.ValueA);
                            return new byte[] { 0x68, bx32[0], bx32[1], bx32[2], bx32[3] };
                    }
                }
            }
            throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
        }
        private static byte[] Ret(Instruction ins)
        {
            if (ins.IsNone) return new byte[] { 0xc3 };
            throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA} {ins.OperandB}");
        }
        private static byte[] Sub(Instruction ins)
        {
            if (ins.IsBinary)
            {
                switch (ins.OperandA)
                {
                    case Operand.eax:
                        switch (ins.OperandB)
                        {
                            case Operand.ebx:
                                throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
                        }
                        break;
                    case Operand.ebx:
                        switch (ins.OperandB)
                        {
                            case Operand.x32:
                                byte[] bx32 = BitConverter.GetBytes((int)ins.ValueB);
                                return new byte[] { 0x81, 0xeb, bx32[0],bx32[1],bx32[2], bx32[3] };
                        }
                        break;
                    case Operand.ecx:
                        throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
                    case Operand.edx:
                        switch (ins.OperandB)
                        {
                            case Operand.ebx:
                                throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
                        }
                        break;
                    case Operand.esi:
                        switch (ins.OperandB)
                        {
                            case Operand.ebx:
                                throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
                        }
                        break;
                    case Operand.esp:
                        switch (ins.OperandB)
                        {
                            case Operand.x8:
                                byte bx8 = (byte)ins.ValueB;
                                return new byte[] { 0x83, 0xec, bx8 };
                        }
                        break;
                    case Operand.x32:
                        throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
                    case Operand._esp_:
                        switch (ins.OperandB)
                        {
                            case Operand.x8:
                                byte bx8 = (byte)ins.ValueB;
                                return new byte[] { 0x80, 0x2c, 0x24, bx8 };
                        }
                        break;
                    case Operand._esp_a_x8_:
                        switch (ins.OperandB)
                        {
                            case Operand.x8:
                                byte ax8 = (byte)ins.ValueA;
                                byte bx8 = (byte)ins.ValueB;
                                return new byte[] { 0x83, 0x6c, 0x24, ax8, bx8 };
                        }
                        break;
                }
            }
            throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
        }
        private static byte[] Xchg(Instruction ins)
        {
            if (ins.IsBinary)
            {
                switch (ins.OperandA)
                {
                    case Operand.eax:
                        switch (ins.OperandB)
                        {
                            case Operand.esi:
                                return new byte[] { 0x96 };
                        }
                        break;
                    case Operand.ebx:
                        throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
                    case Operand.ecx:
                        throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
                    case Operand.edx:
                        throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
                    case Operand.esi:
                        throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
                    case Operand.esp:
                        throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
                    case Operand.x32:
                        throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
                }
            }
            throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
        }
        private static byte[] Xor(Instruction ins)
        {
            switch (ins.OperandA)
            {
                case Operand.ecx:
                    switch (ins.OperandB)
                    {
                        case Operand.ecx:
                            return new byte[] { 0x33, 0xc9 };
                    }
                    break;
                case Operand.esi:
                    switch (ins.OperandB)
                    {
                        case Operand.esi:
                            return new byte[] { 0x33, 0xf6 };
                    }
                    break;
            }
            throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA} {ins.OperandB}");
        }

        private static bool IsSimpleReg(this Operand opr) => (opr >= Operand.eax && opr <= Operand.edi);
        private static bool Is_Reg_(this Operand opr) => (opr >= Operand._eax_ && opr <= Operand._edi_);
        private static bool IsOffsetReg(this Operand opr) => (opr >= Operand._eax_a_x8_ && opr <= Operand._edi_a_x8_);

        /// <summary>
        /// 汇编指令类
        /// </summary>
        private struct Instruction
        {
            /// <summary>
            /// 该汇编指令的操作符
            /// </summary>
            public Operator Operator { get; set; }
            /// <summary>
            /// 该汇编指令操作数A的模式
            /// </summary>
            public Operand OperandA { get; set; }
            /// <summary>
            /// 该汇编指令操作数B的模式
            /// </summary>
            public Operand OperandB { get; set; }
            /// <summary>
            /// 该汇编指令操作数C的模式
            /// </summary>
            public Operand OperandC { get; set; }
            /// <summary>
            /// 与该汇编指令操作数A相关联的数值，可以是偏移量、立即数、标签文本等
            /// </summary>
            public object ValueA { get; set; }
            /// <summary>
            /// 与该汇编指令操作数B相关联的数值，可以是偏移量、立即数、标签文本等
            /// </summary>
            public object ValueB { get; set; }
            /// <summary>
            /// 与该汇编指令操作数C相关联的数值，可以是偏移量、立即数、标签文本等
            /// </summary>
            public object ValueC { get; set; }
            /// <summary>
            /// 是否不存在任何操作数
            /// </summary>
            public bool IsNone { get => (OperandA == Operand.none && OperandB == Operand.none); }
            /// <summary>
            /// 是否只存在操作数A
            /// </summary>
            public bool IsUnary { get => (OperandA != Operand.none && OperandB == Operand.none); }
            /// <summary>
            /// 是否只存在两个操作数
            /// </summary>
            public bool IsBinary { get => (OperandA != Operand.none && OperandB != Operand.none); }
        }
        /// <summary>
        /// 操作符枚举
        /// </summary>
        private enum Operator
        {
            label,
            xor,
            inc,
            dec,
            lodsd,
            ret,
            push,
            pop,
            add,
            sub,
            cmp,
            jne,
            jnz,
            mov,
            lea,
            call,
            xchg,
            jmp,
            undefine
        }
        /// <summary>
        /// 操作数模式枚举
        /// 命名规范：
        /// [ 和 ] 替换为 _；
        /// + 替换为 _a_；
        /// - 替换为 _s_；
        /// * 替换为 _m_；
        /// / 替换为 _d_；
        /// : 替换为 _c_；
        /// 0x?? 替换为 x8；
        /// 0x???? 替换为 x16；
        /// 0x???????? 替换为 x32。
        /// </summary>
        private enum Operand
        {
            none,
            bl,
            bx,
            cx,
            eax,
            ecx,
            edx,
            ebx,
            esp,
            ebp,
            esi,
            edi,
            _eax_,
            _ecx_,
            _ebx_,
            _edx_,
            _esi_,
            _edi_,
            _esp_,
            _ebp_,
            _eax_a_x8_,
            _ecx_a_x8_,
            _ebx_a_x8_,
            _edx_a_x8_,
            _esp_a_x8_,
            _ebp_a_x8_,
            _esi_a_x8_,
            _edi_a_x8_,
            _eax_a_x16_,
            _ecx_a_x16_,
            _ebx_a_x16_,
            _edx_a_x16_,
            _esp_a_x16_,
            _ebp_a_x16_,
            _esi_a_x16_,
            _edi_a_x16_,
            _eax_a_x32_,
            _ecx_a_x32_,
            _ebx_a_x32_,
            _edx_a_x32_,
            _esp_a_x32_,
            _ebp_a_x32_,
            _esi_a_x32_,
            _edi_a_x32_,
            fs_c__ecx_a_x8_,
            _esi_a_ecx_m_2_,
            _esi_a_ecx_m_4_,
            x8,
            x16,
            x32,
            label
        }

    }

}
