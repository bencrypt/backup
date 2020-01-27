using System;
using System.Diagnostics;

namespace Backup.Utils
{
    class Logger
    {
        [Conditional("DEBUG")]
        public static void LogInfo(string text)
        {
            Console.WriteLine("---\nLogInfo:");
            Console.WriteLine(text);
        }

        [Conditional("DEBUG")]
        public static void LogInfo(string text, params object[] args)
        {
            Console.WriteLine("---\nLogInfo:");
            Console.WriteLine(text, args);
            Console.WriteLine("---");
        }
    }
}
