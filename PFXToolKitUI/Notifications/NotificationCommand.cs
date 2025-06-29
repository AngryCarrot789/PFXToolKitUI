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
using PFXToolKitUI.Interactivity.Contexts;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.Notifications;

public delegate void NotificationCommandEventHandler(NotificationCommand sender);

public delegate void NotificationCommandContextDataChangedEventHandler(NotificationCommand sender, IContextData? oldData, IContextData? newData);

public abstract class NotificationCommand {
    private Notification? notification;
    private string? text, tooltip;

    /// <summary>
    /// Gets or sets the text content of the command
    /// </summary>
    public string? Text {
        get => this.text;
        set => PropertyHelper.SetAndRaiseINE(ref this.text, value, this, static t => t.TextChanged?.Invoke(t));
    }

    /// <summary>
    /// Gets or sets the tooltip for this command
    /// </summary>
    public string? ToolTip {
        get => this.tooltip;
        set => PropertyHelper.SetAndRaiseINE(ref this.tooltip, value, this, static t => t.ToolTipChanged?.Invoke(t));
    }

    /// <summary>
    /// Gets the notification that this command exists in
    /// </summary>
    public Notification? Notification {
        get => this.notification;
        internal set {
            PropertyHelper.SetAndRaiseINE(ref this.notification, value, this, static t => t.NotificationChanged?.Invoke(t));
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

    /// <summary>
    /// Gets the current context data for this command. This changes when <see cref="Notification"/> 
    /// changes or the notification's <see cref="Notifications.Notification.ContextData"/> changes
    /// </summary>
    public IContextData? ContextData { get; private set; }

    public event NotificationCommandEventHandler? NotificationChanged;
    public event NotificationCommandEventHandler? TextChanged;
    public event NotificationCommandEventHandler? ToolTipChanged;
    public event NotificationCommandContextDataChangedEventHandler? ContextDataChanged;

    /// <summary>
    /// Fired as a hint that <see cref="CanExecute"/> may return a different value prior to this event being fired
    /// </summary>
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
    public abstract Task Execute();

    protected virtual void OnContextChanged(IContextData? oldData, IContextData? newData) {
        this.ContextDataChanged?.Invoke(this, oldData, newData);
        this.RaiseCanExecuteChanged();
    }

    protected void RaiseCanExecuteChanged() {
        this.CanExecuteChanged?.Invoke(this);
    }

    internal static void InternalOnNotificationContextChanged(NotificationCommand command, IContextData? oldCtx, IContextData? newCtx) {
        Debug.Assert(command.ContextData == oldCtx);

        command.ContextData = newCtx;
        command.OnContextChanged(command.ContextData, newCtx);
    }
}