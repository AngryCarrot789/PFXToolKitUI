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
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.Tasks.Pausable;

public delegate void AdvancedPausableTaskEventHandler(AdvancedPausableTask task);

public delegate Task AdvancedPausableTaskAsyncEventHandler(AdvancedPausableTask task);

/// <summary>
/// The base class for an advanced pausable task. An advanced pausable task provides
/// a "Run First" and "Continue" method. This allows you to, for example, release locks
/// </summary>
public abstract class AdvancedPausableTask : BasePausableTask {
    private enum TASK_STATE : byte {
        WAITING, // activity is not yet running
        RUNNING, // activity is running, just about to call RunFirst
        PAUSED_1, // activity is in paused state, before OnPaused() called
        PAUSED_2, // truely paused, OnPaused() invoked
        COMPLETED_1, // about to complete, OnCompleted() not called yet
        COMPLETED_2, // activity has fully completed successfully
        CANCELLED, // activity was cancelled
        EXCEPTION // activity completed due to an unhandled exception
    }

    private enum PAUSE_STATE : byte {
        NOT_REQUESTED, // No pause request yet
        REQUESTED, // Pause() called
        PAUSED, // Activity is paused
        UNPAUSE_REQUESTED, // Resume() called
        CONTINUE, // Activity is no longer paused
    }

    private readonly bool isIndicatedAsCancellable;
    private volatile TASK_STATE state;
    private volatile PAUSE_STATE pauseState;
    private volatile int reqToPauseCount, reqToUnpauseCount;
    private volatile int continueAttempts;

    internal ActivityTask? activity;
    private volatile Task? firstTask, continueTask;
    private readonly object stateLock = new object();
    private volatile Exception? exception;

    private readonly LinkedList<TaskCompletionSource> completionNotifications = [];

    private CancellationTokenSource? actualCts;
    private bool isOwnerOfActivity;
    
    private CancellationTokenSource? myPauseOrCancelSource;
    private CancellationTokenRegistration? pauseCancellationRegistration;

    /// <summary>
    /// Returns this task's activity.
    /// </summary>
    /// <exception cref="InvalidOperationException"><see cref="Run"/> not called yet</exception>
    public ActivityTask Activity => this.activity ?? throw new InvalidOperationException("Not started yet");

    /// <summary>
    /// Returns true when this task is absolutely paused and no code is running.
    /// Due to tasks being drizzled with async and involving locked states, this should only
    /// be checked from known checkpoints, e.g. during a <see cref="PausedStateChanged"/> handler
    /// </summary>
    public bool IsPaused => this.state == TASK_STATE.PAUSED_2;

    /// <summary>
    /// Returns true when completed for any reason (success, cancellation or exception)
    /// </summary>
    public bool IsCompleted => this.state >= TASK_STATE.COMPLETED_2;

    /// <summary>
    /// Returns true when completed due to task cancellation
    /// </summary>
    public bool IsCompletedByCancellation => this.state == TASK_STATE.CANCELLED;

    /// <summary>
    /// Gets the exception encountered during task execution. When non-null, <see cref="IsCompleted"/> returns true
    /// </summary>
    public Exception? Exception => this.exception;

    /// <summary>
    /// Returns true when this task is cancellable
    /// </summary>
    public bool IsCancellable => this.actualCts != null;

    /// <summary>
    /// Returns true when this task is cancellable and has been marked for cancellation (<see cref="CancellationTokenSource.Cancel()"/> was called)
    /// </summary>
    public bool IsCancellationRequested => this.actualCts?.IsCancellationRequested ?? false;

    /// <summary>
    /// Gets the true cancellation token that becomes cancelled when the user clicks the cancel button.
    /// Returns <see cref="System.Threading.CancellationToken.None"/> when not cancellable
    /// </summary>
    public CancellationToken CancellationToken => this.actualCts?.Token ?? CancellationToken.None;

    /// <summary>
    /// An event fired when the paused state of this task changes. This event is fired from
    /// a task thread, so handlers must jump onto main thread via <see cref="ApplicationPFX.Dispatcher"/>
    /// if they need to
    /// </summary>
    public event AdvancedPausableTaskAsyncEventHandler? PausedStateChanged;

    /// <summary>
    /// Constructor for <see cref="AdvancedPausableTask"/>
    /// </summary>
    /// <param name="isCancellable">
    /// A hint to whether this task is pausable. This task may not actually be cancellable if
    /// ran via <see cref="RunWithCurrentActivity"/> and the current activity is not cancellable.
    /// However, it will still be pausable
    /// </param>
    protected AdvancedPausableTask(bool isCancellable) {
        this.isIndicatedAsCancellable = isCancellable;
    }

    /// <summary>
    /// Runs the operation for the first time. This is guaranteed to run before <see cref="OnPaused"/> or <see cref="OnCompleted"/>
    /// </summary>
    /// <param name="pauseOrCancelToken">A token that becomes 'cancelled' when this task is either paused or actually cancelled</param>
    /// <returns>The first run of the activity, and hopefully the only run. When this returns normally, the activity is completed</returns>
    /// <exception cref="OperationCanceledException">The operation was paused or cancelled</exception>
    protected internal abstract Task RunFirst(CancellationToken pauseOrCancelToken);

    /// <summary>
    /// Continues running this task after being paused. This can for example re-obtain locks
    /// </summary>
    /// <param name="pauseOrCancelToken">A token that becomes 'cancelled' when this task is either paused or actually cancelled</param>
    /// <returns>The second+ run of the activity. When this returns normally, the activity is completed</returns>
    /// <exception cref="OperationCanceledException">The operation was paused or cancelled</exception>
    protected internal abstract Task Continue(CancellationToken pauseOrCancelToken);

    /// <summary>
    /// Runs this task as a complete activity operation
    /// </summary>
    /// <returns>The entire operation</returns>
    public ActivityTask Run(IActivityProgress? progress = null) {
        this.actualCts = this.isIndicatedAsCancellable ? new CancellationTokenSource() : null;
        this.isOwnerOfActivity = true;
        return ActivityTask.InternalStartActivity(ActivityManager.Instance, this.RunMainInternal, progress ?? new DefaultProgressTracker(), this.actualCts, TaskCreationOptions.None, this);
    }

    /// <summary>
    /// Runs this task within the <see cref="ActivityManager.CurrentTask"/>
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">
    /// No activity on the current thread or the activity already has
    /// a task running alongside it, and it has not completed, or this
    /// pausable task has already completed and cannot run again
    /// </exception>
    /// <exception cref="OperationCanceledException">The activity was cancelled</exception>
    public async Task RunWithCurrentActivity() {
        if (!ActivityManager.Instance.TryGetCurrentTask(out ActivityTask? theActivity)) {
            throw new InvalidOperationException("No activity on the current thread");
        }

        if (theActivity.PausableTask != null && !theActivity.PausableTask.IsCompleted) {
            throw new InvalidOperationException("Activity already has an incomplete pausable task associated");
        }

        if (this.state != TASK_STATE.WAITING || this.activity != null) {
            throw new InvalidOperationException("This pausable task is already running with another activity");
        }

        this.isOwnerOfActivity = false;
        this.actualCts = this.isIndicatedAsCancellable ? theActivity.cancellationTokenSource : null;
        if (this.isIndicatedAsCancellable && this.actualCts == null) {
            Debug.WriteLine($"Warning: {this.GetType().FullName} is cancellable but was run within an activity that was not");
        }

        this.activity = theActivity;
        theActivity.PausableTask = this;

        OperationCanceledException? oce = null;
        using ErrorList errorList = new ErrorList("Multiple exceptions while awaiting pausable task", throwOnDispose:false, tryUseFirstException:true);
        try {
            await this.RunMainInternal();
        }
        catch (OperationCanceledException e) {
            oce = e;
        }
        catch (Exception e) {
            errorList.Add(e);
        }
            
        Debug.Assert(this.IsCompleted, "Expected this pausable task to be completed at this point");
        this.activity = null;

        try {
            theActivity.PausableTask = null;
        }
        catch (Exception e) {
            errorList.Add(e);
        }

        if (errorList.TryGetException(out Exception? ex)) {
            throw ex;
        }
        else if (oce != null) {
            throw oce;
        }
    }

    private async Task OnCancelledInternal() {
        lock (this.stateLock) {
            this.state = TASK_STATE.CANCELLED;
            this.pauseState = PAUSE_STATE.NOT_REQUESTED;
        }

        await this.OnCompletedInternal();
    }

    private async Task OnPausedInternalAsync(bool isFirst) {
        lock (this.stateLock) {
            this.pauseState = PAUSE_STATE.PAUSED;
            this.state = TASK_STATE.PAUSED_1;
        }

        await this.OnPaused(isFirst);
        lock (this.stateLock) {
            this.state = TASK_STATE.PAUSED_2;
        }

        await this.RaiseOnPausedChanged();
    }

    private async Task RaiseOnPausedChanged() {
        Delegate[]? handler = this.PausedStateChanged?.GetInvocationList();
        if (handler != null) {
            foreach (Delegate t in handler) {
                await ((AdvancedPausableTaskAsyncEventHandler) t)(this);
            }
        }
    }

    /// <summary>
    /// Pauses this task and waits for it to actually become paused (<see cref="OnPaused"/> will have completed)
    /// </summary>
    public async Task PauseAsync() {
        lock (this.stateLock) {
            TASK_STATE s = this.state;
            if (s >= TASK_STATE.COMPLETED_1) {
                goto WaitForCompletion;
            }

            if (s == TASK_STATE.PAUSED_1 || s == TASK_STATE.PAUSED_2) {
                return;
            }

            Interlocked.Increment(ref this.reqToPauseCount);
            this.pauseState = PAUSE_STATE.REQUESTED;
            this.MarkPausedOrCancelled();
        }

        await this.PauseOrResumeWaitInternal(true);

        return;
        WaitForCompletion:
        await this.WaitForCompletion();
    }

    /// <summary>
    /// Resumes execution of the task, and waits for the task to enter its un-pausing state.
    /// <para>
    /// Must not be called excessively
    /// </para>
    /// </summary>
    public async Task Resume() {
        lock (this.stateLock) {
            TASK_STATE s = this.state;
            if (s >= TASK_STATE.COMPLETED_1) {
                goto WaitForCompletion;
            }

            if ((s != TASK_STATE.PAUSED_1 && s != TASK_STATE.PAUSED_2)) {
                return;
            }

            if (this.reqToPauseCount == 0)
                Debugger.Break(); // Excessive calls to Resume()

            Interlocked.Decrement(ref this.reqToPauseCount);
            Interlocked.Increment(ref this.reqToUnpauseCount);
        }

        await this.PauseOrResumeWaitInternal(false);

        return;
        WaitForCompletion:
        await this.WaitForCompletion();
    }

    /// <summary>
    /// Toggles the paused state of this task. If we're paused, we continue. Otherwise, we pause.
    /// </summary>
    /// <returns>
    /// A task that waits for the new state. The result is the IsPaused state, or null if the task has not
    /// been activated yet (started but not running) or has completed
    /// </returns>
    public async Task<bool?> TogglePaused() {
        bool isPausingTask;
        lock (this.stateLock) {
            TASK_STATE s = this.state;
            switch (s) {
                case TASK_STATE.WAITING: return null;
                case TASK_STATE.RUNNING: {
                    Interlocked.Increment(ref this.reqToPauseCount);
                    this.pauseState = PAUSE_STATE.REQUESTED;
                    this.myPauseOrCancelSource!.Cancel();
                    isPausingTask = true;
                    break;
                }
                case TASK_STATE.PAUSED_1:
                case TASK_STATE.PAUSED_2: {
                    if (this.reqToPauseCount == 0)
                        Debugger.Break(); // Excessive calls to Resume()

                    Interlocked.Decrement(ref this.reqToPauseCount);
                    Interlocked.Increment(ref this.reqToUnpauseCount);
                    isPausingTask = false;
                    break;
                }
                default: goto WaitForCompletion;
            }
        }

        return await this.PauseOrResumeWaitInternal(isPausingTask);

        WaitForCompletion:
        await this.WaitForCompletion();
        return null;
    }

    /// <summary>
    /// Operates a function while paused. Operating when completed from an error or cancelled is optional
    /// </summary>
    /// <param name="operation">The operation</param>
    /// <param name="canOperateOnError">True to allow operating when completed due to an exception</param>
    /// <param name="canOperateOnCancelled">True to allow operating when completed due to cancellation</param>
    /// <returns>A task which contains the result</returns>
    public async Task OperateWhilePaused(Func<Task> operation, bool canOperateOnError = true, bool canOperateOnCancelled = true) {
        await this.OperateWhilePausedInternal(operation, canOperateOnError, canOperateOnCancelled);
    }

    /// <summary>
    /// Operates a function while paused, and returns a value. Operating when completed from an error
    /// or cancelled is optional, and when disallowed, returns default(T)
    /// </summary>
    /// <param name="operation">The operation</param>
    /// <param name="canOperateOnError">True to allow operating when completed due to an exception</param>
    /// <param name="canOperateOnCancelled">True to allow operating when completed due to cancellation</param>
    /// <typeparam name="T">The result value type</typeparam>
    /// <returns></returns>
    public async Task<T?> OperateWhilePaused<T>(Func<Task<T>> operation, bool canOperateOnError = true, bool canOperateOnCancelled = true) {
        Task task = await this.OperateWhilePausedInternal(operation, canOperateOnError, canOperateOnCancelled);
        if (task is Task<T> operationTask) {
            Debug.Assert(operationTask.IsCompleted);
            return await operationTask;
        }

        return default;
    }

    private async Task<Task> OperateWhilePausedInternal(Func<Task> operation, bool canOperateOnError = true, bool canOperateOnCancelled = true) {
        lock (this.stateLock) {
            switch (this.state) {
                case TASK_STATE.WAITING:     goto OperateAbs;
                case TASK_STATE.RUNNING:     goto PauseOperateUnpause;
                case TASK_STATE.PAUSED_1:
                case TASK_STATE.PAUSED_2:    goto WaitForPausedThenOperate;
                case TASK_STATE.COMPLETED_1: goto WaitForCompletionThenOperate;
                case TASK_STATE.COMPLETED_2: goto OperateAbs;
                case TASK_STATE.CANCELLED: {
                    if (canOperateOnCancelled)
                        goto OperateAbs;
                    return Task.CompletedTask;
                }
                case TASK_STATE.EXCEPTION: {
                    if (canOperateOnError)
                        goto OperateAbs;
                    return Task.CompletedTask;
                }
                default: {
                    Debug.Fail("Unknown state");
                    return Task.CompletedTask;
                }
            }
        }

        PauseOperateUnpause:
        await this.PauseAsync();
        Task task = operation();
        try {
            await task;
        }
        finally {
            await this.Resume();
        }

        return task;

        WaitForPausedThenOperate:
        while (this.state != TASK_STATE.PAUSED_2) {
            await Task.Delay(10);
        }

        task = operation();
        await task;
        return task;

        WaitForCompletionThenOperate:
        await this.WaitForCompletion();

        OperateAbs:
        task = operation();
        await task;
        return task;
    }

    private async Task<bool?> PauseOrResumeWaitInternal(bool isPausingTask) {
        while (true) {
            lock (this.stateLock) {
                TASK_STATE s = this.state;
                // Marked as completed, so wait for completion
                if (s >= TASK_STATE.COMPLETED_1) {
                    goto WaitForCompletion;
                }

                if (isPausingTask) {
                    // we're pausing so either wait for it to become fully paused, or wait for completion
                    if (s == TASK_STATE.PAUSED_2) {
                        return true;
                    }
                }
                else {
                    // Activity is continuing execution. We must decrement the unpause count
                    // otherwise the activity will be stuck forever (unless cancelled)
                    if (this.pauseState == PAUSE_STATE.CONTINUE) {
                        if (this.reqToUnpauseCount == 0)
                            Debugger.Break(); // Excessive calls to Resume()

                        Interlocked.Decrement(ref this.reqToUnpauseCount);
                        return false;
                    }
                }
            }

            await Task.Delay(10);
        }

        Debug.Fail("Unreachable case");
        return null;

        WaitForCompletion:
        await this.WaitForCompletion();
        return null;
    }

    /// <summary>
    /// Tries to request this task to be cancelled
    /// </summary>
    /// <returns></returns>
    public bool RequestCancellation() {
        if (this.actualCts == null)
            return false;

        try {
            this.actualCts.Cancel();
        }
        catch (ObjectDisposedException) {
            // ignored
        }

        return true;
    }
    
    public async Task CancelAsync() {
        if (this.actualCts == null)
            throw new InvalidOperationException("Not cancellable");

        try {
            await this.actualCts.CancelAsync().ConfigureAwait(false);
        }
        catch (ObjectDisposedException) {
            return;
        }

        await this.WaitForCompletion().ConfigureAwait(false);
    }

    /// <summary>
    /// Returns a task that completes when this task is completed, either successfully, exceptionally or was cancelled
    /// </summary>
    public async Task WaitForCompletion() {
        if (this.state >= TASK_STATE.COMPLETED_2) {
            return; // task became completed
        }
        
        LinkedListNode<TaskCompletionSource> node;
        lock (this.completionNotifications) {
            if (this.state >= TASK_STATE.COMPLETED_2) {
                return; // task became completed
            }

            node = this.completionNotifications.AddLast(new TaskCompletionSource());
        }

        await node.Value.Task.ConfigureAwait(false);
    }

    /// <summary>
    /// Invoked when the task is paused
    /// </summary>
    /// <param name="isFirst">Paused for the first time</param>
    protected abstract Task OnPaused(bool isFirst);

    /// <summary>
    /// Invoked when this task completes, either successfully, cancelled or with an exception (see <see cref="Exception"/>)
    /// </summary>
    /// <returns></returns>
    protected abstract Task OnCompleted();

    private async Task OnCompletedInternal() {
        // Dispose callback from true cancellation ts
        if (this.pauseCancellationRegistration.HasValue)
            await this.pauseCancellationRegistration.Value.DisposeAsync();

        this.myPauseOrCancelSource?.Dispose();
        
        if (this.isOwnerOfActivity) {
            this.actualCts?.Dispose();
        }
        else {
            this.actualCts = null;
        }

        try {
            await this.OnCompleted();
        }
        finally {
            this.state = TASK_STATE.COMPLETED_2;

            List<TaskCompletionSource> items;
            lock (this.completionNotifications) {
                items = this.completionNotifications.ToList();
                this.completionNotifications.Clear();
            }

            foreach (TaskCompletionSource tcs in items) {
                tcs.TrySetResult();
            }
        }
    }

    private async Task RunMainInternal() {
        CancellationToken trueCancelToken;
        if (this.actualCts != null) {
            trueCancelToken = this.actualCts.Token;

            // Register callback with actual cancellation ts to also 'cancel' the pause token, pausing the task again
            this.pauseCancellationRegistration = trueCancelToken.Register(this.MarkPausedOrCancelled);
        }
        else {
            trueCancelToken = CancellationToken.None;
        }

        lock (this.stateLock) {
            this.state = TASK_STATE.RUNNING;
            this.myPauseOrCancelSource = new CancellationTokenSource();
        }

        CancellationToken pauseOrCancelToken = this.myPauseOrCancelSource.Token;
        try {
            this.continueAttempts = -1;
            await (this.firstTask = this.RunFirst(pauseOrCancelToken));
        }
        catch (OperationCanceledException e) {
            if (this.pauseState == PAUSE_STATE.REQUESTED && !this.IsCancellationRequested) {
                await this.OnPausedInternalAsync(false);
            }
            else {
                await this.OnCancelledInternal();
                throw;
            }
        }
        catch (Exception e) {
            lock (this.stateLock) {
                this.exception = e;
                this.state = TASK_STATE.EXCEPTION;
            }

            await this.OnCompletedInternal();
            return;
        }

        bool ranToSuccess = false;
        lock (this.stateLock) {
            if (this.state == TASK_STATE.RUNNING) {
                this.state = TASK_STATE.COMPLETED_1;
                ranToSuccess = true;
            }
        }

        if (ranToSuccess) {
            await this.OnCompletedInternal();
            return;
        }

        while (true) {
            if (this.state == TASK_STATE.PAUSED_2) {
                // This could probably be massively simplified using semaphores or something similar...
                while (true) {
                    lock (this.stateLock) {
                        if (this.reqToPauseCount < 1) {
                            // No more requests to stay paused, so mark as CONTINUE.
                            // Resume() callers notice this, and decrement reqToUnpauseCount
                            this.pauseState = PAUSE_STATE.CONTINUE;
                            break;
                        }
                    }

                    await this.WaitSomeOrCancel(trueCancelToken);
                }

                while (true) {
                    lock (this.stateLock) {
                        if (this.reqToUnpauseCount < 1) {
                            // All Resume() waiters are done, so 
                            this.pauseState = PAUSE_STATE.CONTINUE;
                            this.state = TASK_STATE.RUNNING;

                            // re-create pause token source, since it will definitely be cancelled at this point
                            this.myPauseOrCancelSource?.Dispose();
                            this.myPauseOrCancelSource = new CancellationTokenSource();
                            pauseOrCancelToken = this.myPauseOrCancelSource.Token;
                            break;
                        }
                    }

                    await this.WaitSomeOrCancel(trueCancelToken);
                }
            }

            await this.RaiseOnPausedChanged();

            try {
                Interlocked.Increment(ref this.continueAttempts);
                await (this.continueTask = this.Continue(pauseOrCancelToken));
            }
            catch (OperationCanceledException e) {
                if (this.pauseState == PAUSE_STATE.REQUESTED && !this.IsCancellationRequested) {
                    await this.OnPausedInternalAsync(false);
                    continue;
                }
                else {
                    await this.OnCancelledInternal();
                    throw;
                }
            }
            catch (Exception e) {
                lock (this.stateLock) {
                    this.exception = e;
                    this.state = TASK_STATE.EXCEPTION;
                }

                await this.OnCompletedInternal();
                return;
            }

            lock (this.stateLock) {
                Debug.Assert(this.state == TASK_STATE.RUNNING);
                this.state = TASK_STATE.COMPLETED_1;
            }

            await this.OnCompletedInternal();
            return;
        }
    }

    private void MarkPausedOrCancelled() {
        this.myPauseOrCancelSource?.Cancel();
    }

    private async Task WaitSomeOrCancel(CancellationToken token) {
        try {
            token.ThrowIfCancellationRequested();
            await Task.Delay(10, token);
        }
        catch (OperationCanceledException) {
            await this.OnCancelledInternal();
            throw;
        }
    }
}