// 
// Copyright (c) 2023-2025 REghZy
// 
// This file is part of FramePFX.
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
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
// 

using PFXToolKitUI.Persistence;

namespace PFXToolKitUI.Configurations;

/// <summary>
/// A class which exposes information about a hierarchy of configuration pages.
/// FramePFX has two: the application settings and project settings, both of which
/// are separate instances and store their own hierarchy of configuration pages
/// </summary>
public abstract class ConfigurationManager {
    /// <summary>
    /// Gets our root configuration entry. Add sub-entries to this if you want them to
    /// appear as top-level tree entries in the configuration tree
    /// </summary>
    public ConfigurationEntry RootEntry { get; }

    public ConfigurationManager() {
        this.RootEntry = new ConfigurationEntry() { DisplayName = "<root>" };
    }

    private const int Flag_None = 0;
    private const int Flag_OnlyIfModified = 1;

    public ValueTask RevertLiveChangesInHierarchyAsync(List<ApplyChangesFailureEntry>? errors) {
        return ApplyPagesRecursive(this.RootEntry, x => x.RevertLiveChanges(errors), Flag_None);
    }

    /// <summary>
    /// Applies all changes to our configuration manager's hierarchy (aka recursively apply)
    /// </summary>
    public async ValueTask ApplyChangesInHierarchyAsync(List<ApplyChangesFailureEntry>? errors) {
        PersistentStorageManager manager = ApplicationPFX.Instance.PersistentStorageManager;

        manager.BeginSavingStack();
        await ApplyPagesRecursive(this.RootEntry, x => x.Apply(errors), Flag_OnlyIfModified);
        if (manager.EndSavingStack()) {
            manager.SaveAll();
        }
    }

    private ValueTask LoadContextAsync(ConfigurationContext context) {
        return ApplyPagesRecursive(this.RootEntry, x => {
            x.IsModified = false;
            return ConfigurationPage.InternalOnContextCreated(x, context);
        }, Flag_None);
    }

    private ValueTask UnloadContextAsync(ConfigurationContext context) {
        return ApplyPagesRecursive(this.RootEntry, x => {
            // Should be null if the page system has no bugs in it. But, check
            // anyway because some pages might do stuff in the context change handler
            if (x.ActiveContext != null) {
                ConfigurationPage.InternalSetContext(x, null);
            }

            return ConfigurationPage.InternalOnContextDestroyed(x, context);
        }, Flag_None);
    }

    private static async ValueTask ApplyPagesRecursive(ConfigurationEntry entry, Func<ConfigurationPage, ValueTask> action, int flags) {
        if (entry.Page != null) {
            // ReSharper disable once ReplaceWithSingleAssignment.True

            bool canExec = true;
            if ((flags & Flag_OnlyIfModified) != 0 && !entry.Page.IsModified)
                canExec = false;

            if (canExec)
                await action(entry.Page);
        }

        foreach (ConfigurationEntry item in entry.Items) {
            await ApplyPagesRecursive(item, action, flags);
        }
    }

    public static ValueTask InternalLoadContext(ConfigurationManager manager, ConfigurationContext context) => manager.LoadContextAsync(context);
    
    public static ValueTask InternalUnloadContext(ConfigurationManager manager, ConfigurationContext context) => manager.UnloadContextAsync(context);
}