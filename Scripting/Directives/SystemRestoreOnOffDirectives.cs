using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scripting.Directives
{
    public class SystemRestoreOnOffDirectives
    {
        public static void ProcessDirectives()
        {
            string filePath = "commands.txt"; // Path to the directives text file

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
                    if (line.StartsWith("SystemRestoreOn::"))
                    {
                        Console.WriteLine("SystemRestoreOn:: directive found. Enabling System Restore.");
                        ToggleSystemRestore(true);
                    }
                    else if (line.StartsWith("SystemRestoreOff::"))
                    {
                        Console.WriteLine("SystemRestoreOff:: directive found. Disabling System Restore.");
                        ToggleSystemRestore(false);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static void ToggleSystemRestore(bool enable)
        {
            try
            {
                string operation = enable ? "on" : "off";
                Console.WriteLine($"Executing command to turn System Restore {operation}.");

                // Use sc.exe to start/stop the System Restore service
                ProcessStartInfo processStartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/C sc config srservice start= {operation}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = new Process { StartInfo = processStartInfo })
                {
                    process.Start();
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    if (process.ExitCode == 0)
                    {
                        Console.WriteLine($"System Restore {operation} completed successfully.");
                    }
                    else
                    {
                        Console.WriteLine($"Error turning System Restore {operation}: {error}");
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"Access denied: Unable to modify System Restore. Run as administrator.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error modifying System Restore: {ex.Message}");
            }
        }
    }
}
