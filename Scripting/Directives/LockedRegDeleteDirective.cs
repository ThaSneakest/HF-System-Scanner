using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scripting.Directives
{
    public class LockedRegDeleteDirective
    {
        // Constants for Windows API
        private const uint KEY_WRITE = 0x20006; // KEY_SET_VALUE | KEY_CREATE_SUB_KEY
        private const uint REG_OPTION_BACKUP_RESTORE = 0x00000004;

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern int RegOpenKeyEx(IntPtr hKey, string lpSubKey, uint ulOptions, uint samDesired, out IntPtr phkResult);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern int RegDeleteKey(IntPtr hKey, string lpSubKey);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern int RegCloseKey(IntPtr hKey);

        // Predefined registry keys
        private static readonly IntPtr HKEY_CLASSES_ROOT = new IntPtr(unchecked((int)0x80000000));
        private static readonly IntPtr HKEY_CURRENT_USER = new IntPtr(unchecked((int)0x80000001));
        private static readonly IntPtr HKEY_LOCAL_MACHINE = new IntPtr(unchecked((int)0x80000002));
        private static readonly IntPtr HKEY_USERS = new IntPtr(unchecked((int)0x80000003));
        private static readonly IntPtr HKEY_CURRENT_CONFIG = new IntPtr(unchecked((int)0x80000005));

        public static void ProcessDirectives()
        {
            string filePath = "commands.txt"; // Path to your directives text file

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
                    if (line.StartsWith("LockedRegDelete::"))
                    {
                        string registryPath = line.Substring("LockedRegDelete::".Length).Trim();
                        if (!string.IsNullOrEmpty(registryPath))
                        {
                            Console.WriteLine($"LockedRegDelete:: directive found. Deleting locked registry key: {registryPath}");
                            DeleteLockedRegistryKey(registryPath);
                        }
                        else
                        {
                            Console.WriteLine("LockedRegDelete:: directive found, but no registry path was specified.");
                        }
                    }

                    // Add additional directive handling here.
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static void DeleteLockedRegistryKey(string registryPath)
        {
            try
            {
                string rootKeyName = registryPath.Split('\\')[0];
                string subKeyPath = registryPath.Substring(rootKeyName.Length + 1);

                IntPtr rootKey;

                switch (rootKeyName.ToUpper())
                {
                    case "HKEY_CLASSES_ROOT":
                        rootKey = HKEY_CLASSES_ROOT;
                        break;
                    case "HKEY_CURRENT_USER":
                        rootKey = HKEY_CURRENT_USER;
                        break;
                    case "HKEY_LOCAL_MACHINE":
                        rootKey = HKEY_LOCAL_MACHINE;
                        break;
                    case "HKEY_USERS":
                        rootKey = HKEY_USERS;
                        break;
                    case "HKEY_CURRENT_CONFIG":
                        rootKey = HKEY_CURRENT_CONFIG;
                        break;
                    default:
                        throw new ArgumentException($"Invalid root key: {rootKeyName}");
                }


                IntPtr hKey;
                int result = RegOpenKeyEx(rootKey, subKeyPath, REG_OPTION_BACKUP_RESTORE, KEY_WRITE, out hKey);

                if (result != 0)
                {
                    Console.WriteLine($"Failed to open registry key: {registryPath}. Error code: {result}");
                    return;
                }

                result = RegDeleteKey(hKey, null); // Null indicates deleting the opened key
                if (result == 0)
                {
                    Console.WriteLine($"Successfully deleted locked registry key: {registryPath}");
                }
                else
                {
                    Console.WriteLine($"Failed to delete registry key: {registryPath}. Error code: {result}");
                }

                RegCloseKey(hKey);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting locked registry key '{registryPath}': {ex.Message}");
            }
        }
    }
}
