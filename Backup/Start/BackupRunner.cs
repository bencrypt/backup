using System;
using System.Collections.Generic;
using System.IO;
using Backup.Utils;

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
                bool updated = BackupDirectoryRecursively(
                                        backupLocation.Path, backupLocation.Destination, backupLocation.ExcludePaths);

                // Informations-Flag für mindestens eine aktualisierte Datei setzen, falls noch nicht getan
                if (!atLeastOneNewer && updated)
                {
                    atLeastOneNewer = true;
                }
            }

            // Kontroll-Nachricht am Ende des Backups
            if (!atLeastOneNewer)
            {
                ConsoleWriter.WriteWithColor("Backup durchgeführt! Alle Dateien waren bereits aktuell.", 
                                             ConsoleColor.Green);
            }
            else
            {
                ConsoleWriter.WriteWithColor("Backup durchgeführt! Alle Dateien wurden auf den aktuellen Stand gebracht.",
                                             ConsoleColor.Green);
            }
        }

        private static bool BackupDirectoryRecursively(string srcDir, string destDir, IList<string> excludePaths)
        {
            // erstelle das Zielverzeichnis, falls notwendig
            if (!Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
            }

            // sichere alle neueren Dateien aus dem Quellverzeichnis ins Zielverzeichnis,
            // falls nicht excluded
            bool atLeastOneNewer = false;
            foreach (string srcFile in Directory.GetFiles(srcDir))
            {
                if (!excludePaths.Contains(srcFile))
                {
                    // Kontroll-Nachricht
                    //Logger.LogInfo("Überprüfe {0}", srcFile);
                    
                    // Dateien sichern
                    bool saved = BackupFileIfNewer(srcFile, destDir);

                    // falls mindestens eine Datei aktualisiert => Flag auf true setzen
                    if (!atLeastOneNewer && saved)
                    {
                        atLeastOneNewer = true;
                    }
                }
            }
            
            // alle nicht mehr vorhandenen Dateien und Ordner löschen
            bool atLeastOneDeleted = DeleteFilesAndSubdirsNotContainedAnymore(srcDir, destDir);

            // gehe alle Sub-Verzeichnisse durch, falls nicht excluded
            foreach (string subDir in Directory.GetDirectories(srcDir))
            {
                if (!excludePaths.Contains(subDir))
                {
                    // kombiniere das Zielverzeichnis mit dem Sub-Ordner-Namen (als FileName abfragen!)
                    // => Backup-Pfad für den Sub-Ordner
                    string destOfSubDir = Path.Combine(destDir, Path.GetFileName(subDir));
                    
                    // sichere das Sub-Verzeichnis
                    BackupDirectoryRecursively(subDir, destOfSubDir, excludePaths);
                }
            }
            
            // gebe zurück, ob mindestens eine Datei aktualisiert wurde
            return (atLeastOneNewer || atLeastOneDeleted);
        }

        private static bool BackupFileIfNewer(string srcFile, string destDir)
        {
            // kombiniere das Zielverzeichnis mit dem Dateinamen
            // => Backup-Pfad für die Datei
            string destOfFile = Path.Combine(destDir, Path.GetFileName(srcFile));
            
            // sichere die Datei, falls noch nicht vorhanden oder neuer
            if (!File.Exists(destOfFile))
            {
                // Kontroll-Nachricht
                ConsoleWriter.WriteWithColor("Neu erstellte Datei '{0}' ...", ConsoleColor.White, srcFile, destOfFile);
            }
            else if (File.GetLastWriteTime(srcFile) > File.GetLastWriteTime(destOfFile))
            {
                // Kontroll-Nachricht
                //Logger.LogInfo("Sichere:\n{0}\n=> {1}", srcFile, destOfFile);
                ConsoleWriter.WriteWithColor("Aktualisierte Datei '{0}' ...", ConsoleColor.White, srcFile, destOfFile);
                
                // Datei sichern
                File.Copy(srcFile, destOfFile, true);
                
                // true zurückgeben, da aktuellere Datei gesichert
                return true;
            }

            // false zurückgeben, da keine aktuellere Datei gesichert werden musste
            return false;
        }

        private static bool DeleteFilesAndSubdirsNotContainedAnymore(string srcDir, string destDir)
        {
            // Ergebnis-Flag
            bool atLeastOneDeleted = false;
            
            // lösche alle Dateien im Zielverzeichnis, die nicht (mehr) im Quellverzeichnis
            // existieren
            foreach (string destFile in Directory.GetFiles(destDir))
            {
                // ursprünglichen Quellpfad ermitteln
                string shouldBeSrcFile = Path.Combine(srcDir, Path.GetFileName(destFile));
                
                // lösche die Zieldatei, falls keine Datei (mehr) unter Quellpfad
                if (!File.Exists(shouldBeSrcFile))
                {
                    // Kontroll-Nachricht
                    //Logger.LogInfo("Gelöschte Datei: {0}", shouldBeSrcFile);
                    ConsoleWriter.WriteWithColor("Gelöschte Datei '{0}' ...", ConsoleColor.White, shouldBeSrcFile);
                    
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
                
                // lösche die Zieldatei, falls kein Verzeichnis (mehr) unter Quellpfad
                if (!Directory.Exists(shouldBeSrcDir))
                {
                    // Kontroll-Nachricht
                    //Logger.LogInfo("Gelöschtes Verzeichnis: {0}", shouldBeSrcDir);
                    ConsoleWriter.WriteWithColor("Gelöschtes Verzeichnis '{0}' ...", ConsoleColor.White, shouldBeSrcDir);
                    
                    // Verzeichnis rekursiv löschen
                    Directory.Delete(destSubDir, true);
                    
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
    }
}