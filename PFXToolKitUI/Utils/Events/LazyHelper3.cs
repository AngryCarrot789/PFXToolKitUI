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

/// <summary>
/// A helper class used to wrap a value that is lazily created at some point, and provides a callback for
/// when the state is activated or deactivates (which may happen before the value is created)
/// </summary>
/// <typeparam name="T1">The first type of value to wrap</typeparam>
/// <typeparam name="T2">The second type of value to wrap</typeparam>
/// <typeparam name="T3">The third type of value to wrap</typeparam>
public class LazyHelper3<T1, T2, T3>(Action<T1, T2, T3, bool> onIsEnabledChanged) {
    private Optional<T1> value1;
    private Optional<T2> value2;
    private Optional<T3> value3;

    public Optional<T1> Value1 {
        get => this.value1;
        set {
            Optional<T1> oldValue = this.value1;
            if (!oldValue.Equals(value)) {
                if (oldValue.HasValue && this.value2.HasValue && this.value3.HasValue)
                    onIsEnabledChanged(oldValue.Value, this.value2.Value, this.value3.Value, false);

                this.value1 = value;
                if (value.HasValue && this.value2.HasValue && this.value3.HasValue)
                    onIsEnabledChanged(value.Value, this.value2.Value, this.value3.Value, true);
            }
        }
    }

    public Optional<T2> Value2 {
        get => this.value2;
        set {
            Optional<T2> oldValue = this.value2;
            if (!oldValue.Equals(value)) {
                if (oldValue.HasValue && this.value1.HasValue && this.value3.HasValue)
                    onIsEnabledChanged(this.value1.Value, oldValue.Value, this.value3.Value, false);

                this.value2 = value;
                if (value.HasValue && this.value1.HasValue && this.value3.HasValue)
                    onIsEnabledChanged(this.value1.Value, value.Value, this.value3.Value, true);
            }
        }
    }

    public Optional<T3> Value3 {
        get => this.value3;
        set {
            Optional<T3> oldValue = this.value3;
            if (!oldValue.Equals(value)) {
                if (oldValue.HasValue && this.value1.HasValue && this.value2.HasValue)
                    onIsEnabledChanged(this.value1.Value, this.value2.Value, oldValue.Value, false);

                this.value3 = value;
                if (value.HasValue && this.value1.HasValue && this.value2.HasValue)
                    onIsEnabledChanged(this.value1.Value, this.value2.Value, value.Value, true);
            }
        }
    }
}