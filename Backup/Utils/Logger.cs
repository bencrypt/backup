using System.Diagnostics;

namespace Backup.Utils
{
    class Logger
    {
        [Conditional("DEBUG")]
        public static void LogInfo(string text, params object[] args)
        {
            ConsoleWriter.WriteLogMessage("---");
            ConsoleWriter.WriteLogMessage("LogInfo:");
            ConsoleWriter.WriteLogMessage(text, args);
            ConsoleWriter.WriteLogMessage("---");
        }
    }
}
