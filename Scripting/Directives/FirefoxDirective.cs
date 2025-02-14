using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scripting.Directives
{
    public class FirefoxDirective
    {
        private static readonly string FirefoxProfilesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Mozilla\\Firefox\\Profiles");

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
                    if (line.StartsWith("Firefox::"))
                    {
                        string command = line.Substring("Firefox::".Length).Trim();
                        ProcessFirefoxCommand(command);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static void ProcessFirefoxCommand(string command)
        {
            string[] parts = command.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 2)
            {
                Console.WriteLine($"Invalid Firefox:: directive: {command}");
                return;
            }

            string action = parts[0].ToLower();
            string argument = parts[1];

            try
            {
                switch (action)
                {
                    case "edit":
                        EditPreference(argument);
                        break;
                    case "remove":
                        RemovePreference(argument);
                        break;
                    case "reset":
                        ResetPreferences();
                        break;
                    default:
                        Console.WriteLine($"Unsupported Firefox:: action: {action}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing Firefox:: {action}: {ex.Message}");
            }
        }

        private static void EditPreference(string preference)
        {
            string[] keyValue = preference.Split(new[] { '=' }, 2);
            if (keyValue.Length != 2)
            {
                Console.WriteLine($"Invalid preference format for edit: {preference}");
                return;
            }

            string key = keyValue[0].Trim();
            string value = keyValue[1].Trim();

            foreach (string profilePath in Directory.GetDirectories(FirefoxProfilesPath))
            {
                string prefsPath = Path.Combine(profilePath, "prefs.js");
                if (File.Exists(prefsPath))
                {
                    string[] lines = File.ReadAllLines(prefsPath);
                    bool updated = false;

                    for (int i = 0; i < lines.Length; i++)
                    {
                        if (lines[i].StartsWith($"user_pref(\"{key}\""))
                        {
                            lines[i] = $"user_pref(\"{key}\", {value});";
                            updated = true;
                            break;
                        }
                    }

                    if (!updated)
                    {
                        File.AppendAllText(prefsPath, $"user_pref(\"{key}\", {value});{Environment.NewLine}");
                    }
                    else
                    {
                        File.WriteAllLines(prefsPath, lines);
                    }

                    Console.WriteLine($"Preference edited: {key} = {value}");
                }
            }
        }

        private static void RemovePreference(string preference)
        {
            foreach (string profilePath in Directory.GetDirectories(FirefoxProfilesPath))
            {
                string prefsPath = Path.Combine(profilePath, "prefs.js");
                if (File.Exists(prefsPath))
                {
                    string[] lines = File.ReadAllLines(prefsPath);
                    File.WriteAllLines(prefsPath, Array.FindAll(lines, line => !line.StartsWith($"user_pref(\"{preference}\"")));

                    Console.WriteLine($"Preference removed: {preference}");
                }
            }
        }

        private static void ResetPreferences()
        {
            foreach (string profilePath in Directory.GetDirectories(FirefoxProfilesPath))
            {
                string prefsPath = Path.Combine(profilePath, "prefs.js");
                if (File.Exists(prefsPath))
                {
                    File.Delete(prefsPath);
                    Console.WriteLine($"Preferences reset for profile: {profilePath}");
                }
            }
        }
    }
}
