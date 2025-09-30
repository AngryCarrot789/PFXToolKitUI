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
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.Avalonia.Interactivity.Windowing.Overlays;

public delegate void PopupTitleBarInfoTitleChangedEventHandler(OverlayWindowTitleBarInfo sender, string? oldValue, string? newValue);

public delegate void PopupTitleBarInfoIconChangedEventHandler(OverlayWindowTitleBarInfo sender, Icon? oldValue, Icon? newValue);

public delegate void PopupTitleBarInfoIconPlacementChangedEventHandler(OverlayWindowTitleBarInfo sender, TitleBarIconPlacement oldValue, TitleBarIconPlacement newValue);

public delegate void PopupTitleBarInfoTitleBarBrushChangedEventHandler(OverlayWindowTitleBarInfo sender, IColourBrush? oldValue, IColourBrush? newValue);

public delegate void PopupTitleBarInfoTitleBarTextAlignmentChangedEventHandler(OverlayWindowTitleBarInfo sender, TextAlignment? oldValue, TextAlignment? newValue);

/// <summary>
/// A builder object for creating a simple title bar for a <see cref="OverlayWindowBuilder"/>
/// </summary>
public sealed class OverlayWindowTitleBarInfo {
    private string? title;
    private Icon? icon;
    private TitleBarIconPlacement iconPlacement;
    private IColourBrush? titleBarBrush;
    private TextAlignment? titleBarTextAlignment;

    /// <summary>
    /// Gets or sets the title bar text (aka caption)
    /// </summary>
    public string? Title {
        get => this.title;
        set => PropertyHelper.SetAndRaiseINE(ref this.title, value, this, static (t, o, n) => t.TitleChanged?.Invoke(t, o, n));
    }

    /// <summary>
    /// Gets or sets the title bar's icon
    /// </summary>
    public Icon? Icon {
        get => this.icon;
        set => PropertyHelper.SetAndRaiseINE(ref this.icon, value, this, static (t, o, n) => t.IconChanged?.Invoke(t, o, n));
    }

    /// <summary>
    /// Gets or sets the placement of the icon
    /// </summary>
    public TitleBarIconPlacement IconPlacement {
        get => this.iconPlacement;
        set => PropertyHelper.SetAndRaiseINE(ref this.iconPlacement, value, this, static (t, o, n) => t.IconPlacementChanged?.Invoke(t, o, n));
    }

    /// <summary>
    /// Gets or sets the title bar background brush
    /// </summary>
    public IColourBrush? TitleBarBrush {
        get => this.titleBarBrush;
        set => PropertyHelper.SetAndRaiseINE(ref this.titleBarBrush, value, this, static (t, o, n) => t.TitleBarBrushChanged?.Invoke(t, o, n));
    }

    /// <summary>
    /// Gets or sets the text alignment for the title bar text
    /// </summary>
    public TextAlignment? TitleBarTextAlignment {
        get => this.titleBarTextAlignment;
        set => PropertyHelper.SetAndRaiseINE(ref this.titleBarTextAlignment, value, this, static (t, o, n) => t.TitleBarTextAlignmentChanged?.Invoke(t, o, n));
    }

    public event PopupTitleBarInfoTitleChangedEventHandler? TitleChanged;
    public event PopupTitleBarInfoIconChangedEventHandler? IconChanged;
    public event PopupTitleBarInfoIconPlacementChangedEventHandler? IconPlacementChanged;
    public event PopupTitleBarInfoTitleBarBrushChangedEventHandler? TitleBarBrushChanged;
    public event PopupTitleBarInfoTitleBarTextAlignmentChangedEventHandler? TitleBarTextAlignmentChanged;

    public OverlayWindowTitleBarInfo() {
    }

    public OverlayWindowTitleBarInfo(string? title, Icon? icon = null, TitleBarIconPlacement iconPlacement = TitleBarIconPlacement.Left, IColourBrush? titleBarBrush = null) {
        this.title = title;
        this.icon = icon;
        this.iconPlacement = iconPlacement;
        this.titleBarBrush = titleBarBrush;
    }
}

/// <summary>
/// An enum for where to place the title bar's icon
/// </summary>
public enum TitleBarIconPlacement {
    Left,
    Right
}