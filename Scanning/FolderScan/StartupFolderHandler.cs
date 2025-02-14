using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

public static class StartupFolderHandler
{
    public static void ProcessStartupFolder(string folderPath)
    {
        if (!Directory.Exists(folderPath)) return;

        var fileArray = Directory.GetFiles(folderPath);
        foreach (var filePath in fileArray)
        {
            string fileName = Path.GetFileName(filePath);
            if (fileName.Equals("desktop.ini", StringComparison.OrdinalIgnoreCase)) continue;

            string dateCreated = File.GetCreationTime(filePath).ToString("yyyy-MM-dd HH:mm:ss");
            string attentionMarker = string.Empty;

            // Handle shortcuts (.lnk)
            if (fileName.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase))
            {
                HandleShortcut(filePath, dateCreated, ref attentionMarker);
            }
            // Handle URLs (.url, .website)
            else if (Regex.IsMatch(fileName, @"(?i)\.(url|website)$"))
            {
                HandleUrlShortcut(filePath, ref attentionMarker);
            }
            // Handle screensavers (.scr)
            else if (Regex.IsMatch(fileName, @"(?i)\.scr\b"))
            {
                HandleScreensaver(filePath, ref attentionMarker);
            }
            // Handle other suspicious files
            else if (IsSuspiciousFile(fileName))
            {
                Console.WriteLine($"Startup: {filePath} [{dateCreated}] <==== Suspicious");
            }
        }
    }

    private static void HandleShortcut(string filePath, string dateCreated, ref string attentionMarker)
    {
        string[] shortcutDetails = GetShortcutDetails(filePath); // Implement this method to retrieve shortcut target and arguments
        string target = shortcutDetails[0];
        string arguments = shortcutDetails.Length > 1 ? shortcutDetails[1] : string.Empty;

        // Analyze shortcut details for suspicious patterns
        if (IsSuspiciousTarget(target) || IsSuspiciousArguments(arguments))
        {
            attentionMarker = " <==== Suspicious";
        }

        Console.WriteLine($"Startup: {filePath} [{dateCreated}] -> {target} {attentionMarker}");
    }

    private static void HandleUrlShortcut(string filePath, ref string attentionMarker)
    {
        string url = GetUrlFromShortcut(filePath); // Implement this to extract the URL
        if (Regex.IsMatch(url, @"(?i)\.(vbs|js)\b"))
        {
            attentionMarker = " <==== Suspicious";
        }
        Console.WriteLine($"InternetURL: {filePath} -> {url} {attentionMarker}");
    }

    private static void HandleScreensaver(string filePath, ref string attentionMarker)
    {
        if (ContainsSuspiciousUnicode(filePath))
        {
            attentionMarker = " <==== Suspicious";
        }
        Console.WriteLine($"Screensaver: {filePath} {attentionMarker}");
    }

    private static bool IsSuspiciousFile(string fileName)
    {
        return Regex.IsMatch(fileName, @"(?i)(certlm\.exe|\.cmd|local\\temp|SystemLogin.+\.vbs|^\w{2}\.(vbs|js)$|^Windows\.|\.scr\b|\A\.exe$)");
    }

    private static bool IsSuspiciousTarget(string target)
    {
        return Regex.IsMatch(target, @"(?i)system\.vbs|\\(AutoIt3|python.*|VBoxSVC|NUP)\.exe");
    }

    private static bool IsSuspiciousArguments(string arguments)
    {
        return Regex.IsMatch(arguments, @"(?i)(powershell|wscript)");
    }

    private static string[] GetShortcutDetails(string shortcutPath)
    {
        // Use Shell32 or Windows APIs to extract shortcut details
        // Return [targetPath, arguments]
        return new[] { "TargetPath", "Arguments" }; // Placeholder
    }

    private static string GetUrlFromShortcut(string shortcutPath)
    {
        // Implement logic to extract the URL from .url/.website files
        return "http://example.com"; // Placeholder
    }

    private static bool ContainsSuspiciousUnicode(string filePath)
    {
        // Check for suspicious Unicode characters
        foreach (char c in filePath)
        {
            if (c > 8100) return true;
        }
        return false;
    }
}