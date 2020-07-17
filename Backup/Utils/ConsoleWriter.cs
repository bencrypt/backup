using System;
using System.Drawing;

namespace Backup.Utils
{
    public class ConsoleWriter
    {
        public static void WriteLineWithColor(string text, ConsoleColor color, params object[] args)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text, args);
            Console.ResetColor();
        }

        public static void WriteWithColor(string text, ConsoleColor color, params object[] args)
        {
            Console.ForegroundColor = color;
            Console.Write(text, args);
            Console.ResetColor();
        }

        public static void WriteBackupAddition(string pathWithoutLocationPrefix)
        {
            WriteWithColor("  + ", OutputColors.Add);
            WriteLineWithColor("'{0}'", OutputColors.Changed, pathWithoutLocationPrefix);
        }
        
        public static void WriteBackupUpdate(string pathWithoutLocationPrefix)
        {
            WriteWithColor("  * ", OutputColors.Update);
            WriteLineWithColor("'{0}'", OutputColors.Changed, pathWithoutLocationPrefix);
        }
        
        public static void WriteBackupRemove(string pathWithoutLocationPrefix)
        {
            WriteWithColor("  \u2013 ", OutputColors.Remove);
            WriteLineWithColor("'{0}'", OutputColors.Changed, pathWithoutLocationPrefix);
        }
    }
}