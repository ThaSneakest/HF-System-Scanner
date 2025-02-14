using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Windows.Forms;
using Wildlands_System_Scanner.Registry;

//Tested and Working
public class FileAssociations
{
    private static string UPD1 = "Attention";
    public static void BATASS()
    {
        try
        {
            Console.WriteLine("Starting BATASS...");

            // Check the default association for .bat files
            using (var baseKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Classes\.bat"))
            {
                if (baseKey != null)
                {
                    string run1 = baseKey.GetValue("")?.ToString();
                    Console.WriteLine($".bat registry key value: {run1}");

                    if (!string.IsNullOrEmpty(run1) && run1 != "batfile")
                    {
                        // Split the registry value if it contains multiple components
                        string[] run1Parts = run1.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        string primaryKey = run1Parts.FirstOrDefault()?.Trim();

                        if (!string.IsNullOrEmpty(primaryKey))
                        {
                            string commandPath = $@"SOFTWARE\Classes\{primaryKey}\shell\open\command";
                            Console.WriteLine($"Checking associated command key: {commandPath}");

                            using (var commandKey = Registry.LocalMachine.OpenSubKey(commandPath))
                            {
                                string commandValue = commandKey?.GetValue("")?.ToString();
                                Console.WriteLine($"Command value: {commandValue}");

                                if (!string.IsNullOrEmpty(commandValue))
                                {
                                    Logger.Instance.LogPrimary($"HKLM\\...\\.bat: {primaryKey} => {commandValue} <==== Attention");
                                }
                                else
                                {
                                    Console.WriteLine($"Command value not found in registry key: {commandPath}");
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Invalid primary key extracted from: {run1}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($".bat registry key is empty or points to 'batfile': {run1}");
                    }

                    // Check the DefaultIcon for batfile
                    string defaultIconPath = @"SOFTWARE\Classes\batfile\DefaultIcon";
                    Console.WriteLine($"Checking DefaultIcon key: {defaultIconPath}");

                    using (var iconKey = Registry.LocalMachine.OpenSubKey(defaultIconPath))
                    {
                        string iconValue = iconKey?.GetValue("")?.ToString();
                        Console.WriteLine($"DefaultIcon value: {iconValue}");

                        if (!string.IsNullOrEmpty(iconValue) && iconValue != @"%SystemRoot%\System32\imageres.dll,-68")
                        {
                            Logger.Instance.LogPrimary($"HKLM\\...\\batfile\\DefaultIcon: {iconValue}");
                        }
                        else if (string.IsNullOrEmpty(iconValue))
                        {
                            Console.WriteLine($"DefaultIcon value not found in registry key: {defaultIconPath}");
                        }
                    }

                    // Check the shell open command for batfile
                    string shellOpenCommandPath = @"SOFTWARE\Classes\batfile\shell\open\command";
                    Console.WriteLine($"Checking shell open command key: {shellOpenCommandPath}");

                    using (var commandKey = Registry.LocalMachine.OpenSubKey(shellOpenCommandPath))
                    {
                        string commandValue = commandKey?.GetValue("")?.ToString();
                        Console.WriteLine($"Shell open command value: {commandValue}");

                        if (!string.IsNullOrEmpty(commandValue) && commandValue != @"""%1"" %*")
                        {
                            Logger.Instance.LogPrimary($"HKLM\\...\\batfile\\shell\\open\\command: {commandValue} <==== Attention");
                        }
                        else if (string.IsNullOrEmpty(commandValue))
                        {
                            Console.WriteLine($"Shell open command not found in registry key: {shellOpenCommandPath}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Registry key not found: HKLM\\SOFTWARE\\Classes\\.bat");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in BATASS: {ex.Message}");
        }
    }



    public static void COMAssociation()
    {
        string attentionMessage = "Attention";
        Console.WriteLine("Starting COM Association...");

        try
        {
            // Check the default association for .com files
            string comAssociationKey = @"HKEY_LOCAL_MACHINE\software\Classes\.com";
            string run1 = Registry.GetValue(comAssociationKey, "", null)?.ToString();

            Console.WriteLine($".com registry key value: {run1}");

            if (!string.IsNullOrEmpty(run1) && !string.Equals(run1, "comfile", StringComparison.OrdinalIgnoreCase))
            {
                string commandPath = $@"HKEY_LOCAL_MACHINE\software\Classes\{run1}\shell\open\command";
                Console.WriteLine($"Checking associated command key: {commandPath}");

                string commandValue = Registry.GetValue(commandPath, "", null)?.ToString();
                if (!string.IsNullOrEmpty(commandValue))
                {
                    Logger.Instance.LogPrimary($"HKLM\\...\\.com: {run1} => {commandValue} <==== {attentionMessage}");
                    Console.WriteLine($"Found custom handler: {run1} => {commandValue}");
                }
                else
                {
                    Console.WriteLine($"Command value not found in registry key: {commandPath}");
                }
            }
            else
            {
                Console.WriteLine($".com registry key is empty or points to 'comfile': {run1}");
            }

            // Check the DefaultIcon for comfile
            string defaultIconKey = @"HKEY_LOCAL_MACHINE\software\Classes\comfile\DefaultIcon";
            run1 = Registry.GetValue(defaultIconKey, "", null)?.ToString();
            Console.WriteLine($"DefaultIcon value: {run1}");

            if (!string.Equals(run1, @"%SystemRoot%\System32\shell32.dll,2", StringComparison.OrdinalIgnoreCase))
            {
                Logger.Instance.LogPrimary($"HKLM\\...\\comfile\\DefaultIcon: {run1} <==== {attentionMessage}");
                Console.WriteLine($"Custom DefaultIcon detected: {run1}");
            }

            // Check the shell open command for comfile
            string shellOpenCommandKey = @"HKEY_LOCAL_MACHINE\software\Classes\comfile\shell\open\command";
            run1 = Registry.GetValue(shellOpenCommandKey, "", null)?.ToString();
            Console.WriteLine($"Shell open command value: {run1}");

            if (!string.Equals(run1, "\"%1\" %*", StringComparison.OrdinalIgnoreCase))
            {
                Logger.Instance.LogPrimary($"HKLM\\...\\comfile\\shell\\open\\command: {run1} <==== {attentionMessage}");
                Console.WriteLine($"Custom shell open command detected: {run1}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in COMAssociation: {ex.Message}");
        }

        Console.WriteLine("COMAssociation completed.");
    }

    public static void CheckExeAssociations()
    {
        try
        {
            Console.WriteLine("Starting CheckExeAssociations...");

            string runKey;

            // Check for .exe association in HKLM
            string exeAssocKey = @"SOFTWARE\Classes\.exe";
            using (var baseKey = Registry.LocalMachine.OpenSubKey(exeAssocKey))
            {
                if (baseKey != null)
                {
                    runKey = baseKey.GetValue("")?.ToString();
                    Console.WriteLine($".exe association key value: {runKey}");

                    if (!string.Equals(runKey, "exefile", StringComparison.OrdinalIgnoreCase))
                    {
                        string commandKeyPath = $@"SOFTWARE\Classes\{runKey}\shell\open\command";
                        using (var commandKey = Registry.LocalMachine.OpenSubKey(commandKeyPath))
                        {
                            if (commandKey != null)
                            {
                                string command = commandKey.GetValue("")?.ToString();
                                Console.WriteLine($"Command value: {command}");

                                if (!string.IsNullOrEmpty(command))
                                {
                                    Logger.Instance.LogPrimary($@"HKLM\...\.exe: {runKey} => {command} <==== Custom Handler Detected");
                                    Console.WriteLine($"Custom handler detected: {runKey} => {command}");
                                }
                                else
                                {
                                    Console.WriteLine($"Command value not found for key: {commandKeyPath}");
                                }
                            }
                            else
                            {
                                Console.WriteLine($"Key not found: {commandKeyPath}");
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine(".exe is correctly associated with 'exefile'.");
                    }
                }
                else
                {
                    Console.WriteLine($"Key not found: {exeAssocKey}");
                }
            }

            // Check for exefile DefaultIcon
            string defaultIconKey = @"SOFTWARE\Classes\exefile\DefaultIcon";
            using (var iconKey = Registry.LocalMachine.OpenSubKey(defaultIconKey))
            {
                if (iconKey != null)
                {
                    runKey = iconKey.GetValue("")?.ToString();
                    Console.WriteLine($"DefaultIcon key value: {runKey}");

                    if (!string.Equals(runKey, @"%SystemRoot%\System32\shell32.dll,2", StringComparison.OrdinalIgnoreCase))
                    {
                        Logger.Instance.LogPrimary($@"HKLM\...\exefile\DefaultIcon: {runKey} <==== Custom Icon Detected");
                        Console.WriteLine($"Custom DefaultIcon detected: {runKey}");
                    }
                }
                else
                {
                    Console.WriteLine($"Key not found: {defaultIconKey}");
                }
            }

            // Check for exefile shell open command
            string shellOpenCommandKey = @"SOFTWARE\Classes\exefile\shell\open\command";
            using (var commandKey = Registry.LocalMachine.OpenSubKey(shellOpenCommandKey))
            {
                if (commandKey != null)
                {
                    runKey = commandKey.GetValue("")?.ToString();
                    Console.WriteLine($"Shell open command value: {runKey}");

                    if (!string.Equals(runKey, "\"%1\" %*", StringComparison.OrdinalIgnoreCase))
                    {
                        Logger.Instance.LogPrimary($@"HKLM\...\exefile\shell\open\command: {runKey} <==== Attention");
                        Console.WriteLine($"Custom shell open command detected: {runKey}");
                    }
                }
                else
                {
                    Console.WriteLine($"Key not found: {shellOpenCommandKey}");
                }
            }

            // Check for Explorer FileExts for .exe
            string explorerFileExtsKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\FileExts\.exe";
            using (var fileExtsKey = Registry.LocalMachine.OpenSubKey(explorerFileExtsKey))
            {
                if (fileExtsKey != null)
                {
                    runKey = fileExtsKey.GetValue("")?.ToString();
                    Console.WriteLine($"Explorer FileExts value: {runKey}");

                    if (!string.IsNullOrEmpty(runKey))
                    {
                        Logger.Instance.LogPrimary($@"HKLM\...\Explorer\FileExts\.exe: {runKey}");
                        Console.WriteLine($"Explorer FileExts .exe entry detected: {runKey}");
                    }
                }
                else
                {
                    Console.WriteLine($"Key not found: {explorerFileExtsKey}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in CheckExeAssociations: {ex.Message}");
        }
    }



    public static void RegAssMethod()
    {
        try
        {
            Console.WriteLine("Starting RegAssMethod...");

            string runKey;

            // Check for .reg association in HKLM
            string regAssocKey = @"SOFTWARE\Classes\.reg";
            using (var baseKey = Registry.LocalMachine.OpenSubKey(regAssocKey))
            {
                if (baseKey != null)
                {
                    runKey = baseKey.GetValue("")?.ToString();
                    Console.WriteLine($".reg association key value: {runKey}");

                    if (!string.Equals(runKey, "regfile", StringComparison.OrdinalIgnoreCase))
                    {
                        string commandKeyPath = $@"SOFTWARE\Classes\{runKey}\shell\open\command";
                        using (var commandKey = Registry.LocalMachine.OpenSubKey(commandKeyPath))
                        {
                            if (commandKey != null)
                            {
                                string commandValue = commandKey.GetValue("")?.ToString();
                                Console.WriteLine($"Command value: {commandValue}");

                                if (!string.IsNullOrEmpty(commandValue))
                                {
                                    Logger.Instance.LogPrimary($@"HKLM\...\.reg: {runKey} => {commandValue} <==== Custom Handler Detected");
                                    Console.WriteLine($"Custom handler detected: {runKey} => {commandValue}");
                                }
                                else
                                {
                                    Console.WriteLine($"Command value not found for key: {commandKeyPath}");
                                }
                            }
                            else
                            {
                                Console.WriteLine($"Key not found: {commandKeyPath}");
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine(".reg is correctly associated with 'regfile'.");
                    }
                }
                else
                {
                    Console.WriteLine($"Key not found: {regAssocKey}");
                }
            }

            // Check for regfile DefaultIcon
            string defaultIconKey = @"SOFTWARE\Classes\regfile\DefaultIcon";
            using (var iconKey = Registry.LocalMachine.OpenSubKey(defaultIconKey))
            {
                if (iconKey != null)
                {
                    runKey = iconKey.GetValue("")?.ToString();
                    Console.WriteLine($"DefaultIcon key value: {runKey}");

                    if (!string.Equals(runKey, @"%SystemRoot%\regedit.exe,1", StringComparison.OrdinalIgnoreCase))
                    {
                        Logger.Instance.LogPrimary($@"HKLM\...\regfile\DefaultIcon: {runKey} <==== Custom Icon Detected");
                        Console.WriteLine($"Custom DefaultIcon detected: {runKey}");
                    }
                }
                else
                {
                    Console.WriteLine($"Key not found: {defaultIconKey}");
                }
            }

            // Check for regfile shell open command
            string shellOpenCommandKey = @"SOFTWARE\Classes\regfile\shell\open\command";
            using (var commandKey = Registry.LocalMachine.OpenSubKey(shellOpenCommandKey))
            {
                if (commandKey != null)
                {
                    runKey = commandKey.GetValue("")?.ToString();
                    Console.WriteLine($"Shell open command value: {runKey}");

                    if (!string.Equals(runKey, "regedit.exe \\\"%1\\\"", StringComparison.OrdinalIgnoreCase))
                    {
                        Logger.Instance.LogPrimary($@"HKLM\...\regfile\shell\open\command: {runKey} <==== Custom Handler Detected");
                        Console.WriteLine($"Custom shell open command detected: {runKey}");
                    }
                }
                else
                {
                    Console.WriteLine($"Key not found: {shellOpenCommandKey}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in RegAssMethod: {ex.Message}");
        }
    }

    public static void SCRASS()
    {
        try
        {
            Console.WriteLine("Starting SCRASS...");

            // Read the registry value for .scr file association
            using (var scrKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Classes\.scr"))
            {
                if (scrKey != null)
                {
                    string run1 = scrKey.GetValue("")?.ToString();
                    Console.WriteLine($".scr registry association value: {run1}");

                    if (!string.Equals(run1, "scrfile", StringComparison.OrdinalIgnoreCase))
                    {
                        string commandKeyPath = $@"SOFTWARE\Classes\{run1}\shell\open\command";
                        using (var commandKey = Registry.LocalMachine.OpenSubKey(commandKeyPath))
                        {
                            if (commandKey != null)
                            {
                                string ret = commandKey.GetValue("")?.ToString();
                                Console.WriteLine($"Command value for {run1}: {ret}");
                                string atten = " <==== " + UPD1;

                                // Check if the value contains "CryptoPreventSCR" and modify atten value
                                if (!string.IsNullOrEmpty(run1) && run1.IndexOf("CryptoPreventSCR", StringComparison.OrdinalIgnoreCase) >= 0)

                                {
                                    atten = "";
                                }

                                Logger.Instance.LogPrimary($@"HKLM\...\{run1}: {run1} => {ret}{atten}");
                            }
                            else
                            {
                                Console.WriteLine($"Command key not found for {run1}: {commandKeyPath}");
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine(".scr is correctly associated with 'scrfile'.");
                    }
                }
                else
                {
                    Console.WriteLine(".scr registry key not found.");
                }
            }

            // Read the registry value for scrfile command association
            string scrfileCommandKeyPath = @"SOFTWARE\Classes\scrfile\shell\open\command";
            using (var scrfileCommandKey = Registry.LocalMachine.OpenSubKey(scrfileCommandKeyPath))
            {
                if (scrfileCommandKey != null)
                {
                    string run1Scrfile = scrfileCommandKey.GetValue("")?.ToString();
                    Console.WriteLine($"scrfile shell open command value: {run1Scrfile}");

                    // Check if the value is not the expected command, and log it if necessary
                    if (!string.Equals(run1Scrfile, "\"%1\" /S", StringComparison.OrdinalIgnoreCase))
                    {
                        Logger.Instance.LogPrimary($@"HKLM\...\scrfile\shell\open\command: {run1Scrfile} <==== " + UPD1);
                    }
                    else
                    {
                        Console.WriteLine("scrfile command is correctly associated.");
                    }
                }
                else
                {
                    Console.WriteLine($"scrfile command key not found: {scrfileCommandKeyPath}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in SCRASS: {ex.Message}");
        }
    }

    public static void CMDASS()
    {
        try
        {
            Console.WriteLine("Starting CMDASS...");

            // Check the default association for .cmd files
            string run1 = Registry.GetValue(@"HKEY_LOCAL_MACHINE\software\Classes\.cmd", "", null)?.ToString();
            if (!string.Equals(run1, "cmdfile", StringComparison.OrdinalIgnoreCase))
            {
                string ret = Registry.GetValue($@"HKEY_LOCAL_MACHINE\software\Classes\{run1}\shell\open\command", "", null)?.ToString();
                if (!string.IsNullOrEmpty(ret))
                {
                    Logger.Instance.LogPrimary($"HKLM\\...\\.cmd: {run1} => {ret} <==== {UPD1}");
                }
                else
                {
                    Console.WriteLine($"Command value not found for .cmd association: {run1}");
                }
            }

            // Check the DefaultIcon for cmdfile
            run1 = Registry.GetValue(@"HKEY_LOCAL_MACHINE\software\Classes\cmdfile\DefaultIcon", "", null)?.ToString();
            Console.WriteLine($"cmdfile DefaultIcon value: {run1}");

            if (!string.IsNullOrEmpty(run1))
            {
                // Get the operating system version
                Version osVersion = Environment.OSVersion.Version;
                float versionNumber = osVersion.Major + osVersion.Minor / 10.0f;

                // Check the DefaultIcon value based on OS version
                if (versionNumber < 6.1)
                {
                    if (!string.Equals(run1, "%SystemRoot%\\System32\\shell32.dll,-153", StringComparison.OrdinalIgnoreCase))
                    {
                        Logger.Instance.LogPrimary($"HKLM\\...\\cmdfile\\DefaultIcon: {run1} <==== {UPD1}");
                    }
                }
                else
                {
                    if (!string.Equals(run1, "%SystemRoot%\\System32\\imageres.dll,-68", StringComparison.OrdinalIgnoreCase))
                    {
                        Logger.Instance.LogPrimary($"HKLM\\...\\cmdfile\\DefaultIcon: {run1} <==== {UPD1}");
                    }
                }
            }
            else
            {
                Console.WriteLine("DefaultIcon value not found for cmdfile.");
            }

            // Check the shell open command for cmdfile
            run1 = Registry.GetValue(@"HKEY_LOCAL_MACHINE\software\Classes\cmdfile\shell\open\command", "", null)?.ToString();
            if (!string.Equals(run1, "\"%1\" %*", StringComparison.OrdinalIgnoreCase))
            {
                Logger.Instance.LogPrimary($"HKLM\\...\\cmdfile\\shell\\open\\command: {run1} <==== {UPD1}");
            }
            else
            {
                Console.WriteLine("cmdfile shell open command is correctly configured.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in CMDASS: {ex.Message}");
        }
    }

}

