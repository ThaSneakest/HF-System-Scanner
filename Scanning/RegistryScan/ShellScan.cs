using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scanning.RegistryScan
{
    public class ShellScan
    {
        public string GetShell()
        {
            string Shell = string.Empty;

            try
            {
                // Open the registry key
                using (RegistryKey CSK = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows NT\CurrentVersion\Winlogon", true))
                {
                    if (CSK != null)
                    {
                        // Retrieve the value for "Shell"
                        object A = CSK.GetValue("Shell");

                        // If value is not null, append to Shell string
                        if (A != null)
                        {
                            Shell += "Shell: " + A.ToString() + Environment.NewLine;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exception if necessary
                Console.WriteLine(ex.Message);
            }

            return Shell;
        }
    }
}
