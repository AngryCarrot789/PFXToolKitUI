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

namespace PFXToolKitUI.Avalonia.Services;

public class DesktopForegroundActivityServiceImpl : IForegroundActivityService {
    public async Task ShowActivity(ITopLevel topLevel, ActivityTask task, CancellationToken dialogCancellation) {
        if (dialogCancellation.IsCancellationRequested)
            return;
        
        await await ApplicationPFX.Instance.Dispatcher.InvokeAsync(async () => {
            if (dialogCancellation.IsCancellationRequested)
                return;

            if (!(topLevel is IWindow theWindow))
                throw new InvalidOperationException("Invalid top level object");

            IWindowManager manager = theWindow.WindowManager;
            ActivityRowControl row = new ActivityRowControl() {
                ActivityTask = task,
                ShowCaption = true
            };

            IActivityProgress progress = task.Progress;
            IWindow window = manager.CreateWindow(new WindowBuilder() {
                Title = progress.Caption,
                Content = row,
                Parent = theWindow
            });

            ActivityProgressEventHandler captionChangedHandler = prog => window.Title = prog.Caption; 
            progress.CaptionChanged += captionChangedHandler;

            window.TryClose += (sender, args) => {
                // user may have clicked the close button
                if (!task.IsCompleted) {
                    args.SetCancelled();
                }
            };

            // Add continuation that closes the window
            _ = task.Task.ContinueWith(t => ApplicationPFX.Instance.Dispatcher.Post(void () => _ = window.RequestCloseAsync()), CancellationToken.None);

            window.Title = progress.Caption ?? "Activity Progress";
            await window.ShowAsync();
            window.Activate();

            await window.WaitForClosedAsync(dialogCancellation);
            row.ActivityTask = null;
            progress.CaptionChanged -= captionChangedHandler;
            
            Debug.Assert(window.IsOpenAndNotClosing == dialogCancellation.IsCancellationRequested);
            if (window.IsOpenAndNotClosing) {
                await window.RequestCloseAsync();
            }
        }, token: CancellationToken.None);
    }

    public async Task ShowMultipleProgressions(ITopLevel topLevel, IEnumerable<(IActivityProgress Progress, Task Task)> progressions, CancellationToken dialogCancellation = default) {
        if (dialogCancellation.IsCancellationRequested)
            return;

        await await ApplicationPFX.Instance.Dispatcher.InvokeAsync(async () => {
            if (dialogCancellation.IsCancellationRequested)
                return;
            if (!(topLevel is IWindow theWindow))
                throw new InvalidOperationException("Invalid top level object");
            
            if (!(progressions is IList<(IActivityProgress Progress, Task Task)> progressionList))
                progressionList = progressions.ToList();
            
            if (progressionList.Any(task => progressionList.Count(x => x == task) != 1))
                throw new ArgumentException("Duplicate task references in the list");

            StackPanel panel = new StackPanel() {
                Margin = new Thickness(5),
                Spacing = 5
            };

            foreach ((IActivityProgress Progress, Task Task) entry in progressionList) {
                if (entry.Task.IsCompleted) {
                    continue;
                }

                ProgressRowControl control = new ProgressRowControl() {
                    ActivityProgress = entry.Progress,
                    ShowCaption = true
                };

                panel.Children.Add(control);

                _ = entry.Task.ContinueWith(t => {
                    ApplicationPFX.Instance.Dispatcher.Post(() => {
                        control.ActivityProgress = null;
                        panel.Children.Remove(control);
                    });
                }, CancellationToken.None);
            }

            if (panel.Children.Count < 1 || dialogCancellation.IsCancellationRequested) {
                return; // all tasks completed, or cancellation requested, so just exit
            }

            Task allTasksCompletedTask = Task.WhenAll(progressionList.Select(x => x.Task));

            IWindow window = theWindow.WindowManager.CreateWindow(new WindowBuilder() {
                Title = "Waiting for multiple activities",
                Content = panel,
                Width = 400,
                SizeToContent = SizeToContent.Height,
                Parent = theWindow
            });

            window.TryClose += (sender, args) => {
                // user may have clicked the close button
                if (!allTasksCompletedTask.IsCompleted) {
                    args.SetCancelled();
                }
            };

            // Add continuation that closes the window
            _ = allTasksCompletedTask.ContinueWith(t => ApplicationPFX.Instance.Dispatcher.Post(void () => _ = window.RequestCloseAsync()), CancellationToken.None);
            
            await window.ShowAsync();
            window.Activate();

            await window.WaitForClosedAsync(dialogCancellation);
            Debug.Assert(window.IsOpenAndNotClosing == dialogCancellation.IsCancellationRequested);
            if (window.IsOpenAndNotClosing) {
                await window.RequestCloseAsync();
            }
        }, token: CancellationToken.None);
    }
}