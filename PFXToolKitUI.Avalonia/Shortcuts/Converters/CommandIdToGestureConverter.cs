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
using Avalonia;
using Avalonia.Data.Converters;
using PFXToolKitUI.CommandSystem;
using PFXToolKitUI.Shortcuts;

namespace PFXToolKitUI.Avalonia.Shortcuts.Converters;

public class CommandIdToGestureConverter : IValueConverter {
    public static CommandIdToGestureConverter Instance { get; } = new CommandIdToGestureConverter();

    public string NoSuchActionText { get; set; } = null;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is string id) {
            return CommandIdToGesture(id, this.NoSuchActionText, out string gesture) ? gesture : AvaloniaProperty.UnsetValue;
        }

        throw new Exception("Value is not a string");
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }

    public static bool CommandIdToGesture(string id, string fallback, out string gesture) {
        if (CommandManager.Instance.GetCommandById(id) == null) {
            return (gesture = fallback) != null;
        }

        IEnumerable<ShortcutEntry>? shortcuts = ShortcutManager.Instance.GetShortcutsByCommandId(id);
        if (shortcuts == null) {
            return (gesture = fallback) != null;
        }

        return (gesture = ShortcutIdToGestureConverter.ShortcutsToGesture(shortcuts, fallback)) != null;
    }
}