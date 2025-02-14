using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scripting.Directives
{
    public class UnlockDirective
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
                    if (line.StartsWith("Unlock::"))
                    {
                        string targetPath = line.Substring("Unlock::".Length).Trim();
                        if (!string.IsNullOrEmpty(targetPath))
                        {
                            Console.WriteLine($"Unlock:: directive found. Unlocking: {targetPath}");
                            UnlockFileOrFolder(targetPath);
                        }
                        else
                        {
                            Console.WriteLine("Unlock:: directive found, but no file or folder path was specified.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static void UnlockFileOrFolder(string path)
        {
            try
            {
                if (!File.Exists(path) && !Directory.Exists(path))
                {
                    Console.WriteLine($"File or folder not found: {path}");
                    return;
                }

                Console.WriteLine($"Attempting to unlock: {path}");

                // Use handle.exe to list and unlock handles (requires Sysinternals handle tool)
                string handleExePath = @"C:\Path\To\Handle.exe"; // Update this path to where handle.exe is located
                if (!File.Exists(handleExePath))
                {
                    Console.WriteLine("handle.exe not found. Please ensure it is installed and the path is correct.");
                    return;
                }

                ProcessStartInfo processStartInfo = new ProcessStartInfo
                {
                    FileName = handleExePath,
                    Arguments = $"-p {path} -accepteula",
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

                    if (!string.IsNullOrEmpty(output))
                    {
                        Console.WriteLine("Handle Output:");
                        Console.WriteLine(output);
                    }

                    if (!string.IsNullOrEmpty(error))
                    {
                        Console.WriteLine("Handle Error:");
                        Console.WriteLine(error);
                    }

                    if (process.ExitCode == 0)
                    {
                        Console.WriteLine($"Successfully unlocked: {path}");
                    }
                    else
                    {
                        Console.WriteLine($"Failed to unlock: {path}. Exit code: {process.ExitCode}");
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"Access denied: Unable to unlock {path}. Run as administrator.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error unlocking {path}: {ex.Message}");
            }
        }
    }
}
