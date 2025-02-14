using System;
using Wildlands_System_Scanner.NativeMethods;

namespace Wildlands_System_Scanner
{
    public class ErrorHandler
    {
        /// <summary>
        /// Simulates an error function returning a common error code.
        /// </summary>
        public static int MYERRFUNC()
        {
            const int comErr = 5; // Example error code
            return comErr;
        }

        /// <summary>
        /// Exits the program with a fatal error and an optional message.
        /// </summary>
        public static void FatalExitWithMessage(int code, string message = "")
        {
            if (!string.IsNullOrEmpty(message))
            {
                Console.WriteLine($"Fatal error: {message}");
            }
            Kernel32NativeMethods.FatalExit(code);
        }

        /// <summary>
        /// Handles error codes and logs appropriate messages based on the error type.
        /// </summary>
        public static void HandleErrorCodes(uint errorCode, string registryKey)
        {
            string message;
            switch (errorCode)
            {
                case 0:
                    message = $"No error. Registry key {registryKey} processed successfully.";
                    break;
                case 1:
                    message = $"Error: Operation failed on registry key {registryKey}. Error code: {errorCode}";
                    break;
                case 3221225506:
                    message = $"Specific error encountered on registry key {registryKey}. Error code: {errorCode}";
                    break;
                default:
                    message = $"Unknown error occurred with error code {errorCode} for registry key {registryKey}.";
                    break;
            }

            // Log or process the message
            LogError(message);

        }

        /// <summary>
        /// Sets an error and logs it with additional context, returning the provided return value.
        /// </summary>
        public static int SetError(int errorCode, int extendedCode, int returnValue)
        {
            Console.WriteLine($"Error: {errorCode}, Extended: {extendedCode}");
            return returnValue;
        }

        /// <summary>
        /// Handles registry error codes and logs specific error messages.
        /// </summary>
        public void RegistryErrorCodes(uint errorCode, string registryKey)
        {
            string message;

            switch (errorCode)
            {
                case 3221225506u:
                    message = $"{registryKey} => Not Deleted. Error Code: {errorCode}";
                    break;

                case 3221225531u:
                    message = $"{registryKey} => Not Deleted. Incorrect Path.";
                    break;

                default:
                    message = $"{registryKey} => Not Deleted. ErrorCode: {errorCode}";
                    break;
            }

            Logger.Instance.LogFix(message + "\n");
        }


        /// <summary>
        /// Sets an error and logs it, including a status message, returning the main error code.
        /// </summary>
        public static int SetError(int errorCode, int errorSubCode, string status)
        {
            Console.WriteLine($"Error Occurred - Code: {errorCode}, SubCode: {errorSubCode}, Status: {status}");
            return errorCode;
        }

        /// <summary>
        /// Logs an error message to the console or to a logging system.
        /// </summary>
        private static void LogError(string message)
        {
            if (string.IsNullOrEmpty(message)) return;

            // Log to console (could be extended to log to a file or external system)
            Console.WriteLine(message);
        }
    }
}
