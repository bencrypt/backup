using System;
using System.Diagnostics;

namespace Backup.Utils
{
    class Logger
    {
        [Conditional("DEBUG")]
        public static void LogInfo(string text)
        {
            ConsoleWriter.WriteLineWithColor("---", OutputColors.LogInfo);
            ConsoleWriter.WriteLineWithColor("LogInfo:", OutputColors.LogInfo);
            ConsoleWriter.WriteLineWithColor(text, OutputColors.LogInfo);
        }

        [Conditional("DEBUG")]
        public static void LogInfo(string text, params object[] args)
        {
            ConsoleWriter.WriteLineWithColor("---", OutputColors.LogInfo);
            ConsoleWriter.WriteLineWithColor("LogInfo:", OutputColors.LogInfo);
            ConsoleWriter.WriteLineWithColor(text, OutputColors.LogInfo, args);
            ConsoleWriter.WriteLineWithColor("---", OutputColors.LogInfo);
        }
    }
}
