using System;
using System.IO;
using System.Text.RegularExpressions;

public static class PathUtils
{
    public static string GetFullPath(string relativePath, string basePath = null)
    {
        // If the relative path is null, empty, or ".", return the base path
        if (string.IsNullOrWhiteSpace(relativePath) || relativePath == ".")
            return basePath ?? Directory.GetCurrentDirectory();

        // Normalize the path separators to Windows-style
        relativePath = relativePath.Replace("/", "\\");

        // Use the base path if provided; otherwise, use the current working directory
        if (basePath == null)
        {
            basePath = Directory.GetCurrentDirectory();
        }

        // If the relative path is already rooted (e.g., starts with "\" or "C:\"), resolve it directly
        if (Path.IsPathRooted(relativePath))
        {
            if (relativePath.StartsWith("\\") && !relativePath.StartsWith("\\\\")) // Handle root-only paths
            {
                return Path.GetFullPath(basePath.Substring(0, 2) + relativePath);
            }
            return Path.GetFullPath(relativePath);
        }

        // Combine the base path and the relative path
        string combinedPath = Path.Combine(basePath, relativePath);

        // Normalize and resolve the combined path
        string resolvedPath = Path.GetFullPath(combinedPath);

        // Return the resolved full path
        return resolvedPath;
    }
    public static string EmptyPath(string inputPath)
    {
        if (string.IsNullOrEmpty(inputPath))
        {
            return inputPath; // Return as-is if null or empty
        }

        // Remove the environment variable placeholder (%...%) and capture the remainder
        inputPath = Regex.Replace(inputPath, @"%.+?%(.+)", "$1");

        // Remove the first two directory levels (e.g., "C:\Folder1\Folder2") and capture the rest
        inputPath = Regex.Replace(inputPath, @".:\\[^\\]+\\[^\\]+(\\.+)", "$1");

        return inputPath;
    }
}