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
/// Event args for an item inserted or removed event
/// </summary>
/// <typeparam name="T">The type of item</typeparam>
public readonly struct ItemIndexEventArgs<T>(T item, int index) {
    /// <summary>
    /// Gets the item that was inserted or removed
    /// </summary>
    public T Item { get; } = item;

    /// <summary>
    /// Gets the index of the inserted or removed item
    /// </summary>
    public int Index { get; } = index;
}