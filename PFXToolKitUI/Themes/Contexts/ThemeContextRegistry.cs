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

using PFXToolKitUI.AdvancedMenuService;
using PFXToolKitUI.Interactivity.Contexts;
using PFXToolKitUI.Shortcuts;
using PFXToolKitUI.Shortcuts.Inputs;
using PFXToolKitUI.Themes.Configurations;

namespace PFXToolKitUI.Themes.Contexts;

public static class ThemeContextRegistry {
    public static readonly ContextRegistry Registry = new ContextRegistry("Theme Options");
    public static readonly DataKey<IThemeTreeEntry> ThemeTreeEntryKey = DataKeys.Create<IThemeTreeEntry>("IThemeTreeEntry");
    public static readonly DataKey<ThemeConfigurationPage> ThemeConfigurationPageKey = DataKeys.Create<ThemeConfigurationPage>("ThemeConfigurationPage");

    static ThemeContextRegistry() {
        Registry.GetFixedGroup("root").AddCommand("commands.themes.ShowKeysInheritingFromThemeCommand", "Show Inherited By", "Shows a list of theme keys that currently inherit from this");
    }

    private class DeleteInputStrokeEntry : CustomContextEntry {
        public ShortcutEntry Entry { get; }

        public IInputStroke Stroke { get; }

        public DeleteInputStrokeEntry(IInputStroke stroke, ShortcutEntry entry, string displayName, string? description) : base(displayName, description) {
            this.Stroke = stroke;
            this.Entry = entry;
        }

        public override Task OnExecute(IContextData context) {
            switch (this.Entry.Shortcut) {
                case KeyboardShortcut ks: {
                    List<KeyStroke> list = ks.KeyStrokes.ToList();
                    list.Remove((KeyStroke) this.Stroke);
                    this.Entry.Shortcut = new KeyboardShortcut(list);
                    break;
                }
                case MouseShortcut ks: {
                    List<MouseStroke> list = ks.MouseStrokes.ToList();
                    list.Remove((MouseStroke) this.Stroke);
                    this.Entry.Shortcut = new MouseShortcut(list);
                    break;
                }
                case MouseKeyboardShortcut ks: {
                    List<IInputStroke> list = ks.InputStrokes.ToList();
                    list.Remove(this.Stroke);
                    this.Entry.Shortcut = new MouseKeyboardShortcut(list);
                    break;
                }
            }

            return Task.CompletedTask;
        }
    }
}