using System;

namespace Gleee.Mathematics
{
    public static class Bitwise
    {
        /// <summary>
        /// 返回x的第n位
        /// </summary>
        /// <param name="x"></param>
        /// <param name="n"></param>
        /// <returns>逻辑值为真则为1，否则为0</returns>
        public static int NthBit(int x, int n)
        {
            int n_mask = 1 << n;
            return x & n_mask;
        }
        /// <summary>
        /// 反转x的前bit_depth位
        /// </summary>
        /// <param name="x"></param>
        /// <param name="bit_depth"></param>
        /// <returns></returns>
        public static int Inverse(int x, int bit_depth)
        {
            int inverted = 0;
            for (int n = 0; n < bit_depth / 2 + 1; n++) //只需要交换前一半
            {
                int n_mask = 1 << n;     //只有第n位为1的掩码
                int last_n_mask = 1 << (bit_depth - n - 1);    //只有倒数第n位为1的掩码
                int nth_bit = (x & n_mask) >> n;
                int last_nth_bit = (x & last_n_mask) >> (bit_depth - n - 1);
                if (nth_bit == 1) inverted |= last_n_mask;     //若第n位为1，则在新索引的倒数第n位写入1
                if (last_nth_bit == 1) inverted |= n_mask;     //上一步的对称操作
                //由于新索引初始化为0，所以0的情况不需要写入
                //Console.WriteLine($"mask1: {Convert.ToString(n_mask, 2).PadLeft(6, '0')}");
                //Console.WriteLine($"mask2: {Convert.ToString(last_n_mask, 2).PadLeft(6, '0')}");
                //Console.WriteLine($"i: {Convert.ToString(x, 2).PadLeft(6, '0')}");
                //Console.WriteLine($"j: {Convert.ToString(inverted, 2).PadLeft(6, '0')}\n");
            }
            return inverted;
        }
    }
}
