using System;
using System.IO;
using System.Security.AccessControl;
using System.Text.RegularExpressions;
using Wildlands_System_Scanner.Blacklist;
using static DevExpress.XtraEditors.Mask.MaskSettings;

public class DummyHandler
{
    public static void DUM(string fix, string hFixLog)
    {
        // Extract the path using regex
        string path = Regex.Replace(fix, @"(?i)CreateDummy:\s*(.+)\s*$", "$1", RegexOptions.IgnoreCase);
        string user = "Domain\\User";  // Specify the user or group to deny access

        if (File.Exists(path) || Directory.Exists(path))
        {
            // Remove "S", "R", "H" attributes if present
            FileAttributes attributes = File.GetAttributes(path);
            if ((attributes & (FileAttributes.System | FileAttributes.ReadOnly | FileAttributes.Hidden)) != 0)
            {
                File.SetAttributes(path, attributes & ~(FileAttributes.System | FileAttributes.ReadOnly | FileAttributes.Hidden));
            }

            // Handle if it's a directory
            if ((attributes & FileAttributes.Directory) != 0)
            {
                if (Blacklist.IsBlacklisted(path)) // Replace `_BLACK` logic with `Blacklisted`
                {
                  //  Logger.Instance.LogToFile(hFixLog, $"{path} => Could not remove (blacklisted).");
                    return;
                }

                Directory.Delete(path, true); // Recursively delete the directory
            }
            else
            {
                // Handle if it's a file
                File.Delete(path);
            }
        }

        // If path still exists, log failure
        if (File.Exists(path) || Directory.Exists(path))
        {
           // Logger.Instance.LogToFile(hFixLog, $"{path} already exists. Could not make dummy.");
        }
        else
        {
            // Create directory
            Directory.CreateDirectory(path);

            // Add "S", "R", "H" attributes
            FileAttributes newAttributes = FileAttributes.System | FileAttributes.ReadOnly | FileAttributes.Hidden;
            File.SetAttributes(path, newAttributes);

            // Apply deny permissions (implement `_DENYE` logic here)
            ApplyDenyPermissions(path, user);

            // Log success if path exists
            if (Directory.Exists(path))
            {
              //  Logger.Instance.LogToFile(hFixLog, $"{path} => Dummy created successfully.");
            }
        }
    }
    public static void ApplyDenyPermissions(string path, string user)
    {
        try
        {
            // Get the file or directory security object
            FileInfo fileInfo = new FileInfo(path);
            FileSecurity fileSecurity = fileInfo.GetAccessControl();

            // Create a new rule that denies access to the specified user
            FileSystemAccessRule denyRule = new FileSystemAccessRule(user,
                FileSystemRights.ReadData | FileSystemRights.WriteData,
                AccessControlType.Deny);

            // Apply the deny rule to the file or directory
            fileSecurity.AddAccessRule(denyRule);

            // Set the updated security settings to the file or directory
            fileInfo.SetAccessControl(fileSecurity);

            Console.WriteLine($"Deny permissions applied to {user} for {path}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error applying deny permissions: {ex.Message}");
        }
    }
}
