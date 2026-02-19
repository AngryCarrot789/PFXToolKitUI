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

using System.Diagnostics.CodeAnalysis;

namespace PFXToolKitUI;

public abstract class BaseDispatcher : IDispatcher {
    private readonly List<IDispatcherTimer> myTimers;

    protected BaseDispatcher() {
        this.myTimers = new List<IDispatcherTimer>();
    }

    public abstract bool CheckAccess();

    public virtual void VerifyAccess() => ((IDispatcher) this).VerifyAccess();

    public virtual Task InvokeAsync(Action action, DispatchPriority priority = DispatchPriority.Normal, bool captureContext = false, CancellationToken token = default) {
        InvokeAsyncInfo info = new InvokeAsyncInfo(action, captureContext, token);
        this.PostCore(InvokeAsyncInfo.s_HandleInvokeAsyncInfo, info, priority);
        return info.Task;
    }

    public virtual Task<T> InvokeAsync<T>(Func<T> function, DispatchPriority priority = DispatchPriority.Normal, bool captureContext = false, CancellationToken token = default) {
        InvokeAsyncInfo<T> info = new InvokeAsyncInfo<T>(function, captureContext, token);
        this.PostCore(InvokeAsyncInfo<T>.s_HandleInvokeAsync, info, priority);
        return info.Task;
    }

    public virtual void Post(Action action, DispatchPriority priority = DispatchPriority.Default, bool captureContext = false) {
        this.PostCore(PostInfo.s_HandlePostInfo, new PostInfo(PostInfo.s_InvokeAction, action, captureContext), priority);
    }

    public virtual void Post(SendOrPostCallback callback, object? state, DispatchPriority priority = DispatchPriority.Default, bool captureContext = false) {
        this.PostCore(PostInfo.s_HandlePostInfo, new PostInfo(callback, state, captureContext), priority);
    }

    public void Shutdown() {
        lock (this.myTimers) {
            for (int i = this.myTimers.Count - 1; i >= 0; i--) {
                this.myTimers[i].Stop();
            }
        }
        
        this.ShutdownCore();
    }

    protected abstract void ShutdownCore();

    public virtual IDispatcherTimer CreateTimer(DispatchPriority priority) {
        return new TimerImpl(priority, this);
    }

    public abstract bool TryGetFrameManager([NotNullWhen(true)] out IDispatcherFrameManager? frameManager);

    protected abstract void PostCore(SendOrPostCallback callback, object? state, DispatchPriority priority);

    private sealed class PostInfo {
        public static readonly SendOrPostCallback s_InvokeAction = InvokeAction;
        private static readonly ContextCallback s_RunPost = RunPost;
        public static readonly SendOrPostCallback s_HandlePostInfo = HandlePost;

        private readonly ExecutionContext? CapturedContext;
        private readonly SendOrPostCallback Callback;
        private readonly object? State;

        public PostInfo(SendOrPostCallback callback, object? state, bool captureContext) {
            this.CapturedContext = captureContext ? ExecutionContext.Capture() : null;
            this.Callback = callback;
            this.State = state;
        }

        private static void InvokeAction(object? s) => ((Action) s!)();

        private static void RunPost(object? state) {
            PostInfo info = (PostInfo) state!;
            info.Callback(info.State);
        }

        private static void HandlePost(object? state) {
            PostInfo info = (PostInfo) state!;
            if (info.CapturedContext != null) {
                ExecutionContext.Run(info.CapturedContext, s_RunPost, info);
            }
            else {
                info.Callback(info.State);
            }
        }
    }

    private sealed class InvokeAsyncInfo : TaskCompletionSource {
        public static readonly SendOrPostCallback s_HandleInvokeAsyncInfo = HandleInvokeAsync;
        private static readonly ContextCallback s_RunCallback = RunCallback;

        private readonly ExecutionContext? CapturedContext;
        private readonly Action Action;
        private readonly CancellationToken Token;

        public InvokeAsyncInfo(Action action, bool captureContext, CancellationToken token) {
            this.CapturedContext = captureContext ? ExecutionContext.Capture() : null;
            this.Action = action;
            this.Token = token;
        }

        private static void HandleInvokeAsync(object? x) {
            InvokeAsyncInfo info = (InvokeAsyncInfo) x!;

            if (info.Token.IsCancellationRequested) {
                info.TrySetCanceled(info.Token);
                return;
            }

            if (info.CapturedContext != null) {
                ExecutionContext.Run(info.CapturedContext, s_RunCallback, info);
            }
            else {
                try {
                    info.Action();
                    info.SetResult();
                }
                catch (Exception e) {
                    info.SetException(e);
                }
            }
        }

        private static void RunCallback(object? inf) {
            InvokeAsyncInfo info = (InvokeAsyncInfo) inf!;
            try {
                info.Action();
                info.SetResult();
            }
            catch (Exception e) {
                info.SetException(e);
            }
        }
    }

    private sealed class InvokeAsyncInfo<T> : TaskCompletionSource<T> {
        [SuppressMessage("ReSharper", "StaticMemberInGenericType")]
        public static readonly SendOrPostCallback s_HandleInvokeAsync = HandleInvokeAsync;

        [SuppressMessage("ReSharper", "StaticMemberInGenericType")]
        private static readonly ContextCallback s_RunCallback = RunCallback;

        private readonly ExecutionContext? CapturedContext;
        private readonly Func<T> Callback;
        private readonly CancellationToken Token;

        public InvokeAsyncInfo(Func<T> callback, bool captureContext, CancellationToken token) {
            this.CapturedContext = captureContext ? ExecutionContext.Capture() : null;
            this.Callback = callback;
            this.Token = token;
        }

        private static void RunCallback(object? inf) {
            InvokeAsyncInfo<T> info = (InvokeAsyncInfo<T>) inf!;
            try {
                info.SetResult(info.Callback());
            }
            catch (Exception e) {
                info.SetException(e);
            }
        }

        private static void HandleInvokeAsync(object? x) {
            InvokeAsyncInfo<T> info = (InvokeAsyncInfo<T>) x!;
            if (info.Token.IsCancellationRequested) {
                info.TrySetCanceled(info.Token);
                return;
            }

            if (info.CapturedContext != null) {
                ExecutionContext.Run(info.CapturedContext, s_RunCallback, info);
            }
            else {
                try {
                    info.SetResult(info.Callback());
                }
                catch (Exception e) {
                    info.SetException(e);
                }
            }
        }
    }

    private sealed class TimerImpl : IDispatcherTimer {
        private readonly DispatchPriority priority;
        private readonly Lock myLock = new Lock();
        private Timer? timer;
        private bool isEnabled;

        public IDispatcher Dispatcher { get; }

        public bool IsEnabled {
            get => this.isEnabled;
            set {
                if (value) {
                    this.Start();
                }
                else {
                    this.Stop();
                }
            }
        }

        public TimeSpan Interval {
            get => field;
            set {
                if (value.Ticks < TimeSpan.TicksPerMillisecond)
                    throw new ArgumentException("Delay must be 1 or more milliseconds", nameof(value));
                if (value.TotalMilliseconds >= int.MaxValue)
                    throw new ArgumentException("Delay is too large", nameof(value));

                lock (this.myLock) {
                    if (field != value) {
                        field = value;
                        if (this.isEnabled) {
                            this.CreateTimer();
                        }
                    }
                }
            }
        }

        public event EventHandler? Tick;

        public TimerImpl(DispatchPriority priority, BaseDispatcher dispatcher) {
            this.priority = priority;
            this.Dispatcher = dispatcher;
        }

        public void Start() {
            lock (this.myLock) {
                if (!this.IsEnabled) {
                    List<IDispatcherTimer> list = ((BaseDispatcher) this.Dispatcher).myTimers;
                    lock (list) {
                        list.Add(this);
                    }

                    this.CreateTimer();
                    this.IsEnabled = true;
                }
            }
        }

        public void Stop() {
            lock (this.myLock) {
                this.timer?.Dispose();
                this.timer = null;
                this.isEnabled = false;
                
                List<IDispatcherTimer> list = ((BaseDispatcher) this.Dispatcher).myTimers;
                lock (list) {
                    list.Remove(this);
                }
            }
        }

        private void CreateTimer() {
            this.timer?.Dispose();
            this.timer = new Timer(static s => ((TimerImpl) s!).OnTimerTick(), this, this.Interval, this.Interval);
        }
        
        private void OnTimerTick() {
            this.Dispatcher.Post(static s => ((TimerImpl) s!).OnTimerTickOnDispatcher(), this, this.priority);
        }

        private void OnTimerTickOnDispatcher() {
            this.Tick?.Invoke(this, EventArgs.Empty);
        }
    }
}