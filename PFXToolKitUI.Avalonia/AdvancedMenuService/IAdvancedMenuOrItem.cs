// 
// Copyright (c) 2023-2025 REghZy
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

using Avalonia.Controls;
using PFXToolKitUI.AdvancedMenuService;

namespace PFXToolKitUI.Avalonia.AdvancedMenuService;

/// <summary>
/// An interface for either an advanced menu or an advanced menu item
/// </summary>
public interface IAdvancedMenuOrItem {
    /// <summary>
    /// Gets the advanced menu that owns this menu item. Returns the current instance if we are an <see cref="IAdvancedMenu"/>
    /// </summary>
    IAdvancedMenu? OwnerMenu { get; }

    /// <summary>
    /// Gets this control's items collection
    /// </summary>
    ItemCollection Items { get; }

    /// <summary>
    /// Returns true when this menu or item is open. When true it means we can generate dynamic items using <see cref="IAdvancedMenu.CapturedContext"/>
    /// </summary>
    bool IsOpen { get; }

    /// <summary>
    /// Stores the dynamic group for insertion at the given index inside this element's item
    /// list. Post-processing must be done on this index during generation
    /// </summary>
    /// <param name="groupPlaceholder">The dynamic group</param>
    /// <param name="index">The unprocessed index</param>
    void StoreDynamicGroup(DynamicGroupPlaceholderContextObject groupPlaceholder, int index);
}