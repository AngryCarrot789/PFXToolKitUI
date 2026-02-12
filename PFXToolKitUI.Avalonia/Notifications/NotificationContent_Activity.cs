// 
// Copyright (c) 2024-2026 REghZy
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

using PFXToolKitUI.Avalonia.Activities;
using PFXToolKitUI.Notifications;

namespace PFXToolKitUI.Avalonia.Notifications;

internal class NotificationContent_Activity : ActivityListItem, INotificationContent {
    protected override Type StyleKeyOverride => typeof(ActivityListItem);

    private readonly ActivityNotification notification;

    public NotificationContent_Activity(ActivityNotification notification) {
        this.ShowCaption = false;

        this.notification = notification;
        if (!this.notification.ActivityTask.IsCompleted) {
            this.notification.ActivityTask.IsCompletedChanged += this.OnIsCompletedChanged;
        }
    }

    private void OnIsCompletedChanged(object? o, EventArgs e) {
        this.notification.ActivityTask.IsCompletedChanged -= this.OnIsCompletedChanged;
        this.notification.Hide();
    }

    public void OnShown() {
        this.ActivityTask = this.notification.ActivityTask;
    }

    public void OnHidden() {
        this.ActivityTask = null;
    }
}