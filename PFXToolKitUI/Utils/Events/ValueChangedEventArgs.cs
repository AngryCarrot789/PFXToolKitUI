// 
// Copyright (c) 2025-2025 REghZy
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
/// Contains information about a value change event
/// </summary>
/// <typeparam name="T">The type of value that changed</typeparam>
public readonly struct ValueChangedEventArgs<T> {
    /// <summary>
    /// Gets the old value
    /// </summary>
    public T OldValue { get; }
    
    /// <summary>
    /// Gets the new/current value
    /// </summary>
    public T NewValue { get; }
    
    /// <summary>
    /// Creates a new instance of <see cref="ValueChangedEventArgs{T}"/> using the given old and new values
    /// </summary>
    /// <param name="oldValue">The old value</param>
    /// <param name="newValue">The new value</param>
    /// <typeparam name="T">The type of value that changed</typeparam>
    public ValueChangedEventArgs(T oldValue, T newValue) {
        this.OldValue = oldValue;
        this.NewValue = newValue;
    }
}