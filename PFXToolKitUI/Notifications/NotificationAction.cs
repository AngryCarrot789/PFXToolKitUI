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

public delegate void NotificationActionEventHandler(NotificationAction sender);

public delegate void NotificationActionContextDataChangedEventHandler(NotificationAction sender, IContextData? oldData, IContextData? newData);

/// <summary>
/// An action that can be executed from within a notification
/// </summary>
public abstract class NotificationAction {
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
    /// changes or the notification's <see cref="Notification.ContextData"/> changes
    /// </summary>
    public IContextData? ContextData { get; private set; }

    public event NotificationActionEventHandler? NotificationChanged;
    public event NotificationActionEventHandler? TextChanged;
    public event NotificationActionEventHandler? ToolTipChanged;
    public event NotificationActionContextDataChangedEventHandler? ContextDataChanged;

    /// <summary>
    /// Fired as a hint that <see cref="CanExecute"/> may return a different value prior to this event being fired
    /// </summary>
    public event NotificationActionEventHandler? CanExecuteChanged;

    protected NotificationAction() {
    }

    protected NotificationAction(string? text) : this() {
        this.text = text;
    }

    /// <summary>
    /// Gets whether this action can be executed
    /// </summary>
    /// <returns>True if <see cref="Execute"/> will (most likely) do useful work</returns>
    public virtual bool CanExecute() {
        return true;
    }

    /// <summary>
    /// Executes this notification action
    /// </summary>
    public abstract Task Execute();

    /// <summary>
    /// Invoked when our <see cref="Notification"/>'s <see cref="Notification.ContextData"/> changes
    /// </summary>
    /// <param name="oldData">The previous context data</param>
    /// <param name="newData">The new context data</param>
    protected virtual void OnContextChanged(IContextData? oldData, IContextData? newData) {
        this.ContextDataChanged?.Invoke(this, oldData, newData);
        this.RaiseCanExecuteChanged();
    }

    protected void RaiseCanExecuteChanged() {
        this.CanExecuteChanged?.Invoke(this);
    }

    internal static void InternalOnNotificationContextChanged(NotificationAction action, IContextData? oldCtx, IContextData? newCtx) {
        Debug.Assert(action.ContextData == oldCtx);

        action.ContextData = newCtx;
        action.OnContextChanged(action.ContextData, newCtx);
    }
}