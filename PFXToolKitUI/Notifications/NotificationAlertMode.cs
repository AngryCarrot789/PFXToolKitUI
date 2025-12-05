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

namespace PFXToolKitUI.Notifications;

/// <summary>
/// An alert state for a notification, e.g. something went wrong, so the alert state will be set to pull the user's attention
/// </summary>
public enum NotificationAlertMode {
    /// <summary>
    /// No alert
    /// </summary>
    None,
    /// <summary>
    /// Continue alert until user interacts with the notification (e.g. cursor over)
    /// </summary>
    UntilUserInteraction,
    /// <summary>
    /// Alert runs forever until manually disabled (e.g. set to <see cref="None"/>) or the notification is hidden
    /// </summary>
    Forever
}