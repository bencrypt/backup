using System.Diagnostics;

namespace Backup.Utils
{
    class Logger
    {
        /// <summary>
        /// Writes a log info message onto the console. Only runs in debug mode.
        /// </summary>
        /// <param name="text">The text to print</param>
        /// <param name="args">(optional) arguments for the text</param>
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
