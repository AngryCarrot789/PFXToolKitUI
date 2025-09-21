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

namespace PFXToolKitUI.Interactivity.Contexts;

/// <summary>
/// An interface for a mutable instance of <see cref="IContextData"/>
/// </summary>
public interface IMutableContextData : IRandomAccessContextData {
    /// <summary>
    /// Sets a value with the given data key.
    /// </summary>
    /// <param name="key">The key</param>
    /// <param name="value">The value to insert</param>
    void Set<T>(DataKey<T> key, T? value);

    /// <summary>
    /// Sets a boolean value with the given key, using a pre-boxed value to avoid boxing.
    /// </summary>
    /// <param name="key">The key</param>
    /// <param name="value">The value to insert</param>
    void Set(DataKey<bool> key, bool? value);

    /// <summary>
    /// Safely sets a raw value for the given key by doing runtime type-checking. 
    /// </summary>
    /// <param name="key">The key</param>
    /// <param name="value">The value to insert, or null, to remove</param>
    void SetSafely(DataKey key, object? value);

    /// <summary>
    /// Unsafely sets a raw value for the given key. Care must be taken using this method,
    /// since <see cref="DataKey{T}"/> will throw if it doesn't receive the correct value.
    /// </summary>
    /// <param name="key">The key</param>
    /// <param name="value">The value to insert, or null, to remove</param>
    void SetUnsafe(string key, object? value);

    /// <summary>
    /// Removes the value with the given key. This is the same as calling <see cref="SetUnsafe"/> with a null value
    /// </summary>
    /// <param name="key">The key</param>
    void Remove(string key) => this.SetUnsafe(key, null);

    /// <summary>
    /// Removes the value by the given key
    /// </summary>
    void Remove(DataKey key) => this.Remove(key.Id);

    /// <summary>
    /// Batch removes the two values by the keys
    /// </summary>
    void Remove(DataKey key1, DataKey key2) {
        using (this.BeginChange()) {
            this.Remove(key1.Id);
            this.Remove(key2.Id);
        }
    }

    /// <summary>
    /// Batch removes the three values by the keys
    /// </summary>
    void Remove(DataKey key1, DataKey key2, DataKey key3) {
        using (this.BeginChange()) {
            this.Remove(key1.Id);
            this.Remove(key2.Id);
            this.Remove(key3.Id);
        }
    }

    /// <summary>
    /// Begins a multi-change process. These processes can be stacked, and the data will only
    /// be applied once all tokens are disposed (but this can be optionally overridden)
    /// </summary>
    /// <returns>A disposable token instance</returns>
    MultiChangeToken BeginChange();
}