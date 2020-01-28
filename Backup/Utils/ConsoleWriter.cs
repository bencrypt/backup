using System;
using System.Drawing;

namespace Backup.Utils
{
    public class ConsoleWriter
    {
        public static void WriteWithColor(string text, ConsoleColor color, params object[] args)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text, args);
            Console.ResetColor();
        }
    }
}