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

using PFXToolKitUI.Utils;

namespace PFXToolKitUI.EventHelpers;

public delegate void LazyHelper1EventHandler<in T>(T value, bool hasValue);

/// <summary>
/// A helper class that stores a lazily created values, and invokes a callback whenever the
/// value change. Note, when the value changes, the callback may be called twice.
/// </summary>
/// <typeparam name="T">The value type</typeparam>
public class LazyHelper1<T>(LazyHelper1EventHandler<T> onValuesChanged) {
    public Optional<T> Value1 {
        get => field;
        set {
            Optional<T> oldValue = field;
            if (!oldValue.Equals(value)) {
                if (oldValue.HasValue)
                    onValuesChanged(oldValue.Value, false);

                field = value;
                if (value.HasValue)
                    onValuesChanged(value.Value, true);
            }
        }
    }
}