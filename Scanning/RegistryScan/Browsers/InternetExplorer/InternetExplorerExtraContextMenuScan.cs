using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scanning.RegistryScan.Browsers.InternetExplorer
{
    public class InternetExplorerExtraContextMenuScan
    {
        public string GetIEExtraContextMenuItems()
        {
            StringBuilder IEExtraContextMenu = new StringBuilder();

            try
            {
                using (RegistryKey CSK = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Internet Explorer\MenuExt"))
                {
                    if (CSK != null)
                    {
                        foreach (string S in CSK.GetSubKeyNames())
                        {
                            try
                            {
                                string A = Convert.ToString(Microsoft.Win32.Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Internet Explorer\MenuExt\" + S, null, null));

                                if (!string.IsNullOrEmpty(A))
                                {
                                    IEExtraContextMenu.Append("IE:\t" + S + " - " + A + Environment.NewLine);
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

            return IEExtraContextMenu.ToString();
        }
    }
}
