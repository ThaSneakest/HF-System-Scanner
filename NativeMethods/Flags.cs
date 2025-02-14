using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.NativeMethods
{
    public class Flags
    {
        [Flags]
        public enum MoveFileFlags : uint
        {
            MOVEFILE_REPLACE_EXISTING = 0x1,
            MOVEFILE_COPY_ALLOWED = 0x2,
            MOVEFILE_DELAY_UNTIL_REBOOT = 0x4,
        }

        [Flags]
        public enum SnapshotFlags : uint
        {
            Process = 0x00000002
        }

        [Flags]
        public enum ProcessAccessFlags : uint
        {
            Terminate = 0x0001,
            CreateThread = 0x0002,
            SetSessionId = 0x0004,
            VMOperation = 0x0008,
            VMRead = 0x0010,
            VMWrite = 0x0020,
            DupHandle = 0x0040,
            SetInformation = 0x0200, // Add this flag
            QueryInformation = 0x0400,
            Synchronize = 0x100000,
            AllAccess = 0x1F0FFF,
            QueryLimitedInformation = 0x1000, // Add this line if missing
        }
    }
}
