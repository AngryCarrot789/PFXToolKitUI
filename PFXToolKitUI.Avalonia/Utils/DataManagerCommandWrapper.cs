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

using System.Diagnostics;
using System.Windows.Input;
using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;
using PFXToolKitUI.Avalonia.Interactivity;
using PFXToolKitUI.CommandSystem;
using PFXToolKitUI.Interactivity.Contexts;
using PFXToolKitUI.Services.Messaging;
using PFXToolKitUI.Utils;
using PFXToolKitUI.Utils.Commands;

namespace PFXToolKitUI.Avalonia.Utils;

/// <summary>
/// An async relay command that invokes a <see cref="Command"/>. This relay command does not support
/// <see cref="Command.AllowMultipleExecutions"/>, so even if the command can run multiple times concurrently, this relay command cannot
/// </summary>
public class DataManagerCommandWrapper : BaseAsyncRelayCommand {
    /// <summary>
    /// Gets the control used as to access the context data
    /// </summary>
    public AvaloniaObject Control { get; }

    /// <summary>
    /// Gets the ID of the command we invoke
    /// </summary>
    public string CommandId { get; }

    /// <summary>
    /// Gets whether <see cref="Command.CanExecute"/> will be queried in our <see cref="ICommand.CanExecute"/> method.
    /// When true, we also listen to <see cref="DataManager.InheritedContextChangedEvent"/> on the control to automatically
    /// raise <see cref="BaseRelayCommand.RaiseCanExecuteChanged"/>
    /// </summary>
    public bool CanQueryExecutability { get; }

    private bool isInvalidatePosted;

    public DataManagerCommandWrapper(AvaloniaObject control, string commandId, bool canQueryExecutability = false) {
        this.Control = control;
        this.CommandId = commandId;
        this.CanQueryExecutability = canQueryExecutability;
        if (this.CanQueryExecutability && control is IInputElement) {
            DataManager.AddInheritedContextChangedHandler(control, this.OnControlContextInvalidated);
        }
    }

    private void OnControlContextInvalidated(object? sender, RoutedEventArgs e) {
        if (!this.isInvalidatePosted) {
            this.isInvalidatePosted = true;
            ApplicationPFX.Instance.Dispatcher.Post(static t => {
                DataManagerCommandWrapper w = (DataManagerCommandWrapper) t!;
                w.RaiseCanExecuteChanged();
                w.isInvalidatePosted = false;
            }, this);
        }
    }

    protected override bool CanExecuteCore(object? parameter) {
        if (!this.CanQueryExecutability) {
            return true;
        }

        CommandManager manager = CommandManager.Instance;
        Command? cmd = manager.GetCommandById(this.CommandId);
        if (cmd == null) {
            return false;
        }

        IContextData data = DataManager.GetFullContextData(this.Control);
        Executability state = CommandManager.Instance.CanExecute(cmd, data, null, null);
        return state == Executability.Valid;
    }

    protected override async Task ExecuteCoreAsync(object? parameter) {
        CommandManager manager = CommandManager.Instance;
        Command? cmd = manager.GetCommandById(this.CommandId);
        if (cmd != null) {
            IContextData data = DataManager.GetFullContextData(this.Control);
            try {
                await CommandManager.Instance.Execute(cmd, data, null, null);
            }
            catch (Exception exception) when (!Debugger.IsAttached) {
                await IMessageDialogService.Instance.ShowExceptionMessage("Command Error", exception);
            }
        }
    }
}