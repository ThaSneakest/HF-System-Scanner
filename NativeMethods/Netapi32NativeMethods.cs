using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.NativeMethods
{
    public class Netapi32NativeMethods
    {
        [DllImport("netapi32.dll", CharSet = CharSet.Auto)]
        public static extern int NetUserEnum(
            string serverName,
            uint level,
            uint filter,
            ref IntPtr bufPtr,
            uint prefMaxLen,
            ref uint entriesRead,
            ref uint totalEntries,
            IntPtr resumeHandle);

        [DllImport("netapi32.dll", CharSet = CharSet.Auto)]
        public static extern int NetApiBufferFree(IntPtr buffer);

        [DllImport("netapi32.dll", CharSet = CharSet.Unicode)]
        public static extern int NetUserDel(string serverName, string userName);


        [DllImport("netapi32.dll", CharSet = CharSet.Auto)]
        public static extern int NetUserEnum(
            string serverName,
            int level,
            int filter,
            ref IntPtr bufPtr,
            uint prefMaxLen,
            ref uint entriesRead,
            ref uint totalEntries,
            ref uint resumeHandle
        );
    }
}
