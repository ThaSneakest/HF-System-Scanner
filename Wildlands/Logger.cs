using System;
using System.IO;
using System.Text;

public class Logger
{
    public const string FixLog = "fixlog.txt"; // Default fix log file path
    private readonly string _primaryLogFilePath;
    private readonly string _additionalLogFilePath;
    private readonly string _fixLogFilePath;
    public static readonly string LogInfoPath = @"C:\Wildlands\Logs\LogInfo.txt";
    public static readonly string LogWarningPath = @"C:\Wildlands\Logs\LogWarning.txt";
    public static readonly string LogErrorPath = @"C:\Wildlands\Logs\LogError.txt";

    private static readonly Logger _instance = new Logger();

    // Singleton instance
    public static Logger Instance => _instance;

    private Logger(
        string primaryLogFilePath = @"C:\Wildlands\WildlandsLog.txt",
        string additionalLogFilePath = @"C:\Wildlands\WildlandsAdditionalLog.txt",
        string fixLogFilePath = @"C:\Wildlands\WildlandsFixLog.txt")
    {
        _primaryLogFilePath = primaryLogFilePath;
        _additionalLogFilePath = additionalLogFilePath;
        _fixLogFilePath = fixLogFilePath;

        // Ensure directories exist before initializing log files
        EnsureDirectoryExists(_primaryLogFilePath);
        EnsureDirectoryExists(_additionalLogFilePath);
        EnsureDirectoryExists(_fixLogFilePath);

        // Initialize log files
        InitializeLogFile(_primaryLogFilePath);
        InitializeLogFile(_additionalLogFilePath);
        InitializeLogFile(_fixLogFilePath);
    }

    private void EnsureDirectoryExists(string filePath)
    {
        string directoryPath = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
    }

    private void InitializeLogFile(string logFilePath)
    {
        try
        {
            using (var log = new FileStream(logFilePath, FileMode.Create, FileAccess.Write))
            using (var writer = new StreamWriter(log))
            {
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing log file {logFilePath}: {ex.Message}");
        }
    }

    public void LogPrimary(string message)
    {
        LogToFile(_primaryLogFilePath, message);
    }

    public void LogAdditional(string message)
    {
        LogToFile(_additionalLogFilePath, message);
    }

    public void LogFix(string message)
    {
        LogToFile(_fixLogFilePath, message);
    }

    private void LogToFile(string filePath, string message)
    {
        try
        {
            using (var log = new FileStream(filePath, FileMode.Append, FileAccess.Write))
            using (var writer = new StreamWriter(log))
            {
                writer.WriteLine($"{message}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error writing to log file {filePath}: {ex.Message}");
        }
    }

    public void ClearPrimaryLog()
    {
        InitializeLogFile(_primaryLogFilePath);
    }

    public void ClearAdditionalLog()
    {
        InitializeLogFile(_additionalLogFilePath);
    }

    public void ClearFixLog()
    {
        InitializeLogFile(_fixLogFilePath);
    }


    /// <summary>
    /// Logs an error message to the console and a log file.
    /// </summary>
    /// <param name="message">The error message to log.</param>
    /// <param name="exception">Optional: The exception to log.</param>
    /// <param name="logFilePath">Optional: The path to the log file. If null, the default log file path is used.</param>
    public static void LogError(string message, Exception exception = null, string logFilePath = null)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("Log message cannot be null or empty.", nameof(message));
        }

        // Use the default log file path if none is provided
        if (logFilePath == null)
        {
            logFilePath = LogErrorPath;
        }

        try
        {
            // Prepare the log message with a timestamp
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string logMessage = $"[ERROR] [{timestamp}] {message}{Environment.NewLine}";

            // Include exception details if provided
            if (exception != null)
            {
                logMessage += $"Exception: {exception.GetType().Name}{Environment.NewLine}";
                logMessage += $"Message: {exception.Message}{Environment.NewLine}";
                logMessage += $"StackTrace: {exception.StackTrace}{Environment.NewLine}";
            }

            // Write to console with red text to indicate an error
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(logMessage);
            Console.ResetColor();

            // Ensure the directory exists
            string directory = Path.GetDirectoryName(logFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Append the log message to the file
            File.AppendAllText(logFilePath, logMessage);
        }
        catch (Exception ex)
        {
            // If logging fails, write the error directly to the console
            Console.WriteLine($"[ERROR] Failed to log error: {ex.Message}");
        }
    }

    /// <summary>
    /// Writes text data to a file, optionally overwriting or appending to it.
    /// </summary>
    /// <param name="filePath">The path of the file to write to.</param>
    /// <param name="data">The text data to write.</param>
    /// <param name="overwrite">If true, overwrites the file; if false, appends to the file. Default is true.</param>
    /// <param name="encoding">The encoding to use. Default is UTF8.</param>
    public static void FileWrite(string filePath, string data, bool overwrite = true, Encoding encoding = null)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));
        }

        if (data == null)
        {
            throw new ArgumentNullException(nameof(data), "Data to write cannot be null.");
        }

        try
        {
            // Ensure the directory exists
            string directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Use the provided encoding or default to UTF8
            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }

            // Write data to the file
            if (overwrite)
            {
                File.WriteAllText(filePath, data, encoding);
            }
            else
            {
                File.AppendAllText(filePath, data, encoding);
            }

            Console.WriteLine($"File successfully written to: {filePath}");
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"Access denied: Unable to write to file {filePath}. Exception: {ex.Message}");
        }
        catch (IOException ex)
        {
            Console.WriteLine($"I/O error occurred while writing to file {filePath}. Exception: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An unexpected error occurred: {ex.Message}");
        }


    }

    /// <summary>
    /// Logs an informational message to the console and a log file.
    /// </summary>
    /// <param name="message">The informational message to log.</param>
    /// <param name="logFilePath">Optional: The path to the log file. If null, the default log file path is used.</param>
    public static void LogInfo(string message, string logFilePath = null)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("Log message cannot be null or empty.", nameof(message));
        }

        // Use the default log file path if none is provided
        if (logFilePath == null)
        {
            logFilePath = LogInfoPath;
        }

        try
        {
            // Prepare the log message with a timestamp
            string logMessage = $"[INFO] [{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}";

            // Write to console
            Console.WriteLine(logMessage);

            // Ensure the directory exists
            string directory = Path.GetDirectoryName(logFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Append the log message to the file
            File.AppendAllText(logFilePath, logMessage);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to log info: {ex.Message}");
        }
    }

    /// <summary>
    /// Logs a warning message to the console and a log file.
    /// </summary>
    /// <param name="message">The warning message to log.</param>
    /// <param name="logFilePath">Optional: The path to the log file. If null, the default log file path is used.</param>
    public static void LogWarning(string message, string logFilePath = null)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("Log message cannot be null or empty.", nameof(message));
        }

        // Use the default log file path if none is provided
        if (logFilePath == null)
        {
            logFilePath = LogWarningPath;
        }

        try
        {
            // Prepare the log message with a timestamp
            string logMessage = $"[WARNING] [{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}";

            // Write to console with yellow text to indicate a warning
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(logMessage);
            Console.ResetColor();

            // Ensure the directory exists
            string directory = Path.GetDirectoryName(logFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Append the log message to the file
            File.AppendAllText(logFilePath, logMessage);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to log warning: {ex.Message}");
        }
    }

}
