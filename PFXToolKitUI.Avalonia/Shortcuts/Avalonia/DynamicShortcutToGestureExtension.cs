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

using System.Runtime.CompilerServices;
using PFXToolKitUI.Avalonia.AvControls;
using PFXToolKitUI.Avalonia.Shortcuts.Converters;

namespace PFXToolKitUI.Avalonia.Shortcuts.Avalonia;

public class DynamicShortcutToGestureExtension {
    public DynamicShortcutToGestureExtension() {
    }

    public DynamicShortcutToGestureExtension(string shortcutId) => this.ShortcutId = shortcutId;

    public string? ShortcutId { get; set; }

    public object? ProvideValue(IServiceProvider serviceProvider) {
        return ProvideValue(serviceProvider, this.ShortcutId);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static object? ProvideValue(IServiceProvider serviceProvider, object? resourceKey) {
        if (resourceKey == null)
            throw new ArgumentException("DynamicShortcutsExtension.ResourceKey must be set.");

        if (ShortcutIdToGestureConverter.ShortcutIdToGesture(resourceKey.ToString() ?? "", null, out string? gesture))
            return gesture;

        return ShortcutLabel.NoShortcutTextProperty.GetDefaultValue(typeof(ShortcutLabel));
    }
}