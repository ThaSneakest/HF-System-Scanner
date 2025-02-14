using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//Tested and working needs to have entries added to whitelist

namespace Wildlands_System_Scanner
{
    public class ExplorerKeyHandler
    {
        public static void HandleExplorerKeys(List<string> userKeys)
        {
            foreach (var userKey in userKeys)
            {
                string explorerSubKey = $@"{userKey}\Software\Microsoft\Windows NT\CurrentVersion\Windows";
                Logger.Instance.LogPrimary($"Scanning Registry Path: HKU\\{explorerSubKey}");

                using (RegistryKey hkuKey = Microsoft.Win32.Registry.Users.OpenSubKey(explorerSubKey))
                {
                    if (hkuKey == null)
                    {
                        Logger.Instance.LogPrimary($"Key not found: HKU\\{explorerSubKey}");
                        continue;
                    }

                    Logger.Instance.LogPrimary($"Key found: HKU\\{explorerSubKey}");
                    foreach (var valueName in hkuKey.GetValueNames())
                    {
                        string valueData = hkuKey.GetValue(valueName)?.ToString();
                        if (!string.IsNullOrEmpty(valueData))
                        {
                            Logger.Instance.LogPrimary($"Explorer Key: [{valueName}] => {valueData}");
                        }
                        else
                        {
                            Logger.Instance.LogPrimary($"Explorer Key: [{valueName}] has no value.");
                        }
                    }
                }
            }
        }

    }
}
