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
/// <typeparam name="T">The type of value to wrap</typeparam>
public class LazyStateHelper<T> where T : class {
    public delegate void StateAutoHelperValueChangedEventHandler(LazyStateHelper<T> sender, T? oldValueA, T? newValueA);

    public delegate void StateAutoHelperIsEnabledChangedEventHandler(LazyStateHelper<T> sender);

    private readonly Action<T, bool> onIsEnabledChanged;
    private T? value;
    private bool isEnabled;

    public T? Value {
        get => this.value;
        set {
            T? oldValue = this.value;
            if (!EqualityComparer<T?>.Default.Equals(oldValue, value)) {
                if (oldValue != null && this.isEnabled)
                    this.onIsEnabledChanged(oldValue, false);

                this.value = value;
                this.ValueChanged?.Invoke(this, oldValue, value);

                if (value != null && this.isEnabled)
                    this.onIsEnabledChanged(value, true);
            }
        }
    }

    public bool IsEnabled {
        get => this.isEnabled;
        set {
            if (this.isEnabled != value) {
                this.isEnabled = value;
                this.IsEnabledChanged?.Invoke(this);

                if (this.value != null) {
                    this.onIsEnabledChanged(this.value, value);
                }
            }
        }
    }

    public event StateAutoHelperValueChangedEventHandler? ValueChanged;
    public event StateAutoHelperIsEnabledChangedEventHandler? IsEnabledChanged;

    public LazyStateHelper(Action<T, bool> onIsEnabledChanged) {
        this.onIsEnabledChanged = onIsEnabledChanged;
    }
}