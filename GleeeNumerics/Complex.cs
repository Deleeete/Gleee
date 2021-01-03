using System;

namespace Gleee.Numerics
{
    public struct Complex
    {
        public double Re { get; set; }
        public double Im { get; set; }
        public double Arg { get => Math.Atan2(Im, Re); }
        public double Norm { get => Math.Sqrt(Re * Re + Im * Im); }

        public Complex(double re)
        {
            Re = re;
            Im = 0;
        }
        public Complex(double re, double im)
        {
            Re = re;
            Im = im;
        }
        public override string ToString()
        {
            if (Im == 0) return Re.ToString();
            else if (Im > 0) return $"{Re}+{Im}i";
            else return $"{Re}{Im}i";
        }
        public string ToString(int n)
        {
            double re = Math.Round(Re, n);
            double im = Math.Round(Im, n);
            if (im == 0) return re.ToString();
            else if (im > 0) return $"{re}+{im}i";
            else return $"{re}{im}i";
        }
        public static Complex i = new Complex(0, 1);
        public static Complex operator ~(Complex a) => new Complex(a.Re, -a.Im);
        public static Complex operator +(double a, Complex b) => new Complex(a + b.Re, b.Im);
        public static Complex operator +(Complex a, double b) => new Complex(a.Re + b, a.Im);
        public static Complex operator +(Complex a, Complex b) => new Complex(a.Re + b.Re, a.Im + b.Im);
        public static Complex operator -(Complex a) => new Complex(-a.Re, -a.Im);
        public static Complex operator -(double a, Complex b) => new Complex(a - b.Re, b.Im);
        public static Complex operator -(Complex a, double b) => new Complex(a.Re - b, a.Im);
        public static Complex operator -(Complex a, Complex b) => new Complex(a.Re - b.Re, a.Im - b.Im);
        public static Complex operator *(double a, Complex b) => new Complex(a * b.Re, a * b.Im);
        public static Complex operator *(Complex a, double b) => new Complex(a.Re * b, a.Im * b);
        public static Complex operator *(Complex a, Complex b)
        {
            double re = (a.Re * b.Re - a.Im * b.Im);
            double im = (a.Im * b.Re + a.Re * b.Im);
            return new Complex(re, im);
        }
        public static Complex operator /(double a, Complex b) => new Complex(a / b.Re, a / b.Im);
        public static Complex operator /(Complex a, double b) => new Complex(a.Re / b, a.Im / b);
        public static Complex operator /(Complex a, Complex b)
        {
            double re = (a.Re * b.Re + a.Im * b.Im) / (b.Re * b.Re + b.Im * b.Im);
            double im = (a.Im * b.Re - a.Re * b.Im) / (b.Re * b.Re + b.Im * b.Im);
            return new Complex(re, im);
        }
        public static Complex Exp(Complex x)
        {
            double cof = Math.Exp(x.Re);
            double re = Math.Cos(x.Im);
            double im = Math.Sin(x.Im);
            return cof * new Complex(re, im);
        }
        public static Complex[] FromDoubleArray(double[] d)
        {
            Complex[] c = new Complex[d.Length];
            for (int i = 0; i < d.Length; i++)
            {
                c[i] = new Complex(d[i], 0);
            }
            return c;
        }
    }
    public static class ComlexArrayExtension
    {
        public static string ToMmaString(this Complex[] c)
        {
            string re = "{";
            for (int i = 0; i < c.Length; i++)
            {
                if (i != 0) re += ",";
                re += c[i];
            }
            re += "}";
            return re;
        }
    }
}
