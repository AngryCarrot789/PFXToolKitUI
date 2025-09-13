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
using PFXToolKitUI.Composition;
using PFXToolKitUI.Utils.Collections.Observable;

namespace PFXToolKitUI.Notifications;

/// <summary>
/// Manages a collection of notification. Typically, once instance exists per window that supports notification
/// </summary>
public class NotificationManager {
    private readonly ObservableList<Notification> notifications;

    /// <summary>
    /// Returns the list of visible notifications
    /// </summary>
    public ReadOnlyObservableList<Notification> Notifications { get; }

    public NotificationManager() {
        this.notifications = new ObservableList<Notification>();
        this.Notifications = new ReadOnlyObservableList<Notification>(this.notifications);
    }

    public static NotificationManager GetInstance(IComponentManager componentManager) {
        return componentManager.GetOrCreateComponent(_ => new NotificationManager());
    }

    public void ShowNotification(Notification entry) {
        if (entry == null)
            throw new ArgumentNullException(nameof(entry), "Cannot show a null entry");
        if (entry.NotificationManager == this)
            throw new InvalidOperationException("Notification already shown");
        if (entry.NotificationManager != null)
            throw new InvalidOperationException("Notification shown in another notification manager");

        entry.NotificationManager = this;
        this.notifications.Add(entry);
        entry.OnShowing();
    }

    public bool HideNotification(Notification entry) {
        if (!ReferenceEquals(entry.NotificationManager, this)) {
            return false;
        }

        int idx = this.notifications.IndexOf(entry);
        Debug.Assert(idx != -1);
        
        entry.OnHidden();
        this.notifications.RemoveAt(idx);
        entry.NotificationManager = null;
        return true;
    }

    public void HideAllNotifications() {
        foreach (Notification t in this.notifications) {
            t.OnHidden();
            t.NotificationManager = null;
        }

        this.notifications.Clear();
    }
}