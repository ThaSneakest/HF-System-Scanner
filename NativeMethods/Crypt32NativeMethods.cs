using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static Wildlands_System_Scanner.NativeMethods.Structs;

namespace Wildlands_System_Scanner.NativeMethods
{
    public class Crypt32NativeMethods
    {
        [DllImport("crypt32.dll", SetLastError = true)]
        public static extern bool CryptQueryObject(int dwObjectType, string pvObject, int dwExpectedContentTypeFlags, int dwExpectedFormatTypeFlags, int dwFlags, out int pdwMsgAndCertEncodingType, out int pdwContentType, out int pdwFormatType, out IntPtr phCertStore, out IntPtr phMsg);

        [DllImport("crypt32.dll", SetLastError = true)]
        public static extern bool CryptMsgGetParam(
            IntPtr hCryptMsg,
            uint dwParamType,
            uint dwIndex,
            IntPtr pvData,
            ref uint pcbData
        );

        [DllImport("crypt32.dll", SetLastError = true)]
        public static extern bool CryptMsgGetParam(IntPtr hCryptMsg, uint dwParamType, uint dwIndex, byte[] pvData, ref uint pcbData);

        [DllImport("crypt32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern int CertGetNameString(IntPtr pCertContext, int dwType, int dwFlags, IntPtr pvTypePara, char[] pszNameString, int cchNameString);

        [DllImport("crypt32.dll", SetLastError = true)]
        public static extern IntPtr CertFindCertificateInStore(IntPtr hCertStore, int dwCertEncodingType, int dwFindFlags, int dwFindType, CERT_INFO pCertInfo);


    }
}
