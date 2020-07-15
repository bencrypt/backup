using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using System.Threading;
using Backup.Resources;
using Backup.Utils;
using Backup.Xml;

namespace Backup.Start
{
    class Start
    {
        static void Main(string[] args)
        {
            // change to English for testing
            //Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en");

            string scriptDir = Path.GetFullPath(Assembly.GetExecutingAssembly().Location);
            string profileDir = Path.GetFullPath(Path.Combine(scriptDir, "..", "..", "backup_profiles"));
            
            // ACHTUNG: ConsoleWriter nutzen anstatt direkt Console.WriteLine

            // Dauerschleife, damit ggf. mehrere Durchgänge an einem Stück möglich sind
            // => Benutzer wird nach jedem Durchgang gefragt, ob fortfahren oder beenden
            while (true)
            {
                // prüfe, ob Ordner mit Backup-Profilen existiert
                if (!Directory.Exists(profileDir))
                {
                    // Fehlermeldung
                    ConsoleWriter.WriteWithColor(Lang.ErrorNotExistingProfile, ConsoleColor.Red, profileDir);
                        
                    // auf Eingabe warten, damit sich das Fenster nicht sofort schließt
                    ConsoleWriter.WriteWithColor(Lang.EndProgram, ConsoleColor.Red);
                    Console.ReadLine();
                    
                    // Programm beenden
                    Environment.Exit(1);
                }
                
                // Backup-Profil auswählen
                BackupProfile profile = SelectBackupProfile(profileDir);

                // falls gültiges Profil ausgewählt, Backup durchführen
                if (profile != null)
                {
                    // Nachricht
                    ConsoleWriter.WriteWithColor(Lang.StartBackup, ConsoleColor.White);
                    
                    // Backup
                    try
                    {
                        BackupRunner.RunBackup(profile);
                    }
                    catch (Exception e)
                    {
                        // Fehlermeldung
                        ConsoleWriter.WriteWithColor(Lang.StopBecauseOfError, ConsoleColor.Red);
                        ConsoleWriter.WriteWithColor(Lang.ErrorMessage, ConsoleColor.Red);
                        ConsoleWriter.WriteWithColor("{0}\n", ConsoleColor.Yellow, e.Message);
                        
                        // auf Eingabe warten, damit sich das Fenster nicht sofort schließt
                        ConsoleWriter.WriteWithColor(Lang.EndProgram, ConsoleColor.Cyan);
                        Console.ReadLine();
                    }
                }
                
                // Nachfrage, ob noch ein Backup-Durchgang
                ConsoleWriter.WriteWithColor(Lang.AnotherRun, ConsoleColor.Cyan);
                string input = Console.ReadLine();
                if (input == null || !input.ToLower().Equals("j"))
                {
                    // abbrechen
                    break;
                }
            }
        }

        private static BackupProfile SelectBackupProfile(string profileDir)
        {
            // Start-Nachricht
            ConsoleWriter.WriteWithColor(Lang.Start, ConsoleColor.Cyan);

            // alle Profile-Pfade in Liste speichern
            IList<string> profilePaths = new List<string>();
            foreach (string profile in Directory.GetFiles(profileDir))
            {
                profilePaths.Add(profile);
            }
            
            // alle Profile zur Auswahl anbieten
            for (int option = 1; option <= profilePaths.Count; option++)
            {
                ConsoleWriter.WriteWithColor("[{0}]: {1}", ConsoleColor.Cyan, 
                                             option, Path.GetFileNameWithoutExtension(profilePaths[option-1]));
            }

            // Auswahl verarbeiten und (falls gültig) Profil laden
            // => solange durchführen, bis gültige Auswahl
            int input = -1;
            while (input == -1)
            {
                // Input entgegennehmen
                bool parsed = int.TryParse(Console.ReadLine(), out input);
                if (!parsed || input <= 0 || input > profilePaths.Count)
                {
                    // Fehlermeldung
                    ConsoleWriter.WriteWithColor(Lang.InvalidNumber, ConsoleColor.Red);
                    
                    // Input zurück auf -1 setzen
                    input = -1;
                }
                else
                {
                    // (gültiges) Profil erstellen und zurückgeben
                    return BackupProfileConverter.LoadBackupProfile(profilePaths[input - 1]);
                }
            }

            // pro-forma null zurückgeben, was aber gar nicht erreicht wird, da 
            // oben immer auf eine gültige Eingabe gewartet wird
            return null;
        }

        
    }
}