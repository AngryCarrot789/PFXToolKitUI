// 
// Copyright (c) 2024-2025 REghZy
// 
// This file is part of FramePFX.
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
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
// 

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using PFXToolKitUI.Avalonia.Utils;
using PFXToolKitUI.Tasks;

namespace PFXToolKitUI.Avalonia.Activities;

public class ActivityListItem : TemplatedControl {
    public static readonly StyledProperty<ActivityTask?> ActivityTaskProperty = AvaloniaProperty.Register<ActivityListItem, ActivityTask?>(nameof(ActivityTask));

    public ActivityTask? ActivityTask {
        get => this.GetValue(ActivityTaskProperty);
        set => this.SetValue(ActivityTaskProperty, value);
    }

    private TextBlock? PART_Header;
    private ProgressBar? PART_ProgressBar;
    private TextBlock? PART_Footer;
    private Button? PART_CancelActivityButton;

    public ActivityListItem() {
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.PART_Header = e.NameScope.GetTemplateChild<TextBlock>(nameof(this.PART_Header));
        this.PART_ProgressBar = e.NameScope.GetTemplateChild<ProgressBar>(nameof(this.PART_ProgressBar));
        this.PART_Footer = e.NameScope.GetTemplateChild<TextBlock>(nameof(this.PART_Footer));
        this.PART_CancelActivityButton = e.NameScope.GetTemplateChild<Button>(nameof(this.PART_CancelActivityButton));
        this.PART_CancelActivityButton.Click += this.PART_CancelActivityButtonOnClick;
    }

    public void Connect(ActivityTask task) {
        this.ActivityTask = task;
        this.PART_CancelActivityButton!.IsEnabled = task.IsDirectlyCancellable;
        this.PART_CancelActivityButton!.IsVisible = task.IsDirectlyCancellable;
        task.Progress.CaptionChanged += this.OnActivityTaskCaptionChanged;
        task.Progress.TextChanged += this.OnActivityTaskTextChanged;
        task.Progress.IsIndeterminateChanged += this.OnActivityTaskIndeterminateChanged;
        task.Progress.CompletionState.CompletionValueChanged += this.OnActivityTaskCompletionValueChanged;

        this.OnActivityTaskCaptionChanged(task.Progress);
        this.OnActivityTaskTextChanged(task.Progress);
        this.OnActivityTaskIndeterminateChanged(task.Progress);
        this.OnActivityTaskCompletionValueChanged(task.Progress.CompletionState);
    }

    public void Disconnect() {
        ActivityTask task = this.ActivityTask!;
        task.Progress.CaptionChanged -= this.OnActivityTaskCaptionChanged;
        task.Progress.TextChanged -= this.OnActivityTaskTextChanged;
        task.Progress.IsIndeterminateChanged -= this.OnActivityTaskIndeterminateChanged;
        task.Progress.CompletionState.CompletionValueChanged -= this.OnActivityTaskCompletionValueChanged;
    }

    private void PART_CancelActivityButtonOnClick(object? sender, RoutedEventArgs e) {
        this.PART_CancelActivityButton!.IsEnabled = false;
        this.ActivityTask?.TryCancel();
    }
    
    private void OnActivityTaskCaptionChanged(IActivityProgress tracker) {
        ApplicationPFX.Instance.Dispatcher.Invoke(() => this.PART_Header!.Text = tracker.Caption, DispatchPriority.Loaded);
    }

    private void OnActivityTaskTextChanged(IActivityProgress tracker) {
        ApplicationPFX.Instance.Dispatcher.Invoke(() => this.PART_Footer!.Text = tracker.Text, DispatchPriority.Loaded);
    }

    private void OnActivityTaskIndeterminateChanged(IActivityProgress tracker) {
        ApplicationPFX.Instance.Dispatcher.Invoke(() => this.PART_ProgressBar!.IsIndeterminate = tracker.IsIndeterminate, DispatchPriority.Loaded);
    }

    private void OnActivityTaskCompletionValueChanged(CompletionState state) {
        ApplicationPFX.Instance.Dispatcher.Invoke(() => this.PART_ProgressBar!.Value = state.TotalCompletion, DispatchPriority.Loaded);
    }
}