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
using Avalonia.Media;
using PFXToolKitUI.Avalonia.Utils;
using PFXToolKitUI.Tasks;

namespace PFXToolKitUI.Avalonia.Activities;

/// <summary>
/// A control shown in a window which contains a list of all running activities
/// </summary>
public class ActivityListControl : TemplatedControl {
    public static readonly StyledProperty<IBrush?> HeaderBrushProperty = AvaloniaProperty.Register<ActivityListControl, IBrush?>(nameof(HeaderBrush));
    public static readonly StyledProperty<ActivityManager?> ActivityManagerProperty = AvaloniaProperty.Register<ActivityListControl, ActivityManager?>(nameof(ActivityManager));

    public IBrush? HeaderBrush {
        get => this.GetValue(HeaderBrushProperty);
        set => this.SetValue(HeaderBrushProperty, value);
    }

    public ActivityManager? ActivityManager {
        get => this.GetValue(ActivityManagerProperty);
        set => this.SetValue(ActivityManagerProperty, value);
    }
    
    private ItemsControl? PART_ItemsControl;
    private readonly Stack<ActivityListItem> itemCache = new Stack<ActivityListItem>();
    
    public ActivityListControl() {
    }
    
    static ActivityListControl() {
        ActivityManagerProperty.Changed.AddClassHandler<ActivityListControl, ActivityManager?>((o, e) => o.OnActivityManagerChanged(e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
    }

    private void OnActivityManagerChanged(ActivityManager? oldManager, ActivityManager? newManager) {
        if (oldManager != null) {
            oldManager.TaskStarted -= this.ActivityManagerOnTaskStarted;
            oldManager.TaskCompleted -= this.ActivityManagerOnTaskCompleted;
            if (this.PART_ItemsControl != null) {
                for (int i = this.PART_ItemsControl!.Items.Count - 1; i >= 0; i--) {
                    this.RemoveItem(i);
                }
            }
        }
        
        if (newManager != null) {
            newManager.TaskStarted += this.ActivityManagerOnTaskStarted;
            newManager.TaskCompleted += this.ActivityManagerOnTaskCompleted;
            if (this.PART_ItemsControl != null) {
                int i = 0;
                foreach (ActivityTask task in newManager.ActiveTasks) {
                    this.InsertItem(i++, task);
                }
            }
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.PART_ItemsControl = e.NameScope.GetTemplateChild<ItemsControl>(nameof(this.PART_ItemsControl));
        if (this.ActivityManager is ActivityManager manager) {
            int i = 0;
            foreach (ActivityTask task in manager.ActiveTasks) {
                this.InsertItem(i++, task);
            }
        }
    }
    
    private void ActivityManagerOnTaskStarted(ActivityManager actMan, ActivityTask task, int index) {
        if (this.PART_ItemsControl != null)
            this.InsertItem(index, task);
    }
    
    private void ActivityManagerOnTaskCompleted(ActivityManager actMan, ActivityTask task, int index) {
        if (this.PART_ItemsControl != null)
            this.RemoveItem(index);
    }

    private void InsertItem(int index, ActivityTask task) {
        if (!this.itemCache.TryPop(out ActivityListItem? item))
            item = new ActivityListItem();
        
        this.PART_ItemsControl!.Items.Insert(index, item);
        TemplateUtils.Apply(item);
        item.ActivityTask = task;
    }
    
    private void RemoveItem(int index) {
        ActivityListItem item = (ActivityListItem) this.PART_ItemsControl!.Items[index]!;
        item.ActivityTask = null;
        this.PART_ItemsControl!.Items.RemoveAt(index);
        if (this.itemCache.Count < 16)
            this.itemCache.Push(item);
    }
}