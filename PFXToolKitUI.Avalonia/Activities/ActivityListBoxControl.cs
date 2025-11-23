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

using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using PFXToolKitUI.Activities;
using PFXToolKitUI.Avalonia.Utils;
using PFXToolKitUI.EventHelpers;
using PFXToolKitUI.Utils;
using PFXToolKitUI.Utils.Collections.Observable;

namespace PFXToolKitUI.Avalonia.Activities;

/// <summary>
/// A control shown in a window which contains a list of all running activities
/// </summary>
public class ActivityListBoxControl : TemplatedControl {
    public static readonly StyledProperty<IBrush?> HeaderBrushProperty = AvaloniaProperty.Register<ActivityListBoxControl, IBrush?>(nameof(HeaderBrush));
    public static readonly StyledProperty<ActivityManager?> ActivityManagerProperty = AvaloniaProperty.Register<ActivityListBoxControl, ActivityManager?>(nameof(ActivityManager));

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
    private ObservableItemProcessorIndexing<ActivityTask>? backgroundActivityListProcessor;

    private readonly LazyHelper2<ActivityManager, ActivityListBoxControl> lazyProcessor;

    public ActivityListBoxControl() {
        this.lazyProcessor = new LazyHelper2<ActivityManager, ActivityListBoxControl>(static (actMan, self, hasBoth) => {
            if (hasBoth) {
                Debug.Assert(self.backgroundActivityListProcessor == null);
                self.backgroundActivityListProcessor = ObservableItemProcessor.MakeIndexable(
                    actMan.BackgroundTasks,
                    (_, index, item) => self.InsertItem(index, item),
                    (_, index, _) => self.RemoveItem(index),
                    (_, oldIdx, newIdx, item) => {
                        self.RemoveItem(oldIdx);
                        self.InsertItem(newIdx, item);
                    }
                );

                self.backgroundActivityListProcessor.AddExistingItems();
            }
            else {
                self.backgroundActivityListProcessor!.RemoveExistingItems();
                self.backgroundActivityListProcessor!.Dispose();
                self.backgroundActivityListProcessor = null;
            }
        });
    }

    static ActivityListBoxControl() {
        ActivityManagerProperty.Changed.AddClassHandler<ActivityListBoxControl, ActivityManager?>((o, e) => o.OnActivityManagerChanged(e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
    }

    private void OnActivityManagerChanged(ActivityManager? oldManager, ActivityManager? newManager) {
        this.lazyProcessor.Value1 = newManager != null ? Optionals.Of(newManager) : default;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.PART_ItemsControl = e.NameScope.GetTemplateChild<ItemsControl>(nameof(this.PART_ItemsControl));
        this.lazyProcessor.Value2 = this;
    }

    public void InsertItem(int index, ActivityTask task) {
        if (!this.itemCache.TryPop(out ActivityListItem? item))
            item = new ActivityListItem();

        this.PART_ItemsControl!.Items.Insert(index, item);
        TemplateUtils.Apply(item);
        item.ActivityTask = task;
    }

    public void RemoveItem(int index) {
        ActivityListItem item = (ActivityListItem) this.PART_ItemsControl!.Items[index]!;
        item.ActivityTask = null;
        this.PART_ItemsControl!.Items.RemoveAt(index);
        if (this.itemCache.Count < 16)
            this.itemCache.Push(item);
    }
}