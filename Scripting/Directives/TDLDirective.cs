using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scripting.Directives
{
    public class TDLDirective
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
                    if (line.StartsWith("TDL::"))
                    {
                        string infectedDriverPath = line.Substring("TDL::".Length).Trim();
                        if (!string.IsNullOrEmpty(infectedDriverPath))
                        {
                            Console.WriteLine($"TDL:: directive found. Attempting to disinfect infected driver: {infectedDriverPath}");
                            DisinfectTDL(infectedDriverPath);
                        }
                        else
                        {
                            Console.WriteLine("TDL:: directive found, but no driver path specified.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static void DisinfectTDL(string infectedDriverPath)
        {
            try
            {
                // Check if the infected driver file exists
                if (!File.Exists(infectedDriverPath))
                {
                    Console.WriteLine($"Infected driver file not found: {infectedDriverPath}");
                    return;
                }

                // Step 1: Stop the infected driver (if running)
                StopInfectedDriver(infectedDriverPath);

                // Step 2: Delete the infected driver
                Console.WriteLine($"Deleting infected driver: {infectedDriverPath}");
                File.Delete(infectedDriverPath);

                // Step 3: Optionally, run TDL disinfecting tool (like TDSSKiller)
                RunTDSSKiller();

                // Step 4: Log success
                Console.WriteLine($"Infected driver {infectedDriverPath} has been disinfected successfully.");
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("Access denied: Unable to modify the infected driver. Run as administrator.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error disinfecting TDL infection: {ex.Message}");
            }
        }

        private static void StopInfectedDriver(string driverPath)
        {
            try
            {
                // Use sc.exe to stop the driver service (if running)
                string driverName = Path.GetFileNameWithoutExtension(driverPath);
                ProcessStartInfo processStartInfo = new ProcessStartInfo
                {
                    FileName = "sc.exe",
                    Arguments = $"stop {driverName}",
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
                        Console.WriteLine($"Driver {driverName} stopped successfully.");
                    }
                    else
                    {
                        Console.WriteLine($"Error stopping driver {driverName}: {error}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error stopping infected driver: {ex.Message}");
            }
        }

        private static void RunTDSSKiller()
        {
            try
            {
                // Ensure TDSSKiller or similar tool is available (download and place in the tool path)
                string tdsskillerPath = @"C:\Tools\TDSSKiller.exe"; // Update the path to TDSSKiller
                if (File.Exists(tdsskillerPath))
                {
                    Console.WriteLine("Running TDSSKiller to disinfect TDL infection...");

                    ProcessStartInfo processStartInfo = new ProcessStartInfo
                    {
                        FileName = tdsskillerPath,
                        Arguments = "/quarantine", // Additional arguments for scanning and quarantining
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
                            Console.WriteLine("TDSSKiller ran successfully.");
                            Console.WriteLine(output);
                        }
                        else
                        {
                            Console.WriteLine("Error running TDSSKiller:");
                            Console.WriteLine(error);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("TDSSKiller not found. Please ensure it is downloaded and placed at the correct path.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error running TDSSKiller: {ex.Message}");
            }
        }
    }
}
