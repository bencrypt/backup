using System;
using Backup.Data;
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

            try
            {
                // init profile selector (might throw BackupException if "../backup_profiles" does not exist)
                BackupProfileSelector profileSelector = new BackupProfileSelector();
                profileSelector.InitProfilePaths();

                // create necessary instances
                BackupProfileConverter profileConverter = new BackupProfileConverter();
                ExcludeUtil excludeUtil = new ExcludeUtil();

                // execute main loop (might forward BackupException if the backup cannot be processed)
                Start starter = new Start();
                starter.DoMainLoop(profileSelector, profileConverter, excludeUtil);
            }
            catch (BackupException e)
            {
                // all BackupExceptions are forwarded here to provide a single exit point when an error occurs
                ExitUtil.ExitAfterError(e);
            }

            // backup was successful or regularly cancelled by user, so exit the application in the default way
            ExitUtil.ExitWithoutError();
        }
        
        /// <summary>
        /// The main loop executes the following steps:<br />
        /// 1. Print start message, explain usage of application<br />
        /// 2. Let the user select a profile<br />
        /// 3. Do a backup according to the selected profile<br />
        /// 4. Ask if another backup run should be started<br />
        /// 5. Exit or repeat from step 1<br />
        /// <br />
        /// If an error occurs while executing the main loop, a BackupException will be thrown by the sub methods
        /// and is forwarded to the Main method for unifying the exit points of the application.
        /// </summary>
        /// <param name="profileSelector">an instance for selecting a backup profile</param>
        /// <param name="profileConverter">an instance for converting a backup profile from a xml file to an internal representation</param>
        /// <param name="excludeUtil">an instance for checking paths which should be excluded by the backup</param>
        private void DoMainLoop(BackupProfileSelector profileSelector, BackupProfileConverter profileConverter, ExcludeUtil excludeUtil) {

            // endless loop to enable multiple backup runs without restarting the program
            // => only stops if an error occurs or if the user does not want to do another run
            bool doAnotherRun = true;

            while (doAnotherRun)
            {
                // starting message to explain the usage of the application
                ConsoleWriter.WriteApplicationTitle();
                PrintStartingMessage();

                // let the user select a backup profile or cancel the application
                string profilePath = profileSelector.SelectBackupProfile();
                if (profilePath == null)
                {
                    break;
                }
                
                // load selected profile (might throw BackupException if invalid)
                BackupProfile profile = profileConverter.LoadBackupProfile(profilePath, profileSelector.IsDryRunSelected);
                
                // valid profile, do backup (might throw BackupException)
                BackupRunner backupRunner = new BackupRunner(profile, excludeUtil);
                backupRunner.RunBackup();

                // if reached here, the backup is completed without errors
                // => ask the user if another backup run should be done
                doAnotherRun = AskForAnotherRun();
            }
        }
        
        /// <summary>
        /// Prints a start message for the user.
        /// </summary>
        private void PrintStartingMessage()
        {
            ConsoleWriter.WriteMainMessage(Lang.Start);
            ConsoleWriter.WriteMainMessage(Lang.DryRunInfo);
            ConsoleWriter.EmptyLine();
            ConsoleWriter.WriteMainMessage(Lang.BackupProfiles);
        }

        /// <summary>
        /// Asks the user if another backup run should be done. Returns true if so, else false.
        /// </summary>
        /// <returns>true if another run should be done, else false</returns>
        private bool AskForAnotherRun()
        {
            // value to be returned
            // (might be changed to true due to user input while running this method)
            bool doAnotherRun = false;
            
            // ask if the user wants another backup run
            // => if not so exit, else the endless loop will take another run
            bool validInput = false;
            while (!validInput)
            {
                // valid inputs:
                // - default empty value (given as null)
                // - "n"/"N"
                // - "y"/"Y"
                ConsoleWriter.WriteMainMessage(Lang.AnotherRun);
                string input = Console.ReadLine();
                
                // if not any of the valid values (see above) is given, show an error message and re-run the input loop
                if (input != null 
                    && !input.ToLower().Trim().Equals("") 
                    && !input.ToLower().Trim().Equals("n") 
                    && !input.ToLower().Trim().Equals("y"))
                {
                    ConsoleWriter.WriteErrorMessage(Lang.InvalidInput);
                    continue;
                }
                
                // if reached here the input is valid
                validInput = true;
                
                // change return value to true if the user entered "y"/"Y"
                if (input != null && input.ToLower().Trim().Equals("y"))
                {
                    doAnotherRun = true;
                    
                    // separate next backup on console
                    ConsoleWriter.EmptyLine();
                }
            } 
            
            // return true if another run should be done, else false
            return doAnotherRun;
        }
    }
}