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

public delegate void LazyHelper3EventHandler<in T1, in T2, in T3>(T1 value1, T2 value2, T3 value3, bool hasBoth);

/// <summary>
/// A helper class that stores three lazily created values, and invokes a callback whenever the
/// values change. Note, when any value is changed, the callback may be called twice.
/// </summary>
/// <typeparam name="T1">The first value type</typeparam>
/// <typeparam name="T2">The second value type</typeparam>
/// <typeparam name="T3">The third value type</typeparam>
public class LazyHelper3<T1, T2, T3>(LazyHelper3EventHandler<T1, T2, T3> onValuesChanged) {
    private Optional<T1> value1;
    private Optional<T2> value2;
    private Optional<T3> value3;

    public Optional<T1> Value1 {
        get => this.value1;
        set {
            Optional<T1> oldValue = this.value1;
            if (!oldValue.Equals(value)) {
                if (oldValue.HasValue && this.value2.HasValue && this.value3.HasValue)
                    onValuesChanged(oldValue.Value, this.value2.Value, this.value3.Value, false);

                this.value1 = value;
                if (value.HasValue && this.value2.HasValue && this.value3.HasValue)
                    onValuesChanged(value.Value, this.value2.Value, this.value3.Value, true);
            }
        }
    }

    public Optional<T2> Value2 {
        get => this.value2;
        set {
            Optional<T2> oldValue = this.value2;
            if (!oldValue.Equals(value)) {
                if (oldValue.HasValue && this.value1.HasValue && this.value3.HasValue)
                    onValuesChanged(this.value1.Value, oldValue.Value, this.value3.Value, false);

                this.value2 = value;
                if (value.HasValue && this.value1.HasValue && this.value3.HasValue)
                    onValuesChanged(this.value1.Value, value.Value, this.value3.Value, true);
            }
        }
    }

    public Optional<T3> Value3 {
        get => this.value3;
        set {
            Optional<T3> oldValue = this.value3;
            if (!oldValue.Equals(value)) {
                if (oldValue.HasValue && this.value1.HasValue && this.value2.HasValue)
                    onValuesChanged(this.value1.Value, this.value2.Value, oldValue.Value, false);

                this.value3 = value;
                if (value.HasValue && this.value1.HasValue && this.value2.HasValue)
                    onValuesChanged(this.value1.Value, this.value2.Value, value.Value, true);
            }
        }
    }
}