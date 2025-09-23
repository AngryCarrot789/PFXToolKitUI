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
/// An implementation of <see cref="IMutableContextData"/> that stores entries in a dictionary
/// </summary>
public sealed class ContextData : IMutableContextData {
    private Dictionary<string, object>? map;

    /// <summary>
    /// The number of entries in our internal map
    /// </summary>
    public int Count => this.map?.Count ?? 0;

    public IEnumerable<KeyValuePair<string, object>> Entries => this.map ?? EmptyContext.EmptyDictionary;

    /// <summary>
    /// Creates a new empty context data instance
    /// </summary>
    public ContextData() {
    }

    /// <summary>
    /// Copy constructor, effectively the same as <see cref="Clone"/>
    /// </summary>
    /// <param name="context">The context to copy from</param>
    public ContextData(ContextData context) {
        if (context.map != null && context.map.Count > 0)
            this.map = new Dictionary<string, object>(context.map);
    }

    /// <summary>
    /// Adds all entries from the given context to this instance
    /// </summary>
    /// <param name="context">The context to copy from</param>
    public ContextData(IContextData context) {
        this.AddAll(context);
    }
    
    /// <summary>
    /// Adds all entries from the given context to this instance
    /// </summary>
    /// <param name="context">The context to copy the entires from</param>
    public void AddAll(IContextData context) {
        if (context is ContextData cd && cd.map != null) {
            using Dictionary<string, object>.Enumerator enumerator = cd.map.GetEnumerator();
            if (enumerator.MoveNext()) {
                Dictionary<string, object> myMap = this.map ??= new Dictionary<string, object>();
                do {
                    KeyValuePair<string, object> entry = enumerator.Current;
                    myMap[entry.Key] = entry.Value;
                } while (enumerator.MoveNext());
            }
        }
        else if (!(context is EmptyContext)) {
            using IEnumerator<KeyValuePair<string, object>> enumerator = context.Entries.GetEnumerator();
            if (enumerator.MoveNext()) {
                Dictionary<string, object> myMap = this.map ??= new Dictionary<string, object>();
                do {
                    KeyValuePair<string, object> entry = enumerator.Current;
                    myMap[entry.Key] = entry.Value;
                } while (enumerator.MoveNext());
            }
        }
    }
    
    #region New Overrides, for builder-styled syntax

    /// <summary>
    /// Sets a value with the given data key.
    /// </summary>
    /// <param name="key">The key</param>
    /// <param name="value">The value to insert</param>
    public ContextData Set<T>(DataKey<T> key, T? value) {
        ((IMutableContextData) this).SetUnsafe(key.Id, value);
        return this;
    }

    /// <summary>
    /// Safely sets a raw value for the given key by doing runtime type-checking. 
    /// </summary>
    /// <param name="key">The key</param>
    /// <param name="value">The value to insert, or null, to remove</param>
    public ContextData SetSafely(DataKey key, object? value) {
        ((IMutableContextData) this).SetSafely(key, value);
        return this;
    }

    /// <summary>
    /// Unsafely sets a raw value for the given key. Care must be taken using this method,
    /// since <see cref="DataKey{T}"/> will throw if it doesn't receive the correct value.
    /// </summary>
    /// <param name="key">The key</param>
    /// <param name="value">The value to insert, or null, to remove</param>
    public ContextData SetUnsafe(string key, object? value) {
        ((IMutableContextData) this).SetUnsafe(key, value);
        return this;
    }

    /// <summary>
    /// Removes the value with the given key. This is the same as calling <see cref="SetUnsafe"/> with a null value
    /// </summary>
    /// <param name="key">The key</param>
    public ContextData Remove(string key) {
        ((IMutableContextData) this).SetUnsafe(key, null);
        return this;
    }

    /// <summary>
    /// Removes the value by the given key
    /// </summary>
    public ContextData Remove(DataKey key) {
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
            this.map?.Remove(key);
        }
        else {
            (this.map ??= new Dictionary<string, object>())[key] = value;
        }
    }

    #endregion
    
    public bool TryGetContext(string key, out object value) {
        if (this.map != null && this.map.TryGetValue(key, out value!))
            return true;
        value = null!;
        return false;
    }

    public bool ContainsKey(string key) => this.map != null && this.map.ContainsKey(key);

    
    public ContextData ToMutable() => this.Clone();
    
    /// <summary>
    /// Creates a new instance of <see cref="ContextData"/> containing all entries from this instance
    /// </summary>
    /// <returns>A new cloned instance</returns>
    public ContextData Clone() {
        ContextData ctx = new ContextData();
        if (this.map != null && this.map.Count > 0)
            ctx.map = new Dictionary<string, object>(this.map);
        return ctx;
    }

    public override string ToString() {
        string details = "";
        if (this.map != null && this.map.Count > 0) {
            details = string.Join(", ", this.map.Select(x => "\"" + x.Key + "\"" + "=" + x.Value));
        }

        return "ContextData[" + details + "]";
    }
}