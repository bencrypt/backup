﻿using System;
using System.Collections.Generic;
using System.IO;
using Backup.Utils;
 using Backup.Resources;

namespace Backup.Start
{
    public class BackupRunner
    {
        public static void RunBackup(BackupProfile profile)
        {
            // sichere nacheinander die einzelnen BackupLocations
            bool atLeastOneNewer = false;
            foreach (BackupLocation backupLocation in profile.BackupLocations)
            {
                // Backup rekursiv für alle Verzeichnisse durchführen
                // => der Rückgabewert gibt an, ob mindestens eine Datei aktualisiert wurde
                ConsoleWriter.WriteWithColor(Lang.CheckBackupLocation, ConsoleColor.Gray, backupLocation.Path);
                bool updated = BackupDirectoryRecursively(
                        backupLocation, backupLocation.Path, backupLocation.Destination, backupLocation.ExcludePaths);

                // Informations-Flag für mindestens eine aktualisierte Datei setzen, falls noch nicht getan
                if (!atLeastOneNewer && updated)
                {
                    atLeastOneNewer = true;
                }
            }

            // Kontroll-Nachricht am Ende des Backups
            if (!atLeastOneNewer)
            {
                ConsoleWriter.WriteWithColor(Lang.SuccessNoUpdate, ConsoleColor.Green);
            }
            else
            {
                ConsoleWriter.WriteWithColor(Lang.SuccessUpdate, ConsoleColor.Green);
            }
        }

        private static bool BackupDirectoryRecursively(BackupLocation backupLocation, string srcDir, string destDir,
            IList<string> excludePaths)
        {
            // Ergebnis-Flag
            // => true, falls mindestens eine Datei oder ein Ordner aktualisiert/hinzugefügt/entfernt
            bool atLeastOneNewer = false;
            
            // erstelle das Zielverzeichnis, falls notwendig
            if (!Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
                atLeastOneNewer = true;
            }

            // sichere alle neueren Dateien aus dem Quellverzeichnis ins Zielverzeichnis,
            // falls nicht excluded
            foreach (string srcFile in Directory.GetFiles(srcDir))
            {
                if (!excludePaths.Contains(srcFile))
                {
                    // Kontroll-Nachricht
                    //Logger.LogInfo("Check {0}", srcFile);
                    
                    // Dateien sichern
                    bool saved = BackupFileIfNewer(backupLocation, srcFile, destDir);

                    // falls mindestens eine Datei aktualisiert => Flag auf true setzen
                    if (!atLeastOneNewer && saved)
                    {
                        atLeastOneNewer = true;
                    }
                }
            }
            
            // alle nicht mehr vorhandenen Dateien und Ordner löschen
            bool atLeastOneDeleted = DeleteFilesAndSubdirsNotContainedAnymore(backupLocation, srcDir, destDir);
            
            // Ergebnis-Flag aktualisieren
            if (!atLeastOneNewer && atLeastOneDeleted)
            {
                atLeastOneNewer = true;
            }

            // gehe alle Sub-Verzeichnisse durch, falls nicht excluded
            foreach (string subDir in Directory.GetDirectories(srcDir))
            {
                if (!excludePaths.Contains(subDir))
                {
                    // kombiniere das Zielverzeichnis mit dem Sub-Ordner-Namen (als FileName abfragen!)
                    // => Backup-Pfad für den Sub-Ordner
                    string destOfSubDir = Path.Combine(destDir, Path.GetFileName(subDir));
                    
                    // sichere das Sub-Verzeichnis
                    bool atLeastOneNewerInSubDir = BackupDirectoryRecursively(
                                                        backupLocation, subDir, destOfSubDir, excludePaths);
                    
                    // Ergebnis-Flag aktualisieren
                    if (!atLeastOneNewer && atLeastOneNewerInSubDir)
                    {
                        atLeastOneNewer = true;
                    }
                }
            }
            
            // gebe zurück, ob mindestens eine Datei aktualisiert wurde
            return (atLeastOneNewer || atLeastOneDeleted);
        }

        private static bool BackupFileIfNewer(BackupLocation backupLocation, string srcFile, string destDir)
        {
            string pathWithoutLocationPrefix = srcFile.Remove(0, backupLocation.Path.Length);
            
            // kombiniere das Zielverzeichnis mit dem Dateinamen
            // => Backup-Pfad für die Datei
            string destOfFile = Path.Combine(destDir, Path.GetFileName(srcFile));
            
            // sichere die Datei, falls noch nicht vorhanden oder neuer
            if (!File.Exists(destOfFile))
            {
                // Kontroll-Nachricht
                ConsoleWriter.WriteWithColor("+ '{0}'", ConsoleColor.White, pathWithoutLocationPrefix);

                // Datei sichern
                File.Copy(srcFile, destOfFile, true);
                
                // true zurückgeben, da Datei neu hinzugefügt
                return true;
            }
            else if (File.GetLastWriteTime(srcFile) > File.GetLastWriteTime(destOfFile))
            {
                // Kontroll-Nachricht
                //Logger.LogInfo("Sichere:\n{0}\n=> {1}", srcFile, destOfFile);
                ConsoleWriter.WriteWithColor("* '{0}'", ConsoleColor.White, pathWithoutLocationPrefix);
                
                // Ziel-Datei (falls schon vorhanden) als "normal" markieren, damit sie überschrieben werden kann
                // (z.B. sind git-Dateien schreibgeschützt, weshalb dieser Schritt vor dem Überschreiben
                //  notwendig ist)
                if (File.Exists(destOfFile))
                {
                    File.SetAttributes(destOfFile, FileAttributes.Normal);
                }
                
                // Datei sichern
                File.Copy(srcFile, destOfFile, true);
                
                // true zurückgeben, da aktuellere Datei gesichert
                return true;
            }

            // false zurückgeben, da keine aktuellere Datei gesichert werden musste
            return false;
        }

        private static bool DeleteFilesAndSubdirsNotContainedAnymore(BackupLocation backupLocation, string srcDir, string destDir)
        {
            // Ergebnis-Flag
            bool atLeastOneDeleted = false;
            
            // lösche alle Dateien im Zielverzeichnis, die nicht (mehr) im Quellverzeichnis
            // existieren
            foreach (string destFile in Directory.GetFiles(destDir))
            {
                // ursprünglichen Quellpfad ermitteln
                string shouldBeSrcFile = Path.Combine(srcDir, Path.GetFileName(destFile));
                string pathWithoutLocationPrefix = shouldBeSrcFile.Remove(0, backupLocation.Path.Length);
                
                // lösche die Zieldatei, falls keine Datei (mehr) unter Quellpfad
                if (!File.Exists(shouldBeSrcFile))
                {
                    // Kontroll-Nachricht
                    //Logger.LogInfo("Gelöschte Datei: {0}", shouldBeSrcFile);
                    ConsoleWriter.WriteWithColor("- '{0}'", ConsoleColor.White, pathWithoutLocationPrefix);
                    
                    // Dateiattribute auf "normal" setzen, damit die Datei gelöscht werden kann
                    // => z.B. sind einige git-Dateien read-only, weshalb man sie sonst nicht löschen kann
                    File.SetAttributes(destFile, FileAttributes.Normal);
                    
                    // Datei löschen
                    File.Delete(destFile);
                    
                    // Ergebnis-Flag aktualisieren
                    if (!atLeastOneDeleted)
                    {
                        atLeastOneDeleted = true;
                    }
                }
            }
            
            // lösche alle Sub-Verzeichnisse im Zielverzeichnis, die nicht (mehr) im Quellverzeichnis
            // existieren
            foreach (string destSubDir in Directory.GetDirectories(destDir))
            {
                // ursprünglichen Quellpfad ermitteln
                string shouldBeSrcDir = Path.Combine(srcDir, Path.GetFileName(destSubDir));
                string pathWithoutLocationPrefix = shouldBeSrcDir.Remove(0, backupLocation.Path.Length);
                
                // lösche die Zieldatei, falls kein Verzeichnis (mehr) unter Quellpfad
                if (!Directory.Exists(shouldBeSrcDir))
                {
                    // Kontroll-Nachricht
                    //Logger.LogInfo("Gelöschtes Verzeichnis: {0}", shouldBeSrcDir);
                    ConsoleWriter.WriteWithColor("- '{0}'", ConsoleColor.White, pathWithoutLocationPrefix);
                    
                    // Verzeichnis rekursiv löschen
                    // => eigene Methode, da ggf. Datei-Attribute geändert werden müssen
                    DeleteDirectoryRecursively(destSubDir);
                    
                    // Ergebnis-Flag aktualisieren
                    if (!atLeastOneDeleted)
                    {
                        atLeastOneDeleted = true;
                    }
                }
            }
            
            // Ergebnis-Flag zurückgeben
            return atLeastOneDeleted;
        }

        private static void DeleteDirectoryRecursively(string destDir)
        {
            // Verzeichnis-Attribute auf "normal" setzen, damit das Verzeichnis am Ende der Methode
            // gelöscht werden kann
            DirectoryInfo directoryInfo = new DirectoryInfo(destDir)
            {
                Attributes = FileAttributes.Normal
            };
            
            // Dateiattribute jeder Datei auf "normal" setzen, damit die Datei gelöscht werden kann, und
            // anschließend die Datei löschen
            // => z.B. sind einige git-Dateien read-only, weshalb man sie sonst nicht löschen kann
            foreach (string file in Directory.GetFiles(destDir))
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }
            
            // in jedem Sub-Verzeichnis wiederholen
            foreach (string subDir in Directory.GetDirectories(destDir))
            {
                DeleteDirectoryRecursively(subDir);
            }
            
            // Verzeichnis selbst löschen
            directoryInfo.Delete();
        }
    }
}