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
using PFXToolKitUI.Utils.Events;

namespace PFXToolKitUI.EventHelpers;

/// <summary>
/// A helper class used to wrap a value that is lazily created at some point, and provides a callback for
/// when the state is activated or deactivates (which may happen before the value is created)
/// </summary>
/// <typeparam name="T">The type of value to wrap</typeparam>
public class LazyStateHelper<T> where T : class {
    private readonly Action<T, bool> onIsEnabledChanged;
    private Optional<T> value;
    private bool isEnabled;

    public Optional<T> Value {
        get => this.value;
        set {
            Optional<T> oldValue = this.value;
            if (!oldValue.Equals(value)) {
                if (oldValue.HasValue && this.isEnabled)
                    this.onIsEnabledChanged(oldValue.Value, false);

                this.value = value;
                this.ValueChanged?.Invoke(this, new ValueChangedEventArgs<Optional<T>>(oldValue, value));

                if (value.HasValue && this.isEnabled)
                    this.onIsEnabledChanged(value.Value, true);
            }
        }
    }

    public bool IsEnabled {
        get => this.isEnabled;
        set {
            if (this.isEnabled != value) {
                this.isEnabled = value;
                this.IsEnabledChanged?.Invoke(this, EventArgs.Empty);

                if (this.value.HasValue) {
                    this.onIsEnabledChanged(this.value.Value, value);
                }
            }
        }
    }

    public event EventHandler<ValueChangedEventArgs<Optional<T>>>? ValueChanged;
    public event EventHandler? IsEnabledChanged;

    public LazyStateHelper(Action<T, bool> onIsEnabledChanged) {
        this.onIsEnabledChanged = onIsEnabledChanged;
    }
}