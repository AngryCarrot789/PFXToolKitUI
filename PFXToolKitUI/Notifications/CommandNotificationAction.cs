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
using PFXToolKitUI.CommandSystem;
using PFXToolKitUI.Interactivity.Contexts;
using PFXToolKitUI.Services.Messaging;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.Notifications;

/// <summary>
/// A notification command that fired a <see cref="Command"/>
/// </summary>
public class CommandNotificationAction : NotificationAction {
    private string? commandId;

    public string? CommandId {
        get => this.commandId;
        set => PropertyHelper.SetAndRaiseINE(ref this.commandId, value, this, static t => t.CommandIdChanged?.Invoke(t));
    }

    public event NotificationActionEventHandler? CommandIdChanged;

    public CommandNotificationAction() {
    }

    public CommandNotificationAction(string? text, string commandId) : base(text) {
        this.commandId = commandId;
    }

    public override bool CanExecute() {
        if (!string.IsNullOrWhiteSpace(this.CommandId)) {
            return CommandManager.Instance.CanExecute(this.CommandId, this.ContextData ?? EmptyContext.Instance, null, null) == Executability.Valid;
        }

        return false;
    }

    public override async Task Execute() {
        if (!string.IsNullOrWhiteSpace(this.CommandId)) {
            try {
                await CommandManager.Instance.Execute(this.CommandId, this.ContextData ?? EmptyContext.Instance, null, null);
            }
            catch (Exception exception) when (!Debugger.IsAttached) {
                await IMessageDialogService.Instance.ShowExceptionMessage("Command Error", exception);
            }
        }
    }
}