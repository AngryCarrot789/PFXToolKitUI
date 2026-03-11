// 
// Copyright (c) 2026-2026 REghZy
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
using System.Text;
using PFXToolKitUI.CommandSystem;
using PFXToolKitUI.Shortcuts.Inputs;
using PFXToolKitUI.Shortcuts.Keymapping;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.Shortcuts;

public static class KeymapUtils {
    private const string DefaultDelimiter = ", ", DefaultFinalDelimiter = " or ";

    public static bool TryGetStringForCommandId(string? id, [NotNullWhen(true)] out string? gesture, bool onlyIfCommandExists = true, bool removeDuplicateStrokes = true) {
        if (!string.IsNullOrWhiteSpace(id)) {
            return (gesture = GetStringForCommandId(id, onlyIfCommandExists, removeDuplicateStrokes)) != null;
        }

        gesture = null;
        return false;
    }

    public static string? GetStringForCommandId(string commandId, bool onlyIfCommandExists = true, bool removeDuplicateStrokes = true) {
        ArgumentException.ThrowIfNullOrWhiteSpace(commandId);
        return GetStringForCommandId(commandId, DefaultDelimiter, DefaultFinalDelimiter, onlyIfCommandExists, removeDuplicateStrokes);
    }

    public static string? GetStringForCommandId(string commandId, string shortcutDelimiter, string finalShortcutDelimiter, bool onlyIfCommandExists = true, bool removeDuplicateStrokes = true) {
        ArgumentException.ThrowIfNullOrWhiteSpace(commandId);
        if (onlyIfCommandExists && CommandManager.Instance.GetCommandById(commandId) == null) {
            return null;
        }

        IEnumerable<KeyMapEntry> list = KeyMapManager.Instance.GetShortcutsByCommandId(commandId);
        return GetStringForShortcuts(list, shortcutDelimiter, finalShortcutDelimiter, removeDuplicateStrokes);
    }

    public static string? GetStringForShortcutPath(string path) {
        KeyMapEntry? shortcutEntry = KeyMapManager.Instance.FindShortcutByPath(path);
        return shortcutEntry != null ? GetStringForShortcut(shortcutEntry.Shortcut) : null;
    }

    public static string? GetStringForShortcutPaths(IEnumerable<string> paths, bool removeDuplicateStrokes = true) {
        return GetStringForShortcutPaths(paths, DefaultDelimiter, DefaultFinalDelimiter, removeDuplicateStrokes);
    }

    public static string? GetStringForShortcutPaths(IEnumerable<string> paths, string shortcutDelimiter, string finalShortcutDelimiter, bool removeDuplicateStrokes = true) {
        return GetStringForShortcuts(KeyMapManager.Instance.FindShortcutsByPaths(paths), shortcutDelimiter, finalShortcutDelimiter, removeDuplicateStrokes);
    }

    public static string? GetStringForShortcuts(IEnumerable<KeyMapEntry> shortcuts, bool removeDuplicateStrokes = true) {
        return GetStringForShortcuts(shortcuts, DefaultDelimiter, DefaultFinalDelimiter, removeDuplicateStrokes);
    }

    public static string? GetStringForShortcuts(IEnumerable<KeyMapEntry> shortcutEntries, string shortcutDelimiter, string finalShortcutDelimiter, bool removeDuplicateStrokes = true) {
        return GetStringForShortcuts(shortcutEntries.Select(x => x.Shortcut), shortcutDelimiter, finalShortcutDelimiter, removeDuplicateStrokes);
    }

    public static string? GetStringForShortcuts(IEnumerable<IShortcut> shortcuts, bool removeDuplicateStrokes = true) {
        return GetStringForShortcuts(shortcuts, DefaultDelimiter, DefaultFinalDelimiter, removeDuplicateStrokes);
    }

    public static string? GetStringForShortcuts(IEnumerable<IShortcut> shortcuts, string shortcutDelimiter, string finalShortcutDelimiter, bool removeDuplicateStrokes = true) {
        ArgumentNullException.ThrowIfNull(shortcuts);
        if (removeDuplicateStrokes) {
            HashSet<string> strokes = new HashSet<string>();
            List<string> newList = new List<string>();
            foreach (IShortcut shortcut in shortcuts) {
                string text = GetStringForShortcut(shortcut);
                if (strokes.Add(text)) {
                    newList.Add(text);
                }
            }

            return newList.JoinString(shortcutDelimiter, finalShortcutDelimiter);
        }
        else {
            return shortcuts.Select(GetStringForShortcut).JoinString(shortcutDelimiter, finalShortcutDelimiter);
        }
    }

    public static string GetStringForShortcut(KeyMapEntry entry) {
        return GetStringForShortcut(entry.Shortcut);
    }

    public static string GetStringForShortcut(IShortcut shortcut) {
        return GetStringForInputStrokes(shortcut.InputStrokes);
    }

    public static string GetStringForInputStrokes(IEnumerable<IInputStroke> inputStrokes, string delimiter = " then ") {
        return string.Join(delimiter, inputStrokes.Select(GetStringForInputStroke));
    }

    public static string GetStringForInputStroke(IInputStroke stroke) {
        switch (stroke) {
            case MouseStroke ms: return GetStringForMouseStroke(ms);
            case KeyStroke ks:   return GetStringForKeyStroke(ks, false, false);
            default:             return stroke.ToString() ?? "invalid";
        }
    }

    public static string GetStringForMouseStroke(MouseStroke stroke) {
        StringBuilder sb = new StringBuilder();
        string mods = KeyStroke.ModifierToStringProvider(stroke.Modifiers);
        if (mods.Length > 0) {
            sb.Append(mods).Append('+');
        }

        string name = MouseStroke.MouseButtonToStringProvider(stroke.MouseButton);
        if (stroke.ClickCount > 1) {
            switch (stroke.ClickCount) {
                case 2:  sb.Append("Double ").Append(name); break;
                case 3:  sb.Append("Triple ").Append(name); break;
                case 4:  sb.Append("Quad ").Append(name); break;
                default: sb.Append(stroke.ClickCount).Append("x ").Append(name); break;
            }
        }
        else {
            sb.Append(name);
        }

        return sb.ToString();
    }

    public static string GetStringForKeyStroke(KeyStroke keyStroke, bool appendKeyDown, bool appendKeyUp) {
        StringBuilder sb = new StringBuilder();
        string mods = KeyStroke.ModifierToStringProvider(keyStroke.Modifiers);
        if (mods.Length > 0) {
            sb.Append(mods).Append('+');
        }

        sb.Append(KeyStroke.KeyCodeToStringProvider(keyStroke.KeyCode));
        if (keyStroke.IsRelease) {
            if (appendKeyUp) {
                sb.Append(" (↑)");
            }
        }
        else if (appendKeyDown) {
            sb.Append(" (↓)");
        }

        return sb.ToString();
    }
}