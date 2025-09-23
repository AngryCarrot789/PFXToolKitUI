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
using System.Diagnostics.CodeAnalysis;
using PFXToolKitUI.Logging;
using PFXToolKitUI.Utils.Collections.Observable;

namespace PFXToolKitUI.Tasks;

public delegate void ActivityManagerTaskIndexEventHandler(ActivityManager activityManager, ActivityTask task, int index);

public delegate void ActivityManagerPrimaryActivityChangedEventHandler(ActivityManager activityManager, ActivityTask? oldActivityTask, ActivityTask? newActivity);

/// <summary>
/// A service which manages activity tasks.
/// <para>
/// Activity tasks basically just wrap a <see cref="Task"/> (do work) or <see cref="Task{TResult}"/> (do work, produce a value)
/// while also providing progression and status feedback and also start/completed notifications that the UI can hook onto
/// and display in the status bar (which can also display a cancel button when provided <see cref="CancellationTokenSource"/>).
/// </para>
/// <para>
/// An example of an activity task to produce a value:
/// <code>
/// <![CDATA[
/// // Providing a CTS makes the activity cancellable. It is not disposed by
/// // the ActivityManager, which is why we use it in a using block
/// using CancellationTokenSource cts = new CancellationTokenSource();
/// Result<byte[]> bytes = await ActivityManager.Instance.RunTask(async () => {
///     ActivityTask task = ActivityManager.Instance.CurrentTask;
///     IActivityProgress progress = task.Progress;
///     progress.Caption = "Produce data";
///     {
///         progress.Text = "Reading data from server...";
///         const int chunks = 32, incrementBy = 1;
///         using var token = progress.CompletionState.PushCompletionRange(0, 1.0 / chunks);
///         for (int i = 0; i < chunks; i += incrementBy) {
///             progress.Text = $"Reading data from server (chunk {i}/{chunks})...";
///             progress.CompletionState.OnProgress(incrementBy);
///             await Task.Delay(100); // get server data
///         }
///     }
///     {
///         progress.Text = "Parsing data...";
///         progress.IsIndeterminate = true;
///         await Task.Delay(1500); // "parse" data
///     }
///     // return produced data
///     return new byte[65536];
/// }, cts);
/// ]]>
/// </code>
/// </para>
/// </summary>
public sealed class ActivityManager : IDisposable {
    /// <summary>
    /// Gets the application's main activity manager
    /// </summary>
    public static ActivityManager Instance => ApplicationPFX.GetComponent<ActivityManager>();

    // private readonly ThreadLocal<ActivityTask> threadToTask;
    private readonly AsyncLocal<ActivityTask?> threadToTask;
    private readonly List<ActivityTask> activeTasks; // all tasks
    private readonly ObservableList<ActivityTask> backgroundTasks; // tasks not present in a dialog

    /// <summary>
    /// Returns a list of activities currently running. This list is only modified on the main thread
    /// </summary>
    public IReadOnlyList<ActivityTask> ActiveTasks { get; }

    /// <summary>
    /// Gets a read-only observable list of the activity tasks running in the background and are therefore
    /// not present in a foreground dialog. This list is only modified on the main thread
    /// </summary>
    public ReadOnlyObservableList<ActivityTask> BackgroundTasks { get; }

    /// <summary>
    /// Fired when a task is started. This is fired on the main thread
    /// </summary>
    public event ActivityManagerTaskIndexEventHandler? TaskStarted;

    /// <summary>
    /// Fired when a task is completed in any way. Fired on the main thread
    /// </summary>
    public event ActivityManagerTaskIndexEventHandler? TaskCompleted;

    public ActivityManager() {
        this.threadToTask = new AsyncLocal<ActivityTask?>();
        this.activeTasks = new List<ActivityTask>();
        this.ActiveTasks = this.activeTasks.AsReadOnly();
        this.backgroundTasks = new ObservableList<ActivityTask>();
        this.BackgroundTasks = new ReadOnlyObservableList<ActivityTask>(this.backgroundTasks);
    }

    public ActivityTask RunTask(Func<Task> action) => this.RunTask(action, (CancellationTokenSource?) null);

    public ActivityTask RunTask(Func<Task> action, IActivityProgress progress) => this.RunTask(action, progress, null);

    public ActivityTask RunTask(Func<Task> action, CancellationTokenSource? cts) => this.RunTask(action, new DispatcherActivityProgress(), cts);

    public ActivityTask RunTask(Func<Task> action, IActivityProgress progress, CancellationTokenSource? cts) {
        return ActivityTask.InternalStartActivity(this, action, progress, cts);
    }

    public ActivityTask<T> RunTask<T>(Func<Task<T>> action) => this.RunTask(action, (CancellationTokenSource?) null);

    public ActivityTask<T> RunTask<T>(Func<Task<T>> action, IActivityProgress progress) => this.RunTask(action, progress, null);

    public ActivityTask<T> RunTask<T>(Func<Task<T>> action, CancellationTokenSource? cts) => this.RunTask(action, new DispatcherActivityProgress(), cts);

    public ActivityTask<T> RunTask<T>(Func<Task<T>> action, IActivityProgress progress, CancellationTokenSource? cts) {
        return ActivityTask<T>.InternalStartActivity(this, action, progress, cts);
    }

    /// <summary>
    /// Tries to get the activity task associated with the current caller thread
    /// </summary>
    /// <param name="task">The task associated with the current thread</param>
    /// <returns>True if this thread is running a task</returns>
    public bool TryGetCurrentTask([NotNullWhen(true)] out ActivityTask? task) {
        return (task = this.threadToTask.Value) != null;
    }

    /// <summary>
    /// Gets the activity running on this thread
    /// </summary>
    /// <exception cref="InvalidOperationException">No task running on this thread</exception>
    public ActivityTask CurrentTask => this.threadToTask.Value ?? throw new Exception("No task running on this thread");

    /// <summary>
    /// Gets either the current task's activity progress tracker, or the <see cref="EmptyActivityProgress"/> instance (for convenience over null-checks)
    /// </summary>
    /// <returns></returns>
    public IActivityProgress GetCurrentProgressOrEmpty() {
        return this.TryGetCurrentTask(out ActivityTask? task) ? task.Progress : EmptyActivityProgress.Instance;
    }

    public void Dispose() {
        // this.threadToTask.Dispose();
    }

    // Activity thread

    internal static Task InternalPreActivateTask(ActivityManager activityManager, ActivityTask task) {
        activityManager.threadToTask.Value = task;
        return ApplicationPFX.Instance.Dispatcher.InvokeAsync(() => InternalOnTaskStarted(activityManager, task), DispatchPriority.Send);
    }

    internal static Task InternalOnActivityCompleted(ActivityManager activityManager, ActivityTask task, int state) {
        activityManager.threadToTask.Value = null;
        return ApplicationPFX.Instance.Dispatcher.InvokeAsync(() => InternalOnTaskCompleted(activityManager, task, state), DispatchPriority.Send);
    }

    // Main Thread

    internal static void InternalOnTaskStarted(ActivityManager manager, ActivityTask task) {
        int index = manager.activeTasks.Count;
        manager.activeTasks.Insert(index, task);
        manager.TaskStarted?.Invoke(manager, task, index);
        
        // The activity task object can be used before the even get started, since starting
        // one requires starting a Task which then calls InternalPreActivateTask which
        // has to dispatch this method call to the main thread.
        if (!task.IsPresentInDialog) {
            manager.backgroundTasks.Add(task);
        }

        ActivityTask.InternalPostActivate(task);
    }

    internal static void InternalOnTaskCompleted(ActivityManager manager, ActivityTask task, int state) {
        ActivityTask.InternalComplete(task, state);
        int index = manager.activeTasks.IndexOf(task);
        if (index == -1) {
            AppLogger.Instance.WriteLine("Completed ActivityTask did not exist in activeTasks list");
            Debug.Fail("Oops");
            manager.backgroundTasks.Remove(task); // don't care if not removed
            return;
        }

        manager.activeTasks.RemoveAt(index);
        manager.TaskCompleted?.Invoke(manager, task, index);
        manager.backgroundTasks.Remove(task); // don't care if not removed

        task.InternalOnCompletedOnMainThread();
    }

    internal static void InternalOnIsPresentInDialogChanged(ActivityManager manager, ActivityTask task) {
        if (task.IsPresentInDialog) {
            manager.backgroundTasks.Remove(task);
        }
        else {
            Debug.Assert(!manager.backgroundTasks.Contains(task), "ActivityTask should not already be in the background task list");
            int realIdx = manager.activeTasks.IndexOf(task);
            if (realIdx == -1) {
                // Either:
                //   Activity was shown in a dialog and then the dialog closed, all before
                //   InternalOnTaskCompleted managed to run. Unlikely, but not impossible
                // Or:
                //   Activity has already completed and is still being used for some reason
                Debug.Assert(!task.IsRunning || task.IsCompleted);
                return;
            }
            
            int dstIdx = manager.backgroundTasks.Count;
            for (int i = 0; i < manager.backgroundTasks.Count; i++) {
                int taskIdx = manager.activeTasks.IndexOf(manager.backgroundTasks[i]);
                if (taskIdx > realIdx) {
                    dstIdx = i;
                    break;
                }
            }

            manager.backgroundTasks.Insert(dstIdx, task);

            // int realIndex = manager.activeTasks.IndexOf(task);
            // int dstIdx = 0;
            // foreach (ActivityTask bgTask in manager.backgroundTasks) {
            //     int j = manager.activeTasks.IndexOf(bgTask);
            //     if (j > realIndex) {
            //         break;
            //     }
            //
            //     dstIdx++;
            // }
            //
            // manager.backgroundTasks.Insert(dstIdx, task);
        }
    }
}