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

using System.Runtime.ExceptionServices;
using PFXToolKitUI.Interactivity.Contexts;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.CommandSystem;

/// <summary>
/// This class is a helper for executing commands when awaiting is not necessarily possible
/// </summary>
public class CommandExecutionContext {
    private List<Action>? onCompleted;

    public CommandManager CommandManager { get; }

    public Command Command { get; }

    public string CommandId { get; }

    public IContextData ContextData { get; }

    public bool IsUserInitiated { get; }

    public CommandExecutionContext(string commandId, Command command, CommandManager manager, IContextData context, bool isUserInitiated) {
        ArgumentException.ThrowIfNullOrWhiteSpace(commandId);
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(manager);
        this.CommandManager = manager;
        this.Command = command;
        this.CommandId = commandId;
        this.ContextData = context;
        this.IsUserInitiated = isUserInitiated;
    }

    public void AddCompletionAction(Action completionAction) {
        (this.onCompleted ??= new List<Action>()).Add(completionAction);
    }

    public async void Execute() {
        try {
            await this.CommandManager.Execute(this.Command, this.ContextData, null, null, this.IsUserInitiated);
        }
        catch (Exception e) {
            ApplicationPFX.Instance.Dispatcher.Post(() => ExceptionDispatchInfo.Throw(e), DispatchPriority.Send);
        }

        if (this.onCompleted != null) {
            using ErrorList list = new ErrorList("One or more errors occurred while running command completion callback", false);
            foreach (Action action in this.onCompleted) {
                try {
                    action();
                }
                catch (Exception e) {
                    list.Add(e);
                }
            }

            this.onCompleted = null;

            if (list.TryGetException(out Exception? exception)) {
                ApplicationPFX.Instance.Dispatcher.Post(() => ExceptionDispatchInfo.Throw(exception), DispatchPriority.Send);
            }
        }
    }
}