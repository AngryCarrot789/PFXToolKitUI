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

namespace PFXToolKitUI.Avalonia.Interactivity.Windowing.Popups;

/// <summary>
/// A builder object for building a popup for a single-view popup manager
/// </summary>
public class PopupDialogBuilder {
    /// <summary>
    /// Gets or sets the parent popup
    /// </summary>
    public IPopupDialog? Parent { get; set; }

    /// <summary>
    /// Gets or sets the title bar builder object, which describes the information for a standard title bar. When set to null, no title bar is present 
    /// </summary>
    public PopupTitleBarInfo? TitleBar { get; set; }

    /// <summary>
    /// Gets or sets the popup's content
    /// </summary>
    public object? Content { get; set; }
}