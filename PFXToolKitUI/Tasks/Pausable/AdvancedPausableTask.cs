// 
// Copyright (c) 2024-2025 REghZy
// 
// This file is part of FramePFX.
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
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
// 

using System.Diagnostics;

namespace PFXToolKitUI.Tasks.Pausable;

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
        COMPLETED, // activity has completed successfully
        CANCELLED, // activity was cancelled
        EXCEPTION // activity completed due to an unhandled exception
    }

    private enum PAUSE_STATE : byte {
        NOT_REQUESTED,     // No pause request yet
        REQUESTED,         // Pause() called
        PAUSED,            // Activity is paused
        UNPAUSE_REQUESTED, // Resume() called
        CONTINUE,          // Activity is no longer paused
    }

    private volatile TASK_STATE state;
    private volatile PAUSE_STATE pauseState;
    private volatile int reqToPauseCount, reqToUnpauseCount;
    private volatile int continueAttempts;

    private volatile Task firstTask, continueTask;
    private readonly object stateLock = new object();
    private volatile ActivityTask? activity;
    private volatile Exception? exception;
    private readonly CancellationTokenSource? actualCts;
    
    private CancellationTokenSource? myPauseOrCancelSource;
    private CancellationTokenRegistration? pauseCancellationRegistration;

    public ActivityTask Activity => this.activity ?? throw new InvalidOperationException("Not started yet");
    
    /// <summary>
    /// Returns true when completed for any reason (success, cancellation or exception)
    /// </summary>
    public bool IsCompleted => this.state >= TASK_STATE.COMPLETED;

    /// <summary>
    /// Returns true when completed due to task cancellation
    /// </summary>
    public bool IsCompletedByCancellation => this.state == TASK_STATE.CANCELLED;
    
    /// <summary>
    /// Gets the exception encountered during task execution. When non-null, <see cref="IsCompleted"/> returns true
    /// </summary>
    public Exception? Exception => this.exception;
    
    protected AdvancedPausableTask(bool isCancellable) {
        this.actualCts = isCancellable ? new CancellationTokenSource() : null;
    }

    /// <summary>
    /// Runs the operation for the first time.
    /// </summary>
    /// <param name="pauseOrCancelToken">A token that becomes 'cancelled' when this task is either paused or actually cancelled</param>
    /// <returns>The first run of the task, and hopefully the only run. When this returns normally, the activity is completed</returns>
    /// <exception cref="OperationCanceledException">The operation was paused or cancelled</exception>
    protected internal abstract Task RunFirst(CancellationToken pauseOrCancelToken);

    /// <summary>
    /// Continues running this task. This can for example re-obtain locks
    /// </summary>
    /// <param name="pauseOrCancelToken">A token that becomes 'cancelled' when this task is either paused or actually cancelled</param>
    /// <returns>The second or more run of the task. When this returns normally, the activity is completed</returns>
    /// <exception cref="OperationCanceledException">The operation was paused or cancelled</exception>
    protected internal abstract Task Continue(CancellationToken pauseOrCancelToken);

    /// <summary>
    /// Runs the task as a complete task operation
    /// </summary>
    /// <returns>The entire operation</returns>
    public ActivityTask Run() {
        return ActivityManager.Instance.RunTask(async () => {
            lock (this.stateLock) {
                this.activity = ActivityManager.Instance.CurrentTask;
                this.state = TASK_STATE.RUNNING;
                this.myPauseOrCancelSource = new CancellationTokenSource();
                
                // Register callback with actual cancellation ts to also 'cancel' the pause token, pausing the task again
                this.pauseCancellationRegistration = this.actualCts?.Token.Register(this.myPauseOrCancelSource.Cancel);
            }

            CancellationToken pauseToken = this.myPauseOrCancelSource.Token;
            try {
                this.continueAttempts = -1;
                await (this.firstTask = this.RunFirst(pauseToken));
            }
            catch (OperationCanceledException e) {
                // Realistically if e is a PausedException, pauseState should always be REQUESTED
                if (this.pauseState == PAUSE_STATE.REQUESTED || e is PausedException) {
                    // unless RunFirst uses a custom CTS setup, e.CancellationToken should equal pauseToken;
                    await this.OnPausedInternalAsync(false);
                }
                else {
                    await this.OnCancelledInternal();
                    return;
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
                    this.state = TASK_STATE.COMPLETED;
                    ranToSuccess = true;
                }
            }
            
            if (ranToSuccess) {
                this.activity!.Progress.Text = "Completed.";
                await this.OnCompletedInternal();
                return;
            }
            
            while (true) {
                if (this.state == TASK_STATE.PAUSED_2) {
                    // This could probably be massively simplified using semaphores or something similar...
                    this.activity!.Progress.Text = "Paused";
                    while (true) {
                        lock (this.stateLock) {
                            if (this.reqToPauseCount < 1) {
                                // No more requests to stay paused, so mark as CONTINUE.
                                // Resume() callers notice this, and decrement reqToUnpauseCount
                                this.pauseState = PAUSE_STATE.CONTINUE;
                                break;
                            }
                        }

                        await Task.Delay(10);
                    }

                    while (true) {
                        lock (this.stateLock) {
                            if (this.reqToUnpauseCount < 1) {
                                // All Resume() waiters are done, so 
                                this.pauseState = PAUSE_STATE.CONTINUE;
                                this.state = TASK_STATE.RUNNING;

                                // Dispose callback from true cancellation ts
                                this.pauseCancellationRegistration?.Dispose();
                                
                                // re-create pause token source, since it will definitely be cancelled at this point
                                this.myPauseOrCancelSource?.Dispose();
                                this.myPauseOrCancelSource = new CancellationTokenSource();
                                
                                // Register callback with actual cancellation ts to also 'cancel' the pause token, pausing the task again
                                this.pauseCancellationRegistration = this.actualCts?.Token.Register(this.myPauseOrCancelSource.Cancel);
                                break;
                            }
                        }

                        await Task.Delay(10);
                    }
                }

                this.activity!.Progress.Text = "Continuing";

                pauseToken = this.myPauseOrCancelSource.Token;
                try {
                    Interlocked.Increment(ref this.continueAttempts);
                    await (this.continueTask = this.Continue(pauseToken));
                }
                catch (OperationCanceledException e) {
                    // Realistically if e is a PausedException, pauseState should always be REQUESTED
                    if (this.pauseState == PAUSE_STATE.REQUESTED || e is PausedException) {
                        // unless Continue uses a custom CTS setup, e.CancellationToken should equal pauseToken;
                        await this.OnPausedInternalAsync(false);
                        continue;
                    }
                    else {
                        await this.OnCancelledInternal();
                        return;
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
                    this.state = TASK_STATE.COMPLETED;
                }

                this.activity!.Progress.Text = "Completed.";
                await this.OnCompletedInternal();
                return;
            }
        }, this.actualCts);
    }

    private async Task OnCancelledInternal() {
        lock (this.stateLock) {
            this.state = TASK_STATE.CANCELLED;
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
    }
    
    /// <summary>
    /// Pauses this task and waits for it to actually become paused (<see cref="OnPaused"/> will have completed)
    /// </summary>
    public async Task Pause() {
        lock (this.stateLock) {
            TASK_STATE s = this.state;
            if (s != TASK_STATE.WAITING && s != TASK_STATE.RUNNING) {
                return;
            }

            Interlocked.Increment(ref this.reqToPauseCount);
            this.pauseState = PAUSE_STATE.REQUESTED;
            this.myPauseOrCancelSource!.Cancel();
        }

        while (true) {
            lock (this.stateLock) {
                TASK_STATE s = this.state;
                if (s == TASK_STATE.PAUSED_2 || s >= TASK_STATE.COMPLETED) {
                    return;
                }
            }
            
            await Task.Delay(10);
        }
    }

    /// <summary>
    /// Resumes execution of the task, and waits for the task to enter its unpausing state
    /// </summary>
    public async Task Resume() {
        lock (this.stateLock) {
            TASK_STATE s = this.state;
            if (s >= TASK_STATE.COMPLETED || (s != TASK_STATE.PAUSED_1 && s != TASK_STATE.PAUSED_2)) {
                return; // task became completed
            }

            if (this.reqToPauseCount == 0)
                Debugger.Break(); // Excessive calls to Resume()

            Interlocked.Decrement(ref this.reqToPauseCount);
            Interlocked.Increment(ref this.reqToUnpauseCount);
        }

        while (true) {
            lock (this.stateLock) {
                if (this.state >= TASK_STATE.COMPLETED) {
                    return; // task became completed
                }
                
                if (this.pauseState == PAUSE_STATE.CONTINUE) {
                    if (this.reqToUnpauseCount == 0)
                        Debugger.Break(); // Excessive calls to Resume()
                    
                    Interlocked.Decrement(ref this.reqToUnpauseCount);
                    break;
                }
            }
            
            await Task.Delay(10);
        }
    }
    
    public bool Cancel() {
        if (this.actualCts == null)
            return false;

        try {
            this.actualCts.Cancel();
        }
        catch (ObjectDisposedException e) {
            // ignored
        }

        return true;
    }
    
    public async Task<bool> CancelAsync() {
        if (this.actualCts == null)
            return false;

        try {
            await this.actualCts.CancelAsync();
        }
        catch (ObjectDisposedException e) {
            return true;
        }

        while (true) {
            lock (this.stateLock) {
                if (this.state >= TASK_STATE.COMPLETED) {
                    return true; // task became completed
                }
            }

            await Task.Delay(10);
        }
    }

    protected void CheckPaused() {
        lock (this.stateLock) {
            if (this.state == TASK_STATE.RUNNING) {
                if (this.pauseState == PAUSE_STATE.REQUESTED) {
                    throw new PausedException();
                }
            }
        }
    }

    protected void CheckCancelled() {
        if (this.activity != null && this.activity.IsCancellationRequested) {
            throw new OperationCanceledException("Activity cancelled");
        }
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
        if (this.pauseCancellationRegistration.HasValue)
            await this.pauseCancellationRegistration.Value.DisposeAsync();
        
        this.myPauseOrCancelSource?.Dispose();
        this.actualCts?.Dispose();
        
        await this.OnCompleted();
    }

    private class PausedException() : OperationCanceledException("Activity paused");
}