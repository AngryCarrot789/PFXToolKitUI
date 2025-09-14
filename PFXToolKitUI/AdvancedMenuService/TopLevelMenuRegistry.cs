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

using PFXToolKitUI.Utils.Collections.Observable;

namespace PFXToolKitUI.AdvancedMenuService;

/// <summary>
/// Manages the top-level menu items for a window's main menu
/// </summary>
public class TopLevelMenuRegistry {
    /// <summary>
    /// Gets the menu item entries for this menu
    /// </summary>
    public ObservableList<ContextEntryGroup> Items { get; }

    public TopLevelMenuRegistry() {
        this.Items = new ObservableList<ContextEntryGroup>();
    }
}