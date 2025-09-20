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
using Avalonia.Interactivity;
using PFXToolKitUI.Avalonia.AvControls;
using PFXToolKitUI.Avalonia.Bindings;
using PFXToolKitUI.Avalonia.ToolTips;
using PFXToolKitUI.Avalonia.Utils;
using PFXToolKitUI.Tasks;
using PFXToolKitUI.Tasks.Pausable;
using PFXToolKitUI.Utils.Commands;

namespace PFXToolKitUI.Avalonia.Activities;

public class ActivityRowControl : TemplatedControl {
    public static readonly StyledProperty<ActivityTask?> ActivityTaskProperty = AvaloniaProperty.Register<ActivityRowControl, ActivityTask?>(nameof(ActivityTask));
    public static readonly StyledProperty<bool> ShowCaptionProperty = AvaloniaProperty.Register<ActivityRowControl, bool>(nameof(ShowCaption), true);

    public ActivityTask? ActivityTask {
        get => this.GetValue(ActivityTaskProperty);
        set => this.SetValue(ActivityTaskProperty, value);
    }

    public bool ShowCaption {
        get => this.GetValue(ShowCaptionProperty);
        set => this.SetValue(ShowCaptionProperty, value);
    }

    private TextBlock? PART_Header;
    private ProgressBar? PART_ProgressBar;
    private TextBlock? PART_Footer;
    private Button? PART_CancelActivityButton;
    private IconButton? PART_PlayPauseButton;
    private readonly AsyncRelayCommand pauseActivityCommand;

    private readonly IBinder<IActivityProgress> binderCaption = new EventUpdateBinder<IActivityProgress>(nameof(IActivityProgress.CaptionChanged), (b) => ((ActivityRowControl) b.Control).PART_Header!.Text = b.Model.Caption);
    private readonly IBinder<IActivityProgress> binderText = new EventUpdateBinder<IActivityProgress>(nameof(IActivityProgress.TextChanged), (b) => ((ActivityRowControl) b.Control).PART_Footer!.Text = b.Model.Text);
    private readonly IBinder<IActivityProgress> binderIsIndeterminate = new EventUpdateBinder<IActivityProgress>(nameof(IActivityProgress.IsIndeterminateChanged), (b) => ((ActivityRowControl) b.Control).PART_ProgressBar!.IsIndeterminate = b.Model.IsIndeterminate);
    private readonly IBinder<CompletionState> binderCompletionValue = new EventUpdateBinder<CompletionState>(nameof(CompletionState.CompletionValueChanged), (b) => ((ActivityRowControl) b.Control).PART_ProgressBar!.Value = b.Model.TotalCompletion);

    public ActivityRowControl() {
        this.pauseActivityCommand = new AsyncRelayCommand(async () => {
            AdvancedPausableTask? task = this.ActivityTask?.PausableTask;
            if (task != null) {
                await task.TogglePaused();
            }
        });
    }

    static ActivityRowControl() {
        ActivityTaskProperty.Changed.AddClassHandler<ActivityRowControl, ActivityTask?>((s, e) => s.OnActivityTaskChanged(e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
        ShowCaptionProperty.Changed.AddClassHandler<ActivityRowControl, bool>((s, e) => {
            if (s.PART_Header != null)
                s.PART_Header.IsVisible = e.NewValue.GetValueOrDefault();
        });
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.PART_Header = e.NameScope.GetTemplateChild<TextBlock>(nameof(this.PART_Header));
        this.PART_Header.IsVisible = this.ShowCaption;
        this.PART_ProgressBar = e.NameScope.GetTemplateChild<ProgressBar>(nameof(this.PART_ProgressBar));
        this.PART_Footer = e.NameScope.GetTemplateChild<TextBlock>(nameof(this.PART_Footer));
        this.PART_CancelActivityButton = e.NameScope.GetTemplateChild<Button>(nameof(this.PART_CancelActivityButton));
        this.PART_CancelActivityButton.Click += this.PART_CancelActivityButtonOnClick;
        this.PART_PlayPauseButton = e.NameScope.GetTemplateChild<IconButton>(nameof(this.PART_PlayPauseButton));
        this.PART_PlayPauseButton.Command = this.pauseActivityCommand;

        this.binderCaption.AttachControl(this);
        this.binderText.AttachControl(this);
        this.binderIsIndeterminate.AttachControl(this);
        this.binderCompletionValue.AttachControl(this);
        this.UpdateCancelButton();
    }

    private void OnActivityTaskChanged(ActivityTask? oldTask, ActivityTask? newTask) {
        if (oldTask != null) {
            oldTask.PausableTaskChanged -= this.OnActivityPausableTaskChanged;
            if (oldTask.PausableTask != null)
                oldTask.PausableTask.PausedStateChanged -= this.OnPausedStateChanged;
        }

        if (newTask != null) {
            this.UpdateCancelButton();
            newTask.PausableTaskChanged += this.OnActivityPausableTaskChanged;
            if (newTask.PausableTask != null)
                newTask.PausableTask.PausedStateChanged += this.OnPausedStateChanged;
        }

        this.binderCaption.SwitchModel(newTask?.Progress);
        this.binderText.SwitchModel(newTask?.Progress);
        this.binderIsIndeterminate.SwitchModel(newTask?.Progress);
        this.binderCompletionValue.SwitchModel(newTask?.Progress.CompletionState);

        this.UpdatePauseContinueButton(newTask?.PausableTask);
    }

    private void OnActivityPausableTaskChanged(ActivityTask sender, AdvancedPausableTask? oldTask, AdvancedPausableTask? newTask) {
        if (oldTask != null)
            oldTask.PausedStateChanged -= this.OnPausedStateChanged;
        if (newTask != null)
            newTask.PausedStateChanged += this.OnPausedStateChanged;

        ApplicationPFX.Instance.Dispatcher.InvokeAsync(() => this.UpdatePauseContinueButton(newTask));
    }

    private void UpdateCancelButton() {
        if (this.PART_CancelActivityButton != null) {
            ActivityTask? task = this.ActivityTask;
            this.PART_CancelActivityButton!.IsEnabled = task?.IsDirectlyCancellable ?? false;
            this.PART_CancelActivityButton!.IsVisible = task?.IsDirectlyCancellable ?? false;
        }
    }

    private void PART_CancelActivityButtonOnClick(object? sender, RoutedEventArgs e) {
        this.PART_CancelActivityButton!.IsEnabled = false;
        this.ActivityTask?.TryCancel();
    }

    private Task OnPausedStateChanged(AdvancedPausableTask task) {
        return ApplicationPFX.Instance.Dispatcher.InvokeAsync(() => {
            this.UpdatePauseContinueButton(task);
        });
    }

    private void UpdatePauseContinueButton(AdvancedPausableTask? task) {
        if (this.PART_PlayPauseButton == null) {
            return;
        }

        if (task == null) {
            this.PART_PlayPauseButton!.IsVisible = false;
        }
        else {
            this.PART_PlayPauseButton!.IsVisible = true;
            this.PART_PlayPauseButton.Icon = task.IsPaused ? ActivityStatusBarControl.ContinueActivityIcon : ActivityStatusBarControl.PauseActivityIcon;
            ToolTipEx.SetTip(this.PART_PlayPauseButton, task.IsPaused ? "Continue the task" : "Pause the task");
        }
    }
}