using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Backup.Utils;
using Backup.Xml;

namespace Backup.Start
{
    class Start
    {
        static void Main(string[] args)
        {
            // ACHTUNG: ConsoleWriter nutzen anstatt direkt Console.WriteLine

            // Dauerschleife, damit ggf. mehrere Durchgänge an einem Stück möglich sind
            // => Benutzer wird nach jedem Durchgang gefragt, ob fortfahren oder beenden
            while (true)
            {
                // Backup-Profil auswählen
                BackupProfile profile = SelectBackupProfile();

                // falls gültiges Profil ausgewählt, Backup durchführen
                if (profile != null)
                {
                    // Nachricht
                    ConsoleWriter.WriteWithColor("Starte Backup ...", ConsoleColor.White);
                    
                    // Backup
                    BackupRunner.RunBackup(profile);
                }
                
                // Nachfrage, ob noch ein Backup-Durchgang
                ConsoleWriter.WriteWithColor("Soll noch ein Backup durchgeführt werden? [J]/[beliebige andere Taste]", 
                                             ConsoleColor.Cyan);
                string input = Console.ReadLine();
                if (input == null || !input.ToLower().Equals("j"))
                {
                    // abbrechen
                    break;
                }
            }
        }

        private static BackupProfile SelectBackupProfile()
        {
            // Start-Nachricht
            ConsoleWriter.WriteWithColor("Willkommen beim Backup! " +
                                         "Bitte die Zahl des auszuführenden Backup-Profils eingeben:", 
                                         ConsoleColor.Cyan);

            // alle Profile-Pfade in Liste speichern
            IList<string> profilePaths = new List<string>();
            foreach (string profile in Directory.GetFiles("../backup_profiles"))
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
                    ConsoleWriter.WriteWithColor("Ungültige Eingabe! Bitte eine der angegebenen Profile durch " +
                                                 "Eingabe der vorangestellten Nummer wählen!", ConsoleColor.Red);
                    
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