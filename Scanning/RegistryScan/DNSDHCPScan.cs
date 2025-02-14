using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scanning.RegistryScan
{
    public class DNSDHCPScan
    {
        public string GetDHCPNameServer()
        {
            string DHCPNameServer = string.Empty;

            try
            {
                using (RegistryKey CSK = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"System\CurrentControlSet\Services\Tcpip\Parameters"))
                {
                    if (CSK != null)
                    {
                        try
                        {
                            object A = CSK.GetValue("DHCPNameServer");
                            if (A != null)
                            {
                                DHCPNameServer += "DHCP Name Server: " + A.ToString();
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

            return DHCPNameServer;
        }
    }
}
