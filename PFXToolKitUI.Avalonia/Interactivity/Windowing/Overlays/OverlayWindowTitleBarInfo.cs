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

using Avalonia.Media;
using PFXToolKitUI.Icons;
using PFXToolKitUI.Themes;
using PFXToolKitUI.Utils.Events;

namespace PFXToolKitUI.Avalonia.Interactivity.Windowing.Overlays;

/// <summary>
/// A builder object for creating a simple title bar for a <see cref="OverlayWindowBuilder"/>
/// </summary>
public sealed class OverlayWindowTitleBarInfo {
    /// <summary>
    /// Gets or sets the title bar text (aka caption)
    /// </summary>
    public string? Title {
        get => field;
        set => PropertyHelper.SetAndRaiseINE(ref field, value, this, this.TitleChanged);
    }

    /// <summary>
    /// Gets or sets the title bar's icon
    /// </summary>
    public Icon? Icon {
        get => field;
        set => PropertyHelper.SetAndRaiseINE(ref field, value, this, this.IconChanged);
    }

    /// <summary>
    /// Gets or sets the placement of the icon
    /// </summary>
    public TitleBarIconPlacement IconPlacement {
        get => field;
        set => PropertyHelper.SetAndRaiseINE(ref field, value, this, this.IconPlacementChanged);
    }

    /// <summary>
    /// Gets or sets the title bar background brush
    /// </summary>
    public IColourBrush? TitleBarBrush {
        get => field;
        set => PropertyHelper.SetAndRaiseINE(ref field, value, this, this.TitleBarBrushChanged);
    }

    /// <summary>
    /// Gets or sets the text alignment for the title bar text
    /// </summary>
    public TextAlignment? TitleBarTextAlignment {
        get => field;
        set => PropertyHelper.SetAndRaiseINE(ref field, value, this, this.TitleBarTextAlignmentChanged);
    }

    public event EventHandler<ValueChangedEventArgs<string?>>? TitleChanged;
    public event EventHandler<ValueChangedEventArgs<Icon?>>? IconChanged;
    public event EventHandler<ValueChangedEventArgs<TitleBarIconPlacement>>? IconPlacementChanged;
    public event EventHandler<ValueChangedEventArgs<IColourBrush?>>? TitleBarBrushChanged;
    public event EventHandler<ValueChangedEventArgs<TextAlignment?>>? TitleBarTextAlignmentChanged;

    public OverlayWindowTitleBarInfo() {
    }

    public OverlayWindowTitleBarInfo(string? title, Icon? icon = null, TitleBarIconPlacement iconPlacement = TitleBarIconPlacement.Left, IColourBrush? titleBarBrush = null) {
        this.Title = title;
        this.Icon = icon;
        this.IconPlacement = iconPlacement;
        this.TitleBarBrush = titleBarBrush;
    }
}

/// <summary>
/// An enum for where to place the title bar's icon
/// </summary>
public enum TitleBarIconPlacement {
    Left,
    Right
}