using System;
using System.IO;
using Microsoft.Win32;
using Wildlands_System_Scanner.Scripting;

public class RebootHandler
{
    // Function to perform the reboot process
    public static void Reboot()
    {
        string valuac;
        string key = @"HKLM\Software\Microsoft\Windows\CurrentVersion\Policies\System";
        string bootPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FRST", "re"); // Set to the location of the reboot file
        string scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FRST", "Hives", "uac");

        // Read the current value of "ConsentPromptBehaviorAdmin" from the registry
        using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(key))
        {
            if (registryKey != null)
            {
                valuac = registryKey.GetValue("ConsentPromptBehaviorAdmin", null) as string;
                if (valuac != null && valuac != "0")
                {
                    // Save the value to a file before changing it
                    File.WriteAllText(scriptPath, valuac);
                    // Set the registry value to 0
                    registryKey.SetValue("ConsentPromptBehaviorAdmin", 0, RegistryValueKind.DWord);
                }
            }
        }

        // Set the reboot path to run the current script once
        string scriptFullPath = "\"" + Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "YourScriptName.exe") + "\"";
        scriptFullPath = scriptFullPath.Replace("\\\\", "\\");

        // Write the reboot script to RunOnce registry
        using (RegistryKey runOnceKey = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\RunOnce", true))
        {
            if (runOnceKey != null)
            {
                runOnceKey.SetValue("*FRST", scriptFullPath, RegistryValueKind.String);
            }
        }

        // Message box to display information about rebooting (implement your UI)
        Console.WriteLine("Reboot is scheduled. Press Enter to continue...");
        Console.ReadLine();

        // Call method to move the file on reboot (not defined in original code)
        FileFix.MoveFileOnReboot(bootPath, "");

    }
}