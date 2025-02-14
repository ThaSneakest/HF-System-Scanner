using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scripting.Directives
{
    public class ListFilePermissionsDirective
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
                    if (line.StartsWith("ListFilePermissions::"))
                    {
                        string fileToCheck = line.Substring("ListFilePermissions::".Length).Trim();
                        if (!string.IsNullOrEmpty(fileToCheck))
                        {
                            Console.WriteLine($"ListFilePermissions:: directive found. Listing permissions for file: {fileToCheck}");
                            ListFilePermissions(fileToCheck);
                        }
                        else
                        {
                            Console.WriteLine("ListFilePermissions:: directive found, but no file path was specified.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static void ListFilePermissions(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"File not found: {filePath}");
                    return;
                }

                FileInfo fileInfo = new FileInfo(filePath);
                FileSecurity fileSecurity = fileInfo.GetAccessControl();

                Console.WriteLine($"Permissions for file: {filePath}");
                foreach (FileSystemAccessRule rule in fileSecurity.GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount)))
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
                Console.WriteLine($"Access denied: Unable to access permissions for file '{filePath}'. Run as administrator.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving permissions for file '{filePath}': {ex.Message}");
            }
        }
    }
}
