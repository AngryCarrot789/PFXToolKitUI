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

namespace PFXToolKitUI.Themes.Configurations;

/// <summary>
/// A group of theme colour entries
/// </summary>
public class ThemeConfigEntryGroup : IThemeTreeEntry {
    private readonly Dictionary<string, IThemeTreeEntry> map;
    private readonly List<ThemeConfigEntryGroup> groups;
    private readonly List<ThemeConfigEntry> entries;

    public IReadOnlyList<ThemeConfigEntryGroup> Groups => this.groups;

    public IReadOnlyList<ThemeConfigEntry> Entries => this.entries;

    public string DisplayName { get; }

    public ThemeConfigEntryGroup(string displayName) {
        this.groups = new List<ThemeConfigEntryGroup>();
        this.entries = new List<ThemeConfigEntry>();
        this.map = new Dictionary<string, IThemeTreeEntry>();
        this.DisplayName = displayName;
    }

    public ThemeConfigEntryGroup GetOrCreateGroupByName(string name) {
        if (this.map.TryGetValue(name, out IThemeTreeEntry? entry)) {
            if (!(entry is ThemeConfigEntryGroup group)) {
                throw new InvalidOperationException($"Entry in use with the name '{name}' but is not a group");
            }

            return group;
        }

        ThemeConfigEntryGroup newGroup = new ThemeConfigEntryGroup(name);
        this.groups.Add(newGroup);
        this.map[name] = newGroup;
        return newGroup;
    }

    public ThemeConfigEntry CreateEntry(string name, string themeKey) {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(themeKey);
        if (this.map.ContainsKey(name)) {
            throw new InvalidOperationException($"Entry already exists with the name '{name}'");
        }

        ThemeConfigEntry newEntry = new ThemeConfigEntry(name, themeKey);
        this.entries.Add(newEntry);
        this.map[name] = newEntry;
        return newEntry;
    }

    internal void UpdateInheritedKeys(Theme? theme) {
        foreach (ThemeConfigEntryGroup group in this.groups) {
            group.UpdateInheritedKeys(theme);
        }

        foreach (ThemeConfigEntry entry in this.entries) {
            if (theme != null) {
                string? key = theme.GetInheritedBy(entry.ThemeKey, out int depth);
                entry.SetInheritedFromKey(key, key == null ? 0 : depth);
            }
            else {
                entry.SetInheritedFromKey(null, 0);
            }
        }
    }
}