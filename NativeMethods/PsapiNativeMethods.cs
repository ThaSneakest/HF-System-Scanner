using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.NativeMethods
{
    public class PsapiNativeMethods
    {
        [DllImport("psapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern uint GetProcessImageFileName(IntPtr hProcess, StringBuilder lpImageFileName, uint nSize);

        [DllImport("psapi.dll", SetLastError = true)]
        public static extern int EnumProcessModules(IntPtr hProcess, IntPtr pModules, uint cb, out uint lpcbNeeded);

        [DllImport("psapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, StringBuilder lpBaseName, uint nSize);

        [DllImport("psapi.dll", SetLastError = true)]
        public static extern bool EnumProcesses(uint[] processIds, int size, out int bytesReturned);

        [DllImport("psapi.dll", SetLastError = true)]
        public static extern bool EnumDeviceDrivers(IntPtr[] lpImageBase, uint cb, out uint lpcbNeeded);

        [DllImport("psapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern uint GetDeviceDriverFileName(IntPtr ImageBase, StringBuilder lpFilename, int nSize);
    }
}
