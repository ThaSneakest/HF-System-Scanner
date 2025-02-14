using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scripting.Directives
{
    public class VirusScanDirective
    {
        private const string ApiUrl = "https://virusscan.jotti.org/api/scan"; // Replace with the correct API endpoint
        private const string ApiKey = "your-api-key-here"; // Replace with your API key

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
                    if (line.StartsWith("VirusScan::"))
                    {
                        string targetFilePath = line.Substring("VirusScan::".Length).Trim();
                        if (!string.IsNullOrEmpty(targetFilePath))
                        {
                            Console.WriteLine($"VirusScan:: directive found. Scanning file: {targetFilePath}");
                            ScanFileWithJotti(targetFilePath).Wait();
                        }
                        else
                        {
                            Console.WriteLine("VirusScan:: directive found, but no file path was specified.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static async Task ScanFileWithJotti(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"File not found: {filePath}");
                return;
            }

            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);

                    using (var content = new MultipartFormDataContent())
                    {
                        var fileContent = new ByteArrayContent(File.ReadAllBytes(filePath));
                        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
                        content.Add(fileContent, "file", Path.GetFileName(filePath));

                        Console.WriteLine("Uploading file to Jotti for scanning...");
                        var response = await httpClient.PostAsync(ApiUrl, content);

                        if (response.IsSuccessStatusCode)
                        {
                            var result = await response.Content.ReadAsStringAsync();
                            Console.WriteLine("Scan Results:");
                            Console.WriteLine(result);
                        }
                        else
                        {
                            Console.WriteLine($"Failed to scan file. HTTP Status: {response.StatusCode}");
                            var error = await response.Content.ReadAsStringAsync();
                            Console.WriteLine($"Error: {error}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error scanning file '{filePath}': {ex.Message}");
            }
        }
    }
}
