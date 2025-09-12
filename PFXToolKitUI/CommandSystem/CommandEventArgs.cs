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

namespace PFXToolKitUI.CommandSystem;

/// <summary>
/// Command event arguments for when a command is about to be executed
/// </summary>
public class CommandEventArgs {
    /// <summary>
    /// The command manager associated with this event
    /// </summary>
    public CommandManager Manager { get; }

    /// <summary>
    /// The contextual data for this specific command execution. This will not be null, but it may be empty (contain no inner data or data context)
    /// <para>
    /// In terms of actual context-specific commands, this will typically contain the data keys that UI controls
    /// have associated with themselves, merged from top to bottom (top of visual tree to the contextual element).
    /// This gives a wide range of access to the objects being acted upon
    /// </para>
    /// </summary>
    public IContextData ContextData { get; }

    /// <summary>
    /// Gets the shortcut entry that caused a command to execute
    /// </summary>
    public ShortcutEntry? Shortcut { get; }

    /// <summary>
    /// Gets the context registry for the context menu that caused a command to execute
    /// </summary>
    public ContextRegistry? SourceContextMenu { get; }

    /// <summary>
    /// Whether this command event was originally caused by a user or not, e.g. via a button/menu click or clicking a check box.
    /// Supply false if this was invoked by, for example, a task or scheduler. A non-user initiated execution usually won't
    /// create error dialogs and may instead log to the console or just throw an exception
    /// <para>
    /// If command events are chain called, it is best to pass the same user initialisation state to the next event
    /// </para>
    /// </summary>
    public bool IsUserInitiated { get; }

    public CommandEventArgs(CommandManager manager, IContextData contextData, ShortcutEntry? shortcut, ContextRegistry? sourceContextMenu, bool isUserInitiated) {
        this.Manager = manager ?? throw new ArgumentNullException(nameof(manager), "Command manager cannot be null");
        this.ContextData = contextData ?? throw new ArgumentNullException(nameof(contextData), "Context data cannot be null");
        this.Shortcut = shortcut;
        this.SourceContextMenu = sourceContextMenu;
        this.IsUserInitiated = isUserInitiated;
    }
}