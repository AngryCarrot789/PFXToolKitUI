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
using PFXToolKitUI.Avalonia.Activities;
using PFXToolKitUI.Avalonia.Interactivity.Windowing;
using PFXToolKitUI.Interactivity.Windowing;
using PFXToolKitUI.Tasks;
using PFXToolKitUI.Themes;
using SkiaSharp;

namespace PFXToolKitUI.Avalonia.Services;

public class DesktopForegroundActivityServiceImpl : IForegroundActivityService {
    public async Task WaitForActivity(ITopLevel parentTopLevel, ActivityTask activity, CancellationToken dialogCancellation) {
        if (ApplicationPFX.Instance.Dispatcher.CheckAccess()) {
            await ShowForActivityImpl(parentTopLevel, activity, dialogCancellation);
        }
        else if (!dialogCancellation.IsCancellationRequested) {
            await await ApplicationPFX.Instance.Dispatcher.InvokeAsync(() => ShowForActivityImpl(parentTopLevel, activity, dialogCancellation), token: CancellationToken.None);
        }
    }

    public async Task WaitForSubActivities(ITopLevel parentTopLevel, IEnumerable<SubActivity> activities, CancellationToken dialogCancellation = default) {
        if (ApplicationPFX.Instance.Dispatcher.CheckAccess()) {
            await ShowForMultipleSubActivitiesImpl(parentTopLevel, activities, dialogCancellation);
        }
        else if (!dialogCancellation.IsCancellationRequested) {
            await await ApplicationPFX.Instance.Dispatcher.InvokeAsync(() => ShowForMultipleSubActivitiesImpl(parentTopLevel, activities, dialogCancellation), token: CancellationToken.None);
        }
    }

    private static async Task ShowForActivityImpl(ITopLevel topLevel, ActivityTask activity, CancellationToken dialogCancellation) {
        if (!(topLevel is IWindow theWindow))
            throw new InvalidOperationException("Invalid top level object");
        
        if (dialogCancellation.IsCancellationRequested || activity.IsCompleted)
            return;

        IWindowManager manager = theWindow.WindowManager;
        ActivityProgressRowControl row = new ActivityProgressRowControl() {
            ActivityTask = activity,
            ShowCaption = false,
            Margin = new Thickness(8)
        };

        IActivityProgress progress = activity.Progress;
        IWindow window = manager.CreateWindow(new WindowBuilder() {
            Title = progress.Caption,
            TitleBarBrush = BrushManager.Instance.GetDynamicThemeBrush("ABrush.Tone6.Background.Static"),
            BorderBrush = BrushManager.Instance.CreateConstant(SKColors.DodgerBlue),
            Content = row,
            Parent = theWindow,
            Width = 400,
            SizeToContent = SizeToContent.Height,
            CanResize = false,
            IsToolWindow = true
        });

        ActivityProgressEventHandler captionChangedHandler = prog => window.Title = prog.Caption;
        progress.CaptionChanged += captionChangedHandler;

        window.TryClose += (sender, args) => {
            if (activity.IsCompleted || dialogCancellation.IsCancellationRequested) {
                return; // closed due to task completing or dialog cancellation
            }

            // try cancel the task when the user clicks the X button
            activity.TryCancel();
            args.SetCancelled();
        };

        window.WindowOpened += static (sender, args) => {
            ActivityTask theTask = ((ActivityProgressRowControl) sender.Content!).ActivityTask!;
            Debug.Assert(theTask != null);
            ActivityTask.InternalOnPresentInDialogChanged(theTask, true);
        };

        window.WindowClosed += static (sender, args) => {
            ActivityProgressRowControl myContent = (ActivityProgressRowControl) sender.Content!;
            ActivityTask theTask = myContent.ActivityTask!;
            Debug.Assert(theTask != null);

            ActivityTask.InternalOnPresentInDialogChanged(theTask, false);
            myContent.ActivityTask = null;
        };

        // Use WeakReference just in case the task continuation doesn't get removed
        // from the parent task for some reason when dialogCancellation is cancelled
        WeakReference dialogRef = new WeakReference(window);

        // Register cancellation to force-close dialog even if myTask is not completed
        await using CancellationTokenRegistration register = dialogCancellation.Register(PostCloseToMainThread, dialogRef);

        // Add activity continuation that closes the window.
        _ = activity.Task.ContinueWith(static (t, winRef) => PostCloseToMainThread(winRef), dialogRef, dialogCancellation);

        window.Title = progress.Caption ?? "Activity Progress";
        await window.ShowDialogAsync();
        progress.CaptionChanged -= captionChangedHandler;

        if (window.OpenState == OpenState.Open) {
            // dialogCancellation cancelled so close quickly
            bool isClosed = await window.RequestCloseAsync(); // should not get cancelled
            Debug.Assert(isClosed);
        }
    }

    private static async Task ShowForMultipleSubActivitiesImpl(ITopLevel topLevel, IEnumerable<SubActivity> activities, CancellationToken dialogCancellation) {
        if (!(topLevel is IWindow theWindow))
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

        IWindow window = theWindow.WindowManager.CreateWindow(new WindowBuilder() {
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
        window.WindowClosing += static (sender, args) => {
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

        if (window.OpenState == OpenState.Open) {
            // dialogCancellation cancelled so close quickly
            bool isClosed = await window.RequestCloseAsync(); // should not get cancelled
            Debug.Assert(isClosed);
        }
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
            IWindow? win = (IWindow?) ((WeakReference) winRef!).Target;
            if (win != null && win.OpenState == OpenState.Open) {
                _ = win.RequestCloseAsync();
            }
        }, weakRef);
    }
}