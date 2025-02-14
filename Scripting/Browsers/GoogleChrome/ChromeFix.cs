using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scripting.Browsers.GoogleChrome
{
    public class ChromeFix
    {
        public static int CHROMEFIX(string fix)
        {
            string filep = "";
            string bro = Regex.Replace(fix, @"(\w{3}) .+", "$1");
            string fol = "";
            string bro1 = "";

            switch (bro)
            {
                case "CHR":
                    bro = "Chrome";
                    fol = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Google\Chrome\User Data\");
                    break;

                case "BRA":
                    bro = "Brave";
                    fol = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"BraveSoftware\Brave-Browser\User Data\");
                    break;

                case "VIV":
                    bro = "Vivaldi";
                    fol = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Vivaldi\User Data\");
                    break;

                case "YAN":
                    bro = "Yandex";
                    fol = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Yandex\YandexBrowser\User Data\");
                    break;
            }

            if (bro == "Yandex")
            {
                bro1 = "Browser";
            }
            else
            {
                bro1 = bro;
            }

            var arrayPro = Process.GetProcesses(); // Get all processes
            foreach (var process in arrayPro)
            {
                if (Regex.IsMatch(process.ProcessName, $"(?i){bro1}\\.exe"))
                {
                    process.Kill();
                }
            }

            if (Regex.IsMatch(fix, "DefaultSearchProvider:"))
            {
                File.AppendAllText("HFIXLOG.txt", fix + " ==> " + "CHR1" + "." + Environment.NewLine);
            }

            if (Regex.IsMatch(fix, @"(?i)(HomePage|StartupUrls|DefaultSearchKeyword|RestoreOnStartup|NewTab|DefaultSearchURL|DefaultSuggestURL|DefaultNewTabURL|Session Restore|crx|Notifications|Custom_url):"))
            {
                string profile = Regex.Replace(fix, @"^\w{3} .+?: (.+?) ->.*", "$1");
                string path = Path.Combine(fol, profile, "preferences");
                string pathSec = Path.Combine(fol, profile, "Secure Preferences");

                string preferences = File.ReadAllText(path);
                string secPreferences = File.ReadAllText(pathSec);

                string tempPrefPath = Path.Combine(Path.GetTempPath(), "preferences00");

                if (fix.Contains("Custom_url:"))
                {
                    if (!Regex.IsMatch(preferences, @"""custom_url""\s*:\s*"""))
                        return 0; // NFOUND method in C#

                    preferences = Regex.Replace(preferences, @"(?is)""tabs""\s*:\s*{.*""new_page""\s*:\s*{""custom_url""\s*:\s*""(.+?)""}},\s*(\r\n|\r|\n)*", "");


                    File.WriteAllText(tempPrefPath, preferences);
                    if (File.Exists(path))
                    {
                        File.Delete(path);  // Delete the destination file if it exists
                    }

                    File.Move(tempPrefPath, path);  // Move the file to the destination

                }

                // Similar replacements for other conditions here...
                // Use Regex.Replace as needed to modify the preferences files
            }

            // Handle other conditions (Session Restore, DefaultSearchURL, etc.)
            // For example:
            if (Regex.IsMatch(fix, @"(?i)""session_restore""\s*:\s*(\d)(}|,)"))
            {
                // Update session restore logic
            }

            return 1;
        }
    }
}
