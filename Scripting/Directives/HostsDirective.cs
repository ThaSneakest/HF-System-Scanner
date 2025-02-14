using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scripting.Directives
{
    public class HostsDirective
    {
        private static readonly string HostsFilePath = @"C:\Windows\System32\drivers\etc\hosts";
        private static readonly string BackupDirectory = @"C:\WSS\Backups";

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
                    if (line.StartsWith("Hosts::"))
                    {
                        Console.WriteLine("Hosts:: directive found. Resetting hosts file to default.");
                        ResetHostsFile();
                        break; // Execute only once
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static void ResetHostsFile()
        {
            try
            {
                // Backup existing hosts file
                if (!Directory.Exists(BackupDirectory))
                {
                    Directory.CreateDirectory(BackupDirectory);
                }

                string backupFilePath = Path.Combine(BackupDirectory, $"hosts_backup_{DateTime.Now:yyyyMMddHHmmss}.bak");

                if (File.Exists(HostsFilePath))
                {
                    File.Copy(HostsFilePath, backupFilePath, overwrite: true);
                    Console.WriteLine($"Backup created: {backupFilePath}");
                }
                else
                {
                    Console.WriteLine($"Hosts file not found: {HostsFilePath}. A new one will be created.");
                }

                // Write default hosts file content
                string defaultHostsContent = GetDefaultHostsContent();
                File.WriteAllText(HostsFilePath, defaultHostsContent);
                Console.WriteLine($"Hosts file reset to default at: {HostsFilePath}");
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"Access denied: Unable to modify the hosts file at {HostsFilePath}. Run as administrator.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error resetting hosts file: {ex.Message}");
            }
        }

        private static string GetDefaultHostsContent()
        {
            return @"# Copyright (c) 1993-2009 Microsoft Corp.
#
# This is a sample HOSTS file used by Microsoft TCP/IP for Windows.
#
# This file contains the mappings of IP addresses to host names. Each
# entry should be kept on an individual line. The IP address should
# be placed in the first column followed by the corresponding host name.
# The IP address and the host name should be separated by at least one
# space.
#
# Additionally, comments (such as these) may be inserted on individual
# lines or following the machine name denoted by a '#' symbol.
#
# For example:
#
#      127.0.0.1       localhost
#      ::1             localhost

127.0.0.1       localhost
::1             localhost
";
        }
    }
}
