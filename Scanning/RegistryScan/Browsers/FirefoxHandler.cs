using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System.Windows.Forms;
using Wildlands_System_Scanner;
using Wildlands_System_Scanner.Registry;
using Wildlands_System_Scanner.Utilities;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;
using Wildlands_System_Scanner.Constants;

public class FirefoxHandler
{
    private static List<string> firefoxEntries = new List<string>();
    private static readonly bool PerformExtraCheck = true; // Simulates GUICtrlRead($CHECKBOX11)
    private static readonly string ScanLabel = "Scan:";
    private static List<string> ffEntries = new List<string>();
    private static string[,] myArray = new string[10, 2];
    bool isCheckboxChecked = true;

    public static void ProcessFirefox()
    {
        Console.WriteLine("Scanning Firefox...");

        var profiles = GetFirefoxProfiles();
        if (profiles.Count > 0)
        {
            foreach (var profile in profiles)
            {
                ProcessFirefoxPreferences(profile); // Pass each profile path as a string
            }
        }


        // Scan directories for Firefox extensions
        ProcessFirefoxExtensions(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            @"Mozilla Firefox\browser\extensions"));
        ProcessFirefoxExtensions(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            @"Mozilla Firefox\browser\features"));
        ProcessFirefoxExtensions(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            @"Mozilla Firefox\distribution\extensions"));

        // Scan registry for Firefox extensions
        ProcessFirefoxExtensionsInRegistry("HKLM", @"Software");
        foreach (var userKey in RegistryUserHandler.GetUserRegistryKeys())
        {
            ProcessFirefoxExtensionsInRegistry("HKU", @"Software", userKey);
        }

        // Process Firefox plugins
        ProcessFirefoxPlugins("Plugin");
        foreach (var userKey in RegistryUserHandler.GetUserRegistryKeys())
        {
            ProcessFirefoxPluginsInRegistry(userKey, @"Software\Mozilla\Firefox\Plugins");
        }


        // Additional scans for Firefox plugins and extensions
        string profilePath = @"C:\Path\To\FirefoxProfile";
        ProcessFirefoxPluginDirectories(profilePath);

        ProcessStartMenuInternetEntries();

        // Process JavaScript and program-based extensions
        ProcessFirefoxExtensionsJs(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            @"Mozilla Firefox\defaults\pref"));
        ProcessFirefoxExtensionsJs(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            @"Mozilla Firefox\browser\defaults\preferences"));
        ProcessFirefoxExtensionsProgramFiles();

        // Write results to log
        WriteFirefoxLog();


    }


    private static void ProcessFirefoxExtensionsJs(string directoryPath)
    {
        if (Directory.Exists(directoryPath))
        {
            Console.WriteLine($"Processing JS extensions in: {directoryPath}");
        }
    }

    private static void WriteFirefoxLog()
    {
        Console.WriteLine("Writing Firefox log...");
        if (firefoxEntries.Count == 0)
        {
            Console.WriteLine("No Firefox-related entries found.");
            return;
        }

        {

            foreach (var entry in firefoxEntries)
            {
                var sanitizedEntry = entry.Replace("http://", "hxxp://").Replace("https://", "hxxps://");
            }
        }

        firefoxEntries.Clear();
    }

    public static string FirefoxExtensionFile1(string manifest, string name1, string name2)
    {
        if (!Regex.IsMatch(name2, @"(?i)\$[^\s]+\$"))
        {
            return name2;
        }

        // Extract the placeholder and replace special characters
        string plh = Regex.Match(name2, @".*(\$[^\s]+\$).*").Groups[1].Value;
        string plh1 = plh.Replace("$", "\"");
        string name3 = name2.Replace("$", "\\$");

        // Search for the pattern in the manifest
        string pattern = $@"(?is)""{name3}[^}}]+?""placeholders"":[^}}]+?{plh1}[^}}]+?""content""\s*:\s*""(.+?)""";
        Match match = Regex.Match(manifest, pattern);

        if (match.Success)
        {
            // Replace the placeholder and return the final string
            plh = plh.Replace("$", "\\$");
            string nameFin = name2.Replace(plh, match.Groups[1].Value);
            return nameFin;
        }

        return name2; // If no match found, return the original name2
    }

    public static void FirefoxExtensionFileFol(string extensionPath, int count)
    {
        // Check if the file attribute contains "D" (indicating it's a directory)
        if ((File.GetAttributes(extensionPath) & FileAttributes.Directory) == FileAttributes.Directory)
        {
            // If it's a directory, recursively call the method again
            FirefoxExtensionFileFol(extensionPath, count);
        }
        else
        {
            // If it's not a directory and the path contains ".xpi", process it
            if (extensionPath.Contains(".xpi"))
            {
                FirefoxExtensionFile(extensionPath, count);
            }
        }
    }

    // Placeholder for the FirefoxExtensionFile method
    public static void FirefoxExtensionFile(string extensionPath, int count)
    {
        // This method would contain the logic for handling ".xpi" files
        Console.WriteLine($"Processing XPI file: {extensionPath}, Count: {count}");
    }

    public static void FirefoxExtensionFol(string path)
    {
        string sig = "";
        string leg = "";
        string upUrl = "";
        string dateCr = FileTimeCm(path); // Assume FileTimeCm is a method that gets the file creation time.

        // Check if necessary files exist in the META-INF folder
        if (!File.Exists(Path.Combine(path, "META-INF", "mozilla.sf")) ||
            !File.Exists(Path.Combine(path, "META-INF", "mozilla.rsa")) ||
            !File.Exists(Path.Combine(path, "META-INF", "manifest.mf")))
        {
            sig = " [" + "FILENS1" + "]"; // Replace "FILENS1" with the actual string or constant
        }

        string regex1 = "FF1"; // Placeholder, replace with actual FF1 value

        // Check for install.rdf or manifest.json
        if (File.Exists(Path.Combine(path, "install.rdf")))
        {
            regex1 = FirefoxExtensionName(Path.Combine(path,
                "install.rdf")); // Assume FirefoxExtensionName is a method that retrieves the extension name.
            leg = " [" + "LEGACY" + "]"; // Replace "LEGACY" with the actual string or constant
        }
        else if (File.Exists(Path.Combine(path, "manifest.json")))
        {
            regex1 = GetChromeExtensionName(path); // Assume ChromeExName is a method that retrieves the Chrome extension name.
            string manifestContent = File.ReadAllText(Path.Combine(path, "manifest.json"));
            var urlMatch = Regex.Match(manifestContent, @"(?i)""update_url""\s*:\s*""(.+?)""");

            if (urlMatch.Success)
            {
                upUrl = " [UpdateUrl:" + urlMatch.Groups[1].Value + "]";
            }
        }

        // Assuming GUI control read and array add functionality
        MainApp form1 = new MainApp();
        if (form1.checkEdit8.Checked) // Check the checkbox state
        {
            if (!path.Contains("{972ce4c6-7e08-4474-a285-3208198ce6fd}"))
            {
                ArrayUtils.AddToArrayAlt(myArray, "FF Extension", "(" + regex1 + ") - " + path + " [" + dateCr + "]" + leg + sig);
            }
        }
        else
        {
            ArrayUtils.AddToArrayAlt(myArray, "FF Extension", "(" + regex1 + ") - " + path + " [" + dateCr + "]" + leg + sig);
        }

    }

    // Placeholder methods for the missing functionality in the provided AutoIt code.
    private static string FileTimeCm(string path)
    {
        // Implement logic to retrieve file creation time.
        return File.GetCreationTime(path).ToString("yyyy-MM-dd HH:mm:ss");
    }

    public static string FirefoxExtensionName(string path)
    {
        string regex1 = "FF1"; // Placeholder for FF1 (replace with actual logic or value)
        string regexRead = File.ReadAllText(path); // Read the content of the file
        string loc = Utility.GetLanguageCode("0x" + System.Globalization.CultureInfo.CurrentCulture.Name);
        loc = Regex.Replace(loc, "(-|_)", ".");

        // Check if "name" is present in the file content
        if (Regex.IsMatch(regexRead, "(?i)name"))
        {
            var regex = Regex.Match(regexRead, "(?i)name(?:>|=\"|\")(.+?)(?:<|\"\")");
            if (regex.Success)
            {
                regex1 = regex.Groups[1].Value;
            }
        }

        if (regexRead.Contains("m:locale") && regexRead.Contains(loc))
        {
            var regex = Regex.Match(regexRead, "(?is)m:locale(?:>|=\")" + loc + "(?:<|\")/em:locale.+?em:localized");
            if (regex.Success)
            {
                var nameRegex = Regex.Match(regex.Groups[0].Value, "em:name(?:>|=\")(.+?)(?:<|\")/em:name");
                if (nameRegex.Success)
                {
                    regex1 = nameRegex.Groups[1].Value;
                }
            }
        }
        else if (Regex.IsMatch(regexRead, "(?i)>en((_|-)US|(-|_)BR|)</em:locale>"))
        {
            var regex = Regex.Match(regexRead,
                "(?is)>en((_|-)US|(-|_)BR|)</em:locale>.*?<em:name(?:>|=\")(.+?)(?:<|\")/em:name");
            if (regex.Success)
            {
                regex1 = regex.Groups[1].Value;
            }
        }
        else if (Regex.IsMatch(regexRead, "(?is)/em:localized(?:>|=\").+(?:<|\")em:name"))
        {
            var regex = Regex.Matches(regexRead, "(?is)description>.+?/em:name");
            foreach (Match match in regex)
            {
                if (!Regex.IsMatch(match.Value, "em:locale"))
                {
                    regex1 = Regex.Replace(match.Value, "(?is).+<em:name(?:>|=\")(.+?)(?:<|\")/em:name.*", "$1");
                }
            }
        }

        return Utility.ConvertToUnicode(regex1);
    }


    public static void FirefoxExtensionReg(string hive, string subKey, string user = "")
    {
        string logHive = hive;
        if (hive == "HKU")
        {
            hive = "HKU\\" + user;
            logHive = "HKU\\" + user;
        }

        string key1 = hive + "\\" + subKey + "\\Mozilla";
        RegistryKey registryKey = RegistryKeyHandler.GetRegistryKey(hive, key1);
        if (registryKey == null)
            return;

        int i = 0;
        while (true)
        {
            string regKey = RegistrySubKeyHandler.GetRegistrySubKey(registryKey, i);
            if (regKey == null) break;

            string subKeyPath = key1 + "\\" + regKey;
            RegistryKey subRegistryKey = RegistryKeyHandler.GetRegistryKey(hive, subKeyPath);
            if (subRegistryKey == null)
                break;

            int j = 0;
            while (true)
            {
                string subKey2 = RegistrySubKeyHandler.GetRegistrySubKey(subRegistryKey, j);
                if (subKey2 == null) break;

                if (subKey2 == "Extensions")
                {
                    string[] valueNames = subRegistryKey.GetValueNames(); // Get all value names
                    foreach (string valueName in valueNames)
                    {
                        string data = RegistryValueHandler.TryReadRegistryValue(subRegistryKey, valueName);
                        if (string.IsNullOrEmpty(data)) continue;

                        if (data.EndsWith("\\"))
                            data = data.TrimEnd('\\');

                        MainApp form1 = new MainApp();
                        if (form1.checkEdit8.Checked)
                        {
                            if (!File.Exists(data))
                                data += " => Not Found";

                            string name = $"FF {logHive}\\...\\{regKey}\\Extensions";
                            string value = $"[{subKey2}] - {data}";

                            // Add to the array (assuming ArrayUtils is implemented elsewhere)
                            ArrayUtils.AddToArray(myArray, name, value);

                            if (File.Exists(data))
                                ProcessExtensionFileFolder(data, new Random().Next(1000, 9999));
                        }
                        else
                        {
                            if (subKey2 != "plugins" && subKey2 != "components")
                            {
                                if (!File.Exists(data))
                                    data += " => Not Found";

                                string name = $"FF {logHive}\\...\\{regKey}\\Extensions";
                                string value = $"[{subKey2}] - {data}";

                                ArrayUtils.AddToArray(myArray, name, value);

                                if (File.Exists(data))
                                    ProcessExtensionFileFolder(data, new Random().Next(1000, 9999));
                            }
                        }
                    }
                }

                j++;
            }

            i++;
        }

        registryKey?.Close();
    }


    public static void ProcessJavaScriptFiles(string path)
    {
        if (!Directory.Exists(path))
        {
            Console.WriteLine($"Directory not found: {path}");
            return;
        }

        var jsFiles = Directory.GetFiles(path, "*.js", SearchOption.TopDirectoryOnly);

        foreach (var filePath in jsFiles)
        {
            string additionalInfo = string.Empty;
            string creationDate = GetFileCreationDate(filePath);
            string fileContent = File.ReadAllText(filePath);

            // Check file content for specific patterns
            if (Regex.IsMatch(fileContent, "(?i)filename.+cfg'") || Regex.IsMatch(fileContent, "(?i)filename.+cfg\""))
            {
                additionalInfo = $" <==== {StringConstants.UPD1})";
            }

            if (Regex.IsMatch(Path.GetFileName(filePath), @"(?i)^!\w{20,}|secure_cert"))
            {
                additionalInfo = $" <==== {StringConstants.UPD1}";
            }

            if (PerformExtraCheck)
            {
                firefoxEntries.Add($"FF ExtraCheck: {filePath} [{creationDate}]{additionalInfo}");
            }
            else
            {
                if (Path.GetFileName(filePath) != "channel-prefs.js")
                {
                    firefoxEntries.Add($"FF ExtraCheck: {filePath} [{creationDate}]{additionalInfo}");
                }
            }
        }
    }

    private static string GetFileCreationDate(string filePath)
    {
        try
        {
            var creationTime = File.GetCreationTime(filePath);
            return creationTime.ToString("yyyy-MM-dd HH:mm:ss");
        }
        catch
        {
            return "0000-00-00 00:00:00";
        }
    }

    // This function mimics the behavior of the FIREFOXEXT_PROG function
    public static int ProcessFirefoxExtensionProgram(string path)
    {
        if (!Directory.Exists(path))
        {
            return 1; // Return 1 if directory does not exist
        }

        string firefoxPath = Path.Combine(path, "mozilla firefox");

        if (!Directory.Exists(firefoxPath))
        {
            return 1; // Return 1 if the Firefox directory doesn't exist
        }

        // Get all files from the specified directory and subdirectories
        var files = Directory.GetFiles(firefoxPath, "*", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            if (Regex.IsMatch(file, @"(?i).+\\(.*?cfg|\w{30,})$"))
            {
                string modificationDate = GetFileModificationDate(file);
                firefoxEntries.Add($"FF ExtraCheck: {file} [{modificationDate}] <==== {StringConstants.UPD1}");
            }
        }

        return 0;
    }

    private static string GetFileModificationDate(string filePath)
    {
        try
        {
            var modificationTime = File.GetLastWriteTime(filePath);
            return modificationTime.ToString("yyyy-MM-dd HH:mm:ss");
        }
        catch
        {
            return "Unknown date";
        }
    }

    // This function mimics the behavior of FIREFOXEXTENSION
    public static void ProcessFirefoxExtensions(string folderPath)
    {
        if (!Directory.Exists(folderPath))
        {
            return; // Exit if folder doesn't exist
        }

        Console.WriteLine($"{ScanLabel} Firefox: Extensions");

        // Get all files in folder recursively, excluding specific folders like 'staged', 'staged-xpis', and 'trash'
        var files = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories);
        List<string> filteredFiles = new List<string>();

        foreach (var file in files)
        {
            if (!Regex.IsMatch(file, @"staged|staged-xpis|trash", RegexOptions.IgnoreCase))
            {
                filteredFiles.Add(file);
            }
        }

        foreach (var file in filteredFiles)
        {
            if (Directory.Exists(file)) // If it's a directory, process subdirectories
            {
                int identifier = new Random().Next(1000, 9999);
                ProcessFirefoxExtensionFolder(file, identifier);
            }
            else if (file.EndsWith(".xpi", StringComparison.OrdinalIgnoreCase))
            {
                // Process each Firefox extension file (".xpi" files)
                ProcessFirefoxExtensionFile(file, Utility.GenerateRandomFileName());
            }
        }
    }



    // This function simulates adding entries to the firefoxEntries list
    private static void AddToFirefoxEntries(string entry)
    {
        firefoxEntries.Add(entry);
    }

    // This function mimics the behavior of FIREFOXEXTENSIONFILE
    public static void ProcessFirefoxExtensionFile(string path, string count)
    {
        string name = string.Empty;

        // Check if the path contains "manifest.*.json"
        if (Regex.IsMatch(path, @"\\manifest.*\.json", RegexOptions.IgnoreCase))
        {
            string ret = File.ReadAllText(path);
            path = Regex.Replace(path, @"(.+)\.\w+$", "$1");
        }
        else
        {
            string manifestPath = Path.Combine(path, "manifest.json");
            if (File.Exists(manifestPath))
            {
                string ret = File.ReadAllText(manifestPath);

                // Check for "__MSG"
                if (Regex.IsMatch(ret, @"(?i)""name""\s*:\s*""__MSG"))
                {
                    Match clMatch = Regex.Match(ret, @"(?i)""default_locale""\s*:\s*""(.+?)""");
                    string cl = clMatch.Success ? clMatch.Groups[1].Value : string.Empty;

                    Match msMatch = Regex.Match(ret, @"(?i)""name""\s*:.*MSG_(.+?)__""");
                    string ms = msMatch.Success ? msMatch.Groups[1].Value : string.Empty;

                    if (!string.IsNullOrEmpty(ms))
                    {
                        string localePath = Path.Combine(path, "_locales", cl, "messages.json");
                        if (File.Exists(localePath))
                        {
                            string readJson = File.ReadAllText(localePath);
                            Match regexMatch = Regex.Match(readJson, $@"(?is)""{ms}"".*?""message"":\s*""(.*?)""");
                            if (regexMatch.Success)
                            {
                                name = regexMatch.Groups[1].Value;
                            }
                        }
                    }
                }
                else
                {
                    ret = Regex.Replace(ret, @"(?s)^\s*\{|\s*\}\s*$", ""); // Remove outer braces
                    ret = Regex.Replace(ret, @"(?s)\{[^{]+?\}", ""); // Remove nested objects
                    ret = Regex.Replace(ret, @"(?s)\{[^{]+?\}", ""); // Repeat for deeply nested objects

                    Match regexMatch = Regex.Match(ret, @"(?i),name""\s*:\s*""(.+?)""");
                    if (!regexMatch.Success)
                    {
                        regexMatch = Regex.Match(ret, @"(?i)""name""\s*:\s*""(.+?)""");
                    }

                    if (regexMatch.Success)
                    {
                        name = Regex.Replace(regexMatch.Groups[1].Value, @"(?i)http(s|):", "hxxp$1:");
                    }
                }
            }
        }
    }

    private static string FileTimeCM(string path)
    {
        FileInfo fileInfo = new FileInfo(path);
        return fileInfo.CreationTime.ToString("yyyy-MM-dd HH:mm:ss");
    }


    


    private static string ReadFile(string path)
    {
        return File.Exists(path) ? File.ReadAllText(path) : string.Empty;
    }

    private static string[] FileListToArrayRec(string directory, string pattern)
    {
        return Directory.GetFiles(directory, pattern, SearchOption.AllDirectories);
    }

    private static string[] IniReadSectionNames(string path)
    {
        // Simulate reading sections from an INI file
        return new string[] { "Section1", "Section2" };
    }

    public static void FirefoxFixMethod(List<string> profiles, List<string> ffArr)
    {
        foreach (var profile in profiles)
        {
            string filePath = profile;

            if (File.Exists(filePath))
            {
                string cDate = GetFileCreationDate(filePath);
                string cDateString = $" [{cDate}]";

                if (Regex.IsMatch(filePath, @"(?i)AppData\\Roaming\\(Firefox|Profiles)|AMozilla\\AFirefox"))
                {
                    string att = " <==== Updated";
                    ffArr.Add($"FF ProfilePath: {filePath}{cDateString}{att}");
                }

                if (File.Exists(Path.Combine(filePath, "user.js")))
                {
                    cDate = GetFileCreationDate(Path.Combine(filePath, "user.js"));
                    cDateString = $" [{cDate}]";
                    ffArr.Add($"FF user.js: detected! => {Path.Combine(filePath, "user.js")}{cDateString}");
                }

                string prof = Regex.Replace(filePath, @"(?i).+?Roaming\\(.+)", "$1");
                if (File.Exists(Path.Combine(filePath, "prefs.js")))
                {
                    string prefsContent = File.ReadAllText(Path.Combine(filePath, "prefs.js"));

                    string downloadDir = Regex
                        .Match(prefsContent, @"(?i)user_pref\(""browser.download.dir"",\s*""([^""]*)""\)").Groups[1]
                        .Value;
                    if (!string.IsNullOrEmpty(downloadDir))
                    {
                        ffArr.Add($"FF DownloadDir: {downloadDir}");
                    }

                    string homepage = Regex
                        .Match(prefsContent, @"(?i)user_pref\(""browser.startup.homepage"",\s*""(.*)""\)").Groups[1]
                        .Value;
                    if (!string.IsNullOrEmpty(homepage))
                    {
                        ffArr.Add($"FF Homepage: {prof} -> {homepage}");
                    }

                    string newTab = Regex.Match(prefsContent, @"(?i)user_pref\(""browser.newtab.url"",\s*""(.*)""\)")
                        .Groups[1].Value;
                    if (!string.IsNullOrEmpty(newTab))
                    {
                        ffArr.Add($"FF NewTab: {prof} -> {newTab}");
                    }

                    string proxy = Regex.Match(prefsContent, @"user_pref\(""network.proxy.(.*)\)").Groups[1].Value;
                    if (!string.IsNullOrEmpty(proxy))
                    {
                        ffArr.Add($"FF NetworkProxy: {prof} -> {proxy}");
                    }

                    string sessionRestore = Regex.Match(prefsContent, @"user_pref\(""browser.startup.page"",\s*(.*)\)")
                        .Groups[1].Value;
                    if (sessionRestore == "3")
                    {
                        ffArr.Add($"FF Session Restore: {prof} -> Internet");
                    }
                }

                string permissionsPath = Path.Combine(filePath, "permissions.sqlite");
                if (File.Exists(permissionsPath))
                {
                    var permissions = GetPermissionsFromSQLite(permissionsPath);
                    if (permissions != null)
                    {
                        ffArr.Add($"FF Notifications: {prof} -> {permissions}");
                    }
                }

                string extensionSettingsPath = Path.Combine(filePath, "extension-settings.json");
                if (File.Exists(extensionSettingsPath))
                {
                    string extensionSettingsContent = File.ReadAllText(extensionSettingsPath);
                    if (extensionSettingsContent.Contains("homepage_override"))
                    {
                        var extensionId = Regex.Match(extensionSettingsContent, @"(?i)""id"":""([^""]+)""").Groups[1]
                            .Value;
                        if (!string.IsNullOrEmpty(extensionId))
                        {
                            ffArr.Add($"FF HomepageOverride: {prof} -> {extensionId}");
                        }
                    }

                    if (extensionSettingsContent.Contains("url_overrides"))
                    {
                        var urlOverrideId = Regex.Match(extensionSettingsContent, @"(?i)""id"":""([^""]+)""").Groups[1]
                            .Value;
                        if (!string.IsNullOrEmpty(urlOverrideId))
                        {
                            ffArr.Add($"FF NewTabOverride: {prof} -> {urlOverrideId}");
                        }
                    }
                }
            }
        }
    }


    // Example method to get a list of profiles (replace with actual profile loading logic)
    public static List<string> GetProfiles()
    {
        // Replace with logic to retrieve profiles
        return new List<string> { @"C:\Users\YourUser\AppData\Roaming\Firefox\Profiles" };
    }

    // Example method to get permissions from SQLite database (replace with actual SQLite logic)
    public static string GetPermissionsFromSQLite(string dbPath)
    {
        // Replace with logic to read from SQLite
        return "Desktop Notifications Enabled";
    }

    public static void FirefoxPluginMethod(string path, List<string> ffArr)
    {
        if (!Directory.Exists(path))
        {
            return;
        }

        // Get all the DLL files in the directory
        string[] arrayPi = Directory.GetFiles(path, "*.dll", SearchOption.TopDirectoryOnly);

        foreach (var filePath in arrayPi)
        {
            string cDate = "";
            string company = "";

            // Check if the file exists and retrieve the modification date
            if (File.Exists(filePath))
            {
                DateTime fileDate = File.GetLastWriteTime(filePath);
                cDate = fileDate.ToString("yyyy-MM-dd HH:mm:ss");
            }

            // Add the file data to the list, appending the modification date and company (if available)
            if (!string.IsNullOrEmpty(cDate))
            {
                cDate = $" [{cDate}]";
            }

            ffArr.Add($"FF Plugin ProgramFiles/Appdata: {filePath}{cDate}{company}");
        }
    }

    public static void FirefoxPlugins(string plugin, List<string> ffArr)
    {
        string key = @"HKLM\Software\MozillaPlugins"; // The registry key to search
        RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(key); // Open the registry key

        if (registryKey == null)
        {
            return;
        }

        int index = 0;
        while (true)
        {
            string subKeyName = GetSubKeyNameAtIndex(registryKey, index);
            if (subKeyName == null)
            {
                break; // Exit if no more subkeys are found
            }

            // Read the 'path' value from the registry
            string path = registryKey.OpenSubKey(subKeyName)?.GetValue("path") as string;

            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                string cDate = File.GetLastWriteTime(path).ToString("yyyy-MM-dd HH:mm:ss");
                ffArr.Add($"FF {plugin}: {subKeyName} -> {path} [{cDate}]");
            }
            else
            {
                ffArr.Add($"FF {plugin}: {subKeyName} -> {path} [File Not Found]");
            }

            index++;
        }
    }

    // Helper method to get subkey names by index
    private static string GetSubKeyNameAtIndex(RegistryKey registryKey, int index)
    {
        try
        {
            return registryKey.GetSubKeyNames()[index]; // Get the subkey at the given index
        }
        catch
        {
            return null; // Return null if index is out of range
        }
    }

    public static bool FirefoxRemEx2(string filePath)
    {
        string userProfileDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string[] arrProf = GetFileListRecursively(userProfileDir,
            "extensions.ini||Thunderbird;Instantbird;uTorrent;PCDr;Notepad++;Quick Access Popup;dell;adobe;google;Microsoft;Macromedia;Dropbox*");

        if (arrProf == null || arrProf.Length == 0)
        {
            return false;
        }

        foreach (var profile in arrProf)
        {
            string extFileContent = File.ReadAllText(profile);
            if (!extFileContent.Contains(filePath))
            {
                continue;
            }

            string sec = Regex.Replace(extFileContent, @"(?is).*ExtensionDirs]*([^[]+?)\[+.*", "$1");
            string filePath1 = Regex.Replace(filePath, @"(\(|\)|\$|\\)", @"\\$1");
            sec = Regex.Replace(sec, @"(?is).*\R(.+?)=" + filePath1 + @"\R.*", "$1");

            bool result = DeleteIniKey(profile, "ExtensionDirs", sec);
            if (result)
            {
                if (File.ReadAllText(profile).Contains(filePath))
                {
                    FirefoxRemEx3(profile, sec, filePath1);
                }

                if (!File.ReadAllText(profile).Contains(filePath))
                {
                    WriteToLog($"{filePath} => {FilePathStatus.Deleted}");
                }
                else
                {
                    WriteToLog($"{filePath} => {FilePathStatus.NotDeleted}");
                }

                return true;
            }
        }

        return false;
    }

    // Placeholder for GetFileListRecursively method
    private static string[] GetFileListRecursively(string directory, string pattern)
    {
        // Implement the logic to recursively list files matching the pattern
        return Directory.GetFiles(directory, pattern, SearchOption.AllDirectories);
    }

    // Placeholder for DeleteIniKey method
    private static bool DeleteIniKey(string filePath, string section, string key)
    {
        // Implement logic to delete the specific key from INI file (returns true if successful)
        return true;
    }

    // Placeholder for logging
    private static void WriteToLog(string message)
    {
        // Implement logic to log the message
        Console.WriteLine(message); // Example logging
    }

    private enum FilePathStatus
    {
        Deleted,
        NotDeleted
    }

    public static void FirefoxRemEx3(string iniFilePath, string section, string filePath)
    {
        try
        {
            // Read the content of the INI file
            string readContent = File.ReadAllText(iniFilePath);

            // Replace the section and file path entry with an empty string
            string updatedContent = Regex.Replace(readContent,
                $"{Regex.Escape(section)}\\s*=\\s*{Regex.Escape(filePath)}\\v*", "");

            // Write the updated content back to the INI file
            File.WriteAllText(iniFilePath, updatedContent);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while processing the INI file: {ex.Message}");
        }
    }

    public static List<string> FirefoxProfilesMethod(List<string> profiles, List<string> ffArr)
    {
        List<string> arrProfiles = new List<string>();

        // Example directory, replace with the actual directory in use
        string appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string[] arrProf =
            GetFileListRecursively(appDataDir, "profiles.ini", 1 + 16, 1, 0, 2); // You can modify this method as needed

        foreach (var file in arrProf)
        {
            string basePath = Regex.Replace(file, @"(?i)(.+)\\profiles.ini", "$1");
            string iniFile = Path.Combine(basePath, "Profiles.ini");

            if (File.Exists(iniFile))
            {
                string fileContent = File.ReadAllText(iniFile);

                string[] sections = ReadIniSectionNames(fileContent);

                foreach (var section in sections)
                {
                    if (Regex.IsMatch(section, @"(?i)General|Install")) continue;

                    string profilePath = ReadIniSectionValue(fileContent, section, "Path");

                    if (!string.IsNullOrEmpty(profilePath))
                    {
                        if (ReadIniSectionValue(fileContent, section, "Default") == "1")
                        {
                            ffArr.Add($"FF DefaultProfile: {Path.GetFileName(profilePath)}");
                        }

                        if (ReadIniSectionValue(fileContent, section, "IsRelative") == "1")
                        {
                            profilePath = GetFullPath(profilePath, basePath);
                        }

                        if (!File.Exists(profilePath))
                        {
                            ffArr.Add($"FF ProfilePath: {profilePath} [Not Found] <==== Updated");
                        }
                        else
                        {
                            arrProfiles.Add(profilePath);
                        }
                    }
                }
            }
        }

        // Check for the fake path (C:\Windows\System32\x32\Data\profile)
        string fakeProfilePath = @"C:\Windows\System32\x32\Data\profile";
        if (File.Exists(fakeProfilePath))
        {
            string cDate = GetFileCreationDate(fakeProfilePath);
            ffArr.Add($"FF ProfilePath: {fakeProfilePath} [{cDate}] <==== Updated");
        }

        // Remove duplicates from the list
        arrProfiles = new List<string>(new HashSet<string>(arrProfiles));

        return arrProfiles;
    }

    // Method to get a list of files (recursive directory scan)
    public static string[] GetFileListRecursively(string directory, string pattern, int flags, int skipCount,
        int maxDepth, int recursive)
    {
        // Implement logic to list files recursively based on the pattern
        return Directory.GetFiles(directory, pattern, SearchOption.AllDirectories);
    }

    // Example method to read section names from an INI file content
    public static string[] ReadIniSectionNames(string iniContent)
    {
        // Replace this with actual INI file section reading logic
        return iniContent.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
    }

    // Example method to read a section value from an INI file content
    public static string ReadIniSectionValue(string iniContent, string section, string key)
    {
        // Replace with actual logic to read values from INI file content
        Match match = Regex.Match(iniContent, $@"\[{section}\].*{key}=(.*)");
        return match.Success ? match.Groups[1].Value : null;
    }

    // Example method to get the full path if it's relative
    public static string GetFullPath(string relativePath, string basePath)
    {
        return Path.Combine(basePath, relativePath);
    }


    

    public static void FirefoxSearchPluginMethod(string folderPath)
    {
        if (!Directory.Exists(folderPath)) return;
        string searchPluginsFolder = Path.Combine(folderPath, "searchplugins");

        if (!Directory.Exists(searchPluginsFolder)) return;

        string[] fileArray = Directory.GetFiles(searchPluginsFolder);
        foreach (var filePath in fileArray)
        {
            string fileDate = File.GetLastWriteTime(filePath).ToString("yyyy-MM-dd HH:mm:ss");
            string fileDateFormatted = $" [{fileDate}]";

            // Check the checkbox condition (assuming some logic for checkbox here)
            MainApp form1 = new MainApp();
            if (form1.checkEdit8.Checked) // This method should be implemented as per the GUI logic
            {
                // Initialize the array if not already done
                string[,] array = new string[100, 3];  // Adjust size as needed

                // Name/identifier for the entry
                string name = "FF SearchPlugin";

                // The value containing the file path and formatted date
                string value = $"{filePath}{fileDateFormatted}";

                // Add to the array (make sure to specify the array and the name/value)
                ArrayUtils.AddToArray(array, name, value);
            }
            else
            {

                // Initialize the 2D array with a fixed size
                string[,] array = new string[100, 2]; // Assuming a max of 100 entries

                // Get the file name
                string fileName = Path.GetFileName(filePath);

                // Check if the file name matches the regex pattern
                if (!Regex.IsMatch(fileName,
                        @"\A(?i)((bing|eBay|google|wikipedia|yahoo|twitter|amazon|amazondotcom|allegro|fbc|merlin|pwn|wp|wolnelektury|ddg|chambers|bolcom|marktplaats|cnrtl-tlfi|heureka|mapy|seznam|slunecnice|leo_ende_de|hoepli|drae|atlas|azet|dunaj|slovnik|zoznam)(-\w{2}|)(-GB|-france|)\.xml)"))
                {
                    // Name to be used in the array
                    string name = "FF SearchPlugin";
                    // Value to be used in the array
                    string value = $"{filePath}{fileDateFormatted}";

                    // Add the data to the array
                    ArrayUtils.AddToArray(array, name, value);
                }

                // Print the array for demonstration
                for (int i = 0; i < array.GetLength(0); i++)
                {
                    if (array[i, 0] != null)  // Only print non-empty entries
                    {
                        Console.WriteLine($"{array[i, 0]}: {array[i, 1]}");
                    }
                }
            }
        }
    }

    public static List<string> GetFirefoxProfiles()
    {
        string firefoxProfilesPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Mozilla",
            "Firefox",
            "Profiles");

        var profiles = new List<string>();

        if (Directory.Exists(firefoxProfilesPath))
        {
            foreach (var directory in Directory.GetDirectories(firefoxProfilesPath))
            {
                profiles.Add(directory);
            }
        }

        return profiles;
    }

    public static void ProcessFirefoxPreferences(string profilePath)
    {
        string prefsFilePath = Path.Combine(profilePath, "prefs.js");

        if (!File.Exists(prefsFilePath))
        {
            Console.WriteLine($"Preferences file not found in {profilePath}");
            return;
        }

        string[] preferences = File.ReadAllLines(prefsFilePath);

        foreach (var line in preferences)
        {
            if (line.StartsWith("user_pref", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine(line);
                // Add additional logic to process the preference line as needed
            }
        }
    }

    public static void ProcessFirefoxExtensionsInRegistry(string rootKey, string subKey, string userKey)
    {
        RegistryKey baseKey;

        // Determine the base registry key
        switch (rootKey.ToUpper())
        {
            case "HKLM":
                baseKey = Registry.LocalMachine;
                break;
            case "HKCU":
                baseKey = Registry.CurrentUser;
                break;
            case "HKU":
                baseKey = Registry.Users;
                break;
            default:
                throw new ArgumentException("Invalid root key. Use 'HKLM', 'HKCU', or 'HKU'.");
        }

        // Combine the subKey with the userKey
        string fullSubKey = $@"{userKey}\{subKey}";

        // Open the subKey and process it
        using (RegistryKey key = baseKey.OpenSubKey(fullSubKey))
        {
            if (key != null)
            {
                foreach (string valueName in key.GetValueNames())
                {
                    string valueData = key.GetValue(valueName)?.ToString();
                    Console.WriteLine($"User Key: {userKey}, Name: {valueName}, Data: {valueData}");
                }
            }
            else
            {
                Console.WriteLine($"Subkey '{fullSubKey}' not found in root '{rootKey}'.");
            }
        }
    }

    public static void ProcessFirefoxExtensionsInRegistry(string rootKey, string subKey)
    {
        // Call the three-argument method with a default userKey
        ProcessFirefoxExtensionsInRegistry(rootKey, subKey, string.Empty);
    }

    public static void ProcessFirefoxPlugins(string profilePath)
    {
        string pluginsPath = Path.Combine(profilePath, "plugins");

        if (Directory.Exists(pluginsPath))
        {
            foreach (var pluginFile in Directory.GetFiles(pluginsPath))
            {
                Console.WriteLine($"Processing plugin: {Path.GetFileName(pluginFile)}");
                // Add logic to process each plugin file
            }
        }
        else
        {
            Console.WriteLine($"No plugins directory found in: {profilePath}");
        }
    }

    public static void ProcessFirefoxPluginsInRegistry(string rootKey, string subKey)
    {
        RegistryKey baseKey;

        // Determine the base registry key
        switch (rootKey.ToUpper())
        {
            case "HKLM":
                baseKey = Registry.LocalMachine;
                break;
            case "HKCU":
                baseKey = Registry.CurrentUser;
                break;
            case "HKU":
                baseKey = Registry.Users;
                break;
            default:
                throw new ArgumentException("Invalid root key. Use 'HKLM', 'HKCU', or 'HKU'.");
        }

        // Open the registry subKey
        using (RegistryKey key = baseKey.OpenSubKey(subKey))
        {
            if (key != null)
            {
                foreach (var pluginName in key.GetValueNames())
                {
                    string pluginPath = key.GetValue(pluginName)?.ToString();
                    Console.WriteLine($"Plugin Name: {pluginName}, Plugin Path: {pluginPath}");
                }
            }
            else
            {
                Console.WriteLine($"Registry subKey '{subKey}' not found in root '{rootKey}'.");
            }
        }
    }
    public static void ProcessFirefoxPluginDirectories(string profilePath)
    {
        string pluginDir = Path.Combine(profilePath, "plugins");

        if (Directory.Exists(pluginDir))
        {
            Console.WriteLine($"Found plugin directory: {pluginDir}");
            foreach (var pluginFile in Directory.GetFiles(pluginDir))
            {
                Console.WriteLine($"Processing plugin file: {Path.GetFileName(pluginFile)}");
                // Add logic for processing each plugin file
            }
        }
        else
        {
            Console.WriteLine($"No plugins directory found for profile: {profilePath}");
        }
    }

    public static void ProcessStartMenuInternetEntries()
    {
        string registryPath = @"SOFTWARE\Clients\StartMenuInternet";

        using (RegistryKey key = Registry.LocalMachine.OpenSubKey(registryPath))
        {
            if (key != null)
            {
                foreach (string subKeyName in key.GetSubKeyNames())
                {
                    using (RegistryKey subKey = key.OpenSubKey(subKeyName))
                    {
                        string displayName = subKey?.GetValue("")?.ToString();
                        Console.WriteLine($"Internet Client: {displayName}");
                    }
                }
            }
            else
            {
                Console.WriteLine("No start menu internet entries found.");
            }
        }
    }

    public static void ProcessFirefoxExtensionsProgramFiles()
    {
        string programFilesPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        string extensionsPath = Path.Combine(programFilesPath, "Mozilla Firefox", "extensions");

        if (Directory.Exists(extensionsPath))
        {
            Console.WriteLine($"Found extensions directory: {extensionsPath}");
            foreach (var extensionDir in Directory.GetDirectories(extensionsPath))
            {
                Console.WriteLine($"Processing extension: {Path.GetFileName(extensionDir)}");
                // Add logic to process each extension
            }
        }
        else
        {
            Console.WriteLine($"No extensions directory found in: {extensionsPath}");
        }
    }
    public static string GetChromeExtensionName(string path)
    {
        string name = "";

        try
        {
            // Determine the manifest path
            string manifestPath = Regex.IsMatch(path, @"\\manifest.*\.json")
                ? path
                : Path.Combine(path, "manifest.json");

            if (!File.Exists(manifestPath))
            {
                Console.WriteLine("Manifest file not found.");
                return "";
            }

            // Read the manifest content
            string manifestContent = File.ReadAllText(manifestPath);

            // Check for "__MSG" pattern in "name"
            if (Regex.IsMatch(manifestContent, @"(?i)""name""\s*:\s*""__MSG"))
            {
                // Get default_locale
                string locale = Regex.Match(manifestContent, @"(?i)""default_locale""\s*:\s*""(.+?)""").Groups[1].Value;

                // Get MSG key
                string msgKey = Regex.Match(manifestContent, @"(?i)""name""\s*:.*MSG_(.+?)__""").Groups[1].Value;

                if (!string.IsNullOrEmpty(locale) && !string.IsNullOrEmpty(msgKey))
                {
                    string localeFilePath = Path.Combine(path, "_locales", locale, "messages.json");
                    if (File.Exists(localeFilePath))
                    {
                        string localeContent = File.ReadAllText(localeFilePath);
                        var localeJson = JObject.Parse(localeContent);
                        if (localeJson.TryGetValue(msgKey, out var messageToken))
                        {
                            name = messageToken["message"]?.ToString() ?? "";
                        }
                    }
                }
            }
            else
            {
                // Strip braces and simplify the manifest content
                manifestContent = Regex.Replace(manifestContent, @"(?s)^\s*\{|\s*\}\s*$", "");
                manifestContent = Regex.Replace(manifestContent, @"(?s)\{[^{]+?\}", "");
                manifestContent = Regex.Replace(manifestContent, @"(?s)\{[^{]+?\}", "");

                // Extract "name" field
                var match = Regex.Match(manifestContent, @"(?i)""name""\s*:\s*""(.+?)""");
                if (match.Success)
                {
                    name = Regex.Replace(match.Groups[1].Value, @"(?i)http(s|):", "hxxp$1:");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing extension: {ex.Message}");
        }

        return string.IsNullOrEmpty(name) ? "Unknown Name" : name;
    }
    public static bool GetCheckboxValue(System.Windows.Forms.CheckBox checkBox)
    {
        return checkBox.Checked;
    }

    public static void ProcessExtensionFileFolder(string path, int identifier)
    {
        if (Directory.Exists(path))
        {
            Console.WriteLine($"Processing folder: {path} with ID {identifier}");
            // Folder processing logic
        }
        else if (File.Exists(path))
        {
            Console.WriteLine($"Processing file: {path} with ID {identifier}");
            // File processing logic
        }
        else
        {
            Console.WriteLine($"Invalid path: {path}");
        }
    }

    public static void ProcessFirefoxExtensionFolder(string folderPath, int identifier)
    {
        if (Directory.Exists(folderPath))
        {
            Console.WriteLine($"Processing Firefox extension folder: {folderPath} with ID {identifier}");
            // Add your folder processing logic here
        }
        else
        {
            Console.WriteLine($"Folder does not exist: {folderPath}");
        }
    }

    
    public static List<string> GetProfileList(string profilesIniPath = @"C:\Users\YourUser\AppData\Roaming\Mozilla\Firefox\profiles.ini")
    {
        var profileList = new List<string>();

        if (File.Exists(profilesIniPath))
        {
            var lines = File.ReadAllLines(profilesIniPath);

            foreach (var line in lines)
            {
                // Look for profile directories (e.g., "Path=")
                if (line.StartsWith("Path=", StringComparison.OrdinalIgnoreCase))
                {
                    // Extract the profile path (assuming each profile has a Path= line)
                    var profilePath = line.Substring(5).Trim();
                    profileList.Add(profilePath);
                }
            }
        }
        else
        {
            Console.WriteLine($"Profiles file not found: {profilesIniPath}");
        }

        return profileList;
    }
    public static List<string> ReadIniSections(string filePath)
    {
        var sections = new List<string>();

        if (File.Exists(filePath))
        {
            string[] lines = File.ReadAllLines(filePath);

            // Regular expression to match sections: [SectionName]
            foreach (var line in lines)
            {
                var match = Regex.Match(line.Trim(), @"^\[(.+?)\]$");
                if (match.Success)
                {
                    sections.Add(match.Groups[1].Value); // Add section name without brackets
                }
            }
        }
        else
        {
            Console.WriteLine($"File not found: {filePath}");
        }

        return sections;
    }

    // Reads the value for a given key in a section from the ini file
    public static string ReadIniValue(string filePath, string section, string key)
    {
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"File not found: {filePath}");
            return null;
        }

        string[] lines = File.ReadAllLines(filePath);
        bool insideSection = false;

        // Iterate through lines in the ini file
        foreach (var line in lines)
        {
            string trimmedLine = line.Trim();

            // If we find the section, start looking for key-value pairs
            if (trimmedLine.Equals($"[{section}]", StringComparison.OrdinalIgnoreCase))
            {
                insideSection = true;
            }
            else if (insideSection)
            {
                // If we encounter a new section, stop searching
                if (trimmedLine.StartsWith("["))
                {
                    break;
                }

                // Match the key-value pair for the specified key
                var match = Regex.Match(trimmedLine, $@"^\s*{key}\s*=\s*(.*)$");
                if (match.Success)
                {
                    return match.Groups[1].Value.Trim(); // Return the value without leading/trailing spaces
                }
            }
        }

        Console.WriteLine($"Key '{key}' not found in section '{section}'");
        return null;
    }


    public static void GetFirefoxVersion()
    {
        // Check if either of the registry paths exist
        bool firefoxExists = RegistryKeyHandler.RegistryKeyExists(RegistryConstants.FirefoxRegistryCurrentUserUninstallPath) ||
                            RegistryKeyHandler.RegistryKeyExists(RegistryConstants.FirefoxRegistryLocalMachineUninstallPath);

        if (firefoxExists)
        {
            Logger.Instance.LogPrimary($"Firefox is installed.");
        }
        else
        {
            Logger.Instance.LogPrimary($"Firefox: Not Installed!");
        }
    }

}