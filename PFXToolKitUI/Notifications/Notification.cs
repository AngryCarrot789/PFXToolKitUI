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

using PFXToolKitUI.Composition;
using PFXToolKitUI.Interactivity.Contexts;
using PFXToolKitUI.Utils;
using PFXToolKitUI.Utils.Collections.Observable;

namespace PFXToolKitUI.Notifications;

public delegate void NotificationEventHandler(Notification sender);

public delegate void NotificationContextDataChangedEventHandler(Notification sender, IContextData? oldContextData, IContextData? newContextData);

/// <summary>
/// The base class for a notification type. The standard notification types
/// are <see cref="TextNotification"/> and <see cref="ActivityNotification"/>
/// <para>
/// A custom notification type requires a custom control.
/// </para>
/// </summary>
public abstract class Notification : IComponentManager {
    private readonly ComponentStorage myComponentStorage;
    private string? caption;
    private bool canAutoHide = true;
    private bool isAutoHideActive;
    private bool flagRestartAutoHide;
    private CancellationTokenSource? ctsAutoHide;
    private IContextData? myContextData;
    private TimeSpan autoHideDelay = TimeSpan.FromSeconds(5);
    private NotificationAlertMode alertMode;

    ComponentStorage IComponentManager.ComponentStorage => this.myComponentStorage;

    /// <summary>
    /// Gets or sets the text displayed in the notification's header
    /// </summary>
    public string? Caption {
        get => this.caption;
        set => PropertyHelper.SetAndRaiseINE(ref this.caption, value, this, static t => t.CaptionChanged?.Invoke(t));
    }

    /// <summary>
    /// Gets or sets if this notification can be auto-hidden (via <see cref="StartAutoHide"/>). Default value is true.
    /// Some notifications may not want to auto hide as they may contain crucial information such as errors which you
    /// may want to look at, so set this property to false.
    /// <para>
    /// Note, <see cref="StartAutoHide"/> is called as soon as the notification is shown in <see cref="NotificationManager"/>.
    /// Changing this property to false after <see cref="Show"/> will cancel auto-hide.
    /// </para>
    /// </summary>
    public bool CanAutoHide {
        get => this.canAutoHide;
        set {
            if (this.canAutoHide != value) {
                this.canAutoHide = value;
                if (!value) {
                    this.flagRestartAutoHide = false;
                    this.CancelAutoHide();
                }

                this.CanAutoHideChanged?.Invoke(this);
            }
        }
    }

    /// <summary>
    /// Gets whether this notification is currently in the process of auto-hiding.
    /// As is, is the background task waiting to finally hide the notification
    /// </summary>
    public bool IsAutoHideActive {
        get => this.isAutoHideActive;
        private set => PropertyHelper.SetAndRaiseINE(ref this.isAutoHideActive, value, this, static t => t.IsAutoHideActiveChanged?.Invoke(t));
    }

    /// <summary>
    /// Gets or sets the amount of time to wait until auto-hide calls <see cref="Hide"/>. Default is 5 seconds.
    /// This cannot be changed while <see cref="IsAutoHideActive"/> is true
    /// </summary>
    public TimeSpan AutoHideDelay {
        get => this.autoHideDelay;
        set {
            if (this.isAutoHideActive)
                throw new InvalidOperationException("Cannot change while auto-hide is currently active");

            if (this.autoHideDelay != value) {
                long totalMilliseconds = (long) value.TotalMilliseconds;
                if (totalMilliseconds < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), "Value cannot represent negative time");
                if (totalMilliseconds > 0xFFFFFFFE)
                    throw new ArgumentOutOfRangeException(nameof(value), "Value is too large");

                this.autoHideDelay = value;
                this.AutoHideDelayChanged?.Invoke(this);
            }
        }
    }

    /// <summary>
    /// Gets the time at which <see cref="IsAutoHideActive"/> was set to true
    /// </summary>
    public DateTime AutoHideStartTime { get; private set; }

    /// <summary>
    /// Gets or sets the context for this notification. This may be used by our <see cref="Actions"/>
    /// </summary>
    public IContextData? ContextData {
        get => this.myContextData;
        set {
            IContextData? oldContextData = this.myContextData;
            if (!Equals(oldContextData, value)) {
                this.myContextData = value;
                this.ContextDataChanged?.Invoke(this, oldContextData, value);
                foreach (NotificationAction action in this.Actions) {
                    NotificationAction.InternalOnNotificationContextChanged(action, oldContextData, value);
                }
            }
        }
    }

    /// <summary>
    /// Gets or sets the alert mode
    /// </summary>
    public NotificationAlertMode AlertMode {
        get => this.alertMode;
        set => PropertyHelper.SetAndRaiseINE(ref this.alertMode, value, this, static t => t.AlertModeChanged?.Invoke(t));
    }

    public CancellationToken CancellationToken => this.ctsAutoHide?.Token ?? CancellationToken.None;

    /// <summary>
    /// Gets the list of notification actions that the user can execute
    /// </summary>
    public ObservableList<NotificationAction> Actions { get; }

    /// <summary>
    /// Gets the notification manager this notification exists in
    /// </summary>
    public NotificationManager? NotificationManager { get; internal set; }

    /// <summary>
    /// Gets whether this notification is visible. This returns true when <see cref="NotificationManager"/> is non-null
    /// </summary>
    public bool IsVisible => this.NotificationManager != null;

    public event NotificationEventHandler? CaptionChanged;
    public event NotificationEventHandler? CanAutoHideChanged;
    public event NotificationEventHandler? IsAutoHideActiveChanged;
    public event NotificationEventHandler? AutoHideDelayChanged;
    public event NotificationContextDataChangedEventHandler? ContextDataChanged;
    public event NotificationEventHandler? AlertModeChanged;

    protected Notification() {
        this.myComponentStorage = new ComponentStorage(this);
        this.Actions = new ObservableList<NotificationAction>();
        this.Actions.ValidateAdd += (list, index, items) => {
            foreach (NotificationAction item in items) {
                if (item.Notification != null)
                    throw new InvalidOperationException($"{nameof(NotificationAction)} already exists in another notification");
            }
        };

        ObservableItemProcessor.MakeSimple(this.Actions, c => c.Notification = this, c => c.Notification = null);
    }

    public void StartAutoHide() {
        ApplicationPFX.Instance.Dispatcher.VerifyAccess();
        if (!this.CanAutoHide || this.IsAutoHideActive || this.NotificationManager == null) {
            return;
        }

        this.ctsAutoHide = new CancellationTokenSource();
        this.AutoHideStartTime = DateTime.Now;
        this.IsAutoHideActive = true;
        Task.Run(async () => {
            if (!this.ctsAutoHide.IsCancellationRequested) {
                try {
                    await Task.Delay(this.autoHideDelay, this.ctsAutoHide.Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException) {
                    // someone stopped the auto-hide, or the notification was hidden
                }
            }

            await ApplicationPFX.Instance.Dispatcher.InvokeAsync(() => {
                // Double check whether auto-hide is cancelled
                if (!this.ctsAutoHide.IsCancellationRequested) {
                    this.Hide();
                }

                this.ctsAutoHide.Cancel();
                this.ctsAutoHide.Dispose();
                this.ctsAutoHide = null;
                this.IsAutoHideActive = false;
                if (this.flagRestartAutoHide) {
                    this.flagRestartAutoHide = false;
                    this.StartAutoHide();
                }
            }, token: CancellationToken.None);
        }, CancellationToken.None);
    }

    public void CancelAutoHide() {
        this.ctsAutoHide?.Cancel();
    }

    protected internal virtual void OnShowing() {
        this.StartAutoHide();
    }

    protected internal virtual void OnHidden() {
        this.ctsAutoHide?.Cancel();
    }

    /// <summary>
    /// Adds this notification to the given notification manager. If already shown in the notification manager, the auto-hide is restarted.
    /// If we already exist in another notification manager, we remove ourself from it first before adding to the new one.
    /// </summary>
    /// <param name="notificationManager">The notification manager to add ourself to</param>
    public void Show(NotificationManager notificationManager) {
        ApplicationPFX.Instance.Dispatcher.VerifyAccess();
        ArgumentNullException.ThrowIfNull(notificationManager);
        if (!ReferenceEquals(this.NotificationManager, notificationManager)) {
            this.NotificationManager?.Toasts.Remove(this);
            notificationManager.Toasts.Add(this);
        }
        else if (this.IsAutoHideActive && !this.flagRestartAutoHide) {
            this.flagRestartAutoHide = true;
            this.CancelAutoHide();
        }
    }

    /// <summary>
    /// Hides this notification, removing it from the <see cref="NotificationManager"/>
    /// </summary>
    public void Hide() {
        ApplicationPFX.Instance.Dispatcher.VerifyAccess();
        this.NotificationManager?.Toasts.Remove(this);
    }
}