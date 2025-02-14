using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Wildlands_System_Scanner.Registry;
using Wildlands_System_Scanner.Utilities;

public class EdgeHandler
{
    public static List<string> EDGE()
    {
        var edgeData = new List<string>();
        string downloadDirectory = RegistryValueHandler.ReadRegistryValue(
            @"HKCU\Software\Classes\Local Settings\Software\Microsoft\Windows\CurrentVersion\AppContainer\Storage\microsoft.microsoftedge_8wekyb3d8bbwe\MicrosoftEdge\Main",
            "Default Download Directory"
        );

        if (!string.IsNullOrEmpty(downloadDirectory))
        {
            edgeData.Add($"DownloadDir: {downloadDirectory}");
        }

        // Iterate over user registry keys
        foreach (var userKey in RegistryUserHandler.GetUserRegistryKeys())
        {
            string baseKeyPath = $@"HKU\{userKey}\Software\Classes\Local Settings\Software\Microsoft\Windows\CurrentVersion\AppContainer\Storage";
            using (var hKey = RegistryKeyHandler.OpenRegistryKey(baseKeyPath))
            {
                if (hKey != null)
                {
                    foreach (var subKeyName in hKey.GetSubKeyNames())
                    {
                        if (subKeyName.IndexOf("microsoft.microsoftedge_", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            string edgeKeyPath = $@"{baseKeyPath}\{subKeyName}\MicrosoftEdge";

                            // Home Button Page
                            string homeButtonPage = RegistryValueHandler.ReadRegistryValue($@"{edgeKeyPath}\Main", "HomeButtonPage");
                            if (!string.IsNullOrEmpty(homeButtonPage))
                            {
                                string sanitizedUrl = Regex.Replace(homeButtonPage, @"(?i)http(s|):", "hxxp$1:");
                                edgeData.Add($"Edge HomeButtonPage: HKU\\{userKey} -> {sanitizedUrl}");
                            }

                            // Continuous Browsing
                            string continuousBrowsing = RegistryValueHandler.ReadRegistryValue($@"{edgeKeyPath}\ContinuousBrowsing", "Enabled");
                            if (!string.IsNullOrEmpty(continuousBrowsing))
                            {
                                edgeData.Add($"Edge Session Restore: HKU\\{userKey} -> Enabled");
                            }
                        }
                    }
                }
            }
        }

        // Handle Edge Extensions
        string extensionsKey = @"HKCU\SOFTWARE\Classes\Local Settings\Software\Microsoft\Windows\CurrentVersion\AppContainer\Storage\microsoft.microsoftedge_8wekyb3d8bbwe\MicrosoftEdge\ExtensionsStore\datastore\Config\";
        using (var hKeyExtensions = RegistryKeyHandler.OpenRegistryKey(extensionsKey))
        {
            if (hKeyExtensions != null)
            {
                foreach (var extensionSubKey in hKeyExtensions.GetSubKeyNames())
                {
                    string extensionName = RegistryValueHandler.ReadRegistryValue($@"{extensionsKey}{extensionSubKey}\LocalizedMessages", "name")
                                           ?? RegistryValueHandler.ReadRegistryValue($@"{extensionsKey}{extensionSubKey}\LocalizedMessages", "extensionName");
                    string extensionPath = RegistryValueHandler.ReadRegistryValue($@"{extensionsKey}{extensionSubKey}", "Path");

                    if (string.IsNullOrEmpty(extensionPath))
                    {
                        if (extensionName == null)
                        {
                            extensionName = "Unknown Extension";
                        }
                        edgeData.Add($"Edge Extension: ({extensionName}) -> {extensionSubKey} => Path not found");
                        continue;
                    }

                    string sanitizedPath = extensionPath.IndexOf(@"\Assets\", StringComparison.OrdinalIgnoreCase) >= 0
                        ? extensionPath
                        : Regex.Replace(extensionPath, @"(.+)\\.+", "$1");

                    if (!File.Exists(sanitizedPath))
                    {
                        if (extensionName == null)
                        {
                            extensionName = "Unknown Extension";
                        }
                        edgeData.Add($"Edge Extension: ({extensionName}) -> {extensionSubKey} => {sanitizedPath} [Not Found]");
                        continue;
                    }

                    edgeData.Add($"Edge Extension: ({extensionName}) -> {extensionSubKey} => {sanitizedPath}");
                }


            }
        }

        // Check Edge User Data directory
        string userDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Microsoft\Edge\User Data");
        if (Directory.Exists(userDataPath))
        {
            ProcessEdgeUserData(edgeData);
        }

        return edgeData;
    }



    private static void ProcessEdgeUserData(List<string> edgeData)
    {
        // Placeholder for processing Edge user data directory
        edgeData.Add("Processed Edge User Data directory.");
    }

    public static void ProcessEdgePreferences(ref List<string> edgeData)
    {
        string label = "Scanning Edge: Preferences";
        Console.WriteLine(label);

        // Read the Local State file
        string localStatePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Microsoft\Edge\User Data\Local State");
        string profile = File.Exists(localStatePath) ? File.ReadAllText(localStatePath) : string.Empty;

        // Extract last used profile
        var match = Regex.Match(profile, @"""last_used"":\s*""(.+?)""");
        if (match.Success)
        {
            string lastUsedProfile = match.Groups[1].Value;
            edgeData.Add($"Edge DefaultProfile: {lastUsedProfile}");
        }

        // Scan for Edge resources
        string resourcesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Microsoft\Edge\Application");
        var resourceFiles = Directory.EnumerateFiles(resourcesPath, "*.*", SearchOption.AllDirectories);
        foreach (var resourceFile in resourceFiles)
        {
            if (resourceFile.EndsWith("resources.pak"))
            {
                string content = File.ReadAllText(resourceFile);
                if (Regex.IsMatch(content, "(64656661756C745F7365617263685F656E67696E655F6E616D65|64656661756C745F7365617263685F656E67696E655F6B6579776F726473)"))
                {
                    edgeData.Add($"Edge resource file indicates search engine configuration issue: {resourceFile}");
                }
            }
        }

        // Check user data directory
        string userDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Microsoft\Edge\User Data");
        if (Directory.Exists(userDataDir))
        {
            var profiles = Directory.EnumerateDirectories(userDataDir, "*", SearchOption.TopDirectoryOnly);
            foreach (var profileDir in profiles)
            {
                string preferencesPath = Path.Combine(profileDir, "Preferences");
                if (File.Exists(preferencesPath))
                {
                    string preferences = File.ReadAllText(preferencesPath);
                    Match downloadMatch = Regex.Match(preferences, @"""download""\s*[^""]+""default_directory""\s*:\s*""([^""]*)""");
                    if (downloadMatch.Success)
                    {
                        string downloadDir = downloadMatch.Groups[1].Value.Replace(@"\\", @"\");
                        edgeData.Add($"Edge Download Directory: {downloadDir}");
                    }
                }
            }
        }

        // Scan extensions
        Console.WriteLine("Scanning Edge: Extensions");
        string extensionsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Microsoft\Edge\User Data\Default\Extensions");
        if (Directory.Exists(extensionsDir))
        {
            var extensions = Directory.EnumerateDirectories(extensionsDir, "*", SearchOption.TopDirectoryOnly);
            foreach (var extension in extensions)
            {
                edgeData.Add($"Edge Extension Found: {extension}");
            }
        }

        // Check for additional configurations in registry
        ProcessEdgeRegistry(edgeData);

        Console.WriteLine("Edge scanning completed.");
    }

    private static void ProcessEdgeRegistry(List<string> edgeData)
    {
        Console.WriteLine("Scanning Edge Registry...");
        string edgeExtensionsKey = @"HKLM\SOFTWARE\Microsoft\Edge\Extensions";
        using (RegistryKey key = Registry.LocalMachine.OpenSubKey(edgeExtensionsKey))
        {
            if (key != null)
            {
                foreach (string subKey in key.GetSubKeyNames())
                {
                    edgeData.Add($"Edge Extension (Registry): {subKey}");
                }
            }
        }
    }

    public static void ProcessEdgeExtensions(string filePath, ref List<string> edgeExtensions)
    {
        // Read Preferences
        string preferencesPath = Path.Combine(filePath, "Preferences");
        string preferences = File.Exists(preferencesPath) ? File.ReadAllText(preferencesPath) : string.Empty;

        // Read Secure Preferences
        string securePreferencesPath = Path.Combine(filePath, "secure preferences");
        string securePreferences = File.Exists(securePreferencesPath) ? File.ReadAllText(securePreferencesPath) : string.Empty;

        // Extensions Directory
        string extensionsDir = Path.Combine(filePath, "Extensions");
        if (Directory.Exists(extensionsDir))
        {
            var extensionFolders = Directory.GetDirectories(extensionsDir);
            foreach (var extensionFolder in extensionFolders)
            {
                string updateUrl = string.Empty;

                // Get Extension Name
                string name = GetExtensionName(preferences, extensionFolder)
                    ?? GetExtensionName(securePreferences, extensionFolder);

                if (string.IsNullOrEmpty(name))
                {
                    string manifestPath = Path.Combine(extensionFolder, "manifest.json");
                    if (File.Exists(manifestPath))
                    {
                        string manifestContent = File.ReadAllText(manifestPath);

                        // Extract Update URL
                        var match = Regex.Match(manifestContent, @"""update_url""\s*:\s*""(.+?)""", RegexOptions.IgnoreCase);
                        if (match.Success)
                        {
                            updateUrl = match.Groups[1].Value;
                        }

                        name = GetExtensionNameFromManifest(manifestContent);
                    }
                }

                string creationDate = DateUtils.GetCreationDate(extensionFolder);
                if (!string.IsNullOrEmpty(updateUrl) && !Regex.IsMatch(updateUrl, @"http(|s)://(edge\.microsoft\.com|extensionwebstorebase.edgesv.net|clients2\.google\.com)/", RegexOptions.IgnoreCase))
                {
                    updateUrl = $" [UpdateUrl:{updateUrl}] <==== Potentially suspicious";
                }
                else
                {
                    updateUrl = string.Empty;
                }

                edgeExtensions.Add($"Edge Extension: ({name}) - {extensionFolder} [{creationDate}] {updateUrl}");
            }
        }

        // Process Secure Preferences for additional paths
        var matches = Regex.Matches(securePreferences, @"""path"":\s*""(.+?)""", RegexOptions.IgnoreCase);
        foreach (Match match in matches)
        {
            string extensionPath = match.Groups[1].Value.Replace(@"\\", @"\");

            if (File.Exists(Path.Combine(extensionPath, "manifest.json")))
            {
                string manifestContent = File.ReadAllText(Path.Combine(extensionPath, "manifest.json"));

                // Extract Update URL
                var urlMatch = Regex.Match(manifestContent, @"""update_url""\s*:\s*""(.+?)""", RegexOptions.IgnoreCase);
                string updateUrl = urlMatch.Success ? urlMatch.Groups[1].Value : string.Empty;

                if (!string.IsNullOrEmpty(updateUrl) && !Regex.IsMatch(updateUrl, @"http(|s)://(edge\.microsoft\.com|extensionwebstorebase.edgesv.net|clients2\.google\.com)/", RegexOptions.IgnoreCase))
                {
                    updateUrl = $" [UpdateUrl:{updateUrl}] <==== Potentially suspicious";
                }
                else
                {
                    updateUrl = string.Empty;
                }

                string name = GetExtensionNameFromManifest(manifestContent);
                string creationDate = DateUtils.GetCreationDate(extensionPath);

                edgeExtensions.Add($"Edge Extension: ({name}) - {extensionPath} [{creationDate}] {updateUrl}");
            }
        }
    }

    private static string GetExtensionName(string preferences, string extensionFolder)
    {
        // Extract the extension name from preferences
        var match = Regex.Match(preferences, $@"""{Path.GetFileName(extensionFolder)}"":\s*""(.+?)""", RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value : null;
    }

    private static string GetExtensionNameFromManifest(string manifestContent)
    {
        // Extract the name from the manifest file
        var match = Regex.Match(manifestContent, @"""name""\s*:\s*""(.+?)""", RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value : "Unknown Extension";
    }

    public static void ProcessEdgeHtml(ref List<string> chromeData)
    {
        int index = 0;

        // Iterate through user registry keys under HKEY_USERS
        while (true)
        {
            string userKey = RegistryUserHandler.EnumerateUserKeys(index++);
            if (string.IsNullOrEmpty(userKey))
            {
                break;
            }

            // Skip system accounts and classes
            if (Regex.IsMatch(userKey, @"^(S-1-5-18|_Classes)$"))
            {
                continue;
            }

            string edgeHtmlKeyPath = $@"HKU\{userKey}\SOFTWARE\Clients\StartMenuInternet\EdgeHTML";
            using (var hKey = RegistryKeyHandler.OpenRegistryKey(edgeHtmlKeyPath))
            {
                if (hKey == null)
                {
                    continue;
                }

                // Read the command for opening EdgeHTML
                string commandKeyPath = $@"HKU\{userKey}\SOFTWARE\Clients\StartMenuInternet\EdgeHTML\shell\open\command";
                string commandValue = RegistryValueHandler.ReadRegistryValue(commandKeyPath, "");

                string filePath = commandValue;
                string company = "";

                // Process the file path (e.g., extract additional metadata if needed)
                if (File.Exists(filePath))
                {
                    commandValue = filePath;
                }

                // Add the processed data to the output
                chromeData.Add($"HKU\\{userKey}\\...\\StartMenuInternet\\EdgeHTML: -> {commandValue}{company}");
            }
        }
    }


    public static string GetEdgeNotifications(string userSid)
    {
        // Define the registry key path for Edge notifications
        string keyPath = $@"{userSid}\Software\Classes\Local Settings\Software\Microsoft\Windows\CurrentVersion\AppContainer\Storage\microsoft.microsoftedge_8wekyb3d8bbwe\MicrosoftEdge\Notifications\Domains";
        string fullKeyPath = $@"HKEY_USERS\{keyPath}";

        try
        {
            using (RegistryKey key = Registry.Users.OpenSubKey(keyPath))
            {
                if (key == null)
                {
                    return null; // Key not found
                }

                List<string> items = new List<string>();

                foreach (var valueName in key.GetValueNames())
                {
                    // Get the value associated with the name
                    object value = key.GetValue(valueName);
                    if (value is int intValue && intValue == 1)
                    {
                        items.Add(valueName);
                    }
                }

                // Combine the items into a semicolon-separated string
                return items.Count > 0 ? string.Join("; ", items) : null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error accessing registry key: {fullKeyPath}. {ex.Message}");
            return null;
        }
    }
}
