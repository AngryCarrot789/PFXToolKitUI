// 
// Copyright (c) 2024-2025 REghZy
// 
// This file is part of FramePFX.
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
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
// 

using System.Collections.ObjectModel;

namespace PFXToolKitUI.Interactivity.Contexts;

/// <summary>
/// An implementation of <see cref="IContextData"/> that is completely empty
/// </summary>
public sealed class EmptyContext : IContextData {
    public static readonly IReadOnlyDictionary<string, object> EmptyDictionary = new ReadOnlyDictionary<string, object>(new Dictionary<string, object>());

    /// <summary>
    /// Returns a singleton instance of this empty context
    /// </summary>
    public static IContextData Instance { get; } = new EmptyContext();

    IEnumerable<KeyValuePair<string, object>> IContextData.Entries => EmptyDictionary;

    public EmptyContext() { }

    bool IContextData.TryGetContext(string key, out object value) {
        value = default;
        return false;
    }

    bool IContextData.ContainsKey(string key) => false;
}