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

using System.Diagnostics.CodeAnalysis;

namespace PFXToolKitUI.Interactivity.Contexts;

/// <summary>
/// An object that stores context information. Any entry will always have a non-null value (null values are not permitted)
/// </summary>
public interface IContextData {
    /// <summary>
    /// Returns an enumerable that allows iteration of all entries in this object
    /// </summary>
    IEnumerable<KeyValuePair<string, object>> Entries { get; }

    /// <summary>
    /// Tries to get a value from a data key
    /// </summary>
    bool TryGetContext(string key, [NotNullWhen(true)] out object? value);

    /// <summary>
    /// Checks if the given data key is contained in this context
    /// </summary>
    bool ContainsKey(string key);
}