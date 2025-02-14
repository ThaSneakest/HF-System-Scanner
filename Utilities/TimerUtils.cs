using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Wildlands_System_Scanner.NativeMethods;
using Wildlands_System_Scanner.Utilities;

public static class TimerUtils
{
    private static List<Structs.TimerInfo> Timers = new List<Structs.TimerInfo>();

    public static bool KillTimer(IntPtr hWnd, uint timerId)
    {
        for (int i = 0; i < Timers.Count; i++)
        {
            var timer = Timers[i];
            if (timer.InternalId == timerId)
            {
                var killSuccess = hWnd == IntPtr.Zero
                    ? User32NativeMethods.KillTimer(hWnd, timer.TimerId)
                    : User32NativeMethods.KillTimer(hWnd, (IntPtr)timer.InternalId);

                if (!killSuccess)
                {
                    // Handle error (log or throw exception)
                    return false;
                }

                // Free callback if allocated
                if (timer.Callback != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(timer.Callback);
                }

                // Remove timer from list
                Timers.RemoveAt(i);
                return true;
            }
        }

        return false;
    }

    public static IntPtr SetTimer(IntPtr hWnd, uint elapse = 250, Action callback = null, uint timerId = 0)
    {
        if (timerId == 0)
        {
            // Generate unique timer ID
            timerId = (uint)Timers.Count + 1000;
            while (Timers.Exists(t => t.InternalId == timerId))
            {
                timerId++;
            }
        }

        IntPtr callbackPtr = IntPtr.Zero;
        if (callback != null)
        {
            // Allocate callback
            callbackPtr = Marshal.GetFunctionPointerForDelegate(callback);
        }

        var timerHandle = User32NativeMethods.SetTimer(hWnd, (IntPtr)timerId, elapse, callbackPtr);
        if (timerHandle == IntPtr.Zero)
        {
            // Handle error (log or throw exception)
            return IntPtr.Zero;
        }

        // Store timer information
        Timers.Add(new Structs.TimerInfo
        {
            TimerId = timerHandle,
            InternalId = timerId,
            Callback = callbackPtr
        });

        return timerHandle;
    }
}
