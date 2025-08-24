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

using System.Diagnostics.CodeAnalysis;
using PFXToolKitUI.Icons;
using PFXToolKitUI.Utils;
using PFXToolKitUI.Utils.Collections.Observable;

namespace PFXToolKitUI.AdvancedMenuService;

/// <summary>
/// A context entry that is a regular entry but also contain child entries in a drop-down menu.
/// This is different from <see cref="IContextGroup"/>
/// </summary>
public class ContextEntryGroup : BaseContextEntry {
    private bool showDummyItemWhenEmpty = true;

    public ObservableList<IContextObject> Items { get; }

    /// <summary>
    /// Gets or sets whether to show a dummy item when <see cref="Items"/> is empty
    /// </summary>
    public bool ShowDummyItemWhenEmpty {
        get => this.showDummyItemWhenEmpty;
        set => PropertyHelper.SetAndRaiseINE(ref this.showDummyItemWhenEmpty, value, this, static t => t.ShowDummyItemWhenEmptyChanged?.Invoke(t));
    }

    /// <summary>
    /// Gets or sets the unique ID associated with this group. This is relative to the parent entry.
    /// </summary>
    public string? UniqueID { get; set; }

    public event BaseContextEntryEventHandler? ShowDummyItemWhenEmptyChanged;

    public ContextEntryGroup() {
        this.Items = new ObservableList<IContextObject>();
    }

    public ContextEntryGroup(string displayName, string? description = null, Icon? icon = null) : base(displayName, description, icon) {
        this.Items = new ObservableList<IContextObject>();
    }

    public bool TryGetGroupById(string uniqueId, [NotNullWhen(true)] out ContextEntryGroup? group) {
        ArgumentException.ThrowIfNullOrEmpty(uniqueId);
        foreach (IContextObject obj in this.Items) {
            if (obj is ContextEntryGroup g && g.UniqueID == uniqueId) {
                group = g;
                return true;
            }
        }

        group = null;
        return false;
    }
}