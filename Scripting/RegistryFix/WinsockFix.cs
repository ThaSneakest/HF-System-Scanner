using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Constants;
using Wildlands_System_Scanner.Registry;

namespace Wildlands_System_Scanner.Scripting.RegistryFix
{
    public class WinsockFix
    {
        private static string TEMP_DIR = @"C:\Temp";
        private static string LP = @"%SystemRoot%\System32\mswsock.dll";
        public static void WINSOCKFIX(string FIX)
        {
            string ID = "";
            string LP = "";
            string VAL = "";
            string KEY1 = @"HKLM\SYSTEM\CurrentControlSet\Services\WinSock2\Parameters\";

            VAL = Regex.Replace(FIX, @"Winsock: Catalog\d (\d+) .+", "$1");
            if (VAL.Length == 2)
            {
                VAL = VAL.PadLeft(10, '0'); // Add leading zeros to make it 10 digits
            }

            if (FIX.Contains("Winsock: Catalog5 "))
            {
                string KEY = KEY1 + @"NameSpace_Catalog5\Catalog_Entries\";

                using (RegistryKey registryKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(KEY + VAL))
                {
                    if (registryKey != null)
                    {
                        ID = RegistryValueHandler.TryReadRegistryValue(registryKey, "ProviderId");
                    }
                    else
                    {
                        Console.WriteLine($"Registry key not found: {KEY + VAL}");
                        ID = null; // Assign a default value if the key is not found
                    }
                }

                switch (ID)
                {
                    case "0x409D05229E7ECF11AE5A00AA00A7112B":
                        LP = @"%SystemRoot%\System32\mswsock.dll";
                        FixCatalog5(KEY, VAL);
                        break;

                    case "0xA2CB4A96BCB2EB408C6AA6DB40161CAE":
                        LP = @"%SystemRoot%\system32\napinsp.dll";
                        FixCatalog5(KEY, VAL);
                        break;

                    case "0xCE89FE036D767649B9C1BB9BC42C7B4D":
                    case "0xCD89FE036D767649B9C1BB9BC42C7B4D":
                        LP = @"%SystemRoot%\system32\pnrpnsp.dll";
                        FixCatalog5(KEY, VAL);
                        break;

                    case "0x3A244266A83BA64ABAA52E0BD71FDD83":
                        if (SystemConstants.OperatingSystemNumber.Version.Major < 6)
                        {
                            LP = @"%SystemRoot%\System32\mswsock.dll";
                        }
                        else
                        {
                            LP = @"%SystemRoot%\system32\NLAapi.dll";
                        }
                        FixCatalog5(KEY, VAL);
                        break;

                    case "0xEE37263B80E5CF11A55500C04FD8D4AC":
                        LP = @"%SystemRoot%\System32\winrnr.dll";
                        FixCatalog5(KEY, VAL);
                        break;

                    default:
                        FixCatalog5(KEY, VAL);
                        File.AppendAllText(Path.Combine(TEMP_DIR, "catalog0"), "Catalog5 ");
                        break;
                }
            }

            if (FIX.Contains("Winsock: Catalog9 "))
            {
                string KEY = KEY1 + @"Protocol_Catalog9\Catalog_Entries\";

                if (FIX.Contains("mswsock") || FIX.Contains("rsvpsp"))
                {
                    Logger.Instance.LogPrimary($"{StringConstants.CAT1} \"{VAL}\" {StringConstants.CAT2}.{Environment.NewLine}");
                }
                else
                {
                    DelCatalog(KEY, VAL);
                    File.AppendAllText(Path.Combine(TEMP_DIR, "catalog0"), "Catalog9 ");
                }
            }
        }
        public static void FixCatalog5(string key, string val)
        {
            string registryPath = $"{key}{val}\\LibraryPath";
            bool ret = RegistryUtilsScript.RegWrite(registryPath, "LibraryPath", "REG_SZ", LP);

            if (ret)
            {
                Logger.Instance.LogPrimary($"Winsock: Catalog5 {val}\\LibraryPath => {StringConstants.RESTORED} ({LP}){Environment.NewLine}");
            }
            else
            {
                Logger.Instance.LogPrimary($"{key}{val}\\LibraryPath => {StringConstants.ERRSV}{Environment.NewLine}");
            }
        }

        public static void DelCatalog(string key, string val)
        {
            string fullKey = key + val;
            RegistryKeyHandler.DeleteRegistryKey(fullKey);
        }

        public static void ResetCatalog(string key)
        {
            int i = 1;
            string vars = null;  // Initialize to null
            string vard = null;  // Initialize to null

            // Open the registry key
            using (RegistryKey registryKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(key, writable: true))
            {
                if (registryKey == null)
                {
                    Console.WriteLine("Error: Unable to open the registry key.");
                    return;
                }

                // Loop through all subkeys
                while (true)
                {
                    // Enumerate the subkey at the given index
                    try
                    {
                        vars = registryKey.GetSubKeyNames()[i - 1]; // Get subkey name at index i (1-based index in AutoIt, 0-based in C#)

                        if (vars == null)
                        {
                            break;
                        }

                        // Format the new name with padded zeros
                        if (i < 10)
                            vard = "00000000000" + i;
                        else if (i >= 10 && i < 100)
                            vard = "0000000000" + i;
                        else if (i >= 100)
                            vard = "000000000" + i;

                        // Move (rename) the registry key
                        RegistryKeyHandler.RenameRegistryKey(registryKey, vars, vard);
                    }
                    catch (Exception ex)
                    {
                        // Log the error if any
                        File.AppendAllText("fixlog.txt", $"Winsock reset error: {key}: {vars}={ex.Message}{Environment.NewLine}");
                        break;
                    }

                    i++;
                }
            }
        }
    }
}
