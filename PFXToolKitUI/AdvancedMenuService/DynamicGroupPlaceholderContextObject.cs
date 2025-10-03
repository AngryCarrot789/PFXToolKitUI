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

namespace PFXToolKitUI.AdvancedMenuService;

/// <summary>
/// This context object is an invisible placeholder which is where dynamic items will be inserted after
/// </summary>
public class DynamicGroupPlaceholderContextObject : IContextObject {
    /// <summary>
    /// Gets the dynamic context group we used to generate entries on demand
    /// </summary>
    public DynamicContextGroup DynamicGroup { get; }
    
    public DynamicGroupPlaceholderContextObject(DynamicContextGroup dynamicGroup) {
        this.DynamicGroup = dynamicGroup;
    }
}