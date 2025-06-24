// 
// Copyright (c) 2023-2025 REghZy
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

using PFXToolKitUI.Avalonia.Interactivity;
using PFXToolKitUI.CommandSystem;

namespace PFXToolKitUI.Avalonia.CommandUsages;

public class SimpleButtonCommandUsage : CommandUsage {
    private ClickableControlHelper? button;

    protected ClickableControlHelper Button => this.button ?? throw new InvalidOperationException("Not connected");

    public SimpleButtonCommandUsage(string commandId) : base(commandId) { }
    
    protected override void OnConnecting() {
        base.OnConnecting();
        this.button = ClickableControlHelper.Create(this.Control, this.OnButtonClicked);
    }

    protected override void OnDisconnected() {
        base.OnDisconnected();
        this.button?.Dispose();
        this.button = null;
    }

    protected virtual void OnButtonClicked() {
        if (!CommandManager.Instance.TryFindCommandById(this.CommandId, out Command? command)) {
            return;
        }

        CommandManager.Instance.Execute(this.CommandId, command, DataManager.GetFullContextData(this.Control!));
        if (!command.AllowMultipleExecutions && command.IsExecuting) {
            // IsExecuting cannot change in this scope. Reason: The command uses true async (e.g. Task.Delay)
            // and therefore the dispatcher is used to jump back to the main thread (sync context)
            command.ExecutingChanged += this.DoOnIsExecutingChanged;
            this.OnIsExecutingChanged(true);
        }
        else {
            this.OnIsExecutingChanged(null);
        }
    }

    private void DoOnIsExecutingChanged(Command theCmd, CommandEventArgs commandEventArgs) {
        this.OnIsExecutingChanged(false);
        theCmd.ExecutingChanged -= this.DoOnIsExecutingChanged;
    }

    /// <summary>
    /// Invoked when the executing state changed.
    /// </summary>
    /// <param name="isExecuting">
    /// Null if the command is not async and finished immediately,
    /// True if now executing, False is command completed
    /// </param>
    protected virtual void OnIsExecutingChanged(bool? isExecuting) {
        this.UpdateCanExecute();
    }

    protected override void OnUpdateForCanExecuteState(Executability state) {
        this.button!.IsEnabled = state == Executability.Valid;
    }
}