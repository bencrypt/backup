﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;
using Backup.Resources;
using Backup.Utils;

namespace Backup.Xml
{
    public class BackupProfileConverter
    {
        /// <summary>
        /// Loads the BackupProfile saved at the given path.
        /// 
        /// Does not check wheter the given path is valid so this must be verified before calling this
        /// method. However this method checks if the specification of the profile is correct and prints
        /// errors that might occur while parsing.
        ///
        /// Returns the converted BackupProfile or null if an error occured.
        /// </summary>
        /// <param name="path">a valid path for a backup profile</param>
        /// <returns>a valid backup profile or null</returns>
        public BackupProfile LoadBackupProfile(string path)
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
                // error while loading the xml (like syntax error)
                ConsoleWriter.WriteErrorMessage(Lang.ErrorXmlMalformatted);
                ConsoleWriter.WriteErrorMessage(Lang.ErrorMessage);
                ConsoleWriter.WriteErrorDetails("{0}", e.Message);

                // return null due to the error
                return null;
            }

            /*
             * convert XML entries to BackupProfile,
             * check for errors while that
             */

            // parse backup locations inside the profile
            IList<BackupLocation> xmlLocs = ParseBackupLocations(doc);

            // create backup profile when no errors occured while parsing
            if (xmlLocs != null)
            {
                // create a BackupProfile from the BackupLocations
                string name = Path.GetFileNameWithoutExtension(path);
                BackupProfile profile = new BackupProfile(name, xmlLocs);
                
                // control message
                //Logger.LogInfo(profile.ToString());
                
                // return the profile
                return profile;
            }

            // errors occured, return null
            return null;
        }

        /// <summary>
        /// Parses the backup locations from the given xml document. If an error occurs it
        /// will be printed out and null will be returned. Else a list of valid backup locations is returned.
        /// </summary>
        /// <param name="doc">the xml document to parse from</param>
        /// <returns>valid backup locations or null (when error)</returns>
        private IList<BackupLocation> ParseBackupLocations(XDocument doc)
        {
            // result list
            IList<BackupLocation> locs = new List<BackupLocation>();
            
            // check if there are backup locations defined
            if (doc.XPathSelectElement("/backup_profile/backup_location") == null)
            {
                ConsoleWriter.WriteErrorMessage(Lang.ErrorXmlMissingBackupLocations);
                return null;
            }
            
            // all backup locations specified in the xml
            IEnumerable<XElement> xmlLocs = doc.XPathSelectElements("/backup_profile/backup_location");
            
            // go through each BackupLocation-entry, convert it to an program-internal object
            // and add this one to the created list
            foreach (XElement xmlLoc in xmlLocs)
            {
                // check for errors,
                // if any error occuss the returned list is filled with error messages
                IList<string> xmlValidationErrors = checkForValidXmlElements(xmlLoc);
                
                // print errors, return null
                if (xmlValidationErrors.Count > 0)
                {
                    ConsoleWriter.WriteErrorMessage(Lang.ErrorXmlNodes);
                    ConsoleWriter.WriteErrorMessage(Lang.ErrorMessage);
                    foreach (var errorMessage in xmlValidationErrors)
                    {
                        ConsoleWriter.WriteErrorDetails(errorMessage);
                    }
                    return null;
                }
                
                // no errors, add backup location to internal backup profile
                locs.Add(convertXml(xmlLoc));
            }

            // no error while parsing, return the result list
            return locs;
        }

        /// <summary>
        /// Check if the XML representation of the given backup location is valid.
        /// Especially check if the source, destination and exclude paths are valid and existing.
        /// 
        /// Returns a list with the errors that occured while checking the xml element. If no errors occured the
        /// list ist empty.
        /// </summary>
        /// <param name="xmlLoc">the xml representation of one backup location</param>
        /// <returns>a list with errors while checking</returns>
        private IList<string> checkForValidXmlElements(XElement xmlLoc)
        {
            // result list, might be empty when everything is valid
            IList<string> errorMessages = new List<string>();
            
            // check source and dest path
            bool validPath = xmlLoc.Element("src") != null && Directory.Exists(xmlLoc.Element("src")?.Value);
            bool validDest = xmlLoc.Element("dest") != null && Directory.Exists(xmlLoc.Element("dest")?.Value);

            // add messages for invalid src and dest path into result collection if needed
            if (!validPath)
            {
                errorMessages.Add(Lang.ErrorXmlSrc);
            }
            
            if (!validDest)
            {
                errorMessages.Add(Lang.ErrorXmlDest);
            }

            // check exclude paths
            if (xmlLoc.Element("exclude") != null)
            {
                // check if the exclude element contains valid paths
                if (xmlLoc.Element("exclude")?.Elements("path") == null)
                {
                    // no paths inside exclude element
                    errorMessages.Add(Lang.ErrorXmlExcludeMissingPaths);
                }
                else
                {
                    // check each path for validity
                    IEnumerable<XElement> excludePaths = xmlLoc.Element("exclude").Elements("path");
                    foreach (var excPath in excludePaths)
                    {
                        if (!Directory.Exists(excPath.Value))
                        {
                            // invalid path
                            errorMessages.Add(string.Format(Lang.ErrorXmlExcludeInvalidPath, excPath));
                            break;
                        }
                    }
                }
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