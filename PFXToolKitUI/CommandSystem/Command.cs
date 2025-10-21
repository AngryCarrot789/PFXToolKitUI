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
using PFXToolKitUI.Services.Messaging;

namespace PFXToolKitUI.CommandSystem;

public delegate void CommandEventHandler(Command command, CommandEventArgs e);

/// <summary>
/// A class that represents something that can be executed. Commands are given contextual information
/// (see <see cref="CommandEventArgs.ContextData"/>) to do work. Commands do their work in the
/// <see cref="ExecuteCommandAsync"/> method, and can optionally specify their executability
/// via the <see cref="CanExecuteCore"/> method.
/// <para>
/// Commands are used primarily by the shortcut and advanced menu service to do work, but
/// they can also be used by things like buttons
/// </para>
/// <para>
/// These commands can be executed through the <see cref="CommandManager"/>
/// </para>
/// </summary>
[DebuggerDisplay("Command ({RegisteredCommandId}), Executing: {IsExecuting}, AllowMultipleExecutions: {AllowMultipleExecutions}")]
public abstract class Command {
    private int executingCount;
    internal string? registeredCommandId;

    // Used to track issues of commands staying permanently executing
    private volatile Task? theLastRunTask;

    public bool AllowMultipleExecutions { get; }

    /// <summary>
    /// Gets whether the command is currently executing. Only changes on the main thread
    /// </summary>
    public bool IsExecuting => this.executingCount != 0;
    
    public string RegisteredCommandId => this.registeredCommandId ?? throw new Exception("Command is not registered");

    protected Command() : this(false) {
    }

    protected Command(bool allowMultipleExecutions) {
        this.AllowMultipleExecutions = allowMultipleExecutions;
    }
    
    // When focus changes, raise notification to update commands
    // Then fire ContextDataChanged for those command hooks or whatever, they can then disconnect
    // old event handlers and attach new ones

    /// <summary>
    /// Gets the executability state for this command. When <see cref="Executability.Valid"/>, it means that
    /// the command can do what it's supposed to do. Any other state is up to the UI on how to present the UI.
    /// <para>
    /// Commands used by menu items and context menu items will typically be hidden when the state is <see cref="Executability.Invalid"/>,
    /// and will just be disabled when the state is <see cref="Executability.ValidButCannotExecute"/>, otherwise clickable.
    /// However, buttons will typically just be disabled when the state is not <see cref="Executability.Valid"/>
    /// </para>
    /// </summary>
    /// <param name="e">Command event args</param>
    /// <returns>The executability</returns>
    public Executability CanExecute(CommandEventArgs e) {
        Executability result = this.CanExecuteCore(e);

        // Prevent ValidButCannotExecute being used first
        if (result == Executability.Invalid)
            return result;

        return !this.AllowMultipleExecutions && this.IsExecuting ? Executability.ValidButCannotExecute : result;
    }

    /// <summary>
    /// Gets this command's executability state based on the given command event args context.
    /// This typically isn't checked before actually executing, but instead is used by the UI to
    /// determine if something like a button or menu item is actually clickable
    /// <para>
    /// This method should be quick to execute, as it may be called quite often
    /// </para>
    /// </summary>
    /// <param name="e">The command event args, containing info about the current context</param>
    /// <returns>
    /// True if executing this command would most likely result in success, otherwise false
    /// </returns>
    protected virtual Executability CanExecuteCore(CommandEventArgs e) => Executability.Valid;

    /// <summary>
    /// Executes this command with the given command event args. This is always called on the application main thread (AMT)
    /// </summary>
    /// <param name="e">The command event args, containing info about the current context</param>
    protected abstract Task ExecuteCommandAsync(CommandEventArgs e);

    internal async Task InternalExecuteImpl(CommandEventArgs args) {
        ApplicationPFX.Instance.Dispatcher.VerifyAccess();
        int executing;
        if (this.AllowMultipleExecutions) {
            executing = Interlocked.Increment(ref this.executingCount);
        }
        else if ((executing = Interlocked.CompareExchange(ref this.executingCount, 1, 0)) != 0) {
            await this.OnAlreadyExecuting(args);
            return;
        }

        if (executing < 0) {
            Debugger.Break();
        }

        try {
            await (this.theLastRunTask = this.ExecuteCommandAsync(args));
        }
        catch (OperationCanceledException) {
            // ignroed
        }
        finally {
            executing = Interlocked.Decrement(ref this.executingCount);
            if (executing < 0) {
                Debugger.Break();
            }

            this.theLastRunTask = null;
        }
    }

    /// <summary>
    /// Invoked when this command is already running, but <see cref="ExecuteCommand_NotAsync"/> was called again.
    /// By default, this shows a message box
    /// </summary>
    /// <param name="args">Command event args</param>
    protected virtual Task OnAlreadyExecuting(CommandEventArgs args) {
        if (args.IsUserInitiated)
            return IMessageDialogService.Instance.ShowMessage("Already running", "This command is already running. Please wait for it to complete", MessageBoxButtons.OK, MessageBoxResult.OK);

        return Task.CompletedTask;
    }
}