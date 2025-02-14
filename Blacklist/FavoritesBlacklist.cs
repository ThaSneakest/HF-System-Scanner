using System;
using System.Collections.Generic;
using System.IO;

namespace Wildlands_System_Scanner.Blacklist
{
    public class FavoritesBlacklist
    {
        public static void ScanForBlacklistedFiles()
        {
            // Predefined blacklist of favorite file names (case insensitive)
            HashSet<string> blacklistedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "suspicious_favorite.url",  // Example: specific suspicious shortcut
                "malicious_link.url",       // Example: malicious URL shortcut
                "phishing_site.url",        // Example: phishing site link
                "unknown_favorite.html",    // Example: unknown HTML favorite
                "dangerous_redirect.url"    // Example: dangerous redirect link
            };

            // Path to the Favorites directory
            string favoritesPath = @"C:\Users\12565\Favorites";

            Console.WriteLine($"Scanning files in {favoritesPath}...");

            try
            {
                // Ensure the directory exists
                if (!Directory.Exists(favoritesPath))
                {
                    Console.WriteLine($"Directory not found: {favoritesPath}");
                    return;
                }

                // Get all files in Favorites directory and subdirectories
                string[] files = Directory.GetFiles(favoritesPath, "*.*", SearchOption.AllDirectories);

                // Check each file against the blacklist
                foreach (string file in files)
                {
                    string fileName = Path.GetFileName(file);

                    if (blacklistedFiles.Contains(fileName))
                    {
                        Logger.Instance.LogPrimary($"Blacklisted file found: {file}");
                        // Optionally, take action (e.g., delete or quarantine the file)
                        // File.Delete(file); // Be cautious with this line!
                    }
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Access denied to some directories: {ex.Message}");
            }
            catch (PathTooLongException ex)
            {
                Console.WriteLine($"File path too long: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during scanning: {ex.Message}");
            }

            Console.WriteLine("Scan complete.");
        }
    }
}
