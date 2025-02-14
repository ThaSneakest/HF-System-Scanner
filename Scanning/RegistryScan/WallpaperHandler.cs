using System;
using System.IO;
using System.Linq;
using Microsoft.Win32;
using Wildlands_System_Scanner.Registry;

public class WallpaperHandler
{
    public void WALLPAPER()
    {
        // Example list of user registry keys, should be populated as required
        string[] userReg = RegistryUserHandler.GetUserRegistryKeys().ToArray();

        foreach (string user in userReg)
        {
            // Skip specific well-known user identifiers (S-1-5-19, S-1-5-20, .default)
            if (user.Equals("S-1-5-19", StringComparison.OrdinalIgnoreCase) ||
                user.Equals("S-1-5-20", StringComparison.OrdinalIgnoreCase) ||
                user.Equals(".default", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            string desktopKeyPath = $@"{user}\Control Panel\Desktop";

            try
            {
                // Open the registry key for the user
                using (RegistryKey userKey = Registry.Users.OpenSubKey(desktopKeyPath))
                {
                    if (userKey != null)
                    {
                        // Read the "Wallpaper" value
                        string wallpaperPath = userKey.GetValue("Wallpaper")?.ToString();

                        if (!string.IsNullOrEmpty(wallpaperPath))
                        {
                            string logEntry = $"{desktopKeyPath}\\Wallpaper -> {wallpaperPath}{Environment.NewLine}";
                            File.AppendAllText("WallpaperLog.txt", logEntry); // Log the result to a file
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error accessing wallpaper registry key for user {user}: {ex.Message}");
            }
        }
    }

    public static void WallpaperScan()
    {
        string WallPaperRegistryPath = @"Control Panel\Desktop";

        RegistryKey desktopKey = Registry.CurrentUser.OpenSubKey(WallPaperRegistryPath);

        if (desktopKey != null)
        {
            string wallpaper = desktopKey.GetValue("WallPaper") as string;

            Logger.Instance.LogPrimary($"Wallpaper Path: {wallpaper}");
            desktopKey.Close();
        }
        else
        {
            Logger.Instance.LogPrimary("Failed to open the registry key.");
        }
    }

}
