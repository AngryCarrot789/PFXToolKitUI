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

using PFXToolKitUI.Icons;

namespace PFXToolKitUI.AdvancedMenuService;

/// <summary>
/// A fixed group of context items
/// </summary>
public class FixedWeightedMenuEntryGroup : IWeightedMenuEntryGroup {
    private readonly List<IMenuEntry> items;

    /// <summary>
    /// Gets our items
    /// </summary>
    public IReadOnlyList<IMenuEntry> Items => this.items;

    public FixedWeightedMenuEntryGroup() {
        this.items = new List<IMenuEntry>();
    }

    public void AddEntry(IMenuEntry item) {
        this.items.Add(item);
    }

    public void AddSeparator() => this.AddEntry(new SeparatorEntry());

    public CaptionSeparatorEntry AddHeader(string caption) {
        CaptionSeparatorEntry separatorEntry = new CaptionSeparatorEntry(caption);
        this.items.Add(separatorEntry);
        return separatorEntry;
    }

    public CommandMenuEntry AddCommand(string cmdId, string displayName, string? description = null, Icon? icon = null) {
        CommandMenuEntry entry = new CommandMenuEntry(cmdId, displayName, description, icon);
        this.AddEntry(entry);
        return entry;
    }

    public void AddDynamicSubGroup(DynamicGenerateContextFunction generate) {
        this.AddEntry(new DynamicGroupPlaceholderMenuEntry(new DynamicWeightedMenuEntryGroup(generate)));
    }
}