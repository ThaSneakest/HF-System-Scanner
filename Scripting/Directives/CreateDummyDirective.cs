using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scripting.Directives
{
    public class CreateDummyDirective
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
                    if (line.StartsWith("CreateDummy::"))
                    {
                        string targetPath = line.Substring("CreateDummy::".Length).Trim();
                        if (!string.IsNullOrEmpty(targetPath))
                        {
                            Console.WriteLine($"CreateDummy:: directive found. Creating locked dummy folder: {targetPath}");
                            CreateLockedDummyFolder(targetPath);
                        }
                        else
                        {
                            Console.WriteLine("CreateDummy:: directive found, but no target path was specified.");
                        }
                    }
                    else if (line.StartsWith("RemoveDummy::"))
                    {
                        string targetPath = line.Substring("RemoveDummy::".Length).Trim();
                        if (!string.IsNullOrEmpty(targetPath))
                        {
                            Console.WriteLine($"RemoveDummy:: directive found. Removing dummy folder: {targetPath}");
                            RemoveDummyFolder(targetPath);
                        }
                        else
                        {
                            Console.WriteLine("RemoveDummy:: directive found, but no target path was specified.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static void CreateLockedDummyFolder(string folderPath)
        {
            try
            {
                if (Directory.Exists(folderPath))
                {
                    Console.WriteLine($"The folder already exists: {folderPath}");
                    return;
                }

                // Create the folder
                Directory.CreateDirectory(folderPath);
                Console.WriteLine($"Dummy folder created: {folderPath}");

                // Lock the folder by setting restrictive permissions
                DirectoryInfo dirInfo = new DirectoryInfo(folderPath);
                DirectorySecurity security = dirInfo.GetAccessControl();

                // Deny all permissions to Everyone
                security.AddAccessRule(new FileSystemAccessRule(
                    new SecurityIdentifier(WellKnownSidType.WorldSid, null),
                    FileSystemRights.FullControl,
                    AccessControlType.Deny));

                dirInfo.SetAccessControl(security);
                Console.WriteLine($"Dummy folder locked: {folderPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating locked dummy folder: {ex.Message}");
            }
        }

        private static void RemoveDummyFolder(string folderPath)
        {
            try
            {
                if (!Directory.Exists(folderPath))
                {
                    Console.WriteLine($"Dummy folder not found: {folderPath}");
                    return;
                }

                // Reset permissions to allow deletion
                DirectoryInfo dirInfo = new DirectoryInfo(folderPath);
                DirectorySecurity security = dirInfo.GetAccessControl();

                // Remove all deny rules
                AuthorizationRuleCollection rules = security.GetAccessRules(true, true, typeof(SecurityIdentifier));
                foreach (FileSystemAccessRule rule in rules)
                {
                    if (rule.AccessControlType == AccessControlType.Deny)
                    {
                        security.RemoveAccessRule(rule);
                    }
                }

                dirInfo.SetAccessControl(security);

                // Delete the folder
                Directory.Delete(folderPath, true);
                Console.WriteLine($"Dummy folder removed: {folderPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing dummy folder: {ex.Message}");
            }
        }
    }
}
