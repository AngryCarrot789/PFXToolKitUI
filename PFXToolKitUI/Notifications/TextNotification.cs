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

using PFXToolKitUI.Icons;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.Notifications;

public delegate void TextNotificationIconChangedEventHandler(TextNotification sender, Icon? oldIcon, Icon? newIcon);

/// <summary>
/// A notification that displays text in the body of the notification
/// </summary>
public class TextNotification : Notification {
    private string? text;
    private Icon? icon;

    /// <summary>
    /// Gets or sets the main body text for this notification
    /// </summary>
    public string? Text {
        get => this.text;
        set => PropertyHelper.SetAndRaiseINE(ref this.text, value, this, static t => t.TextChanged?.Invoke(t));
    }

    /// <summary>
    /// Gets or sets the icon to display in this notification. This will be on the left side of the notification, just like a message box.
    /// </summary>
    public Icon? Icon {
        get => this.icon;
        set => PropertyHelper.SetAndRaiseINE(ref this.icon, value, this, static (t, o, n) => t.IconChanged?.Invoke(t, o, n));
    }

    public event NotificationEventHandler? TextChanged;
    public event TextNotificationIconChangedEventHandler? IconChanged;

    public TextNotification() {
    }
}