using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

public class ArrayUtils
{
    public static void ArrayAddAlt(ref string[] array, string value, string separator)
    {
        // Example method to add to an array
        Array.Resize(ref array, array.Length + 1);
        array[array.Length - 1] = value;
    }



    private static int Partition1D<T>(T[] array, int start, int end) where T : IComparable<T>
    {
        T pivot = array[end];
        int i = start - 1;

        for (int j = start; j < end; j++)
        {
            if (array[j].CompareTo(pivot) <= 0)
            {
                i++;
                (array[i], array[j]) = (array[j], array[i]);
            }
        }

        (array[i + 1], array[end]) = (array[end], array[i + 1]);
        return i + 1;
    }


    private static int Partition2D<T>(
        ref T[,] array,
        int start,
        int end,
        int subItem,
        bool descending) where T : IComparable<T>
    {
        T pivot = array[end, subItem];
        int i = start - 1;

        for (int j = start; j < end; j++)
        {
            int comparison = array[j, subItem].CompareTo(pivot);
            if ((descending && comparison >= 0) || (!descending && comparison <= 0))
            {
                i++;
                SwapRows(ref array, i, j);
            }
        }

        SwapRows(ref array, i + 1, end);
        return i + 1;
    }


    private static void SwapRows<T>(ref T[,] array, int row1, int row2)
    {
        int cols = array.GetLength(1);
        for (int col = 0; col < cols; col++)
        {
            (array[row1, col], array[row2, col]) = (array[row2, col], array[row1, col]);
        }
    }


    public static void QuickSort1D<T>(T[] array, int start, int end) where T : IComparable<T>
    {
        if (start >= end) return;

        int pivotIndex = Partition1D(array, start, end);
        QuickSort1D(array, start, pivotIndex - 1);
        QuickSort1D(array, pivotIndex + 1, end);
    }


    public static void QuickSort1D(object[] array, int start, int end, Comparison<object> comparison)
    {
        if (start >= end) return;

        int pivotIndex = Partition(array, start, end, comparison);
        QuickSort1D(array, start, pivotIndex - 1, comparison);
        QuickSort1D(array, pivotIndex + 1, end, comparison);
    }

    private static int Partition(object[] array, int start, int end, Comparison<object> comparison)
    {
        object pivot = array[end];
        int i = start - 1;

        for (int j = start; j < end; j++)
        {
            if (comparison(array[j], pivot) <= 0)
            {
                i++;
                Swap(array, i, j);
            }
        }

        Swap(array, i + 1, end);
        return i + 1;
    }

    private static void Swap(object[] array, int i, int j)
    {
        object temp = array[i];
        array[i] = array[j];
        array[j] = temp;
    }

    public static void QuickSort2D<T>(
        ref T[,] array,
        int start,
        int end,
        int subItem,
        bool descending) where T : IComparable<T>
    {
        if (start >= end) return;

        int pivotIndex = Partition2D(ref array, start, end, subItem, descending);
        QuickSort2D(ref array, start, pivotIndex - 1, subItem, descending);
        QuickSort2D(ref array, pivotIndex + 1, end, subItem, descending);
    }

    public static void QuickSort2D(
        ref object[,] array,
        int startRow,
        int endRow,
        int sortColumn,
        bool descending = false)
    {
        if (startRow >= endRow)
            return;

        int pivotIndex = Partition2D(ref array, startRow, endRow, sortColumn, descending);
        QuickSort2D(ref array, startRow, pivotIndex - 1, sortColumn, descending);
        QuickSort2D(ref array, pivotIndex + 1, endRow, sortColumn, descending);
    }

    private static int Partition2D(
        ref object[,] array,
        int startRow,
        int endRow,
        int sortColumn,
        bool descending)
    {
        object pivot = array[endRow, sortColumn];
        int i = startRow - 1;

        for (int j = startRow; j < endRow; j++)
        {
            int comparison = CompareObjects(array[j, sortColumn], pivot);
            if ((descending && comparison >= 0) || (!descending && comparison <= 0))
            {
                i++;
                SwapRows(ref array, i, j);
            }
        }

        SwapRowsAlt(ref array, i + 1, endRow);
        return i + 1;
    }

    private static int CompareObjects(object x, object y)
    {
        if (x is IComparable xComparable && y is IComparable yComparable)
            return xComparable.CompareTo(y);
        throw new InvalidOperationException("Values in the sort column must implement IComparable.");
    }

    private static void SwapRowsAlt<T>(ref T[,] array, int row1, int row2)
    {
        int cols = array.GetLength(1);
        for (int col = 0; col < cols; col++)
        {
            (array[row1, col], array[row2, col]) = (array[row2, col], array[row1, col]);
        }
    }

    public static void DualPivotQuickSort<T>(T[] data, int pivotLeft, int pivotRight, bool leftmost = true)
    {
        if (pivotLeft >= pivotRight) return;

        int length = pivotRight - pivotLeft + 1;

        // Optimization: Use insertion sort for small subarrays
        if (length < 45)
        {
            if (leftmost)
            {
                for (int i = pivotLeft + 1; i <= pivotRight; i++)
                {
                    T temp = data[i];
                    int j = i - 1;
                    while (j >= pivotLeft && Compare(data[j], temp) > 0)
                    {
                        data[j + 1] = data[j];
                        j--;
                    }
                    data[j + 1] = temp;
                }
            }
            else
            {
                while (pivotLeft < pivotRight)
                {
                    if (Compare(data[pivotLeft], data[pivotLeft + 1]) <= 0)
                    {
                        pivotLeft++;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return;
        }

        // Determine pivots using a median-of-five approach
        int seventh = length / 8 + length / 64 + 1;
        int e3 = (pivotLeft + pivotRight) / 2;
        int e2 = e3 - seventh;
        int e1 = e2 - seventh;
        int e4 = e3 + seventh;
        int e5 = e4 + seventh;

        // Sort the five elements
        SortFive(ref data, e1, e2, e3, e4, e5);

        T pivot1 = data[e2];
        T pivot2 = data[e4];
        data[e2] = data[pivotLeft];
        data[e4] = data[pivotRight];

        int less = pivotLeft + 1;
        int great = pivotRight - 1;

        for (int k = less; k <= great; k++)
        {
            if (Compare(data[k], pivot1) < 0)
            {
                Swap(ref data, k, less);
                less++;
            }
            else if (Compare(data[k], pivot2) > 0)
            {
                while (Compare(data[great], pivot2) > 0 && k < great)
                    great--;

                Swap(ref data, k, great);
                great--;

                if (Compare(data[k], pivot1) < 0)
                {
                    Swap(ref data, k, less);
                    less++;
                }
            }
        }

        less--;
        great++;

        Swap(ref data, pivotLeft, less);
        Swap(ref data, pivotRight, great);

        DualPivotQuickSort(data, pivotLeft, less - 1, leftmost);
        DualPivotQuickSort(data, great + 1, pivotRight, false);

        if (less < e3 && e5 < great)
        {
            for (int k = less + 1; k < great; k++)
            {
                if (Compare(data[k], pivot1) == 0)
                {
                    Swap(ref data, k, less);
                    less++;
                }
                else if (Compare(data[k], pivot2) == 0)
                {
                    while (Compare(data[great - 1], pivot2) == 0 && k < great - 1)
                        great--;

                    Swap(ref data, k, great - 1);
                    great--;

                    if (Compare(data[k], pivot1) == 0)
                    {
                        Swap(ref data, k, less);
                        less++;
                    }
                }
            }
        }

        DualPivotQuickSort(data, less + 1, great - 1, false);
    }

    private static void SortFive<T>(ref T[] data, int i1, int i2, int i3, int i4, int i5)
    {
        SwapIfGreater(ref data, i1, i2);
        SwapIfGreater(ref data, i2, i3);
        SwapIfGreater(ref data, i3, i4);
        SwapIfGreater(ref data, i4, i5);
        SwapIfGreater(ref data, i1, i2);
        SwapIfGreater(ref data, i2, i3);
        SwapIfGreater(ref data, i3, i4);
    }

    private static void SwapIfGreater<T>(ref T[] data, int i, int j)
    {
        if (Compare(data[i], data[j]) > 0)
        {
            Swap(ref data, i, j);
        }
    }

    private static void Swap<T>(ref T[] data, int i, int j)
    {
        T temp = data[i];
        data[i] = data[j];
        data[j] = temp;
    }

    private static int Compare<T>(T x, T y)
    {
        // Null checks
        if (x == null && y == null) return 0; // Both are null, they are equal.
        if (x == null) return -1; // Null is considered less than non-null.
        if (y == null) return 1;  // Non-null is considered greater than null.

        // Ensure both objects are of the same type
        if (x.GetType() != y.GetType())
            throw new ArgumentException("Objects must be of the same type.");

        // Use Comparer<T>.Default to compare values
        return Comparer<T>.Default.Compare(x, y);
    }

    public static void AddToRegistryArray(string entry)
    {
        // Add the entry to a list or some storage for processing (this is a placeholder)
        Console.WriteLine(entry);
    }
    public static string[,] AddToArray(string[,] array, string name, string data)
    {
        int length = array.GetLength(0);
        var newArray = new string[length + 1, 2];
        for (int i = 0; i < length; i++)
        {
            newArray[i, 0] = array[i, 0];
            newArray[i, 1] = array[i, 1];
        }
        newArray[length, 0] = name;
        newArray[length, 1] = data;

        return newArray;
    }

    public static void AddToArrayAlt(string[,] array, string name, string value)
    {
        for (int i = 0; i < array.GetLength(0); i++)
        {
            if (array[i, 0] == null) // Find an empty slot
            {
                array[i, 0] = name;
                array[i, 1] = value;
                return;
            }
        }

        throw new InvalidOperationException("The array is full.");
    }
    // Define the AddToArrayRegistry method
    public static void AddToArrayRegistry(List<string> array, string keyPath, string valueName, string valueData, string issue)
    {
        // Check if the provided arguments are valid
        if (!string.IsNullOrEmpty(keyPath) && !string.IsNullOrEmpty(valueName))
        {
            // Create a formatted string for the registry entry
            string registryEntry = $"{keyPath}\\{valueName}: {valueData} - {issue}";

            // Add the registry entry to the list
            array.Add(registryEntry);
        }
    }

    public static void AddToArray2Args(List<string> targetList, string item)
    {
        if (!targetList.Contains(item))
        {
            targetList.Add(item);
            Console.WriteLine($"Added to list: {item}");
        }
        else
        {
            Console.WriteLine($"Item already exists in the list: {item}");
        }
    }

    public static T[] AddToArray1Arg<T>(T[] array, T item)
    {
        if (array == null) return new[] { item };

        T[] newArray = new T[array.Length + 1];
        Array.Copy(array, newArray, array.Length);
        newArray[array.Length] = item;
        return newArray;
    }

    public static bool IsArray(object obj)
    {
        return obj is Array && ((Array)obj).Length > 0;
    }


}