using System.Collections.Generic;
using System.Text;
using Backup.Utils;

namespace Backup
{
    public class BackupLocation
    {
        public string Path { get; private set; }
        public string Destination { get; private set; }
        public IList<string> ExcludePaths { get; set; }

        public BackupLocation(string path, string destination, IList<string> excludePaths)
        {
            Path = path;
            Destination = destination;
            ExcludePaths = excludePaths;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Backup-Location:\n")
                .Append("Path:\t'")
                .Append(Path)
                .Append("'\n")
                .Append("Dest:\t'")
                .Append(Destination)
                .Append("'\n")
                .Append("Excludes:\n");

            foreach (string exclude in ExcludePaths)
            {
                sb.Append("\t- '")
                    .Append(exclude)
                    .Append("'\n");
            }

            if (ExcludePaths.Count == 0)
            {
                sb.Append("\t[Kein Exclude-Path angegeben]");
            }
            
            return sb.ToString();
        }
    }
}