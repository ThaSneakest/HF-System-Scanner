using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scanning.RegistryScan
{
    public class LayeredServiceProvidersScan
    {
        private HashSet<string> LSP_WL = new HashSet<string>(); // Assuming LSP_WL is a HashSet of strings
        public string GetLayeredServiceProviders()
        {
            string LSPs = string.Empty;

            try
            {
                using (RegistryKey CSK = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\services\WinSock2\Parameters\Protocol_Catalog9\Catalog_Entries"))
                {
                    if (CSK != null)
                    {
                        foreach (string S in CSK.GetSubKeyNames())
                        {
                            try
                            {
                                string A = Convert.ToString(Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\services\WinSock2\Parameters\Protocol_Catalog9\Catalog_Entries\" + S, "PackedCatalogItem", null));

                                if (!LSP_WL.Contains(A) && !string.IsNullOrEmpty(A))
                                {
                                    LSPs += "LSP:\t" + A + Environment.NewLine;
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

            return LSPs;
        }
    }
}
