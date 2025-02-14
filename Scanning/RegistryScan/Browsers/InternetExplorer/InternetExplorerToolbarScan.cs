using DevExpress.Utils;
using Microsoft.Win32;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Wildlands_System_Scanner;
using Wildlands_System_Scanner.Constants;
using Wildlands_System_Scanner.Registry;

public class InternetExplorerToolbarScan
{
    private const string COMPANY = "Microsoft";

    public static void Toolbar(string key)
    {
        string clsid, valName, filePath;
        string company = string.Empty;
        string cdate = string.Empty;
        int i = 1;

        // Log the key being scanned

        // Assuming LABEL1 is a label control on Form1
        Label LABEL1 = new Label(); // This should be the actual label from the form

        // Assuming UpdateLabel is a method to update label text
        UpdateLabel(LABEL1, $"{StringConstants.SCANB} Internet: {key}");

        // Delete temp file if it exists
        string tempFile = Path.Combine("toolbr");
        if (File.Exists(tempFile))
        {
            File.Delete(tempFile);
        }

        while (true)
        {
            // Initialize variables
            company = string.Empty;
            clsid = RegistryValueHandler.EnumerateRegistryValue(key, i); // Example, implement registry enumeration method
            if (string.IsNullOrEmpty(clsid))
                break;

            if (Regex.IsMatch(clsid, @"\{.+\}"))
            {
                valName = RegistryUtils.RegRead(@"HKCU\Software" + clsid, string.Empty).ToString(); // Example, implement registry read method
                if (string.IsNullOrEmpty(valName))
                {
                    valName = StringConstants.FF1;
                }

                filePath = RegistryUtils.RegRead(@"HKCU\Software" + clsid + @"\InprocServer32", string.Empty).ToString(); // Example, implement registry read method

                if (filePath.Contains("mscoree.dll"))
                {
                    MSCOREEHandler.MSCOREE(@"HKCU\Software" + clsid + @"\InprocServer32", ref filePath);

                }

                string file = filePath;
                cdate = string.Empty;

                // Perform additional file processing (e.g., checking file existence)
                ProcessFile(ref file, ref cdate);

                // Write data to temp file
                File.AppendAllText(tempFile, $"HKLM {valName} - {clsid} - {filePath} {cdate} {company}{Environment.NewLine}");
            }
            i++;
        }

        // Handle GUI control reading and output generation
        int checkBoxValue = 4; // Example, implement GUI control reading
        if (checkBoxValue == 4)
        {
            string fileContent = File.ReadAllText(tempFile);
            Logger.Instance.LogPrimary(fileContent);
        }
        else
        {
            string regexContent = File.ReadAllText(tempFile);
            regexContent = Regex.Replace(regexContent, @"(?i)Toolbar: HKU\\.+? -> .+\\system32\\(SHELL32|browseui|ieframe).dll \[.+\] \(Microsoft.+\)\v{2}", string.Empty);
            Logger.Instance.LogPrimary(regexContent);
        }

        // Delete temp file
        File.Delete(tempFile);
    }

    private static void ProcessFile(ref string file, ref string cdate)
    {
        // Simulate file processing, e.g., check file existence
        if (File.Exists(file))
        {
            cdate = "[File Date]";
        }
        else
        {
            file = $"{file} {StringConstants.REGIST8}";
        }
    }

    
    public string GetIEToolbars()
    {
        StringBuilder Toolbars = new StringBuilder();

        try
        {
            using (RegistryKey IEToolbarsCSK = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Internet Explorer\Toolbar"))
            {
                if (IEToolbarsCSK != null)
                {
                    foreach (string S in IEToolbarsCSK.GetValueNames())
                    {
                        try
                        {
                            string IEToolbarA = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\CLSID\" + S, null, null);
                            string IEToolbarB = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\CLSID\" + S + @"\InprocServer32", null, null);

                            if (!string.IsNullOrEmpty(IEToolbarB))
                            {
                                Toolbars.Append("HKLM\\TB:\t" + S + " ");
                                if (!string.IsNullOrEmpty(IEToolbarA)) Toolbars.Append(IEToolbarA + " - ");
                                Toolbars.Append(IEToolbarB + Environment.NewLine);

                                using (RegistryKey CSK_User = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Internet Explorer\Toolbar"))
                                {
                                    if (CSK_User != null)
                                    {
                                        foreach (string S1 in CSK_User.GetValueNames())
                                        {
                                            try
                                            {
                                                IEToolbarA = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\CLSID\" + S, null, null);
                                                IEToolbarB = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\CLSID\" + S + @"\InprocServer32", null, null);

                                                if (!string.IsNullOrEmpty(IEToolbarB))
                                                {
                                                    Toolbars.Append("HKCU\\TB:\t" + S1 + " ");
                                                    if (!string.IsNullOrEmpty(IEToolbarA)) Toolbars.Append(IEToolbarA + " - ");
                                                    Toolbars.Append(IEToolbarB + Environment.NewLine);
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
        }
        catch (Exception ex)
        {
            // Handle exception if necessary
            Console.WriteLine(ex.Message);
        }

        return Toolbars.ToString();
    }

    private static void UpdateLabel(Label label, string text)
    {
        if (label == null)
            throw new ArgumentNullException(nameof(label));

        label.Text = text;

        // Optional: Refresh the label to reflect the updated text immediately
        label.Refresh();
    }
}
