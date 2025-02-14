using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Registry;
using Wildlands_System_Scanner.Scripting;

namespace Wildlands_System_Scanner
{
    public class ScriptHandler
    {
        public static void SDelete()
        {
            string frst = "FRST"; // Replace with the actual value of $FRST in AutoIt
            string deleted = "Files deleted"; // Replace with the actual value of $DELETED in AutoIt
            string reboot2 = "System will reboot"; // Replace with the actual value of $REBOOT2 in AutoIt
            string reboot3 = "Rebooting now..."; // Replace with the actual value of $REBOOT3 in AutoIt

            // Display message box
            Console.WriteLine($"{frst} {deleted}\n\n{reboot2}\n{reboot3}");

            // Generate a random number between 100 and 999
            Random random = new Random();
            int rand = random.Next(100, 1000);

            // Move the current executable to a temporary location
            string scriptDir = AppDomain.CurrentDomain.BaseDirectory;
            string scriptName = AppDomain.CurrentDomain.FriendlyName;
            string tempDir = Path.GetTempPath();
            string tempFileName = Path.Combine(tempDir, $"FRST{rand}.TEMP");

            try
            {
                File.Move(Path.Combine(scriptDir, scriptName), tempFileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error moving file: {ex.Message}");
            }

            // Write registry entries
            string cDrive = "C:"; // Replace with actual value of $C if different
            string frstDir = Path.Combine(cDrive, "frst");

            if (Directory.Exists(frstDir))
            {
                RegistryUtilsScript.WriteRegistryRunOnce("*Removed", $"cmd /c rd /q/s \"{frstDir}\"");
            }

            RegistryUtilsScript.WriteRegistryRunOnce("*DelTemp", $"cmd /c DEL /F /Q /A \"{tempFileName}\"");

            // Reboot system
            Process.Start("shutdown", "/r /t 0");

            // Exit application
            Environment.Exit(0);
        }
    }
}
