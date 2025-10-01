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
using PFXToolKitUI.Activities.Pausable;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.Activities;

public delegate void ActivityTaskEventHandler(ActivityTask sender);

public delegate void ActivityTaskPausableTaskChangedEventHandler(ActivityTask sender, AdvancedPausableTask? oldPausableTask, AdvancedPausableTask? newPausableTask);

/// <summary>
/// Represents a task that can be run by a <see cref="ActivityManager"/> on a background thread
/// </summary>
[DebuggerDisplay("{ToString()}")]
public class ActivityTask {
    internal enum EnumRunState { Waiting, Running, Completed, Cancelled }
    
    private readonly ActivityManager activityManager;
    private readonly Func<Task> myAction;  // user-specified procedure
    internal readonly CancellationTokenSource? myCts;
    private readonly TaskCompletionSource<ActivityTask> myTcsForRunning;
    private EnumRunState myState; // Current running state
    private Task? myMainTask;     // task we created
    private int myDialogPresence; // Counts how many dialogs the activity is present in (via IForegroundActivityService)
    private AdvancedPausableTask? myPausableTask; // A ref to the APT that runs in this activity

    /// <summary>
    /// Gets the activity running in the current asynchronous control flow
    /// </summary>
    /// <exception cref="InvalidOperationException">No task in the current control flow</exception>
    /// <remarks>This delegates to <see cref="ActivityManager.CurrentTask"/></remarks>
    public static ActivityTask Current => ActivityManager.Instance.CurrentTask;
    
    /// <summary>
    /// Returns true if the task is currently still running
    /// </summary>
    public bool IsRunning => this.myState == EnumRunState.Running;

    /// <summary>
    /// Returns true if the task is completed. <see cref="Exception"/> may be non-null when this is true
    /// </summary>
    public bool IsCompleted => this.myState > EnumRunState.Running;

    /// <summary>
    /// Returns true when this activity was completed due to cancellation
    /// </summary>
    public bool IsCancelled => this.myState == EnumRunState.Cancelled;

    /// <summary>
    /// Returns true when this task can be cancelled via <see cref="TryCancel"/>
    /// </summary>
    public bool IsDirectlyCancellable => this.myCts != null;

    /// <summary>
    /// Returns true when cancellation is requested 
    /// </summary>
    public bool IsCancellationRequested => this.IsDirectlyCancellable && this.CancellationToken.IsCancellationRequested;

    /// <summary>
    /// Gets the exception that was thrown during the execution of the user action
    /// </summary>
    public Exception? Exception { get; private set; }

    /// <summary>
    /// Gets the progress handler associated with this task. Will always be non-null
    /// </summary>
    public IActivityProgress Progress { get; }

    /// <summary>
    /// Gets the first token created from our <see cref="CancellationTokenSource"/>
    /// </summary>
    public CancellationToken CancellationToken { get; }

    /// <summary>
    /// Gets this activity's task, which can be used to await completion.
    /// This task is a proxy of the user-specified function.
    /// <para>
    /// This task will always complete successfully.
    /// </para>
    /// </summary>
    public Task Task => this.myMainTask!;

    /// <summary>
    /// Gets the pausable task associated with this activity. When non-null, pausing is supported.
    /// </summary>
    public AdvancedPausableTask? PausableTask {
        get => this.myPausableTask;
        internal set {
            AdvancedPausableTask? oldPausableTask = this.myPausableTask;
            if (oldPausableTask != value) {
                this.myPausableTask = value;
                this.PausableTaskChanged?.Invoke(this, oldPausableTask, value);
            }
        }
    }

    /// <summary>
    /// Gets whether this activity is being shown in one or more foreground dialogs via <see cref="IForegroundActivityService"/>
    /// </summary>
    public bool IsPresentInDialog => this.myDialogPresence > 0;

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

    protected ActivityTask(ActivityManager activityManager, Func<Task> action, IActivityProgress activityProgress, CancellationTokenSource? cancellation) {
        this.activityManager = activityManager ?? throw new ArgumentNullException(nameof(activityManager));
        this.myAction = action ?? throw new ArgumentNullException(nameof(action));
        this.Progress = activityProgress ?? throw new ArgumentNullException(nameof(activityProgress));
        this.myCts = cancellation;
        this.CancellationToken = cancellation?.Token ?? CancellationToken.None;
        this.myTcsForRunning = new TaskCompletionSource<ActivityTask>();
    }
    
    private async Task TaskMain() {
        Task? userTask = null;
        
        try {
            await ActivityManager.InternalActivateTask(this.activityManager, this);
            this.ThrowIfCancellationRequested();
            await (userTask = this.myAction());
            await this.OnCompleted(null, userTask);
        }
        catch (OperationCanceledException e) {
            await this.OnCancelled(e, userTask);
        }
        catch (Exception e) {
            await this.OnCompleted(e, userTask);
        }
    }

    protected void ValidateNotStarted() {
        if (this.myState != EnumRunState.Waiting)
            throw new InvalidOperationException("An activity task cannot be restarted");
    }

    /// <summary>
    /// Creates the user task
    /// </summary>
    /// <param name="mainTask">The main function for the activity, which must be called</param>
    /// <returns>A delegate task to <see cref="mainTask"/></returns>
    protected virtual Task RunMainTask(Func<Task> mainTask) {
        this.ValidateNotStarted();
        return Task.Run(mainTask, CancellationToken.None);
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

    /// <summary>
    /// Throws <see cref="OperationCanceledException"/> if our <see cref="CancellationToken"/> is cancelled
    /// </summary>
    public void ThrowIfCancellationRequested() => this.CancellationToken.ThrowIfCancellationRequested();

    public bool TryCancel() {
        if (this.myCts == null) {
            return false;
        }

        try {
            this.myCts.Cancel();
        }
        catch (ObjectDisposedException) {
            // ignored
        }

        return true;
    }

    protected virtual Task OnCancelled(OperationCanceledException e, Task? userTask) {
        return ActivityManager.InternalOnActivityCompleted(this.activityManager, this, EnumRunState.Cancelled);
    }

    protected virtual async Task OnCompleted(Exception? e, Task? userTask) {
        this.Exception = e;
        await ActivityManager.InternalOnActivityCompleted(this.activityManager, this, EnumRunState.Completed);
    }

    internal static ActivityTask InternalStartActivity(ActivityManager activityManager, Func<Task> action, IActivityProgress? progress, CancellationTokenSource? cancellation, AdvancedPausableTask? pausableTask = null) {
        ActivityTask task = new ActivityTask(activityManager, action, progress ?? new DispatcherActivityProgress(), cancellation) { myPausableTask = pausableTask };
        if (pausableTask != null)
            pausableTask.activity = task;
        return InternalStartActivityImpl(task);
    }

    internal static ActivityTask InternalStartActivityImpl(ActivityTask task) {
        task.myMainTask = task.RunMainTask(task.TaskMain);
        return task;
    }

    internal static void InternalOnActivated(ActivityTask task) {
        task.myState = EnumRunState.Running;
        task.myTcsForRunning.SetResult(task);
    }

    internal static void InternalPreComplete(ActivityTask task, EnumRunState state) => task.myState = state;

    internal static void InternalPostCompleted(ActivityTask task) => task.IsCompletedChanged?.Invoke(task);
    
    internal static void InternalOnPresentInDialogChanged(ActivityTask task, bool isPresent) {
        Debug.Assert(isPresent ? (task.myDialogPresence >= 0) : (task.myDialogPresence > 0), "Excessive calls to " + nameof(InternalOnPresentInDialogChanged));
        ApplicationPFX.Instance.Dispatcher.VerifyAccess();

        int oldValue = task.myDialogPresence;
        task.myDialogPresence += (isPresent ? 1 : -1);
        if (oldValue == (isPresent ? 0 : 1)) {
            task.IsPresentInDialogChanged?.Invoke(task);
            ActivityManager.InternalOnIsPresentInDialogChanged(task.activityManager, task);
        }
    }

    public readonly struct RunningTaskAwaitable(ActivityTask task) {
        public TaskAwaiter<ActivityTask> GetAwaiter() => task.myTcsForRunning.Task.GetAwaiter();
    }

    public override string ToString() {
        string textState = this.myState switch {
            EnumRunState.Waiting => "Waiting for activation",
            EnumRunState.Running => "Running",
            EnumRunState.Completed => "Completed",
            EnumRunState.Cancelled => "Cancelled",
            _ => "INVALID"
        };

        return $"{this.GetType().Name}({textState}, {this.Progress.Caption}: {this.Progress.Text})";
    }
}

// This system isn't great but it just about works... i'd rather not use public new ... methods but oh well

public sealed class ActivityTask<T> : ActivityTask {
    private Result<T> myResult;

    public new Task<Result<T>> Task => (Task<Result<T>>) base.Task;

    private ActivityTask(ActivityManager activityManager, Func<Task<T>> action, IActivityProgress activityProgress, CancellationTokenSource? cts) : base(activityManager, action, activityProgress, cts) {
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

    protected override Task RunMainTask(Func<Task> mainTask) {
        Task task = base.RunMainTask(mainTask);
        
        // Create a Task<T>
        return task.ContinueWith(static (_, a) => ((ActivityTask<T>) a!).myResult, this, TaskContinuationOptions.ExecuteSynchronously);
    }

    protected override Task OnCancelled(OperationCanceledException e, Task? userTask) {
        this.myResult = Result<T>.FromException(e);
        return base.OnCancelled(e, userTask);
    }

    protected override Task OnCompleted(Exception? e, Task? userTask) {
        this.myResult = e != null
            ? Result<T>.FromException(e)
            : Result<T>.FromValue(((Task<T>) userTask!).Result);
        return base.OnCompleted(e, userTask);
    }
}