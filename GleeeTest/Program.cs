using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Gleee.Cryptography;
using Gleee.Cryptography.Algorithms;
using Gleee.Utils.Extension;

namespace GleeeTest
{
    class Program
    {
        static void Main(string[] args)
        {
            byte[] plain = Encoding.ASCII.GetBytes("Never gonna give you up never gonna get you down");
            byte[] key = "abcd12344321dcbadeadbeef12345678".ToByteArray();
            TEA tea = new TEA();
            TEA tcc = new TEA(0xcccccccc);
            EncryptionMode cbc = new EncryptionMode(
                sizeof(uint) * 2,
                (p, i, k) => tea.Encryptor(p, i, k, 0),
                (c, i, k) => tea.Decryptor(c, i, k, 0)
                );
            EncryptionMode cbccc = new EncryptionMode(
                sizeof(uint) * 2,
                (p, i, k) => tcc.Encryptor(p, i, k, 0),
                (c, i, k) => tcc.Decryptor(c, i, k, 0)
                );
            var fc = cbc.EncryptCBC(out byte[] fciv, plain, key);
            var fcc = cbccc.EncryptCBC(out byte[] fcciv, plain, key);
            var ic = tea.EncryptCBC(out byte[] iciv, plain, key);
            var icc = tcc.EncryptCBC(out byte[] icciv, plain, key);
            Console.WriteLine($"函数式密文: {fc.ToHexString()}");
            Console.WriteLine($"iv: {fciv.ToHexString()}\n");
            Console.WriteLine($"函数式改C密文: {fcc.ToHexString()}");
            Console.WriteLine($"iv: {fcciv.ToHexString()}\n");
            Console.WriteLine($"接口密文: {ic.ToHexString()}");
            Console.WriteLine($"iv: {iciv.ToHexString()}\n");
            Console.WriteLine($"接口改C密文: {icc.ToHexString()}");
            Console.WriteLine($"iv: {icciv.ToHexString()}\n");
            var dfc = cbc.DecryptCBC(fciv, fc, key);
            var dfcc = cbccc.DecryptCBC(fcciv, fcc, key);
            var dic = tea.DecryptCBC(iciv, ic, key);
            var dicc = tcc.DecryptCBC(icciv, icc, key);
            Console.WriteLine($"函数式明文: {dfc.ToHexString()}");
            Console.WriteLine($"iv: {fciv.ToHexString()}\n");
            Console.WriteLine($"函数式改C明文: {dfcc.ToHexString()}");
            Console.WriteLine($"iv: {fcciv.ToHexString()}\n\n");
            Console.WriteLine($"接口明文: {dic.ToHexString()}");
            Console.WriteLine($"iv: {iciv.ToHexString()}\n");
            Console.WriteLine($"接口改C明文: {dicc.ToHexString()}");
            Console.WriteLine($"iv: {icciv.ToHexString()}\n");

            Console.WriteLine($"函数式_text: {Encoding.ASCII.GetString(dfc)}");
            Console.WriteLine($"函数式改C_text: {Encoding.ASCII.GetString(dfcc)}");
            Console.WriteLine($"接口_text: {Encoding.ASCII.GetString(dic)}");
            Console.WriteLine($"接口改C_text: {Encoding.ASCII.GetString(dicc)}");
            Console.ReadKey();
        }
    }
}
