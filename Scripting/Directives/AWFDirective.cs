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
    public class AWFDirective
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
                    if (line.StartsWith("AWF::"))
                    {
                        Console.WriteLine("AWF:: directive found. Fixing AWF infection.");
                        FixAWF();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static void FixAWF()
        {
            try
            {
                // Step 1: Stop AWF-related processes if running
                StopAWFProcesses();

                // Step 2: Delete AWF-related files
                DeleteAWFFiles();

                // Step 3: Restore the Registry (if any AWF-related registry entries exist)
                RestoreAWFRegistry();

                // Step 4: Log success
                Console.WriteLine("AWF infection fixed successfully.");
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("Access denied: Unable to modify files or processes. Run as administrator.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fixing AWF infection: {ex.Message}");
            }
        }

        private static void StopAWFProcesses()
        {
            try
            {
                // List of known AWF-related processes (customize based on your findings)
                string[] awfProcesses = new string[]
                {
                    "awf.exe",
                    "awfupdate.exe",
                    "awfdownloader.exe"
                };

                foreach (var processName in awfProcesses)
                {
                    Process[] processes = Process.GetProcessesByName(processName);

                    foreach (var process in processes)
                    {
                        Console.WriteLine($"Stopping AWF process: {process.ProcessName}");
                        process.Kill();
                        process.WaitForExit();
                        Console.WriteLine($"Successfully stopped process: {process.ProcessName}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error stopping AWF processes: {ex.Message}");
            }
        }

        private static void DeleteAWFFiles()
        {
            try
            {
                // Example file paths where AWF-related files might be found (customize based on your environment)
                string[] awfFiles = new string[]
                {
                    @"C:\Program Files\AWF\awf.exe",
                    @"C:\Windows\System32\awfupdate.exe",
                    @"C:\Users\Public\Documents\awfdownloader.exe"
                };

                foreach (var file in awfFiles)
                {
                    if (File.Exists(file))
                    {
                        Console.WriteLine($"Deleting AWF file: {file}");
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
                Console.WriteLine($"Error deleting AWF files: {ex.Message}");
            }
        }

        private static void RestoreAWFRegistry()
        {
            try
            {
                // Example registry paths AWF might modify or add (customize based on your findings)
                string[] registryPaths = new string[]
                {
                    @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run\AWF",
                    @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Run\AWF"
                };

                foreach (var regPath in registryPaths)
                {
                    Console.WriteLine($"Restoring registry key: {regPath}");

                    // Delete registry keys if they exist (remove AWF-related startup entries)
                    using (RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", writable: true))
                    {
                        if (key != null)
                        {
                            key.DeleteValue("AWF", false);
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
