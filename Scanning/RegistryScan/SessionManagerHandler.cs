using Microsoft.Win32;
using System;
using System.Windows.Forms;
using Wildlands_System_Scanner.Constants;

namespace Wildlands_System_Scanner
{
    public class SessionManagerHandler
    {
        public static string GetSubsystemValue()
        {
            // Check if not in recovery mode or the DllDirectory32 key exists
            using (RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Session Manager\KnownDLLs"))
            {
                if (SystemConstants.BootMode != "Recovery" || key?.GetValue("DllDirectory32") != null)
                {
                    return null;
                }
            }

            string subsystemValue = "";
            string sharedSectionValue;

            // Determine the SharedSection value based on OS version
            var osVersion = SystemConstants.OperatingSystemNumber.Version;

            if (osVersion.Major == 5 && osVersion.Minor == 1)
            {
                sharedSectionValue = "3072";
            }
            else
            {
                sharedSectionValue = "12288";
            }

            // Base string for the subsystem value
            string baseSubsystemValue = @"%SystemRoot%\system32\csrss.exe ObjectDirectory=\Windows SharedSection=1024," + sharedSectionValue + @",512 Windows=On SubSystemType=Windows ServerDll=basesrv,1 ServerDll=winsrv:UserServerDllInitialization,3 ServerDll=";

            switch (osVersion.Major)
            {
                case 5:
                    if (osVersion.Minor == 1)
                    {
                        if (SystemConstants.BootMode != "Recovery") return null;
                        subsystemValue = baseSubsystemValue + "winsrv:ConServerDllInitialization,2 ProfileControl=Off MaxRequestThreads=16";
                    }
                    break;

                case 6:
                    if (osVersion.Minor == 0 || osVersion.Minor == 1)
                    {
                        if (SystemConstants.BootMode != "Recovery") return null;
                        subsystemValue = baseSubsystemValue + "winsrv:ConServerDllInitialization,2 ServerDll=sxssrv,4 ProfileControl=Off MaxRequestThreads=16";
                    }
                    else if (osVersion.Minor == 2 || osVersion.Minor == 3)
                    {
                        subsystemValue = @"%SystemRoot%\system32\csrss.exe ObjectDirectory=\Windows SharedSection=1024,12288,512 Windows=On SubSystemType=Windows ServerDll=basesrv,1 ServerDll=winsrv:UserServerDllInitialization,3 ServerDll=sxssrv,4 ProfileControl=Off MaxRequestThreads=16";
                    }
                    break;

                case 10:
                    subsystemValue = @"%SystemRoot%\system32\csrss.exe ObjectDirectory=\Windows SharedSection=1024,12288,512 Windows=On SubSystemType=Windows ServerDll=basesrv,1 ServerDll=winsrv:UserServerDllInitialization,3 ServerDll=sxssrv,4 ProfileControl=Off MaxRequestThreads=16";
                    break;
            }

            return subsystemValue;
        }
    }
}
