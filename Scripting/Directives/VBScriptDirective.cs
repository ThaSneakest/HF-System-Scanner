using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scripting.Directives
{
    public class VBScriptDirective
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
                    if (line.StartsWith("StartVBScript::"))
                    {
                        Console.WriteLine("StartVBScript:: directive found. Beginning VBScript collection.");
                        scriptBuilder = new StringBuilder();
                        continue;
                    }

                    if (line.StartsWith("EndVBScript::"))
                    {
                        if (scriptBuilder == null)
                        {
                            Console.WriteLine("Warning: EndVBScript:: found without a matching StartVBScript::.");
                        }
                        else
                        {
                            Console.WriteLine("EndVBScript:: directive found. Executing collected VBScript.");
                            ExecuteVBScript(scriptBuilder.ToString());
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
                    Console.WriteLine("Warning: StartVBScript:: directive found without a matching EndVBScript::. Script ignored.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static void ExecuteVBScript(string scriptContent)
        {
            string tempScriptPath = Path.Combine(Path.GetTempPath(), $"tempScript_{Guid.NewGuid()}.vbs");

            try
            {
                // Save the script to a temporary file
                File.WriteAllText(tempScriptPath, scriptContent);
                Console.WriteLine($"Temporary VBScript created: {tempScriptPath}");

                // Execute the script using cscript.exe
                ProcessStartInfo processStartInfo = new ProcessStartInfo
                {
                    FileName = "cscript.exe",
                    Arguments = $"//NoLogo \"{tempScriptPath}\"",
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
                        Console.WriteLine("VBScript Output:");
                        Console.WriteLine(output);
                    }

                    if (!string.IsNullOrEmpty(error))
                    {
                        Console.WriteLine("VBScript Error:");
                        Console.WriteLine(error);
                    }

                    Console.WriteLine($"VBScript finished with exit code {process.ExitCode}.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing VBScript: {ex.Message}");
            }
            finally
            {
                // Clean up temporary script file
                if (File.Exists(tempScriptPath))
                {
                    File.Delete(tempScriptPath);
                    Console.WriteLine($"Temporary VBScript deleted: {tempScriptPath}");
                }
            }
        }
    }
}
