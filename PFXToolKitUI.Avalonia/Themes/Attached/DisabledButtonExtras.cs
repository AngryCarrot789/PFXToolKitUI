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

public static class DisabledButtonExtras {
    public static readonly AttachedProperty<bool> IgnoreDisabledBackgroundProperty = AvaloniaProperty.RegisterAttached<Control, bool>("IgnoreDisabledBackground", typeof(TabControlStripExtras));

    /// <summary>
    /// Sets whether to leave the background unaffected when a button is disabled
    /// </summary>
    public static void SetIgnoreDisabledBackground(Control obj, bool value) => obj.SetValue(IgnoreDisabledBackgroundProperty, value);

    /// <summary>
    /// Gets whether to leave the background unaffected when a button is disabled
    /// </summary>
    public static bool GetIgnoreDisabledBackground(Control obj) => obj.GetValue(IgnoreDisabledBackgroundProperty);
}