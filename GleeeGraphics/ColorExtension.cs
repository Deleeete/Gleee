using System.Drawing;

namespace Gleee.Graphics
{
    public static class ColorExtension
    {
        public static Color Add(this Color c, Color color)
        {
            int r = c.R + color.R;
            int g = c.G + color.G;
            int b = c.B + color.B;
            if (r > 255) r = 255;
            else if (r < 0) r = 0;
            if (g > 255) g = 255;
            else if (g < 0) g = 0;
            if (b > 255) b = 255;
            else if (b < 0) b = 0;
            return Color.FromArgb(r, g, b);
        }
        public static Color Sub(this Color c, Color color)
        {
            int r = c.R - color.R;
            int g = c.G - color.G;
            int b = c.B - color.B;
            if (r > 255) r = 255;
            else if (r < 0) r = 0;
            if (g > 255) g = 255;
            else if (g < 0) g = 0;
            if (b > 255) b = 255;
            else if (b < 0) b = 0;
            return Color.FromArgb(r, g, b);
        }
        public static Color Multipy(this Color c, Color color)
        {
            int r = c.R * color.R;
            int g = c.G * color.G;
            int b = c.B * color.B;
            if (r > 255) r = 255;
            else if (r < 0) r = 0;
            if (g > 255) g = 255;
            else if (g < 0) g = 0;
            if (b > 255) b = 255;
            else if (b < 0) b = 0;
            return Color.FromArgb(r, g, b);
        }
        public static Color Divide(this Color c, Color color)
        {
            int r = c.R / color.R;
            int g = c.G / color.G;
            int b = c.B / color.B;
            if (r > 255) r = 255;
            else if (r < 0) r = 0;
            if (g > 255) g = 255;
            else if (g < 0) g = 0;
            if (b > 255) b = 255;
            else if (b < 0) b = 0;
            return Color.FromArgb(r, g, b);
        }
    }
}
