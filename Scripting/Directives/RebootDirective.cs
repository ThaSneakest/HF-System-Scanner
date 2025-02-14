using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scripting.Directives
{
    public class RebootDirective
    {
        private static bool rebootAfterFix = false;

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
                    if (line.StartsWith("Reboot::"))
                    {
                        Console.WriteLine("Reboot:: directive found. WSS will reboot after the fix script finishes.");
                        rebootAfterFix = true;
                        continue;
                    }

                    // Add additional directive handling here.
                }

                // Simulate running the fix script
                RunFixScript();

                // Handle reboot if the directive is set
                if (rebootAfterFix)
                {
                    ScheduleReboot();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static void RunFixScript()
        {
            Console.WriteLine("Running fix script...");
            // Simulate fix script processing
            System.Threading.Thread.Sleep(2000); // Simulate time taken for fixes
            Console.WriteLine("Fix script completed.");
        }

        private static void ScheduleReboot()
        {
            try
            {
                Console.WriteLine("Rebooting the system...");
                Process.Start(new ProcessStartInfo
                {
                    FileName = "shutdown",
                    Arguments = "/r /t 0", // Reboot immediately
                    CreateNoWindow = true,
                    UseShellExecute = false
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to reboot the system: {ex.Message}");
            }
        }
    }
}
