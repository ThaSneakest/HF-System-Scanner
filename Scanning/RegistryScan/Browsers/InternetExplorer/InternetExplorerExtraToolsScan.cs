using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scanning.RegistryScan.Browsers
{
    public class InternetExplorerExtraToolsScan
    {
        public string GetIEExtraToolsMenuItems()
        {
            StringBuilder IEExtraToolsMenu = new StringBuilder();

            try
            {
                using (RegistryKey CSK = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Internet Explorer\Extensions"))
                {
                    if (CSK != null)
                    {
                        foreach (string S in CSK.GetSubKeyNames())
                        {
                            try
                            {
                                string C = Convert.ToString(Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Internet Explorer\Extensions\" + S, "ButtonText", null));
                                if (C == null)
                                {
                                    C = Convert.ToString(Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Internet Explorer\Extensions\" + S, "MenuText", null));
                                }

                                string A = Convert.ToString(Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Internet Explorer\Extensions\" + S, "ClsidExtension", null));
                                string B = Convert.ToString(Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\CLSID\" + A + @"\InprocServer32", null, null));
                                if (!string.IsNullOrEmpty(B))
                                {
                                    IEExtraToolsMenu.Append("IE:\t" + A + " ");
                                    if (!string.IsNullOrEmpty(C)) IEExtraToolsMenu.Append(C + " - ");
                                    IEExtraToolsMenu.Append(B + " - (COM)");
                                    IEExtraToolsMenu.Append(Environment.NewLine);
                                }

                                A = Convert.ToString(Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Internet Explorer\Extensions\" + S, "BandCLSID", null));
                                B = Convert.ToString(Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\CLSID\" + A + @"\InprocServer32", null, null));
                                if (!string.IsNullOrEmpty(B))
                                {
                                    IEExtraToolsMenu.Append("IE:\t" + A + " ");
                                    if (!string.IsNullOrEmpty(C)) IEExtraToolsMenu.Append(C + " - ");
                                    IEExtraToolsMenu.Append(B + " - (explorer bar)");
                                    IEExtraToolsMenu.Append(Environment.NewLine);
                                }

                                A = Convert.ToString(Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Internet Explorer\Extensions\" + S, "Script", null));
                                B = Convert.ToString(Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\CLSID\" + A + @"\InprocServer32", null, null));
                                if (!string.IsNullOrEmpty(B))
                                {
                                    IEExtraToolsMenu.Append("IE:\t" + A + " ");
                                    if (!string.IsNullOrEmpty(C)) IEExtraToolsMenu.Append(C + " - ");
                                    IEExtraToolsMenu.Append(B + " - (script)");
                                    IEExtraToolsMenu.Append(Environment.NewLine);
                                }

                                A = Convert.ToString(Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Internet Explorer\Extensions\" + S, "Exec", null));
                                B = Convert.ToString(Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\CLSID\" + A + @"\InprocServer32", null, null));
                                if (!string.IsNullOrEmpty(B))
                                {
                                    IEExtraToolsMenu.Append("IE:\t" + A + " ");
                                    if (!string.IsNullOrEmpty(C)) IEExtraToolsMenu.Append(C + " - ");
                                    IEExtraToolsMenu.Append(B + " - (executable)");
                                    IEExtraToolsMenu.Append(Environment.NewLine);
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

            return IEExtraToolsMenu.ToString();
        }
    }
}
