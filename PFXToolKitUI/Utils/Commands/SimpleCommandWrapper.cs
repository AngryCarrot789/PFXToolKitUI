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

using System.Windows.Input;
using PFXToolKitUI.CommandSystem;
using PFXToolKitUI.Interactivity.Contexts;

namespace PFXToolKitUI.Utils.Commands;

/// <summary>
/// An <see cref="System.Windows.Input.ICommand"/> wrapper for a <see cref="PFXToolKitUI.CommandSystem.Command"/>
/// </summary>
public class SimpleCommandWrapper : BaseAsyncRelayCommand {
    private readonly string commandId;
    private readonly CommandManager manager;

    /// <summary>
    /// The data key used to insert the parameter passed to <see cref="ICommand.CanExecute"/>
    /// and <see cref="ICommand.Execute"/> into the context data passed to the actual command
    /// </summary>
    public DataKey? ParameterDataKey { get; }

    /// <summary>
    /// Additional context data passed to the command. This is treated as the start of the new context data object
    /// </summary>
    public IContextData? ContextData { get; }

    public SimpleCommandWrapper(string commandId, DataKey? parameterDataKey, IContextData? contextData = null) {
        ArgumentException.ThrowIfNullOrWhiteSpace(commandId);
        this.commandId = commandId;
        this.manager = CommandManager.Instance;
        this.ParameterDataKey = parameterDataKey;
        this.ContextData = contextData;
    }

    public IContextData GetContextData(object? parameter) {
        IContextData finalData;
        if (this.ContextData != null) {
            ContextData data = new ContextData(this.ContextData);
            if (this.ParameterDataKey != null && parameter != null) {
                data.SetSafely(this.ParameterDataKey, parameter);
            }
            else if (parameter is IContextData) {
                data.AddAll((IContextData)parameter);
            }

            finalData = data;
        }
        else if (this.ParameterDataKey != null && parameter != null) {
            finalData = new ContextData().SetSafely(this.ParameterDataKey, parameter);
        }
        else if (parameter is IContextData) {
            finalData = (IContextData) parameter;
        }
        else {
            finalData = EmptyContext.Instance;
        }

        return finalData;
    }

    protected override bool CanExecuteCore(object? parameter) {
        Command? command = this.manager.GetCommandById(this.commandId);
        if (command == null) {
            return false;
        }

        IContextData ctx = this.GetContextData(parameter);
        Executability result = this.manager.CanExecute(command, ctx, null, null);
        return result == Executability.Valid;
    }

    protected override Task ExecuteCoreAsync(object? parameter) {
        Command? command = this.manager.GetCommandById(this.commandId);
        if (command == null) {
            return Task.CompletedTask;
        }

        IContextData ctx = this.GetContextData(parameter);
        return this.manager.Execute(command, ctx, null, null);
    }
}