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
using Avalonia.Controls.Primitives;
using PFXToolKitUI.Avalonia.Utils;
using PFXToolKitUI.Tasks;

namespace PFXToolKitUI.Avalonia.Activities;

public class ActivityListItem : TemplatedControl {
    public static readonly StyledProperty<ActivityTask?> ActivityTaskProperty = AvaloniaProperty.Register<ActivityListItem, ActivityTask?>(nameof(ActivityTask));
    public static readonly StyledProperty<bool> ShowCaptionProperty = AvaloniaProperty.Register<ActivityListItem, bool>(nameof(ShowCaption), true);

    public ActivityTask? ActivityTask {
        get => this.GetValue(ActivityTaskProperty);
        set => this.SetValue(ActivityTaskProperty, value);
    }

    public bool ShowCaption {
        get => this.GetValue(ShowCaptionProperty);
        set => this.SetValue(ShowCaptionProperty, value);
    }

    private ActivityRowControl? PART_Row;

    public ActivityListItem() {
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.PART_Row = e.NameScope.GetTemplateChild<ActivityRowControl>(nameof(this.PART_Row));
    }
}