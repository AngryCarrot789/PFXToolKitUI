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
        if (!ApplicationPFX.Instance.Dispatcher.CheckAccess()) {
            if (!dialogCancellation.IsCancellationRequested) {
                await await ApplicationPFX.Instance.Dispatcher.InvokeAsync(() => ShowForActivityImpl(parentTopLevel, activity, dialogCancellation), token: CancellationToken.None);
            }
        }
        else {
            await ShowForActivityImpl(parentTopLevel, activity, dialogCancellation);
        }
    }

    public async Task WaitForSubActivities(ITopLevel parentTopLevel, IEnumerable<SubActivity> activities, CancellationToken dialogCancellation = default) {
        if (!ApplicationPFX.Instance.Dispatcher.CheckAccess()) {
            if (!dialogCancellation.IsCancellationRequested) {
                await await ApplicationPFX.Instance.Dispatcher.InvokeAsync(() => ShowForMultipleSubActivitiesImpl(parentTopLevel, activities, dialogCancellation), token: CancellationToken.None);
            }
        }
        else {
            await ShowForMultipleSubActivitiesImpl(parentTopLevel, activities, dialogCancellation);
        }
    }

    private static async Task ShowForActivityImpl(ITopLevel topLevel, ActivityTask task, CancellationToken dialogCancellation) {
        if (dialogCancellation.IsCancellationRequested)
            return;

        if (!(topLevel is IWindow theWindow))
            throw new InvalidOperationException("Invalid top level object");

        IWindowManager manager = theWindow.WindowManager;
        ActivityProgressRowControl row = new ActivityProgressRowControl() {
            ActivityTask = task,
            ShowCaption = false,
            Margin = new Thickness(8)
        };

        IActivityProgress progress = task.Progress;
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
            if (task.IsCompleted || dialogCancellation.IsCancellationRequested) {
                return; // closed due to task completing or dialog cancellation
            }
            
            // try cancel the task when the user clicks the X button
            task.TryCancel();
            args.SetCancelled();
        };

        window.WindowOpened += (sender, args) => {
            ActivityTask theTask = ((ActivityProgressRowControl) sender.Content!).ActivityTask!;
            Debug.Assert(theTask != null);
            ActivityTask.InternalOnPresentInDialogChanged(theTask, true);
        };

        window.WindowClosed += (sender, args) => {
            ActivityProgressRowControl myContent = (ActivityProgressRowControl) sender.Content!;
            ActivityTask theTask = myContent.ActivityTask!;
            Debug.Assert(theTask != null);
            
            ActivityTask.InternalOnPresentInDialogChanged(theTask, false);
            myContent.ActivityTask = null;
        };

        // Add continuation that closes the window
        await using CancellationTokenRegistration register = dialogCancellation.Register(PostCloseToMainThread, window);
        _ = task.Task.ContinueWith((t, w) => PostCloseToMainThread((IWindow) w!), window, CancellationToken.None);

        window.Title = progress.Caption ?? "Activity Progress";
        await window.ShowDialogAsync();

        progress.CaptionChanged -= captionChangedHandler;
        if (window.OpenState == OpenState.Open) {
            bool isClosed = await window.RequestCloseAsync(); // should not get cancelled
            Debug.Assert(isClosed);
        }
    }

    private static void PostCloseToMainThread(object? window) {
        ApplicationPFX.Instance.Dispatcher.Post(void (w) => {
            IWindow win = (IWindow) w!;
            if (win.OpenState == OpenState.Open)
                _ = win.RequestCloseAsync();
        }, window);
    }

    private static async Task ShowForMultipleSubActivitiesImpl(ITopLevel topLevel, IEnumerable<SubActivity> activities, CancellationToken dialogCancellation) {
        if (dialogCancellation.IsCancellationRequested)
            return;
        if (!(topLevel is IWindow theWindow))
            throw new InvalidOperationException("Invalid top level object");

        if (!(activities is IList<SubActivity> activityList))
            activityList = activities.ToList();

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

            _ = activity.Task.ContinueWith(t => {
                ApplicationPFX.Instance.Dispatcher.Post(() => {
                    control.SubActivity = null;
                    panel.Children.Remove(control);
                });
            }, CancellationToken.None);
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
            // user may have clicked the close button
            if (!allTasksCompletedTask.IsCompleted) {
                args.SetCancelled();
            }
        };

        // Add continuation that closes the window
        _ = allTasksCompletedTask.ContinueWith(t => ApplicationPFX.Instance.Dispatcher.Post(void () => _ = window.RequestCloseAsync()), CancellationToken.None);

        await window.ShowDialogAsync();
        window.Activate();

        await window.WaitForClosedAsync(dialogCancellation);
        Debug.Assert((window.OpenState == OpenState.Open) == dialogCancellation.IsCancellationRequested);
        if (window.OpenState == OpenState.Open) {
            await window.RequestCloseAsync();
        }
    }
}