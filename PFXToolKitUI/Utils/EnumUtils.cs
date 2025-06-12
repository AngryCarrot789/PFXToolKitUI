// 
// Copyright (c) 2023-2025 REghZy
// 
// This file is part of FramePFX.
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
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
// 

using System.Runtime.CompilerServices;
using PFXToolKitUI.PropertyEditing.DataTransfer.Enums;

namespace PFXToolKitUI.Utils;

public static class EnumUtils {
    public static bool IsValid<T>(T value) where T : unmanaged, Enum {
        T min = DataParameterEnumInfo<T>.MinValue;
        T max = DataParameterEnumInfo<T>.MaxValue;
        
        long val64 = Unsafe.As<T, long>(ref value);
        long min64 = Unsafe.As<T, long>(ref min);
        long max64 = Unsafe.As<T, long>(ref max);
        return val64 >= min64 && val64 <= max64;
    }

    public static void Validate<T>(T value, [CallerArgumentExpression(nameof(value))] string? paramName = null) where T : unmanaged, Enum {
        if (!IsValid(value)) {
            throw new ArgumentOutOfRangeException(paramName ?? nameof(value), value, $"Enum value is out of range. Must be between {DataParameterEnumInfo<T>.MinValue} and {DataParameterEnumInfo<T>.MaxValue}");
        }
    }

    public static long GetSignedValue<TEnum>(TEnum value, Type underlyingType) where TEnum : struct, Enum {
        if (underlyingType == typeof(sbyte))
            return Unsafe.As<TEnum, sbyte>(ref value);
        if (underlyingType == typeof(short))
            return Unsafe.As<TEnum, short>(ref value);
        if (underlyingType == typeof(int))
            return Unsafe.As<TEnum, int>(ref value);
        if (underlyingType == typeof(long))
            return Unsafe.As<TEnum, long>(ref value);
        throw new ArgumentOutOfRangeException(nameof(value));
    }
    
    public static ulong GetUnsignedValue<TEnum>(TEnum value, Type underlyingType) where TEnum : struct, Enum {
        if (underlyingType == typeof(byte))
            return Unsafe.As<TEnum, byte>(ref value);
        if (underlyingType == typeof(ushort))
            return Unsafe.As<TEnum, ushort>(ref value);
        if (underlyingType == typeof(uint))
            return Unsafe.As<TEnum, uint>(ref value);
        if (underlyingType == typeof(ulong))
            return Unsafe.As<TEnum, ulong>(ref value);
        throw new ArgumentOutOfRangeException(nameof(value));
    }
}