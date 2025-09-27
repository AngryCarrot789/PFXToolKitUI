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
    /// Sets the value for a data key, or removes the entry if the value is null.
    /// </summary>
    /// <param name="key">The key</param>
    /// <param name="value">The value to insert, or null to remove</param>
    public void Set<T>(DataKey<T> key, T? value) => this.SetUnsafe(key.Id, value);

    /// <summary>
    /// Safely sets a raw value for the given key by doing runtime type-checking,
    /// or removes the entry if the value is null.
    /// </summary>
    /// <param name="key">The key</param>
    /// <param name="value">The value to insert, or null to remove</param>
    public void SetSafely(DataKey key, object? value) {
        if (!this.TrySetSafely(key, value)) {
            throw new ArgumentException($"Value is not an instance of the data key's data type. {value!.GetType().Name} is not {key.DataType.Name}");
        }
    }
    
    /// <summary>
    /// Tries to safely set a raw value for the given key by doing runtime type-checking, or removes the entry if the value is null.
    /// If the value is incompatible, false is returned and nothing else happens.
    /// </summary>
    /// <param name="key">The key</param>
    /// <param name="value">The value to insert, or null to remove</param>
    /// <returns>True if the entry was added/replaced/removed, False if trying to add/replace an entry with an incompatible value</returns>
    public bool TrySetSafely(DataKey key, object? value) {
        if (value == null) {
            this.Remove(key);
        }
        else if (key.DataType.IsInstanceOfType(value)) {
            this.SetUnsafe(key.Id, value);
        }
        else {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Sets the raw value for a key, or removes the entry if the value is null.
    /// </summary>
    /// <param name="key">The key</param>
    /// <param name="value">The value to insert</param>
    public void SetUnsafe(string key, object? value);

    /// <summary>
    /// Removes the value with the given key. This is the same as invoking <see cref="SetUnsafe"/> with a null value
    /// </summary>
    /// <param name="key">The key</param>
    public void Remove(string key) => this.SetUnsafe(key, null);

    /// <summary>
    /// Removes the value by the given key
    /// </summary>
    public void Remove(DataKey key) => this.Remove(key.Id);

    /// <summary>
    /// Batch removes the two values by the keys
    /// </summary>
    public void Remove(DataKey key1, DataKey key2) {
        using (this.BeginChange()) {
            this.Remove(key1.Id);
            this.Remove(key2.Id);
        }
    }

    /// <summary>
    /// Batch removes the three values by the keys
    /// </summary>
    public void Remove(DataKey key1, DataKey key2, DataKey key3) {
        using (this.BeginChange()) {
            this.Remove(key1.Id);
            this.Remove(key2.Id);
            this.Remove(key3.Id);
        }
    }

    /// <summary>
    /// Begins a multi-change process. These processes can be stacked, and the data will only be applied
    /// once all tokens are disposed (unless explicitly done by derived context data typed).
    /// </summary>
    /// <returns>A disposable token instance</returns>
    /// <remarks>
    /// It's important not to create local copies of the token and/or dispose them excessively, since this
    /// can lead to undefined behaviour (e.g. say three batch scopes are entered, and you dispose the same
    /// token three times, now all scopes are exited)
    /// </remarks>
    public sealed BatchToken BeginChange() {
        this.OnEnterBatchScope();
        return new BatchToken(this);
    }

    /// <summary>
    /// Invoked by <see cref="BeginChange"/>
    /// </summary>
    protected void OnEnterBatchScope();
    
    /// <summary>
    /// Invoked when the token returned by <see cref="BeginChange"/> is disposed
    /// </summary>
    protected void OnExitBatchScope();
    
    /// <summary>
    /// A token used to automatically exit a batch-change scope
    /// </summary>
    public ref struct BatchToken : IDisposable {
        private IMutableContextData? myContext;

        /// <summary>
        /// Gets the context data this token was created by
        /// </summary>
        public IMutableContextData Context => this.myContext ?? throw new ObjectDisposedException(nameof(BatchToken), "Token is disposed");

        internal BatchToken(IMutableContextData myContext) => this.myContext = myContext;

        void IDisposable.Dispose() {
            IMutableContextData? ctx = this.myContext;
            if (ctx != null) {
                this.myContext = null;
                ctx.OnExitBatchScope();
            }
        }
    }
}