using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public class JottiScanHandler
{
    private static string HFIXLOG = @"C:\path\to\fixlog.txt";  // Update path as needed
    private static string FIX = @"C:\path\to\fix.txt";  // Update path as needed
    private static string APIK = "your_api_key";  // Update your API key
    private static string ERR0 = "Error";  // Example value

    public static async Task<string> Jotti()
    {
        StringBuilder results = new StringBuilder();

        string path1 = Regex.Replace(FIX, @"(?i)(?:Virusscan|Virustotal):\s*(.+)", "$1");
        string[] paths = path1.Split(';');
        if (paths.Length == 0)
        {
            return "Virusscan: Error reading paths";
        }

        for (int i = 0; i < paths.Length; i++)
        {
            paths[i] = paths[i].Trim();
            if (!File.Exists(paths[i]))
            {
                results.AppendLine($"Virusscan: {paths[i]} => Not Found");
                continue;
            }

            string hash = Utility.CalculateMD5(paths[i]);
            if (string.IsNullOrEmpty(hash) || hash == "d41d8cd98f00b204e9800998ecf8427e") // empty file hash
            {
                results.AppendLine($"Virusscan: {paths[i]} => Empty File");
                continue;
            }

            string link = await JottiSearch(hash);
            if (!string.IsNullOrEmpty(link))
            {
                results.AppendLine($"Virusscan: {paths[i]} => {link}");
                continue;
            }

            string scanResult = await JottiScan(paths[i]);
            results.AppendLine($"Virusscan: {paths[i]} => {scanResult}");
        }

        return results.ToString();
    }


    private static void NotFound(string message)
    {
        Console.WriteLine(message);  // Handle file not found error
    }

    private static async Task<string> JottiSearch(string hash)
    {
        try
        {
            using (var client = new HttpClient())
            {
                string url = $"https://virusscan.jotti.org/api/filescanjob/v2/getfileinfo/{hash}";
                client.DefaultRequestHeaders.Add("Authorization", "Key " + APIK);
                client.DefaultRequestHeaders.Add("Accept", "application/vnd.filescanjob-api.v2+json");
                var response = await client.GetStringAsync(url);

                // Check for valid response and extract URL
                var match = Regex.Match(response, @"""webUrl"":\s*""(.+?)""");
                if (match.Success)
                {
                    return match.Groups[1].Value.Replace("\\/", "/");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in JottiSearch: {ex.Message}");
        }

        return null;
    }

    private static async Task<string> JottiScan(string filePath)
    {
        try
        {
            string token = await CreateScanToken();
            if (string.IsNullOrEmpty(token)) return ERR0 + ":(2)";

            string mimeType = GetMimeType(filePath);
            string fileContent = File.ReadAllText(filePath);
            string fileName = Path.GetFileName(filePath);

            using (var client = new HttpClient())
            {
                var formContent = new MultipartFormDataContent();
                formContent.Add(new StringContent(token), "scanToken");
                formContent.Add(new StringContent(fileContent), "file", fileName);

                client.DefaultRequestHeaders.Add("Authorization", "Key " + APIK);
                client.DefaultRequestHeaders.Add("Accept", "application/vnd.filescanjob-api.v2+json");
                client.DefaultRequestHeaders.Add("Content-Type", "multipart/form-data");

                var response = await client.PostAsync("https://virusscan.jotti.org/api/filescanjob/v2/createjob", formContent);

                if (response.IsSuccessStatusCode)
                {
                    var jobId = Regex.Match(await response.Content.ReadAsStringAsync(), @"""ScanJobid"":\s*""(.+?)""").Groups[1].Value;
                    if (string.IsNullOrEmpty(jobId)) return ERR0 + ":(3)";

                    string statusUrl = $"https://virusscan.jotti.org/api/filescanjob/v2/getjobstatus/{jobId}";
                    var statusResponse = await client.GetStringAsync(statusUrl);

                    var statusMatch = Regex.Match(statusResponse, @"""webUrl"":\s*""(.+?)""");
                    if (statusMatch.Success)
                    {
                        return statusMatch.Groups[1].Value.Replace("\\/", "/");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in JottiScan: {ex.Message}");
        }

        return ERR0 + ":(4)";
    }

    private static async Task<string> CreateScanToken()
    {
        try
        {
            using (var client = new HttpClient())
            {
                var content = new StringContent("{}");
                client.DefaultRequestHeaders.Add("Authorization", "Key " + APIK);
                client.DefaultRequestHeaders.Add("Accept", "application/vnd.filescanjob-api.v2+json");

                var response = await client.PostAsync("https://virusscan.jotti.org/api/filescanjob/v2/createscantoken", content);
                var responseBody = await response.Content.ReadAsStringAsync();

                var match = Regex.Match(responseBody, @"""token"":\s*""(.+?)""");
                if (match.Success)
                {
                    return match.Groups[1].Value;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in CreateScanToken: {ex.Message}");
        }

        return null;
    }

    private static string GetMimeType(string filePath)
    {
        // Logic to determine MIME type (you can improve this with a MIME library)
        return "application/octet-stream";
    }
}
