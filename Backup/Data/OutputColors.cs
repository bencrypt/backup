using System;

namespace Backup
{
    public class OutputColors
    {
        public static readonly ConsoleColor MainMessages = ConsoleColor.Cyan;
        public static readonly ConsoleColor BackupLocations = ConsoleColor.White;
        
        public static readonly ConsoleColor Changed = ConsoleColor.Gray;
        public static readonly ConsoleColor Add = ConsoleColor.Green;
        public static readonly ConsoleColor Remove = ConsoleColor.Red;
        public static readonly ConsoleColor Update = ConsoleColor.Yellow;

        public static readonly ConsoleColor Success = ConsoleColor.Green;
        
        public static readonly ConsoleColor Error = ConsoleColor.Red;
        public static readonly ConsoleColor ErrorDetails = ConsoleColor.Yellow;
        
        public static readonly ConsoleColor LogInfo = ConsoleColor.Magenta;
    }
}