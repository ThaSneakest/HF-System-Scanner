using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Utilities;

namespace Wildlands_System_Scanner.Scanning.RegistryScan
{
    public class ShellEx
    {
        private static readonly string[] RegistryPaths = new[]
{
            @"Software\Classes\ShellEx",
            @"Software\Classes\Drive\ShellEx\ContextMenuHandlers",
            @"Software\Classes\*\ShellEx\PropertySheetHandlers",
            @"Software\Classes\AllFileSystemObjects\ShellEx\ContextMenuHandlers",
            @"Software\Classes\AllFileSystemObjects\ShellEx\DragDropHandlers",
            @"Software\Classes\AllFileSystemObjects\ShellEx\PropertySheetHandlers",
            @"Software\Classes\Directory\ShellEx\ContextMenuHandlers",
            @"Software\Classes\Directory\ShellEx\DragDropHandlers",
            @"Software\Classes\Directory\ShellEx\PropertySheetHandlers",
            @"Software\Classes\Directory\ShellEx\CopyHookHandlers",
            @"Software\Classes\Directory\Background\ShellEx\ContextMenuHandlers",
            @"Software\Classes\Folder\ShellEx\ContextMenuHandlers",
            @"Software\Classes\Folder\ShellEx\DragDropHandlers",
            @"Software\Classes\Folder\ShellEx\PropertySheetHandlers",
            @"Software\Classes\Folder\ShellEx\ColumnHandlers",
            @"Software\Classes\Folder\ShellEx\ExtShellFolderViews",
            @"Software\Wow6432Node\Classes\*\ShellEx\ContextMenuHandlers",
            @"Software\Wow6432Node\Classes\Drive\ShellEx\ContextMenuHandlers",
            @"Software\Wow6432Node\Classes\*\ShellEx\PropertySheetHandlers",
            @"Software\Wow6432Node\Classes\AllFileSystemObjects\ShellEx\ContextMenuHandlers",
            @"Software\Wow6432Node\Classes\AllFileSystemObjects\ShellEx\DragDropHandlers",
            @"Software\Wow6432Node\Classes\AllFileSystemObjects\ShellEx\PropertySheetHandlers",
            @"Software\Wow6432Node\Classes\Directory\ShellEx\ContextMenuHandlers",
            @"Software\Wow6432Node\Classes\Directory\ShellEx\DragDropHandlers",
            @"Software\Wow6432Node\Classes\Directory\ShellEx\PropertySheetHandlers",
            @"Software\Wow6432Node\Classes\Directory\ShellEx\CopyHookHandlers",
            @"Software\Wow6432Node\Classes\Directory\Background\ShellEx\ContextMenuHandlers",
            @"Software\Wow6432Node\Classes\Folder\ShellEx\ContextMenuHandlers",
            @"Software\Wow6432Node\Classes\Folder\ShellEx\DragDropHandlers",
            @"Software\Wow6432Node\Classes\Folder\ShellEx\PropertySheetHandlers",
            @"Software\Wow6432Node\Classes\Folder\ShellEx\ColumnHandlers",
            @"Software\Wow6432Node\Classes\Folder\ShellEx\ExtShellFolderViews",
            @"Software\Classes\*\ShellEx",
            @"Software\Classes\*\ShellEx\ContextMenuHandlers",
            @"Software\Classes\Drive\ShellEx\ContextMenuHandlers",
            @"Software\Classes\*\ShellEx\PropertySheetHandlers",
            @"Software\Classes\AllFileSystemObjects\ShellEx\ContextMenuHandlers",
            @"Software\Classes\AllFileSystemObjects\ShellEx\DragDropHandlers",
            @"Software\Classes\AllFileSystemObjects\ShellEx\PropertySheetHandlers",
            @"Software\Classes\Directory\ShellEx\ContextMenuHandlers",
            @"Software\Classes\Directory\ShellEx\DragDropHandlers",
            @"Software\Classes\Directory\ShellEx\PropertySheetHandlers",
            @"Software\Classes\Directory\ShellEx\CopyHookHandlers",
            @"Software\Classes\Directory\Background\ShellEx\ContextMenuHandlers",
            @"Software\Classes\Folder\ShellEx\ContextMenuHandlers",
            @"Software\Classes\Folder\ShellEx\DragDropHandlers",
            @"Software\Classes\Folder\ShellEx\PropertySheetHandlers",
            @"Software\Classes\Folder\ShellEx\ColumnHandlers",
            @"Software\Classes\Folder\ShellEx\ExtShellFolderViews",
            @"Classes\AllFilesystemObjects\shellex\ContextMenuHandlers\{B7CDF620-DB73-44C0-8611-832B261A0107}",
            @"Classes\Directory\Background\ShellEx\ContextMenuHandlers"
        };

        private static readonly HashSet<Tuple<string, string, string>> Whitelist = new HashSet<Tuple<string, string, string>>(new TupleStringComparer())
        {
            Tuple.Create(@"HKEY_LOCAL_MACHINE\Software\Classes\Folder\ShellEx\ContextMenuHandlers", "ApprovedShellExtension", "SafeExtension"),
        };

        private static readonly HashSet<Tuple<string, string, string>> Blacklist = new HashSet<Tuple<string, string, string>>(new TupleStringComparer())
        {
            Tuple.Create(@"HKEY_LOCAL_MACHINE\Software\Classes\Folder\ShellEx\ContextMenuHandlers", "MaliciousExtension", "C:\\Malware\\bad.dll"),
        };

        public static void ScanShellExtensions()
        {
            Console.WriteLine($"Starting scan for Shell Extensions registry keys...");

            ScanRegistryRoot(Microsoft.Win32.Registry.LocalMachine, "HKEY_LOCAL_MACHINE");
            ScanRegistryRoot(Microsoft.Win32.Registry.CurrentUser, "HKEY_CURRENT_USER");
            ScanRegistryRoot(Microsoft.Win32.Registry.ClassesRoot, "HKEY_CLASSES_ROOT");
        }

        private static void ScanRegistryRoot(RegistryKey rootKey, string rootName)
        {
            foreach (var path in RegistryPaths)
            {
                ScanKey(rootKey, path, rootName);
            }
        }

        private static void ScanKey(RegistryKey baseRegistry, string subPath, string rootName)
        {
            string fullKeyPath = $@"{rootName}\{subPath}";

            try
            {
                using (RegistryKey baseKey = baseRegistry.OpenSubKey(subPath, false))
                {
                    if (baseKey == null)
                    {
                        Console.WriteLine($"Registry key not found or inaccessible: {fullKeyPath}");
                        return;
                    }

                    foreach (string valueName in baseKey.GetValueNames())
                    {
                        string valueData = baseKey.GetValue(valueName)?.ToString() ?? "NULL";

                        if (Whitelist.Contains(Tuple.Create(fullKeyPath, valueName, valueData)))
                            continue;

                        string attn = Blacklist.Contains(Tuple.Create(fullKeyPath, valueName, valueData))
                            ? " <==== Malicious Registry Entry Found"
                            : string.Empty;

                        Logger.Instance.LogPrimary($"{fullKeyPath}: [{valueName}] -> {valueData}{attn}");
                    }

                    foreach (string subKeyName in baseKey.GetSubKeyNames())
                    {
                        string subKeyFullPath = $@"{subPath}\{subKeyName}";
                        ScanKey(baseRegistry, subKeyFullPath, rootName);
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"Access denied: {fullKeyPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error scanning {fullKeyPath}: {ex.Message}");
            }
        }
    }
}
