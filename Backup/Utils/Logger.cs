using System.Diagnostics;

namespace Backup.Utils
{
    class Logger
    {
        [Conditional("DEBUG")]    // only runs in backup mode
        public static void LogInfo(string text, params object[] args)
        {
            ConsoleWriter.WriteLogMessage("---");
            ConsoleWriter.WriteLogMessage("LogInfo:");
            ConsoleWriter.WriteLogMessage(text, args);
            ConsoleWriter.WriteLogMessage("---");
        }
    }
}
