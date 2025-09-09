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

namespace PFXToolKitUI.Utils.Events;

public delegate void LazyHelper2EventHandler<in T1, in T2>(T1 value1, T2 value2, bool hasBoth);

/// <summary>
/// A helper class that stores two lazily created values, and invokes a callback whenever the
/// values change. Note, when any value is changed, the callback may be called twice.
/// </summary>
/// <typeparam name="T1">The first value type</typeparam>
/// <typeparam name="T2">The second value type</typeparam>
public class LazyHelper2<T1, T2>(LazyHelper2EventHandler<T1, T2> onValuesChanged) {
    private Optional<T1> value1;
    private Optional<T2> value2;

    public Optional<T1> Value1 {
        get => this.value1;
        set {
            Optional<T1> oldValue = this.value1;
            if (!oldValue.Equals(value)) {
                if (oldValue.HasValue && this.value2.HasValue)
                    onValuesChanged(oldValue.Value, this.value2.Value, false);

                this.value1 = value;
                if (value.HasValue && this.value2.HasValue)
                    onValuesChanged(value.Value, this.value2.Value, true);
            }
        }
    }
    
    public Optional<T2> Value2 {
        get => this.value2;
        set {
            Optional<T2> oldValue = this.value2;
            if (!oldValue.Equals(value)) {
                if (oldValue.HasValue && this.value1.HasValue)
                    onValuesChanged(this.value1.Value, oldValue.Value, false);

                this.value2 = value;
                if (value.HasValue && this.value1.HasValue)
                    onValuesChanged(this.value1.Value, value.Value, true);
            }
        }
    }
}