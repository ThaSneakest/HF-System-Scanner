using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Wildlands_System_Scanner.Scripting;

public class OperaFix
{
    public static void FixOpera(string fix)
    {
        string profile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Opera Software\Opera Stable");
        string path = Path.Combine(profile, "Preferences");

        if (Regex.IsMatch(fix, "(DefaultSearchKeyword|DefaultSearchURL):", RegexOptions.IgnoreCase))
        {
            path = Path.Combine(profile, "Secure Preferences");
        }

        string preferences = File.ReadAllText(path);
        string tempPath = Path.Combine(Path.GetTempPath(), "preferences00");

        using (StreamWriter writer = new StreamWriter(tempPath))
        {
            Process[] processes = Process.GetProcessesByName("opera");
            foreach (var process in processes)
            {
                process.Kill();
            }

            if (fix.Contains("Notifications:"))
            {
                if (Regex.IsMatch(preferences, @"(?i),""notifications"":\{\},"))
                {
                //    Logger.NotifyNotFound("OPR Notifications:");
                    return;
                }

                preferences = Regex.Replace(preferences, @"(?is),""notifications"":\{.+?\}\},", @",""notifications"":{},");

                writer.Write(preferences);
                if (File.Exists(path))
                {
                    File.Delete(path); // Delete the existing file if it exists
                }

                File.Move(tempPath, path); // Move the file to the target location
                FileFix.MarkDeleted("OPR Notifications");
            }
            else if (fix.Contains("DefaultSuggestURL:"))
            {
                if (!Regex.IsMatch(preferences, @"\s*""suggest(?:ions|)_url""\s*:\s*""[^""]*?(?:[a-z]|\.)+.*?""", RegexOptions.IgnoreCase))
                {
                //    Logger.NotifyNotFound("Opera DefaultSuggestURL");
                    return;
                }

                preferences = Regex.Replace(preferences, @"\s*\""suggest(?:ions|)_url\""\s*:\s*\""[^\""]*?(?:[a-z]|\.)+.*?\"",?,?\r?\n*", "", RegexOptions.IgnoreCase);




                writer.Write(preferences);
                if (File.Exists(path))
                {
                    File.Delete(path); // Delete the existing file
                }

                File.Move(tempPath, path); // Move the file

                FileFix.MarkDeleted(fix);
            }
            else if (fix.Contains("StartupUrls:"))
            {
                if (!Regex.IsMatch(preferences, @"(?i)""startup_urls""\s*:\s*\[.*?\]", RegexOptions.IgnoreCase))
                {
                //    Logger.NotifyNotFound(fix);
                    return;
                }

                preferences = Regex.Replace(preferences, @"(?is)\s*""startup_urls""\s*:\s*\[.+?\],?,?\r?\n*", "", RegexOptions.IgnoreCase);


                writer.Write(preferences);
                if (File.Exists(path))
                {
                    File.Delete(path); // Delete the existing file
                }

                File.Move(tempPath, path); // Move the file

                FileFix.MarkDeleted(fix);
            }
            else if (fix.Contains("Session Restore:"))
            {
                var matches = Regex.Matches(preferences, @"""restore_on_startup""\s*:\s*(\d)(}|,)");
                if (matches.Count == 0 || matches[0].Groups[1].Value != "1")
                {
                 //   Logger.NotifyNotFound("Opera Session Restore:");
                    return;
                }

                preferences = Regex.Replace(preferences, @"""restore_on_startup""\s*:\s*\d,", @"""restore_on_startup"":5,", RegexOptions.IgnoreCase);
                preferences = Regex.Replace(preferences, @"""restore_on_startup""\s*:\s*\d}", @"""restore_on_startup"":5}", RegexOptions.IgnoreCase);

                writer.Write(preferences);
                if (File.Exists(path))
                {
                    File.Delete(path); // Delete the existing file at the destination
                }

                File.Move(tempPath, path); // Move the file to the destination

                FileFix.MarkDeleted(fix);
            }
            else if (fix.Contains("DefaultSearchKeyword:"))
            {
                if (!Regex.IsMatch(preferences, @",\s*""keyword""\s*:\s*""[^""]*?(?:[a-z]|\.)+.*?""", RegexOptions.IgnoreCase))
                {
                 //   Logger.NotifyNotFound("Opera DefaultSearchKeyword");
                    return;
                }

                preferences = Regex.Replace(preferences, @",\s*""keyword""\s*:\s*""[^""]*?(?:[a-z]|\.)+.*?"",?\r?\n*", "", RegexOptions.IgnoreCase);


                writer.Write(preferences);
                if (File.Exists(path))
                {
                    File.Delete(path); // Delete the existing file at the destination
                }

                File.Move(tempPath, path); // Move the file to the destination

                FileFix.MarkDeleted(fix);
            }
            else if (fix.Contains("DefaultSearchURL:"))
            {
                if (!Regex.IsMatch(preferences, @",\s*""(?:search_|)url""\s*:\s*""[^""]*?(?:[a-z]|\.)+.*?""", RegexOptions.IgnoreCase))
                {
                 //   Logger.NotifyNotFound(fix);
                    return;
                }

                preferences = Regex.Replace(preferences, @",\s*""(?:search_|)url""\s*:\s*""[^""]*?(?:[a-z]|\.)+.*?"",?\r?\n*", "", RegexOptions.IgnoreCase);


                writer.Write(preferences);
                if (File.Exists(path))
                {
                    File.Delete(path); // Delete the existing file at the destination
                }

                File.Move(tempPath, path); // Move the file to the destination

                FileFix.MarkDeleted(fix);
            }
        }
    }
}
