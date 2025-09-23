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
using System.Runtime.CompilerServices;
using PFXToolKitUI.Tasks.Pausable;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.Tasks;

public delegate void ActivityTaskEventHandler(ActivityTask sender);

public delegate void ActivityTaskPausableTaskChangedEventHandler(ActivityTask sender, AdvancedPausableTask? oldPausableTask, AdvancedPausableTask? newPausableTask);

/// <summary>
/// Represents a task that can be run by a <see cref="ActivityManager"/> on a background thread
/// </summary>
[DebuggerDisplay("{ToString()}")]
public class ActivityTask {
    private readonly ActivityManager activityManager;
    private readonly Func<Task> action;
    private readonly TaskCompletionSource<ActivityTask> tcsStarted;
    private volatile Exception? exception;

    //  0 = waiting for activation
    //  1 = running
    //  2 = completed
    //  3 = cancelled
    private volatile int state;
    private volatile Task? userTask; // task from action()
    protected Task? theMainTask; // task we created
    internal readonly CancellationTokenSource? cancellationTokenSource;
    private AdvancedPausableTask? myInternalPausableTask;

    // Counts how many dialogs the activity is present in (via IForegroundActivityService)
    private int dialogPresence;

    /// <summary>
    /// Gets the task returned by the user's function when running the activity
    /// </summary>
    protected Task? UserTask => this.userTask;

    /// <summary>
    /// Returns true if the task is currently still running
    /// </summary>
    public bool IsRunning => this.state == 1;

    /// <summary>
    /// Returns true if the task is completed. <see cref="Exception"/> may be non-null when this is true
    /// </summary>
    public bool IsCompleted => this.state > 1;

    /// <summary>
    /// Returns true when this activity was completed due to cancellation
    /// </summary>
    public bool IsCancelled => this.state == 3;

    /// <summary>
    /// Returns true when this task can be cancelled via <see cref="TryCancel"/>
    /// </summary>
    public bool IsDirectlyCancellable => this.cancellationTokenSource != null;

    /// <summary>
    /// Returns true when cancellation is requested 
    /// </summary>
    public bool IsCancellationRequested => this.IsDirectlyCancellable && this.CancellationToken.IsCancellationRequested;

    /// <summary>
    /// Gets the exception that was thrown during the execution of the user action
    /// </summary>
    public Exception? Exception => this.exception;

    /// <summary>
    /// Gets the progress handler associated with this task. Will always be non-null
    /// </summary>
    public IActivityProgress Progress { get; }

    /// <summary>
    /// Gets the first token created from our <see cref="CancellationTokenSource"/>
    /// </summary>
    public CancellationToken CancellationToken { get; }

    /// <summary>
    /// Gets this activity's task, which can be used to await completion. This task is a proxy of the user
    /// task function, and will not throw any exceptions, even if the activity is cancelled or faulted
    /// </summary>
    public Task Task {
        get => this.theMainTask!;
        private set => this.theMainTask = value;
    }

    /// <summary>
    /// Gets the pausable task associated with this activity. When non-null, pausing is supported.
    /// <para>
    /// This value never changes once the activity has started running
    /// </para>
    /// </summary>
    public AdvancedPausableTask? PausableTask {
        get => this.myInternalPausableTask;
        internal set {
            AdvancedPausableTask? oldPausableTask = this.myInternalPausableTask;
            if (oldPausableTask != value) {
                this.myInternalPausableTask = value;
                this.PausableTaskChanged?.Invoke(this, oldPausableTask, value);
            }
        }
    }

    /// <summary>
    /// Gets whether this activity is being shown in one or more foreground dialogs via <see cref="IForegroundActivityService"/>
    /// </summary>
    public bool IsPresentInDialog => this.dialogPresence > 0;

    /// <summary>
    /// An event fired when <see cref="PausableTask"/> changes. This event is fired from a task thread,
    /// so handlers must jump onto main thread via <see cref="ApplicationPFX.Dispatcher"/> if they need to
    /// </summary>
    public event ActivityTaskPausableTaskChangedEventHandler? PausableTaskChanged;

    /// <summary>
    /// An event fired when <see cref="IsCompleted"/> changes. This event is fired on the application main thread.
    /// </summary>
    public event ActivityTaskEventHandler? IsCompletedChanged;

    /// <summary>
    /// An event fired when <see cref="IsPresentInDialog"/> changes. This event is fired on the application main thread.
    /// </summary>
    public event ActivityTaskEventHandler? IsPresentInDialogChanged;

    protected ActivityTask(ActivityManager activityManager, Func<Task> action, IActivityProgress activityProgress, CancellationTokenSource? cancellationTokenSource) {
        this.activityManager = activityManager ?? throw new ArgumentNullException(nameof(activityManager));
        this.action = action ?? throw new ArgumentNullException(nameof(action));
        this.Progress = activityProgress ?? throw new ArgumentNullException(nameof(activityProgress));
        this.cancellationTokenSource = cancellationTokenSource;
        this.CancellationToken = cancellationTokenSource?.Token ?? CancellationToken.None;
        this.tcsStarted = new TaskCompletionSource<ActivityTask>();
    }

    protected void ValidateNotStarted() {
        if (this.state != 0)
            throw new InvalidOperationException("An activity task cannot be restarted");
    }

    protected virtual Task CreateTask() {
        this.ValidateNotStarted();

        // We don't provide the cancellation token, because we want to handle it
        // separately. Awaiting this activity task should never throw an exception
        return Task.Factory.StartNew(this.TaskMain, CancellationToken.None).Unwrap();
    }

    /// <summary>
    /// Returns an awaiter used to await completion of this activity. Awaiting this will not throw <see cref="OperationCanceledException"/>
    /// </summary>
    /// <returns>The awaiter</returns>
    public TaskAwaiter GetAwaiter() => this.Task.GetAwaiter();

    /// <summary>
    /// Returns an awaitable that can be used to await when this activity has started (i.e. <see cref="IsRunning"/> becomes true).
    /// Awaiting this will not throw <see cref="OperationCanceledException"/>
    /// </summary>
    /// <returns>The awaitable</returns>
    public RunningTaskAwaitable GetRunningAwaitable() => new RunningTaskAwaitable(this);

    protected async Task TaskMain() {
        try {
            await ActivityManager.InternalPreActivateTask(this.activityManager, this);
            this.CheckCancelled();
            await (this.userTask = this.action());
            await this.OnCompleted(null);
        }
        catch (OperationCanceledException e) {
            await this.OnCancelled(e);
        }
        catch (Exception e) {
            await this.OnCompleted(e);
        }
    }

    public void CheckCancelled() => this.CancellationToken.ThrowIfCancellationRequested();

    public bool TryCancel() {
        if (this.cancellationTokenSource == null) {
            return false;
        }

        try {
            this.cancellationTokenSource.Cancel();
        }
        catch (ObjectDisposedException) {
            // ignored
        }

        return true;
    }

    protected virtual Task OnCancelled(OperationCanceledException e) {
        return ActivityManager.InternalOnActivityCompleted(this.activityManager, this, 3);
    }

    protected virtual async Task OnCompleted(Exception? e) {
        this.exception = e;
        await ActivityManager.InternalOnActivityCompleted(this.activityManager, this, 2);
    }

    internal static ActivityTask InternalStartActivity(ActivityManager activityManager, Func<Task> action, IActivityProgress? progress, CancellationTokenSource? cts, AdvancedPausableTask? pausableTask = null) {
        ActivityTask task = new ActivityTask(activityManager, action, progress ?? new DispatcherActivityProgress(), cts) { myInternalPausableTask = pausableTask };
        if (pausableTask != null)
            pausableTask.activity = task;
        return InternalStartActivityImpl(task);
    }

    internal static ActivityTask InternalStartActivityImpl(ActivityTask task) {
        task.Task = task.CreateTask();
        return task;
    }

    public static void InternalPostActivate(ActivityTask task) {
        task.state = 1;
        task.tcsStarted.SetResult(task);
    }

    public static void InternalComplete(ActivityTask task, int state) {
        task.state = state;
    }

    public static void InternalOnPresentInDialogChanged(ActivityTask task, bool isPresent) {
        Debug.Assert(isPresent ? (task.dialogPresence >= 0) : (task.dialogPresence > 0), "Excessive calls to " + nameof(InternalOnPresentInDialogChanged));
        ApplicationPFX.Instance.Dispatcher.VerifyAccess();

        int oldValue = task.dialogPresence;
        task.dialogPresence += (isPresent ? 1 : -1);
        if (oldValue == (isPresent ? 0 : 1)) {
            task.IsPresentInDialogChanged?.Invoke(task);
            ActivityManager.InternalOnIsPresentInDialogChanged(task.activityManager, task);
        }
    }

    internal void InternalOnCompletedOnMainThread() => this.IsCompletedChanged?.Invoke(this);

    public readonly struct RunningTaskAwaitable(ActivityTask task) {
        public TaskAwaiter<ActivityTask> GetAwaiter() => task.tcsStarted.Task.GetAwaiter();
    }

    public override string ToString() {
        string textState = this.state switch {
            0 => "Waiting for activation",
            1 => "Running",
            2 => "Completed",
            3 => "Cancelled",
            _ => "INVALID"
        };

        return $"{this.GetType().Name}({textState}, {this.Progress.Caption}: {this.Progress.Text})";
    }
}

// This system isn't great but it just about works... i'd rather not use public new ... methods but oh well

public class ActivityTask<T> : ActivityTask {
    private Result<T> myResult;

    public new Task<Result<T>> Task => (Task<Result<T>>) this.theMainTask!;

    protected ActivityTask(ActivityManager activityManager, Func<Task<T>> action, IActivityProgress activityProgress, CancellationTokenSource? cts) : base(activityManager, action, activityProgress, cts) {
    }

    internal static ActivityTask<T> InternalStartActivity(ActivityManager activityManager, Func<Task<T>> action, IActivityProgress? progress, CancellationTokenSource? cts) {
        return (ActivityTask<T>) InternalStartActivityImpl(new ActivityTask<T>(activityManager, action, progress ?? new DispatcherActivityProgress(), cts));
    }

    /// <summary>
    /// Returns a task awaiter that can be used to wait for the activity to become completed or faulted.
    /// </summary>
    /// <returns>
    /// An awaiter whose result is a <see cref="Result{T}"/> value, which can either contain the
    /// successful value or an exception (which may be <see cref="OperationCanceledException"/>)
    /// </returns>
    public new TaskAwaiter<Result<T>> GetAwaiter() => this.Task.GetAwaiter();

    protected override Task CreateTask() {
        this.ValidateNotStarted();
        return System.Threading.Tasks.Task.Factory.
                      StartNew(this.TaskMain, CancellationToken.None).
                      Unwrap().
                      ContinueWith(x => this.myResult, TaskContinuationOptions.ExecuteSynchronously);
    }

    protected override Task OnCancelled(OperationCanceledException e) {
        this.myResult = Result<T>.FromException(e);
        return base.OnCancelled(e);
    }

    protected override Task OnCompleted(Exception? e) {
        this.myResult = e != null
            ? Result<T>.FromException(e)
            : Result<T>.FromValue(((Task<T>) this.UserTask!).Result);
        return base.OnCompleted(e);
    }
}