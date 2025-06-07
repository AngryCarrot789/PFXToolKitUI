//
// Copyright (c) 2024-2025 REghZy
//
// This file is part of FramePFX.
//
// FramePFX is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
//
// FramePFX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
//

using PFXToolKitUI.CommandSystem;
using PFXToolKitUI.Interactivity.Contexts;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.Notifications;

/// <summary>
/// A notification command that fired a <see cref="PFXToolKitUI.CommandSystem.Command"/>
/// </summary>
public class RegisteredNotificationCommand : NotificationCommand {
    private string? commandId;

    public string? CommandId {
        get => this.commandId;
        set => PropertyHelper.SetAndRaiseINE(ref this.commandId, value, this, static t => t.CommandIdChanged?.Invoke(t));
    }

    public event NotificationCommandEventHandler? CommandIdChanged;

    public RegisteredNotificationCommand() {
    }

    public RegisteredNotificationCommand(string? text, string commandId) : base(text) {
        this.commandId = commandId;
    }

    public override bool CanExecute() {
        if (!base.CanExecute() || this.CommandId == null)
            return false;
        return CommandManager.Instance.CanExecute(this.CommandId, this.ContextData ?? EmptyContext.Instance) == Executability.Valid;
    }

    public override Task Execute() {
        if (this.CommandId == null || this.ContextData == null) {
            return Task.CompletedTask;
        }
        
        return CommandManager.Instance.Execute(this.CommandId, this.ContextData);
    }
}