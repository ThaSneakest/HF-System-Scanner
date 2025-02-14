using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Constants
{
    public class NativeMethodConstants
    {
        public const int PROCESS_QUERY_INFORMATION = 0x0400;
        public const int PROCESS_VM_READ = 0x0010;
        public const uint SERVICE_STOP = 0x0020;
        public const uint SERVICE_CONTROL_STOP = 0x00000001;
        public const uint SERVICE_QUERY_STATUS = 0x0004;
        public const string SERVICES_ACTIVE_DATABASE = "ServicesActive";
        public const uint SC_MANAGER_CONNECT = 0x0001;
        public const int TOKEN_ADJUST_PRIVILEGES = 0x0020;
        public const int TOKEN_QUERY = 0x0008;
        public const int SE_PRIVILEGE_ENABLED = 0x0002;

        public const int FILE_FLAG_OPEN_REPARSE_POINT = 0x00200000;
        public const int FILE_FLAG_BACKUP_SEMANTICS = 0x02000000;
        public const int DELETE = 0x10000;
        public const int OPEN_EXISTING = 3;
        public const uint GENERIC_READ = 0x80000000;

        public const uint DONT_RESOLVE_DLL_REFERENCES = 0x00000001;
        public const uint LOAD_WITH_ALTERED_SEARCH_PATH = 0x00000008;
        public const uint LOAD_LIBRARY_AS_DATAFILE = 0x00000002;
        public const uint LOAD_LIBRARY_AS_IMAGE_RESOURCE = 0x00000020;

        public const uint CRYPT_VERIFYCONTEXT = 0xF0000000; // Equivalent to 4026531840
        public static IntPtr _advapi32Handle = IntPtr.Zero;
        public static IntPtr _cryptContextHandle = IntPtr.Zero;
        public static int _refCount = 0;
        public static IntPtr CryptContext = IntPtr.Zero;
        public static IntPtr _dllHandle = IntPtr.Zero;
        public static IntPtr _cryptContext = IntPtr.Zero;

        public const uint IOCTL_DISK_GET_LENGTH_INFO = 0x0007405C;
        public const uint FORMAT_MESSAGE_IGNORE_INSERTS = 0x00000200;
        public const uint FORMAT_MESSAGE_FROM_HMODULE = 0x00000800;

        public const long EpochOffset = 116444736000000000; // Offset to convert FILETIME to Unix epoch
        public const long TicksMultiplier = 10000000; // Multiplier to convert seconds to 100-nanosecond intervals

        public const uint EVENTLOG_SEQUENTIAL_READ = 0x0001;
        public const uint EVENTLOG_SEEK_READ = 0x0002;
        public const uint EVENTLOG_FORWARDS_READ = 0x0004;
        public const uint EVENTLOG_BACKWARDS_READ = 0x0008;

        public const int FILE_SHARE_READ = 0x00000001;
        public const int FILE_ATTRIBUTE_NORMAL = 0x80;
        public const uint GENERIC_WRITE = 0x40000000;
        public static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
        public const uint FSCTL_GET_REPARSE_POINT = 0x900A8;
        public const int MAXIMUM_REPARSE_DATA_BUFFER_SIZE = 16 * 1024;
        public const uint SYMBOLIC_LINK_FLAG_FILE = 0;
        public const uint SYMBOLIC_LINK_FLAG_DIRECTORY = 1;
        public const uint MAX_REPARSE_DATA_BUFFER_SIZE = 1024;
        public const uint MOVEFILE_REPLACE_EXISTING = 0x1;
        public const uint MOVEFILE_DELAY_UNTIL_REBOOT = 0x4;
        public const uint FILE_SHARE_WRITE = 0x2;
        public const int FILE_USERS_DEFAULT = 131097;
        public const int FILE_AUTH_USERS_DEFAULT = 268435456;
        public const uint FSCTL_DELETE_REPARSE_POINT = 0x900A4; // Control code for deleting reparse points
        public const uint DDD_REMOVE_DEFINITION = 0x00000002;

        public const int MaxPath = 260;
        public const int ProcessBasicInformation = 0;
        public const int ProcessBreakOnTermination = 29;

        public const uint KEY_READ = 0x20019;  // Read access to the registry key
        public const uint KEY_WRITE = 0x20006; // Write access to the registry key
        public const uint KEY_ALL_ACCESS = 0xF003F;
        public const int STATUS_SUCCESS = 0x00000000;
        public const uint NTSTATUS_NO_MORE_ENTRIES = 0x8000001A;
        public const uint NTSTATUS_BUFFER_TOO_SMALL = 0x80000007;
        public const int STATUS_NO_MORE_ENTRIES = unchecked((int)0x8000001A);
        public const int STATUS_BUFFER_OVERFLOW = unchecked((int)0x80000005);
        public const int STATUS_BUFFER_TOO_SMALL = unchecked((int)0x80000005);
        public const int ERROR_ACCESS_DENIED = 5;
        public const int STATUS_ACCESS_DENIED = 5;
        public const string SE_BACKUP_NAME = "SeBackupPrivilege";
        public const string SE_RESTORE_NAME = "SeRestorePrivilege";
        public const int OWNER_SECURITY_INFORMATION = 1;
        public const int GROUP_SECURITY_INFORMATION = 2;
        public const int DACL_SECURITY_INFORMATION = 4;
        public const int SACL_SECURITY_INFORMATION = 8;

        public const int REG_SZ = 1;
        public const int REG_BINARY = 3;
        public const int REG_DWORD = 4;
        public static readonly IntPtr HKEY_LOCAL_MACHINE = new IntPtr(unchecked((int)0x80000002));
        public const int OBJ_CASE_INSENSITIVE = 0x40;
        public const int ERROR_SUCCESS = 0;
        public const int ERROR_NO_MORE_ITEMS = 259; // No more items in enumeration
        public const int REG_LINK = 6; // Registry link type

        public const uint SE_PRIVILEGE_DISABLED = 0x0;

        public const int SQLITE_ROW = 100;
        public const int SQLITE_DONE = 101;
        public const int ERROR_MORE_DATA = 234;
        public const uint TH32CS_SNAPMODULE = 0x00000008;
        public const string WINTRUST_ACTION_GENERIC_VERIFY_V2 = "{00AAC56B-CD44-11D0-8CC2-00C04FC295EE}";
        public const uint SERVICE_QUERY_CONFIG = 0x0001;

        public const uint TH32CS_SNAPHEAPLIST = 0x00000001;
        public const uint TH32CS_SNAPPROCESS = 0x00000002;
        public const uint TH32CS_SNAPTHREAD = 0x00000004;
        public const uint TH32CS_SNAPMODULE32 = 0x00000010; // Include 32-bit modules
        public const uint TH32CS_SNAPALL = (TH32CS_SNAPHEAPLIST | TH32CS_SNAPPROCESS | TH32CS_SNAPTHREAD | TH32CS_SNAPMODULE);
        public const uint TH32CS_INHERIT = 0x80000000;

        public const int ERROR_HANDLE_EOF = 38;

        public enum SidNameUse
        {
            User = 1,
            Group,
            Domain,
            Alias,
            WellKnownGroup,
            DeletedAccount,
            Invalid,
            Unknown,
            Computer
        }

        public enum STREAM_INFO_LEVELS
        {
            FindStreamInfoStandard = 0
        }
    }
}
