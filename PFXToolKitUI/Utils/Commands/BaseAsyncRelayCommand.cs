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

namespace PFXToolKitUI.Utils.Commands;

/// <summary>
/// <para>
/// A base async relay command class which extends <see cref="BaseRelayCommand"/> and also implements a mechanism for
/// tracking if the command is currently being run, and if so, ignores any attempt to execute (either via <see cref="Execute"/>,
/// <see cref="ExecuteAsync"/> or <see cref="TryExecuteAsync"/>)
/// </para>
/// <para>
/// <see cref="IsRunning"/> (and most likely <see cref="CanExecute"/> too) return false if the command is being run, however, this shouldn't
/// be relied on due to the reality of multithreading; the command could finish just after another piece of code detects it's already running
/// </para>
/// </summary>
public abstract class BaseAsyncRelayCommand : BaseRelayCommand, IAsyncRelayCommand {
    /// <summary>
    /// Because <see cref="Execute"/> is async void, it can be fired multiple
    /// times while the task that <see cref="execute"/> returns is still running. This
    /// is used to track if it's running or not
    /// </summary>
    private volatile int isRunningState;

    /// <summary>
    /// Whether this command is running. Realistically, this shouldn't be used to determine whether to execute the
    /// command or not, due to the fact that the command may finish as soon as this property is fetched.
    /// See <see cref="TryExecuteAsync"/> for a somewhat workaround
    /// </summary>
    public bool IsRunning => this.isRunningState != 0;

    protected BaseAsyncRelayCommand() {
    }

    /// <summary>
    /// Whether this async command can actually run or not. If it is already running, this will (typically) return false
    /// <para>
    /// <see cref="TryExecuteAsync"/> is recommended over the standard way of calling commands which is:
    /// <code>if (cmd.CanExecute(param)) cmd.Execute(param)</code>
    /// </para>
    /// </summary>
    /// <param name="parameter">The parameter passed to this command</param>
    /// <returns>Whether this command can be executed or not</returns>
    public sealed override bool CanExecute(object? parameter) {
        return this.isRunningState == 0 && base.CanExecute(parameter) && this.CanExecuteCore(parameter);
    }

    /// <summary>
    /// The core method for checking if the command implementation can execute or not. This is called by <see cref="CanExecute"/>
    /// </summary>
    /// <param name="parameter">The parameter passed to this command</param>
    /// <returns>Whether this command can be executed or not</returns>
    protected virtual bool CanExecuteCore(object? parameter) {
        return true;
    }

    /// <summary>
    /// Executes this async command. This is async void, so it may return before the actual command has
    /// finished executing. <see cref="ExecuteAsync"/> is recommended if awaiting is required.
    /// </summary>
    /// <param name="parameter">The parameter passed to this command</param>
    // ReSharper disable once AsyncVoidMethod
    public sealed override async void Execute(object? parameter) {
        if (Interlocked.CompareExchange(ref this.isRunningState, 1, 0) != 0) {
            return;
        }

        try {
            await this.InternalInvokeAsync(parameter);
        }
        finally {
            this.EnsureNotRunning();
        }
    }

    // The 2 functions below have almost the same copied code in order to give a slightly cleaner
    // stack trace when debugging... and it probably also improves performance slightly

    /// <summary>
    /// Executes this async command, if it is not running. If the command is already running,
    /// then this method will return and the command will not be executed.
    /// <para>
    /// <see cref="TryExecuteAsync"/> should be used if you need to check if the command actually executed or not
    /// </para>
    /// </summary>
    /// <param name="parameter">The parameter passed to this command</param>
    // Slight optimisation by not using async for ExecuteAsync, so that a state machine isn't needed
    public async Task ExecuteAsync(object? parameter) {
        if (Interlocked.CompareExchange(ref this.isRunningState, 1, 0) == 0) {
            try {
                await this.InternalInvokeAsync(parameter);
            }
            finally {
                this.EnsureNotRunning();
            }
        }
    }

    /// <summary>
    /// Attempts to executing this async command. If the command is already running, then this method will return
    /// false and the command will not be executed. Otherwise, the command is executed and true is returned
    /// <para>
    /// This will query <see cref="CanExecute"/>
    /// </para>
    /// </summary>
    /// <param name="parameter">The parameter passed to this command</param>
    public async Task<bool> TryExecuteAsync(object? parameter) {
        if (this.CanExecute(parameter) && Interlocked.CompareExchange(ref this.isRunningState, 1, 0) == 0) {
            try {
                await this.InternalInvokeAsync(parameter);
                return true;
            }
            finally {
                this.EnsureNotRunning();
            }
        }

        return false;
    }

    private async Task InternalInvokeAsync(object? parameter) {
        try {
            this.RaiseCanExecuteChanged();
            await this.ExecuteCoreAsync(parameter);
        }
        catch (OperationCanceledException) {
            // ignored
        }
        catch (Exception exception) when (!Debugger.IsAttached) {
            await LogExceptionHelper.ShowMessageAndPrintToLogs("Application Action Error", exception);
        }
        finally {
            this.isRunningState = 0;
            this.RaiseCanExecuteChanged();
        }
    }

    /// <summary>
    /// The abstract function to be implemented by classes that extend <see cref="BaseAsyncRelayCommand"/> to actually "do" something
    /// </summary>
    /// <param name="parameter">The parameter passed to this command</param>
    /// <returns>A task...</returns>
    protected abstract Task ExecuteCoreAsync(object? parameter);

    private void EnsureNotRunning() {
        Debug.Assert(this.isRunningState == 0, "Task continuation failure, possibly caused by debugger issue");
        this.isRunningState = 0;
    }
}