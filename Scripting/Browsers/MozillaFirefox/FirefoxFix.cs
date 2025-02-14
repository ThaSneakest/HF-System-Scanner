using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Registry;
using Wildlands_System_Scanner.Utilities;

namespace Wildlands_System_Scanner.Scripting.Browsers.MozillaFirefox
{
    public class FirefoxFix
    {
        public static void FixFirefox(string fix, string bootMode)
        {
            if (bootMode != "Recovery")
            {
                var processList = ProcessUtils.GetProcessList();
                foreach (var process in processList)
                {
                    if (Regex.IsMatch(process.ProcessName, "(?i)firefox\\.exe"))
                    {
                        ProcessFix.CloseProcess(process.Id);
                    }
                }
            }

            if (fix.Contains("Extension:") || fix.Contains("user.js:"))
            {
                string filePath = Regex.Replace(fix, "(?i).* ([A-Z]:\\\\.+?) \\[.*", "$1");
                if (File.Exists(filePath))
                {
                    if (DirectoryUtils.IsDirectory(filePath))
                    {
                        DirectoryFix.MoveDirectory(filePath);
                    }
                    else
                    {
                        FileFix.MoveFile(filePath);
                    }
                    RemoveFirefoxExtension(filePath);
                }
                else
                {
                    if (!RemoveFirefoxExtension(filePath))
                    {
                        //   Logger.NotifyNotFound(filePath);
                    }
                }
            }

            if (fix.Contains("ProfilePath:"))
            {
                HandleProfilePath(fix);
            }

            if (Regex.IsMatch(fix, "(?i)(NetworkProxy|Homepage|NewTab|Session Restore|NewTabOverride|HomepageOverride|Notifications):"))
            {
                HandleFirefoxSettings(fix);
            }

            if (fix.Contains("FF Plugin:"))
            {
                HandleFirefoxPlugin(fix);
            }

            if (fix.Contains("SearchPlugin:"))
            {
                HandleSearchPlugin(fix);
            }

            if (fix.Contains("Plugin ProgramFiles/Appdata:"))
            {
                HandleProgramFilesPlugin(fix);
            }

            if (fix.Contains("Plugin HKU"))
            {
                HandleHKUPlugin(fix);
            }

            if (fix.Contains("ExtraCheck:"))
            {
                HandleExtraCheck(fix);
            }

            if (fix.Contains("HKLM\\SOFTWARE\\Policies\\Mozilla\\Firefox:"))
            {
                RegistryKeyHandler.DeleteRegistryKey("HKLM\\SOFTWARE\\Policies\\Mozilla");
            }

            if (fix.Contains("SOFTWARE\\Policies\\Mozilla\\Firefox:"))
            {
                string userKey = Regex.Replace(fix, "(?i)HKU\\\\(.+?)\\..+", "$1");
                RegistryKeyHandler.DeleteRegistryKey($"HKU\\{userKey}\\SOFTWARE\\Policies\\Mozilla");
            }
        }

        private static void HandleProfilePath(string fix)
        {
            string dirPath = Regex.Replace(fix, "(?i).* ([A-Z]:\\\\.+?) \\[.*", "$1");
            if (Regex.IsMatch(fix, "(?i).* ([A-Z]:\\\\.+?) \\[\\d"))
            {
                DirectoryFix.MoveDirectory(dirPath);
            }

            string profileName = Regex.Replace(dirPath, ".+\\\\(.+)", "$1");
            var profileList = FirefoxHandler.GetProfileList();
            foreach (var profileFile in profileList)
            {
                var sections = FirefoxHandler.ReadIniSections(profileFile);
                foreach (var section in sections)
                {
                    if (section == "General") continue;

                    string profilePath = FirefoxHandler.ReadIniValue(profileFile, section, "Path");
                    if (Regex.IsMatch(profilePath, $"(=|/|\\\\){profileName}$"))
                    {
                        DeleteIniSection(profileFile, section);
                    }
                }
            }
        }

        private static void HandleFirefoxSettings(string fix)
        {
            string profilePath = Regex.Replace(fix, ".+?: (.+?) ->.*", "$1");
            profilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), profilePath);

            if (fix.Contains("NewTabOverride:"))
            {
                ProcessExtensionSettings(profilePath, "extension-settings.json", "url_overrides", fix);
            }
            else if (fix.Contains("HomepageOverride:"))
            {
                ProcessExtensionSettings(profilePath, "extension-settings.json", "homepage_override", fix);
            }
            else if (fix.Contains("NetworkProxy:"))
            {
                ProcessPrefsJs(profilePath, "prefs.js", "network.proxy");
            }
            else if (fix.Contains("Homepage:"))
            {
                ProcessPrefsJs(profilePath, "prefs.js", "browser.startup.homepage");
            }
            else if (fix.Contains("NewTab:"))
            {
                ProcessPrefsJs(profilePath, "prefs.js", "browser.newtab.url");
            }
            else if (fix.Contains("Session Restore:"))
            {
                ProcessPrefsJs(profilePath, "prefs.js", "browser.startup.page");
            }
        }

        private static void HandleFirefoxPlugin(string fix)
        {
            string pluginKey = Regex.Replace(fix, "(?i)FF Plugin: (.+?) ->.*", "$1");
            string keyPath = $"HKLM\\Software\\MozillaPlugins\\{pluginKey}";
            RegistryKeyHandler.DeleteRegistryKey(keyPath);

            string filePath = Regex.Replace(fix, ".+-> (.+\\.dll) .+", "$1");
            if (File.Exists(filePath))
            {
                FileFix.MoveFile(filePath);
            }
            else
            {
                //  Logger.NotifyNotFound(filePath);
            }
        }

        private static void HandleSearchPlugin(string fix)
        {
            string filePath = Regex.Replace(fix, "(?i)FF SearchPlugin: (.+) \\[.*", "$1");
            if (File.Exists(filePath))
            {
                FileFix.MoveFile(filePath);
            }
            else
            {
                //  Logger.NotifyNotFound(filePath);
            }
        }

        private static void HandleProgramFilesPlugin(string fix)
        {
            string filePath = Regex.Replace(fix, "(?i)FF Plugin ProgramFiles/Appdata: (.+\\.dll) .+", "$1");
            if (File.Exists(filePath))
            {
                FileFix.MoveFile(filePath);
            }
            else
            {
                //  Logger.NotifyNotFound(filePath);
            }
        }

        private static void HandleHKUPlugin(string fix)
        {
            string userKey = Regex.Replace(fix, "(?i)FF Plugin HKU\\\\(.+?):.+", "$1");
            string pluginKey = Regex.Replace(fix, "(?i)FF Plugin HKU\\\\.+?: (.+) -> .+\\.dll", "$1");
            string filePath = Regex.Replace(fix, "(?i)FF Plugin HKU\\\\.+?: .+ -> (.+\\.dll) .+", "$1");
            string keyPath = $"HKU\\{userKey}\\Software\\MozillaPlugins\\{pluginKey}";

            RegistryKeyHandler.DeleteRegistryKey(keyPath);
            if (File.Exists(filePath))
            {
                FileFix.MoveFile(filePath);
            }
            else
            {
                //  Logger.NotifyNotFound(filePath);
            }
        }

        private static void HandleExtraCheck(string fix)
        {
            string filePath = Regex.Replace(fix, "(?i)FF ExtraCheck: (.+?) \\[.+", "$1");
            if (File.Exists(filePath))
            {
                FileFix.MoveFile(filePath);
            }
            else
            {
                //  Logger.NotifyNotFound(filePath);
            }
        }

        public static void FirefoxExtensionRegFix(string fix)
        {
            // Check if the fix contains HKLM
            if (fix.Contains("HKLM\\"))
            {
                string key = Regex.Replace(fix, @"(?i)FF HKLM\\\.\.\.\\(.+\\Extensions): \[.+", "$1");
                key = "HKLM\\Software\\Mozilla\\" + key;

                string value = Regex.Replace(fix, @"(?i)FF HKLM\\.+\\Extensions: \[(.+)\] -.*", "$1");

                // Delete registry value
                RegistryValueHandler.DeleteRegistryValue(key, value);
            }

            // Check if the fix contains HKU
            if (fix.Contains("HKU\\"))
            {
                string user = Regex.Replace(fix, @"(?i)FF HKU\\(.+?)\..+", "$1");
                string key = Regex.Replace(fix, @"(?i)FF HKU\\.+?\\\.\.\.\\(.+\\Extensions): \[.+", "$1");
                key = "HKU\\" + user + "\\Software\\Mozilla\\" + key;

                string value = Regex.Replace(fix, @"(?i)FF HKU\\.+\\Extensions: \[(.+)\] -.*", "$1");

                // Delete registry value
                RegistryValueHandler.DeleteRegistryValue(key, value);
            }
        }

        public static bool RemoveFirefoxExtension(string filePath)
        {
            try
            {
                if (Directory.Exists(filePath))
                {
                    // Remove directory
                    Directory.Delete(filePath, true);
                    Console.WriteLine($"Removed Firefox extension directory: {filePath}");
                }
                else if (File.Exists(filePath))
                {
                    // Remove file
                    File.Delete(filePath);
                    Console.WriteLine($"Removed Firefox extension file: {filePath}");
                }
                else
                {
                    // File or directory not found
                    Console.WriteLine($"Firefox extension not found: {filePath}");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing Firefox extension: {ex.Message}");
                return false;
            }
        }

        public static void ProcessExtensionSettings(string profilePath, string settingsFile, string settingKey, string fix)
        {
            // Build the full path to the extension settings file
            string settingsFilePath = Path.Combine(profilePath, settingsFile);

            // Check if the settings file exists
            if (File.Exists(settingsFilePath))
            {
                // Read the content of the settings file
                string fileContent = File.ReadAllText(settingsFilePath);

                // Parse the JSON content
                JObject json = JObject.Parse(fileContent);

                // Check if the key exists and modify it as needed
                if (json.ContainsKey(settingKey))
                {
                    // Perform the fix or modification (replace with appropriate logic)
                    string ext = Regex.Match(fix, @"(?i)led: (.+)").Groups[1].Value;
                    JToken settingValue = json[settingKey];

                    // Check if the extension exists in the settings and make necessary changes
                    if (!settingValue.ToString().Contains(ext))
                    {
                        // Modify the setting (Example: add new value or modify it)
                        settingValue.Last.AddAfterSelf(JToken.Parse($"\"{ext}\": {{}}"));

                        // Save the modified settings back to the file
                        File.WriteAllText(settingsFilePath, json.ToString());
                        Console.WriteLine($"Updated {settingKey} in {settingsFilePath}");
                    }
                }
                else
                {
                    Console.WriteLine($"Setting key '{settingKey}' not found in {settingsFilePath}");
                }
            }
            else
            {
                Console.WriteLine($"Settings file not found: {settingsFilePath}");
            }
        }

        public static void ProcessPrefsJs(string profilePath, string prefsFile, string settingKey)
        {
            string prefsFilePath = Path.Combine(profilePath, prefsFile);

            if (File.Exists(prefsFilePath))
            {
                // Read the content of prefs.js
                string prefsContent = File.ReadAllText(prefsFilePath);

                // Check if the key exists in prefs.js
                string pattern = $@"user_pref\(""{settingKey}"",.*\);";
                Match match = Regex.Match(prefsContent, pattern);

                if (match.Success)
                {
                    // Modify the value (Example: remove the proxy setting)
                    prefsContent = Regex.Replace(prefsContent, pattern, $"user_pref(\"{settingKey}\", \"\");");

                    // Write the modified content back to prefs.js
                    File.WriteAllText(prefsFilePath, prefsContent);
                    Console.WriteLine($"Modified {settingKey} in {prefsFilePath}");
                }
                else
                {
                    Console.WriteLine($"{settingKey} not found in {prefsFilePath}");
                }
            }
            else
            {
                Console.WriteLine($"Prefs file not found: {prefsFilePath}");
            }
        }

        public static bool DeleteIniSection(string filePath, string section)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"File not found: {filePath}");
                return false;
            }

            var lines = File.ReadAllLines(filePath);
            var newLines = new System.Collections.Generic.List<string>();
            bool insideSection = false;

            // Iterate through lines in the ini file
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();

                // If we find the section header, start skipping lines
                if (trimmedLine.Equals($"[{section}]", StringComparison.OrdinalIgnoreCase))
                {
                    insideSection = true;
                }
                else if (insideSection)
                {
                    // If we encounter another section, stop skipping
                    if (trimmedLine.StartsWith("["))
                    {
                        insideSection = false;
                    }
                    continue; // Skip the lines inside the section
                }

                // Add the line to the new list if it's not part of the section
                newLines.Add(line);
            }

            // If we found the section, rewrite the file
            if (insideSection)
            {
                // Write the new content back to the file
                File.WriteAllLines(filePath, newLines);
                Console.WriteLine($"Section '{section}' deleted.");
                return true;
            }
            else
            {
                Console.WriteLine($"Section '{section}' not found.");
                return false;
            }
        }

    }
}
