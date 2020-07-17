using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using System.Threading;
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
                    ConsoleWriter.WriteLineWithColor(Lang.ErrorNotExistingProfile, OutputColors.Error, profileDir);
                        
                    // wait for input to avoid that the window closes immediately
                    ConsoleWriter.WriteLineWithColor(Lang.EndProgram, OutputColors.Error);
                    Console.ReadLine();
                    
                    // exit
                    Environment.Exit(1);
                }
                
                // choose backup profile
                BackupProfile profile = SelectBackupProfile(profileDir);

                // run backup if there is a valid profile chosen
                if (profile != null)
                {
                    // info message
                    ConsoleWriter.WriteLineWithColor(Lang.StartBackup, OutputColors.MainMessages);
                    
                    // backup
                    try
                    {
                        BackupRunner.RunBackup(profile);
                    }
                    catch (Exception e)
                    {
                        // error message
                        ConsoleWriter.WriteLineWithColor(Lang.StopBecauseOfError, OutputColors.Error);
                        ConsoleWriter.WriteLineWithColor(Lang.ErrorMessage, OutputColors.Error);
                        ConsoleWriter.WriteLineWithColor("{0}", OutputColors.ErrorDetails, e.Message);

                        // stack trace
                        //ConsoleWriter.WriteWithColor(e.StackTrace, ConsoleColor.Yellow);
                        
                        // wait for input to avoid that the window closes immediately
                        ConsoleWriter.WriteLineWithColor(Lang.EndProgram, OutputColors.MainMessages);
                        Console.ReadLine();
                        
                        // exit
                        Environment.Exit(1);
                    }
                }
                
                // ask if the user wants another backup run
                // => if not so exit, else the endless loop will take another run
                ConsoleWriter.WriteLineWithColor(Lang.AnotherRun, OutputColors.MainMessages);
                string input = Console.ReadLine();
                if (input == null || !input.ToLower().Equals("j"))
                {
                    break;
                }
            }
        }

        private static BackupProfile SelectBackupProfile(string profileDir)
        {
            // print starting message
            ConsoleWriter.WriteLineWithColor(Lang.Start, OutputColors.MainMessages);

            // save all profile paths into a list
            IList<string> profilePaths = new List<string>();
            foreach (string profile in Directory.GetFiles(profileDir))
            {
                profilePaths.Add(profile);
            }
            
            // make all profile paths selectable by the user through entering a corresponding number
            for (int option = 1; option <= profilePaths.Count; option++)
            {
                ConsoleWriter.WriteLineWithColor("[{0}]: {1}", OutputColors.MainMessages, 
                                             option, Path.GetFileNameWithoutExtension(profilePaths[option-1]));
            }

            // check input and load profile when it is valid
            // => repeat until there is valid input
            int input = -1;
            while (input == -1)
            {
                // take input
                bool parsed = int.TryParse(Console.ReadLine(), out input);
                
                // check input
                if (!parsed || input <= 0 || input > profilePaths.Count)
                {
                    // error message because of invalid input
                    ConsoleWriter.WriteLineWithColor(Lang.InvalidNumber, OutputColors.Error);
                    
                    // reset input on -1 for taking another loop run
                    input = -1;
                }
                else
                {
                    // valid input, return backup profile
                    return BackupProfileConverter.LoadBackupProfile(profilePaths[input - 1]);
                }
            }

            // return null due to compiler checking, is never reached since the loop aboves
            // repeats until there is a valid input for continuing the program
            return null;
        }

        
    }
}