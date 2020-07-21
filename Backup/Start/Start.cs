using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Backup.Resources;
using Backup.Utils;
using Backup.Xml;

namespace Backup.Start
{
    class Start
    {
        // ATTENTION: Use ConsoleWriter instead of Console.WriteLine to use Colors
        
        static void Main(string[] args)
        {
            // change to English for testing
            //Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en");

            // get path of backup profiles, seen from the .exe or .sh in "../backup_profiles"
            string scriptPath = Path.GetFullPath(Assembly.GetExecutingAssembly().Location);
            string profileDir = Path.GetFullPath(Path.Combine(scriptPath, "..", "..", "backup_profiles"));

            // endless loop to enable multiple runs without restarting the program
            // => the user is asked if he wants to continue after each run
            while (true)
            {
                // check if the backup profiles directory is existing
                // => if not so the program can not continue
                if (!Directory.Exists(profileDir))
                {
                    // error message
                    ConsoleWriter.WriteErrorMessage(Lang.ErrorNotExistingProfile, profileDir);
                        
                    // wait for input to avoid that the window closes immediately
                    ConsoleWriter.WriteErrorMessage(Lang.EndProgram);
                    Console.ReadLine();
                    
                    // exit
                    Environment.Exit(1);
                }
                
                // choose backup profile and run it if valid
                BackupProfile profile = SelectBackupProfile(profileDir);
                
                if (profile != null)
                {
                    RunBackup(profile);
                }
                
                // ask if the user wants another backup run
                // => if not so exit, else the endless loop will take another run
                ConsoleWriter.WriteMainMessage(Lang.AnotherRun);
                string input = Console.ReadLine();
                if (input == null || !input.ToLower().Equals("j"))
                {
                    break;
                }
            }
        }
        
        /// <summary>
        /// Runs the backup basing on the given profile. The profile should be validated since this method does
        /// rely on it.
        /// If an error occurs while doing the backup, the backup will stop right there and an error message
        /// will be printed.
        /// </summary>
        /// <param name="profile">a valid backup profile</param>
        private static void RunBackup(BackupProfile profile)
        {
            // info message
            ConsoleWriter.WriteMainMessage(Lang.StartBackup);
                    
            // do backup, might have errors (due to permissions etc.) that are
            // printed out directly when occuring and cause the backup process to stop at the error's point
            try
            {
                BackupRunner.RunBackup(profile);
            }
            catch (Exception e)
            {
                // error message
                ConsoleWriter.WriteErrorMessage(Lang.StopBecauseOfError);
                ConsoleWriter.WriteErrorMessage(Lang.ErrorMessage);
                ConsoleWriter.WriteErrorDetails("{0}", e.Message);

                // stack trace
                //ConsoleWriter.WriteErrorDetails(e.StackTrace, ConsoleColor.Yellow);
                        
                // wait for input to avoid that the window closes immediately
                ConsoleWriter.WriteMainMessage(Lang.EndProgram);
                Console.ReadLine();
                        
                // exit
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// Makes the user select interactively a backup profile from the profiles available in the given
        /// directory. Returns the parsed backup profile if it is valid, else null.
        /// Does or delegates all the checks needed for verifying that the selected profile is valid, like
        /// validating the file paths.
        /// </summary>
        /// <param name="profileDir">the directory with the profiles</param>
        /// <returns>a valid backup profile or null (when error)</returns>
        private static BackupProfile SelectBackupProfile(string profileDir)
        {
            // print starting message
            ConsoleWriter.WriteMainMessage(Lang.Start);
            
            // TODO: check if there are profiles to select

            // save all profile paths (xmls in profile directory) into a list
            IList<string> profilePaths = new List<string>();
            foreach (string profile in Directory.GetFiles(profileDir))
            {
                if (profile.EndsWith(".xml"))
                {
                    profilePaths.Add(profile);
                }
            }
            
            // make all profile paths selectable by the user through entering a corresponding number
            for (int option = 1; option <= profilePaths.Count; option++)
            {
                ConsoleWriter.WriteMainMessage("[{0}]: {1}", option, Path.GetFileNameWithoutExtension(profilePaths[option-1]));
            }

            // check input and load profile when it is valid
            // => repeat until there is valid input
            BackupProfileConverter profileConverter = new BackupProfileConverter();
            int input = -1;
            
            while (input == -1)
            {
                // take input
                bool parsed = int.TryParse(Console.ReadLine(), out input);
                
                // check input
                if (!parsed || input <= 0 || input > profilePaths.Count)
                {
                    // error message because of invalid input
                    ConsoleWriter.WriteErrorMessage(Lang.InvalidNumber);
                    
                    // reset input on -1 for taking another loop run
                    input = -1;
                }
                else
                {
                    // valid input, return backup profile
                    return profileConverter.LoadBackupProfile(profilePaths[input - 1]);
                }
            }

            // return null due to compiler checking, is never reached since the loop aboves
            // repeats until there is a valid input for continuing the program
            return null;
        }
        
    }
}