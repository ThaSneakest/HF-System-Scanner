using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.NativeMethods
{
    public class Ole32NativeMethods
    {
        [DllImport("ole32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern int CLSIDFromString(string lpsz, out Guid pclsid);

        [DllImport("ole32.dll", SetLastError = true)]
        public static extern int CoInitializeEx(IntPtr pvReserved, uint dwCoInit);

        [DllImport("ole32.dll", SetLastError = true)]
        public static extern void CoTaskMemFreeNative(IntPtr pv);
    }
}
