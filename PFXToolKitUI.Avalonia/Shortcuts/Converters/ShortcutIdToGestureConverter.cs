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

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using PFXToolKitUI.Shortcuts;
using PFXToolKitUI.Shortcuts.Inputs;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.Avalonia.Shortcuts.Converters;

public class ShortcutIdToGestureConverter : IValueConverter {
    public static ShortcutIdToGestureConverter Instance { get; } = new ShortcutIdToGestureConverter();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is string path) {
            return ShortcutIdToGesture(path, null, out string? gesture) ? gesture : AvaloniaProperty.UnsetValue;
        }
        else if (value is IEnumerable<string> paths) {
            return ShortcutIdToGesture(paths, null, out string? gesture) ? gesture : AvaloniaProperty.UnsetValue;
        }

        throw new Exception("Value is not a shortcut string");
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }

    public static bool ShortcutIdToGesture(string path, string fallback, out string? gesture) {
        ShortcutEntry? shortcutEntry = ShortcutManager.Instance?.FindShortcutByPath(path);
        if (shortcutEntry == null) {
            return (gesture = fallback) != null;
        }

        gesture = shortcutEntry.Shortcut.ToString();
        return true;
    }

    public static bool ShortcutIdToGesture(IEnumerable<string> paths, string? fallback, [NotNullWhen(true)] out string? gesture) {
        List<ShortcutEntry>? shortcut = ShortcutManager.Instance?.FindShortcutsByPaths(paths).ToList();
        if (shortcut == null || shortcut.Count < 1) {
            return (gesture = fallback) != null;
        }

        return (gesture = ShortcutsToGesture(shortcut, null)) != null;
    }

    public static string? ShortcutsToGesture(IEnumerable<ShortcutEntry> shortcuts, string? fallback, bool removeDupliateInputStrokes = true) {
        if (removeDupliateInputStrokes) {
            HashSet<string> strokes = new HashSet<string>();
            List<string> newList = new List<string>();
            foreach (ShortcutEntry sc in shortcuts) {
                string text = ToString(sc);
                if (strokes.Add(text)) {
                    newList.Add(text);
                }
            }

            return newList.JoinString(", ", " or ", fallback);
        }
        else {
            return shortcuts.Select(ToString).JoinString(", ", " or ", fallback);
        }
    }

    public static string ToString(ShortcutEntry shortcutEntry) {
        return string.Join("+", shortcutEntry.Shortcut.InputStrokes.Select(ToString));
    }

    public static string ToString(IInputStroke stroke) {
        if (stroke is MouseStroke ms) {
            return MouseStrokeStringConverter.ToStringFunction(ms.MouseButton, ms.Modifiers, ms.ClickCount);
        }
        else if (stroke is KeyStroke ks) {
            return KeyStrokeStringConverter.ToStringFunction(ks.KeyCode, ks.Modifiers, ks.IsRelease, false, true);
        }
        else {
            return stroke?.ToString() ?? "";
        }
    }
}