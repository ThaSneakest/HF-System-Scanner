using IWshRuntimeLibrary;
using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace Wildlands_System_Scanner.Scripting
{
    public class ShortcutFix
    {
        private string targetPath;
        private string shortcutPath;
        private string arguments;
        private string description;
        private string iconLocation;
        private string workingDirectory;
        private string hotkey;
        private string windowStyle;

        // Constructor - Must have the same name as the class
        public ShortcutFix(string target, string shortcut, string args, string desc, string icon, string workDir, string key, string style)
        {
            // Initialize fields through constructor
            targetPath = target;
            shortcutPath = shortcut;
            arguments = args;
            description = desc;
            iconLocation = icon;
            workingDirectory = workDir;
            hotkey = key;
            windowStyle = style;
        }

        private static void CreateShortcut(string shortcutPath, string targetPath, string arguments, string description, string iconLocation, string workingDirectory, string hotkey, string windowStyle)
        {
            try
            {
                WshShell shell = new WshShell();
                IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);

                shortcut.TargetPath = targetPath;
                shortcut.Arguments = arguments;
                shortcut.Description = description;
                shortcut.IconLocation = iconLocation;
                shortcut.WorkingDirectory = workingDirectory;
                shortcut.Hotkey = hotkey;
                shortcut.WindowStyle = !string.IsNullOrEmpty(windowStyle) ? int.Parse(windowStyle) : 1;

                shortcut.Save();
                Console.WriteLine($"Shortcut created at {shortcutPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating shortcut: {ex.Message}");
            }
        }

        public void ProcessShortcut(string fix, string logFile)
        {
            // Extract shortcut path from input string
            string path = Regex.Replace(fix, @"Shortcut: (.+?) ->.*", "$1");
            path = Regex.Replace(path, @"ShortcutWithArgument: (.+?) ->.*", "$1");

            // Check if the file exists
            if (!System.IO.File.Exists(path))
            {
                Logger.Instance.LogFix($"{path} => NOT FOUND");
                return;
            }

            // If it's a shortcut, move it to a new location
            if (fix.Contains("Shortcut:"))
            {
                // Move the file using your own file utility function
                FileFix.MoveFile(path);
            }

            // Get the target of the shortcut
            string[] target = ShortcutHandler.GetShortcutTarget(path);
            if (target == null)
            {
                Logger.Instance.LogFix($"{path} => SHORTCUT ERROR");
                return;
            }

            // Generate a new file name and move the shortcut
            string newFileName = Regex.Replace(path, @"(?i)([a-z]):", "$1");
            string destination = Path.Combine("C:\\FRST\\Quarantine\\", newFileName);
            System.IO.File.Move(path, destination + ".xBAD");

            // Re-create the shortcut using specified parameters
            CreateShortcut(shortcutPath, targetPath, arguments, description, iconLocation, workingDirectory, hotkey, windowStyle);

            // Check the updated shortcut target
            target = ShortcutHandler.GetShortcutTarget(path);
            if (target != null)
            {
                // Log restoration results
                Logger.Instance.LogFix($"{path} => SHORTCUT RESTORED");
            }
            else
            {
                Logger.Instance.LogFix($"{path} => SHORTCUT ERROR");
            }
        }
    }
}
