using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Registry;
using Wildlands_System_Scanner.Utilities;

namespace Wildlands_System_Scanner.Scripting.Browsers.MicrosoftEdge
{
    public class EdgeFix
    {
        public static void FixEdge(string fix, ref List<string> log)
        {
            // Close any running Edge processes
            CloseEdgeProcesses();

            // Handle specific cases based on the `fix` string
            if (fix.Contains(@"HKLM\SOFTWARE\Policies\Microsoft\Edge:"))
            {
                RegistryKeyHandler.DeleteRegistryKey(@"HKLM\SOFTWARE\Policies\Microsoft\Edge");
                return;
            }
            else if (fix.Contains(@"SOFTWARE\Policies\Microsoft\Edge:"))
            {
                string user = Regex.Replace(fix, @"(?i)HKU\\([^\\]+)\\.+", "$1");
                RegistryKeyHandler.DeleteRegistryKey($@"HKU\{user}\SOFTWARE\Policies\Microsoft\Edge");
                return;
            }
            else if (Regex.IsMatch(fix, @"Edge (HKLM|HKU)\\") && fix.Contains(@"\Extension"))
            {
                HandleEdgeExtensionFix(fix);
                return;
            }
            else if (Regex.IsMatch(fix, @"(HomeButtonPage|Restore|Notifications|Extension):"))
            {
                HandleEdgeSettings(fix);
                return;
            }
            else if (Regex.IsMatch(fix, @"(HomePage|StartupUrls|DefaultSearchKeyword|RestoreOnStartup|NewTab|DefaultSearchURL|DefaultSuggestURL|DefaultNewTabURL|Session Restore|crx|Notifications):"))
            {
                HandleEdgeProfileSettings(fix);
                return;
            }
            else if (fix.Contains("Profile:"))
            {
                string filePath = Regex.Replace(fix, @"(?i).*([A-Z]:\\.+?) \[.*", "$1");
                DirectoryFix.MoveDirectory(filePath);
                return;
            }
        }

        private static void CloseEdgeProcesses()
        {
            var processes = Process.GetProcesses();
            foreach (var process in processes)
            {
                if (Regex.IsMatch(process.ProcessName, @"(?i)(MicrosoftEdge|MicrosoftEdgeCP|browser_broker|msedge)\.exe"))
                {
                    try
                    {
                        process.Kill();
                    }
                    catch
                    {
                        // Ignore any exceptions during process termination
                    }
                }
            }
        }

        private static void HandleEdgeExtensionFix(string fix)
        {
            string subKey = Regex.Replace(fix, @".+? \[(.+?)\].*", "$1");

            if (fix.Contains("Edge HKLM\\"))
            {
                RegistryKeyHandler.DeleteRegistryKey($@"HKLM\SOFTWARE\Microsoft\Edge\Extensions\{subKey}");
            }
            else if (fix.Contains("Edge HKU\\"))
            {
                string user = Regex.Replace(fix, @"Edge HKU\\(.+?)\\.+", "$1");
                RegistryKeyHandler.DeleteRegistryKey($@"HKU\{user}\SOFTWARE\Microsoft\Edge\Extensions\{subKey}");
            }

            string filePath = Regex.Replace(fix, @".+ \[.+\] - (.+) \[.*\]", "$1");

            if (File.Exists(filePath))
            {
                DirectoryFix.MoveDirectory(filePath);
            }
            else
            {
                Console.WriteLine($"File not found: {filePath}");
            }
        }

        private static void HandleEdgeSettings(string fix)
        {
            if (fix.Contains("HomeButtonPage:") || fix.Contains("Restore: HKU"))
            {
                string user = Regex.Replace(fix, @".*HKU\\(.+?) ->.*", "$1");
                string baseKey = $@"HKU\{user}\Software\Classes\Local Settings\Software\Microsoft\Windows\CurrentVersion\AppContainer\Storage\microsoft.microsoftedge_8wekyb3d8bbwe\MicrosoftEdge";

                if (fix.Contains("HomeButtonPage:"))
                {
                    RegistryKeyHandler.DeleteRegistryKey($@"{baseKey}\Main\HomeButtonPage");
                }
                else if (fix.Contains("Restore: HKU"))
                {
                    RegistryKeyHandler.DeleteRegistryKey($@"{baseKey}\ContinuousBrowsing");
                }
            }
        }

        private static void HandleEdgeProfileSettings(string fix)
        {
            string profile = Regex.Replace(fix, @"Edge .+?: (.+?) ->.*", "$1");
            string preferencesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), $@"Microsoft\Edge\User Data\{profile}\Preferences");
            string securePreferencesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), $@"Microsoft\Edge\User Data\{profile}\Secure Preferences");

            if (fix.Contains("StartupUrls:"))
            {
                ModifyPreferenceFile(securePreferencesPath, @"(?is)\s*""startup_urls""\s*:\s*\[[^]]*?\]\s*", "");
            }
            else if (fix.Contains("DefaultSearchKeyword:"))
            {
                ModifyPreferenceFile(securePreferencesPath, @",\s*""keyword""\s*:\s*""[^""]*?(?:[a-z]|\.)+.*?"",?\s*", "");
            }
            else if (fix.Contains("HomePage:"))
            {
                ModifyPreferenceFile(securePreferencesPath, @"\s*""homepage""\s*:\s*""[^""]*?(?:[a-z]|\.)+.*?"",?\s*", "");
            }
            else if (fix.Contains("RestoreOnStartup:"))
            {
                ModifyPreferenceFile(securePreferencesPath, @"(?is)\s*""urls_to_restore_on_startup""\s*:\s*\[[^]]*?\]\s*", "");
            }
            else if (fix.Contains("NewTab:"))
            {
                ModifyPreferenceFile(preferencesPath, @"(?is)\s*""Edge_url_overrides""\s*:.+?,""newtab""[^]]*\]\},?\s*", "");
            }
            else if (fix.Contains("DefaultSearchURL:"))
            {
                ModifyPreferenceFile(securePreferencesPath, @",\s*""(?:search_|)url""\s*:\s*""[^""]*?(?:[a-z]|\.)+.*?"",?\s*", "");
            }
        }

        private static void ModifyPreferenceFile(string filePath, string pattern, string replacement)
        {
            if (File.Exists(filePath))
            {
                string content = File.ReadAllText(filePath);
                string modifiedContent = Regex.Replace(content, pattern, replacement, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                File.WriteAllText(filePath, modifiedContent);
                Console.WriteLine($"Modified preferences: {filePath}");
            }
            else
            {
                Console.WriteLine($"Preferences file not found: {filePath}");
            }
        }
    }
}
