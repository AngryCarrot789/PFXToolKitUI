//
// Copyright (c) 2024-2025 REghZy
//
// This file is part of FramePFX.
//
// FramePFX is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
//
// FramePFX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
//

namespace PFXToolKitUI.Notifications;

/// <summary>
/// A notification command that invokes a callback lambda, optionally only when context data is available
/// </summary>
public class LambdaNotificationCommand : NotificationCommand {
    private readonly Func<LambdaNotificationCommand, Task> action;
    private readonly bool requireContext;

    public LambdaNotificationCommand(Func<LambdaNotificationCommand, Task> action, bool requireContext = true) : this(null, action, requireContext) {
    }

    public LambdaNotificationCommand(string? text, Func<LambdaNotificationCommand, Task> action, bool requireContext = true) : base(text) {
        this.action = action;
        this.requireContext = requireContext;
    }

    public override Task Execute() {
        return (this.requireContext && this.ContextData == null) 
            ? Task.CompletedTask 
            : this.action(this);
    }
}