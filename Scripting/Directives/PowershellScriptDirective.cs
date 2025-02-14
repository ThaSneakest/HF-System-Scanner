using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scripting.Directives
{
    public class PowershellScriptDirective
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
                    if (line.StartsWith("PowershellScript::"))
                    {
                        string scriptPath = line.Substring("PowershellScript::".Length).Trim();
                        if (!string.IsNullOrEmpty(scriptPath))
                        {
                            Console.WriteLine($"PowershellScript:: directive found. Running script: {scriptPath}");
                            RunPowershellScript(scriptPath);
                        }
                        else
                        {
                            Console.WriteLine("PowershellScript:: directive found, but no script path was specified.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static void RunPowershellScript(string scriptPath)
        {
            try
            {
                if (!File.Exists(scriptPath))
                {
                    Console.WriteLine($"Script file not found: {scriptPath}");
                    return;
                }

                ProcessStartInfo processStartInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -ExecutionPolicy Bypass -File \"{scriptPath}\"",
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
                        Console.WriteLine("PowerShell Output:");
                        Console.WriteLine(output);
                    }

                    if (!string.IsNullOrEmpty(error))
                    {
                        Console.WriteLine("PowerShell Error:");
                        Console.WriteLine(error);
                    }

                    Console.WriteLine($"PowerShell script finished with exit code {process.ExitCode}.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing PowerShell script: {ex.Message}");
            }
        }
    }
}
