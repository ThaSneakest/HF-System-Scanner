using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scripting.Directives
{
    public class PowershellScriptBuilderDirective
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
                StringBuilder scriptBuilder = null;

                foreach (string line in lines)
                {
                    if (line.StartsWith("StartPowershell::"))
                    {
                        Console.WriteLine("StartPowershell:: directive found. Beginning PowerShell script collection.");
                        scriptBuilder = new StringBuilder();
                        continue;
                    }

                    if (line.StartsWith("EndPowershell::"))
                    {
                        if (scriptBuilder == null)
                        {
                            Console.WriteLine("Warning: EndPowershell:: found without a matching StartPowershell::.");
                        }
                        else
                        {
                            Console.WriteLine("EndPowershell:: directive found. Executing collected PowerShell script.");
                            ExecutePowershellScript(scriptBuilder.ToString());
                            scriptBuilder = null;
                        }
                        continue;
                    }

                    if (scriptBuilder != null)
                    {
                        scriptBuilder.AppendLine(line);
                    }
                }

                if (scriptBuilder != null)
                {
                    Console.WriteLine("Warning: StartPowershell:: directive found without a matching EndPowershell::. Script ignored.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static void ExecutePowershellScript(string script)
        {
            try
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{script}\"",
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
