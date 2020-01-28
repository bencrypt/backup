using System;
using System.Diagnostics;

namespace Backup.Utils
{
    class Logger
    {
        [Conditional("DEBUG")]
        public static void LogInfo(string text)
        {
            ConsoleWriter.WriteWithColor("---\nLogInfo:", ConsoleColor.Yellow);
            ConsoleWriter.WriteWithColor(text, ConsoleColor.Yellow);
        }

        [Conditional("DEBUG")]
        public static void LogInfo(string text, params object[] args)
        {
            ConsoleWriter.WriteWithColor("---\nLogInfo:", ConsoleColor.Yellow);
            ConsoleWriter.WriteWithColor(text, ConsoleColor.Yellow, args);
            ConsoleWriter.WriteWithColor("---", ConsoleColor.Yellow);
        }
    }
}
