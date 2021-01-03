using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Gleee.Asm
{
    public static class Asm
    {
        //private static Regex _reg_ = new Regex(@"\[([a-z][a-z][a-z])\]");
        //private static readonly Regex _reg_x8 = new Regex(@"^\[([a-z]{3})\+(0x[0-9a-fA-F]{2})\]$");
        //private static readonly Regex fs_reg_x8 = new Regex(@"^fs\:\[([a-z]{3})\+(0x[0-9a-fA-F]{2})\]$");
        private static readonly Regex x8_pat = new Regex(@"\b(0x[a-fA-F0-9]{2})\b");
        private static readonly Regex x16_pat = new Regex(@"\b(0x[a-fA-F0-9]{4})\b");
        private static readonly Regex x32_pat = new Regex(@"\b(0x[a-fA-F0-9]{8})\b");
        private static readonly Regex labelC_pat = new Regex(@"^([a-zA-Z0-9_]+)\:$");
        private static readonly Regex label_pat = new Regex(@"^([a-zA-Z0-9_]+)$");
        //private static Regex _reg_reg_m_2 = new Regex(@"\[([a-z][a-z][a-z])\+([a-z][a-z][a-z])\*2\]");
        //private static Regex _reg_reg_m_4 = new Regex(@"\[([a-z][a-z][a-z])\+([a-z][a-z][a-z])\*4\]");

        public static byte[] AssembleAll(string[] asm)
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
                    bin.AddRange(AssembleLine(ins));
                } 
            }
            foreach (var kvp in jnzs)
            {
                int jnz_addr = kvp.Key;    //跳转命令的地址
                Debug.Print($"跳转命令的地址为0x{jnz_addr:x8}，标签为{kvp.Value}");
                string label = kvp.Value;   //目标标签
                Debug.Print($"标签的地址为0x{jnz_addr:x8}");
                int lbl_addr = labels[label];   //标签的地址
                byte offset = (byte)(lbl_addr - jnz_addr - 0x2);
                Debug.Print($"计算出的跳转值为0x{offset:x2}");
                bin[kvp.Key + 1] = offset;
            }
            return bin.ToArray();
        }
        public static Instruction ParseLine(string line)
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

            //单独处理跳转的操作数
            if (opt == Operator.jne || opt == Operator.jnz)
            {
                if (strs.Length == 2 && !strs[1].StartsWith(";"))
                {
                    string lbl = label_pat.Match(strs[1]).Groups[1].Value;
                    valuea = lbl;
                    opra = Operand.label;
                    Instruction jmp_ins = new Instruction
                    {
                        Operator = opt,
                        OperandA = opra,
                        OperandB = oprb,
                        ValueA = valuea,
                        ValueB = valueb,
                    };
                    return jmp_ins;
                }
                else throw new Exception("跳转的操作数数量不正确");
            }
            //解析第一个操作数（若存在）
            else if (strs.Length > 1 && !strs[1].StartsWith(";") && strs[1].Trim().Length != 0)
            {
                string[] oprs = strs[1].Split(',');
                string opra_str = oprs[0].Split(';')[0];
                string opra_pre_str = OperandSignature(opra_str);
                //获取操作数类型
                if (!Enum.TryParse(opra_pre_str, out opra)) throw new Exception($"无法识别的操作数模式：{opra_str}");
                //获取与操作数A相关的值
                valuea = ExtractValue(opra_str);

                //解析第二个操作数（若存在）
                if (oprs.Length == 2 && !oprs[1].StartsWith(";") && oprs[1].Trim().Length!=0)   //存在，非注释，非空白字符串
                {
                    string oprb_str = oprs[1].Split(';')[0];
                    string oprb_pre_str = OperandSignature(oprb_str);
                    //解析操作数类型
                    if (!Enum.TryParse(oprb_pre_str, out oprb)) throw new Exception($"无法识别的操作数模式：{oprb_str}");
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
        //汇编一行
        public static byte[] AssembleLine(Instruction ins)
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
                case Operator.jne:
                    return Jne(ins);
                case Operator.jnz:
                    return Jne(ins);
                case Operator.mov:
                    return Mov(ins);
                case Operator.call:
                    return Call(ins);

            }
            throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA} {ins.OperandB}");
        }
        //生成操作数模式签名
        public static string OperandSignature(string raw)
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
        private static byte[] Lodsd(Instruction ins)
        {
            if (ins.IsNone) return new byte[] { 0xad };
            throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA} {ins.OperandB}");
        }
        private static byte[] Ret(Instruction ins)
        {
            if (ins.IsNone) return new byte[] { 0xc3 };
            throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA} {ins.OperandB}");
        }
        private static byte[] Push(Instruction ins)
        {
            if (ins.IsUnary)
            {
                switch (ins.OperandA)
                {
                    case Operand.eax:
                        return new byte[] { 0x50 };
                    case Operand.ebx:
                        return new byte[] { 0x53 };
                    case Operand.ecx:
                        return new byte[] { 0x51 };
                    case Operand.edx:
                        return new byte[] { 0x52 };
                    case Operand.esp:
                        return new byte[] { 0x54 };
                    case Operand.esi:
                        return new byte[] { 0x56 };
                    case Operand.x32:
                        byte[] bx32 = BitConverter.GetBytes((int)ins.ValueA);
                        return new byte[] { 0x68, bx32[0], bx32[1], bx32[2], bx32[3] };
                }
            }
            throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
        }
        private static byte[] Pop(Instruction ins)
        {
            if (ins.IsUnary)
            {
                switch (ins.OperandA)
                {
                    case Operand.eax:
                        throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
                    case Operand.ebx:
                        return new byte[] { 0x5b };
                    case Operand.ecx:
                        return new byte[] { 0x59 };
                    case Operand.edx:
                        return new byte[] { 0x5a };
                    case Operand.esi:
                        return new byte[] { 0x5e };
                    case Operand.esp:
                        throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
                    case Operand.x32:
                        throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
                }
            }
            throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
        }
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
                        }
                        break;
                    case Operand.ebx:
                        throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
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
                    case Operand.x32:
                        throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
                }
            }
            throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
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
                        throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
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
                                throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
                        }
                        break;
                    case Operand.x32:
                        throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
                    case Operand._esp_a_x8_:
                        switch (ins.OperandB)
                        {
                            case Operand.x8:
                                byte a8 = (byte)ins.ValueA;
                                byte b8 = (byte)ins.ValueB;
                                return new byte[] { 0x83, 0x6c, a8, b8};
                        }
                        break;
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
                                return new byte[] { 0x81, 0x38, bx32[0], bx32[1], bx32[2], bx32[3]};
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
                        //int src = ins.ValueA;
                        //int dst = ins.ValueB;
                        //byte len = (byte)(dst - src);
                        return new byte[] { 0x75, 0xff };
                }
            }
            throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
        }
        private static byte[] Mov(Instruction ins)
        {
            if (ins.IsBinary)
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
                                return new byte[] { 0x8b, 0x40, bx8};
                            case Operand.fs_c__ecx_a_x8_:
                                bx8 = (byte)ins.ValueB;
                                return new byte[] { 0x64, 0x8b, 0x41, bx8 };
                        }
                        break;
                    case Operand.ebx:
                        switch (ins.OperandB)
                        {
                            case Operand._eax_:
                                throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
                            case Operand._eax_a_x8_:
                                byte bx8 = (byte)ins.ValueB;
                                return new byte[] { 0x8b, 0x58, bx8 };
                            case Operand.fs_c__ecx_a_x8_:
                                throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
                        }
                        break;
                    case Operand.ecx:
                        switch (ins.OperandB)
                        {
                            case Operand.x32:
                                byte[] bx32 = BitConverter.GetBytes((int)ins.ValueB);
                                return new byte[] { 0xb9, bx32[0], bx32[1], bx32[2], bx32[3] };
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
                                return new byte[] { 0x8b, 0x54, bx8 };
                            case Operand._esi_a_ecx_m_4_:
                                return new byte[] { 0x8b, 0x14, 0x8e };
                        }
                        break;
                    case Operand.esi:
                        switch (ins.OperandB)
                        {
                            case Operand._edx_a_x8_:
                                byte bx8 = (byte)ins.ValueB;
                                return new byte[] { 0x8b, 0x72, bx8 };
                        }
                        break;
                    case Operand.esp:
                        throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
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
            throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
        }
        private static byte[] Call(Instruction ins)
        {
            if (ins.IsUnary)
            {
                switch (ins.OperandA)
                {
                    case Operand.eax:
                        return new byte[] { 0xff, 0xd0 };
                    case Operand.ebx:
                        throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
                    case Operand.ecx:
                        throw new Exception($"遇到未实现或错误的指令组合{ins.Operator} {ins.OperandA},{ins.OperandB}");
                    case Operand.edx:
                        return new byte[] { 0xff, 0xd2 };
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
    }

    public class Instruction
    {
        public Operator Operator { get; set; }
        public Operand OperandA { get; set; }
        public Operand OperandB { get; set; }
        public object ValueA { get; set; }
        public object ValueB { get; set; }
        public bool IsNone { get => (OperandA == Operand.none && OperandB == Operand.none); }
        public bool IsUnary { get => (OperandA != Operand.none && OperandB == Operand.none); }
        public bool IsBinary { get => (OperandA != Operand.none && OperandB != Operand.none); }
    }
    public enum Operator
    {
        [Description("lable")]
        label,
        [Description("xor")]
        xor,
        [Description("inc")]
        inc,
        [Description("dec")]
        dec,
        [Description("lodsd")]
        lodsd,
        [Description("ret")]
        ret,
        [Description("push")]
        push,
        [Description("pop")]
        pop,
        [Description("add")]
        add,
        [Description("sub")]
        sub,
        [Description("cmp")]
        cmp,
        [Description("jne")]
        jne,
        [Description("jnz")]
        jnz,
        [Description("mov")]
        mov,
        [Description("call")]
        call,
        undefine
    }
    public enum Operand
    {
        /*弱智命名规范：
          [ 和 ] 替换为 _
          + 替换为 _a_
          - 替换为 _s_
          * 替换为 _m_
          / 替换为 _d_
          : 替换为 _c_
          0x?? 替换为 x8
          0x???? 替换为 x16
          0x???????? 替换为 x32
        */
        none,
        [Description("cx")]
        cx,
        [Description("eax")]
        eax,
        [Description("ebx")]
        ebx,
        [Description("ecx")]
        ecx,
        [Description("edx")]
        edx,
        [Description("esi")]
        esi,
        [Description("esp")]
        esp,
        [Description("[eax]")]
        _eax_,
        [Description("[ebx]")]
        _ebx_,
        [Description("[ecx]")]
        _ecx_,
        [Description("[edx]")]
        _edx_,
        [Description("[esi]")]
        _esi_,
        [Description("[esp]")]
        _esp_,
        [Description("[eax+x8]")]
        _eax_a_x8_,
        [Description("[ebx+x8]")]
        _ebx_a_x8_,
        [Description("[ecx+x8]")]
        _ecx_a_x8_,
        [Description("[edx+x8]")]
        _edx_a_x8_,
        [Description("[esi+x8]")]
        _esi_a_x8_,
        [Description("[esp+x8]")]
        _esp_a_x8_,
        [Description("fs:[ecx+x8]")]
        fs_c__ecx_a_x8_,
        [Description("[esi+ecx*2]")]
        _esi_a_ecx_m_2_,
        [Description("[esi+ecx*4]")]
        _esi_a_ecx_m_4_,
        [Description("x8")]
        x8,
        [Description("x16")]
        x16,
        [Description("x32")]
        x32,
        label
    }
}
