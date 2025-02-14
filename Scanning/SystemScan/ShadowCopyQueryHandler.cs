using System;
using System.Management;

public class ShadowCopyQueryHandler
{
    public string DEVOBJ(ManagementObjectSearcher objWMI)
    {
        string SHAD = null;

        // WMI Query for Win32_ShadowCopy
        ManagementObjectCollection objWMIService1 = objWMI.Get();

        foreach (ManagementObject objItem in objWMIService1)
        {
            // Retrieve DeviceObject property from each shadow copy
            SHAD = objItem["DeviceObject"]?.ToString();
        }

        // Return the last found shadow copy device object (or null if none found)
        return SHAD;
    }
}