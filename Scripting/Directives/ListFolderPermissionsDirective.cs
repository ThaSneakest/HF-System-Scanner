using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scripting.Directives
{
    public class ListFolderPermissionsDirective
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
                    if (line.StartsWith("ListFolderPermissions::"))
                    {
                        string folderPath = line.Substring("ListFolderPermissions::".Length).Trim();
                        if (!string.IsNullOrEmpty(folderPath))
                        {
                            Console.WriteLine($"ListFolderPermissions:: directive found. Listing permissions for: {folderPath}");
                            ListFolderPermissions(folderPath);
                        }
                        else
                        {
                            Console.WriteLine("ListFolderPermissions:: directive found, but no folder path was specified.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static void ListFolderPermissions(string folderPath)
        {
            try
            {
                if (!Directory.Exists(folderPath))
                {
                    Console.WriteLine($"Folder not found: {folderPath}");
                    return;
                }

                DirectoryInfo directoryInfo = new DirectoryInfo(folderPath);
                DirectorySecurity security = directoryInfo.GetAccessControl();

                Console.WriteLine($"Permissions for folder: {folderPath}");
                foreach (FileSystemAccessRule rule in security.GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount)))
                {
                    Console.WriteLine($"User/Group: {rule.IdentityReference}");
                    Console.WriteLine($" - Rights: {rule.FileSystemRights}");
                    Console.WriteLine($" - Access Control Type: {rule.AccessControlType}");
                    Console.WriteLine($" - Inherited: {rule.IsInherited}");
                    Console.WriteLine();
                }
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"Access denied: Unable to access permissions for folder '{folderPath}'. Run as administrator.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving permissions for folder '{folderPath}': {ex.Message}");
            }
        }
    }
}
