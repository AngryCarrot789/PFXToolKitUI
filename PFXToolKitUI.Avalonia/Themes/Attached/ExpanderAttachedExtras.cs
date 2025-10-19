// 
// Copyright (c) 2025-2025 REghZy
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

public static class ExpanderAttachedExtras {
    public static readonly AttachedProperty<Thickness> ToggleButtonMarginProperty = AvaloniaProperty.RegisterAttached<Expander, Thickness>("ToggleButtonMargin", typeof(ExpanderAttachedExtras));

    public static void SetToggleButtonMargin(Expander obj, Thickness value) => obj.SetValue(ToggleButtonMarginProperty, value);
    public static Thickness GetToggleButtonMargin(Expander obj) => obj.GetValue(ToggleButtonMarginProperty);
}