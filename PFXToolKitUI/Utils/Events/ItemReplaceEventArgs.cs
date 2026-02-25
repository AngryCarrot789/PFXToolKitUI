// 
// Copyright (c) 2026-2026 REghZy
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
/// Event args for an item being replaced with another item
/// </summary>
/// <typeparam name="T">The type of item</typeparam>
public readonly struct ItemReplaceEventArgs<T>(int index, T oldItem, T newItem) {
    /// <summary>
    /// The index of <see cref="OldItem"/>
    /// </summary>
    public int Index { get; } = index;

    /// <summary>
    /// The old/existing item
    /// </summary>
    public T OldItem { get; } = oldItem;

    /// <summary>
    /// The new item
    /// </summary>
    public T NewItem { get; } = newItem;
}