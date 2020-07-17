using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Backup.Resources;
using Backup.Utils;

namespace Backup.Xml
{
    public class BackupProfileConverter
    {
        public static BackupProfile LoadBackupProfile(string path)
        {
            /*
             * load XML
             */
            
            // check that there is an xml under the given path
            if (!File.Exists(path) || !path.EndsWith(".xml"))
            {
                throw new Exception(Lang.InvalidProfilePath);
            }

            // load XML
            XDocument doc = XDocument.Load(path);

            /*
             * convert XML entries to BackupProfile
             */
            
            // get all BackupLocations and create an (now empty) list for them
            IEnumerable<XElement> xmlLocs = doc.XPathSelectElements("/backup_profile/backup_location");
            IList<BackupLocation> locs = new List<BackupLocation>();

            // go through each BackupLocation-entry, convert it to an program-internal object
            // and add this one to the created list
            foreach (XElement xmlLoc in xmlLocs)
            {
                // source and dest path
                string locPath = xmlLoc.Element("path").Value;
                string locDest = xmlLoc.Element("dest").Value;

                // excluding paths (result list may be empty if not existing)
                XElement locExcludes = xmlLoc.Element("exclude");
                IList<string> excludePaths = new List<string>();

                if (locExcludes != null && locExcludes.HasElements)
                {
                    foreach (XElement excludePath in locExcludes.Elements("path"))
                    {
                        excludePaths.Add(excludePath.Value);
                    }
                }

                // create the BackupLocation-object and add it to the result list
                locs.Add(new BackupLocation(locPath, locDest, excludePaths));
            }

            // create a BackupProfile from the BackupLocations
            string name = doc.XPathSelectElement("/backup_profile/name").Value;
            BackupProfile profile = new BackupProfile(name, locs);
            
            /*
             * return converted BackupProfile
             */

            // control message
            //Logger.LogInfo(profile.ToString());
            
            // return the profile
            return profile;
        }
    }
}