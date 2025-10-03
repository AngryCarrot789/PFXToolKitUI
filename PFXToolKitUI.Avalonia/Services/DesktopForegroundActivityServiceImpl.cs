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
using Avalonia.Layout;
using PFXToolKitUI.Activities;
using PFXToolKitUI.Avalonia.Activities;
using PFXToolKitUI.Avalonia.Interactivity.Windowing;
using PFXToolKitUI.Avalonia.Interactivity.Windowing.Desktop;
using PFXToolKitUI.Avalonia.ToolTips;
using PFXToolKitUI.Interactivity.Contexts;
using PFXToolKitUI.Interactivity.Windowing;
using PFXToolKitUI.Logging;
using PFXToolKitUI.Themes;
using SkiaSharp;

namespace PFXToolKitUI.Avalonia.Services;

public sealed class DesktopForegroundActivityServiceImpl : AbstractForegroundActivityService {
    private static readonly DataKey<bool> IsClosingToHideToBackground = DataKeys.Create<bool>(nameof(DesktopForegroundActivityServiceImpl) + "_internal_" + nameof(IsClosingToHideToBackground));

    public override async Task<WaitForActivityResult> WaitForActivity(WaitForActivityOptions options) {
        WaitForActivityOptions.Validate(ref options);
        if (ApplicationPFX.Instance.Dispatcher.CheckAccess()) {
            return await ShowForActivityImpl(options);
        }
        else if (!options.DialogCancellation.IsCancellationRequested) {
            return await await ApplicationPFX.Instance.Dispatcher.InvokeAsync(() => ShowForActivityImpl(options), token: CancellationToken.None);
        }
        else {
            return WaitForActivityResult.DialogCancellationRequested;
        }
    }

    public override async Task WaitForSubActivities(ITopLevel parentTopLevel, IEnumerable<SubActivity> activities, CancellationToken dialogCancellation = default) {
        if (ApplicationPFX.Instance.Dispatcher.CheckAccess()) {
            await ShowForMultipleSubActivitiesImpl(parentTopLevel, activities, dialogCancellation);
        }
        else if (!dialogCancellation.IsCancellationRequested) {
            await await ApplicationPFX.Instance.Dispatcher.InvokeAsync(() => ShowForMultipleSubActivitiesImpl(parentTopLevel, activities, dialogCancellation), token: CancellationToken.None);
        }
    }

    private static async Task<WaitForActivityResult> ShowForActivityImpl(WaitForActivityOptions options) {
        if (ActivityManager.Instance.TryGetCurrentTask(out ActivityTask? currentActivity)) {
            if (ReferenceEquals(currentActivity, options.Activity) && (!options.DialogCancellation.CanBeCanceled || options.WaitForActivityOnCloseRequest)) {
                AppLogger.Instance.WriteLine($"Fatal Deadlock Risk: {nameof(WaitForActivity)} called from within an activity, " +
                                             $"and the cancellation token is not cancellable " +
                                             $"or the options specify we must wait for the activity to complete on close request");
                Debugger.Break();
            }
        }

        if (!(options.ParentTopLevel is IDesktopWindow theWindow))
            throw new InvalidOperationException("Invalid top level object");
        if (options.DialogCancellation.IsCancellationRequested)
            return WaitForActivityResult.DialogCancellationRequested;
        if (options.Activity.IsCompleted)
            return WaitForActivityResult.ActivityCompletedEarly;

        IWindowManager manager = theWindow.WindowManager;
        ActivityProgressRowControl row = new ActivityProgressRowControl() {
            ActivityTask = options.Activity,
            ShowCaption = false,
            Margin = new Thickness(8, 8, 8, options.CanMinimizeIntoBackgroundActivity ? 0 : 8)
        };

        StackPanel stack = new StackPanel() {
            Children = { row }
        };

        if (options.CanMinimizeIntoBackgroundActivity) {
            stack.Children.Add(new StackPanel() {
                HorizontalAlignment = HorizontalAlignment.Right,
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(5, 0, 5, 5),
                Children = {
                    CreateRunInBackgroundButton()
                }
            });
        }

        IActivityProgress progress = options.Activity.Progress;
        IDesktopWindow window = manager.CreateWindow(new WindowBuilder() {
            Title = progress.Caption,
            TitleBarBrush = BrushManager.Instance.GetDynamicThemeBrush("ABrush.Tone6.Background.Static"),
            BorderBrush = BrushManager.Instance.CreateConstant(SKColors.DodgerBlue),
            Content = stack,
            Parent = theWindow,
            Width = 400,
            SizeToContent = SizeToContent.Height,
            CanResize = false,
            IsToolWindow = true
        });

        ActivityProgressEventHandler captionChangedHandler = prog => window.Title = prog.Caption;
        progress.CaptionChanged += captionChangedHandler;

        window.TryClose += (sender, args) => {
            bool forceClose = options.DialogCancellation.IsCancellationRequested;
            bool isMinimizingToBackground = IsClosingToHideToBackground.GetContext(window.LocalContextData);
            if (options.Activity.IsCompleted || forceClose || isMinimizingToBackground) {
                return;
            }

            if (options.CancelActivityOnCloseRequest) {
                options.Activity.TryCancel();
            }

            if (options.WaitForActivityOnCloseRequest) {
                args.SetCancelled();
            }
        };

        window.Opened += static (sender, args) => {
            ActivityTask theTask = ((ActivityProgressRowControl) ((StackPanel) sender.Content!).Children[0]).ActivityTask!;
            Debug.Assert(theTask != null);
            SetIsActivityPresentInDialog(theTask, true);
        };

        window.Closed += static (sender, args) => {
            ActivityProgressRowControl myContent = (ActivityProgressRowControl) ((StackPanel) sender.Content!).Children[0];
            ActivityTask theTask = myContent.ActivityTask!;
            Debug.Assert(theTask != null);

            SetIsActivityPresentInDialog(theTask, false);
            myContent.ActivityTask = null;
        };

        // Use WeakReference just in case the task continuation doesn't get removed
        // from the parent task for some reason when dialogCancellation is cancelled
        WeakReference dialogRef = new WeakReference(window);

        // Register cancellation to force-close dialog even if myTask is not completed
        await using CancellationTokenRegistration register = options.DialogCancellation.Register(PostCloseToMainThread, dialogRef);

        // Add activity continuation that closes the window.
        _ = options.Activity.Task.ContinueWith(static (t, winRef) => PostCloseToMainThread(winRef), dialogRef, options.DialogCancellation);

        window.Title = progress.Caption ?? "Activity Progress";
        await window.ShowDialogAsync();
        progress.CaptionChanged -= captionChangedHandler;

        bool isMinimizingToBackground = IsClosingToHideToBackground.GetContext(window.LocalContextData);
        return new WaitForActivityResult(isMinimizingToBackground);
    }

    private static Button CreateRunInBackgroundButton() {
        Button button = new Button() {
            HorizontalAlignment = HorizontalAlignment.Right,
            Padding = new Thickness(8, 3),
            Content = "Run in background",
            CornerRadius = new CornerRadius(3),
            [ToolTipEx.TipProperty] = "Close the dialog and run this task in the background"
        };

        button.Click += static (sender, args) => {
            IDesktopWindow? window = IDesktopWindow.FromVisual((Button) sender!);
            if (window != null && window.OpenState == OpenState.Open) {
                window.LocalContextData.Set(IsClosingToHideToBackground, true);
                window.RequestClose();
            }
                
            args.Handled = true;
        };
        
        return button;
    }

    private static async Task ShowForMultipleSubActivitiesImpl(ITopLevel topLevel, IEnumerable<SubActivity> activities, CancellationToken dialogCancellation) {
        if (!(topLevel is IDesktopWindow theWindow))
            throw new InvalidOperationException("Invalid top level object");

        if (dialogCancellation.IsCancellationRequested)
            return;

        if (!(activities is IList<SubActivity> activityList))
            activityList = activities.ToList();

        if (activityList.All(x => x.Task.IsCompleted))
            return;

        StackPanel panel = new StackPanel() {
            Margin = new Thickness(8),
            Spacing = 5
        };

        foreach (SubActivity activity in activityList) {
            if (activity.Task.IsCompleted) {
                continue;
            }

            SubActivityProgressRowControl control = new SubActivityProgressRowControl() {
                SubActivity = activity,
                ShowCaption = true,
            };

            panel.Children.Add(control);

            // Register continuation for when the task completes. Use weakref in case task lasts a long time and outlives dialog
            _ = activity.Task.ContinueWith(OnSubActivityTaskCompleted, new WeakReference(control), CancellationToken.None);
        }

        if (panel.Children.Count < 1 || dialogCancellation.IsCancellationRequested) {
            return;
        }

        Task allTasksCompletedTask = Task.WhenAll(activityList.Select(x => x.Task));

        IDesktopWindow window = theWindow.WindowManager.CreateWindow(new WindowBuilder() {
            Title = "Waiting for multiple activities",
            TitleBarBrush = BrushManager.Instance.GetDynamicThemeBrush("ABrush.Tone6.Background.Static"),
            BorderBrush = BrushManager.Instance.CreateConstant(SKColors.DodgerBlue),
            Content = panel,
            Width = 400,
            SizeToContent = SizeToContent.Height,
            Parent = theWindow,
            CanResize = false,
            IsToolWindow = true
        });

        window.TryClose += (sender, args) => {
            if (allTasksCompletedTask.IsCompleted || dialogCancellation.IsCancellationRequested) {
                return; // closed due to tasks completing or dialog cancellation
            }

            // user may have clicked the close button. try cancel all sub activities still running
            foreach (Control control in panel.Children) {
                SubActivity? subActivity = ((SubActivityProgressRowControl) control).SubActivity;
                CancellationTokenSource? cancellation = subActivity?.Cancellation;
                cancellation?.Cancel();
            }

            args.SetCancelled();
        };

        // clean up sub activity controls in case dialogCancellation becomes cancelled
        window.Closing += static (sender, args) => {
            StackPanel panel = (StackPanel) sender.Content!;
            foreach (Control control in panel.Children)
                ((SubActivityProgressRowControl) control).SubActivity = null;

            panel.Children.Clear();
        };

        // Use WeakReference just in case the task continuation doesn't get removed
        // from the parent task for some reason when dialogCancellation is cancelled
        WeakReference dialogRef = new WeakReference(window);

        // Register cancellation to force-close dialog even if myTask is not completed
        await using CancellationTokenRegistration register = dialogCancellation.Register(PostCloseToMainThread, dialogRef);

        // Add activity continuation that closes the window.
        _ = allTasksCompletedTask.ContinueWith(static (t, winRef) => PostCloseToMainThread(winRef), dialogRef, dialogCancellation);

        await window.ShowDialogAsync();
    }

    private static void OnSubActivityTaskCompleted(Task task, object? controlWeakRef) {
        SubActivityProgressRowControl? target = (SubActivityProgressRowControl?) ((WeakReference) controlWeakRef!).Target;
        if (target != null) {
            ApplicationPFX.Instance.Dispatcher.Post(static ctrl => {
                SubActivityProgressRowControl control = (SubActivityProgressRowControl) ctrl!;
                control.SubActivity = null;
                if (control.Parent is StackPanel ctrlPanel) {
                    ctrlPanel.Children.Remove(control);
                }
            }, target);
        }
    }

    private static void PostCloseToMainThread(object? weakRef) {
        ApplicationPFX.Instance.Dispatcher.Post(static void (winRef) => {
            IDesktopWindow? win = (IDesktopWindow?) ((WeakReference) winRef!).Target;
            if (win != null && win.OpenState == OpenState.Open) {
                win.RequestClose();
            }
        }, weakRef);
    }
}