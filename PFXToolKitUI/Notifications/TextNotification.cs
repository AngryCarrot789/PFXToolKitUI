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
using PFXToolKitUI.Utils.Events;

namespace PFXToolKitUI.Notifications;

/// <summary>
/// A notification that displays text in the body of the notification
/// </summary>
public class TextNotification : Notification {
    /// <summary>
    /// Gets or sets the main body text for this notification
    /// </summary>
    public string? Text {
        get => field;
        set => PropertyHelper.SetAndRaiseINE(ref field, value, this, this.TextChanged);
    }

    /// <summary>
    /// Gets or sets the icon to display in this notification. This will be on the left side of the notification, just like a message box.
    /// </summary>
    public Icon? Icon {
        get => field;
        set => PropertyHelper.SetAndRaiseINE(ref field, value, this, this.IconChanged);
    }

    public event EventHandler? TextChanged;
    public event EventHandler<ValueChangedEventArgs<Icon?>>? IconChanged;

    public TextNotification() {
    }
}