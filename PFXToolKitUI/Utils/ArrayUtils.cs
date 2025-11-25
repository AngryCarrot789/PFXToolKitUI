// 
// Copyright (c) 2024-2025 REghZy
// 
// This file is part of PFXToolKitUI.
// 
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 3 of the License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with PFXToolKitUI. If not, see <https://www.gnu.org/licenses/>.
// 

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace PFXToolKitUI.Utils;

/// <summary>
/// Contains function that apparently aren't in C# but are in java, used for NBT equality testing
/// </summary>
public static class ArrayUtils {
    // Using IEqualityComparer + generic functions is easier than having
    // a Hash function for all types of non-struct type arrays

    public static int Hash<T>(T[]? array) {
        if (array == null)
            return 0;

        EqualityComparer<T> cmp = EqualityComparer<T>.Default;
        int result = 1;
        foreach (T t in array)
            result = 31 * result + (t != null ? cmp.GetHashCode(t) : 0);
        return result;
    }

    public static bool Equals<T>(T[]? a, T[]? b) {
        int length;
        if (a == b)
            return true;
        if (a == null || b == null || b.Length != (length = a.Length))
            return false;

        EqualityComparer<T> cmp = EqualityComparer<T>.Default;
        for (int i = 0; i < length; i++) {
            if (!cmp.Equals(a[i], b[i])) {
                return false;
            }
        }

        return true;
    }

    public static bool Equals<T>(T[]? a, T[]? b, Func<T, T, bool> equalityFunction) {
        int length;
        if (a == b)
            return true;
        if (a == null || b == null || a.Length != (length = b.Length))
            return false;

        for (int i = 0; i < length; i++) {
            if (!equalityFunction(a[i], b[i])) {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Creates a new array and may use unsafe methods of copying data
    /// from the source to destination if the array is large enough
    /// </summary>
    /// <param name="array"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    [return: NotNullIfNotNull(nameof(array))]
    public static unsafe T[]? CloneArrayUnsafe<T>(this T[]? array) where T : unmanaged {
        if (array == null)
            return null;

        int length = array.Length;
        T[] values = new T[length];
        int bytes = sizeof(T) * length;
        if (bytes > 100 || length > 50) {
            // BlockCopy will most likely help out
            Buffer.BlockCopy(array, 0, values, 0, bytes);
        }
        else if (length > 0) {
            for (int i = 0; i < length; i++)
                values[i] = array[i];
        }

        return values;
    }

    [return: NotNullIfNotNull(nameof(array))]
    public static T[]? CloneArrayMax<T>(this T[]? array) => array != null ? CloneArrayMax(array, array.Length) : null;

    public static T[] CloneArrayMax<T>(this T[] array, int count) {
        int len = array.Length;
        T[] values = new T[Math.Max(len, count)];
        for (int i = 0; i < len; i++)
            values[i] = array[i];
        return values;
    }

    public static T[] CloneArrayMin<T>(this T[] array, int minCount) {
        T[] values = new T[minCount];
        for (int i = 0, count = Math.Min(array.Length, minCount); i < count; i++)
            values[i] = array[i];
        return values;
    }

    public static T[] Add<T>(T[] array, T value) {
        T[] newArray = new T[array.Length + 1];
        Array.Copy(array, 0, newArray, 0, array.Length);
        newArray[array.Length] = value;
        return newArray;
    }

    // RemoveAt([ # # # # # ], 4)
    //                    _
    public static T[] RemoveAt<T>(T[] array, int index) {
        Debug.Assert(array.Length > 0);
        T[] newArray = new T[array.Length - 1];
        if (index > 0)
            Array.Copy(array, 0, newArray, 0, index);
        if (index != newArray.Length)
            Array.Copy(array, index + 1, newArray, index, newArray.Length - index);
        return newArray;
    }

    public static int IndexOf_RefType<T>(T[] array, T value) where T : class {
        for (int i = 0; i < array.Length; i++) {
            if (Equals(value, array[i])) {
                return i;
            }
        }

        return -1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ThrowIfOutOfBounds(Array array, int offset, int count) {
        ArgumentNullException.ThrowIfNull(array);
        ArgumentOutOfRangeException.ThrowIfNegative(offset);
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        
        ulong length = (ulong) array.LongLength;
        if ((uint) offset > length || (ulong) count > length - (uint) offset)
            throw new ArgumentException($"Offset and count are out of bounds for the buffer: {offset} + {count}");
    }
}