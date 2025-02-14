using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scripting.Directives
{
    public class FileLookDirective
    {
        public static void ProcessDirectives()
        {
            string filePath = "commands.txt"; // Path to the directives file

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
                    if (line.StartsWith("FileLook::"))
                    {
                        string fileName = line.Substring("FileLook::".Length).Trim();
                        if (!string.IsNullOrEmpty(fileName))
                        {
                            Console.WriteLine($"FileLook:: directive found. Analyzing file: {fileName}");
                            DisplayFileInformation(fileName);
                        }
                        else
                        {
                            Console.WriteLine("FileLook:: directive found, but no file name was specified.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static void DisplayFileInformation(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"File not found: {filePath}");
                    return;
                }

                // Verify if it's a PE file
                if (!IsPortableExecutable(filePath))
                {
                    Console.WriteLine($"The specified file is not a valid PE file: {filePath}");
                    return;
                }

                FileInfo fileInfo = new FileInfo(filePath);
                FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(filePath);

                // Display general file information
                Console.WriteLine($"File: {filePath}");
                Console.WriteLine($"Size: {fileInfo.Length} bytes");
                Console.WriteLine($"Created: {fileInfo.CreationTime}");
                Console.WriteLine($"Modified: {fileInfo.LastWriteTime}");
                Console.WriteLine($"File Version: {versionInfo.FileVersion}");
                Console.WriteLine($"Product Version: {versionInfo.ProductVersion}");
                Console.WriteLine($"Description: {versionInfo.FileDescription}");
                Console.WriteLine($"Company: {versionInfo.CompanyName}");

                // Compute and display hashes
                Console.WriteLine($"MD5: {ComputeHash(filePath, MD5.Create())}");
                Console.WriteLine($"SHA1: {ComputeHash(filePath, SHA1.Create())}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error analyzing file '{filePath}': {ex.Message}");
            }
        }

        private static string ComputeHash(string filePath, HashAlgorithm algorithm)
        {
            try
            {
                using (var stream = File.OpenRead(filePath))
                {
                    byte[] hash = algorithm.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
            catch (Exception ex)
            {
                return $"Error computing hash: {ex.Message}";
            }
        }

        private static bool IsPortableExecutable(string filePath)
        {
            try
            {
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    byte[] buffer = new byte[2];
                    stream.Read(buffer, 0, 2);

                    // Check for the "MZ" magic number at the start of the file
                    return buffer[0] == 'M' && buffer[1] == 'Z';
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
