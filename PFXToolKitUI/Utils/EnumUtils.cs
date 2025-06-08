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
}