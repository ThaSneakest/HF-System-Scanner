using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Constants;
using Wildlands_System_Scanner.Registry;
using Wildlands_System_Scanner.Utilities;

namespace Wildlands_System_Scanner.Scripting.RegistryFix
{
    public class CertificateFix
    {
        public static void CertFix(string fix)
        {
            if (SystemConstants.BootMode != "Recovery" && ServiceUtils.ServiceStatus("AppInf") == "R")
            {
                ServicesFix.StopService("AppInf");
                if (ServiceUtils.ServiceStatus("AppInf") != "S") System.Threading.Thread.Sleep(2500);
                if (ServiceUtils.ServiceStatus("AppInf") != "S") System.Threading.Thread.Sleep(2500);
            }

            string sub = Regex.Replace(fix, ".*?: (.+?) \\(.+", "$1");

            string key = string.Empty;

            if (fix.Contains("HKLM"))
            {
                key = @"HKLM\SOFTWARE\Microsoft\SystemCertificates\Disallowed\Certificates\" + sub;
                if (fix.Contains("policies")) key = @"HKLM\SOFTWARE\Policies\Microsoft\SystemCertificates\Disallowed\Certificates\" + sub;
            }
            else
            {
                string user = Regex.Replace(fix, @"(?i)HKU\\(.+?)\\..+", "$1");
                key = @"HKU\" + user + @"\Software\Microsoft\SystemCertificates\Disallowed\Certificates\" + sub;
                if (fix.Contains("policies")) key = @"HKU\" + user + @"\Software\Policies\Microsoft\SystemCertificates\Disallowed\Certificates\" + sub;
            }

            RegistrySubKeyHandler.DeleteSubKey(key);
        }
    }
}
