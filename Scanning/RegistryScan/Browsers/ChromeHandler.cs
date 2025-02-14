using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.DirectoryServices.ActiveDirectory;
using System.IO;
using System.Text.RegularExpressions;
using Wildlands_System_Scanner.Constants;
using Wildlands_System_Scanner.Registry;

public class ChromeHandler
{
    public static List<string> GetChromeInfo(string browser = "CHR")
    {
        List<string> chromeInfo = new List<string>();
        string folder = string.Empty;
        string profile = string.Empty;
        string preferences = string.Empty;
        string securePreferences = string.Empty;
        string path = string.Empty;
        string[] arrFold = Array.Empty<string>();

        // Assuming 'Label1' and 'ScanB' are part of a UI and will be set elsewhere
        // Update label or scan status with the browser type and preferences
        Console.WriteLine($"{browser}: Preferences");

        // Define the browser-specific folder paths
        switch (browser)
        {
            case "BRA":
                folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BraveSoftware\\Brave-Browser\\User Data");
                break;
            case "CHR":
                folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Google\\Chrome\\User Data");
                break;
            case "OPR":
                folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Opera Software\\Opera Stable");
                break;
            case "VIV":
                folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Vivaldi\\User Data");
                break;
            case "YAN":
                folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Yandex\\YandexBrowser\\User Data");
                break;
        }

        // Read "Local State" file to get the last used profile
        string localStateFile = Path.Combine(folder, "Local State");
        if (File.Exists(localStateFile))
        {
            var localStateContent = File.ReadAllText(localStateFile);
            var match = Regex.Match(localStateContent, @"""last_used"":\s*""(.+?)""");
            if (match.Success)
            {
                profile = match.Groups[1].Value;
                chromeInfo.Add($"{browser} DefaultProfile: {profile} ||||");
            }
        }

        // Get extensions and other settings from browser profile directories
        Console.WriteLine($"{browser}: Extensions");

        arrFold = Directory.GetDirectories(folder, "*", SearchOption.AllDirectories);
        foreach (var subFolder in arrFold)
        {
            string securePreferencesPath = Path.Combine(subFolder, "secure preferences");

            if (!File.Exists(securePreferencesPath))
                continue;

            string att = string.Empty;
            string dateCr = File.GetCreationTime(subFolder).ToString();
            chromeInfo.Add($"{browser} Profile: {subFolder} [{dateCr}] {att} ||||");

            preferences = File.ReadAllText(Path.Combine(subFolder, "Preferences"));
            var downloadDirMatch = Regex.Match(preferences, @"""download""\s*[^""]+""default_directory""\s*:\s*""([^""]*)""");

            if (downloadDirMatch.Success)
            {
                path = downloadDirMatch.Groups[1].Value.Replace("\\\\", "\\");
                chromeInfo.Add($"{browser} DownloadDir: {path} ||||");
            }

            securePreferences = File.ReadAllText(securePreferencesPath);
            string profileName = Path.GetFileName(subFolder) + " -> ";
            chromeInfo.Add($"Notifications: {profileName}");

            // Further parsing of secure preferences if necessary
            var homepageMatch = Regex.Match(securePreferences, @"""homepage""\s*:\s*""([^""]*)""");
            if (homepageMatch.Success)
            {
                chromeInfo.Add($"{browser} HomePage: {profileName}{homepageMatch.Groups[1].Value} ||||");
            }
        }

        return chromeInfo;
    }
   
    public static void GetChromeExtensions(string file, ref List<string> arrChrome, string browser)
    {
        string preferences = File.ReadAllText(Path.Combine(file, "Preferences"));
        string securePreferences = File.ReadAllText(Path.Combine(file, "secure preferences"));
        string extFolderAll = Path.Combine(file, "Extensions");
        string[] fileArray = Directory.GetFiles(extFolderAll, "*", SearchOption.AllDirectories);

        foreach (var extPath in fileArray)
        {
            string upUrl = string.Empty;
            string name = GetExtensionName(preferences, extPath);
            if (string.IsNullOrEmpty(name))
            {
                name = GetExtensionName(securePreferences, extPath);
            }

            if (string.IsNullOrEmpty(name))
            {
                string[] pathArray = Directory.GetFiles(extPath, "*", SearchOption.TopDirectoryOnly);
                foreach (var path in pathArray)
                {
                    if (File.Exists(Path.Combine(path, "manifest.json")))
                    {
                        string manifestContent = File.ReadAllText(Path.Combine(path, "manifest.json"));
                        upUrl = Regex.Match(manifestContent, @"(?i)""update_url""\s*:\s*""(.+?)""").Groups[1].Value;
                        name = GetExtensionName(path);
                    }
                }
            }

            if (string.IsNullOrEmpty(upUrl))
            {
                string[] pathArray = Directory.GetFiles(extPath, "*", SearchOption.TopDirectoryOnly);
                foreach (var path in pathArray)
                {
                    if (File.Exists(Path.Combine(path, "manifest.json")))
                    {
                        string manifestContent = File.ReadAllText(Path.Combine(path, "manifest.json"));
                        upUrl = Regex.Match(manifestContent, @"(?i)""update_url""\s*:\s*""(.+?)""").Groups[1].Value;
                    }
                }
            }

            name = ConvertName(name);
            string dateCreated = File.GetCreationTime(extPath).ToString();

            if (!string.IsNullOrEmpty(upUrl) && !Regex.IsMatch(upUrl, @"(?i)http(s)?://(clients2\.google|extension-updates\.opera)\.com/"))
            {
                upUrl = " [UpdateUrl:" + upUrl + "] <==== [Updated]";
            }
            else
            {
                upUrl = string.Empty;
            }

            arrChrome.Add($"{browser} Extension: ({name}) - {extPath} [{dateCreated}] {upUrl} |||");
        }

        var extPaths = Regex.Matches(securePreferences, @"""path""\s*:\s*""(.+?)""");

        foreach (Match extPath in extPaths)
        {
            string upUrl = string.Empty;
            string extPathStr = extPath.Groups[1].Value;
            if (Regex.IsMatch(extPathStr, @"(?i)[c-z]:\\") && !Regex.IsMatch(extPathStr, @"(?i)\\chrome\\.+\\resources") && !Regex.IsMatch(extPathStr, @"(?i)User Data\\\\(Default|Profile \d+)\\\\Extensions|Brave-Browser\\\\User Data"))
            {
                extPathStr = extPathStr.Replace("\\\\", "\\");
                if (File.Exists(Path.Combine(extPathStr, "manifest.json")))
                {
                    string manifestContent = File.ReadAllText(Path.Combine(extPathStr, "manifest.json"));
                    upUrl = Regex.Match(manifestContent, @"(?i)""update_url""\s*:\s*""(.+?)""").Groups[1].Value;

                    if (!string.IsNullOrEmpty(upUrl) && !Regex.IsMatch(upUrl, @"(?i)http(s)?://(clients2\.google|extension-updates\.opera)\.com/"))
                    {
                        upUrl = " [UpdateUrl:" + upUrl + "] <==== [Updated]";
                    }
                    else
                    {
                        upUrl = string.Empty;
                    }

                    string name = GetExtensionName(extPathStr);
                    string dateCreated = File.GetCreationTime(extPathStr).ToString();
                    arrChrome.Add($"{browser} Extension: ({name}) - {extPathStr} [{dateCreated}] {upUrl} |||");
                }
            }
        }
    }
    private static string GetExtensionName(string preferences, string filePath)
    {
        var match = Regex.Match(preferences, @"(?i)""name""\s*:\s*""(.+?)""");
        return match.Success ? match.Groups[1].Value : string.Empty;
    }

    public static string GetExtensionName(string path1)
    {
        string name = string.Empty;
        string ret = string.Empty;

        // Check if the path has the manifest file
        if (Regex.IsMatch(path1, @"\\manifest.*.json"))
        {
            ret = File.ReadAllText(path1);
            path1 = Regex.Replace(path1, @"(.+)\\.+", "$1");
        }
        else
        {
            ret = File.ReadAllText(Path.Combine(path1, "manifest.json"));
        }

        // Check if the 'name' field in manifest is a localized string
        if (Regex.IsMatch(ret, @"(?i)""name""\s*:\s*""__MSG"))
        {
            var clMatch = Regex.Match(ret, @"(?i)""default_locale""\s*:\s*""(.+?)""");
            string cl = clMatch.Success ? clMatch.Groups[1].Value : string.Empty;

            var msMatch = Regex.Match(ret, @"(?i)""name""\s*:.*MSG_(.+?)__");
            string ms = msMatch.Success ? msMatch.Groups[1].Value : string.Empty;

            if (!string.IsNullOrEmpty(ms))
            {
                // Read the locale messages file
                string readJl = File.ReadAllText(Path.Combine(path1, "_locales", cl, "messages.json"));
                var regex = Regex.Match(readJl, $@"(?is)""{ms}""[^}}]+?""message"":\s*""(.*?)""");

                if (regex.Success)
                {
                    name = readJl; // Assuming FIREFOXEXTENSIONFILE1() returns the name from readJl and regex
                }
            }
        }
        else
        {
            ret = Regex.Replace(ret, @"(?s)^\s*\{|\s*\}\s*$", "");
            ret = Regex.Replace(ret, @"(?s)\{[^{]+?\}", "");
            ret = Regex.Replace(ret, @"(?s)\{[^{]+?\}", "");

            var regexMatch = Regex.Match(ret, @"(?i),name""\s*:\s*""(.+?)""");
            if (regexMatch.Success)
            {
                name = regexMatch.Groups[1].Value;
            }
            else
            {
                var altMatch = Regex.Match(ret, @"(?i)""name""\s*:\s*""(.+?)""");
                if (altMatch.Success)
                {
                    name = altMatch.Groups[1].Value;
                }
            }
        }

        // If the name is still empty, use a default value
        if (string.IsNullOrEmpty(name))
        {
            name = "Unknown Extension"; // Substitute for $FF1 in the original code
        }

        return name;
    }

    private static string ConvertName(string name)
    {
        return name; // Implement conversion logic if required
    }

    public static string GetExtensionNameFromRead(string readContent, string exId)
    {
        // Check if the read content contains the extension ID and path
        if (Regex.IsMatch(readContent, $"{exId}\"\\s*:\\s*{{") && Regex.IsMatch(readContent, "\"path\":\\s*\"" + exId))
        {
            // Extract the part of the content that matches the pattern with the extension ID
            var match = Regex.Match(readContent, $@"(?is){exId}(?:.(?!{exId}))+?""path"":\s*""{exId}""");
            if (match.Success)
            {
                // Extract the 'name' field from the match
                var nameMatch = Regex.Match(match.Value, @",\s*""name""\s*:\s*""((?!__MS)[^:]+?)"",");
                if (nameMatch.Success)
                {
                    return nameMatch.Groups[1].Value; // Return the found name
                }
            }
        }
        return null; // Return null if no match was found
    }

    public void CHROMENOTI(string read, ref List<string> arr, string prof, string brows = "CHR Notifications: ")
    {
        var read1 = Regex.Match(read, @",\""notifications\"":\{\},");
        if (read1.Success) return;

        read1 = Regex.Match(read, @",\""notifications\"":\{(.+?)\}\},");
        if (!read1.Success) return;

        var notiMatches = Regex.Matches(read1.Groups[1].Value, @"(?i)http.+?setting"":\[?\d");
        if (notiMatches.Count == 0) return;

        List<string> arr1 = new List<string>();
        foreach (Match match in notiMatches)
        {
            if (Regex.IsMatch(match.Value, @"(?i)setting"":\[?1"))
            {
                var url = Regex.Replace(match.Value, @"(?i)(https?:[^:]+):.+", "$1");
                arr1.Add(url);
            }
        }

        if (arr1.Count > 1)
        {
            arr.Add($"{brows}{prof}{string.Join("; ", arr1)} ||||");
        }
    }

    public void CHROMEHKLM(string key, ref List<string> chrome, string bro = "Chrome")
    {
        string hklm = "HKLM";
        if (key.Contains("HKU\\"))
        {
            hklm = System.Text.RegularExpressions.Regex.Replace(key, "(HKU\\.+?).+", "$1");
        }

        string bro1 = bro;
        if (bro == "Chrome")
        {
            bro1 = "CHR";
        }

        RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(key);
        if (registryKey != null)
        {
            int i = 0;
            while (true)
            {
                string subKeyName = null;
                try
                {
                    subKeyName = registryKey.GetSubKeyNames()[i];
                }
                catch (IndexOutOfRangeException)
                {
                    break;
                }

                string data = registryKey.OpenSubKey(subKeyName)?.GetValue("Path") as string;
                if (data != null)
                {
                    string dateCr = "";
                    DateTime? dateC = FileUtils.GetFileCreationDateTime(data);
                    if (dateC.HasValue)
                    {
                        dateCr = $" [{dateC.Value:yyyy-MM-dd}]";
                    }
                    else
                    {
                        if (!data.Contains("http(s|):"))
                        {
                            dateCr = " <NFOUND>";
                        }
                    }

                    chrome.Add($"{bro1} {hklm}\\...\\{bro}\\Extension: [{subKeyName}] - {data}{dateCr} ||||");
                    i++;
                    continue;
                }

                data = registryKey.OpenSubKey(subKeyName)?.GetValue("update_url") as string;
                if (data != null)
                {
                    if (System.Text.RegularExpressions.Regex.IsMatch(data, "(?i)http(|s)://(edge\\.microsoft\\.com|extensionwebstorebase\\.edgesv\\.net|clients2\\.google\\.com)/"))
                    {
                        data = "";
                    }
                    else
                    {
                        data = " - " + data;
                    }

                    chrome.Add($"{bro1} {hklm}\\...\\{bro}\\Extension: [{subKeyName}] {data} ||||");
                    i++;
                    continue;
                }

                chrome.Add($"{bro1} {hklm}\\...\\{bro}\\Extension: [{subKeyName}] - <NFOUND Path/update_url> ||||");
                i++;
            }
            registryKey.Close();
        }
    }
    // Handle Chrome HTML key processing
    public static void ProcessClsidChr(string userC)
    {
        string company = string.Empty;
        string vdata = string.Empty;
        string file = string.Empty;

        // Build the registry path
        string registryPath = $@"{userC}\ChromeHTML\shell\open\command";

        // Open the registry key
        using (RegistryKey baseKey = Registry.Users.OpenSubKey(registryPath))
        {
            if (baseKey != null)
            {
                // Read the "Default" value
                vdata = RegistryValueHandler.TryReadRegistryValue(baseKey, "Default");
            }
            else
            {
                // Handle the case where the registry key is not found
                Console.WriteLine($"Registry path not found: HKU\\{registryPath}");
                return;
            }
        }

        file = vdata;
        userC = userC.Replace("_Classes", "");

        if (File.Exists(file))
        {
            vdata = file;
        }

        // Check if the file path matches Chrome's executable and company is "Google Inc"
        if (Regex.IsMatch(vdata, @"(?i):\\(Program Files|Users\\[^\\]+\\AppData\\Local)\\Google\\Chrome\\Application\\chrome\.exe") &&
            company.Contains("Google Inc"))
        {
            // Chrome, no action needed
            Console.WriteLine("Chrome executable detected, no action needed.");
        }
        else
        {
            // Handle other cases
            Console.WriteLine($"Non-Chrome executable or company mismatch detected: {vdata}");
        }
    }

    public static void GetChromeVersion()
    {
        // Check if either of the registry paths exist
        bool chromeExists = RegistryKeyHandler.RegistryKeyExists(RegistryConstants.GoogleChromeRegistryCurrentUserUninstallPath) ||
                            RegistryKeyHandler.RegistryKeyExists(RegistryConstants.GoogleChromeRegistryLocalMachineUninstallPath);

        if (chromeExists)
        {
            Logger.Instance.LogPrimary($"Google Chrome is installed.");
        }
        else
        {
            Logger.Instance.LogPrimary($"Google Chrome: Not Installed!");
        }
    }

}
