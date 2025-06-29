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

namespace PFXToolKitUI.Tasks;

public delegate void TaskManagerTaskEventHandler(ActivityManager activityManager, ActivityTask task, int index);

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
/// byte[] bytes = await ActivityManager.Instance.RunTask(async () => {
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
    public static ActivityManager Instance => ApplicationPFX.Instance.ServiceManager.GetService<ActivityManager>();

    // private readonly ThreadLocal<ActivityTask> threadToTask;
    private readonly AsyncLocal<ActivityTask?> threadToTask;
    private readonly List<ActivityTask> tasks;
    private readonly object locker;

    /// <summary>
    /// Fired when a task is started. This is fired on the main thread
    /// </summary>
    public event TaskManagerTaskEventHandler? TaskStarted;
    
    /// <summary>
    /// Fired when a task is completed in any way. Fired on the main thread
    /// </summary>
    public event TaskManagerTaskEventHandler? TaskCompleted;

    /// <summary>
    /// Returns a list of activities currently running. This list is only modified on the main thread
    /// </summary>
    public IReadOnlyList<ActivityTask> ActiveTasks => this.tasks;

    public ActivityManager() {
        this.threadToTask = new AsyncLocal<ActivityTask?>();
        this.tasks = new List<ActivityTask>();
        this.locker = new object();
    }

    public ActivityTask RunTask(Func<Task> action, TaskCreationOptions creationOptions = TaskCreationOptions.None) => this.RunTask(action, (CancellationTokenSource?) null, creationOptions);

    public ActivityTask RunTask(Func<Task> action, IActivityProgress progress, TaskCreationOptions creationOptions = TaskCreationOptions.None) => this.RunTask(action, progress, null, creationOptions);

    public ActivityTask RunTask(Func<Task> action, CancellationTokenSource? cts, TaskCreationOptions creationOptions = TaskCreationOptions.None) => this.RunTask(action, new DefaultProgressTracker(), cts, creationOptions);

    public ActivityTask RunTask(Func<Task> action, IActivityProgress progress, CancellationTokenSource? cts, TaskCreationOptions creationOptions = TaskCreationOptions.None) {
        return ActivityTask.InternalStartActivity(this, action, progress, cts, creationOptions);
    }

    public ActivityTask<T> RunTask<T>(Func<Task<T>> action, TaskCreationOptions creationOptions = TaskCreationOptions.None) => this.RunTask(action, (CancellationTokenSource?) null, creationOptions);

    public ActivityTask<T> RunTask<T>(Func<Task<T>> action, IActivityProgress progress, TaskCreationOptions creationOptions = TaskCreationOptions.None) => this.RunTask(action, progress, null, creationOptions);

    public ActivityTask<T> RunTask<T>(Func<Task<T>> action, CancellationTokenSource? cts, TaskCreationOptions creationOptions = TaskCreationOptions.None) => this.RunTask(action, new DefaultProgressTracker(), cts, creationOptions);

    public ActivityTask<T> RunTask<T>(Func<Task<T>> action, IActivityProgress progress, CancellationTokenSource? cts, TaskCreationOptions creationOptions = TaskCreationOptions.None) {
        return ActivityTask<T>.InternalStartActivity(this, action, progress, cts, creationOptions);
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
        return ApplicationPFX.Instance.Dispatcher.InvokeAsync(() => InternalOnTaskStarted(activityManager, task));
    }

    internal static Task InternalOnActivityCompleted(ActivityManager activityManager, ActivityTask task, int state) {
        activityManager.threadToTask.Value = null;
        return ApplicationPFX.Instance.Dispatcher.InvokeAsync(() => InternalOnTaskCompleted(activityManager, task, state));
    }

    // Main Thread

    internal static void InternalOnTaskStarted(ActivityManager activityManager, ActivityTask task) {
        lock (activityManager.locker) {
            int index = activityManager.tasks.Count;
            activityManager.tasks.Insert(index, task);
            activityManager.TaskStarted?.Invoke(activityManager, task, index);
        }

        ActivityTask.InternalPostActivate(task);
    }

    internal static void InternalOnTaskCompleted(ActivityManager activityManager, ActivityTask task, int state) {
        ActivityTask.InternalComplete(task, state);
        lock (activityManager.locker) {
            int index = activityManager.tasks.IndexOf(task);
            if (index == -1) {
                Debug.WriteLine("[FATAL] Completed activity task did not exist in this task manager's internal task list");
                Debugger.Break();
                return;
            }

            activityManager.tasks.RemoveAt(index);
            activityManager.TaskCompleted?.Invoke(activityManager, task, index);

            task.InternalOnCompletedOnMainThread();
        }
    }
}