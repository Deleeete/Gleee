using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Xml;

namespace Gleee.Consoleee
{
    /// <summary>
    /// 广义、增强版的控制台类
    /// </summary>
    public class Consoleee
    {
        /// <summary>
        /// 广义的控制台操作。默认连接到System.Console类
        /// </summary>
        private Action<object> Write_Method { get; set; } = DefaultWrite;
        private Func<ConsoleKeyInfo> ReadKey_Method { get; set; } = DefaultReadKey;
        private Action<ConsoleColor> SetForeColor { get; set; } = DefaultSetForeColor;
        private Action<ConsoleColor> SetBackColor { get; set; } = DefaultSetBackColor;
        private Func<ConsoleColor> GetForeColor { get; set; } = DefaultGetForeColor;
        private Func<ConsoleColor> GetBackColor { get; set; } = DefaultGetBackColor;
        private Action<int, int> SetCursorPosition { get; set; } = DefaultSetCursorPosition;

        /// <summary>
        /// 获取或设置该控制台实例的换行符。默认为\n
        /// </summary>
        public string LnChar { get; set; } = "\n";
        public void Clear() => DefaultClear();
        public ConsoleKeyInfo ReadKey() => ReadKey_Method();
        public ConsoleKeyInfo ReadKey(object obj)
        {
            WriteLn(obj);
            return ReadKey_Method();
        }
        public void Write(object obj) => Write_Method(obj.ToString());
        public void Write(object obj, ConsoleColor c)
        {
            var tmp = GetForeColor();
            SetForeColor(c);
            Write(obj);
            SetForeColor(tmp);
        }
        public void WriteLn(object obj) => Write_Method(obj.ToString() + LnChar);
        public void WriteLn(object obj, ConsoleColor c) => Write(obj.ToString() + LnChar, c);
        public void WriteNewLn(object obj) => Write_Method(LnChar + obj.ToString() + LnChar);
        public void WriteNewLn(object obj, ConsoleColor c) => Write(LnChar + obj.ToString() + LnChar, c);
        public void WriteOperationLn(string description, Action operation)
        {
            WriteOperationLn(description, operation, "[完成]", ConsoleColor.Green);
        }
        public void WriteOperationLn(string description, Action operation, string finish_string)
        {
            WriteOperationLn(description, operation, finish_string, ConsoleColor.Green);
        }
        public void WriteOperationLn(string description, Action operation, string finish_string, ConsoleColor finish_color)
        {
            Write(description + "...");
            operation();
            WriteLn(finish_string, finish_color);
        }
        public bool WriteOperationLn(string description, Func<bool> operation)
        {
            return WriteOperationLn(description, operation, "[成功]", "[失败]", ConsoleColor.Green, ConsoleColor.Red);
        }
        public bool WriteOperationLn(string description, Func<bool> operation, string success_string, string failed_string)
        {
            return WriteOperationLn(description, operation, success_string, failed_string, ConsoleColor.Green, ConsoleColor.Red);
        }
        public bool WriteOperationLn(string description, Func<bool> operation, string success_string, string failed_string, ConsoleColor success_color, ConsoleColor failed_color)
        {
            Write(description + "...");
            bool result = operation();
            if (result) WriteLn(success_string, success_color);
            else WriteLn(failed_string, failed_color);
            return result;
        }
        public int WriteOperationLn(string description, Func<int> operation)
        {
            return WriteOperationLn(description, operation, "[成功]", "[失败]", ConsoleColor.Green, ConsoleColor.Red);
        }
        public int WriteOperationLn(string description, Func<int> operation, string success_string, string failed_string)
        {
            return WriteOperationLn(description, operation, success_string, failed_string, ConsoleColor.Green, ConsoleColor.Red);
        }
        public int WriteOperationLn(string description, Func<int> operation, string success_string, string failed_string, ConsoleColor success_color, ConsoleColor failed_color)
        {
            Write(description + "...");
            int result = operation();
            if (result == 0) WriteLn(success_string, success_color);
            else WriteLn(failed_string, failed_color);
            return result;
        }

        private static void DefaultClear() => Console.Clear();
        private static void DefaultWrite(object obj) => Console.Write(obj);
        private static ConsoleKeyInfo DefaultReadKey()
        {
            while (Console.KeyAvailable) Console.ReadKey(true);
            return Console.ReadKey(true);
        }
        private static void DefaultSetForeColor(ConsoleColor c) => Console.ForegroundColor = c;
        private static void DefaultSetBackColor(ConsoleColor c) => Console.BackgroundColor = c;
        private static ConsoleColor DefaultGetForeColor() => Console.ForegroundColor;
        private static ConsoleColor DefaultGetBackColor() => Console.BackgroundColor;
        private static void DefaultSetCursorPosition(int left, int top) => Console.SetCursorPosition(left, top);
    }

    public interface IConsole
    {
        void Write(object obj);
        char Read();
        string ReadLn();
        void ReadKey();
        ConsoleColor ForeColor { get; set; }
        ConsoleColor BackColor { get; set; }
        void SetCursorPos(int left, int top);

    }
}