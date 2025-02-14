using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scanning.RegistryScan
{
    public class DownloadedProgramFilesScan
    {
        public string GetDownloadedProgramFiles()
        {
            StringBuilder DPFs = new StringBuilder();

            try
            {
                using (RegistryKey CSK = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Code Store Database\Distribution Units"))
                {
                    if (CSK != null)
                    {
                        foreach (string S in CSK.GetSubKeyNames())
                        {
                            try
                            {
                                string A = (string)Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Code Store Database\Distribution Units\" + S, null, null);
                                if (A == null)
                                {
                                    A = (string)Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\CLSID\" + S, null, null);
                                }
                                string B = (string)Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Code Store Database\Distribution Units\" + S + @"\DownloadInformation", "CODEBASE", null);
                                string C = (string)Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\CLSID\" + S + @"\InprocServer32", null, null);

                                if (B != null)
                                {
                                    DPFs.Append("HKLM\\DPF:\t" + S + " ");
                                    if (A != null) DPFs.Append(A + " - ");
                                    DPFs.Append(B + Environment.NewLine);
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

            return DPFs.ToString();
        }
    }
}
