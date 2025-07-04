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

using System.Globalization;
using Avalonia.Data.Converters;
using PFXToolKitUI.Shortcuts;

namespace PFXToolKitUI.Avalonia.Shortcuts.Converters;

public class ShortcutPathToInputGestureTextConverter : IValueConverter {
    public string NoSuchShortcutFormat { get; set; } = "<{0}>";

    public string? ShortcutFormat { get; set; } = null;

    public static string ShortcutToInputGestureText(string path, string? shortcutFormat = null, string? noSuchShortcutFormat = null) {
        ShortcutEntry? shortcutEntry = ShortcutManager.Instance.FindShortcutByPath(path);
        if (shortcutEntry == null) {
            return noSuchShortcutFormat == null ? path : string.Format(noSuchShortcutFormat, path);
        }

        string representation = shortcutEntry.Shortcut.ToString() ?? "";
        return shortcutFormat == null ? representation : string.Format(shortcutFormat, representation);
    }

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (parameter is string path && !string.IsNullOrWhiteSpace(path)) {
            return ShortcutToInputGestureText(path, this.ShortcutFormat, this.NoSuchShortcutFormat);
        }
        else {
            throw new Exception("Invalid shortcut path (converter parameter): " + parameter);
        }
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }
}