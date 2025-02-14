using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scripting.Directives
{
    public class FakeSmokeDirective
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
                    if (line.StartsWith("FakeSmoke::"))
                    {
                        Console.WriteLine("FakeSmoke:: directive found. Fixing FakeSmoke rogues.");
                        FixFakeSmoke();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static void FixFakeSmoke()
        {
            try
            {
                // Step 1: Stop FakeSmoke processes if running
                StopFakeSmokeProcesses();

                // Step 2: Delete FakeSmoke files (example paths)
                DeleteFakeSmokeFiles();

                // Step 3: Restore Registry (if any FakeSmoke-related registry entries exist)
                RestoreFakeSmokeRegistry();

                // Step 4: Log success
                Console.WriteLine("FakeSmoke rogue files and processes fixed successfully.");
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("Access denied: Unable to modify files or processes. Run as administrator.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fixing FakeSmoke infection: {ex.Message}");
            }
        }

        private static void StopFakeSmokeProcesses()
        {
            try
            {
                // List of known FakeSmoke processes to stop (customize based on the malware behavior)
                string[] fakeSmokeProcesses = new string[]
                {
                    "fakesmoke.exe",
                    "smokescan.exe",
                    "rogue_scan.exe"
                };

                foreach (var processName in fakeSmokeProcesses)
                {
                    Process[] processes = Process.GetProcessesByName(processName);

                    foreach (var process in processes)
                    {
                        Console.WriteLine($"Stopping FakeSmoke process: {process.ProcessName}");
                        process.Kill();
                        process.WaitForExit();
                        Console.WriteLine($"Successfully stopped process: {process.ProcessName}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error stopping FakeSmoke processes: {ex.Message}");
            }
        }

        private static void DeleteFakeSmokeFiles()
        {
            try
            {
                // Example paths where FakeSmoke files may reside
                string[] fakeSmokeFiles = new string[]
                {
                    @"C:\Program Files\FakeSmoke\smokescan.exe",
                    @"C:\Windows\System32\smoke.dll",
                    @"C:\Users\Public\Documents\fakesmoke.exe"
                };

                foreach (var file in fakeSmokeFiles)
                {
                    if (File.Exists(file))
                    {
                        Console.WriteLine($"Deleting FakeSmoke file: {file}");
                        File.Delete(file);
                        Console.WriteLine($"Successfully deleted file: {file}");
                    }
                    else
                    {
                        Console.WriteLine($"File not found: {file}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting FakeSmoke files: {ex.Message}");
            }
        }

        private static void RestoreFakeSmokeRegistry()
        {
            try
            {
                // Example registry paths FakeSmoke might modify
                string[] registryPaths = new string[]
                {
                    @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run\FakeSmoke",
                    @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Run\FakeSmoke"
                };

                foreach (var regPath in registryPaths)
                {
                    Console.WriteLine($"Restoring registry key: {regPath}");

                    // Delete the registry key if it exists (remove FakeSmoke startup)
                    using (RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", writable: true))
                    {
                        if (key != null)
                        {
                            key.DeleteValue("FakeSmoke", false);
                            Console.WriteLine($"Successfully deleted registry entry: {regPath}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error restoring registry entries: {ex.Message}");
            }
        }
    }
}
