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

using System.Diagnostics;
using PFXToolKitUI.Shortcuts;

namespace PFXToolKitUI.Configurations.Shortcuts;

public delegate void ModifiedShortcutsChangedEventHandler(ShortcutEditorConfigurationPage page);

public class ShortcutEditorConfigurationPage : ConfigurationPage {
    public ShortcutManager ShortcutManager { get; }

    public ShortcutGroupEntry? RootGroupEntry { get; private set; }

    private Dictionary<ShortcutEntry, IShortcut>? originalShortcuts;

    public IEnumerable<ShortcutEntry> ModifiedShortcuts => this.originalShortcuts?.Keys ?? Enumerable.Empty<ShortcutEntry>();

    public ShortcutEditorConfigurationPage(ShortcutManager shortcutManager) {
        this.ShortcutManager = shortcutManager;
    }

    protected override ValueTask OnContextCreated(ConfigurationContext context) {
        this.RootGroupEntry = this.ShortcutManager.Root;
        this.originalShortcuts = new Dictionary<ShortcutEntry, IShortcut>();
        return base.OnContextCreated(context);
    }

    protected override void OnIsViewingChanged(ConfigurationContext? oldContext, ConfigurationContext? newContext) {
        base.OnIsViewingChanged(oldContext, newContext);
        if (newContext == null) {
            this.originalShortcuts = null;
        }
    }

    protected override ValueTask OnContextDestroyed(ConfigurationContext context) {
        this.RootGroupEntry = null;
        return base.OnContextDestroyed(context);
    }

    // TODO: Create a shadow of the shortcut tree that is mutable, and then reflect back to tree in this method
    public override ValueTask Apply(List<ApplyChangesFailureEntry>? errors) {
        return ValueTask.CompletedTask;
    }

    public void OnShortcutChanged(ShortcutEntry entry, IShortcut oldShortcut, IShortcut newShortcut) {
        if (this.originalShortcuts == null) {
            Debug.Assert(false, "This method should not get called when the page is not active");
            return;
        }

        if (this.originalShortcuts.TryGetValue(entry, out IShortcut? original)) {
            if (Equals(original, newShortcut)) {
                this.originalShortcuts.Remove(entry);
            }
        }
        else {
            this.originalShortcuts[entry] = newShortcut;
        }

        if (this.originalShortcuts.Count < 1) {
            this.IsModified = false;
        }
        else if (!this.IsModified) {
            this.IsModified = true;
        }
    }
}