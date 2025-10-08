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

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace PFXToolKitUI.Interactivity.Contexts;

/// <summary>
/// An implementation of <see cref="IMutableContextData"/> that stores entries in a dictionary
/// </summary>
public sealed class ConcurrentContextData : IMutableContextData {
    private readonly ConcurrentDictionary<string, object> myMap;
    
    public int Count => this.myMap.Count;

    public bool IsEmpty => this.myMap.IsEmpty;

    public IEnumerable<KeyValuePair<string, object>> Entries => this.myMap;

    /// <summary>
    /// Creates a new empty context data instance
    /// </summary>
    public ConcurrentContextData() {
        this.myMap = new ConcurrentDictionary<string, object>();
    }

    /// <summary>
    /// Copy constructor, effectively the same as <see cref="Clone"/>
    /// </summary>
    /// <param name="context">The context to copy from</param>
    public ConcurrentContextData(ConcurrentContextData context) {
        this.myMap = !context.myMap.IsEmpty
            ? new ConcurrentDictionary<string, object>(context.myMap) 
            : new ConcurrentDictionary<string, object>();
    }

    /// <summary>
    /// Adds all entries from the given context to this instance
    /// </summary>
    /// <param name="context">The context to copy from</param>
    public ConcurrentContextData(IContextData context) : this() {
        this.AddAll(context);
    }
    
    /// <summary>
    /// Adds all entries from the given context to this instance
    /// </summary>
    /// <param name="context">The context to copy the entires from</param>
    public ConcurrentContextData AddAll(IContextData context) {
        using IEnumerator<KeyValuePair<string, object>> enumerator = context.Entries.GetEnumerator();
        while (enumerator.MoveNext()) {
            KeyValuePair<string, object> entry = enumerator.Current;
            this.myMap[entry.Key] = entry.Value;
        }

        return this;
    }
    
    #region New Overrides, for builder-styled syntax

    /// <summary>
    /// Sets a value with the given data key.
    /// </summary>
    /// <param name="key">The key</param>
    /// <param name="value">The value to insert</param>
    public ConcurrentContextData Set<T>(DataKey<T> key, T? value) {
        ((IMutableContextData) this).SetUnsafe(key.Id, value);
        return this;
    }

    /// <summary>
    /// Safely sets a raw value for the given key by doing runtime type-checking. 
    /// </summary>
    /// <param name="key">The key</param>
    /// <param name="value">The value to insert, or null, to remove</param>
    public ConcurrentContextData SetSafely(DataKey key, object? value) {
        ((IMutableContextData) this).SetSafely(key, value);
        return this;
    }
    
    /// <summary>
    /// Tries to safely sets a raw value for the given key by doing runtime type-checking, or does nothing. 
    /// </summary>
    /// <param name="key">The key</param>
    /// <param name="value">The value to insert, or null, to remove</param>
    public ConcurrentContextData TrySetSafely(DataKey key, object? value) {
        ((IMutableContextData) this).TrySetSafely(key, value);
        return this;
    }

    /// <summary>
    /// Unsafely sets a raw value for the given key. Care must be taken using this method,
    /// since <see cref="DataKey{T}"/> will throw if it doesn't receive the correct value.
    /// </summary>
    /// <param name="key">The key</param>
    /// <param name="value">The value to insert, or null, to remove</param>
    public ConcurrentContextData SetUnsafe(string key, object? value) {
        ((IMutableContextData) this).SetUnsafe(key, value);
        return this;
    }

    /// <summary>
    /// Removes the value with the given key. This is the same as calling <see cref="SetUnsafe"/> with a null value
    /// </summary>
    /// <param name="key">The key</param>
    public ConcurrentContextData Remove(string key) {
        ((IMutableContextData) this).SetUnsafe(key, null);
        return this;
    }

    /// <summary>
    /// Removes the value by the given key
    /// </summary>
    public ConcurrentContextData Remove(DataKey key) {
        ((IMutableContextData) this).SetUnsafe(key.Id, null);
        return this;
    }

    void IMutableContextData.OnEnterBatchScope() {
    }

    void IMutableContextData.OnExitBatchScope() {
    }

    #endregion

    #region Hidden inteface implementations
    
    // We hide the interface impl so that we can overwrite it with the builder-style version
    void IMutableContextData.SetUnsafe(string key, object? value) {
        if (value == null) {
            this.myMap.TryRemove(key, out _);
        }
        else {
            this.myMap[key] = value;
        }
    }

    #endregion
    
    public bool TryGetContext(string key, [NotNullWhen(true)] out object? value) {
        if (this.myMap.TryGetValue(key, out value))
            return true;
        value = null!;
        return false;
    }

    public bool ContainsKey(string key) => this.myMap.ContainsKey(key);
    
    /// <summary>
    /// Creates a new instance of <see cref="ConcurrentContextData"/> containing all entries from this instance
    /// </summary>
    /// <returns>A new cloned instance</returns>
    public ConcurrentContextData Clone() => new(this);

    public override string ToString() {
        string details = "";
        if (!this.myMap.IsEmpty) {
            details = string.Join(", ", this.myMap.Select(x => "\"" + x.Key + "\"" + "=" + x.Value));
        }

        return "ConcurrentContextData[" + details + "]";
    }
}