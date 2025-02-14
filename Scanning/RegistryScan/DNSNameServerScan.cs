using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scanning.RegistryScan
{
    public class DNSNameServerScan
    {
        public string GetNameServer()
        {
            string NameServer = string.Empty;

            try
            {
                using (RegistryKey CSK = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"System\CurrentControlSet\Services\Tcpip\Parameters"))
                {
                    if (CSK != null)
                    {
                        try
                        {
                            object A = CSK.GetValue("NameServer");
                            if (A != null)
                            {
                                NameServer += "Name Server: " + A.ToString();
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

            return NameServer;
        }
    }
}
