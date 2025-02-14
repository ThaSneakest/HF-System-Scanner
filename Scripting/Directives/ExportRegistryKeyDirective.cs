using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scripting.Directives
{
    public class ExportRegistryKeyDirective
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
                    if (line.StartsWith("ExportRegKey::"))
                    {
                        string keyAndOutput = line.Substring("ExportRegKey::".Length).Trim();
                        if (!string.IsNullOrEmpty(keyAndOutput))
                        {
                            string[] parts = keyAndOutput.Split(new[] { '|' }, 2, StringSplitOptions.RemoveEmptyEntries);

                            if (parts.Length == 2)
                            {
                                string registryKey = parts[0].Trim();
                                string outputFilePath = parts[1].Trim();
                                Console.WriteLine($"ExportRegKey:: directive found. Exporting registry key: {registryKey} to {outputFilePath}");
                                ExportRegistryKey(registryKey, outputFilePath);
                            }
                            else
                            {
                                Console.WriteLine($"Invalid ExportRegKey:: format. Expected 'RegistryKey|OutputFilePath', got: {keyAndOutput}");
                            }
                        }
                        else
                        {
                            Console.WriteLine("ExportRegKey:: directive found, but no registry key was specified.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static void ExportRegistryKey(string registryKey, string outputFilePath)
        {
            try
            {
                // Ensure the output directory exists
                string outputDirectory = Path.GetDirectoryName(outputFilePath);
                if (!string.IsNullOrEmpty(outputDirectory) && !Directory.Exists(outputDirectory))
                {
                    Directory.CreateDirectory(outputDirectory);
                }

                // Use reg.exe to export the key
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "reg",
                    Arguments = $"export \"{registryKey}\" \"{outputFilePath}\" /y",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using (Process process = new Process { StartInfo = psi })
                {
                    process.Start();
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    if (process.ExitCode == 0)
                    {
                        Console.WriteLine($"Successfully exported registry key: {registryKey} to {outputFilePath}");
                        Console.WriteLine($"Export output: {output}");
                    }
                    else
                    {
                        Console.WriteLine($"Error exporting registry key: {registryKey}");
                        Console.WriteLine($"Export error: {error}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error exporting registry key '{registryKey}': {ex.Message}");
            }
        }
    }
}
