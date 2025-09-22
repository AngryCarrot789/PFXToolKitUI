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

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace PFXToolKitUI.Interactivity.Contexts;

/// <summary>
/// Facilitates creating and accessing data keys
/// </summary>
public static class DataKeys {
    private static readonly Dictionary<string, DataKey> Registry = new Dictionary<string, DataKey>();

    /// <summary>
    /// Creates and registers a data key with the given ID
    /// </summary>
    /// <param name="id">The data key's id</param>
    /// <returns>The created data key</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static DataKey<T> Create<T>(string id) {
        lock (Registry) {
            if (Registry.ContainsKey(id))
                throw new InvalidOperationException("ID already in use: " + id);
            
            DataKey<T> key = new DataKey<T>(id);
            Registry[id] = key;
            return key;
        }
    }

    /// <summary>
    /// Gets the data key from its identifier
    /// </summary>
    /// <param name="id">The key's id</param>
    /// <returns>The data key</returns>
    public static DataKey? GetKeyById(string id) {
        lock (Registry)
            return Registry.GetValueOrDefault(id);
    }
}

/// <summary>
/// Represents an identifiable piece of data of a specific type
/// </summary>
public abstract class DataKey {
    /// <summary>
    /// A unique identifier for this data key
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the type of data this key is used to access
    /// </summary>
    public abstract Type DataType { get; }

    internal DataKey(string id) {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        this.Id = id;
    }
    
    /// <summary>
    /// Checks if the context data contains this key
    /// </summary>
    /// <param name="contextData">The context data</param>
    /// <returns>True if the context contains this key</returns>
    public bool IsPresent(IContextData contextData) => contextData.ContainsKey(this.Id);

    public sealed override bool Equals(object? obj) {
        if (!ReferenceEquals(this, obj)) {
            return false;
        }

        Debug.Assert(obj is DataKey key && key.Id == this.Id && key.DataType == this.DataType);
        return true;
    }

    public sealed override int GetHashCode() => this.Id.GetHashCode();

    public sealed override string ToString() => $"DataKey(\"{this.Id}\", type = {this.DataType.Name})";
}

public sealed class DataKey<T> : DataKey {
    public override Type DataType => typeof(T);
    
    internal DataKey(string id) : base(id) {
    }

    /// <summary>
    /// Tries to get the value of this data key from the context
    /// </summary>
    /// <param name="context">The context to get the value from</param>
    /// <param name="value">The value, or default</param>
    /// <returns>True if the key exists in the context, otherwise False</returns>
    /// <remarks>
    /// If the context contains an invalid object for this data key (i.e. <see cref="IMutableContextData.SetUnsafe"/> was used),
    /// then this method returns false anyway and the <see cref="value"/> will still be null
    /// </remarks>
    public bool TryGetContext(IContextData context, [NotNullWhen(true)] out T? value) {
        ArgumentNullException.ThrowIfNull(context);
        if (context.TryGetContext(this.Id, out object? obj) && obj is T val) {
            value = val;
            return true;
        }
        else {
            if (obj != null) {
                Debug.WriteLine($"Context contained an invalid value for this key: type mismatch ({typeof(T)} != {obj.GetType()})");
            }

            value = default;
            return false;
        }
    }

    /// <summary>
    /// Gets the value of this data key from the context data, or returns <see cref="def"/> if not present.
    /// </summary>
    /// <param name="context">The context to get the value from</param>
    /// <param name="def">The default value if the context does not contain this key</param>
    /// <returns>The value in the context, or <c>default(T)</c></returns>
    /// <remarks>
    /// If the context contains an invalid object for this data key (i.e. <see cref="IMutableContextData.SetUnsafe"/> was used),
    /// then this method returns the <see cref="def"/> parameter anyway
    /// </remarks>
    [return: NotNullIfNotNull("def")]
    public T? GetContext(IContextData context, T? def = default) {
        return this.TryGetContext(context, out T? value) ? value : def;
    }
}