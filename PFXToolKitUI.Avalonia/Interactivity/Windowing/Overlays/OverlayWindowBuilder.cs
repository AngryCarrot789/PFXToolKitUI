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
using PFXToolKitUI.Themes;

namespace PFXToolKitUI.Avalonia.Interactivity.Windowing.Overlays;

/// <summary>
/// A builder object for building a popup for a single-view popup manager
/// </summary>
public sealed class OverlayWindowBuilder {
    /// <summary>
    /// Gets or sets the parent popup
    /// </summary>
    public IOverlayWindow? Parent { get; set; }

    /// <summary>
    /// Gets or sets the title bar builder object, which describes the information for a standard title bar. When set to null, no title bar is present 
    /// </summary>
    public OverlayWindowTitleBarInfo? TitleBar { get; set; }

    /// <summary>
    /// Gets or sets the border brush for the popup. When null, no border is shown.
    /// </summary>
    public IColourBrush? BorderBrush { get; set; }

    /// <summary>
    /// Gets or sets the popup's content
    /// </summary>
    public object? Content { get; set; }

    /// <summary>
    /// Gets or sets whether to automatically close the popup when the user clicks outside of its visual bounds
    /// </summary>
    public bool CloseOnLostFocus { get; set; }

    /// <summary>
    /// Gets the minimum width suggestion for the overlay window
    /// </summary>
    public double? MinWidth { get; set; }

    /// <summary>
    /// Gets the minimum height suggestion for the overlay window
    /// </summary>
    public double? MinHeight { get; set; }

    /// <summary>
    /// Gets the maximum width suggestion for the overlay window
    /// </summary>
    public double? MaxWidth { get; set; }

    /// <summary>
    /// Gets the maximum height suggestion for the overlay window
    /// </summary>
    public double? MaxHeight { get; set; }

    /// <summary>
    /// Gets the maximum width suggestion for the overlay window
    /// </summary>
    public double? Width { get; set; }

    /// <summary>
    /// Gets the maximum height suggestion for the overlay window
    /// </summary>
    public double? Height { get; set; }

    /// <summary>
    /// Gets or sets the size to content mode. Default is <see cref="Avalonia.Controls.SizeToContent.Manual"/>
    /// </summary>
    public SizeToContent SizeToContent { get; set; }
}