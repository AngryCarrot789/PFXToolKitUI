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
using PFXToolKitUI.Utils.Collections.Observable;

namespace PFXToolKitUI.Notifications;

/// <summary>
/// Manages a collection of notification. Typically once instance is present per window that supports notification
/// </summary>
public class NotificationManager {
    private readonly ObservableList<Notification> notifications;

    /// <summary>
    /// Gets our notifications
    /// </summary>
    public ReadOnlyObservableList<Notification> Notifications { get; }

    public NotificationManager() {
        this.notifications = new ObservableList<Notification>();
        this.Notifications = new ReadOnlyObservableList<Notification>(this.notifications);
    }

    public void AddNotification(Notification entry) => this.InsertNotification(this.notifications.Count, entry);

    public void InsertNotification(int index, Notification entry) {
        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(index), "Negative indices not allowed");
        if (index > this.notifications.Count)
            throw new ArgumentOutOfRangeException(nameof(index), $"Index is beyond the range of this list: {index} > count({this.notifications.Count})");

        if (entry == null)
            throw new ArgumentNullException(nameof(entry), "Cannot add a null entry");
        if (entry.NotificationManager == this)
            throw new InvalidOperationException("Entry already exists in this entry. It must be removed first");
        if (entry.NotificationManager != null)
            throw new InvalidOperationException("Entry already exists in another container. It must be removed first");

        entry.NotificationManager = this;
        this.notifications.Insert(index, entry);
        entry.OnShowing();
    }

    public void AddNotifications(IEnumerable<Notification> layers) {
        foreach (Notification entry in layers) {
            this.AddNotification(entry);
        }
    }

    public bool RemoveNotification(Notification entry) {
        if (!ReferenceEquals(entry.NotificationManager, this)) {
            return false;
        }

        int idx = this.IndexOf(entry);
        Debug.Assert(idx != -1);
        this.RemoveNotificationAt(idx);

        Debug.Assert(entry.NotificationManager != this, "Entry parent not updated, still ourself");
        Debug.Assert(entry.NotificationManager == null, "Entry parent not updated to null");
        return true;
    }

    public void RemoveNotificationAt(int index) {
        Notification entry = this.notifications[index];
        this.notifications.RemoveAt(index);
        entry.OnHidden();
        entry.NotificationManager = null;
    }

    public void RemoveNotifications(IEnumerable<Notification> entries) {
        foreach (Notification entry in entries) {
            this.RemoveNotification(entry);
        }
    }

    public void ClearNotifications() {
        foreach (Notification t in this.notifications) {
            t.NotificationManager = null;
            t.OnHidden();
        }

        this.notifications.Clear();
    }

    public int IndexOf(Notification entry) {
        return ReferenceEquals(entry.NotificationManager, this) ? this.notifications.IndexOf(entry) : -1;
    }

    public bool Contains(Notification entry) {
        return this.IndexOf(entry) != -1;
    }
}