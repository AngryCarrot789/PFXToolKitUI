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

using System.Runtime.ExceptionServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using PFXToolKitUI.Avalonia.Interactivity;
using PFXToolKitUI.Avalonia.ToolTips;
using PFXToolKitUI.CommandSystem;
using PFXToolKitUI.Icons;
using PFXToolKitUI.Tasks;
using PFXToolKitUI.Tasks.Pausable;
using PFXToolKitUI.Themes;
using PFXToolKitUI.Utils.Commands;

namespace PFXToolKitUI.Avalonia.Activities;

/// <summary>
/// A control that typically sits in a status bar that represents the oldest
/// activity currently running. Optionally supports showing a list of active
/// activities when clicked
/// </summary>
public partial class ActivityStatusBarControl : UserControl {
    public static readonly Icon CancelIcon =
        IconManager.Instance.RegisterGeometryIcon(
            nameof(CancelIcon),
            [
                new GeometryEntry("m.0005 1.9789q-0-.3411.2388-.5798l1.1599-1.1598q.2388-.2388.5799-.2388.3411-0 .5799.2388l2.5077 2.507 2.5077-2.507q.2388-.2388.5799-.2388.3411-0 .5799.2388l1.1599 1.1598Q10.1336 1.6378 10.1336 1.9789t-.2388.5799l-2.5077 2.507L9.8949 7.5726Q10.1337 7.8114 10.1337 8.1524t-.2388.5799l-1.1599 1.1598q-.2388.2386-.58.2387-.3411 0-.58-.2387l-2.5077-2.507-2.5077 2.507q-.2388.2386-.58.2387-.3411 0-.58-.2387l-1.1599-1.1598q-.2388-.2388-.2388-.5799t.2388-.5798L2.7469 5.0658.2392 2.5588q-.2388-.2388-.2388-.5799z", BrushManager.Instance.GetDynamicThemeBrush("ABrush.Glyph.Static")),
            ], 
            stretch: StretchMode.Uniform);
    
    public static readonly Icon CancelIconInverted =
        IconManager.Instance.RegisterGeometryIcon(
            nameof(CancelIconInverted),
            [
                new GeometryEntry("m.0005 1.9789q-0-.3411.2388-.5798l1.1599-1.1598q.2388-.2388.5799-.2388.3411-0 .5799.2388l2.5077 2.507 2.5077-2.507q.2388-.2388.5799-.2388.3411-0 .5799.2388l1.1599 1.1598Q10.1336 1.6378 10.1336 1.9789t-.2388.5799l-2.5077 2.507L9.8949 7.5726Q10.1337 7.8114 10.1337 8.1524t-.2388.5799l-1.1599 1.1598q-.2388.2386-.58.2387-.3411 0-.58-.2387l-2.5077-2.507-2.5077 2.507q-.2388.2386-.58.2387-.3411 0-.58-.2387l-1.1599-1.1598q-.2388-.2388-.2388-.5799t.2388-.5798L2.7469 5.0658.2392 2.5588q-.2388-.2388-.2388-.5799z", BrushManager.Instance.GetDynamicThemeBrush("ABrush.Tone0.Background.Static")),
            ], 
            stretch: StretchMode.Uniform);
    
    public static readonly Icon ContinueActivityIcon =
        IconManager.Instance.RegisterGeometryIcon(
            nameof(ContinueActivityIcon),
            [
                new GeometryEntry("M5.4947 2.5732 1.5008.1422C.8321-.2646 0 .25 0 1.0689L0 5.9309C0 6.7509.8321 7.2644 1.5008 6.8577L5.4947 4.4277C6.1684 4.0177 6.1684 2.9832 5.4947 2.5732", BrushManager.Instance.GetDynamicThemeBrush("ABrush.Glyph.Static")),
            ], 
            stretch: StretchMode.Uniform);
    
    public static readonly Icon PauseActivityIcon =
        IconManager.Instance.RegisterGeometryIcon(
            nameof(PauseActivityIcon),
            [
                new GeometryEntry("M0 8 2 8 2 0 0 0 0 8ZM4 8 6 8 6 0 4 0 4 8Z", BrushManager.Instance.GetDynamicThemeBrush("ABrush.Glyph.Static")),
            ], 
            stretch: StretchMode.Uniform);
    
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
        am.TaskStarted += this.OnTaskStarted;
        am.TaskCompleted += this.OnTaskCompleted;
        if (am.ActiveTasks.Count > 0) {
            this.ActivityTask = am.ActiveTasks[0];
        }
    }

    protected override void OnUnloaded(RoutedEventArgs e) {
        base.OnUnloaded(e);
        
        ActivityManager am = ActivityManager.Instance;
        am.TaskStarted -= this.OnTaskStarted;
        am.TaskCompleted -= this.OnTaskCompleted;
        this.ActivityTask = null;
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

        ApplicationPFX.Instance.Dispatcher.InvokeAsync(() => this.UpdatePauseContinueButton(newTask));
    }

    private void OnTaskStarted(ActivityManager manager, ActivityTask task, int index) {
        if ((this.ActivityTask == null || this.ActivityTask.IsCompleted)) {
            this.ActivityTask = task;
        }
    }

    private void OnTaskCompleted(ActivityManager manager, ActivityTask task, int index) {
        if ((task == this.ActivityTask || this.ActivityTask == null || this.ActivityTask.IsCompleted)) {
            this.ActivityTask = manager.ActiveTasks.Count > 0 ? manager.ActiveTasks[0] : null;
        }
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
            this.ShowActivityListAsync();
            // if (WindowingSystem.TryGetInstance(out WindowingSystem? system)) {
            //     IWindow window = system.CreateWindow(new ActivityListWindowingContent());
            //     window.IsResizable = false;
            //     window.CanAutoSizeToContent = false;
            //     window.Show(system.GetActiveWindowOrNull());
            // }
        }
    }

    private async void ShowActivityListAsync() {
        try {
            this.isExecutingShowActivityListCmd = true;
            await CommandManager.Instance.Execute("commands.pfx.ShowActivityListCommand", DataManager.GetFullContextData(this), null, null);
        }
        catch (Exception e) {
            ApplicationPFX.Instance.Dispatcher.Post(() => ExceptionDispatchInfo.Throw(e), DispatchPriority.Send);
        }
        finally {
            this.isExecutingShowActivityListCmd = false;
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
            this.PART_PlayPauseButton.Icon = task.IsPaused ? ContinueActivityIcon : PauseActivityIcon;
            ToolTipEx.SetTip(this.PART_PlayPauseButton, task.IsPaused ? "Continue the task" : "Pause the task");
        }
    }
}