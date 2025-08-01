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

using PFXToolKitUI.AdvancedMenuService;

namespace PFXToolKitUI.Avalonia.AdvancedMenuService;

/// <summary>
/// An interface for some sort of object inside an advanced menu that can be connected to and from a context menu entry.
/// An example implementation is <see cref="CaptionSeparator"/> which wants to listen to text changes on its entry
/// </summary>
public interface IAdvancedEntryConnection {
    IContextObject? Entry { get; }

    void OnAdding(IAdvancedMenu menu, IAdvancedMenuOrItem parent, IContextObject entry);
    void OnAdded();
    void OnRemoving();
    void OnRemoved();
}