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
using PFXToolKitUI.Interactivity.Contexts;
using PFXToolKitUI.Shortcuts;
using PFXToolKitUI.Utils.RDA;

namespace PFXToolKitUI.CommandSystem;

public delegate void FocusChangedEventHandler(CommandManager manager, IContextData newFocus);

/// <summary>
/// A class which manages registered commands and the execution of commands.
/// Commands are registered at application startup, before any primary UI is loaded
/// </summary>
public sealed class CommandManager {
    public static CommandManager Instance => ApplicationPFX.Instance.ServiceManager.GetService<CommandManager>();

    // using this just in case I soon add more data associated with commands
    private class CommandEntry {
        public readonly Command Command;
#if DEBUG
        public readonly string CreationStackTrace;
#endif

        public CommandEntry(Command command) {
            this.Command = command;
#if DEBUG
            this.CreationStackTrace = new StackTrace().ToString();
#endif
        }
    }

    /// <summary>
    /// Gets the number of commands registered
    /// </summary>
    public int Count => this.commands.Count;

    public IEnumerable<KeyValuePair<string, Command>> Commands => this.commands.Select(x => new KeyValuePair<string, Command>(x.Key, x.Value.Command));

    /// <summary>
    /// An event fired when the application's focus changes, possibly invalidating the executability state of a command presentation
    /// </summary>
    public event FocusChangedEventHandler? FocusChanged {
        add {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            this.focusChangeHandlerSet.Add(value);
        }
        remove {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            this.focusChangeHandlerSet.Remove(value);
        }
    }

    private readonly Dictionary<string, CommandEntry> commands;
    private readonly HashSet<FocusChangedEventHandler> focusChangeHandlerSet;
    private readonly RapidDispatchAction<Func<IContextData>> focusChangeRaiserRda;

    public CommandManager() {
        this.commands = new Dictionary<string, CommandEntry>();
        this.focusChangeHandlerSet = new HashSet<FocusChangedEventHandler>();
        this.focusChangeRaiserRda = new RapidDispatchAction<Func<IContextData>>(this.OnFocusChange, DispatchPriority.Background);
    }

    /// <summary>
    /// Registers a command with the given ID
    /// </summary>
    /// <param name="id">The ID to register the command with</param>
    /// <param name="command">The command to register</param>
    /// <exception cref="ArgumentException">Command ID is null or empty</exception>
    /// <exception cref="ArgumentNullException">Command is null</exception>
    public void Register(string id, Command command) {
        ValidateId(id);
        if (command == null)
            throw new ArgumentNullException(nameof(command));
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
    public Task Execute(string commandId, IContextData context, bool isUserInitiated = true) {
        ValidateId(commandId);
        ValidateContext(context);
        if (this.commands.TryGetValue(commandId, out CommandEntry? command)) {
            return command.Command.InternalExecuteImpl(new CommandEventArgs(this, context, ShortcutManager.Instance.CurrentlyActivatingShortcut, isUserInitiated));
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Executes the given command
    /// </summary>
    /// <param name="command"></param>
    /// <param name="context"></param>
    /// <param name="isUserInitiated"></param>
    /// <returns></returns>
    public Task Execute(Command command, IContextData context, bool isUserInitiated = true) {
        ValidateContext(context);
        return command.InternalExecuteImpl(new CommandEventArgs(this, context, ShortcutManager.Instance.CurrentlyActivatingShortcut, isUserInitiated));
    }

    public CommandExecutionContext BeginExecution(string commandId, Command command, IContextData context, bool isUserInitiated = true) {
        return new CommandExecutionContext(commandId, command, this, context, isUserInitiated);
    }

    /// <summary>
    /// Tries to get the presentation for a command with the given ID
    /// </summary>
    /// <param name="commandId">The command ID to execute</param>
    /// <param name="context">The context to use. Cannot be null</param>
    /// <param name="isUserInitiated">Whether a user caused the command to need to be executed. Supply false if this was invoked by a task or scheduler for example</param>
    /// <returns>The command's presentation for the current context</returns>
    /// <exception cref="Exception">The context is null, or the assembly was compiled in debug mode and the GetPresentation function threw ane exception</exception>
    /// <exception cref="ArgumentException">ID is null, empty or consists of only whitespaces</exception>
    /// <exception cref="ArgumentNullException">Context is null</exception>
    public Executability CanExecute(string commandId, IContextData context, bool isUserInitiated = true) {
        ValidateId(commandId);
        ValidateContext(context);
        if (this.commands.TryGetValue(commandId, out CommandEntry? command)) {
            return this.CanExecute(command.Command, context, isUserInitiated);
        }

        return Executability.Invalid;
    }

    public Executability CanExecute(Command command, IContextData context, bool isUserInitiated = true) {
        ValidateContext(context);
        return command.CanExecute(new CommandEventArgs(this, context, ShortcutManager.Instance.CurrentlyActivatingShortcut, isUserInitiated));
    }

    /// <summary>
    /// Invokes all focus change handlers for the given ID. This also invokes global handlers first
    /// </summary>
    /// <exception cref="ArgumentNullException">newFocusProvider is null</exception>
    internal static void InternalOnApplicationFocusChanged(Func<IContextData> newFocusProvider) {
        if (newFocusProvider == null)
            throw new ArgumentNullException(nameof(newFocusProvider));

        Instance.focusChangeRaiserRda.InvokeAsync(newFocusProvider);
    }

    private void OnFocusChange(Func<IContextData> newFocusProvider) {
        // only calls newFocusProvider if there are handlers
        if (this.focusChangeHandlerSet.Count >= 1) {
            IContextData ctx = newFocusProvider();
            if (ctx == null)
                throw new Exception("New Focus Context provider gave a null context");

            foreach (FocusChangedEventHandler handler in this.focusChangeHandlerSet) {
                handler(this, ctx);
            }
        }
    }

    public static void ValidateId(string id) {
        if (id != null && string.IsNullOrWhiteSpace(id)) {
            throw new ArgumentException("Command ID cannot be null or empty", nameof(id));
        }
    }

    public static void ValidateContext(IContextData context) {
        if (context == null) {
            throw new ArgumentNullException(nameof(context), "Context cannot be null");
        }
    }
}