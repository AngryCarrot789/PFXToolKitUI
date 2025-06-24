// 
// Copyright (c) 2024-2025 REghZy
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

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using PFXToolKitUI.Avalonia.AvControls.ListBoxes;
using PFXToolKitUI.Notifications;

namespace PFXToolKitUI.Avalonia.Notifications;

public class NotificationListBox : ModelBasedListBox<Notification> {
    public static readonly StyledProperty<NotificationManager?> NotificationManagerProperty = AvaloniaProperty.Register<NotificationListBox, NotificationManager?>(nameof(NotificationManager));

    public NotificationManager? NotificationManager {
        get => this.GetValue(NotificationManagerProperty);
        set => this.SetValue(NotificationManagerProperty, value);
    }
    
    public NotificationListBox() : base(8) {
        this.MinHeight = 1;
        ScrollViewer.SetVerticalScrollBarVisibility(this, ScrollBarVisibility.Disabled);
    }

    static NotificationListBox() {
        NotificationManagerProperty.Changed.AddClassHandler<NotificationListBox, NotificationManager?>((s, e) => s.OnNotificationManagerChanged(e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
    }
    
    private void OnNotificationManagerChanged(NotificationManager? oldSeq, NotificationManager? newSeq) {
        this.SetItemsSource(newSeq?.Notifications);
    }

    protected override ModelBasedListBoxItem<Notification> CreateItem() => new NotificationListBoxItem();
}