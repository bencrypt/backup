using System;
using System.Collections.Generic;
using System.Text;
using Backup.Utils;

namespace Backup
{
    public class BackupLocation
    {
        /*==================================================*
         *==                   FIELDS                     ==*
         *==================================================*/
        
        public string Path { get; }
        public string Destination { get; }
        public IList<string> ExcludePaths { get; }
        
        /*==================================================*
         *==                CONSTRUCTORS                  ==*
         *==================================================*/

        public BackupLocation(string path, string destination, IList<string> excludePaths)
        {
            Path = path;
            Destination = destination;
            ExcludePaths = excludePaths;
        }
        
        /*==================================================*
         *==                 TOSTRING()                   ==*
         *==================================================*/

        public override string ToString()
        {
            // basic structure
            StringBuilder sb = new StringBuilder();
            sb.Append("BackupLocation:")
                .Append(Environment.NewLine)
                .Append("Path:\t'")
                .Append(Path)
                .Append("'")
                .Append(Environment.NewLine)
                .Append("Destination:\t'")
                .Append(Destination)
                .Append("'")
                .Append(Environment.NewLine)
                .Append("Excludes:")
                .Append(Environment.NewLine);

            // show exclude paths (or that there is no such path)
            foreach (string exclude in ExcludePaths)
            {
                sb.Append("\t- '")
                    .Append(exclude)
                    .Append("'")
                    .Append(Environment.NewLine);
            }

            if (ExcludePaths.Count == 0)
            {
                sb.Append("\t[No ExcludePath given]");
            }
            
            // return created string
            return sb.ToString();
        }
    }
}