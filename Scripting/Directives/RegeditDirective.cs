using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scripting.Directives
{
    public class RegeditDirective
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
                    if (line.StartsWith("StartRegedit::"))
                    {
                        Console.WriteLine("StartRegedit:: directive found. Beginning registry script collection.");
                        scriptBuilder = new StringBuilder();
                        continue;
                    }

                    if (line.StartsWith("EndRegedit::"))
                    {
                        if (scriptBuilder == null)
                        {
                            Console.WriteLine("Warning: EndRegedit:: found without a matching StartRegedit::.");
                        }
                        else
                        {
                            Console.WriteLine("EndRegedit:: directive found. Executing collected registry script.");
                            ExecuteRegistryScript(scriptBuilder.ToString());
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
                    Console.WriteLine("Warning: StartRegedit:: directive found without a matching EndRegedit::. Script ignored.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static void ExecuteRegistryScript(string scriptContent)
        {
            string tempScriptPath = Path.Combine(Path.GetTempPath(), $"tempRegistryScript_{Guid.NewGuid()}.reg");

            try
            {
                // Save the script to a temporary file
                File.WriteAllText(tempScriptPath, scriptContent, Encoding.UTF8);
                Console.WriteLine($"Temporary registry script created: {tempScriptPath}");

                // Execute the script using reg.exe
                ProcessStartInfo processStartInfo = new ProcessStartInfo
                {
                    FileName = "reg.exe",
                    Arguments = $"import \"{tempScriptPath}\"",
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
                        Console.WriteLine("Registry script executed successfully.");
                        Console.WriteLine("Output:");
                        Console.WriteLine(output);
                    }
                    else
                    {
                        Console.WriteLine("Error executing registry script:");
                        Console.WriteLine(error);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing registry script: {ex.Message}");
            }
            finally
            {
                // Clean up temporary script file
                if (File.Exists(tempScriptPath))
                {
                    File.Delete(tempScriptPath);
                    Console.WriteLine($"Temporary registry script deleted: {tempScriptPath}");
                }
            }
        }
    }
}
