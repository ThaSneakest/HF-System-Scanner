using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scripting.Directives
{
    public class SuspectDirective
    {
        private static readonly string UploadUrl = "https://example.com/upload"; // Replace with your upload URL

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
                    if (line.StartsWith("Suspect::"))
                    {
                        string targetPath = line.Substring("Suspect::".Length).Trim();
                        if (!string.IsNullOrEmpty(targetPath))
                        {
                            Console.WriteLine($"Suspect:: directive found. Processing: {targetPath}");
                            ZipAndUpload(targetPath);
                        }
                        else
                        {
                            Console.WriteLine("Suspect:: directive found, but no target path was specified.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static void ZipAndUpload(string targetPath)
        {
            try
            {
                if (!File.Exists(targetPath) && !Directory.Exists(targetPath))
                {
                    Console.WriteLine($"Target path does not exist: {targetPath}");
                    return;
                }

                // Create a zip archive of the file or folder
                string zipFilePath = CreateZipArchive(targetPath);

                // Upload the zip archive
                if (UploadFile(zipFilePath))
                {
                    Console.WriteLine($"File successfully uploaded: {zipFilePath}");
                }
                else
                {
                    Console.WriteLine("Upload failed.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing target '{targetPath}': {ex.Message}");
            }
        }

        private static string CreateZipArchive(string targetPath)
        {
            string zipFileName = $"{Path.GetFileName(targetPath)}_{DateTime.Now:yyyyMMddHHmmss}.zip";
            string zipFilePath = Path.Combine(Path.GetTempPath(), zipFileName);

            if (File.Exists(targetPath))
            {
                using (var archive = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
                {
                    archive.CreateEntryFromFile(targetPath, Path.GetFileName(targetPath));
                }
            }
            else if (Directory.Exists(targetPath))
            {
                ZipFile.CreateFromDirectory(targetPath, zipFilePath);
            }

            Console.WriteLine($"Created zip archive: {zipFilePath}");
            return zipFilePath;
        }

        private static bool UploadFile(string filePath)
        {
            try
            {
                using (var client = new WebClient())
                {
                    Console.WriteLine($"Uploading file: {filePath}");
                    client.UploadFile(UploadUrl, filePath);
                    Console.WriteLine("File uploaded successfully.");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading file: {ex.Message}");
                return false;
            }
        }
    }
}
