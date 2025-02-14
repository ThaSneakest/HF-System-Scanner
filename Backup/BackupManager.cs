using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Wildlands_System_Scanner.Constants;
using Wildlands_System_Scanner.NativeMethods;

public class BackupManager
{
    private static readonly string Fix = "FCheck: C:\\ExamplePath\\ExampleFile.txt [AdditionalInfo]";

    public static void PerformBackup()
    {
        try
        {
            EnsureRequiredDirectories();
            EnsureSqliteDll();
            EnsureConfigFile();
            BackupRegistryKeys();
            BackupBootConfiguration();
        }
        catch (Exception ex)
        {
            Logger.LogError("Backup failed.", ex);
        }
    }

    private static void EnsureRequiredDirectories()
    {
        string[] directories =
        {
            FolderConstants.WildlandsFolderPath,
            FolderConstants.LogsPath,
            FolderConstants.QuarantinePath,
            FolderConstants.BinPath,
            FolderConstants.DestinationPath
        };

        foreach (string path in directories)
        {
            EnsureDirectory(path);
        }
    }

    private static void EnsureSqliteDll()
    {
        string sqlitePath = Path.Combine(FolderConstants.BinPath, "sqlite3.dll");
        if (!File.Exists(sqlitePath))
        {
            File.WriteAllText(sqlitePath, ""); // Simulating FileInstall
        }
    }

    private static void EnsureConfigFile()
    {
        string configFilePath = Path.Combine(FolderConstants.LogsPath, "ct.ini");
        if (!File.Exists(configFilePath))
        {
            File.WriteAllText(configFilePath, "[Run]\nct=0\n");
            File.SetAttributes(configFilePath, FileAttributes.Hidden | FileAttributes.System);
        }
    }

    private static void BackupRegistryKeys()
    {
        string[] registryKeys = { "SOFTWARE", "SYSTEM", "SAM", "DEFAULT", "SECURITY", "COMPONENTS" };
        foreach (string key in registryKeys)
        {
            string sourcePath = Path.Combine(@"C:\Windows\System32\config", key);
            string destPath = Path.Combine(FolderConstants.DestinationPath, Path.GetFileName(sourcePath));
            CopyFileIfExists(sourcePath, destPath);
        }
    }

    private static void BackupBootConfiguration()
    {
        string[] drives = { "Y", "C", "D", "E" };
        foreach (string drive in drives)
        {
            if (!DriveReady(drive)) continue;

            string bootPath = Path.Combine($"{drive}:", "boot", "bcd");
            string destBootPath = Path.Combine(FolderConstants.DestinationPath, $"BCD.{drive}");
            CopyFileIfExists(bootPath, destBootPath);
        }
    }

    private static bool DriveReady(string drive)
    {
        try
        {
            string formattedDrive = drive.EndsWith(":") ? drive : $"{drive}:";
            return DriveInfo.GetDrives()
                .Any(di => di.Name.Equals(formattedDrive, StringComparison.OrdinalIgnoreCase) && di.IsReady);
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to check drive readiness for '{drive}'", ex);
            return false;
        }
    }

    private static void CopyFileIfExists(string sourcePath, string destPath)
    {
        if (File.Exists(sourcePath) && !File.Exists(destPath))
        {
            File.Copy(sourcePath, destPath, overwrite: true);
        }
    }

    private static void EnsureDirectory(string path)
    {
        try
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            var attributes = File.GetAttributes(path);
            var unwantedAttributes = FileAttributes.System | FileAttributes.ReadOnly | FileAttributes.Hidden;

            if ((attributes & unwantedAttributes) != 0)
            {
                File.SetAttributes(path, attributes & ~unwantedAttributes);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to ensure directory: {path}", ex);
        }
    }
}
