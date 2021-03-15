using System;
using System.Linq;

namespace Gleee.Utils.Extension
{
    public static class ByteArrayExtensions
    {
        public static string ToHexString(this byte[] bin)
        {
            string[] re = new string[bin.Length];
            for (int i = 0; i < bin.Length; i++)
            {
                re[i] = bin[i].ToString("x2");
            }
            return string.Join("", re);
        }
        public static string ToHexString(this byte[] bin, string separator)
        {
            string[] re = new string[bin.Length];
            for (int i = 0; i < bin.Length; i++)
            {
                re[i] = bin[i].ToString("x2");
            }
            return string.Join(separator, re);
        }
        public static void LoadFromHexString(this byte[] bin, string hex_array)
        {
            if (bin.Length * 2 != hex_array.Length)
                throw new Exception("无法导入十六进制字符串：长度不一致");
            for (int i = 0; i < hex_array.Length; i += 2)
            {
                string hex = hex_array.Substring(i, 2);
                bin[i / 2] = Convert.ToByte(hex, 16);
            }
        }
        public static void LoadFromHexString(this byte[] bin, string hex_array, int start_index)
        {
            if ((bin.Length - start_index) * 2 < hex_array.Length)
                throw new Exception("无法导入十六进制字符串：字符串长度超出目标数组界限");
            for (int i = start_index; i < hex_array.Length; i += 2)
            {
                string hex = hex_array.Substring(i, 2);
                bin[i] = Convert.ToByte(hex, 16);
            }
        }
        public static byte[] SubArray(this byte[] bin, int start_index, int length)
        {
            byte[] sub = new byte[length];
            for (int i = 0; i < length; i++)
            {
                sub[i] = bin[start_index + i];
            }
            return sub;
        }
        public static byte[] ReverseArray(this byte[] bin) => bin.Reverse().ToArray();
        public static byte[] Cat(this byte[] bin, byte[] cat) => bin.Concat(cat).ToArray();
    }
    public static class StringExtensions
    {
        public static byte[] ToByteArray(this string str)
        {
            if (str.Length % 2 != 0)
                throw new Exception("十六进制字符串的字数为奇数，无法正确转换");
            byte[] bin = new byte[str.Length / 2];
            for (int i = 0; i < str.Length; i += 2)
            {
                string hex = str.Substring(i, 2);
                bin[i / 2] = Convert.ToByte(hex, 16);
            }
            return bin;
        }
    }
    public static class UintExtensions
    {
        public static byte[] GetBytes(this uint x) => BitConverter.GetBytes(x);
    }
}
