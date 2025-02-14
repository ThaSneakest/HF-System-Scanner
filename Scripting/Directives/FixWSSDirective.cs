using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scripting.Directives
{
    public class FixWSSDirective
    {
        private const string BackupDirectory = @"C:\WSS_Backup\";
        private const string ApplicationPath = @"C:\Program Files\WSS\WildlandsSystemScanner.exe";
        private const string BackupApplicationPath = BackupDirectory + "WildlandsSystemScanner.exe";
        private const string HashFilePath = BackupDirectory + "WSS_Hash.txt";

        public static void ProcessDirectives()
        {
            string filePath = "commands.txt"; // Path to your directives text file

            try
            {
                if (!File.Exists(filePath))
                {
                    Console.WriteLine("File not found: " + filePath);
                    return;
                }

                string[] lines = File.ReadAllLines(filePath);

                foreach (string line in lines)
                {
                    if (line.StartsWith("FixWSS::"))
                    {
                        Console.WriteLine("FixWSS:: directive found. Initiating self-repair process...");
                        VerifyAndRepair();
                        continue;
                    }

                    // Add additional directive handling logic here.
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static void VerifyAndRepair()
        {
            try
            {
                if (!File.Exists(ApplicationPath))
                {
                    Console.WriteLine("Application executable not found. Attempting to restore from backup...");
                    RestoreFromBackup();
                    return;
                }

                string currentHash = ComputeFileHash(ApplicationPath);
                string expectedHash = File.ReadAllText(HashFilePath);

                if (currentHash != expectedHash)
                {
                    Console.WriteLine("Integrity check failed. Application executable has been modified.");
                    RestoreFromBackup();
                }
                else
                {
                    Console.WriteLine("Integrity check passed. No action required.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during verification and repair: {ex.Message}");
            }
        }

        private static string ComputeFileHash(string filePath)
        {
            using (var sha256 = SHA256.Create())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    byte[] hashBytes = sha256.ComputeHash(stream);
                    return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        private static void RestoreFromBackup()
        {
            try
            {
                if (File.Exists(BackupApplicationPath))
                {
                    File.Copy(BackupApplicationPath, ApplicationPath, overwrite: true);
                    Console.WriteLine("Application restored from backup successfully.");
                }
                else
                {
                    Console.WriteLine("Backup not found. Unable to restore the application.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during restoration: {ex.Message}");
            }
        }
    }
}
