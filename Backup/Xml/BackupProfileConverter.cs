using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;
using Backup.Data;
using Backup.Resources;

namespace Backup.Xml
{
    public class BackupProfileConverter
    {
        /// <summary>
        /// Loads the BackupProfile saved at the given path.
        /// <br />
        /// Does not check wheter the given path is valid so this must be verified before calling this
        /// method. However this method checks if the specification of the profile is correct and prints
        /// errors that might occur while parsing.
        /// <br />
        /// Returns the converted BackupProfile or throws/forwards a BackupException if an error occurs.
        /// So the Main method can handle the exception for unifying the exit point of the application.
        /// </summary>
        /// <param name="path">a valid path for a backup profile</param>
        /// <param name="dryRun">true if changes should only be shown but not actually be made</param>
        /// <exception cref="BackupException">thrown or forwarded if the backup profile has errors (not existing source,
        /// malformatted xml etc.)</exception>
        /// <returns>a valid backup profile or null</returns>
        public BackupProfile LoadBackupProfile(string path, bool dryRun)
        {
            /*
             * load XML
             */

            // load XML, might have errors 
            XDocument doc;
            try
            {
                doc = XDocument.Load(path);
            }
            catch (Exception e)
            {
                // error while loading the xml (like syntax error), throw a BackupException
                IList<string> errorMessages = new List<string>() { Lang.ErrorXmlMalformatted, Lang.ErrorMessage };
                throw new BackupException(errorMessages, e.Message);
            }

            /*
             * convert XML entries to BackupProfile,
             * check for errors while that
             */

            // parse backup locations inside the profile if valid (might throw a BackupException)
            IList<BackupLocation> xmlLocs = ParseBackupLocations(doc);

            // no errors occured while parsing, so create a BackupProfile from the BackupLocations
            string name = Path.GetFileNameWithoutExtension(path);
            BackupProfile profile = new BackupProfile(name, xmlLocs, dryRun);
            
            // control message
            //Logger.LogInfo(profile.ToString());
            
            // return the profile
            return profile;
        }

        /// <summary>
        /// Parses the backup locations from the given xml document and returns them as list.
        /// <br />
        /// If an error occurs, a BackupException will be thrown and forwarded to the Main method for unifying the
        /// exit point of the application.
        /// </summary>
        /// <param name="doc">the xml document to parse from</param>
        /// <exception cref="BackupException">thrown if the backup location has errors (not existing source etc.)</exception>
        /// <returns>valid backup locations or null (when error)</returns>
        private IList<BackupLocation> ParseBackupLocations(XDocument doc)
        {
            // result list
            IList<BackupLocation> locs = new List<BackupLocation>();
            
            // check if there are backup locations defined
            if (doc.XPathSelectElement("/backup_profile/backup_location") == null)
            {
                throw new BackupException(Lang.ErrorXmlMissingBackupLocations);
            }
            
            // all backup locations specified in the xml
            IEnumerable<XElement> xmlLocs = doc.XPathSelectElements("/backup_profile/backup_location");
            
            // go through each BackupLocation-entry, convert it to an program-internal object
            // and add this one to the created list
            foreach (XElement xmlLoc in xmlLocs)
            {
                // check for errors and throw a BackupException if an error occurs
                IList<string> xmlValidationErrors = checkForValidXmlElements(xmlLoc);
                if (xmlValidationErrors.Count > 0)
                {
                    IList<string> errorMessages = new List<string>() { Lang.ErrorXmlNodes, Lang.ErrorMessage };
                    throw new BackupException(errorMessages, xmlValidationErrors);
                }
                
                // no errors, add backup location to internal backup profile
                locs.Add(convertXml(xmlLoc));
            }

            // no error while parsing, return the result list
            return locs;
        }

        /// <summary>
        /// Check if the XML representation of the given backup location is valid.
        /// Especially check if the source and exclude paths are valid and existing. Pay attention to that the
        /// destination path does not need to be checked since it can be created first by the backup.
        /// <br />
        /// Returns a list with the errors that occured while checking the xml element. If no errors occured the
        /// list ist empty.
        /// </summary>
        /// <param name="xmlLoc">the xml representation of one backup location</param>
        /// <returns>a list with errors while checking</returns>
        private IList<string> checkForValidXmlElements(XElement xmlLoc)
        {
            // result list, might be empty when everything is valid
            IList<string> errorMessages = new List<string>();
            
            // check source path
            bool validSrc = xmlLoc.Element("src") != null && (Directory.Exists(xmlLoc.Element("src")?.Value) 
                                                              || File.Exists(xmlLoc.Element("src")?.Value));
            
            // check if dest element exists (but do not check path since it might be created during the backup)
            bool existingDest = xmlLoc.Element("dest") != null;

            // add messages for invalid src and dest path into result collection if needed
            if (!validSrc)
            {
                errorMessages.Add(String.Format(Lang.ErrorXmlSrc, xmlLoc));
            }

            if (!existingDest)
            {
                errorMessages.Add(String.Format(Lang.ErrorXmlDest, xmlLoc));
            }

            // check if at least one exclude path is given when an exclude element is existing
            if (xmlLoc.Element("exclude") != null && xmlLoc.Element("exclude")?.Elements("path") == null)
            {
                errorMessages.Add(String.Format(Lang.ErrorXmlExcludeMissingPaths, xmlLoc));
            }

            // return true when all elements are valid, else false
            return errorMessages;
        }

        /// <summary>
        /// Converts the given xml representation of a backup location to an internal object and returns this
        /// internal representation.
        /// Requires that the given xml element and it's sub elements are all valid.
        /// </summary>
        /// <param name="xmlLoc">the xml representation of one backup location</param>
        /// <returns>a valid backup location object</returns>
        private BackupLocation convertXml(XElement xmlLoc) {
    
            // source and dest path, should already be checked for validity
            string locPath = xmlLoc.Element("src").Value;
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
            return new BackupLocation(locPath, locDest, excludePaths);
        }
    }
}