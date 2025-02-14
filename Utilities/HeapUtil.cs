using System;
using System.Runtime.InteropServices;
using Wildlands_System_Scanner.NativeMethods;
using Wildlands_System_Scanner.Utilities;

public static class HeapUtil
{
    private static IntPtr _heapHandle = IntPtr.Zero;

    public static IntPtr HeapAlloc(uint size, bool abortOnFail = false)
    {
        if (_heapHandle == IntPtr.Zero)
        {
            _heapHandle = Kernel32NativeMethods.HeapCreate(0, UIntPtr.Zero, UIntPtr.Zero);
            if (_heapHandle == IntPtr.Zero)
            {
                throw new InvalidOperationException("HeapCreate failed.");
            }
        }

        IntPtr memory = Kernel32NativeMethods.HeapAlloc(_heapHandle, 0x00000008, new UIntPtr(size));
        if (memory == IntPtr.Zero)
        {
            if (abortOnFail)
            {
                throw new OutOfMemoryException("HeapAlloc failed.");
            }
            return IntPtr.Zero;
        }

        return memory;
    }

    public static bool HeapFree(ref IntPtr memory, bool check = false)
    {
        if (check && !Kernel32NativeMethods.HeapValidate(_heapHandle, 0, memory))
        {
            return false;
        }

        if (!Kernel32NativeMethods.HeapFree(_heapHandle, 0, memory))
        {
            return false;
        }

        memory = IntPtr.Zero;
        return true;
    }

    public static UIntPtr HeapSize(IntPtr memory, bool check = false)
    {
        if (check && !Kernel32NativeMethods.HeapValidate(_heapHandle, 0, memory))
        {
            return UIntPtr.Zero;
        }

        return Kernel32NativeMethods.HeapSize(_heapHandle, 0, memory);
    }

    public static bool HeapValidate(IntPtr memory)
    {
        return Kernel32NativeMethods.HeapValidate(_heapHandle, 0, memory);
    }
}
