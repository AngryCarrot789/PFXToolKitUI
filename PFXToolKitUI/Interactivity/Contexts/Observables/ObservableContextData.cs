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

namespace PFXToolKitUI.Interactivity.Contexts.Observables;

/// <summary>
/// An implementation of <see cref="IMutableContextData"/> that stores entries in a dictionary
/// </summary>
public sealed class ObservableContextData : BaseObservableContextData {
    private Dictionary<string, object>? internalDictionary;

    /// <summary>
    /// The number of entries in our internal map
    /// </summary>
    public override int Count => this.internalDictionary?.Count ?? 0;

    public override IEnumerable<KeyValuePair<string, object>> Entries => this.internalDictionary ?? EmptyContext.EmptyDictionary;

    protected override IDictionary<string, object> InternalDictionary => this.internalDictionary ??= new Dictionary<string, object>();

    /// <summary>
    /// Creates a new empty context data instance
    /// </summary>
    public ObservableContextData() {
    }

    /// <summary>
    /// Copy constructor, effectively the same as <see cref="Clone"/>
    /// </summary>
    /// <param name="context">The context to copy from</param>
    public ObservableContextData(ObservableContextData context) {
        if (context.internalDictionary != null && context.internalDictionary.Count > 0)
            this.internalDictionary = new Dictionary<string, object>(context.internalDictionary);
    }

    /// <summary>
    /// Adds all entries from the given context to this instance
    /// </summary>
    /// <param name="context">The context to copy from</param>
    public ObservableContextData(IContextData context) {
        this.AddAll(context);
    }

    /// <summary>
    /// Adds all entries from the given context to this instance
    /// </summary>
    /// <param name="context">The context to copy the entires from</param>
    public ObservableContextData AddAll(IContextData context) {
        if (context is IRandomAccessContextData racd && racd.Count < 1) {
            return this;
        }
        
        using IEnumerator<KeyValuePair<string, object>> enumerator = context.Entries.GetEnumerator();
        if (enumerator.MoveNext()) {
            using (((IMutableContextData) this).BeginChange()) {
                KeyValuePair<string, object> entry = enumerator.Current;
                this.SetUnsafe(entry.Key, entry.Value);
            }
        }

        return this;
    }

    #region New Overrides, for builder-styled syntax

    /// <summary>
    /// Sets a value with the given data key.
    /// </summary>
    /// <param name="key">The key</param>
    /// <param name="value">The value to insert</param>
    public ObservableContextData Set<T>(DataKey<T> key, T? value) {
        ((IMutableContextData) this).SetUnsafe(key.Id, value);
        return this;
    }

    /// <summary>
    /// Safely sets a raw value for the given key by doing runtime type-checking. 
    /// </summary>
    /// <param name="key">The key</param>
    /// <param name="value">The value to insert, or null, to remove</param>
    public ObservableContextData SetSafely(DataKey key, object? value) {
        ((IMutableContextData) this).SetSafely(key, value);
        return this;
    }

    /// <summary>
    /// Tries to safely sets a raw value for the given key by doing runtime type-checking, or does nothing. 
    /// </summary>
    /// <param name="key">The key</param>
    /// <param name="value">The value to insert, or null, to remove</param>
    public ObservableContextData TrySetSafely(DataKey key, object? value) {
        ((IMutableContextData) this).TrySetSafely(key, value);
        return this;
    }

    /// <summary>
    /// Unsafely sets a raw value for the given key. Care must be taken using this method,
    /// since <see cref="DataKey{T}"/> will throw if it doesn't receive the correct value.
    /// </summary>
    /// <param name="key">The key</param>
    /// <param name="value">The value to insert, or null, to remove</param>
    public ObservableContextData SetUnsafe(string key, object? value) {
        ((IMutableContextData) this).SetUnsafe(key, value);
        return this;
    }

    /// <summary>
    /// Removes the value with the given key. This is the same as calling <see cref="SetUnsafe"/> with a null value
    /// </summary>
    /// <param name="key">The key</param>
    public ObservableContextData Remove(string key) {
        ((IMutableContextData) this).SetUnsafe(key, null);
        return this;
    }

    /// <summary>
    /// Removes the value by the given key
    /// </summary>
    public ObservableContextData Remove(DataKey key) {
        ((IMutableContextData) this).SetUnsafe(key.Id, null);
        return this;
    }

    #endregion

    /// <summary>
    /// Creates a new instance of <see cref="ContextData"/> containing all entries from this instance
    /// </summary>
    /// <returns>A new cloned instance</returns>
    public ObservableContextData Clone() {
        ObservableContextData ctx = new ObservableContextData();
        if (this.internalDictionary != null && this.internalDictionary.Count > 0)
            ctx.internalDictionary = new Dictionary<string, object>(this.internalDictionary);
        return ctx;
    }
}