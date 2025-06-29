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

using PFXToolKitUI.Shortcuts;

namespace PFXToolKitUI.Avalonia.Shortcuts.Trees;

/// <summary>
/// An interface for shared properties between a <see cref="ShortcutTreeView"/> and <see cref="ShortcutTreeViewItem"/>
/// </summary>
public interface IShortcutTreeOrNode {
    /// <summary>
    /// Gets the configuration tree view associated with this node, or returns the current instance
    /// </summary>
    ShortcutTreeView? ShortcutTree { get; }

    /// <summary>
    /// Gets the parent node, or null if we are a root node or a tree
    /// </summary>
    ShortcutTreeViewItem? ParentNode { get; }

    /// <summary>
    /// Gets this node's entry, or returns the "root" configuration entry which contains all the root level entries
    /// </summary>
    IKeyMapEntry? Entry { get; }

    /// <summary>
    /// Gets the node at the specific UI index
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    ShortcutTreeViewItem GetNodeAt(int index);

    void InsertGroup(ShortcutGroupEntry entry, int index);

    void InsertInputState(InputStateEntry entry, int index);

    void InsertShortcut(ShortcutEntry entry, int index);

    void RemoveGroup(int index, bool canCache = true);

    void RemoveInputState(int index, bool canCache = true);

    void RemoveShortcut(int index, bool canCache = true);
}