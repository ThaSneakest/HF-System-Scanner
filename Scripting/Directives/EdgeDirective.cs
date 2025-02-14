using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scripting.Directives
{
    public class EdgeDirective
    {
        private static readonly string EdgeProfilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Microsoft\Edge\User Data\Default");

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
                    if (line.StartsWith("Edge::"))
                    {
                        string command = line.Substring("Edge::".Length).Trim();
                        ProcessEdgeCommand(command);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static void ProcessEdgeCommand(string command)
        {
            string[] parts = command.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 2)
            {
                Console.WriteLine($"Invalid Edge:: directive: {command}");
                return;
            }

            string action = parts[0].ToLower();
            string argument = parts[1];

            try
            {
                switch (action)
                {
                    case "edit":
                        EditEdgePreference(argument);
                        break;
                    case "remove":
                        RemoveEdgePreference(argument);
                        break;
                    case "reset":
                        ResetEdgePreferences();
                        break;
                    default:
                        Console.WriteLine($"Unsupported Edge:: action: {action}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing Edge:: {action}: {ex.Message}");
            }
        }

        private static void EditEdgePreference(string preference)
        {
            string[] keyValue = preference.Split(new[] { '=' }, 2);
            if (keyValue.Length != 2)
            {
                Console.WriteLine($"Invalid preference format for edit: {preference}");
                return;
            }

            string key = keyValue[0].Trim();
            string value = keyValue[1].Trim();

            string preferencesPath = Path.Combine(EdgeProfilePath, "Preferences");
            if (!File.Exists(preferencesPath))
            {
                Console.WriteLine("Edge Preferences file not found.");
                return;
            }

            var json = JObject.Parse(File.ReadAllText(preferencesPath));

            // Update or add the preference key
            json[key] = value;
            File.WriteAllText(preferencesPath, json.ToString());
            Console.WriteLine($"Preference edited: {key} = {value}");
        }

        private static void RemoveEdgePreference(string preference)
        {
            string preferencesPath = Path.Combine(EdgeProfilePath, "Preferences");
            if (!File.Exists(preferencesPath))
            {
                Console.WriteLine("Edge Preferences file not found.");
                return;
            }

            var json = JObject.Parse(File.ReadAllText(preferencesPath));

            // Remove the key
            if (json.ContainsKey(preference))
            {
                json.Remove(preference);
                File.WriteAllText(preferencesPath, json.ToString());
                Console.WriteLine($"Preference removed: {preference}");
            }
            else
            {
                Console.WriteLine($"Preference not found: {preference}");
            }
        }

        private static void ResetEdgePreferences()
        {
            string preferencesPath = Path.Combine(EdgeProfilePath, "Preferences");
            if (File.Exists(preferencesPath))
            {
                File.Delete(preferencesPath);
                Console.WriteLine("Edge Preferences reset to default.");
            }
            else
            {
                Console.WriteLine("Edge Preferences file not found.");
            }
        }
    }
}
