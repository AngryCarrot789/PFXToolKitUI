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

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace PFXToolKitUI.Avalonia.Themes.Controls;

public class WindowTitleBar : TemplatedControl {
    public static readonly StyledProperty<WindowIcon?> IconProperty = Window.IconProperty.AddOwner<WindowTitleBar>();
    public static readonly StyledProperty<string?> TitleProperty = Window.TitleProperty.AddOwner<WindowTitleBar>();
    public static readonly StyledProperty<bool> IsMinimizedProperty = AvaloniaProperty.Register<WindowTitleBar, bool>("IsMinimized");

    public WindowIcon? Icon {
        get => this.GetValue(IconProperty);
        set => this.SetValue(IconProperty, value);
    }

    public string? Title {
        get => this.GetValue(TitleProperty);
        set => this.SetValue(TitleProperty, value);
    }

    public bool IsMinimized {
        get => this.GetValue(IsMinimizedProperty);
        set => this.SetValue(IsMinimizedProperty, value);
    }

    public WindowTitleBar() {
    }
}