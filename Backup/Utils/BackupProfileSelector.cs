using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Backup.Data;
using Backup.Resources;

namespace Backup.Utils
{
    public class BackupProfileSelector
    {
        public bool IsDryRunSelected { get; private set; }
        
        private string _profileDir;
        private IList<string> _profilePaths;

        /// <summary>
        /// Initializes and checks the path were the backup profiles should be stored. This is the directory
        /// '../backup_profiles' seen from the application's script file.
        /// </summary>
        public void InitProfilePaths()
        {
            // init profile directory with absolute path of backup profiles for "../backup_profiles"
            string scriptPath = Path.GetFullPath(Assembly.GetExecutingAssembly().Location);
            _profileDir = Path.GetFullPath(Path.Combine(scriptPath, "..", "..", "backup_profiles"));
            
            // if invalid profile path show an error message and exit
            if (!Directory.Exists(_profileDir))
            {
                throw new BackupException(string.Format(Lang.ErrorNotExistingProfileDir, _profileDir));
            }
            
            // create a list from all profile xml files inside the (existing) directory
            _profilePaths = new List<string>();
            foreach (string profile in Directory.GetFiles(_profileDir))
            {
                if (profile.EndsWith(".xml"))
                {
                    _profilePaths.Add(profile);
                }
            }
            
            // check if there are profiles to select
            // => if not so inform the user and exit
            if (_profilePaths.Count == 0)
            {
                throw new BackupException(string.Format(Lang.NoProfileAtPath, _profileDir), Lang.NoProfileAtPathHelp);
            }
        }
        
        
        /// <summary>
        /// Let the user select interactively a backup profile from the profiles available in the given
        /// directory. Returns the path of the selected backup profile.
        /// <br />
        /// The user can also cancel the application by entering '0'. In this case the application will exit after
        /// another key stroke of the user.
        /// </summary>
        /// <returns>the path of the selected backup profile or null if the user wishes to cancel the application</returns>
        public string SelectBackupProfile()
        {
            PrintAvailableBackupProfiles();
            return LetUserSelectProfilePath(_profilePaths);
        }

        /// <summary>
        /// Prints all available backup profiles on the console lead by a number like [x] which is the index of the
        /// backup profile. This index can be entered by the user afterwards to select a specific profile.
        /// <br/>
        /// Also adds [0] at the end of the backup profiles to make it possible for the user to cancel the application.
        /// </summary>
        private void PrintAvailableBackupProfiles()
        {
            // make all profile paths selectable by the user through entering a corresponding number
            for (int option = 1; option <= _profilePaths.Count; option++)
            {
                ConsoleWriter.WriteMainMessage("[{0}]: {1}", option, Path.GetFileNameWithoutExtension(_profilePaths[option-1]));
            }
            
            // add option to close the program without running a backup
            ConsoleWriter.WriteMainMessage("[0]: {0}", Lang.OptionCancel);
        }

        /// <summary>
        /// Let the user select one of the given profiles via terminal input.
        /// Returns the path of the selected backup profile or cancels the application if the user enters '0'.
        /// </summary>
        /// <param name="profilePaths">the paths to the single backup profiles</param>
        /// <returns>the path of the selected backup profile</returns>
        private string LetUserSelectProfilePath(IList<string> profilePaths)
        {
            string profilePath = null;
            
            // repeat until there is valid input (profile path selected or application cancelled)
            while (true)
            {
                // get input
                ConsoleWriter.EmptyLine();
                ConsoleWriter.WriteMainMessage(Lang.ChosenProfile);
                string input = Console.ReadLine();

                // check if a dry run should make (changes will be shown but not actually be made);
                // afterwards remove dry-flag so that the rest of the string can be parsed to a backup profile path
                IsDryRunSelected = false;
                if (input != null && input.Contains(" --dry"))
                {
                    IsDryRunSelected = true;
                    input = input.Remove(input.IndexOf(" --dry", StringComparison.CurrentCulture));
                }

                // get profile number if provided (else the returned value will be false)
                bool parsed = int.TryParse(input, out int selectedProfile);

                // handle exit input and stop input loop
                if (parsed && selectedProfile == 0)
                {
                    ConsoleWriter.WriteMainMessage(Lang.SelectExit);
                    break;
                }

                // check non-exit input for valid selection of a backup profile
                // => write error message and restart input loop
                if (!parsed || selectedProfile <= 0 || selectedProfile > profilePaths.Count)
                {
                    ConsoleWriter.WriteErrorMessage(Lang.InvalidNumber);
                    continue;
                }
                
                // valid input, set return value and stop input loop
                profilePath = profilePaths[selectedProfile - 1];
                break;
            }

            // profile path or null if cancelled
            return profilePath;
        }
    }
}