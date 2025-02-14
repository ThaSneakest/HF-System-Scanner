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
    public class VundoDirective
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
                    if (line.StartsWith("Vundo::"))
                    {
                        Console.WriteLine("Vundo:: directive found. Fixing Vundo infection in startup files.");
                        FixVundo();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static void FixVundo()
        {
            try
            {
                // Step 1: Stop Vundo-related processes if running
                StopVundoProcesses();

                // Step 2: Restore or rename infected startup files
                RestoreStartupFiles();

                // Step 3: Clean registry entries associated with Vundo
                CleanVundoRegistry();

                // Step 4: Log success
                Console.WriteLine("Vundo infection fixed successfully.");
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("Access denied: Unable to modify files or processes. Run as administrator.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fixing Vundo infection: {ex.Message}");
            }
        }

        private static void StopVundoProcesses()
        {
            try
            {
                // List of known Vundo-related processes to stop (customize based on infection behavior)
                string[] vundoProcesses = new string[]
                {
                    "vundo.exe",
                    "vundoinf.exe",
                    "fakeprocess.exe"
                };

                foreach (var processName in vundoProcesses)
                {
                    Process[] processes = Process.GetProcessesByName(processName);

                    foreach (var process in processes)
                    {
                        Console.WriteLine($"Stopping Vundo process: {process.ProcessName}");
                        process.Kill();
                        process.WaitForExit();
                        Console.WriteLine($"Successfully stopped process: {process.ProcessName}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error stopping Vundo processes: {ex.Message}");
            }
        }

        private static void RestoreStartupFiles()
        {
            try
            {
                // Example paths for startup files that might be infected by Vundo
                string[] infectedFiles = new string[]
                {
                    @"C:\Users\<username>\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup\infectedStartup.lnk",
                    @"C:\Windows\System32\infectedStartupFile.exe"
                };

                // Example safe filenames to rename back to
                string[] safeFileNames = new string[]
                {
                    @"C:\Users\<username>\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup\legitStartup.lnk",
                    @"C:\Windows\System32\legitStartupFile.exe"
                };

                for (int i = 0; i < infectedFiles.Length; i++)
                {
                    string infectedFile = infectedFiles[i];
                    string safeFileName = safeFileNames[i];

                    if (File.Exists(infectedFile))
                    {
                        Console.WriteLine($"Restoring file: {infectedFile} to {safeFileName}");
                        File.Move(infectedFile, safeFileName);
                        Console.WriteLine($"Successfully restored {infectedFile} to {safeFileName}");
                    }
                    else
                    {
                        Console.WriteLine($"Infected file not found: {infectedFile}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error restoring startup files: {ex.Message}");
            }
        }

        private static void CleanVundoRegistry()
        {
            try
            {
                // Example registry entries Vundo might modify (customize as needed)
                string[] registryPaths = new string[]
                {
                    @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run\Vundo",
                    @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Run\Vundo"
                };

                foreach (var regPath in registryPaths)
                {
                    Console.WriteLine($"Cleaning registry key: {regPath}");

                    using (RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", writable: true))
                    {
                        if (key != null)
                        {
                            key.DeleteValue("Vundo", false);
                            Console.WriteLine($"Successfully deleted registry entry: {regPath}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cleaning registry entries: {ex.Message}");
            }
        }
    }
}
