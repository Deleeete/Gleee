using Gleee.Asm;
using System.IO;

namespace GleeeTest
{
    class Program
    {
        static void Main(string[] args)
        {
            //string a = "[eax+0xef]";
            //Console.WriteLine(Asm.PreProcessOperand(a));
            //Console.ReadKey();
            string[] asm = File.ReadAllLines("test.asm");
            byte[] test = Asm.AssembleAll(asm);
            File.WriteAllBytes("test.bin", test);
        }
    }
}
