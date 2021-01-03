using System;
using System.Security.Authentication;

namespace Gleee.Numerics
{
    public struct VectorN
    {
        private readonly double[] vec;
        public int Dimension => vec.Length;
        /// <summary>
        /// 向量的长度
        /// </summary>
        public double Norm
        {
            get
            {
                double sum = 0;
                for (int i = 0; i < Dimension; i++)
                {
                    sum += this[i] * this[i];
                }
                return Math.Sqrt(sum);
            }
        }
        /// <summary>
        /// 构造维度为length的零向量
        /// </summary>
        public VectorN(int dimension)
        {
            vec = new double[dimension];
            vec.Initialize();
        }
        /// <summary>
        /// 构造dimension维向量，其中任意分量均为initial_value
        /// </summary>
        /// <param name="dimension"></param>
        /// <param name="initial_value"></param>
        public VectorN(int dimension, double initial_value)
        {
            vec = new double[dimension];
            for (int i = 0; i < dimension; i++)
            {
                vec[i] = initial_value;
            }
        }
        /// <summary>
        /// 从双精度数组构造向量
        /// </summary>
        public VectorN(double[] xs)
        {
            vec = xs;
        }
        /// <summary>
        /// 获取向量的某个分量
        /// </summary>
        /// <param name="i">指定的维度</param>
        /// <returns>向量在第i维的分量</returns>
        public double this[int i]
        {
            get 
            {
                if (i < 0 && i >= Dimension) throw new Exception("索引超出了向量的维度范围");
                return vec[i]; 
            }
            set 
            {
                if (i < 0 && i >= Dimension) throw new Exception("索引超出了向量的维度范围");
                vec[i] = value; 
            }
        }
        /// <summary>
        /// 检查两个向量维数是否相同，若否则抛出异常
        /// </summary>
        /// <param name="a">向量1</param>
        /// <param name="b">向量2</param>
        private static void CheckDimension(VectorN a, VectorN b)
        {
            if (a.Dimension != b.Dimension) throw new Exception("两个输入矢量的长度不相等");
        }
        public static VectorN operator +(VectorN a, VectorN b)
        {
            CheckDimension(a, b);
            return new VectorN(a.vec.Add(b.vec));
        }
        public static VectorN operator -(VectorN a, VectorN b)
        {
            CheckDimension(a, b);
            return new VectorN(a.vec.Add(b.vec));
        }
        public static VectorN operator -(VectorN a)
        {
            VectorN r = new VectorN(a.Dimension);
            for (int i = 0; i < a.Dimension; i++)
            {
                r[i] = -a[i];
            }
            return r;
        }
        public static VectorN operator *(double a, VectorN b)
        {
            return new VectorN(b.vec.Map( x => a * x ));
        }
        public static VectorN operator *(VectorN a, double b)
        {
            return new VectorN(a.vec.Map(x => b * x));
        }
        /// <summary>
        /// 注意，此运算符返回的是向量内积，而非元素分别相乘的列表
        /// </summary>
        /// <param name="a">向量1</param>
        /// <param name="b">向量2</param>
        /// <returns>向量1与向量2的内积结果</returns>
        public static double operator *(VectorN a, VectorN b)
        {
            CheckDimension(a, b);
            double sum = 0;
            for (int i = 0; i < a.Dimension; i++)
            {
                sum += a[i] * b[i];
            }
            return sum;
        }
        public static VectorN operator /(double a, VectorN b)
        {
            return new VectorN(b.vec.Map(x => a * x));
        }
        public static VectorN operator /(VectorN a, double b)
        {
            return new VectorN(a.vec.Map( x => x / b));
        }
        /// <summary>
        /// 判断向量中是否存在无效值
        /// </summary>
        /// <returns></returns>
        public bool ContainsNaN()
        {
            foreach (double i in vec)
            {
                if (double.IsNaN(i)) return true;
            }
            return false;
        }
        /// <summary>
        /// 将向量转换成Mathematica风格的字符串
        /// </summary>
        public override string ToString()
        {
            string re = "{";
            for (int i = 0; i < Dimension; i++)
            {
                if (i != 0) re += ",";
                re += vec[i];
            }
            re += "}";
            return re;
        }
        /// <summary>
        /// 产生向量的复制体
        /// </summary>
        public VectorN Duplicate()
        {
            VectorN r = new VectorN(Dimension);
            for (int i = 0; i < Dimension; i++)
            {
                r[i] = this[i];
            }
            return r;
        }
        /// <summary>
        /// 交换向量中的第i维和第j维分量
        /// </summary>
        public void Exchange(int i, int j)
        {
            double tmp = this[i];
            this[i] = this[j];
            this[j] = tmp;
        }
    }
}
