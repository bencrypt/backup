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
        
        public string Name { get; private set; }
        public IList<BackupLocation> BackupLocations { get; private set; }

        /*==================================================*
         *==                CONSTRUCTORS                  ==*
         *==================================================*/
        
        public BackupProfile(string name, IList<BackupLocation> backupLocations)
        {
            Name = name;
            BackupLocations = backupLocations;
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