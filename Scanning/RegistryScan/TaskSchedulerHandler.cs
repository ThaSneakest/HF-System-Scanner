using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using static TaskSchedulerHandler;
using System.Threading.Tasks;
using Microsoft.Win32.TaskScheduler;
using Wildlands_System_Scanner.Blacklist;
using Wildlands_System_Scanner.Whitelist;
using Task = Microsoft.Win32.TaskScheduler.Task;

public static class TaskSchedulerHandler
{
    [ComImport]
    [Guid("F5BC8FC5-536D-4F77-B852-FBC1356FDEB6")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ITaskService
    {
        void Connect(object serverName = null, object user = null, object domain = null, object password = null);
        ITaskFolder GetFolder(string path);
        // Other methods skipped for brevity.
    }

    [ComImport]
    [Guid("8CFAC062-A080-4C15-9A88-AA7C2AF80DFC")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ITaskFolder
    {
        string Name { get; }
        string Path { get; }
        ITaskFolderCollection GetFolders(int flags);
        IRegisteredTaskCollection GetTasks(int flags);
        // Other methods skipped for brevity.
    }

    [ComImport]
    [Guid("86627EB4-42A7-41E4-A4D9-AC33A72F2D52")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ITaskFolderCollection : IEnumerable<ITaskFolder>
    {
        ITaskFolder this[int index] { get; }
        int Count { get; }
    }

    [ComImport]
    [Guid("79184A66-8664-423F-97F1-637356A5D812")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IRegisteredTaskCollection : IEnumerable<IRegisteredTask>
    {
        IRegisteredTask this[int index] { get; }
        int Count { get; }
    }

    [ComImport]
    [Guid("9C86F320-DEE3-4DD1-B972-A303F26B061E")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IRegisteredTask
    {
        string Name { get; }
        string Path { get; }
        DateTime LastRunTime { get; }
        DateTime NextRunTime { get; }
        int State { get; }
    }

    public static List<string> GetTaskFolders(string startFolder = "\\", bool recurse = true)
    {
        var folders = new List<string>();

        // Create an instance of the COM TaskScheduler object
        var type = Type.GetTypeFromProgID("Schedule.Service");
        if (type == null)
            throw new InvalidOperationException("Failed to get TaskScheduler type from ProgID.");

        var taskService = (ITaskService)Activator.CreateInstance(type);
        taskService.Connect();

        ITaskFolder rootFolder = taskService.GetFolder(startFolder);
        CollectFoldersRecursive(rootFolder, folders, recurse);

        return folders;
    }

    private static void CollectFoldersRecursive(ITaskFolder folder, List<string> folders, bool recurse)
    {
        folders.Add(folder.Path);

        foreach (ITaskFolder subFolder in folder.GetFolders(0))
        {
            if (recurse)
            {
                CollectFoldersRecursive(subFolder, folders, true);
            }
        }
    }

    public static void ListTaskDetails(string startFolder = "\\", bool includeHidden = true, bool recurse = true)
    {
        var folders = GetTaskFolders(startFolder, recurse);

        var type = Type.GetTypeFromProgID("Schedule.Service");
        if (type == null)
            throw new InvalidOperationException("Failed to get TaskScheduler type from ProgID.");

        var taskService = (ITaskService)Activator.CreateInstance(type);
        taskService.Connect();

        foreach (var folderPath in folders)
        {
            ITaskFolder folder = taskService.GetFolder(folderPath);

            foreach (IRegisteredTask task in folder.GetTasks(includeHidden ? 1 : 0))
            {
                Console.WriteLine($"Task Name: {task.Name}");
                Console.WriteLine($"Last Run Time: {task.LastRunTime}");
                Console.WriteLine($"Next Run Time: {task.NextRunTime}");
                Console.WriteLine($"State: {task.State}");
                Console.WriteLine(new string('-', 50));
            }
        }
    }

    public static string GetSharedTaskScheduler()
    {
        StringBuilder STS = new StringBuilder();

        try
        {
            using (RegistryKey CSK = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\SharedTaskScheduler"))
            {
                if (CSK != null)
                {
                    foreach (string S in CSK.GetValueNames())
                    {
                        try
                        {
                            string A = Convert.ToString(Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\CLSID\" + S, null, null));
                            string B = Convert.ToString(Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\CLSID\" + S + @"\InprocServer32", null, null));

                            if (!string.IsNullOrEmpty(B))
                            {
                                STS.Append("HKLM\\STS:\t" + S + " ");
                                if (!string.IsNullOrEmpty(A)) STS.Append(A + " - ");
                                STS.Append(B + Environment.NewLine);
                            }
                        }
                        catch (Exception ex)
                        {
                            // Handle exception if necessary
                            Console.WriteLine(ex.Message);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Handle exception if necessary
            Console.WriteLine(ex.Message);
        }

        return STS.ToString();
    }

    // This method retrieves and logs the entries from Task Scheduler registry keys
    public static string GetTaskSchedulerEntries()
    {
        StringBuilder taskSchedulerEntries = new StringBuilder();

        // Registry paths for TaskScheduler (for both HKLM and HKCU)
        string[] registryPaths = new string[]
        {
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\TaskScheduler",   // For HKLM
                @"Software\Microsoft\Windows\CurrentVersion\TaskScheduler"    // For HKCU
        };

        // Iterate through each registry path to find Task Scheduler entries
        foreach (string registryPath in registryPaths)
        {
            try
            {
                // Open the registry key for TaskScheduler (for both HKLM and HKCU)
                using (RegistryKey taskSchedulerKey = Registry.LocalMachine.OpenSubKey(registryPath) ?? Registry.CurrentUser.OpenSubKey(registryPath))
                {
                    if (taskSchedulerKey != null)
                    {
                        // Enumerate the subkeys (tasks) in the TaskScheduler registry key
                        foreach (string subKeyName in taskSchedulerKey.GetSubKeyNames())
                        {
                            try
                            {
                                // Get the task information (you can add more specific data as per your need)
                                string taskPath = subKeyName;

                                // Log the task name and path in the desired format
                                taskSchedulerEntries.Append("Task:\t" + taskPath + " ");
                                taskSchedulerEntries.Append(Environment.NewLine);
                            }
                            catch (Exception ex)
                            {
                                taskSchedulerEntries.AppendLine($"Error accessing task '{subKeyName}': {ex.Message}");
                            }
                        }
                    }
                    else
                    {
                        taskSchedulerEntries.AppendLine("The registry key does not exist: " + registryPath);
                    }
                }
            }
            catch (Exception ex)
            {
                taskSchedulerEntries.AppendLine($"Error accessing the registry path '{registryPath}': {ex.Message}");
            }
        }

        return taskSchedulerEntries.ToString();
    }

    public static void EnumerateScheduledTasks()
    {
        try
        {
            using (TaskService taskService = new TaskService())
            {
                StringBuilder sb = new StringBuilder();

                foreach (Task task in taskService.AllTasks)
                {
                    try
                    {
                        string taskName = task.Name;
                        string taskPath = task.Path;
                        string actionPath = GetActionPath(task);

                        // Skip whitelisted tasks
                        if (ScheduledTaskWhitelist.TaskWhitelist.Contains(taskPath))
                        {
                            continue;
                        }

                        // Check if the task is in the blacklist
                        if (ScheduledTaskBlacklist.TaskBlacklist.Contains(taskPath))
                        {
                            sb.AppendLine($"Task: {taskPath} => {actionPath} <---- Malicious Task Found");
                        }
                        else
                        {
                            // Default task logging
                            sb.AppendLine($"Task: {taskPath} => {actionPath}");
                        }
                    }
                    catch (Exception ex)
                    {
                        sb.AppendLine($"Failed to enumerate task: {task.Name}, Error: {ex.Message}");
                    }
                }

                // Write the output to the log
                Logger.Instance.LogPrimary(sb.ToString());
            }
        }
        catch (Exception ex)
        {
            Logger.Instance.LogPrimary($"Failed to enumerate scheduled tasks: {ex.Message}");
        }
    }

    private static string GetActionPath(Task task)
    {
        try
        {
            foreach (Microsoft.Win32.TaskScheduler.Action action in task.Definition.Actions)
            {
                if (action is ExecAction execAction)
                {
                    string filePath = execAction.Path;

                    // Get file details
                    if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                    {
                        FileInfo fileInfo = new FileInfo(filePath);
                        string fileSize = fileInfo.Length.ToString();
                        string fileDate = fileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm");
                        string company = FileUtils.GetFileCompany(filePath);
                        string signStatus = FileUtils.IsFileSigned(filePath) ? "Signed" : "File not signed";

                        return $"{filePath} [{fileSize} {fileDate}] ({company}) [{signStatus}]";
                    }

                    return filePath;
                }
            }
        }
        catch (Exception ex)
        {
            return $"Error retrieving action path: {ex.Message}";
        }

        return "No executable action found";
    }
}
