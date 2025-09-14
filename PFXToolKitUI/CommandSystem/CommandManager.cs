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
using System.Diagnostics.CodeAnalysis;
using PFXToolKitUI.AdvancedMenuService;
using PFXToolKitUI.Interactivity.Contexts;
using PFXToolKitUI.Shortcuts;

namespace PFXToolKitUI.CommandSystem;

/// <summary>
/// A class which manages registered commands and the execution of commands.
/// Commands are registered at application startup, before any primary UI is loaded
/// </summary>
public sealed class CommandManager {
    public static CommandManager Instance => ApplicationPFX.GetComponent<CommandManager>();

    // using this just in case I soon add more data associated with commands
    private class CommandEntry {
        public readonly Command Command;
#if DEBUG
        public readonly string CreationStackTrace;
#endif

        public CommandEntry(Command command) {
            this.Command = command;
#if DEBUG
            this.CreationStackTrace = new StackTrace(true).ToString();
#endif
        }
    }

    /// <summary>
    /// Gets the number of commands registered
    /// </summary>
    public int Count => this.commands.Count;

    public IEnumerable<KeyValuePair<string, Command>> Commands => this.commands.Select(x => new KeyValuePair<string, Command>(x.Key, x.Value.Command));

    private readonly Dictionary<string, CommandEntry> commands;

    public CommandManager() {
        this.commands = new Dictionary<string, CommandEntry>();
    }

    /// <summary>
    /// Registers a command with the given ID
    /// </summary>
    /// <param name="id">The ID to register the command with</param>
    /// <param name="command">The command to register</param>
    /// <exception cref="ArgumentException">Command ID is null or empty</exception>
    /// <exception cref="ArgumentNullException">Command is null</exception>
    public void Register(string id, Command command) {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(command);
        this.RegisterInternal(id, command);
    }

    private void RegisterInternal(string id, Command command) {
        if (this.commands.TryGetValue(id, out CommandEntry? existing)) {
            throw new Exception($"A command is already registered with the ID '{id}': {existing.Command.GetType()}");
        }

        this.commands[id] = new CommandEntry(command);
        command.registeredCommandId = id;
    }

    /// <summary>
    /// Gets a command with the given ID
    /// </summary>
    public Command? GetCommandById(string? id) {
        return !string.IsNullOrEmpty(id) && this.commands.TryGetValue(id, out CommandEntry? command) ? command.Command : null;
    }

    public bool TryFindCommandById(string id, [NotNullWhen(true)] out Command? command) => (command = this.GetCommandById(id)) != null;

    /// <summary>
    /// Executes a command with the given ID and context
    /// </summary>
    /// <param name="commandId">The command ID to execute</param>
    /// <param name="context">The context to use. Cannot be null</param>
    /// <param name="isUserInitiated">
    /// Whether a user executed the command, e.g. via a button/menu click or clicking a check box.
    /// Supply false if this was invoked by, for example, a task or scheduler. A non-user initiated
    /// execution usually won't create error dialogs and may instead log to the console or just throw an exception
    /// </param>
    /// <exception cref="Exception">The context is null, or the assembly was compiled in debug mode and the command threw ane exception</exception>
    /// <exception cref="ArgumentException">ID is null, empty or consists of only whitespaces</exception>
    /// <exception cref="ArgumentNullException">Context is null</exception>
    public Task Execute(string commandId, IContextData context, ShortcutEntry? shortcut, ContextRegistry? contextMenu, bool isUserInitiated = true) {
        ArgumentException.ThrowIfNullOrWhiteSpace(commandId);
        ArgumentNullException.ThrowIfNull(context);
        if (this.commands.TryGetValue(commandId, out CommandEntry? command)) {
            return command.Command.InternalExecuteImpl(new CommandEventArgs(this, context, shortcut, contextMenu, isUserInitiated));
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Executes the command using the given context data and other information
    /// </summary>
    /// <param name="command">The command to execute</param>
    /// <param name="context">The context data to pass to the command</param>
    /// <param name="shortcut">
    /// The shortcut that caused the command to be executed, usually only
    /// non-null when called by the <see cref="ShortcutManager"/>
    /// </param>
    /// <param name="contextMenu">
    /// The context menu that owns the context entry that caused the command to be executed
    /// </param>
    /// <param name="isUserInitiated">
    /// Whether a user effectively caused the command to execute (e.g. button/context/menu
    /// click or a key/mouse shortcut was triggered).
    /// </param>
    /// <returns>
    /// A task that represents the command operation. When completed, <see cref="Command.IsExecuting"/>
    /// will return false if <see cref="Command.AllowMultipleExecutions"/> is also false
    /// </returns>
    public Task Execute(Command command, IContextData context, ShortcutEntry? shortcut, ContextRegistry? contextMenu, bool isUserInitiated = true) {
        ArgumentNullException.ThrowIfNull(context);
        return command.InternalExecuteImpl(new CommandEventArgs(this, context, shortcut, contextMenu, isUserInitiated));
    }

    public CommandExecutionContext BeginExecution(string commandId, Command command, IContextData context, bool isUserInitiated = true) {
        return new CommandExecutionContext(commandId, command, this, context, isUserInitiated);
    }
    
    /// <summary>
    /// Tries to get the executability for a command with the given ID
    /// </summary>
    /// <param name="commandId"></param>
    /// <param name="context"></param>
    /// <param name="shortcut"></param>
    /// <param name="contextMenu"></param>
    /// <param name="isUserInitiated"></param>
    /// <returns></returns>
    public Executability CanExecute(string commandId, IContextData context, ShortcutEntry? shortcut, ContextRegistry? contextMenu, bool isUserInitiated = true) {
        ArgumentException.ThrowIfNullOrWhiteSpace(commandId);
        ArgumentNullException.ThrowIfNull(context);
        if (this.commands.TryGetValue(commandId, out CommandEntry? command)) {
            return this.CanExecute(command.Command, context, shortcut, contextMenu, isUserInitiated);
        }

        return Executability.Invalid;
    }

    /// <summary>
    /// Gets the executability of a command
    /// </summary>
    /// <param name="command">The command</param>
    /// <param name="context">The context to use. Cannot be null</param>
    /// <param name="shortcut"></param>
    /// <param name="contextMenu"></param>
    /// <param name="isUserInitiated"></param>
    /// <returns></returns>
    public Executability CanExecute(Command command, IContextData context, ShortcutEntry? shortcut, ContextRegistry? contextMenu, bool isUserInitiated = true) {
        ArgumentNullException.ThrowIfNull(context);
        return command.CanExecute(new CommandEventArgs(this, context, shortcut, contextMenu, isUserInitiated));
    }
}