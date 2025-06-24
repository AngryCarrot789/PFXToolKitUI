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

using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Data;

namespace PFXToolKitUI.Avalonia.Utils;

public static class AvalonidaPropertyHelper {
    public static bool TryGetOldValue<TValue>(this AvaloniaPropertyChangedEventArgs<TValue> args, [NotNullWhen(true)] out TValue value) {
        Optional<TValue> oldVal = (args).OldValue;
        if (oldVal.HasValue && (value = oldVal.Value) != null)
            return true;

        value = default!;
        return false;
    }

    public static bool TryGetNewValue<TValue>(this AvaloniaPropertyChangedEventArgs<TValue> args, [NotNullWhen(true)] out TValue value) {
        BindingValue<TValue> newVal = (args).NewValue;
        if (newVal.HasValue && (value = newVal.Value) != null)
            return true;

        value = default!;
        return false;
    }
}