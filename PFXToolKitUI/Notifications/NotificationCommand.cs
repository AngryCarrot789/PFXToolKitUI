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

using System.Diagnostics;
using PFXToolKitUI.Interactivity.Contexts;

namespace PFXToolKitUI.Notifications;

public delegate void NotificationCommandEventHandler(NotificationCommand sender);

public abstract class NotificationCommand {
    private Notification? notification;
    private string? text, tooltip;

    public string? Text {
        get => this.text;
        set {
            if (this.text != value) {
                this.text = value;
                this.TextChanged?.Invoke(this);
            }
        }
    }

    public string? ToolTip {
        get => this.tooltip;
        set {
            if (this.tooltip != value) {
                this.tooltip = value;
                this.ToolTipChanged?.Invoke(this);
            }
        }
    }

    /// <summary>
    /// Gets the notification that this command exists in
    /// </summary>
    public Notification? Notification {
        get => this.notification;
        internal set {
            if (this.notification != value) {
                this.notification = value;
                this.NotificationChanged?.Invoke(this);
            }

            if (value != null) {
                if (!ReferenceEquals(this.ContextData, value.ContextData)) {
                    this.ContextData = value.ContextData;
                    this.OnContextChanged(this.ContextData, value.ContextData);
                }
            }
            else if (this.ContextData != null) {
                this.ContextData = null;
                this.OnContextChanged(this.ContextData, null);
            }
        }
    }

    public IContextData? ContextData { get; private set; }

    public event NotificationCommandEventHandler? NotificationChanged;
    public event NotificationCommandEventHandler? TextChanged;
    public event NotificationCommandEventHandler? ToolTipChanged;
    public event NotificationCommandEventHandler? ContextDataChanged;
    public event NotificationCommandEventHandler? CanExecuteChanged;

    protected NotificationCommand() {
    }

    protected NotificationCommand(string? text) : this() {
        this.text = text;
    }

    /// <summary>
    /// Returns true when this command can execute, false when it cannot
    /// </summary>
    /// <returns></returns>
    public virtual bool CanExecute() {
        return true;
    }

    /// <summary>
    /// Executes the command
    /// </summary>
    /// <returns></returns>
    public abstract Task Execute();

    protected virtual void OnContextChanged(IContextData? oldData, IContextData? newData) {
        this.ContextDataChanged?.Invoke(this);
        this.CanExecuteChanged?.Invoke(this);
    }

    internal static void InternalOnNotificationContextChanged(NotificationCommand command, IContextData? oldCtx, IContextData? newCtx) {
        Debug.Assert(command.ContextData == oldCtx);

        command.ContextData = newCtx;
        command.OnContextChanged(command.ContextData, newCtx);
    }
}