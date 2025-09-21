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

using Avalonia.Controls;
using PFXToolKitUI.AdvancedMenuService;
using PFXToolKitUI.Avalonia.Shortcuts.Avalonia;
using PFXToolKitUI.Icons;
using PFXToolKitUI.Themes;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.Avalonia.Interactivity.Windowing;

/// <summary>
/// A builder object for building a window
/// </summary>
public sealed class WindowBuilder {
    /// <summary>
    /// Gets or sets the parent of the window
    /// </summary>
    public IWindow? Parent { get; set; }

    /// <summary>
    /// Gets or sets if this window should become a main window when shown. Main windows are stacked,
    /// where the most recent main window shown becomes the single primary main window
    /// </summary>
    public bool MainWindow { get; set; }

    /// <summary>
    /// Gets or sets if the window's title bar should be visible. Default is true. See <see cref="IWindow.IsTitleBarVisible"/> for more info
    /// </summary>
    public bool IsTitleBarVisible { get; set; } = true;

    /// <summary>
    /// Gets or sets the initial title
    /// </summary>
    public string? Title { get; set; }
    
    /// <summary>
    /// Gets or sets the window icon. This cannot use icons produced by the <see cref="IconManager"/> because
    /// window icons can be presented in the OS task bar, other native programs, etc., and this cannot be handled
    /// by icons created by operations such as <see cref="IconManager.RegisterGeometryIcon"/>.
    /// <para>
    /// If instead you want to change the icon shown in the title bar, you can use <see cref="TitleBarIcon"/>
    /// </para>
    /// <para>
    /// When this value is set as <see cref="Optional{T}.Empty"/>, it will use the default icon
    /// that may be provided by the <see cref="IWindowManager"/>.
    /// </para>
    /// </summary>
    public Optional<WindowIcon?> Icon { get; set; }
    
    /// <summary>
    /// Gets or sets the icon used specifically for the left side of the title bar
    /// </summary>
    public Icon? TitleBarIcon { get; set; }
    
    /// <summary>
    /// Gets or sets the title bar background brush
    /// </summary>
    public IColourBrush? TitleBarBrush { get; set; }
    
    /// <summary>
    /// Gets or sets the window border brush. Setting to null will disable the window border
    /// </summary>
    public IColourBrush? BorderBrush { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="UIInputManager.FocusPathProperty"/> of the <see cref="IWindow.Control"/> once created
    /// </summary>
    public string? FocusPath { get; set; }

    /// <summary>
    /// Gets or sets whether to show the title bar icon. If both <see cref="Icon"/> and
    /// <see cref="TitleBarIcon"/> are null, it is equivalent to setting this property to false
    /// </summary>
    public bool ShowTitleBarIcon { get; set; } = true;

    /// <summary>
    /// Gets or sets if this window will be a "tool" window. The definition is different across platforms,
    /// but on windows, usually has a smaller and simplified title bar
    /// </summary>
    public bool IsToolWindow { get; set; }

    /// <summary>
    /// Gets or sets whether the window can be resized by the user
    /// </summary>
    public bool CanResize { get; set; } = true;

    /// <summary>
    /// Gets or sets the window's content
    /// </summary>
    public object? Content { get; set; }
    
    /// <summary>
    /// Gets or sets the initial minimum width of the window
    /// </summary>
    public double? MinWidth { get; set; }

    /// <summary>
    /// Gets or sets the initial minimum height of the window
    /// </summary>
    public double? MinHeight { get; set; }
        
    /// <summary>
    /// Gets or sets the initial maximum width of the window
    /// </summary>
    public double? MaxWidth { get; set; }

    /// <summary>
    /// Gets or sets the initial maximum height of the window
    /// </summary>
    public double? MaxHeight { get; set; }
    
    /// <summary>
    /// Gets or sets the initial width of the window
    /// </summary>
    public double? Width { get; set; }

    /// <summary>
    /// Gets or sets the initial height of the window
    /// </summary>
    public double? Height { get; set; }

    /// <summary>
    /// Gets or sets the size-to-content mode, which tells the window how to automatically size to the content.
    /// This value takes priority over <see cref="WindowSizingInfo.SizeToContent"/> during the window opening process
    /// </summary>
    public SizeToContent SizeToContent { get; set; }

    /// <summary>
    /// Builds the final window. Note, this does not actually show the window.
    /// </summary>
    /// <returns>The created window</returns>
    public IWindow Build(IWindowManager manager) => manager.CreateWindow(this);
}