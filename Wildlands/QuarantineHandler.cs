using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Wildlands_System_Scanner.Blacklist;
using Wildlands_System_Scanner.Constants;
using Wildlands_System_Scanner.Scripting;
using Wildlands_System_Scanner.Utilities;

public class QuarantineHandler
{
    private static readonly string RestoreQuarantineCommand = "Restore Quarantine";

    /// <summary>
    /// Handles the quarantine process for a specified folder.
    /// </summary>
    public static void Quarantine(string mainFolder)
    {
        try
        {
            CleanupTemporaryFiles();

            string tempDir = FolderConstants.TempDir;
            string quarFile = Path.Combine(tempDir, "quar00");
            string finalFile = Path.Combine(tempDir, "final00");
            string finalFile22 = Path.Combine(tempDir, "final22");

            // Step 1: Write all files and subfolders to quar00
            WriteAllEntriesToFile(mainFolder, quarFile);

            // Step 2: Filter entries without spaces in short names and write to final00
            FilterEntriesWithoutSpaces(quarFile, finalFile);

            // Step 3: Remove duplicate entries and write to final22
            RemoveDuplicateEntries(finalFile, finalFile22);

            // Step 4: Process entries in final22 (delete files or directories)
            ProcessQuarantineEntries(finalFile22);

            FileFix.DeleteFile(finalFile22);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in Quarantine: {ex.Message}");
        }
    }

    /// <summary>
    /// Cleans up temporary files.
    /// </summary>
    private static void CleanupTemporaryFiles()
    {
        string tempDir = FolderConstants.TempDir;
        FileFix.DeleteFile(Path.Combine(tempDir, "quar00"));
        FileFix.DeleteFile(Path.Combine(tempDir, "final00"));
        FileFix.DeleteFile(Path.Combine(tempDir, "final22"));
    }

    /// <summary>
    /// Writes all files and subfolders of a directory to a file.
    /// </summary>
    private static void WriteAllEntriesToFile(string folder, string outputFile)
    {
        using (StreamWriter writer = new StreamWriter(outputFile))
        {
            foreach (var entry in Directory.GetFileSystemEntries(folder, "*", SearchOption.AllDirectories))
            {
                writer.WriteLine(entry);
            }
        }
    }

    /// <summary>
    /// Filters entries without spaces in their names and writes them to a new file.
    /// </summary>
    private static void FilterEntriesWithoutSpaces(string inputFile, string outputFile)
    {
        using (StreamWriter writer = new StreamWriter(outputFile))
        using (StreamReader reader = new StreamReader(inputFile))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                string shortName = Path.GetFileName(line);
                if (!Regex.IsMatch(shortName, @"\s"))
                {
                    writer.WriteLine(line);
                }
            }
        }
        FileFix.DeleteFile(inputFile);
    }

    /// <summary>
    /// Removes duplicate entries and writes them to a new file.
    /// </summary>
    private static void RemoveDuplicateEntries(string inputFile, string outputFile)
    {
        using (StreamWriter writer = new StreamWriter(outputFile))
        using (StreamReader reader = new StreamReader(inputFile))
        {
            string previousLine = null;
            string currentLine;

            while ((currentLine = reader.ReadLine()) != null)
            {
                if (previousLine == null || !currentLine.Contains(previousLine))
                {
                    writer.WriteLine(previousLine);
                }
                previousLine = currentLine;
            }

            if (!string.IsNullOrEmpty(previousLine))
            {
                writer.WriteLine(previousLine);
            }
        }
        FileFix.DeleteFile(inputFile);
    }

    /// <summary>
    /// Processes quarantine entries by deleting files or directories.
    /// </summary>
    private static void ProcessQuarantineEntries(string filePath)
    {
        using (StreamReader reader = new StreamReader(filePath))
        {
            string path;
            while ((path = reader.ReadLine()) != null)
            {
                if (File.Exists(path))
                {
                    DeleteFile(path);
                }
                else if (Directory.Exists(path))
                {
                    DeleteDirectory(path);
                }
            }
        }
    }

    /// <summary>
    /// Deletes a file with access and attribute adjustments.
    /// </summary>
    private static void DeleteFile(string filePath)
    {
        try
        {
            DirectoryFix.GrantAccess(filePath);
            File.SetAttributes(filePath, FileAttributes.Normal);
            File.Delete(filePath);
        }
        catch
        {
            // Retry in case of failure
            DirectoryFix.GrantAccess(filePath);
            File.SetAttributes(filePath, FileAttributes.Normal);
            File.Delete(filePath);
        }
    }

    /// <summary>
    /// Deletes a directory with access adjustments.
    /// </summary>
    private static void DeleteDirectory(string dirPath)
    {
        try
        {
            DirectoryFix.GrantAccess(dirPath);
            Directory.Delete(dirPath, true);
        }
        catch
        {
            // Retry in case of failure
            DirectoryFix.GrantAccess(dirPath);
            Directory.Delete(dirPath, true);
        }
    }

    /// <summary>
    /// Handles restoring quarantined files or folders.
    /// </summary>
    public static void ResQuar()
    {
        try
        {
            if (IsRestoreQuarantine(RestoreQuarantineCommand))
            {
                DirectoryFix.ResFol(FolderConstants.QuarantinePath);
                return;
            }

            string path = FileUtils.ExtractPath(RestoreQuarantineCommand);
            if (string.IsNullOrEmpty(path) || (!File.Exists(path) && !Directory.Exists(path)))
            {
                Logger.Instance.LogFix($"\"{path}\" => Path not found.");
                return;
            }

            if (Directory.Exists(path))
            {
                DirectoryFix.ResFol(path);
            }
            else if (File.Exists(path))
            {
                FileFix.ResFile(path);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in ResQuar: {ex.Message}");
        }
    }

    /// <summary>
    /// Checks if the input string matches the restore quarantine command.
    /// </summary>
    private static bool IsRestoreQuarantine(string input)
    {
        return input.Trim().Equals(RestoreQuarantineCommand, StringComparison.OrdinalIgnoreCase);
    }
}
