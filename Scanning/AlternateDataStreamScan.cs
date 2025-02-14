using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Constants;
using Wildlands_System_Scanner.NativeMethods;
using static Wildlands_System_Scanner.NativeMethods.Structs;

namespace Wildlands_System_Scanner.Scanning.AccountScan
{
    public class AlternateDataStreamScan
    {
        public static void ScanAllDirectories(string rootDirectory)
        {
            try
            {
                // Get all directories and files under the root directory
                foreach (var directory in Directory.GetDirectories(rootDirectory, "*", SearchOption.AllDirectories))
                {
                    ScanDirectoryForADS(directory);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger.Instance.LogPrimary($"Access denied to directory: {rootDirectory}. Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while scanning: {ex.Message}");
            }
        }

        private static void ScanDirectoryForADS(string directoryPath)
        {
            try
            {
                var files = Directory.GetFiles(directoryPath);
                foreach (var file in files)
                {
                    List<string> streams = EnumerateStreams(file);
                    foreach (var stream in streams)
                    {
                        Logger.Instance.LogPrimary(stream);
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"Access denied to directory: {directoryPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error scanning directory {directoryPath}: {ex.Message}");
            }
        }

        private static List<string> EnumerateStreams(string filePath)
        {
            var streams = new List<string>();
            var findStreamData = new WIN32_FIND_STREAM_DATA();

            IntPtr hFindStream = Kernel32NativeMethods.FindFirstStreamWAlt2(filePath, 0, out findStreamData, 0);
            if (hFindStream == IntPtr.Zero)
            {
                int errorCode = Marshal.GetLastWin32Error();
                if (errorCode != NativeMethodConstants.ERROR_HANDLE_EOF)
                {
                    Console.WriteLine($"Error retrieving streams for {filePath}: {errorCode}");
                }
                return streams;
            }

            try
            {
                do
                {
                    if (!string.IsNullOrEmpty(findStreamData.StreamName) && findStreamData.StreamName != "::")
                    {
                        string streamName = findStreamData.StreamName.TrimStart(':');
                        long streamSize = findStreamData.StreamSize;
                        Logger.Instance.LogPrimary($"AlternateDataStreams: {filePath}:{streamName} [{streamSize}]");
                    }
                } while (Kernel32NativeMethods.FindNextStreamWAlt2(hFindStream, out findStreamData));
            }
            finally
            {
                Kernel32NativeMethods.FindClose(hFindStream);
            }

            return streams;
        }

    }
}
