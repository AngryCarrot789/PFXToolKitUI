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
using PFXToolKitUI.Avalonia.AvControls;
using PFXToolKitUI.Avalonia.Utils;
using PFXToolKitUI.Tasks;
using PFXToolKitUI.Tasks.Pausable;
using PFXToolKitUI.Utils.Commands;

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
    private IconButton? PART_PlayPauseButton;
    private readonly AsyncRelayCommand pauseActivityCommand;

    public ActivityListItem() {
        this.pauseActivityCommand = new AsyncRelayCommand(async () => {
            AdvancedPausableTask? task = this.ActivityTask?.PausableTask;
            if (task != null) {
                await task.TogglePaused();
            }
        });
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.PART_Header = e.NameScope.GetTemplateChild<TextBlock>(nameof(this.PART_Header));
        this.PART_ProgressBar = e.NameScope.GetTemplateChild<ProgressBar>(nameof(this.PART_ProgressBar));
        this.PART_Footer = e.NameScope.GetTemplateChild<TextBlock>(nameof(this.PART_Footer));
        this.PART_CancelActivityButton = e.NameScope.GetTemplateChild<Button>(nameof(this.PART_CancelActivityButton));
        this.PART_CancelActivityButton.Click += this.PART_CancelActivityButtonOnClick;
        this.PART_PlayPauseButton = e.NameScope.GetTemplateChild<IconButton>(nameof(this.PART_PlayPauseButton));
        this.PART_PlayPauseButton.Command = this.pauseActivityCommand;
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
        
        if (task.PausableTask != null)
            task.PausableTask.PausedStateChanged += this.OnPausedStateChanged;
        this.UpdatePauseContinueButton(task.PausableTask);
    }

    public void Disconnect() {
        ActivityTask task = this.ActivityTask!;
        task.Progress.CaptionChanged -= this.OnActivityTaskCaptionChanged;
        task.Progress.TextChanged -= this.OnActivityTaskTextChanged;
        task.Progress.IsIndeterminateChanged -= this.OnActivityTaskIndeterminateChanged;
        task.Progress.CompletionState.CompletionValueChanged -= this.OnActivityTaskCompletionValueChanged;
        
        if (task.PausableTask != null)
            task.PausableTask.PausedStateChanged -= this.OnPausedStateChanged;
        this.UpdatePauseContinueButton(null);
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

    private Task OnPausedStateChanged(AdvancedPausableTask task) {
        return ApplicationPFX.Instance.Dispatcher.InvokeAsync(() => {
            this.UpdatePauseContinueButton(task);
        });
    }

    private void UpdatePauseContinueButton(AdvancedPausableTask? task) {
        if (task == null) {
            this.PART_PlayPauseButton!.IsVisible = false;
        }
        else {
            this.PART_PlayPauseButton!.IsVisible = true;
            this.PART_PlayPauseButton.Icon = task.IsPaused ? ActivityStatusBarControl.ContinueActivityIcon : ActivityStatusBarControl.PauseActivityIcon;
            ToolTip.SetTip(this.PART_PlayPauseButton, task.IsPaused ? "Continue the task" : "Pause the task");
        }
    }
}