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

using Avalonia.Controls;
using PFXToolKitUI.AdvancedMenuService;
using PFXToolKitUI.Interactivity.Contexts;

namespace PFXToolKitUI.Avalonia.AdvancedMenuService;

/// <summary>
/// An 
/// </summary>
public interface IAdvancedMenu : IAdvancedMenuOrItem {
    /// <summary>
    /// Gets the captured context data 
    /// </summary>
    IContextData? CapturedContext { get; }

    /// <summary>
    /// Returns the current instance
    /// </summary>
    IAdvancedMenu IAdvancedMenuOrItem.OwnerMenu => this;

    bool PushCachedItem(Type entryType, Control element);

    Control? PopCachedItem(Type entryType);

    /// <summary>
    /// Creates or gets a cached item that can represent the given menu entry
    /// </summary>
    Control CreateItem(IMenuEntry entry) => AdvancedMenuHelper.CreateItem(this, entry);
}