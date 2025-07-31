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

using Avalonia;
using PFXToolKitUI.Avalonia.Interactivity;
using PFXToolKitUI.CommandSystem;
using PFXToolKitUI.Interactivity.Contexts;
using PFXToolKitUI.Utils.Commands;

namespace PFXToolKitUI.Avalonia.Utils;

/// <summary>
/// An async relay command that invokes a <see cref="Command"/>. This relay command does not support
/// <see cref="Command.AllowMultipleExecutions"/>, so even if the command can run multiple times concurrently, this relay command cannot
/// </summary>
public class DataManagerCommandWrapper : BaseAsyncRelayCommand {
    public AvaloniaObject Control { get; }

    public string CommandId { get; }

    public bool CanQueryExecutability { get; }

    public DataManagerCommandWrapper(AvaloniaObject control, string commandId, bool canQueryExecutability = false) {
        this.Control = control;
        this.CommandId = commandId;
        this.CanQueryExecutability = canQueryExecutability;
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
        Executability state = CommandManager.Instance.CanExecute(cmd, data);
        return state == Executability.Valid;
    }

    protected override Task ExecuteCoreAsync(object? parameter) {
        CommandManager manager = CommandManager.Instance;
        Command? cmd = manager.GetCommandById(this.CommandId);
        if (cmd == null) {
            return Task.CompletedTask;
        }

        IContextData data = DataManager.GetFullContextData(this.Control);
        return CommandManager.Instance.Execute(cmd, data);
    }
}