using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Gleee.Cryptography;
using Gleee.Utils.Extension;
using Gleee.Cryptography.EncrptionModes;

namespace GleeeTest
{
    class Program
    {
        static void Main(string[] args)
        {
            byte[] plain = Encoding.ASCII.GetBytes("Never gonna give you up never gonna get you down");
            byte[] key = "abcd12344321dcbadeadbeef12345678".ToByteArray();
            TEA tea = new TEA();
            TEA teacc = new TEA(0xcccccccc);
            CBC cbc = new CBC(
                sizeof(uint) * 2,
                (p, i, k) => tea.Encryptor(p, i, k, 0),
                (c, i, k) => tea.Decryptor(c, i, k, 0)
                ) ;
            CBC cbccc = new CBC(
                sizeof(uint) * 2,
                (p, i, k) => teacc.Encryptor(p, i, k, 0),
                (c, i, k) => teacc.Decryptor(c, i, k, 0)
                );
            var cipher = cbc.Encrypt(out byte[] iv, plain, key);
            var ciphercc = cbccc.Encrypt(out byte[] ivcc, plain, key);
            Console.WriteLine($"CBC: {cipher.ToHexString()}");
            Console.WriteLine($"iv: {iv.ToHexString()}\n");
            Console.WriteLine($"CCC: {ciphercc.ToHexString()}");
            Console.WriteLine($"iv: {ivcc.ToHexString()}\n");
            var dc = cbc.Decrypt(out iv, cipher, key);
            var dccc = cbccc.Decrypt(out ivcc, ciphercc, key);
            Console.WriteLine($"DCBC: {dc.ToHexString()}");
            Console.WriteLine($"iv: {iv.ToHexString()}\n");
            Console.WriteLine($"DCCC: {dccc.ToHexString()}");
            Console.WriteLine($"iv: {ivcc.ToHexString()}\n\n");
            Console.WriteLine($"dcbc_text: {Encoding.ASCII.GetString(dc)}");
            Console.WriteLine($"dccc_text: {Encoding.ASCII.GetString(dccc)}");
            Console.ReadKey();
        }
    }
}
