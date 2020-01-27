using System;
using Backup.Xml;

namespace Backup.Start
{
    class Start
    {
        static void Main(string[] args)
        {
            BackupProfileConverter.LoadBackupProfile("Resources/test.xml");
        }
    }
}