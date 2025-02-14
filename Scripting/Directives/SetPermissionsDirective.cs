using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scripting.Directives
{
    public class SetPermissionsDirective
    {
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
                    if (line.StartsWith("SetPermissions::"))
                    {
                        string arguments = line.Substring("SetPermissions::".Length).Trim();
                        string[] parts = arguments.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);


                        if (parts.Length == 3)
                        {
                            string targetPath = parts[0].Trim();
                            string userOrGroup = parts[1].Trim();
                            string permissions = parts[2].Trim();
                            Console.WriteLine($"SetPermissions:: directive found. Setting permissions for '{targetPath}' to '{permissions}' for '{userOrGroup}'.");
                            SetPermissions(targetPath, userOrGroup, permissions);
                        }
                        else
                        {
                            Console.WriteLine($"Invalid SetPermissions:: format. Expected 'Path|UserOrGroup|Permissions', got: {arguments}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static void SetPermissions(string targetPath, string userOrGroup, string permissions)
        {
            try
            {
                if (!File.Exists(targetPath) && !Directory.Exists(targetPath))
                {
                    Console.WriteLine($"Target path not found: {targetPath}");
                    return;
                }

                // Convert the permissions string to FileSystemRights
                FileSystemRights rights = ParsePermissions(permissions);
                FileSystemSecurity security;

                if (File.Exists(targetPath))
                {
                    security = File.GetAccessControl(targetPath);
                }
                else
                {
                    security = Directory.GetAccessControl(targetPath);
                }

                // Create a new access rule
                FileSystemAccessRule accessRule = new FileSystemAccessRule(
                    userOrGroup,
                    rights,
                    InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                    PropagationFlags.None,
                    AccessControlType.Allow
                );

                // Modify the ACL
                security.AddAccessRule(accessRule);

                if (File.Exists(targetPath))
                {
                    File.SetAccessControl(targetPath, (FileSecurity)security);
                }
                else
                {
                    Directory.SetAccessControl(targetPath, (DirectorySecurity)security);
                }

                Console.WriteLine($"Permissions successfully set for '{targetPath}' to '{permissions}' for '{userOrGroup}'.");
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"Access denied: Unable to set permissions for '{targetPath}'. Run as administrator.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting permissions for '{targetPath}': {ex.Message}");
            }
        }

        private static FileSystemRights ParsePermissions(string permissions)
        {
            switch (permissions.ToUpper())
            {
                case "FULLCONTROL":
                    return FileSystemRights.FullControl;
                case "READ":
                    return FileSystemRights.Read;
                case "WRITE":
                    return FileSystemRights.Write;
                case "MODIFY":
                    return FileSystemRights.Modify;
                default:
                    throw new ArgumentException($"Invalid permissions: {permissions}");
            }
        }

    }
}
