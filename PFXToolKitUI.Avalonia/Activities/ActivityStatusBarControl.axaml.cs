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

using System.Collections.Specialized;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using PFXToolKitUI.Activities;
using PFXToolKitUI.Activities.Pausable;
using PFXToolKitUI.Avalonia.Interactivity;
using PFXToolKitUI.Avalonia.ToolTips;
using PFXToolKitUI.CommandSystem;
using PFXToolKitUI.Services.Messaging;
using PFXToolKitUI.Utils;
using PFXToolKitUI.Utils.Collections.Observable;
using PFXToolKitUI.Utils.Commands;

namespace PFXToolKitUI.Avalonia.Activities;

/// <summary>
/// A control that typically sits in a status bar that represents the oldest
/// activity currently running. Optionally supports showing a list of active
/// activities when clicked
/// </summary>
public partial class ActivityStatusBarControl : UserControl {
    public static readonly StyledProperty<ActivityTask?> ActivityTaskProperty = AvaloniaProperty.Register<ActivityStatusBarControl, ActivityTask?>(nameof(ActivityTask));

    public ActivityTask? ActivityTask {
        get => this.GetValue(ActivityTaskProperty);
        set => this.SetValue(ActivityTaskProperty, value);
    }

    private readonly AsyncRelayCommand pauseActivityCommand;
    private bool isExecutingShowActivityListCmd;

    public ActivityStatusBarControl() {
        this.InitializeComponent();
        this.PART_PlayPauseButton.Command = this.pauseActivityCommand = new AsyncRelayCommand(async () => {
            AdvancedPausableTask? task = this.ActivityTask?.PausableTask;
            if (task != null) {
                await task.TogglePaused();
            }
        });
    }

    static ActivityStatusBarControl() {
        ActivityTaskProperty.Changed.AddClassHandler<ActivityStatusBarControl, ActivityTask?>((c, e) => c.OnActivityChanged(e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
    }

    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);

        ActivityManager am = ActivityManager.Instance;
        am.BackgroundTasks.CollectionChanged += this.OnBackgroundTasksCollectionChanged;
        if (am.BackgroundTasks.Count > 0) {
            this.ActivityTask = am.BackgroundTasks[0];
        }
    }

    protected override void OnUnloaded(RoutedEventArgs e) {
        base.OnUnloaded(e);

        ActivityManager am = ActivityManager.Instance;
        am.BackgroundTasks.CollectionChanged -= this.OnBackgroundTasksCollectionChanged;
        this.ActivityTask = null;
    }

    private void OnBackgroundTasksCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
        ReadOnlyObservableList<ActivityTask> bgTasks = (ReadOnlyObservableList<ActivityTask>) sender!;
        this.ActivityTask = bgTasks.Count > 0 ? bgTasks[bgTasks.Count - 1] : null;
    }

    private void OnActivityChanged(ActivityTask? oldActivity, ActivityTask? newActivity) {
        IActivityProgress? prog = null;
        if (oldActivity != null) {
            prog = oldActivity.Progress;
            prog.TextChanged -= this.OnActivityTaskTextChanged;
            prog.CompletionState.CompletionValueChanged -= this.OnPrimaryActionCompletionValueChanged;
            prog.IsIndeterminateChanged -= this.OnActivityTaskIndeterminateChanged;

            if (oldActivity.IsDirectlyCancellable && !Design.IsDesignMode)
                this.PART_CancelActivityButton.IsVisible = false;

            oldActivity.PausableTaskChanged -= this.OnCurrentActivityPausableTaskChanged;
            if (oldActivity.PausableTask != null)
                oldActivity.PausableTask.PausedStateChanged -= this.OnPausedStateChanged;

            prog = null;
        }

        this.ActivityTask = newActivity;
        if (newActivity != null) {
            prog = newActivity.Progress;
            prog.TextChanged += this.OnActivityTaskTextChanged;
            prog.CompletionState.CompletionValueChanged += this.OnPrimaryActionCompletionValueChanged;
            prog.IsIndeterminateChanged += this.OnActivityTaskIndeterminateChanged;
            if (newActivity.IsDirectlyCancellable && !Design.IsDesignMode)
                this.PART_CancelActivityButton.IsVisible = true;

            if (!Design.IsDesignMode)
                this.IsVisible = true;

            newActivity.PausableTaskChanged += this.OnCurrentActivityPausableTaskChanged;
            if (newActivity.PausableTask != null)
                newActivity.PausableTask.PausedStateChanged += this.OnPausedStateChanged;
        }
        else {
            if (!Design.IsDesignMode)
                this.IsVisible = false;
        }

        this.UpdatePauseContinueButton(newActivity?.PausableTask);
        this.PART_CancelActivityButton.IsEnabled = true;
        this.OnActivityTaskTextChanged(prog);
        this.OnPrimaryActionCompletionValueChanged(prog?.CompletionState);
        this.OnActivityTaskIndeterminateChanged(prog);
    }

    private void OnCurrentActivityPausableTaskChanged(ActivityTask sender, AdvancedPausableTask? oldTask, AdvancedPausableTask? newTask) {
        if (oldTask != null)
            oldTask.PausedStateChanged -= this.OnPausedStateChanged;
        if (newTask != null)
            newTask.PausedStateChanged += this.OnPausedStateChanged;

        ApplicationPFX.Instance.Dispatcher.Post(() => this.UpdatePauseContinueButton(newTask));
    }

    private void OnActivityTaskTextChanged(IActivityProgress? tracker) {
        this.PART_TaskBodyText.Text = tracker?.Text ?? "";
    }

    private void OnPrimaryActionCompletionValueChanged(CompletionState? state) {
        this.PART_ActiveBgProgress.Value = state?.TotalCompletion ?? 0.0;
    }

    private void OnActivityTaskIndeterminateChanged(IActivityProgress? tracker) {
        this.PART_ActiveBgProgress.IsIndeterminate = tracker?.IsIndeterminate ?? false;
    }

    private void PART_CancelActivityButton_OnClick(object? sender, RoutedEventArgs e) {
        ((Button) sender!).IsEnabled = false;
        this.ActivityTask?.TryCancel();
    }

    private void InputElement_OnPointerReleased(object? sender, PointerReleasedEventArgs e) {
        if (!e.Handled && this.IsPointerOver && !this.isExecutingShowActivityListCmd) {
            ShowActivityListAsync();
            return;

            async void ShowActivityListAsync() {
                try {
                    this.isExecutingShowActivityListCmd = true;
                    await CommandManager.Instance.Execute("commands.pfx.ShowActivityListCommand", DataManager.GetFullContextData(this), null, null);
                }
                catch (Exception exception) when (!Debugger.IsAttached) {
                    await IMessageDialogService.Instance.ShowExceptionMessage("Command Error", exception);
                }
                finally {
                    this.isExecutingShowActivityListCmd = false;
                }
            }
        }
    }


    private Task OnPausedStateChanged(AdvancedPausableTask task) {
        return ApplicationPFX.Instance.Dispatcher.InvokeAsync(() => {
            this.UpdatePauseContinueButton(task);
        });
    }

    private void UpdatePauseContinueButton(AdvancedPausableTask? task) {
        if (task == null) {
            this.PART_PlayPauseButton.IsVisible = false;
        }
        else {
            this.PART_PlayPauseButton.IsVisible = true;
            this.PART_PlayPauseButton.Icon = task.IsPaused ? StandardIcons.SmallContinueActivityIconColourful : StandardIcons.PauseActivityIcon;
            ToolTipEx.SetTip(this.PART_PlayPauseButton, task.IsPaused ? "Continue the task" : "Pause the task");
        }
    }
}