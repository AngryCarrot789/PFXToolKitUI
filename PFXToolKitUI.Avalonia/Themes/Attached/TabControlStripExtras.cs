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

namespace PFXToolKitUI.Avalonia.Themes.Attached;

public static class TabControlStripExtras {
    public static readonly AttachedProperty<object?> LeftContentProperty = AvaloniaProperty.RegisterAttached<Control, object?>("LeftContent", typeof(TabControlStripExtras));
    public static readonly AttachedProperty<object?> RightContentProperty = AvaloniaProperty.RegisterAttached<Control, object?>("RightContent", typeof(TabControlStripExtras));

    public static void SetLeftContent(Control obj, object? value) => obj.SetValue(LeftContentProperty, value);
    public static object? GetLeftContent(Control obj) => obj.GetValue(LeftContentProperty);
    
    public static void SetRightContent(Control obj, object? value) => obj.SetValue(RightContentProperty, value);
    public static object? GetRightContent(Control obj) => obj.GetValue(RightContentProperty);
}