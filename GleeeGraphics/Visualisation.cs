using System;
using System.Drawing;

namespace Gleee.Graphics
{
    /// <summary>
    /// 基于DX2D的可视化工具类
    /// </summary>
    public static class Visualisation
    {
        /// <summary>
        /// 将x转换为彩虹条中的对应颜色
        /// </summary>
        /// <param name="x">自变量</param>
        /// <param name="x_min">自变量的最小值</param>
        /// <param name="x_max">自变量的最大值</param>
        /// <returns>自变量的彩虹颜色表示</returns>
        public static Color ToRainbowColor(double x, double x_min, double x_max)
        {
            double length = x_max - x_min;
            double slope = 1020 / length;   //255/length/4
            double offset(double n) => x_min + length * n / 4;
            int R = 0, G = 0, B = 0;
            if (x < x_min)
            {
                R = 0;
                G = 0;
                B = 255;
            }
            else if (x_min <= x && x < offset(1))
            {
                R = 0;
                G = Convert.ToInt32(slope * (x-x_min));
                B = 255;
            }
            else if (x < offset(2))
            {
                R = 0;
                G = 255;
                B = Convert.ToInt32(slope * (offset(2) - x));
            }
            else if (x < offset(3))
            {
                R = Convert.ToInt32(slope * (x - offset(2)));
                G = 255;
                B = Convert.ToInt32(slope * (x - offset(2)));
            }
            else if (x < x_max)
            {
                R = 255;
                G = Convert.ToInt32(slope * (x_max - x));
                B = 255;
            }
            else if (x > x_max)
            {
                R = 255;
                G = 0;
                B = 255;
            } 
            return Color.FromArgb(R, G, B);
        }
        /// <summary>
        /// 使用线性函数将数值转换为颜色
        /// </summary>
        /// <param name="x">要转换的数值</param>
        /// <param name="x_min">数值可能的最小值</param>
        /// <param name="x_max">数值可能的最大值</param>
        /// <param name="color">当数值为x_max时返回的颜色</param>
        /// <returns>与数值相对应的颜色</returns>
        public static Color ToColor(double x, double x_min, double x_max, Color color)
        {
            double sr = color.R / (x_max - x_min);
            double sg = color.G / (x_max - x_min);
            double sb = color.B / (x_max - x_min);
            int r = Convert.ToInt32((x - x_min) * sr);
            int g = Convert.ToInt32((x - x_min) * sg);
            int b = Convert.ToInt32((x - x_min) * sb);
            if (r > 255) r = 255;
            if (g > 255) g = 255;
            if (b > 255) b = 255;
            if (r < 0) r = 0;
            if (g < 0) g = 0;
            if (b < 0) b = 0;
            return Color.FromArgb(r, g, b);
        }
        /// <summary>
        /// 使用线性函数将数值转换为颜色
        /// </summary>
        /// <param name="x">要转换的数值</param>
        /// <param name="x_min">数值可能的最小值</param>
        /// <param name="x_max">数值可能的最大值</param>
        /// <param name="color_min">当数值为x_min时返回的颜色</param>
        /// <param name="color_max">当数值为x_max时返回的颜色</param>
        /// <returns>与数值相对应的颜色</returns>
        public static Color ToLinearColor(double x, double x_min, double x_max, Color color_min, Color color_max)
        {
            double sr = (color_max.R - color_min.R) / (x_max - x_min);
            double sg = (color_max.G - color_min.G) / (x_max - x_min);
            double sb = (color_max.B - color_min.B) / (x_max - x_min);
            int r = Convert.ToInt32((x - x_min) * sr);
            int g = Convert.ToInt32((x - x_min) * sg);
            int b = Convert.ToInt32((x - x_min) * sb);
            if (r > 255) r = 255;
            if (g > 255) g = 255;
            if (b > 255) b = 255;
            if (r < 0) r = 0;
            if (g < 0) g = 0;
            if (b < 0) b = 0;
            return Color.FromArgb(r, g, b);
        }
        /// <summary>
        /// 将数据列表用渐变色条可视化
        /// </summary>
        /// <param name="data">需要可视化的数据</param>
        /// <param name="bar_length">条带的总宽度</param>
        /// <param name="y">条带的y坐标</param>
        /// <param name="height">条带的高</param>
        /// <param name="data_min">数据中的最小可能值</param>
        /// <param name="data_max">数据中的最大可能值</param>
        /// <param name="color_min">数据最小时对应的颜色</param>
        /// <param name="color_max">数据最大时对应的颜色</param>
        public static void ArrayLinearPlot(double[] data, float bar_length, float y, float height, double data_min, double data_max, Color color_min, Color color_max)
        {
            ArrayCustomPlot(data, bar_length, y, height, data_min, data_max, 
                (x, x_min, x_max) => ToLinearColor(x, x_min, x_max, color_min, color_max));
        }
        /// <summary>
        /// 将数据列表用渐变灰度条可视化,其中最小值对应纯黑色，最大值对应纯白色
        /// </summary>
        /// <param name="data">需要可视化的数据</param>
        /// <param name="bar_length">条带的总宽度</param>
        /// <param name="y">条带的y坐标</param>
        /// <param name="height">条带的高</param>
        /// <param name="data_min">数据中的最小可能值</param>
        /// <param name="data_max">数据中的最大可能值</param>
        public static void ArrayLinearPlot(double[] data, float bar_length, float y, float height, double data_min, double data_max)
        {
            ArrayLinearPlot(data, bar_length, y, height, data_min, data_max, Color.Black, Color.White);
        }
        /// <summary>
        /// 将数据列表用彩虹色条可视化
        /// </summary>
        /// <param name="data">需要可视化的数据</param>
        /// <param name="bar_length">条带的总宽度</param>
        /// <param name="y">条带的y坐标</param>
        /// <param name="height">条带的高</param>
        /// <param name="data_min">数据中的最小可能值</param>
        /// <param name="data_max">数据中的最大可能值</param>
        public static void ArrayRainbowPlot(double[] data, float bar_length, float y, float height, double data_min, double data_max)
        {
            ArrayCustomPlot(data, bar_length, y, height, data_min, data_max, ToRainbowColor);
        }
        /// <summary>
        /// 将数据列表用自定义函数可视化，自定义函数的参数分别为(x, x_min, x_max)，返回值为对应的颜色
        /// </summary>
        /// <param name="data">需要可视化的数据</param>
        /// <param name="bar_length">条带的总宽度</param>
        /// <param name="y">条带的y坐标</param>
        /// <param name="height">条带的高</param>
        /// <param name="data_min">数据中的最小可能值</param>
        /// <param name="data_max">数据中的最大可能值</param>
        /// <param name="func">自定义函数</param>
        public static void ArrayCustomPlot(double[] data, float bar_length, float y, float height, double data_min, double data_max, Func<double, double, double, Color> func)
        {
            DX2D.Render = () =>
            {
                float pix_per_data = bar_length / data.Length;
                if (true)
                {
                    int block_width = Convert.ToInt32(pix_per_data);
                    for (int n = 0; n < data.Length; n++)
                    {
                        DX2D.Shape.ImportRect(n * block_width, y, block_width, height, func(data[n], data_min, data_max));
                    }
                }
                else
                {
                    for (int n = 0; n < data.Length; n++)
                    {
                        DX2D.Shape.ImportRect(pix_per_data * n, y, pix_per_data, height, func(data[n], data_min, data_max));
                    }
                }
            };
            DX2D.Present();
        }
        /// <summary>
        /// 将数据列表用自定义函数可视化，自定义函数的参数分别为(x, x_min, x_max)，返回值为对应的颜色
        /// </summary>
        /// <param name="data">需要可视化的数据</param>
        /// <param name="offset">需要可视化的数据起始点</param>
        /// <param name="data_count">需要可视化的数据数量</param>
        /// <param name="bar_length">条带的总宽度</param>
        /// <param name="y">条带的y坐标</param>
        /// <param name="height">条带的高</param>
        /// <param name="data_min">数据中的最小可能值</param>
        /// <param name="data_max">数据中的最大可能值</param>
        /// <param name="func">自定义函数</param>
        public static void ArrayCustomPlot(double[] data, int offset, int data_count, float bar_length, float y, float height, double data_min, double data_max, Func<double, double, double, Color> func)
        {
            DX2D.Render = () =>
            {
                float pix_per_data = bar_length / data_count;
                if (pix_per_data > 1)   //每个数据点占据了超过1个像素
                {
                    int block_width = Convert.ToInt32(pix_per_data+1);  //上取整保证中间不漏像素
                    for (int n = offset; n < data_count; n++)
                    {
                        DX2D.Shape.ImportRect(n * block_width, y, block_width, height, func(data[n], data_min, data_max));
                    }
                }
                else
                {
                    for (int n = offset; n < data_count; n++)
                    {
                        DX2D.Shape.ImportRect(pix_per_data * n, y, pix_per_data, height, func(data[n], data_min, data_max));
                    }
                }
            };
            DX2D.Present();
        }
        /// <summary>
        /// 绘制一条给定颜色的实线
        /// </summary>
        /// <param name="x1">端点1的x坐标</param>
        /// <param name="y1">端点1的y坐标</param>
        /// <param name="x2">端点2的x坐标</param>
        /// <param name="y2">端点2的y坐标</param>
        /// <param name="color">实线的颜色</param>
        public static void DrawLine(float x1, float y1, float x2, float y2, Color color)
        {
            DX2D.Render = () => DX2D.Shape.ImportLine(x1, y1, x2, y2, color);
            DX2D.Present();
        }
        /// <summary>
        /// 绘制一块给定颜色的空心矩形
        /// </summary>
        /// <param name="x">左上角的x坐标</param>
        /// <param name="y">左上角的y坐标</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="color">颜色</param>
        public static void DrawEmptyRect(float x, float y, float width, float height, Color color)
        {
            DX2D.Render = () => DX2D.Shape.ImportEmptyRect(x, y, width, height, color);
            DX2D.Present();
        }
        /// <summary>
        /// 绘制一块给定颜色的矩形
        /// </summary>
        /// <param name="x">左上角的x坐标</param>
        /// <param name="y">左上角的y坐标</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="color">颜色</param>
        public static void DrawRect(float x, float y, float width, float height, Color color)
        {
            DX2D.Render = () => DX2D.Shape.ImportRect(x, y, width, height, color);
            DX2D.Present();
        }
    }
}
