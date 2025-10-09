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
using PFXToolKitUI.Logging;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.Activities.Pausable;

public delegate void AdvancedPausableTaskEventHandler(AdvancedPausableTask task);

public delegate Task AdvancedPausableTaskAsyncEventHandler(AdvancedPausableTask task);

/// <summary>
/// The base class for an advanced pausable task. An advanced pausable task provides
/// a "Run First" and "Continue" method. This allows you to, for example, release locks
/// </summary>
public abstract class AdvancedPausableTask : BasePausableTask {
    private enum TaskState : byte {
        WaitingForActivation, // activity is not yet running
        Running, // activity is running, just about to call RunFirst
        BeforePaused, // activity is in paused state, before OnPaused() called
        AfterPaused, // truely paused, OnPaused() invoked
        BeforeCompleted, // about to complete, OnCompleted() not called yet
        AfterCompleted, // activity has fully completed successfully
        BeforeCancelled, // activity was cancelled
        AfterCancelled, // activity was cancelled
        Faulted // activity completed due to an unhandled exception
    }

    private enum PauseState : byte {
        NotRequested, // No pause request yet
        Requested, // Pause() called
        Paused, // Activity is paused
        UnpauseRequested, // Resume() called
        Continue, // Activity is no longer paused
    }

    private readonly bool isIndicatedAsCancellable;
    private volatile TaskState state;
    private volatile PauseState pauseState;
    private volatile int reqToPauseCount, reqToUnpauseCount;

    internal ActivityTask? activity;
    private readonly Lock stateLock = new Lock();
    private volatile Exception? exception;

    private volatile TaskCompletionSource? myTcs;
    private volatile CancellationTokenSource? actualCts;
    private bool isOwnerOfActivity;

    private CancellationTokenSource? myPauseOrCancelSource;

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
    public bool IsPaused => this.state == TaskState.AfterPaused;

    /// <summary>
    /// Returns true when completed for any reason (success, cancellation or exception)
    /// </summary>
    public bool IsCompleted => this.state >= TaskState.AfterCompleted;

    /// <summary>
    /// Returns true when completed due to task cancellation
    /// </summary>
    public bool IsCompletedByCancellation => this.state == TaskState.AfterCancelled;

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
    /// Gets the true cancellation token that becomes cancelled when the user clicks the cancel button.
    /// Returns <see cref="System.Threading.CancellationToken.None"/> when not cancellable
    /// </summary>
    public CancellationToken PauseOrCancelToken { get; private set; }

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
    /// Runs the operation. This is guaranteed to run before <see cref="OnPaused"/> or <see cref="OnCompleted"/>
    /// </summary>
    /// <param name="pauseOrCancelToken">A token that becomes 'cancelled' when this task is either paused or actually cancelled</param>
    /// <param name="isFirstRun">True when this is the first run, false when the activity has been paused and then continued</param>
    /// <returns>The operation task</returns>
    /// <exception cref="OperationCanceledException">The operation was paused or cancelled</exception>
    protected internal abstract Task RunOperation(CancellationToken pauseOrCancelToken, bool isFirstRun);

    /// <summary>
    /// Runs this task as a complete activity operation
    /// </summary>
    /// <returns>The entire operation</returns>
    public ActivityTask Run(IActivityProgress? progress = null) {
        this.actualCts = this.isIndicatedAsCancellable ? new CancellationTokenSource() : null;
        this.isOwnerOfActivity = true;
        return ActivityTask.InternalStartActivity(ActivityManager.Instance, this.PausableTaskMain, progress ?? new DispatcherActivityProgress(), this.actualCts, false, this);
    }

    /// <summary>
    /// Runs this task within the <see cref="ActivityManager.CurrentTask"/>
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">
    /// No activity with the current async control flow, or the activity already has a pausable task
    /// running alongside it and it has not completed, or this pausable task has already completed
    /// and cannot run again
    /// </exception>
    /// <exception cref="OperationCanceledException">The activity was cancelled</exception>
    public async Task RunWithCurrentActivity() {
        if (!ActivityManager.Instance.TryGetCurrentTask(out ActivityTask? theActivity))
            throw new InvalidOperationException("No activity on the current thread");

        if (theActivity.PausableTask != null && !theActivity.PausableTask.IsCompleted)
            throw new InvalidOperationException("Activity already has an incomplete pausable task associated");

        if (this.state != TaskState.WaitingForActivation || this.activity != null)
            throw new InvalidOperationException("This pausable task is already running with another activity");

        this.isOwnerOfActivity = false;
        this.actualCts = this.isIndicatedAsCancellable ? theActivity.myCts : null;
        if (this.isIndicatedAsCancellable && this.actualCts == null) {
            Debug.WriteLine($"Warning: {this.GetType().FullName} is cancellable but was run within an activity that was not");
        }

        this.activity = theActivity;
        theActivity.PausableTask = this;

        OperationCanceledException? oce = null;
        using (ErrorList errorList = new ErrorList("Multiple exceptions while awaiting pausable task", throwOnDispose: true, tryUseFirstException: true)) {
            try {
                await this.PausableTaskMain();
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
        }

        if (oce != null) {
            throw oce;
        }
    }

    private async Task OnCancelledInternal() {
        lock (this.stateLock) {
            this.state = TaskState.BeforeCancelled;
            this.pauseState = PauseState.NotRequested;
        }

        await this.InternalCompleted(TaskState.AfterCancelled);
    }

    private async Task RaiseOnPausedChanged() {
        Delegate[]? handler = this.PausedStateChanged?.GetInvocationList();
        if (handler != null) {
            foreach (Delegate t in handler) {
                try {
                    await ((AdvancedPausableTaskAsyncEventHandler) t)(this);
                }
                catch (Exception e) {
                    AppLogger.Instance.WriteLine($"[Pausable Task] handler ({t}) threw: {e.GetToString()}");
                }
            }
        }
    }

    /// <summary>
    /// Pauses this task and waits for it to actually become paused (<see cref="OnPaused"/> will have completed)
    /// </summary>
    public async Task PauseAsync() {
        lock (this.stateLock) {
            TaskState s = this.state;
            if (s >= TaskState.BeforeCompleted) {
                goto WaitForCompletion;
            }

            if (s == TaskState.BeforePaused || s == TaskState.AfterPaused) {
                return;
            }

            Interlocked.Increment(ref this.reqToPauseCount);
            this.pauseState = PauseState.Requested;
            this.MarkPausedOrCancelled();
        }

        await this.PauseOrResumeWaitInternal(true);

        return;
        WaitForCompletion:
        await this.WaitForCompletion();
    }

    /// <summary>
    /// Requests this pausable task to pause
    /// </summary>
    public void RequestPause(out bool isAlreadyCompleted, out bool isAlreadyPaused) {
        lock (this.stateLock) {
            TaskState s = this.state;
            if (s >= TaskState.BeforeCompleted) {
                isAlreadyCompleted = true;
                isAlreadyPaused = false;
                return;
            }

            if (s == TaskState.BeforePaused || s == TaskState.AfterPaused) {
                isAlreadyCompleted = false;
                isAlreadyPaused = true;
                return;
            }

            Interlocked.Increment(ref this.reqToPauseCount);
            this.pauseState = PauseState.Requested;
            this.MarkPausedOrCancelled();
            _ = this.PauseOrResumeWaitInternal(true);

            isAlreadyCompleted = false;
            isAlreadyPaused = false;
        }
    }

    /// <summary>
    /// Requests this pausable task to pause
    /// </summary>
    public void RequestResume(out bool isAlreadyCompleted, out bool isAlreadyRunning) {
        lock (this.stateLock) {
            TaskState s = this.state;
            if (s >= TaskState.BeforeCompleted) {
                isAlreadyCompleted = true;
                isAlreadyRunning = false;
                return;
            }

            if ((s != TaskState.BeforePaused && s != TaskState.AfterPaused)) {
                isAlreadyCompleted = true;
                isAlreadyRunning = false;
                return;
            }
            
            isAlreadyCompleted = false;
            isAlreadyRunning = false;
            if (this.reqToPauseCount == 0) {
                return; // excessive calls
            }

            Interlocked.Decrement(ref this.reqToPauseCount); 
            Interlocked.Increment(ref this.reqToUnpauseCount);
            _ = this.PauseOrResumeWaitInternal(false);
        }
    }

    /// <summary>
    /// Resumes execution of the task, and waits for the task to enter its un-pausing state.
    /// <para>
    /// Must not be called excessively
    /// </para>
    /// </summary>
    public async Task Resume() {
        lock (this.stateLock) {
            TaskState s = this.state;
            if (s >= TaskState.BeforeCompleted) {
                goto WaitForCompletion;
            }

            if ((s != TaskState.BeforePaused && s != TaskState.AfterPaused)) {
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
            TaskState s = this.state;
            switch (s) {
                case TaskState.WaitingForActivation: return null;
                case TaskState.Running: {
                    Interlocked.Increment(ref this.reqToPauseCount);
                    this.pauseState = PauseState.Requested;
                    this.myPauseOrCancelSource!.Cancel();
                    isPausingTask = true;
                    break;
                }
                case TaskState.BeforePaused:
                case TaskState.AfterPaused: {
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
                case TaskState.WaitingForActivation: goto OperateAbs;
                case TaskState.Running:              goto PauseOperateUnpause;
                case TaskState.BeforePaused:
                case TaskState.AfterPaused:
                    goto WaitForPausedThenOperate;
                case TaskState.BeforeCompleted: goto WaitForCompletionThenOperate;
                case TaskState.AfterCompleted:  goto OperateAbs;
                case TaskState.BeforeCancelled:
                case TaskState.AfterCancelled: {
                    if (canOperateOnCancelled)
                        goto OperateAbs;
                    return Task.CompletedTask;
                }
                case TaskState.Faulted: {
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
        while (this.state != TaskState.AfterPaused) {
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
                TaskState s = this.state;
                // Marked as completed, so wait for completion
                if (s >= TaskState.BeforeCompleted) {
                    goto WaitForCompletion;
                }

                if (isPausingTask) {
                    // we're pausing so either wait for it to become fully paused, or wait for completion
                    if (s == TaskState.AfterPaused) {
                        return true;
                    }
                }
                else {
                    // Activity is continuing execution. We must decrement the unpause count
                    // otherwise the activity will be stuck forever (unless cancelled)
                    if (this.pauseState == PauseState.Continue) {
                        if (this.reqToUnpauseCount == 0)
                            Debugger.Break(); // Excessive calls to Resume()

                        Interlocked.Decrement(ref this.reqToUnpauseCount);
                        return false;
                    }
                }
            }

            await Task.Delay(10);
        }

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
        if (this.state >= TaskState.AfterCompleted) {
            return; // task became completed
        }

        await (this.myTcs?.Task ?? Task.CompletedTask).ConfigureAwait(false);
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

    private async Task InternalCompleted(TaskState finalState) {
        try {
            // Dispose callback from true cancellation ts
            this.PauseOrCancelToken = CancellationToken.None;
            this.myPauseOrCancelSource?.Dispose();
            if (this.isOwnerOfActivity) {
                this.actualCts?.Dispose();
            }
            else {
                this.actualCts = null;
            }

            await this.OnCompleted();
        }
        finally {
            this.state = finalState;
            this.myTcs!.TrySetResult();
            this.myTcs = null;
        }
    }

    private async Task PausableTaskMain() {
        CancellationToken trueCancelToken = this.actualCts?.Token ?? CancellationToken.None;
        lock (this.stateLock) {
            this.state = TaskState.Running;
            this.myTcs = new TaskCompletionSource();
            this.myPauseOrCancelSource = CancellationTokenSource.CreateLinkedTokenSource(trueCancelToken);
        }

        CancellationToken pauseOrCancelToken = this.PauseOrCancelToken = this.myPauseOrCancelSource.Token;
        for (ulong iterations = 0;; ++iterations) {
            if (this.state == TaskState.AfterPaused) {
                // This could probably be massively simplified using semaphores or something similar...
                while (true) {
                    lock (this.stateLock) {
                        if (this.reqToPauseCount < 1) {
                            // No more requests to stay paused, so mark as CONTINUE.
                            // Resume() callers notice this, and decrement reqToUnpauseCount
                            this.pauseState = PauseState.Continue;
                            break;
                        }
                    }

                    await this.WaitSomeOrCancel(trueCancelToken);
                }

                while (true) {
                    lock (this.stateLock) {
                        if (this.reqToUnpauseCount < 1) {
                            // All Resume() waiters are done, so 
                            this.pauseState = PauseState.Continue;
                            this.state = TaskState.Running;

                            // re-create pause token source, since it will definitely be cancelled at this point
                            this.myPauseOrCancelSource?.Dispose();
                            this.myPauseOrCancelSource = CancellationTokenSource.CreateLinkedTokenSource(trueCancelToken = this.actualCts?.Token ?? CancellationToken.None);
                            pauseOrCancelToken = this.PauseOrCancelToken = this.myPauseOrCancelSource.Token;
                            break;
                        }
                    }

                    await this.WaitSomeOrCancel(trueCancelToken);
                }
            }

            await this.RaiseOnPausedChanged();

            try {
                await this.RunOperation(pauseOrCancelToken, iterations == 0);
                pauseOrCancelToken.ThrowIfCancellationRequested();
            }
            catch (OperationCanceledException) {
                if (this.pauseState == PauseState.Requested && !trueCancelToken.IsCancellationRequested) {
                    lock (this.stateLock) {
                        this.pauseState = PauseState.Paused;
                        this.state = TaskState.BeforePaused;
                    }

                    await this.OnPaused(iterations == 0);
                    lock (this.stateLock) {
                        this.state = TaskState.AfterPaused;
                    }

                    await this.RaiseOnPausedChanged();
                    await this.TryCancelAfterPausing(trueCancelToken);
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
                    this.state = TaskState.Faulted;
                }

                await this.InternalCompleted(TaskState.Faulted);
                return;
            }

            lock (this.stateLock) {
                Debug.Assert(this.state == TaskState.Running);
                this.state = TaskState.BeforeCompleted;
            }

            await this.InternalCompleted(TaskState.AfterCompleted);
            return;
        }
    }

    private async Task TryCancelAfterPausing(CancellationToken trueCancelToken) {
        if (trueCancelToken.IsCancellationRequested) {
            lock (this.stateLock) {
                if (this.state == TaskState.BeforeCancelled || this.state == TaskState.AfterCancelled)
                    return; // already processed

                this.state = TaskState.BeforeCancelled;
                this.pauseState = PauseState.NotRequested;
            }

            await this.InternalCompleted(TaskState.AfterCancelled);
            trueCancelToken.ThrowIfCancellationRequested();
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