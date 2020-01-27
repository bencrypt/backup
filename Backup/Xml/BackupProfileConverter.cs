using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Backup.Utils;

namespace Backup.Xml
{
    public class BackupProfileConverter
    {
        public static void LoadBackupProfile(string path)
        {
            if (!File.Exists(path) || !path.EndsWith(".xml"))
            {
                throw new Exception("Unter dem angegebenen Pfad konnte kein gültiges Backup-Profil gefunden werden");
            }

            // XML-Datei mit Backup-Profiles laden
            XDocument doc = XDocument.Load(path);

            // alle Backup-Locations in programm-interne Daten umwandeln
            IEnumerable<XElement> xmlLocs = doc.XPathSelectElements("/backup_profile/backup_location");
            IList<BackupLocation> locs = new List<BackupLocation>();
            

            foreach (XElement xmlLoc in xmlLocs)
            {
                string locPath = xmlLoc.Element("path").Value;

                IEnumerable<XElement> locExcludes = xmlLoc.Elements("exclude");
                
                IList<string> excludePaths = new List<string>();
                foreach (XElement exclude in locExcludes)
                {
                    excludePaths.Add(exclude.Element("path").Value);
                }
                
                locs.Add(new BackupLocation(locPath, excludePaths));
            }

            // Backup-Profil aus importierten Daten erstellen
            string name = doc.XPathSelectElement("/backup_profile/name").Value;
            BackupProfile profile = new BackupProfile(name, locs);

            // Kontrollnachricht
            Logger.LogInfo(profile.ToString());
        }
    }
}