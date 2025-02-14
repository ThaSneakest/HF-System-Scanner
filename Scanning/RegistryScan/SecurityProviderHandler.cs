using System;
using Microsoft.Win32;
using Wildlands_System_Scanner.Constants;
using Wildlands_System_Scanner.Registry;

//Tested and working
public class SecurityProviderHandler
{

    public void SECPRO()
    {
        string keyPath = @"SYSTEM\CurrentControlSet\Control\SecurityProviders";
        string valueName = "SecurityProviders";
        string valueData;

        // Based on the OS version, set the appropriate value for the SecurityProviders registry key
        if (int.Parse(SystemConstants.OperatingSystemNumber.Version.Major.ToString()) < 6)
        {
            valueData = "msapsspc.dll, schannel.dll, digest.dll, msnsspc.dll";
        }
        else
        {
            valueData = "credssp.dll";
        }

        // Call method to restore the registry value
        RegistryValueHandler.RestoreRegistryValue(
            RegistryHive.LocalMachine,     // Specify the hive (HKLM)
            keyPath,                       // Registry key path
            valueName,                     // Registry value name
            valueData,                     // Data to set
            RegistryValueKind.String       // Registry value kind (REG_SZ)
        );
    }


    public static void ScanSecurityProviders()
    {
        string keyPath = @"SYSTEM\CurrentControlSet\Control\SecurityProviders";
        try
        {
            // Open the registry key
            using (var key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(keyPath))
            {
                if (key != null)
                {
                    // Fetch the value of "SecurityProviders"
                    var valueData = key.GetValue("SecurityProviders")?.ToString();
                    if (!string.IsNullOrEmpty(valueData))
                    {
                        // Log the retrieved value
                        Logger.Instance.LogPrimary($"SecurityProviders Key: HKLM\\{keyPath}, ValueData: {valueData}");
                        Console.WriteLine($"Found SecurityProviders Value: {valueData}");
                    }
                    else
                    {
                        // Handle missing or empty "SecurityProviders" value
                        Console.WriteLine($"The 'SecurityProviders' value is not set in: HKLM\\{keyPath}");
                    }
                }
                else
                {
                    // Handle missing registry key
                    Console.WriteLine($"Registry key not found: HKLM\\{keyPath}");
                }
            }
        }
        catch (Exception ex)
        {
            // Log any unexpected errors
            Console.WriteLine($"Error accessing registry key: HKLM\\{keyPath}, Error: {ex.Message}");
        }
    }
    public string GetSecurityProviders()
    {
        string SecurityProviders = string.Empty;

        try
        {
            using (RegistryKey CSK = Registry.LocalMachine.OpenSubKey(@"System\CurrentControlSet\Control\SecurityProviders"))
            {
                if (CSK != null)
                {
                    try
                    {
                        object A = CSK.GetValue("SecurityProviders");
                        if (A != null)
                        {
                            SecurityProviders += "Security Providers: " + A.ToString();
                        }
                    }
                    catch (Exception ex)
                    {
                        // Handle exception if necessary
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Handle exception if necessary
            Console.WriteLine(ex.Message);
        }

        return SecurityProviders;
    }

}
