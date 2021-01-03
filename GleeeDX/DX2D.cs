using System;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX;
using Device = Microsoft.DirectX.Direct3D.Device;
using DeviceType = Microsoft.DirectX.Direct3D.DeviceType;
using System.Diagnostics;
using System.Drawing;

namespace Gleee
{
    /// <summary>
    /// DX2D基本类
    /// </summary>
    public static class DX2D
    {
        /// <summary>
        /// 目标DirectX2D设备
        /// </summary>
        public static Device Device { get; private set; }
        /// <summary>
        /// 指示设备是否已初始化
        /// </summary>
        public static bool IsInitialized { get; private set; } = false;
        /// <summary>
        /// 此处存放所有渲染任务
        /// </summary>
        public static Action Render { get; set; }
        /// <summary>
        /// 初始化DirectX设备
        /// </summary>
        /// <param name="handle">目标viewport的句柄</param>
        /// <returns>初始化是否顺利完成</returns>
        public static bool InitializeDX2D(IntPtr handle)
        {
            try
            {
                PresentParameters presentParams = new PresentParameters
                {
                    Windowed = true,
                    SwapEffect = SwapEffect.Discard
                };
                if (Device != null) Device.Dispose();
                Device = new Device(0, DeviceType.Hardware, handle, CreateFlags.SoftwareVertexProcessing, presentParams);
                IsInitialized = true;
#if DEBUG
                Debug.Print("DXDevice init success.");
#endif
                return true;
            }
            catch (DirectXException ex)
            {
                Console.WriteLine($"Initailization Failed：{ex.Message}");
                return false;
            }
        }
        /// <summary>
        /// 用单一颜色覆盖整个viewport
        /// </summary>
        /// <param name="color">指定的颜色</param>
        public static void ClearDevice(Color color)
        {
            Device.BeginScene();
            Device.Clear(ClearFlags.Target, color, 1f, 0);
            Device.EndScene();
            Device.Present();
        }
        /// <summary>
        /// 将在Render中准备好的内容绘制到viewport。旧的内容不会被自动清理
        /// </summary>
        public static void Present()
        {
            Device.BeginScene();
            Render.Invoke();
            Device.EndScene();
            Device.Present();
        }
        /// <summary>
        /// 绘制在Render中准备好的内容
        /// </summary>
        /// <param name="auto_clear">绘制之前是否用黑色清理整个viewport</param>
        public static void Present(bool auto_clear)
        {
            Device.BeginScene();
            if (auto_clear) Device.Clear(ClearFlags.Target, Color.Black, 1f, 0);
            Render.Invoke();
            Device.EndScene();
            Device.Present();
        }
        /// <summary>
        /// 使用指定颜色清理viewport，然后绘制在Render中准备好的内容
        /// </summary>
        public static void Present(Color color)
        {
            Device.BeginScene();
            Device.Clear(ClearFlags.Target, color, 1f, 0);
            Render.Invoke();
            Device.EndScene();
            Device.Present();
        }
        /// <summary>
        /// 绘制简单几何图形的类
        /// </summary>
        public static class Shape
        {
            /// <summary>
            /// 在场景中加入点
            /// </summary>
            /// <param name="x">点的x坐标</param>
            /// <param name="y">点的y坐标</param>
            /// <param name="color">点的颜色</param>
            public static void ImportPoint(float x, float y, Color color)
            {
                CustomVertex.TransformedColored[] vertices = new CustomVertex.TransformedColored[1];
                Vector4 p = new Vector4(x, y, 0, 1);
                vertices[0].Position = p;
                vertices[0].Color = color.ToArgb();
                Device.VertexFormat = CustomVertex.TransformedColored.Format;
                Device.DrawUserPrimitives(PrimitiveType.PointList, 1, vertices);
            }
            /// <summary>
            /// 在场景中加入直线
            /// </summary>
            /// <param name="x1">端点1的x坐标</param>
            /// <param name="y1">端点1的y坐标</param>
            /// <param name="x2">端点2的x坐标></param>
            /// <param name="y2">端点2的y坐标</param>
            /// <param name="color">直线的颜色</param>
            public static void ImportLine(float x1, float y1, float x2, float y2, Color color)
            {
                CustomVertex.TransformedColored[] vertices = new CustomVertex.TransformedColored[2];
                Vector4 a = new Vector4(x1, y1, 0, 1);
                Vector4 b = new Vector4(x2, y2, 0, 1);
                vertices[0].Position = a;
                vertices[0].Color = color.ToArgb();
                vertices[1].Position = b;
                vertices[1].Color = color.ToArgb();
                Device.VertexFormat = CustomVertex.TransformedColored.Format;
                Device.DrawUserPrimitives(PrimitiveType.LineList, 1, vertices);
            }
            /// <summary>
            /// 在场景中加入渐变直线
            /// </summary>
            /// <param name="x1">端点1的x坐标</param>
            /// <param name="y1">端点1的y坐标</param>
            /// <param name="x2">端点2的x坐标</param>
            /// <param name="y2">端点2的y坐标</param>
            /// <param name="color1">端点1的颜色</param>
            /// <param name="color2">端点2的颜色</param>
            public static void ImportLine(float x1, float y1, float x2, float y2, Color color1, Color color2)
            {
                CustomVertex.TransformedColored[] vertices = new CustomVertex.TransformedColored[1];
                Vector4 a = new Vector4(x1, y1, 0, 1);
                Vector4 b = new Vector4(x2, y2, 0, 1);
                vertices[0].Position = a;
                vertices[0].Color = color1.ToArgb();
                vertices[1].Position = b;
                vertices[1].Color = color2.ToArgb();
                Device.VertexFormat = CustomVertex.TransformedColored.Format;
                Device.DrawUserPrimitives(PrimitiveType.LineList, 1, vertices);
            }
            /// <summary>
            /// 在场景中加入空心矩形
            /// </summary>
            /// <param name="x">左上顶点的x坐标</param>
            /// <param name="y">左上顶点的y坐标</param>
            /// <param name="width">矩形的横向长度（的绝对值）</param>
            /// <param name="height">矩形的纵向长度（的绝对值）</param>
            /// <param name="color">矩形的颜色</param>
            public static void ImportEmptyRect(float x, float y, float width, float height, Color color)
            {
                CustomVertex.TransformedColored[] vertices = new CustomVertex.TransformedColored[8];
                Vector4 a = new Vector4(x, y, 0, 1);
                Vector4 b = new Vector4(x + Math.Abs(width), y, 0, 1);
                Vector4 c = new Vector4(x + Math.Abs(width), y + Math.Abs(height), 0, 1);
                Vector4 d = new Vector4(x, y + Math.Abs(height), 0, 1);
                vertices[0].Position = a;
                vertices[0].Color = color.ToArgb();
                vertices[1].Position = b;
                vertices[1].Color = color.ToArgb();

                vertices[2] = vertices[1];
                vertices[3].Position = c;
                vertices[3].Color = color.ToArgb();

                vertices[4] = vertices[3];
                vertices[5].Position = d;
                vertices[5].Color = color.ToArgb();

                vertices[6] = vertices[5];
                vertices[6].Color = color.ToArgb();
                vertices[7] = vertices[0];
                Device.VertexFormat = CustomVertex.TransformedColored.Format;
                Device.DrawUserPrimitives(PrimitiveType.LineList, 4, vertices);
            }
            /// <summary>
            /// 在场景中加入实心矩形
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <param name="width"></param>
            /// <param name="height"></param>
            /// <param name="color"></param>
            public static void ImportRect(float x, float y, float width, float height, Color color)
            {
                CustomVertex.TransformedColored[] vertices = new CustomVertex.TransformedColored[4];
                Vector4 a = new Vector4(x, y, 0, 1);
                Vector4 b = new Vector4(x + Math.Abs(width), y, 0, 1);
                Vector4 c = new Vector4(x + Math.Abs(width), y + Math.Abs(height), 0, 1);
                Vector4 d = new Vector4(x, y + Math.Abs(height), 0, 1);
                vertices[0].Position = a;
                vertices[1].Position = b;
                vertices[2].Position = c;
                vertices[3].Position = d;
                vertices[0].Color = color.ToArgb();
                vertices[1].Color = color.ToArgb();
                vertices[2].Color = color.ToArgb();
                vertices[3].Color = color.ToArgb();
                Device.VertexFormat = CustomVertex.TransformedColored.Format;
                Device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 2, vertices);
            }
        }
    }
}
