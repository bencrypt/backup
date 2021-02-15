﻿using System.Collections.Generic;
using System.IO;
using Backup.Resources;
using Backup.Utils;

namespace Backup.Start
{
    public class BackupRunner
    {
        /// <summary>
        /// Initiates the actual backup specified by the given backup profile. This includes adding new files/dirs,
        /// updating existing but newer files, removing not (anymore) existing files and directories.
        /// Writes on the console when the backup has finished successfully. Might throw errors which needs to be
        /// captured from the caller of this method.
        /// </summary>
        /// <param name="profile">the backup profile to run</param>
        public static void RunBackup(BackupProfile profile)
        {
            // go through all BackupLocations in the BackupProfile to backup them
            bool atLeastOneNewer = false;
            foreach (BackupLocation backupLocation in profile.BackupLocations)
            {
                // backup each directory recursively
                // => the return value marks if at least one file was updated (for better output to the user)
                ConsoleWriter.WriteBackupLocationHeadline(Lang.CheckBackupLocation, backupLocation.Path);
                bool updated = BackupDirectoryRecursively(
                        backupLocation, backupLocation.Path, backupLocation.Destination, backupLocation.ExcludePaths,
                        profile.DryRun);

                // set return flag if an file was changed and the flag is not yet set
                if (!atLeastOneNewer && updated)
                {
                    atLeastOneNewer = true;
                }
            }
            
            // feedback output to user
            if (profile.DryRun)
            {
                // output to the user after a dry run
                ConsoleWriter.WriteSuccessMessage(Lang.SuccessDryRun);
            }
            else
            {
                // output to the user after the backup (informs whether there was something updated or not)
                if (!atLeastOneNewer)
                {
                    ConsoleWriter.WriteSuccessMessage(Lang.SuccessNoUpdate);
                }
                else
                {
                    ConsoleWriter.WriteSuccessMessage(Lang.SuccessUpdate);
                }
            }
        }

        /// <summary>
        /// Mirrors the given source directory to the given destination directory. This means new files are added,
        /// not anymore existing files at the source will be deleted at the destination and newer files or
        /// subdirectories are updated. Returns true when at least one modification was done, else false.
        /// </summary>
        /// <param name="backupLocation">the backup location for which the backup is run</param>
        /// <param name="srcDir">the source directory to be mirrored</param>
        /// <param name="destDir">the destination directory to contain the backup</param>
        /// <param name="excludePaths">maybe defined exclude paths which are not to consider when doing the backup</param>
        /// <param name="dryRun">true if changes should only be shown but not actually be made</param>
        /// <returns>true when modifications, else false</returns>
        private static bool BackupDirectoryRecursively(BackupLocation backupLocation, string srcDir, string destDir,
            IList<string> excludePaths, bool dryRun)
        {
            // return flag
            // => true, if at least one file or folder was updated/added/removed
            bool atLeastOneUpdated = false;
            
            // create the destination directory if it does not yet exist
            if (!Directory.Exists(destDir) && !dryRun)
            {
                Directory.CreateDirectory(destDir);
                atLeastOneUpdated = true;
            }

            // backup all newer files from the source to the destination path if not excluded
            foreach (string srcFile in Directory.GetFiles(srcDir))
            {
                if (!ExcludeUtil.ShouldFileBeExcluded(srcFile, excludePaths))
                {
                    // control message
                    //Logger.LogInfo("Check {0}", srcFile);
                    
                    // backup
                    bool saved = BackupFileIfNewer(backupLocation, srcFile, destDir, dryRun);

                    // update change-flag if necessary
                    if (!atLeastOneUpdated && saved)
                    {
                        atLeastOneUpdated = true;
                    }
                }
            }
            
            // remove all files and directories that are no longer contained at the source path
            bool atLeastOneDeleted = DeleteFilesAndSubdirsNotContainedAnymore(backupLocation, srcDir, destDir, dryRun);
            
            // update change-flag
            if (!atLeastOneUpdated && atLeastOneDeleted)
            {
                atLeastOneUpdated = true;
            }

            // repeat backup at all sub directories if not excluded
            foreach (string subDir in Directory.GetDirectories(srcDir))
            {
                if (!ExcludeUtil.ShouldDirectoryBeExcluded(subDir, excludePaths))
                {
                    // combine destination path with sub directory name (must be got as file name!)
                    // => backup path for sub folder
                    string destOfSubDir = Path.Combine(destDir, Path.GetFileName(subDir));
                    
                    // backup the sub directory
                    bool atLeastOneNewerInSubDir = BackupDirectoryRecursively(
                                                        backupLocation, subDir, destOfSubDir, excludePaths, dryRun);
                    
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

        /// <summary>
        /// Mirrors the given source file to the given destination file. This means new files are added
        /// and newer files are updated.
        /// Returns true when at least one modification was done, else false.
        /// </summary>
        /// <param name="backupLocation">the backup location for which the backup is run</param>
        /// <param name="srcFile">the source file to be mirrored</param>
        /// <param name="destDir">the destination file to contain the backup</param>
        /// <param name="dryRun">true if changes should only be shown but not actually be made</param>
        /// <returns>true when modifications, else false</returns>
        private static bool BackupFileIfNewer(BackupLocation backupLocation, string srcFile, string destDir, bool dryRun)
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
                if (!dryRun)
                {
                    File.Copy(srcFile, destOfFile, true);
                }

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
                if (File.Exists(destOfFile) && !dryRun)
                {
                    FileInfo destFileInfo = new FileInfo(destOfFile);
                    if (destFileInfo.IsReadOnly)
                    {
                        new FileInfo(destOfFile).IsReadOnly = false;
                        //File.SetAttributes(destOfFile, FileAttributes.Normal);
                    }
                }
                
                // backup
                if (!dryRun)
                {
                    File.Copy(srcFile, destOfFile, true);
                }

                // return true because newer file
                return true;
            }

            // false because no newer file
            return false;
        }

        /// <summary>
        /// Deletes files and subdirectories contained in the destination directory but not contained in the given
        /// source directory.
        /// Returns true when at least one modification was done, else false.
        /// </summary>
        /// <param name="backupLocation">the backup location for which the backup is run</param>
        /// <param name="srcDir">the source directory to be mirrored</param>
        /// <param name="destDir">the destination directory to contain the backup</param>
        /// <param name="dryRun">true if changes should only be shown but not actually be made</param>
        /// <returns>true when modifications, else false</returns>
        private static bool DeleteFilesAndSubdirsNotContainedAnymore(BackupLocation backupLocation, string srcDir, string destDir, bool dryRun)
        {
            // result flag
            // => true if at least one change had been made
            bool atLeastOneDeleted = false;
            
            // if a dry run is done the destination directory might not exist since it might not be created before
            // because of the dry flag
            // => in this case the method cannot be executed without errors since no file and sub directory can be
            //    deleted in the destination directory when the destination directory does not exist itself
            if (dryRun && !Directory.Exists(destDir))
            {
                return false;
            }
            
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
                    if (destFileInfo.IsReadOnly && !dryRun)
                    {
                        new FileInfo(destFile).IsReadOnly = false;
                        //File.SetAttributes(destOfFile, FileAttributes.Normal);
                    }
                    
                    // remove file
                    if (!dryRun)
                    {
                        File.Delete(destFile);
                    }

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
                    DeleteDirectoryRecursively(destSubDir, dryRun);
                    
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

        /// <summary>
        /// Deletes the given directory recursively. Forces this deletion with removing a probably readonly
        /// attribute.
        /// </summary>
        /// <param name="destDir">the directory to be deleted recursively</param>
        /// <param name="dryRun">true if changes should only be shown but not actually be made</param>
        private static void DeleteDirectoryRecursively(string destDir, bool dryRun)
        {
            // set directory attributes to "normal" so that the directory can be deleted at the end
            // of the method
            DirectoryInfo directoryInfo = new DirectoryInfo(destDir);
            if ((directoryInfo.Attributes & FileAttributes.ReadOnly) != 0 && !dryRun)
            {
                directoryInfo.Attributes &= ~FileAttributes.ReadOnly;
            }

            // mark the file as writeable if it is readonly so that it can be removed
            // => e.g. needed for git-files since they are read-only
            if (!dryRun)
            {
                foreach (string file in Directory.GetFiles(destDir))
                {
                    FileInfo fileInfo = new FileInfo(file);
                    fileInfo.IsReadOnly = false;
                    //File.SetAttributes(file, FileAttributes.Normal);
                    File.Delete(file);
                }
            }

            // repeat in each sub directory recursively
            foreach (string subDir in Directory.GetDirectories(destDir))
            {
                DeleteDirectoryRecursively(subDir, dryRun);
            }
            
            // delete the (now empty) directory itself
            if (!dryRun)
            {
                directoryInfo.Delete();
            }
        }
    }
}