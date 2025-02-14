using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.NativeMethods
{
    public class IphlpapiNativeMethods
    {

        [DllImport("iphlpapi.dll", CharSet = CharSet.Auto)]
        public static extern uint GetNetworkParams(IntPtr pFixedInfo, ref int pOutBufLen);
    }
}
