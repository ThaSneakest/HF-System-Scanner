using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Constants;
using Wildlands_System_Scanner.Registry;

//Needs testing

namespace Wildlands_System_Scanner
{
    public class SubHandler
    {
        public static string SUB()
        {
            try
            {
                // Define the registry key path
                string registryKey = $@"SYSTEM\CurrentControlSet\Control\Session Manager\KnownDLLs";

                // Get the value of DllDirectory32
                object dllDirectory32 = Microsoft.Win32.Registry.GetValue(registryKey, "DllDirectory32", null);

                // Check conditions
                if (SystemConstants.BootMode != "Recovery" || dllDirectory32 != null)
                {
                    return null;
                }

                string VD = string.Empty;
                string NUMB;

                // Extract OS version information
                var osVersion = SystemConstants.OperatingSystemNumber.Version;

                // Determine NUMB based on OS version
                if (osVersion.Major == 5 && osVersion.Minor == 1)
                {
                    NUMB = "3072";
                }
                else
                {
                    NUMB = "12288";
                }

                // Base string
                string AAA = $@"%SystemRoot%\system32\csrss.exe ObjectDirectory=\Windows SharedSection=1024,{NUMB},512 Windows=On SubSystemType=Windows ServerDll=basesrv,1 ServerDll=winsrv:UserServerDllInitialization,3 ServerDll=";

                // Determine VD based on OS version
                if (osVersion.Major == 5 && osVersion.Minor == 1)
                {
                    if (SystemConstants.BootMode != "Recovery") return null;
                    VD = AAA + "winsrv:ConServerDllInitialization,2 ProfileControl=Off MaxRequestThreads=16";
                }
                else if (osVersion.Major == 6 && osVersion.Minor == 0)
                {
                    if (SystemConstants.BootMode != "Recovery") return null;
                    VD = AAA + "winsrv:ConServerDllInitialization,2 ProfileControl=Off MaxRequestThreads=16";
                }
                else if (osVersion.Major == 6 && osVersion.Minor == 1)
                {
                    if (SystemConstants.BootMode != "Recovery") return null;
                    VD = AAA + "winsrv:ConServerDllInitialization,2 ServerDll=sxssrv,4 ProfileControl=Off MaxRequestThreads=16";
                }
                else if ((osVersion.Major == 6 && (osVersion.Minor == 2 || osVersion.Minor == 3)) || osVersion.Major == 10)
                {
                    VD = @"%SystemRoot%\system32\csrss.exe ObjectDirectory=\Windows SharedSection=1024,12288,512 Windows=On SubSystemType=Windows ServerDll=basesrv,1 ServerDll=winsrv:UserServerDllInitialization,3 ServerDll=sxssrv,4 ProfileControl=Off MaxRequestThreads=16";
                }
                else
                {
                    // Handle unsupported OS versions
                    Console.WriteLine($"Unsupported OS version: {osVersion.Major}.{osVersion.Minor}");
                    return null;
                }

                return VD;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SUB: {ex.Message}");
                return null;
            }
        }




    }
}
