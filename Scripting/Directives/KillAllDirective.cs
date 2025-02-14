using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scripting.Directives
{
    public class KillAllDirective
    {
        public static void KillAll()
        {
            string filePath = "commands.txt"; // Path to your text file

            try
            {
                if (!File.Exists(filePath))
                {
                    Console.WriteLine("File not found: " + filePath);
                    return;
                }

                string[] lines = File.ReadAllLines(filePath);
                bool killAllMode = false;

                foreach (string line in lines)
                {
                    if (line.StartsWith("KillAll::"))
                    {
                        killAllMode = true;
                        Console.WriteLine("KillAll:: directive found. Terminating non-essential processes...");
                        KillNonEssentialProcesses();
                        killAllMode = false;
                        continue; // Skip to the next command
                    }

                    // Add additional directive handling logic here.
                    if (killAllMode)
                    {
                        Console.WriteLine("Processing next directive: " + line);
                        // Implement logic for subsequent commands here.
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static void KillNonEssentialProcesses()
        {
            // Define a list of essential process names (case insensitive)
            string[] essentialProcesses = new[]
            {
                "explorer", "winlogon", "csrss", "lsass", "services", "svchost", "smss", "taskmgr", "dwm"
            };

            try
            {
                Process[] allProcesses = Process.GetProcesses();

                foreach (var process in allProcesses)
                {
                    try
                    {
                        // Check if the process is not in the essential list
                        if (!essentialProcesses.Contains(process.ProcessName, StringComparer.OrdinalIgnoreCase))
                        {
                            Console.WriteLine($"Terminating process: {process.ProcessName} (ID: {process.Id})");
                            process.Kill();
                            process.WaitForExit();
                            Console.WriteLine($"Process {process.ProcessName} terminated.");
                        }
                    }
                    catch
                    {
                        // Ignore exceptions for processes that cannot be accessed or terminated
                        Console.WriteLine($"Unable to terminate process: {process.ProcessName}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while terminating non-essential processes: {ex.Message}");
            }
        }
    }
}
