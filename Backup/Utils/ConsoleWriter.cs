using System;

namespace Backup.Utils
{
    public class ConsoleWriter
    {
        private static void WriteLineWithColor(string text, ConsoleColor color, params object[] args)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text, args);
            Console.ResetColor();
        }

        private static void WriteWithColor(string text, ConsoleColor color, params object[] args)
        {
            Console.ForegroundColor = color;
            Console.Write(text, args);
            Console.ResetColor();
        }
        
        /*
         * ERROR MESSAGES
         */

        public static void WriteErrorMessage(string text, params object[] args)
        {
            WriteLineWithColor(text, OutputColors.Error, args);
        }
        
        public static void WriteErrorDetails(string text, params object[] args)
        {
            WriteLineWithColor(text, OutputColors.ErrorDetails, args);
        }
        
        /*
         * SUCCESS MESSAGES
         */
        
        public static void WriteSuccessMessage(string text, params object[] args)
        {
            WriteLineWithColor(text, OutputColors.Success, args);
        }
        
        /*
         * BACKUP INFORMATION MESSAGE
         */
        
        public static void WriteBackupLocationHeadline(string text, params object[] args)
        {
            WriteLineWithColor(text, OutputColors.BackupLocations, args);
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
        
        /*
         * OTHER MESSAGES
         */

        public static void WriteMainMessage(string text, params object[] args)
        {
            WriteLineWithColor(text, OutputColors.MainMessages, args);
        }
        
        public static void WriteLogMessage(string text, params object[] args)
        {
            WriteLineWithColor(text, OutputColors.LogInfo, args);
        }
    }
}