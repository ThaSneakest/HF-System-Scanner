using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using Wildlands_System_Scanner.Constants;
using Wildlands_System_Scanner.NativeMethods;
using Wildlands_System_Scanner.Utilities;

public static class SQLiteUtils
{
    private static IntPtr _sqliteDllHandle = IntPtr.Zero;
    private static IntPtr _sqliteDbHandle = IntPtr.Zero;
    private static List<IntPtr> _sqliteDbHandles = new List<IntPtr>();
    private static List<IntPtr> _sqliteQueryHandles = new List<IntPtr>();
    private static Action<string> _printCallback = Console.WriteLine;
    private static bool _useUtf8ErrorMsg = false;

    public static bool StartSQLite(string dllFilename = "sqlite3.dll", bool useUtf8ErrorMsg = false)
    {
        if (Environment.Is64BitProcess && !dllFilename.Contains("_x64"))
        {
            dllFilename = dllFilename.Replace(".dll", "_x64.dll");
        }

        _sqliteDllHandle = Kernel32NativeMethods.LoadLibrary(dllFilename);
        if (_sqliteDllHandle == IntPtr.Zero)
        {
            return false; // Equivalent to SetError(1, ...)
        }

        // Store UTF-8 error handling configuration
        // and perform any additional setup if needed
        return true;
    }

    public static void ShutdownSQLite()
    {
        if (_sqliteDllHandle != IntPtr.Zero)
        {
            Kernel32NativeMethods.FreeLibrary(_sqliteDllHandle);
            _sqliteDllHandle = IntPtr.Zero;
        }

        if (_sqliteDbHandle != IntPtr.Zero)
        {
            CloseDatabase(_sqliteDbHandle);
            _sqliteDbHandle = IntPtr.Zero;
        }
    }

    public static bool OpenDatabase(string databaseFilename = ":memory:")
    {
        if (_sqliteDllHandle == IntPtr.Zero)
            return false; // Equivalent to SetError(3, ...)

        var result = Sqlite3NativeMethods.sqlite3_open_v2(databaseFilename, out _sqliteDbHandle, 6 /* ReadWrite | Create */,
            IntPtr.Zero);
        if (result != 0)
        {
            CloseDatabase(_sqliteDbHandle);
            return false; // Equivalent to SetError(..., result, ...)
        }

        return true;
    }

    public static bool ExecuteQuery(string query, out List<List<string>> results)
    {
        results = new List<List<string>>();
        if (_sqliteDbHandle == IntPtr.Zero)
            return false; // Database not initialized

        IntPtr statement = IntPtr.Zero;

        // Prepare the SQL query
        int prepareResult = Sqlite3NativeMethods.sqlite3_prepare_v2(_sqliteDbHandle, query, -1, out statement, IntPtr.Zero);
        if (prepareResult != 0)
        {
            return false; // Query preparation failed
        }

        // Retrieve the results
        int columnCount = Sqlite3NativeMethods.sqlite3_column_count(statement);
        while (true)
        {
            int stepResult = Sqlite3NativeMethods.sqlite3_step(statement);
            if (stepResult == NativeMethodConstants.SQLITE_ROW)
            {
                var row = new List<string>();
                for (int i = 0; i < columnCount; i++)
                {
                    string value = Utility.PtrToStringUTF8(Sqlite3NativeMethods.sqlite3_column_text(statement, i));
                    row.Add(value);
                }

                results.Add(row);
            }
            else if (stepResult == NativeMethodConstants.SQLITE_DONE)
            {
                break; // Query execution completed
            }
            else
            {
                return false; // Query execution failed
            }
        }

        Sqlite3NativeMethods.sqlite3_finalize(statement);
        return true;
    }


    private static void CloseDatabase(IntPtr dbHandle)
    {
        if (dbHandle != IntPtr.Zero)
        {
            Sqlite3NativeMethods.sqlite3_close(dbHandle);
        }
    }


    public static int SQLiteExec(IntPtr db, string sql)
    {
        IntPtr errmsg = IntPtr.Zero;
        int result = Sqlite3NativeMethods.sqlite3_exec(db, sql, IntPtr.Zero, IntPtr.Zero, out errmsg);

        if (result != 0)
        {
            string errorMsg = Marshal.PtrToStringUni(errmsg);
            Console.WriteLine($"Error in SQLiteExec: {errorMsg}");
            return result;
        }

        return 0;
    }

    public static string SQLiteErrMsg(IntPtr db)
    {
        if (db == IntPtr.Zero)
        {
            return "Library used incorrectly";
        }

        IntPtr errmsgPtr = Sqlite3NativeMethods.sqlite3_errmsg16(db);
        return Marshal.PtrToStringUni(errmsgPtr) ?? "Library used incorrectly";
    }

    public static int SQLiteQuery(IntPtr db, string sql, out IntPtr stmt)
    {
        stmt = IntPtr.Zero;
        int result = Sqlite3NativeMethods.sqlite3_prepare16_v2(db, sql, -1, out stmt, IntPtr.Zero);

        if (result != 0)
        {
            Console.WriteLine($"Error in SQLiteQuery: {SQLiteErrMsg(db)}");
            return result;
        }

        return 0;
    }

    public static int SQLiteFetchData(IntPtr stmt, out List<object> rowData, bool binaryMode = false)
    {
        rowData = new List<object>();

        int stepResult = Sqlite3NativeMethods.sqlite3_step(stmt);
        if (stepResult != 100) // SQLITE_ROW
        {
            return stepResult;
        }

        int columnCount = Sqlite3NativeMethods.sqlite3_column_count(stmt);
        for (int i = 0; i < columnCount; i++)
        {
            int columnType = Sqlite3NativeMethods.sqlite3_column_type(stmt, i);

            if (columnType == 5) // SQLITE_NULL
            {
                rowData.Add(null);
                continue;
            }

            if (!binaryMode && columnType != 4) // Not BLOB
            {
                IntPtr textPtr = Sqlite3NativeMethods.sqlite3_column_text16(stmt, i);
                rowData.Add(Marshal.PtrToStringUni(textPtr));
            }
            else
            {
                IntPtr blobPtr = Sqlite3NativeMethods.sqlite3_column_blob(stmt, i);
                int blobSize = Sqlite3NativeMethods.sqlite3_column_bytes(stmt, i);

                byte[] blobData = new byte[blobSize];
                Marshal.Copy(blobPtr, blobData, 0, blobSize);
                rowData.Add(blobData);
            }
        }

        return 0;
    }

    public static int SQLiteClose(IntPtr dbHandle)
    {
        if (dbHandle == IntPtr.Zero)
            return -1; // Equivalent to SetError(21)

        int result = Sqlite3NativeMethods.sqlite3_close(dbHandle);
        if (result != 0)
        {
            Console.WriteLine("Error closing database.");
            return result; // Equivalent to SetError(...)
        }

        _sqliteDbHandle = IntPtr.Zero;
        _sqliteDbHandles.Remove(dbHandle);
        return result;
    }

    public static int SQLiteQueryFinalize(IntPtr stmtHandle)
    {
        if (stmtHandle == IntPtr.Zero)
            return -1; // Equivalent to SetError(21)

        int result = Sqlite3NativeMethods.sqlite3_finalize(stmtHandle);
        if (result != 0)
        {
            Console.WriteLine("Error finalizing query.");
            return result; // Equivalent to SetError(...)
        }

        _sqliteQueryHandles.Remove(stmtHandle);
        return result;
    }

    public static int SQLiteQueryReset(IntPtr stmtHandle)
    {
        if (stmtHandle == IntPtr.Zero)
            return -1; // Equivalent to SetError(21)

        int result = Sqlite3NativeMethods.sqlite3_reset(stmtHandle);
        if (result != 0)
        {
            Console.WriteLine("Error resetting query.");
            return result; // Equivalent to SetError(...)
        }

        return result;
    }

    public static int SQLiteFetchNames(IntPtr stmtHandle, out List<string> columnNames)
    {
        columnNames = new List<string>();

        if (stmtHandle == IntPtr.Zero)
            return -1; // Equivalent to SetError(21)

        int columnCount = Sqlite3NativeMethods.sqlite3_column_count(stmtHandle);
        if (columnCount <= 0)
        {
            return -1; // Equivalent to SetError(101)
        }

        for (int i = 0; i < columnCount; i++)
        {
            IntPtr columnNamePtr = Sqlite3NativeMethods.sqlite3_column_name16(stmtHandle, i);
            if (columnNamePtr == IntPtr.Zero)
            {
                Console.WriteLine("Error fetching column name.");
                return -1; // Equivalent to SetError(...)
            }

            string columnName = Marshal.PtrToStringUni(columnNamePtr);
            columnNames.Add(columnName);
        }

        return 0; // Success
    }

    private static bool SQLiteHandleCheck(IntPtr handle, bool isDb = true)
    {
        if (_sqliteDllHandle == IntPtr.Zero)
            return false; // Equivalent to SetError(21)

        if (handle == IntPtr.Zero)
        {
            if (isDb)
                handle = _sqliteDbHandle;
        }

        if (isDb)
        {
            return _sqliteDbHandles.Contains(handle);
        }
        else
        {
            return _sqliteQueryHandles.Contains(handle);
        }
    }

    private static void SQLiteHandleAdd(List<IntPtr> handleList, IntPtr handle)
    {
        if (!handleList.Contains(handle))
        {
            handleList.Add(handle);
        }
    }

    private static void SQLiteHandleRemove(List<IntPtr> handleList, IntPtr handle)
    {
        handleList.Remove(handle);
    }

    public static void SQLiteReportError(
        IntPtr dbHandle,
        string functionName,
        string query = null,
        string errorMessage = null,
        object returnValue = null,
        int currentError = 0,
        int currentExtendedError = 0)
    {
        if (string.IsNullOrEmpty(errorMessage))
        {
            errorMessage = SQLiteGetErrorMessage(dbHandle);
        }

        StringBuilder output = new StringBuilder();
        output.AppendLine("!   SQLite Error");
        output.AppendLine($"--> Function: {functionName}");
        if (!string.IsNullOrEmpty(query))
        {
            output.AppendLine($"--> Query:    {query}");
        }

        output.AppendLine($"--> Error:    {errorMessage}");

        SQLitePrint(output.ToString());

        if (returnValue != null)
        {
            throw new SQLiteException(currentError, currentExtendedError, returnValue);
        }

        throw new SQLiteException(currentError, currentExtendedError);
    }

    public static void SQLiteFree(IntPtr ptr)
    {
        if (ptr != IntPtr.Zero)
        {
            Sqlite3NativeMethods.sqlite3_free(ptr);
        }
    }

    public static byte[] StringToUtf8Bytes(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return Array.Empty<byte>();
        }

        int byteCount = Kernel32NativeMethods.WideCharToMultiByte(
            65001, 0, input, -1, IntPtr.Zero, 0, IntPtr.Zero, IntPtr.Zero);

        if (byteCount == 0)
        {
            throw new Exception("Failed to convert string to UTF-8.");
        }

        byte[] utf8Bytes = new byte[byteCount];
        IntPtr buffer = Marshal.AllocHGlobal(byteCount);

        try
        {
            Kernel32NativeMethods.WideCharToMultiByte(
                65001, 0, input, -1, buffer, byteCount, IntPtr.Zero, IntPtr.Zero);
            Marshal.Copy(buffer, utf8Bytes, 0, byteCount);
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }

        return utf8Bytes;
    }

    public static void SQLiteConsoleWrite(string text)
    {
        Console.WriteLine(text);
    }

    public static void SQLitePrint(string text)
    {
        if (_printCallback != null)
        {
            if (_useUtf8ErrorMsg)
            {
                byte[] utf8Bytes = StringToUtf8Bytes(text);
                _printCallback(Encoding.UTF8.GetString(utf8Bytes));
            }
            else
            {
                _printCallback(text);
            }
        }
    }

    private static string SQLiteGetErrorMessage(IntPtr dbHandle)
    {
        if (dbHandle == IntPtr.Zero)
        {
            return "Library used incorrectly.";
        }

        IntPtr errMsgPtr = Sqlite3NativeMethods.sqlite3_errmsg16(dbHandle);
        return Marshal.PtrToStringUni(errMsgPtr) ?? "Unknown error.";
    }

    public static IntPtr SQLiteOpen(string databasePath)
    {
        // Implement SQLite open operation
        return IntPtr.Zero;
    }

    public static bool SQLiteGetTable2D(IntPtr hSq, string query, out List<List<string>> result, out int rows,
        out int columns)
    {
        // Simulated SQLite Get Table operation
        result = new List<List<string>>();
        rows = 0;
        columns = 0;
        return true;
    }

    public static void SQLiteCl(IntPtr hSq)
    {
        SQLiteClose(hSq);
    }

    public static string SQLiteGt(string databasePath)
    {
        string items = string.Empty;
        var hSq = SQLiteOpen(databasePath);

        if (hSq == IntPtr.Zero)
            return null;

        if (SQLiteGetTable2D(hSq, "Select * From moz_perms;", out var result, out var rows,
                out var columns))
            for (int i = 1; i < rows; i++)
            {
                if (result[i][2].IndexOf("notification", StringComparison.OrdinalIgnoreCase) >= 0 &&
                    result[i][3] == "1")
                {
                    {
                        items += result[i][1] + "; ";
                    }
                }

                if (!string.IsNullOrEmpty(items))
                {
                    items = items.TrimEnd(';', ' ');
                }
            }

        SQLiteCl(hSq);
        return items;
    }

    public static ServiceControllerStatus GetServiceStatus(string serviceName)
    {
        var service = new ServiceController(serviceName);

        try
        {
            return service.Status;
        }
        catch
        {
            return ServiceControllerStatus.Stopped;
        }
    }

    public static bool StopService(string serviceName)
    {
        var service = new ServiceController(serviceName);

        try
        {
            if (service.CanStop && service.Status == ServiceControllerStatus.Running)
            {
                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(10));
                return true;
            }
        }
        catch
        {
            // Handle service stop error
        }

        return false;
    }

    public static string ConvertToUnicodeHex(string input)
    {
        var sb = new StringBuilder();

        foreach (var ch in input)
        {
            sb.Append(((int)ch).ToString("X4"));
        }

        return sb.ToString();
    }

    public static string AdjustRegistryKey(string key)
    {
        key = key.Replace("HKEY_CLASSES_ROOT", "CLASSES_ROOT")
            .Replace("HKEY_CURRENT_USER", "CURRENT_USER")
            .Replace("HKEY_LOCAL_MACHINE", "MACHINE")
            .Replace("HKEY_USERS", "USERS");
        return key;
    }


    public class SQLiteException : Exception
    {
        public int ErrorCode { get; }
        public int ExtendedErrorCode { get; }
        public object ReturnValue { get; }

        public SQLiteException(int errorCode, int extendedErrorCode, object returnValue = null)
            : base($"SQLite Error: Code {errorCode}, Extended Code {extendedErrorCode}")
        {
            ErrorCode = errorCode;
            ExtendedErrorCode = extendedErrorCode;
            ReturnValue = returnValue;
        }
    }
}
