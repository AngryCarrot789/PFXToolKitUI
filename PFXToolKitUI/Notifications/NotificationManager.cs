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
using PFXToolKitUI.Utils.Collections.Observable;
using PFXToolKitUI.Utils.Events;

namespace PFXToolKitUI.Notifications;

/// <summary>
/// Manages a collection of notifications
/// </summary>
public class NotificationManager : IComponentManager {
    ComponentStorage IComponentManager.ComponentStorage => field ??= new ComponentStorage(this);
    
    /// <summary>
    /// Gets the list of visible toast notifications shown in the UI
    /// </summary>
    public ObservableList<Notification> Toasts { get; }
    
    /// <summary>
    /// Gets whether any notifications have alert mode currently active
    /// </summary>
    public bool IsAlertActive {
        get => field;
        private set => PropertyHelper.SetAndRaiseINE(ref field, value, this, this.IsAlertActiveChanged);
    }

    public event EventHandler? IsAlertActiveChanged;

    public NotificationManager() {
        this.Toasts = new ObservableList<Notification>();
        this.Toasts.ValidateAdd += (list, e) => {
            foreach (Notification toast in e.Items) {
                if (toast == null)
                    throw new InvalidOperationException("Attempt to show null notification");
                if (toast.NotificationManager == this)
                    throw new InvalidOperationException("Notification already shown");
                if (toast.NotificationManager != null)
                    throw new InvalidOperationException("Notification shown in another notification manager");
            }
        };

        
        this.Toasts.ItemsAdded += (list, e) => {
            foreach (Notification toast in e.Items) {
                toast.NotificationManager = this;
                toast.OnShowing();
            }
            
            this.UpdateIsAlertActive();
        };

        this.Toasts.ItemsRemoved += (list, e) => {
            foreach (Notification toast in e.Items) {
                toast.OnHidden();
                toast.NotificationManager = null;
            }
            
            this.UpdateIsAlertActive();
        };

        this.Toasts.ItemReplaced += (list, e) => {
            e.OldItem.NotificationManager = null;
            e.NewItem.NotificationManager = this;
            this.UpdateIsAlertActive();
        };
    }

    public static NotificationManager GetInstance(IComponentManager componentManager) {
        return componentManager.GetComponent<NotificationManager>();
    }

    internal void InternalOnAlertModeChanged(Notification notification) {
        this.UpdateIsAlertActive();
    }

    private void UpdateIsAlertActive() {
        this.IsAlertActive = this.Toasts.Any(x => x.AlertMode != NotificationAlertMode.None);
    }
}