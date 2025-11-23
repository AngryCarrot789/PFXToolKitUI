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

namespace PFXToolKitUI.Themes;

/// <summary>
/// A brush whose underlying brush updates dynamically when modifying via the theme in the application preferences, or by the theme changing
/// </summary>
public interface IDynamicColourBrush : IColourBrush {
    /// <summary>
    /// Gets the key that locates the "brush" contents
    /// </summary>
    string ThemeKey { get; }

    /// <summary>
    /// Gets the number of references using this dynamic colour brush.
    /// </summary>
    int ReferenceCount { get; }

    /// <summary>
    /// An event fired when the underlying brush content change (typically caused
    /// by the application theme changing, or maybe the user modified the specific brush).
    /// <para>
    /// This will only be fired when the UI has subscribed to changes,
    /// since listening to resource changes is expensive
    /// </para>
    /// </summary>
    event EventHandler? BrushChanged;
}