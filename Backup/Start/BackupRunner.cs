﻿using System;
using System.Collections.Generic;
using System.IO;
using Backup.Utils;
 using Backup.Resources;

namespace Backup.Start
{
    public class BackupRunner
    {
        public static void RunBackup(BackupProfile profile)
        {
            // go through all BackupLocations in the BackupProfile to backup them
            bool atLeastOneNewer = false;
            foreach (BackupLocation backupLocation in profile.BackupLocations)
            {
                // backup each directory recursively
                // => the return value marks if at least one file was updated (for better output to the user)
                ConsoleWriter.WriteLineWithColor(Lang.CheckBackupLocation, OutputColors.BackupLocations, backupLocation.Path);
                bool updated = BackupDirectoryRecursively(
                        backupLocation, backupLocation.Path, backupLocation.Destination, backupLocation.ExcludePaths);

                // set return flag if an file was changed and the flag is not yet set
                if (!atLeastOneNewer && updated)
                {
                    atLeastOneNewer = true;
                }
            }

            // output to the user after the backup (informs wheter there was something updated or not)
            if (!atLeastOneNewer)
            {
                ConsoleWriter.WriteLineWithColor(Lang.SuccessNoUpdate, OutputColors.Success);
            }
            else
            {
                ConsoleWriter.WriteLineWithColor(Lang.SuccessUpdate, OutputColors.Success);
            }
        }

        private static bool BackupDirectoryRecursively(BackupLocation backupLocation, string srcDir, string destDir,
            IList<string> excludePaths)
        {
            // return flag
            // => true, if at least one file or folder was updated/added/removed
            bool atLeastOneUpdated = false;
            
            // create the destination directory if it does not yet exist
            if (!Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
                atLeastOneUpdated = true;
            }

            // backup all newer files from the source to the destination path if not excluded
            foreach (string srcFile in Directory.GetFiles(srcDir))
            {
                if (!excludePaths.Contains(srcFile))
                {
                    // control message
                    //Logger.LogInfo("Check {0}", srcFile);
                    
                    // backup
                    bool saved = BackupFileIfNewer(backupLocation, srcFile, destDir);

                    // update change-flag if necessary
                    if (!atLeastOneUpdated && saved)
                    {
                        atLeastOneUpdated = true;
                    }
                }
            }
            
            // remove all files and directories that are no longer contained at the source path
            bool atLeastOneDeleted = DeleteFilesAndSubdirsNotContainedAnymore(backupLocation, srcDir, destDir);
            
            // update change-flag
            if (!atLeastOneUpdated && atLeastOneDeleted)
            {
                atLeastOneUpdated = true;
            }

            // repeat backup at all sub directories if not excluded
            foreach (string subDir in Directory.GetDirectories(srcDir))
            {
                if (!excludePaths.Contains(subDir))
                {
                    // combine destination path with sub directory name (must be got as file name!)
                    // => backup path for sub folder
                    string destOfSubDir = Path.Combine(destDir, Path.GetFileName(subDir));
                    
                    // backup the sub directory
                    bool atLeastOneNewerInSubDir = BackupDirectoryRecursively(
                                                        backupLocation, subDir, destOfSubDir, excludePaths);
                    
                    // update change-flag if necessary
                    if (!atLeastOneUpdated && atLeastOneNewerInSubDir)
                    {
                        atLeastOneUpdated = true;
                    }
                }
            }
            
            // return if at least one file/folder was updated
            return (atLeastOneUpdated);
        }

        private static bool BackupFileIfNewer(BackupLocation backupLocation, string srcFile, string destDir)
        {
            // get the file path without the path given in the BackupLocation
            // => that shortens the path for a clearer output to the user
            string pathWithoutLocationPrefix = srcFile.Remove(0, backupLocation.Path.Length);
            
            // combine the destination directory with the file name
            // => backup path for the file
            string destOfFile = Path.Combine(destDir, Path.GetFileName(srcFile));
            
            // backup the file if not existing yet
            if (!File.Exists(destOfFile))
            {
                // output to user
                ConsoleWriter.WriteBackupAddition(pathWithoutLocationPrefix);

                // backup
                File.Copy(srcFile, destOfFile, true);
                
                // return true because a new file was added
                return true;
            }
            
            // backup the file if newer on source path than in destination path
            if (File.GetLastWriteTime(srcFile) > File.GetLastWriteTime(destOfFile))
            {
                // control message
                //Logger.LogInfo("Backup: {0} => {1}", srcFile, destOfFile);
                
                // user output, show only the part of the source path after the BackupLocation-path for
                // a clearer output
                ConsoleWriter.WriteBackupUpdate(pathWithoutLocationPrefix);
                
                // if the destination part already exists and is readonly, mark it as writeable so that it
                // can be removed
                // => e.g. needed for git-files since they are read-only
                if (File.Exists(destOfFile))
                {
                    FileInfo destFileInfo = new FileInfo(destOfFile);
                    if (destFileInfo.IsReadOnly)
                    {
                        new FileInfo(destOfFile).IsReadOnly = false;
                        //File.SetAttributes(destOfFile, FileAttributes.Normal);
                    }
                }
                
                // backup
                File.Copy(srcFile, destOfFile, true);
                
                // return true because newer file
                return true;
            }

            // false because no newer file
            return false;
        }

        private static bool DeleteFilesAndSubdirsNotContainedAnymore(BackupLocation backupLocation, string srcDir, string destDir)
        {
            // result flag
            // => true if at least one change had been made
            bool atLeastOneDeleted = false;
            
            // delete all files in the destination directory which are not (any more) in the source directory
            foreach (string destFile in Directory.GetFiles(destDir))
            {
                // determine the source path of each file in the destination path to check whether
                // the file exists on the source path
                string shouldBeSrcFile = Path.Combine(srcDir, Path.GetFileName(destFile));
                
                // get the file path without the path given in the BackupLocation
                // => that shortens the path for a clearer output to the user
                string pathWithoutLocationPrefix = shouldBeSrcFile.Remove(0, backupLocation.Path.Length);
                
                // remove the file in the destination path if there is no corresponding file in the source path
                if (!File.Exists(shouldBeSrcFile))
                {
                    // control message
                    //Logger.LogInfo("Deleted: {0}", shouldBeSrcFile);
                    
                    // output for user
                    ConsoleWriter.WriteBackupRemove(pathWithoutLocationPrefix);
                    
                    // mark the destination file as writeable if it is readonly so that it can be removed
                    // => e.g. needed for git-files since they are read-only
                    FileInfo destFileInfo = new FileInfo(destFile);
                    if (destFileInfo.IsReadOnly)
                    {
                        new FileInfo(destFile).IsReadOnly = false;
                        //File.SetAttributes(destOfFile, FileAttributes.Normal);
                    }
                    
                    // remove file
                    File.Delete(destFile);
                    
                    // update change-flag
                    if (!atLeastOneDeleted)
                    {
                        atLeastOneDeleted = true;
                    }
                }
            }
            
            // remove all sub directories in the destination path which do not (any more) exist at the source path
            foreach (string destSubDir in Directory.GetDirectories(destDir))
            {
                // determine the source path of each sub directory in the destination path to check whether
                // the sub directory exists on the source path (needs to be asked as file name!)
                string shouldBeSrcDir = Path.Combine(srcDir, Path.GetFileName(destSubDir));
                
                // get the sub directory path without the path given in the BackupLocation
                // => that shortens the path for a clearer output to the user
                string pathWithoutLocationPrefix = shouldBeSrcDir.Remove(0, backupLocation.Path.Length);
                
                // remove the sub directory if there is no corresponding sub directory at the source path
                if (!Directory.Exists(shouldBeSrcDir))
                {
                    // control message
                    //Logger.LogInfo("Removed sub dir: {0}", shouldBeSrcDir);
                    
                    // output to user
                    ConsoleWriter.WriteBackupRemove(pathWithoutLocationPrefix);
                    
                    // delete this sub directory recursively
                    // => own method because read only attributes might be changed
                    DeleteDirectoryRecursively(destSubDir);
                    
                    // update change-flag
                    if (!atLeastOneDeleted)
                    {
                        atLeastOneDeleted = true;
                    }
                }
            }
            
            // return the change flag
            // => true if some file or folder was updated
            return atLeastOneDeleted;
        }

        private static void DeleteDirectoryRecursively(string destDir)
        {
            // set directory attributes to "normal" so that the directory can be deleted at the end
            // of the method
            DirectoryInfo directoryInfo = new DirectoryInfo(destDir);
            if ((directoryInfo.Attributes & FileAttributes.ReadOnly) != 0)
            {
                directoryInfo.Attributes &= ~FileAttributes.ReadOnly;
            }

            // mark the file as writeable if it is readonly so that it can be removed
            // => e.g. needed for git-files since they are read-only
            foreach (string file in Directory.GetFiles(destDir))
            {
                FileInfo fileInfo = new FileInfo(file);
                fileInfo.IsReadOnly = false;
                //File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }
            
            // repeat in each sub directory recursively
            foreach (string subDir in Directory.GetDirectories(destDir))
            {
                DeleteDirectoryRecursively(subDir);
            }
            
            // delete the (now empty) directory itself
            directoryInfo.Delete();
        }
    }
}