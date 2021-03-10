using System;
using Backup.Data;
using Backup.Resources;

namespace Backup.Utils
{
    public class ExitUtil
    {
        /// <summary>
        /// Prints an exit message in an error color and waits for ENTER before closing the program so that
        /// the window does not close immediately.
        /// </summary>
        public static void ExitAfterError(BackupException e)
        {
            // print error messages and (if given) error details
            foreach (string msg in e.ErrorMessages)
            {
                ConsoleWriter.WriteErrorMessage(msg);
            }
            
            if (e.ErrorDetails != null)
            {
                foreach (string details in e.ErrorDetails)
                {
                    ConsoleWriter.WriteErrorDetails(details);
                }
            }
            
            // exit message with error color
            ConsoleWriter.WriteErrorMessage(Lang.EndProgram);
                    
            // wait for input until actual closing
            Console.ReadLine();
            Environment.Exit(0);
        }
        
        /// <summary>
        /// Prints an exit message in an non-error color and waits for ENTER before closing the program so that
        /// the window does not close immediately.
        /// </summary>
        public static void ExitWithoutError()
        {
            // exit message with non-error color
            ConsoleWriter.WriteMainMessage(Lang.EndProgram);
                    
            // wait for input until actual closing
            Console.ReadLine();
            Environment.Exit(0);
        }
    }
}