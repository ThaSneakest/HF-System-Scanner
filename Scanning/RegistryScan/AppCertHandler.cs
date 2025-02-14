using System;
using Microsoft.Win32;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Wildlands_System_Scanner.Constants;
using Wildlands_System_Scanner.Registry;

public class AppCertHandler
{
    // Method to handle the AppCert functionality

    private static DevExpress.XtraEditors.LabelControl _labelControlProgressB;

    // Assign the label control instance
    public static void InitializeLabel(DevExpress.XtraEditors.LabelControl labelControlProgressB)
    {
        _labelControlProgressB = labelControlProgressB;
    }

    public static void HandleAppCert()
    {
        string registryKey = $@"HKLM\Software\Microsoft\Windows NT\CurrentVersion\Control\Session Manager\AppCertDlls";
        string labelText = $"Registry: {registryKey}";

        // Update label with key info (replace with your UI update logic)
        UpdateLabel(labelText);

        // Open the registry key for reading
        using (var key = RegistryKeyHandler.OpenRegistryKey(registryKey))
        {
            if (key == null)
                return;

            // Get the values of the registry key
            var registryValues = key.GetValueNames();

            if (registryValues.Length > 0)
            {
                string attention = " <==== " + StringConstants.UPD1;

                foreach (var valueName in registryValues)
                {
                    string valueData = key.GetValue(valueName)?.ToString() ?? string.Empty;

                    if (File.Exists(valueData))
                    {
                        Logger.Instance.LogPrimary(
                            $"HKLM\\...\\AppCertDlls: [{valueName}] -> {valueData} " +
                            $"[{FileUtils.GetFileSize(valueData)} {FileUtils.GetFileCreationDate(valueData)}] {FileUtils.GetFileCompany(valueData)} {attention}"
                        );
                    }
                    else
                    {
                        // Handle missing or invalid value data
                        if (string.IsNullOrWhiteSpace(valueData))
                            valueData = "[X]";

                        Logger.Instance.LogPrimary($"HKLM\\...\\AppCertDlls: [{valueName}] -> {valueData} {attention}");
                    }
                }
            }
        }
    }



    private static void UpdateLabel(string text)
    {
        // Update the DevExpress label if available
        if (_labelControlProgressB != null)
        {
            _labelControlProgressB.Text = text;
        }
        else
        {
            // Fallback to console output
            Console.WriteLine(text);
        }
    }
}