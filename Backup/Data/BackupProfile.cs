using System.Collections.Generic;
using System.Text;

namespace Backup
{
    public class BackupProfile
    {
        public string Name { get; private set; }
        public IList<BackupLocation> BackupLocations { get; private set; }

        public BackupProfile(string name, IList<BackupLocation> backupLocations)
        {
            Name = name;
            BackupLocations = backupLocations;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Backup-Profil '")
                .Append(Name)
                .Append("':\n");

            foreach (BackupLocation loc in BackupLocations)
            {
                sb.Append(">>>\n");
                sb.Append(loc);
            }

            return sb.ToString();
        }
    }
}