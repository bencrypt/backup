using System;
using System.Collections.Generic;
using System.Text;

namespace Backup
{
    public class BackupProfile
    {
        /*==================================================*
         *==                   FIELDS                     ==*
         *==================================================*/
        
        private string Name { get; }
        public IList<BackupLocation> BackupLocations { get; }
        public bool DryRun { get; }

        /*==================================================*
         *==                CONSTRUCTORS                  ==*
         *==================================================*/
        
        public BackupProfile(string name, IList<BackupLocation> backupLocations, bool dryRun)
        {
            Name = name;
            BackupLocations = backupLocations;
            DryRun = dryRun;
        }
        
        /*==================================================*
         *==                 TOSTRING()                   ==*
         *==================================================*/

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("BackupProfile '")
                .Append(Name)
                .Append("':")
                .Append(Environment.NewLine);

            foreach (BackupLocation loc in BackupLocations)
            {
                sb.Append(">>>")
                    .Append(Environment.NewLine)
                    .Append(loc);
            }

            return sb.ToString();
        }
    }
}