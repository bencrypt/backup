using System.Collections.Generic;

namespace Backup.Utils
{
    public class ExcludeUtil
    {
        /// <summary>
        /// Returns true if the given file should be excluded from the backup, else false.
        /// </summary>
        /// <param name="filePath">the file path to check</param>
        /// <param name="excludePaths">all exclude paths of the current used backup profile</param>
        /// <returns>true if the file should be excluded, else false</returns>
        public static bool ShouldFileBeExcluded(string filePath, IList<string> excludePaths)
        {
            // full path is listed in exclude paths
            if (excludePaths.Contains(filePath))
            {
                return true;
            }
            
            // file extension is listed in exclude paths
            if (ExcludeUtil.ContainsExcludedFileExtension(filePath, excludePaths))
            {
                return true;
            }
            
            // none of the above exclude checks did pass, thus the file should not be excluded
            return false;
        }

        /// <summary>
        /// Returns true if the given file has an extension that should be excluded. Else returns false because
        /// the file extension should not be excluded.
        /// </summary>
        /// <param name="filePath">the file path to check</param>
        /// <param name="excludePaths">all exclude paths (path entries like "*.extension" is checked here)</param>
        /// <returns>true if the file should be excluded, else false</returns>
        private static bool ContainsExcludedFileExtension(string filePath, IList<string> excludePaths)
        {
            // go through each exclude path definition
            foreach (string excludePath in excludePaths)
            {
                // "*." in excludePath shows that a file extension should be ignored,
                // everything after the star character should be checked against the given file path ending
                if (excludePath.StartsWith("*.") && filePath.EndsWith(excludePath.Substring(1)))
                {
                    //Logger.LogInfo("Exclude file because of extension: {0}", filePath);
                    return true;
                }
            }

            // file extension should not be ignored
            return false;
        }
    }
}