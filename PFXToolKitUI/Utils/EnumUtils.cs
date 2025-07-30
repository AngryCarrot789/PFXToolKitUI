// 
// Copyright (c) 2023-2025 REghZy
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

using System.Runtime.CompilerServices;

namespace PFXToolKitUI.Utils;

public static class EnumUtils {
    public static bool IsValid<T>(T value) where T : unmanaged, Enum {
        if (EnumInfo<T>.IsUnsigned) {
            ulong val64 = EnumInfo<T>.GetUnsignedValue(value);
            ulong min64 = EnumInfo<T>.GetUnsignedValue(EnumInfo<T>.MinValue);
            ulong max64 = EnumInfo<T>.GetUnsignedValue(EnumInfo<T>.MaxValue);
            return val64 >= min64 && val64 <= max64;
        }
        else {
            long val64 = EnumInfo<T>.GetSignedValue(value);
            long min64 = EnumInfo<T>.GetSignedValue(EnumInfo<T>.MinValue);
            long max64 = EnumInfo<T>.GetSignedValue(EnumInfo<T>.MaxValue);
            return val64 >= min64 && val64 <= max64;
        }
    }

    public static void Validate<T>(T value, [CallerArgumentExpression(nameof(value))] string? paramName = null) where T : unmanaged, Enum {
        if (!IsValid(value)) {
            throw new ArgumentOutOfRangeException(paramName ?? nameof(value), value, $"Enum value is out of range. Must be between {EnumInfo<T>.MinValue} and {EnumInfo<T>.MaxValue}");
        }
    }
}