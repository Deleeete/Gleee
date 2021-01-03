using Gleee.Numerics;
using System;
using System.Diagnostics;

namespace Gleee.Mathematics
{
    /// <summary>
    /// 离散傅里叶变换类
    /// </summary>
    public static class DFT
    {
        /// <summary>
        /// 对一段实信号进行DFT
        /// </summary>
        /// <param name="samples">信号数组</param>
        /// <returns>信号的频域表示F(k)，其中k的范围为0~N-1</returns>
        public static Complex[] Transform(double[] samples)
        {
            int N = samples.Length;
            Complex[] dft = new Complex[N - 1];
            for (int k = 0; k < N - 1; k++)
            {
                Complex product = new Complex(0, 0);
                for (int n = 0; n < N - 1; n++)
                {
                    product += samples[n] * Complex.Exp(-Constants.Tau * n * k / N * Complex.i);
                }
                dft[k] = product / Math.Sqrt(N);
            }
            return dft;
        }
        /// <summary>
        /// 对一段实信号进行DFT
        /// </summary>
        /// <param name="sample">信号数组，其中的元素将会在计算前尝试转换为double类型</param>
        /// <returns></returns>
        public static Complex[] Transform<T>(T[] samples)
        {
            double[] signals = new double[samples.Length];
            for (int i = 0; i < signals.Length; i++)
            {
                signals[i] = Convert.ToDouble(samples[i]);
            }
            int N = signals.Length;
            Complex[] dft = new Complex[N-1];
            for (int k = 0; k < N - 1; k++)
            {
                Complex product = new Complex(0, 0);
                for (int n = 0; n < N - 1; n++)
                {
                    product += signals[n] * Complex.Exp(-Constants.Tau * n * k / N * Complex.i);
                }
                dft[k] = product / Math.Sqrt(N);
            }
            return dft;
        }
        /// <summary>
        /// 对一段实信号进行DFT，并取其模长
        /// </summary>
        /// <param name="samples">信号数组</param>
        /// <returns></returns>
        public static double[] TransformNorm(double[] samples)
        {
            Complex[] dft = Transform(samples);
            double[] dft_norm = new double[dft.Length];
            for (int i = 0; i < dft.Length; i++)
            {
                dft_norm[i] = dft[i].Norm;
            }
            return dft_norm;
        }
        /// <summary>
        /// 对一段实信号进行DFT，并取其模长
        /// </summary>
        /// <param name="samples">信号数组，其中的元素将会在计算前尝试转换为double类型</param>
        /// <returns></returns>
        public static double[] TransformNorm<T>(T[] samples)
        {
            Complex[] dft = Transform(samples);
            double[] dft_norm = new double[dft.Length];
            for (int i = 0; i < dft.Length; i++)
            {
                dft_norm[i] = dft[i].Norm;
            }
            return dft_norm;
        }
    }
    /// <summary>
    /// 快速傅里叶变换类
    /// </summary>
    public static class FFT
    {
        private static int TryGetExpOf2(int x)
        {
            double exp = Math.Log(x, 2);
            if (exp - Math.Floor(exp) > double.Epsilon) throw new Exception("FFT运算需要2^N维数据");
            return Convert.ToInt32(exp);
        }
        private static void BitwiseInvertExchange(Complex[] data)
        {
            int bit_depth = TryGetExpOf2(data.Length);
            for (int i = 0; i < data.Length; i++)
            {
                int new_i = Bitwise.Inverse(i, bit_depth);
                if (i < new_i)
                {
                    Complex tmp = data[i];
                    data[i] = data[new_i];
                    data[new_i] = tmp;
                } 
            }
        }
        private static void ButterflyW(Complex[] input, int i, int j, Complex W)
        {
            Complex tmp = input[i];
            Complex product = input[j] * W;
            input[i] = tmp + product;
            input[j] = tmp - product;
        }
        private static Complex W(int kn, int N)
        {
            return Complex.Exp(-Constants.Tau * kn / N * Complex.i);
        }
        /// <summary>
        /// 使用FFT将复信号就地转换到频域
        /// </summary>
        /// <param name="samples"></param>
        public static void Transform(Complex[] samples)
        {
            //Console.WriteLine($"对{samples.ToMmaString()}进行FFT变换...");
            int exp = TryGetExpOf2(samples.Length);
            //Console.WriteLine($"N={samples.Length}，运算深度为{exp}。");
            //先对下标进行位反转变换
            BitwiseInvertExchange(samples);
            for (int stage = 1; stage <= exp; stage++)
            {
                int groups_count = Convert.ToInt32(Math.Pow(2, exp - stage));                  //蝶形运算的小组数
                int group_length = samples.Length / 2 / groups_count;      //每个小组中蝶形运算的数量。可以注意到groups_count*group_length满足结果恒等于N/2
                //Console.WriteLine($"第{stage}级：小组数{groups_count}\t小组长度：{group_length}");
                for (int group = 0; group < groups_count; group++)    //每一次循环代表一组蝶形运算
                {
                    for (int offset = 0; offset < group_length; offset++) //计算本组的蝶形运算
                    {
                        int i = Convert.ToInt32(group * Math.Pow(2, stage)) + offset;   //第n个小组的头部位于n*2^stage处
                        int j = i + group_length;   //输入端距离恒等于小组的长度
                        //Console.WriteLine($"\t正在计算{i}-{j}对{offset * groups_count}的蝶形运算...");
                        ButterflyW(samples, i, j, W(offset * groups_count, samples.Length));//旋转因子的角度等于小组数
                    }
                }
            }
            for (int i = 0; i < samples.Length; i++)
            {
                if (double.IsInfinity(samples[i].Re) || double.IsInfinity(samples[i].Re)) Debugger.Break();
            }
        }
        /// <summary>
        /// 使用FFT将实信号转换到频域
        /// </summary>
        /// <param name="samples"></param>
        /// <returns>频域的复数数组</returns>
        public static Complex[] Transform(double[] samples)
        {
            Complex[] c = Complex.FromDoubleArray(samples);
            Transform(c);
            return c;
        }
        /// <summary>
        /// 使用FFT将复信号就地转换到频域，并提取模长。注意，传入的复信号数组将会被覆写
        /// </summary>
        /// <param name="samples"></param>
        /// <returns>信号在频域的模长数组</returns>
        public static double[] TransformNorm(Complex[] samples)
        {
            Transform(samples);
            double[] d = new double[samples.Length];
            for (int i = 0; i < samples.Length; i++)
            {
                d[i] = samples[i].Norm;
            }
            return d;
        }
        /// <summary>
        /// 使用FFT将实信号转换到频域，并提取模长
        /// </summary>
        /// <param name="samples"></param>
        /// <returns>信号在频域的模长数组</returns>
        public static double[] TransformNorm(double[] samples)
        {
            Complex[] c = Transform(samples);
            double[] d = new double[c.Length];
            for (int i = 0; i < c.Length; i++)
            {
                d[i] = c[i].Norm;
            }
            return d;
        }
        /// <summary>
        /// 使用FFT将实信号转换到频域，并提取模长。信号数据将会被尝试转换为双精度浮点数
        /// </summary>
        /// <typeparam name="T">信号的数据类型</typeparam>
        /// <param name="samples"></param>
        /// <returns>信号在频域的模长数组</returns>
        public static double[] TransformNorm<T>(T[] samples)
        {
            double[] signals = new double[samples.Length];
            for (int i = 0; i < signals.Length; i++)
            {
                signals[i] = Convert.ToDouble(samples[i]);
            }
            Complex[] c = Complex.FromDoubleArray(signals);
            Transform(c);
            double[] d = new double[c.Length];
            for (int i = 0; i < c.Length; i++)
            {
                d[i] = c[i].Norm;
            }
            return d;
        }
        /// <summary>
        /// 使用FFT将复信号就地转换到频域，并提取实部。注意，传入的复信号数组将会被覆写
        /// </summary>
        /// <param name="data"></param>
        /// <returns>信号在频域的实部数组</returns>
        public static double[] TransformReal(Complex[] data)
        {
            Transform(data);
            double[] d = new double[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                d[i] = data[i].Re;
            }
            return d;
        }
        /// <summary>
        /// 使用FFT将实信号转换到频域，并提取实部
        /// </summary>
        /// <param name="samples"></param>
        /// <returns>信号在频域的实部数组</returns>
        public static double[] TransformReal(double[] samples)
        {
            Complex[] c = Transform(samples);
            double[] d = new double[c.Length];
            for (int i = 0; i < c.Length; i++)
            {
                d[i] = c[i].Re;
            }
            return d;
        }
        /// <summary>
        /// 使用FFT将实信号转换到频域，并提取实部。信号数据将会被尝试转换为双精度浮点数
        /// </summary>
        /// <typeparam name="T">信号的数据类型</typeparam>
        /// <param name="samples"></param>
        /// <returns>信号在频域的实部数组</returns>
        public static double[] TransformReal<T>(T[] samples)
        {
            double[] signals = new double[samples.Length];
            for (int i = 0; i < signals.Length; i++)
            {
                signals[i] = Convert.ToDouble(samples[i]);
            }
            Complex[] c = Complex.FromDoubleArray(signals);
            Transform(c);
            double[] d = new double[c.Length];
            for (int i = 0; i < c.Length; i++)
            {
                d[i] = c[i].Re;
            }
            return d;
        }
    }
}
