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

namespace PFXToolKitUI.PropertyEditing;

/// <summary>
/// An enum that represents the visual group type
/// </summary>
public enum GroupType {
    /// <summary>
    /// This is a primary group; it has a big and bold expander
    /// </summary>
    PrimaryExpander,

    /// <summary>
    /// This is a secondary group; it has a small and less obvious expander,
    /// </summary>
    SecondaryExpander,

    /// <summary>
    /// This group has no expander and the contents are always showing (if they are applicable ofc)
    /// </summary>
    NoExpander
}