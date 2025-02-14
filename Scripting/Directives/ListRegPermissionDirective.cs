using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scripting.Directives
{
    public class ListRegPermissionDirective
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
                    if (line.StartsWith("ListRegPermission::"))
                    {
                        string registryKeyPath = line.Substring("ListRegPermission::".Length).Trim();
                        if (!string.IsNullOrEmpty(registryKeyPath))
                        {
                            Console.WriteLine($"ListRegPermission:: directive found. Listing permissions for registry key: {registryKeyPath}");
                            ListRegistryPermissions(registryKeyPath);
                        }
                        else
                        {
                            Console.WriteLine("ListRegPermission:: directive found, but no registry key was specified.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static void ListRegistryPermissions(string registryKeyPath)
        {
            try
            {
                RegistryKey baseKey = GetBaseKey(ref registryKeyPath);
                if (baseKey == null)
                {
                    Console.WriteLine($"Invalid registry base key in: {registryKeyPath}");
                    return;
                }

                using (RegistryKey key = baseKey.OpenSubKey(registryKeyPath, writable: false))
                {
                    if (key == null)
                    {
                        Console.WriteLine($"Registry key not found: {registryKeyPath}");
                        return;
                    }

                    Console.WriteLine($"Permissions for registry key: {baseKey.Name}\\{registryKeyPath}");

                    RegistrySecurity security = key.GetAccessControl();

                    foreach (RegistryAccessRule rule in security.GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount)))
                    {
                        Console.WriteLine($"User/Group: {rule.IdentityReference}");
                        Console.WriteLine($" - Rights: {rule.RegistryRights}");
                        Console.WriteLine($" - Access Control Type: {rule.AccessControlType}");
                        Console.WriteLine($" - Inherited: {rule.IsInherited}");
                        Console.WriteLine();
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"Access denied: Unable to access permissions for registry key '{registryKeyPath}'. Run as administrator.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving permissions for registry key '{registryKeyPath}': {ex.Message}");
            }
        }

        private static RegistryKey GetBaseKey(ref string registryKeyPath)
        {
            if (registryKeyPath.StartsWith(@"HKEY_CLASSES_ROOT\"))
            {
                registryKeyPath = registryKeyPath.Substring(@"HKEY_CLASSES_ROOT\".Length);
                return Microsoft.Win32.Registry.ClassesRoot;
            }
            if (registryKeyPath.StartsWith(@"HKEY_CURRENT_USER\"))
            {
                registryKeyPath = registryKeyPath.Substring(@"HKEY_CURRENT_USER\".Length);
                return Microsoft.Win32.Registry.CurrentUser;
            }
            if (registryKeyPath.StartsWith(@"HKEY_LOCAL_MACHINE\"))
            {
                registryKeyPath = registryKeyPath.Substring(@"HKEY_LOCAL_MACHINE\".Length);
                return Microsoft.Win32.Registry.LocalMachine;
            }
            if (registryKeyPath.StartsWith(@"HKEY_USERS\"))
            {
                registryKeyPath = registryKeyPath.Substring(@"HKEY_USERS\".Length);
                return Microsoft.Win32.Registry.Users;
            }
            if (registryKeyPath.StartsWith(@"HKEY_CURRENT_CONFIG\"))
            {
                registryKeyPath = registryKeyPath.Substring(@"HKEY_CURRENT_CONFIG\".Length);
                return Microsoft.Win32.Registry.CurrentConfig;
            }

            return null;
        }
    }
}
