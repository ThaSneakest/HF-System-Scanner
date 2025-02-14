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
    public class CollectDirective
    {
        private static readonly string UploadUrl = "https://example.com/upload"; // Replace with actual upload URL

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
                    if (line.StartsWith("Collect::"))
                    {
                        string targetPath = line.Substring("Collect::".Length).Trim();
                        if (!string.IsNullOrEmpty(targetPath))
                        {
                            Console.WriteLine($"Collect:: directive found. Processing: {targetPath}");
                            CollectAndUpload(targetPath);
                        }
                        else
                        {
                            Console.WriteLine("Collect:: directive found, but no target path was specified.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static void CollectAndUpload(string targetPath)
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
                    Console.WriteLine($"Upload successful. Deleting original: {targetPath}");
                    DeleteOriginal(targetPath);
                }
                else
                {
                    Console.WriteLine("Upload failed. Original file/folder not deleted.");
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

        private static void DeleteOriginal(string targetPath)
        {
            try
            {
                if (File.Exists(targetPath))
                {
                    File.Delete(targetPath);
                    Console.WriteLine($"Deleted file: {targetPath}");
                }
                else if (Directory.Exists(targetPath))
                {
                    Directory.Delete(targetPath, recursive: true);
                    Console.WriteLine($"Deleted folder: {targetPath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting original target '{targetPath}': {ex.Message}");
            }
        }
    }
}
