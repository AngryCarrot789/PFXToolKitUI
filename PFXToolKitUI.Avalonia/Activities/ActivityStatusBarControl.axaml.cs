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
using Avalonia.Input;
using Avalonia.Interactivity;
using PFXToolKitUI.Avalonia.Services;
using PFXToolKitUI.Avalonia.Services.Windowing;
using PFXToolKitUI.Avalonia.Utils;
using PFXToolKitUI.Icons;
using PFXToolKitUI.Tasks;
using PFXToolKitUI.Themes;

namespace PFXToolKitUI.Avalonia.Activities;

/// <summary>
/// A control that typically sits in a status bar that represents the oldest
/// activity currently running. Optionally supports showing a list of active
/// activities when clicked
/// </summary>
public partial class ActivityStatusBarControl : UserControl {
    public static readonly Icon CrossIcon =
        IconManager.Instance.RegisterGeometryIcon(
            nameof(CrossIcon),
            [
                new GeometryEntry("m.0005 1.9789q-0-.3411.2388-.5798l1.1599-1.1598q.2388-.2388.5799-.2388.3411-0 .5799.2388l2.5077 2.507 2.5077-2.507q.2388-.2388.5799-.2388.3411-0 .5799.2388l1.1599 1.1598Q10.1336 1.6378 10.1336 1.9789t-.2388.5799l-2.5077 2.507L9.8949 7.5726Q10.1337 7.8114 10.1337 8.1524t-.2388.5799l-1.1599 1.1598q-.2388.2386-.58.2387-.3411 0-.58-.2387l-2.5077-2.507-2.5077 2.507q-.2388.2386-.58.2387-.3411 0-.58-.2387l-1.1599-1.1598q-.2388-.2388-.2388-.5799t.2388-.5798L2.7469 5.0658.2392 2.5588q-.2388-.2388-.2388-.5799z", BrushManager.Instance.GetDynamicThemeBrush("ABrush.Glyph.Static")),
            ], 
            stretch: StretchMode.Uniform);
    
    public static readonly StyledProperty<ActivityTask?> ActivityTaskProperty = AvaloniaProperty.Register<ActivityStatusBarControl, ActivityTask?>(nameof(ActivityTask));

    public ActivityTask? ActivityTask {
        get => this.GetValue(ActivityTaskProperty);
        set => this.SetValue(ActivityTaskProperty, value);
    }

    public ActivityStatusBarControl() {
        this.InitializeComponent();
    }

    static ActivityStatusBarControl() {
        ActivityTaskProperty.Changed.AddClassHandler<ActivityStatusBarControl, ActivityTask?>((c, e) => c.OnActivityChanged(e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
    }
    
    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
        
        ActivityManager activityManager = ActivityManager.Instance;
        activityManager.TaskStarted += this.OnTaskStarted;
        activityManager.TaskCompleted += this.OnTaskCompleted;
    }

    protected override void OnUnloaded(RoutedEventArgs e) {
        base.OnUnloaded(e);
        
        ActivityManager activityManager = ActivityManager.Instance;
        activityManager.TaskStarted -= this.OnTaskStarted;
        activityManager.TaskCompleted -= this.OnTaskCompleted;
    }

    private void OnActivityChanged(ActivityTask? oldActivity, ActivityTask? newActivity) {
        IActivityProgress? prog = null;
        if (oldActivity != null) {
            prog = oldActivity.Progress;
            prog.TextChanged -= this.OnActivityTaskTextChanged;
            prog.CompletionState.CompletionValueChanged -= this.OnPrimaryActionCompletionValueChanged;
            prog.IsIndeterminateChanged -= this.OnActivityTaskIndeterminateChanged;
            if (oldActivity.IsDirectlyCancellable)
                this.PART_CancelActivityButton.IsVisible = false;

            prog = null;
        }

        this.ActivityTask = newActivity;
        if (newActivity != null) {
            prog = newActivity.Progress;
            prog.TextChanged += this.OnActivityTaskTextChanged;
            prog.CompletionState.CompletionValueChanged += this.OnPrimaryActionCompletionValueChanged;
            prog.IsIndeterminateChanged += this.OnActivityTaskIndeterminateChanged;
            if (newActivity.IsDirectlyCancellable)
                this.PART_CancelActivityButton.IsVisible = true;

            this.IsVisible = true;
        }
        else {
            this.IsVisible = false;
        }

        this.PART_CancelActivityButton.IsEnabled = true;
        this.OnActivityTaskTextChanged(prog);
        this.OnPrimaryActionCompletionValueChanged(prog?.CompletionState);
        this.OnActivityTaskIndeterminateChanged(prog);
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
        ApplicationPFX.Instance.Dispatcher.Invoke(() => this.PART_TaskCaption.Text = tracker?.Text ?? "", DispatchPriority.Loaded);
    }

    private void OnPrimaryActionCompletionValueChanged(CompletionState? state) {
        ApplicationPFX.Instance.Dispatcher.Invoke(() => this.PART_ActiveBgProgress.Value = state?.TotalCompletion ?? 0.0, DispatchPriority.Loaded);
    }

    private void OnActivityTaskIndeterminateChanged(IActivityProgress? tracker) {
        ApplicationPFX.Instance.Dispatcher.Invoke(() => this.PART_ActiveBgProgress.IsIndeterminate = tracker?.IsIndeterminate ?? false, DispatchPriority.Loaded);
    }
    
    private void PART_CancelActivityButton_OnClick(object? sender, RoutedEventArgs e) {
        ((Button) sender!).IsEnabled = false;
        this.ActivityTask?.TryCancel();
    }

    private void InputElement_OnPointerReleased(object? sender, PointerReleasedEventArgs e) {
        if (!e.Handled && this.IsPointerOver) {
            if (WindowingSystem.TryGetInstance(out WindowingSystem? system)) {
                IWindow window = system.CreateWindow(new ActivityListWindowingContent());
                window.IsResizable = false;
                window.CanAutoSizeToContent = false;
                window.Show(system.GetActiveWindowOrNull());
            }
        }
    }

    private class ActivityListWindowingContent : WindowingContentControl {
        public ActivityListWindowingContent() {
            this.Content = new ActivityListControl();
        }

        protected override void OnWindowOpened() {
            base.OnWindowOpened();
            this.Window!.Control.MinWidth = 300;
            this.Window!.Control.MinHeight = 150;

            this.Window!.Control.SetValue(this.Window!.TitleBarBrushProperty, ((ActivityListControl) this.Content!).HeaderBrush);
            this.Window!.Control.SetValue(this.Window!.BorderBrushProperty, ((ActivityListControl) this.Content!).BorderBrush);
            this.Window.Title = "Background Activities";
        }
    }
}