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

using PFXToolKitUI.Tasks;

namespace PFXToolKitUI.Notifications;

public class ActivityNotification : Notification {
    public ActivityTask ActivityTask { get; }

    public ActivityNotification(ActivityTask activityTask) {
        this.ActivityTask = activityTask;
        this.Caption = activityTask.Progress.Caption;
        this.CanAutoHide = false;
    }

    protected internal override void OnShowing() {
        base.OnShowing();
        this.ActivityTask.Progress.CaptionChanged += this.ProgressOnCaptionChanged;
    }

    protected internal override void OnHidden() {
        base.OnHidden();
        this.ActivityTask.Progress.CaptionChanged -= this.ProgressOnCaptionChanged;
    }

    private void ProgressOnCaptionChanged(IActivityProgress tracker) {
        this.Caption = tracker.Caption;
    }
}