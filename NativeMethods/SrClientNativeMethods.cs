using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.NativeMethods
{
    public class SrClientNativeMethods
    {
        [DllImport("SrClient.dll", CharSet = CharSet.Unicode)]
        public static extern bool SRSetRestorePointW(IntPtr restorePtSpec, IntPtr mgrStatus);

        [DllImport("SrClient.dll", CharSet = CharSet.Unicode)]
        public static extern uint SRRemoveRestorePoint(uint sequenceNumber);
    }
}
