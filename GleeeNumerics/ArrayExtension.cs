using System;

namespace Gleee.Numerics
{
    /// <summary>
    /// 16位无符号整型数组扩展方法
    /// </summary>
    public static class UInt16ArrayExtension
    {
        /// <summary>
        /// 将两个数组的值分别相加
        /// </summary>
        /// <param name="a">加数数组1</param>
        /// <param name="b">加数数组2</param>
        /// <returns>和数组</returns>
        public static ushort[] Add(this ushort[] a, ushort[] b)
        {
            if (a.Length != b.Length) throw new Exception("进行运算的两个数组具有不同长度");
            ushort[] c = new ushort[a.Length];
            for (ushort n = 0; n < a.Length; n++)
            {
                c[n] = Convert.ToUInt16(a[n] + b[n]);
            }
            return a;
        }
        /// <summary>
        /// 将两个数组的值分别相减
        /// </summary>
        /// <param name="a">被减数数组</param>
        /// <param name="b">减数数组</param>
        /// <returns>差数组</returns>
        public static ushort[] Sub(this ushort[] a, ushort[] b)
        {
            if (a.Length != b.Length) throw new Exception("进行运算的两个数组具有不同长度");
            ushort[] c = new ushort[a.Length];
            for (ushort n = 0; n < a.Length; n++)
            {
                c[n] = Convert.ToUInt16(a[n] - b[n]);
            }
            return a;
        }
        /// <summary>
        /// 将两个数组的值分别相乘
        /// </summary>
        /// <param name="a">因数数组1</param>
        /// <param name="b">因数数组1</param>
        /// <returns>积数组</returns>
        public static ushort[] Multiply(this ushort[] a, ushort[] b)
        {
            if (a.Length != b.Length) throw new Exception("进行运算的两个数组具有不同长度");
            ushort[] c = new ushort[a.Length];
            for (ushort n = 0; n < a.Length; n++)
            {
                c[n] = Convert.ToUInt16(a[n] * b[n]);
            }
            return a;
        }
        /// <summary>
        /// 将两个数组的值分别相除
        /// </summary>
        /// <param name="a">被除数数组</param>
        /// <param name="b">除数数组</param>
        /// <returns>商数组</returns>
        public static ushort[] Divide(this ushort[] a, ushort[] b)
        {
            if (a.Length != b.Length) throw new Exception("进行运算的两个数组具有不同长度");
            ushort[] c = new ushort[a.Length];
            for (ushort n = 0; n < a.Length; n++)
            {
                c[n] = Convert.ToUInt16(a[n] / b[n]);
            }
            return a;
        }
        /// <summary>
        /// 将任意一元函数应用于数组的每一个元素
        /// </summary>
        /// <param name="a">自变量数组</param>
        /// <param name="func">一元函数</param>
        /// <returns>函数值数组</returns>
        public static ushort[] Map(this ushort[] a, Func<ushort, ushort> func)
        {
            ushort[] c = new ushort[a.Length];
            for (ushort n = 0; n < a.Length; n++)
            {
                c[n] = func(a[n]);
            }
            return a;
        }
    }

    /// <summary>
    /// 16位整型数组扩展方法
    /// </summary>
    public static class Int16ArrayExtension
    {
        /// <summary>
        /// 将两个数组的值分别相加
        /// </summary>
        /// <param name="a">加数数组1</param>
        /// <param name="b">加数数组2</param>
        /// <returns>和数组</returns>
        public static short[] Add(this short[] a, short[] b)
        {
            if (a.Length != b.Length) throw new Exception("进行运算的两个数组具有不同长度");
            short[] c = new short[a.Length];
            for (short n = 0; n < a.Length; n++)
            {
                c[n] = Convert.ToInt16(a[n] + b[n]);
            }
            return a;
        }
        /// <summary>
        /// 将两个数组的值分别相减
        /// </summary>
        /// <param name="a">被减数数组</param>
        /// <param name="b">减数数组</param>
        /// <returns>差数组</returns>
        public static short[] Sub(this short[] a, short[] b)
        {
            if (a.Length != b.Length) throw new Exception("进行运算的两个数组具有不同长度");
            short[] c = new short[a.Length];
            for (short n = 0; n < a.Length; n++)
            {
                c[n] = Convert.ToInt16(a[n] - b[n]);
            }
            return a;
        }
        /// <summary>
        /// 将两个数组的值分别相乘
        /// </summary>
        /// <param name="a">因数数组1</param>
        /// <param name="b">因数数组1</param>
        /// <returns>积数组</returns>
        public static short[] Multiply(this short[] a, short[] b)
        {
            if (a.Length != b.Length) throw new Exception("进行运算的两个数组具有不同长度");
            short[] c = new short[a.Length];
            for (short n = 0; n < a.Length; n++)
            {
                c[n] = Convert.ToInt16(a[n] * b[n]);
            }
            return a;
        }
        /// <summary>
        /// 将两个数组的值分别相除
        /// </summary>
        /// <param name="a">被除数数组</param>
        /// <param name="b">除数数组</param>
        /// <returns>商数组</returns>
        public static short[] Divide(this short[] a, short[] b)
        {
            if (a.Length != b.Length) throw new Exception("进行运算的两个数组具有不同长度");
            short[] c = new short[a.Length];
            for (short n = 0; n < a.Length; n++)
            {
                c[n] = Convert.ToInt16(a[n] / b[n]);
            }
            return a;
        }
        /// <summary>
        /// 将任意一元函数应用于数组的每一个元素
        /// </summary>
        /// <param name="a">自变量数组</param>
        /// <param name="func">一元函数</param>
        /// <returns>函数值数组</returns>
        public static short[] Map(this short[] a, Func<short, short> func)
        {
            short[] c = new short[a.Length];
            for (short n = 0; n < a.Length; n++)
            {
                c[n] = func(a[n]);
            }
            return a;
        }
    }

    /// <summary>
    /// 32位无符号整型数组扩展方法
    /// </summary>
    public static class UInt32ArrayExtension
    {
        /// <summary>
        /// 将两个数组的值分别相加
        /// </summary>
        /// <param name="a">加数数组1</param>
        /// <param name="b">加数数组2</param>
        /// <returns>和数组</returns>
        public static uint[] Add(this uint[] a, uint[] b)
        {
            if (a.Length != b.Length) throw new Exception("进行运算的两个数组具有不同长度");
            uint[] c = new uint[a.Length];
            for (uint n = 0; n < a.Length; n++)
            {
                c[n] = a[n] + b[n];
            }
            return a;
        }
        /// <summary>
        /// 将两个数组的值分别相减
        /// </summary>
        /// <param name="a">被减数数组</param>
        /// <param name="b">减数数组</param>
        /// <returns>差数组</returns>
        public static uint[] Sub(this uint[] a, uint[] b)
        {
            if (a.Length != b.Length) throw new Exception("进行运算的两个数组具有不同长度");
            uint[] c = new uint[a.Length];
            for (uint n = 0; n < a.Length; n++)
            {
                c[n] = a[n] - b[n];
            }
            return a;
        }
        /// <summary>
        /// 将两个数组的值分别相乘
        /// </summary>
        /// <param name="a">因数数组1</param>
        /// <param name="b">因数数组1</param>
        /// <returns>积数组</returns>
        public static uint[] Multiply(this uint[] a, uint[] b)
        {
            if (a.Length != b.Length) throw new Exception("进行运算的两个数组具有不同长度");
            uint[] c = new uint[a.Length];
            for (uint n = 0; n < a.Length; n++)
            {
                c[n] = a[n] * b[n];
            }
            return a;
        }
        /// <summary>
        /// 将两个数组的值分别相除
        /// </summary>
        /// <param name="a">被除数数组</param>
        /// <param name="b">除数数组</param>
        /// <returns>商数组</returns>
        public static uint[] Divide(this uint[] a, uint[] b)
        {
            if (a.Length != b.Length) throw new Exception("进行运算的两个数组具有不同长度");
            uint[] c = new uint[a.Length];
            for (uint n = 0; n < a.Length; n++)
            {
                c[n] = a[n] / b[n];
            }
            return a;
        }
        /// <summary>
        /// 将任意一元函数应用于数组的每一个元素
        /// </summary>
        /// <param name="a">自变量数组</param>
        /// <param name="func">一元函数</param>
        /// <returns>函数值数组</returns>
        public static uint[] Map(this uint[] a, Func<uint, uint> func)
        {
            uint[] c = new uint[a.Length];
            for (uint n = 0; n < a.Length; n++)
            {
                c[n] = func(a[n]);
            }
            return a;
        }
    }

    /// <summary>
    /// 32位整型数组扩展方法
    /// </summary>
    public static class Int32ArrayExtension
    {
        /// <summary>
        /// 将两个数组的值分别相加
        /// </summary>
        /// <param name="a">加数数组1</param>
        /// <param name="b">加数数组2</param>
        /// <returns>和数组</returns>
        public static int[] Add(this int[] a, int[] b)
        {
            if (a.Length != b.Length) throw new Exception("进行运算的两个数组具有不同长度");
            int[] c = new int[a.Length];
            for (int n = 0; n < a.Length; n++)
            {
                c[n] = a[n] + b[n];
            }
            return a;
        }
        /// <summary>
        /// 将两个数组的值分别相减
        /// </summary>
        /// <param name="a">被减数数组</param>
        /// <param name="b">减数数组</param>
        /// <returns>差数组</returns>
        public static int[] Sub(this int[] a, int[] b)
        {
            if (a.Length != b.Length) throw new Exception("进行运算的两个数组具有不同长度");
            int[] c = new int[a.Length];
            for (int n = 0; n < a.Length; n++)
            {
                c[n] = a[n] - b[n];
            }
            return a;
        }
        /// <summary>
        /// 将两个数组的值分别相乘
        /// </summary>
        /// <param name="a">因数数组1</param>
        /// <param name="b">因数数组1</param>
        /// <returns>积数组</returns>
        public static int[] Multiply(this int[] a, int[] b)
        {
            if (a.Length != b.Length) throw new Exception("进行运算的两个数组具有不同长度");
            int[] c = new int[a.Length];
            for (int n = 0; n < a.Length; n++)
            {
                c[n] = a[n] * b[n];
            }
            return a;
        }
        /// <summary>
        /// 将两个数组的值分别相除
        /// </summary>
        /// <param name="a">被除数数组</param>
        /// <param name="b">除数数组</param>
        /// <returns>商数组</returns>
        public static int[] Divide(this int[] a, int[] b)
        {
            if (a.Length != b.Length) throw new Exception("进行运算的两个数组具有不同长度");
            int[] c = new int[a.Length];
            for (int n = 0; n < a.Length; n++)
            {
                c[n] = a[n] / b[n];
            }
            return a;
        }
        /// <summary>
        /// 将任意一元函数应用于数组的每一个元素
        /// </summary>
        /// <param name="a">自变量数组</param>
        /// <param name="func">一元函数</param>
        /// <returns>函数值数组</returns>
        public static int[] Map(this int[] a, Func<int, int> func)
        {
            int[] c = new int[a.Length];
            for (int n = 0; n < a.Length; n++)
            {
                c[n] = func(a[n]);
            }
            return a;
        }
    }

    /// <summary>
    /// 单精度数组扩展方法
    /// </summary>
    public static class SingleArrayExtension
    {
        /// <summary>
        /// 将两个数组的值分别相加
        /// </summary>
        /// <param name="a">加数数组1</param>
        /// <param name="b">加数数组2</param>
        /// <returns>和数组</returns>
        public static float[] Add(this float[] a, float[] b)
        {
            if (a.Length != b.Length) throw new Exception("进行运算的两个数组具有不同长度");
            float[] c = new float[a.Length];
            for (int n = 0; n < a.Length; n++)
            {
                c[n] = a[n] + b[n];
            }
            return a;
        }
        /// <summary>
        /// 将两个数组的值分别相减
        /// </summary>
        /// <param name="a">被减数数组</param>
        /// <param name="b">减数数组</param>
        /// <returns>差数组</returns>
        public static float[] Sub(this float[] a, float[] b)
        {
            if (a.Length != b.Length) throw new Exception("进行运算的两个数组具有不同长度");
            float[] c = new float[a.Length];
            for (int n = 0; n < a.Length; n++)
            {
                c[n] = a[n] - b[n];
            }
            return a;
        }
        /// <summary>
        /// 将两个数组的值分别相乘
        /// </summary>
        /// <param name="a">因数数组1</param>
        /// <param name="b">因数数组1</param>
        /// <returns>积数组</returns>
        public static float[] Multiply(this float[] a, float[] b)
        {
            if (a.Length != b.Length) throw new Exception("进行运算的两个数组具有不同长度");
            float[] c = new float[a.Length];
            for (int n = 0; n < a.Length; n++)
            {
                c[n] = a[n] * b[n];
            }
            return a;
        }
        /// <summary>
        /// 将两个数组的值分别相除
        /// </summary>
        /// <param name="a">被除数数组</param>
        /// <param name="b">除数数组</param>
        /// <returns>商数组</returns>
        public static float[] Divide(this float[] a, float[] b)
        {
            if (a.Length != b.Length) throw new Exception("进行运算的两个数组具有不同长度");
            float[] c = new float[a.Length];
            for (int n = 0; n < a.Length; n++)
            {
                c[n] = a[n] / b[n];
            }
            return a;
        }
        /// <summary>
        /// 将任意一元函数应用于数组的每一个元素
        /// </summary>
        /// <param name="a">自变量数组</param>
        /// <param name="func">一元函数</param>
        /// <returns>函数值数组</returns>
        public static float[] Map(this float[] a, Func<float, float> func)
        {
            float[] c = new float[a.Length];
            for (int n = 0; n < a.Length; n++)
            {
                c[n] = func(a[n]);
            }
            return a;
        }
    }

    /// <summary>
    /// 双精度数组扩展方法
    /// </summary>
    public static class DoubleArrayExtension
    {
        /// <summary>
        /// 将两个数组的值分别相加
        /// </summary>
        /// <param name="a">加数数组1</param>
        /// <param name="b">加数数组2</param>
        /// <returns>和数组</returns>
        public static double[] Add(this double[] a, double[] b)
        {
            if (a.Length != b.Length) throw new Exception("进行运算的两个数组具有不同长度");
            double[] c = new double[a.Length];
            for (int n = 0; n < a.Length; n++)
            {
                c[n] = a[n] + b[n];
            }
            return a;
        }
        /// <summary>
        /// 将两个数组的值分别相减
        /// </summary>
        /// <param name="a">被减数数组</param>
        /// <param name="b">减数数组</param>
        /// <returns>差数组</returns>
        public static double[] Sub(this double[] a, double[] b)
        {
            if (a.Length != b.Length) throw new Exception("进行运算的两个数组具有不同长度");
            double[] c = new double[a.Length];
            for (int n = 0; n < a.Length; n++)
            {
                c[n] = a[n] - b[n];
            }
            return a;
        }
        /// <summary>
        /// 将两个数组的值分别相乘
        /// </summary>
        /// <param name="a">因数数组1</param>
        /// <param name="b">因数数组1</param>
        /// <returns>积数组</returns>
        public static double[] Multiply(this double[] a, double[] b)
        {
            if (a.Length != b.Length) throw new Exception("进行运算的两个数组具有不同长度");
            double[] c = new double[a.Length];
            for (int n = 0; n < a.Length; n++)
            {
                c[n] = a[n] * b[n];
            }
            return a;
        }
        /// <summary>
        /// 将两个数组的值分别相除
        /// </summary>
        /// <param name="a">被除数数组</param>
        /// <param name="b">除数数组</param>
        /// <returns>商数组</returns>
        public static double[] Divide(this double[] a, double[] b)
        {
            if (a.Length != b.Length) throw new Exception("进行运算的两个数组具有不同长度");
            double[] c = new double[a.Length];
            for (int n = 0; n < a.Length; n++)
            {
                c[n] = a[n] / b[n];
            }
            return a;
        }
        /// <summary>
        /// 将任意一元函数应用于数组的每一个元素
        /// </summary>
        /// <param name="a">自变量数组</param>
        /// <param name="func">一元函数</param>
        /// <returns>函数值数组</returns>
        public static double[] Map(this double[] a, Func<double, double> func)
        {
            double[] c = new double[a.Length];
            for (int n = 0; n < a.Length; n++)
            {
                c[n] = func(a[n]);
            }
            return c;
        }
        /// <summary>
        /// 将数组转换为Mathematica风格的字符串
        /// </summary>
        /// <param name="a">待转换的数组</param>
        /// <returns>转换后的字符串</returns>
        public static string ToMmaString(this double[] a)
        {
            string re = "{";
            for (int i = 0; i < a.Length; i++)
            {
                if (i != 0) re += ",";
                re += a[i];
            }
            re += "}";
            return re;
        }
    }

}
