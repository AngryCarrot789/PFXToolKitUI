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

namespace PFXToolKitUI.Notifications;

/// <summary>
/// Manages a collection of notifications
/// </summary>
public class NotificationManager : IComponentManager {
    private readonly ComponentStorage myComponentStorage;

    ComponentStorage IComponentManager.ComponentStorage => this.myComponentStorage;
    
    /// <summary>
    /// Gets the list of visible toast notifications shown in the UI
    /// </summary>
    public ObservableList<Notification> Toasts { get; }

    public NotificationManager() {
        this.myComponentStorage = new ComponentStorage(this);
        this.Toasts = new ObservableList<Notification>();
        this.Toasts.BeforeItemsAdded += (list, index, items) => {
            foreach (Notification toast in items) {
                if (toast == null)
                    throw new InvalidOperationException("Attempt to show null notification");
                if (toast.NotificationManager == this)
                    throw new InvalidOperationException("Notification already shown");
                if (toast.NotificationManager != null)
                    throw new InvalidOperationException("Notification shown in another notification manager");
            }
        };

        this.Toasts.ItemsAdded += (list, index, items) => {
            foreach (Notification toast in items) {
                toast.NotificationManager = this;
                toast.OnShowing();
            }
        };

        this.Toasts.ItemsRemoved += (list, index, items) => {
            foreach (Notification toast in items) {
                toast.OnHidden();
                toast.NotificationManager = null;
            }
        };

        this.Toasts.ItemReplaced += (list, index, oldItem, newItem) => {
            oldItem.NotificationManager = null;
            newItem.NotificationManager = this;
        };
    }

    public static NotificationManager GetInstance(IComponentManager componentManager) {
        return componentManager.GetComponent<NotificationManager>();
    }
}