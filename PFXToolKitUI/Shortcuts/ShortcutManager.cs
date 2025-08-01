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

using PFXToolKitUI.CommandSystem;
using PFXToolKitUI.Shortcuts.Events;
using PFXToolKitUI.Shortcuts.Inputs;
using PFXToolKitUI.Shortcuts.Usage;

namespace PFXToolKitUI.Shortcuts;

public delegate void ShortcutActivityEventHandler(ShortcutInputProcessor processor);

/// <summary>
/// A class for storing and managing shortcuts
/// </summary>
public abstract class ShortcutManager {
    private List<ShortcutEntry>? cachedAllShortcuts;
    private readonly Dictionary<string, LinkedList<ShortcutEntry>> cachedCmdToShortcut; // linked because there will only really be like 1 or 2 ever
    private readonly Dictionary<string, ShortcutEntry> cachedPathToShortcut;
    private readonly Dictionary<string, InputStateManager> stateGroups;
    private ShortcutGroupEntry root;

    public ShortcutGroupEntry Root {
        get => this.root;
        protected set {
            ShortcutGroupEntry old = this.root;
            this.root = value;
            this.OnRootChanged(old, value);
        }
    }

    /// <summary>
    /// Gets the shortcut currently being activated
    /// </summary>
    public IShortcut? CurrentlyActivatingShortcut { get; private set; }
    
    /// <summary>
    /// An event fired when a <see cref="ShortcutEntry"/>'s shortcut is modified
    /// </summary>
    public event ShortcutModifiedEventHandler<ShortcutEntry>? ShortcutModified;

    /// <summary>
    /// Gets or sets the application wide shortcut manager. Realistically, only 1 needs to exist during the runtime of the app
    /// </summary>
    public static ShortcutManager Instance => ApplicationPFX.Instance.ServiceManager.GetService<ShortcutManager>();

    protected ShortcutManager() {
        this.cachedCmdToShortcut = new Dictionary<string, LinkedList<ShortcutEntry>>();
        this.cachedPathToShortcut = new Dictionary<string, ShortcutEntry>();
        this.stateGroups = new Dictionary<string, InputStateManager>();
        this.root = ShortcutGroupEntry.CreateRoot(this);
    }

    public virtual void ReloadFromFile(string filePath) {
        using BufferedStream stream = new BufferedStream(File.OpenRead(filePath));
        this.ReloadFromStream(stream);
    }

    /// <summary>
    /// Completely wipes the shortcut manager and then re-loads the keymap from the given stream
    /// </summary>
    /// <param name="stream"></param>
    public abstract void ReloadFromStream(Stream stream);

    public ShortcutGroupEntry? FindGroupByPath(string path) {
        return this.Root.GetGroupByPath(path);
    }

    public ShortcutEntry? FindShortcutByPath(string path) {
        this.EnsureCacheBuilt();
        return this.cachedPathToShortcut.GetValueOrDefault(path);
        // return this.Root.GetShortcutByPath(path);
    }

    public ShortcutEntry? FindFirstShortcutByCommandId(string cmdId) {
        this.EnsureCacheBuilt();
        return this.cachedCmdToShortcut.TryGetValue(cmdId, out LinkedList<ShortcutEntry>? list) && list.Count > 0 ? list.First!.Value : null;
    }

    /// <summary>
    /// Called when the root group changes
    /// </summary>
    protected virtual void OnRootChanged(ShortcutGroupEntry oldRoot, ShortcutGroupEntry newRoot) {
        this.InvalidateShortcutCache();
    }

    /// <summary>
    /// This will invalidate the cached shortcuts, meaning they will be regenerated when needed
    /// <para>
    /// This should be called if a shortcut or shortcut group was modified (e.g. a new shortcut group and added or a shortcut was removed, shortcut changed)
    /// </para>
    /// </summary>
    public virtual void InvalidateShortcutCache() {
        this.cachedAllShortcuts = null;
        this.stateGroups.Clear();
        this.cachedCmdToShortcut.Clear();
        this.cachedPathToShortcut.Clear();
    }

    /// <summary>
    /// Creates a new shortcut processor for this manager
    /// </summary>
    /// <returns>A new processor</returns>
    public abstract ShortcutInputProcessor NewProcessor();

    public IEnumerable<ShortcutEntry> GetAllShortcuts() {
        this.EnsureCacheBuilt();
        return this.cachedAllShortcuts!;
    }

    public IEnumerable<ShortcutEntry>? GetShortcutsByCommandId(string cmdId) {
        this.EnsureCacheBuilt();
        return this.cachedCmdToShortcut.GetValueOrDefault(cmdId);
    }

    public static void GetAllShortcuts(ShortcutGroupEntry rootGroupEntry, ICollection<ShortcutEntry> accumulator) {
        foreach (ShortcutEntry shortcut in rootGroupEntry.Shortcuts) {
            accumulator.Add(shortcut);
        }

        foreach (ShortcutGroupEntry innerGroup in rootGroupEntry.Groups) {
            GetAllShortcuts(innerGroup, accumulator);
        }
    }

    protected void EnsureCacheBuilt() {
        if (this.cachedAllShortcuts == null) {
            this.RebuildShortcutCache();
        }
    }

    private void RebuildShortcutCache() {
        this.cachedAllShortcuts = new List<ShortcutEntry>(64);
        this.cachedCmdToShortcut.Clear();
        this.cachedPathToShortcut.Clear();
        if (this.root != null) {
            GetAllShortcuts(this.root, this.cachedAllShortcuts);
        }

        foreach (ShortcutEntry shortcut in this.cachedAllShortcuts) {
            if (!string.IsNullOrWhiteSpace(shortcut.CommandId)) {
                if (!this.cachedCmdToShortcut.TryGetValue(shortcut.CommandId, out LinkedList<ShortcutEntry>? list)) {
                    this.cachedCmdToShortcut[shortcut.CommandId] = list = new LinkedList<ShortcutEntry>();
                }

                list.AddLast(shortcut);
            }

            // path should only be null or non-empty
            if (!string.IsNullOrWhiteSpace(shortcut.FullPath)) {
                if (this.cachedPathToShortcut.ContainsKey(shortcut.FullPath))
                    throw new Exception("Duplicate shortcut path: " + shortcut.FullPath);
                this.cachedPathToShortcut[shortcut.FullPath] = shortcut;
            }
        }

        this.cachedAllShortcuts.TrimExcess();
    }

    public IEnumerable<ShortcutEntry> FindShortcutsByPaths(IEnumerable<string> paths) {
        foreach (string path in paths) {
            ShortcutEntry? shortcut = this.FindShortcutByPath(path);
            if (shortcut != null) {
                yield return shortcut;
            }
        }
    }

    /// <summary>
    /// Invoked when a <see cref="ShortcutInputProcessor"/> activates a shortcut. This should be called first, in order to
    /// fire the <see cref="MonitorShortcutActivated"/> event handlers.
    /// <para>
    /// If none of the event handlers handle the activation, this method calls <see cref="OnShortcutActivatedOverride"/>
    /// which then attempts to invoke the debug reflection handlers, command manager, etc.
    /// </para>
    /// </summary>
    /// <param name="inputProcessor">The processor that caused this activation</param>
    /// <param name="shortcutEntry">The shortcut that was activated</param>
    /// <returns>The outcome of the shortcut activation used by the processor's input manager</returns>
    public bool OnShortcutActivated(ShortcutInputProcessor inputProcessor, ShortcutEntry shortcutEntry) {
        try {
            this.CurrentlyActivatingShortcut = shortcutEntry.Shortcut;
            return this.OnShortcutActivatedOverride(inputProcessor, shortcutEntry);
        }
        finally {
            this.CurrentlyActivatingShortcut = null;
        }
    }

    /// <summary>
    /// Further attempts to 'activate' a shortcut
    /// </summary>
    /// <param name="inputProcessor">The processor that caused this activation</param>
    /// <param name="shortcutEntry">The shortcut that was activated</param>
    /// <returns>The result of the shortcut activation used by the processor's input manager</returns>
    protected virtual bool OnShortcutActivatedOverride(ShortcutInputProcessor inputProcessor, ShortcutEntry shortcutEntry) {
        Command? command = CommandManager.Instance.GetCommandById(shortcutEntry.CommandId);
        if (command == null) {
            return false;
        }

        CommandManager.Instance.Execute(command, inputProcessor.ProvideCurrentContextInternal()!);
        return true;
    }

    /// <summary>
    /// Called by the <see cref="ShortcutInputProcessor"/> when an input state is activated
    /// </summary>
    /// <param name="inputProcessor">The processor which caused the state to be activated</param>
    /// <param name="stateEntry">The state that was activated</param>
    protected internal virtual void OnInputStateActivated(ShortcutInputProcessor inputProcessor, InputStateEntry stateEntry) { }

    /// <summary>
    /// Called by the <see cref="ShortcutInputProcessor"/> when an input state is deactivated
    /// </summary>
    /// <param name="inputProcessor">The processor which caused the state to be deactivated</param>
    /// <param name="stateEntry">The state that was activated</param>
    protected internal virtual void OnInputStateDeactivated(ShortcutInputProcessor inputProcessor, InputStateEntry stateEntry) { }

    /// <summary>
    /// Gets or creates an <see cref="InputStateManager"/> for the given path
    /// </summary>
    /// <param name="id">The state manager's ID, which is shared across this <see cref="ShortcutManager"/> instance</param>
    /// <returns>An existing or new instance</returns>
    public InputStateManager GetInputStateManager(string id) {
        if (!this.stateGroups.TryGetValue(id, out InputStateManager? group))
            this.stateGroups[id] = group = new InputStateManager(this, id);
        return group;
    }

    public void OnShortcutModified(ShortcutEntry shortcutEntry, IShortcut oldShortcut) {
        this.InvalidateShortcutCache();
        this.ShortcutModified?.Invoke(shortcutEntry, oldShortcut);
    }

    protected internal virtual void OnSecondShortcutUsagesProgressed(ShortcutInputProcessor inputProcessor) { }

    protected internal virtual void OnShortcutUsagesCreated(ShortcutInputProcessor inputProcessor) { }

    protected internal virtual void OnCancelUsageForNoSuchNextMouseStroke(ShortcutInputProcessor inputProcessor, IShortcutUsage usage, ShortcutEntry shortcutEntry, MouseStroke stroke) { }

    protected internal virtual void OnCancelUsageForNoSuchNextKeyStroke(ShortcutInputProcessor inputProcessor, IShortcutUsage usage, ShortcutEntry shortcutEntry, KeyStroke stroke) { }

    protected internal virtual void OnNoSuchShortcutForMouseStroke(ShortcutInputProcessor inputProcessor, string? group, MouseStroke stroke) { }

    protected internal virtual void OnNoSuchShortcutForKeyStroke(ShortcutInputProcessor inputProcessor, string? group, KeyStroke stroke) { }
}