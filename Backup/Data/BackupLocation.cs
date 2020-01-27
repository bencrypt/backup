using System.Collections.Generic;
using System.Text;
using Backup.Utils;

namespace Backup
{
    public class BackupLocation
    {
        public string Path { get; set; }
        public IList<string> ExcludePaths { get; set; }

        public BackupLocation(string path, IList<string> excludePaths)
        {
            Path = path;
            ExcludePaths = excludePaths;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Backup-Location:\n")
                .Append("Path:\t'")
                .Append(Path)
                .Append("'\n")
                .Append("Excludes:\n");

            foreach (string exclude in ExcludePaths)
            {
                sb.Append("- '")
                    .Append(exclude)
                    .Append("'\n");
            }
            
            return sb.ToString();
        }
    }
}