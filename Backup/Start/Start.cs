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
        
        /// <summary>
        /// Starts the backup program initiating all required actions like profile choosing, parsing and running
        /// the backup.
        /// </summary>
        /// <param name="args">unused command line arguments</param>
        static void Main(string[] args)
        {
            // change to English for testing
            //Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en");

            // get path of backup profiles, seen from the .exe or .sh in "../backup_profiles"
            string scriptPath = Path.GetFullPath(Assembly.GetExecutingAssembly().Location);
            string profileDir = Path.GetFullPath(Path.Combine(scriptPath, "..", "..", "backup_profiles"));

            // check if the backup profiles directory is existing
            // => if not so, the program can not continue
            if (!Directory.Exists(profileDir))
            {
                // error message and exit
                ConsoleWriter.WriteErrorMessage(Lang.ErrorNotExistingProfileDir, profileDir);
                ExitAfterError();
            }
            
            // choose backup profile, might be null when no one is existing or if an invalid
            // profile is chosen
            BackupProfile profile = SelectBackupProfile(profileDir);

            // run backup if valid backup file, else prepare closing the program
            if (profile == null)
            {
                // invalid or not existing profile,
                // show information for closing the program in error color
                ExitAfterError();
                
            } else {
                
                // valid profile, do backup
                // => might exit with error when an error occurs while doing the backup
                RunBackup(profile);

                // if reached here, the backup made fully without errors
                ExitWithoutError();
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
            
            // additional info message if a dry run is executed
            if (profile.DryRun)
            {
                ConsoleWriter.WriteMainMessage(Lang.DoDryRun);
            }
                    
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
                ConsoleWriter.WriteErrorDetails(e.StackTrace, ConsoleColor.Yellow);
                        
                // finish program
                ExitAfterError();
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
            ConsoleWriter.WriteMainMessage(Lang.DryRunInfo);

            // save all profile paths (xmls in profile directory) into a list
            IList<string> profilePaths = new List<string>();
            foreach (string profile in Directory.GetFiles(profileDir))
            {
                if (profile.EndsWith(".xml"))
                {
                    profilePaths.Add(profile);
                }
            }
            
            // check if there are profiles to select
            // => if not so inform the user and return null
            if (profilePaths.Count == 0)
            {
                ConsoleWriter.WriteErrorMessage(Lang.NoProfileAtPath, profileDir);
                ConsoleWriter.WriteErrorDetails(Lang.NoProfileAtPathHelp);
                return null;
            }
            
            // make all profile paths selectable by the user through entering a corresponding number
            for (int option = 1; option <= profilePaths.Count; option++)
            {
                ConsoleWriter.WriteMainMessage("[{0}]: {1}", option, Path.GetFileNameWithoutExtension(profilePaths[option-1]));
            }
            
            // add option to close the program without running a backup
            ConsoleWriter.WriteMainMessage("[0]: {0}", Lang.OptionCancel);
            

            // check input and load profile when it is valid
            // => repeat until there is valid input
            BackupProfileConverter profileConverter = new BackupProfileConverter();
            
            while (true)
            {
                // get input
                string input = Console.ReadLine();

                // check if a dry run should make (changes will be shown but not actually be made);
                // afterwards remove dry-flag so that the rest of the string can be parsed to a backup profile
                bool dryRun = false;
                if (input != null && input.Contains(" --dry"))
                {
                    dryRun = true;
                    input = input.Remove(input.IndexOf(" --dry", StringComparison.CurrentCulture));
                }
                
                // get profile number if provided (else the returned value will be false)
                bool parsed = int.TryParse(input, out int selectedProfile);
                
                // handle exit input
                if (parsed && selectedProfile == 0)
                {
                    // repeat selection as information
                    ConsoleWriter.WriteMainMessage(Lang.SelectExit);
                    
                    // close
                    ExitWithoutError();
                }
                
                // check non-exit input for valid selection of a backup profile
                if (!parsed || selectedProfile <= 0 || selectedProfile > profilePaths.Count)
                {
                    // error message because of invalid input
                    ConsoleWriter.WriteErrorMessage(Lang.InvalidNumber);
                }
                else
                {
                    // valid input, return backup profile
                    return profileConverter.LoadBackupProfile(profilePaths[selectedProfile - 1], dryRun);
                }
            }

            // return null due to compiler checking, is never reached since the loop above
            // repeats until there is a valid input for continuing the program
            return null;
        }

        /// <summary>
        /// Prints an exit message in an error color and waits for ENTER before closing the program so that
        /// the window does not close immediately.
        /// </summary>
        private static void ExitAfterError()
        {
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
        private static void ExitWithoutError()
        {
            // exit message with non-error color
            ConsoleWriter.WriteMainMessage(Lang.EndProgram);
                    
            // wait for input until actual closing
            Console.ReadLine();
            Environment.Exit(0);
        }

    }
}