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

namespace PFXToolKitUI.Avalonia.Shortcuts.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class ShortcutTargetAttribute : Attribute {
    public string[] ShortcutPaths { get; }

    public ShortcutTargetAttribute(string shortcutPath) {
        this.ShortcutPaths = string.IsNullOrWhiteSpace(shortcutPath) ? null : new string[] { shortcutPath };
    }

    public ShortcutTargetAttribute(params string[] shortcutPaths) {
        shortcutPaths = shortcutPaths.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
        this.ShortcutPaths = shortcutPaths.Length < 1 ? null : shortcutPaths;
    }
}