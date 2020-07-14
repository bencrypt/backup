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
        public static BackupProfile LoadBackupProfile(string path)
        {
            if (!File.Exists(path) || !path.EndsWith(".xml"))
            {
                throw new Exception("Unter dem angegebenen Pfad konnte kein gültiges Backup-Profil gefunden werden");
            }

            // XML-Datei mit Backup-Profiles laden
            XDocument doc = XDocument.Load(path);

            /*
             * alle BackupLocations in programm-interne Daten umwandeln
             */
            
            // ermittle alle BackupLocations und erstelle eine (noch leere) Liste dafür
            IEnumerable<XElement> xmlLocs = doc.XPathSelectElements("/backup_profile/backup_location");
            IList<BackupLocation> locs = new List<BackupLocation>();

            // gehe alle BackupLocations durch, erstelle daraus interne Elemente und füge sie zur Liste hinzu
            foreach (XElement xmlLoc in xmlLocs)
            {
                // Quell- und Zielordner
                string locPath = xmlLoc.Element("path").Value;
                string locDest = xmlLoc.Element("dest").Value;

                // auszulassene Pfade (ggf. leer, falls nicht vorhanden)
                XElement locExcludes = xmlLoc.Element("exclude");
                IList<string> excludePaths = new List<string>();

                if (locExcludes != null && locExcludes.HasElements)
                {
                    foreach (XElement excludePath in locExcludes.Elements("path"))
                    {
                        excludePaths.Add(excludePath.Value);
                    }
                }

                // erstelle eine interne BackupLocation und füge sie zur Liste hinzu
                locs.Add(new BackupLocation(locPath, locDest, excludePaths));
            }

            // Backup-Profil aus importierten Daten erstellen
            string name = doc.XPathSelectElement("/backup_profile/name").Value;
            BackupProfile profile = new BackupProfile(name, locs);

            // Kontrollnachricht
            //Logger.LogInfo(profile.ToString());
            
            // gebe das erstellte BackupProfile zurück
            return profile;
        }
    }
}