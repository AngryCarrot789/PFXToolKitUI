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
using System.Reflection;
using System.Runtime.CompilerServices;
using Avalonia.Threading;

namespace PFXToolKitUI.Avalonia;

/// <summary>
/// A delegate around the avalonia dispatcher so that core projects can access it, since features RateLimitedDispatchAction require it
/// </summary>
public class AvaloniaDispatcherDelegate : IDispatcher {
    private static readonly ConstructorInfo TimerCTOR = typeof(DispatcherTimer).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, [typeof(DispatcherPriority), typeof(Dispatcher)], null) ?? throw new Exception("Missing constructor for Avalonia DispatcherTimer");

    private readonly Dispatcher dispatcher;
    private readonly FrameManagerImpl? myFrameManager;

    public AvaloniaDispatcherDelegate(Dispatcher dispatcher) {
        this.dispatcher = dispatcher;
        this.myFrameManager = this.dispatcher.SupportsRunLoops ? new FrameManagerImpl(this) : null;
    }

    public bool CheckAccess() => this.dispatcher.CheckAccess();

    public void VerifyAccess() => this.dispatcher.VerifyAccess();

    public Task InvokeAsync(Action action, DispatchPriority priority = DispatchPriority.Normal, bool captureContext = false, CancellationToken token = default) {
        TaskCompletionSource tcs = new TaskCompletionSource();
        this.dispatcher.Post(InvokeHandlers.s_HandleInvokeAsyncInfo, new InvokeAsyncInfo(action, tcs, token, captureContext), ToAvaloniaPriority(priority));
        return tcs.Task;
    }

    public Task<T> InvokeAsync<T>(Func<T> function, DispatchPriority priority = DispatchPriority.Normal, bool captureContext = false, CancellationToken token = default) {
        TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();
        this.dispatcher.Post(InvokeHandlers<T>.s_HandleInvokeAsync, new InvokeAsyncInfo<T>(function, tcs, token, captureContext), ToAvaloniaPriority(priority));
        return tcs.Task;
    }

    public void Post(Action action, DispatchPriority priority = DispatchPriority.Default, bool captureContext = false) {
        this.dispatcher.Post(InvokeHandlers.s_HandlePostInfo, new PostInfo(InvokeHandlers.s_InvokeAction, action, captureContext), ToAvaloniaPriority(priority));
    }

    public void Post(SendOrPostCallback action, object? state, DispatchPriority priority = DispatchPriority.Default, bool captureContext = false) {
        this.dispatcher.Post(InvokeHandlers.s_HandlePostInfo, new PostInfo(action, state, captureContext), ToAvaloniaPriority(priority));
    }

    public void Shutdown() => this.dispatcher.InvokeShutdown();

    public IDispatcherTimer CreateTimer(DispatchPriority priority) {
        DispatcherTimer timer = (DispatcherTimer) TimerCTOR.Invoke([ToAvaloniaPriority(priority), this.dispatcher]);
        return new TimerDelegate(timer, this);
    }

    public bool TryGetFrameManager([NotNullWhen(true)] out IDispatcherFrameManager? frameManager) {
        return (frameManager = this.myFrameManager) != null;
    }

    private static DispatcherPriority ToAvaloniaPriority(DispatchPriority priority) {
        return Unsafe.As<DispatchPriority, DispatcherPriority>(ref priority);
    }

    private class TimerDelegate : IDispatcherTimer {
        private readonly DispatcherTimer timer;
        private readonly AvaloniaDispatcherDelegate dispatcher;

        public IDispatcher Dispatcher => this.dispatcher;

        public bool IsEnabled {
            get => this.timer.IsEnabled;
            set => this.timer.IsEnabled = value;
        }

        public TimeSpan Interval {
            get => this.timer.Interval;
            set => this.timer.Interval = value;
        }

        public event EventHandler? Tick {
            add => this.timer.Tick += value;
            remove => this.timer.Tick -= value;
        }

        public TimerDelegate(DispatcherTimer timer, AvaloniaDispatcherDelegate dispatcher) {
            this.timer = timer;
            this.dispatcher = dispatcher;
        }

        public void Start() => this.timer.Start();

        public void Stop() => this.timer.Stop();
    }

    private sealed class FrameManagerImpl(AvaloniaDispatcherDelegate dispatcher) : IDispatcherFrameManager {
        private static readonly PropertyInfo DisabledProcessingCountInfo = typeof(Dispatcher).GetProperty("DisabledProcessingCount", BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new Exception("Missing field \"DisabledProcessingCount\"");

        public bool IsFramePushingSuspended => (int) DisabledProcessingCountInfo.GetValue(dispatcher.dispatcher)! > 0;

        public void PushFrame(CancellationToken cancellationToken) {
            if (!cancellationToken.CanBeCanceled)
                throw new ArgumentException("The token is not cancellable, meaning this method would never return");
            if (cancellationToken.IsCancellationRequested)
                return;

            DispatcherFrame frame = new DispatcherFrame();
            using (cancellationToken.Register(static f => ((DispatcherFrame) f!).Continue = false, frame)) {
                dispatcher.dispatcher.PushFrame(frame);
            }
        }

        static FrameManagerImpl() {
            // defined to force DisabledProcessingCountInfo to be evaluated
        }
    }

    private sealed class PostInfo(SendOrPostCallback callback, object? state, bool captureContext) {
        public readonly ExecutionContext? CapturedContext = captureContext ? ExecutionContext.Capture() : null;
        public readonly SendOrPostCallback Callback = callback;
        public readonly object? State = state;
    }

    private sealed class InvokeAsyncInfo(Action action, TaskCompletionSource taskCompletionSource, CancellationToken token, bool captureContext) {
        public readonly ExecutionContext? CapturedContext = captureContext ? ExecutionContext.Capture() : null;
        public readonly Action Action = action;
        public readonly TaskCompletionSource TaskCompletionSource = taskCompletionSource;
        public readonly CancellationToken Token = token;
    }

    private sealed class InvokeAsyncInfo<T>(Func<T> callback, TaskCompletionSource<T> taskCompletionSource, CancellationToken token, bool captureContext) {
        public readonly ExecutionContext? CapturedContext = captureContext ? ExecutionContext.Capture() : null;
        public readonly Func<T> Callback = callback;
        public readonly TaskCompletionSource<T> TaskCompletionSource = taskCompletionSource;
        public readonly CancellationToken Token = token;
    }

    private static class InvokeHandlers<T> {
        [SuppressMessage("ReSharper", "StaticMemberInGenericType")]
        public static readonly SendOrPostCallback s_HandleInvokeAsync = HandleInvokeAsync;

        [SuppressMessage("ReSharper", "StaticMemberInGenericType")]
        private static readonly ContextCallback s_RunCallback = RunCallback;

        private static void RunCallback(object? inf) {
            InvokeAsyncInfo<T> info = (InvokeAsyncInfo<T>) inf!;
            try {
                info.TaskCompletionSource.SetResult(info.Callback());
            }
            catch (Exception e) {
                info.TaskCompletionSource.SetException(e);
            }
        }

        private static void HandleInvokeAsync(object? x) {
            InvokeAsyncInfo<T> info = (InvokeAsyncInfo<T>) x!;
            TaskCompletionSource<T> tcs = info.TaskCompletionSource;

            if (info.Token.IsCancellationRequested) {
                tcs.TrySetCanceled(info.Token);
                return;
            }

            if (info.CapturedContext != null) {
                ExecutionContext.Run(info.CapturedContext, s_RunCallback, info);
            }
            else {
                try {
                    tcs.SetResult(info.Callback());
                }
                catch (Exception e) {
                    tcs.SetException(e);
                }
            }
        }
    }

    private static class InvokeHandlers {
        public static readonly SendOrPostCallback s_HandleInvokeAsyncInfo = HandleInvokeAsync;
        private static readonly ContextCallback s_RunCallback = RunCallback;
        public static readonly SendOrPostCallback s_InvokeAction = InvokeAction;
        private static readonly ContextCallback s_RunPost = RunPost;
        public static readonly SendOrPostCallback s_HandlePostInfo = HandlePost;

        private static void RunCallback(object? inf) {
            InvokeAsyncInfo info = (InvokeAsyncInfo) inf!;
            try {
                info.Action();
                info.TaskCompletionSource.SetResult();
            }
            catch (Exception e) {
                info.TaskCompletionSource.SetException(e);
            }
        }

        private static void HandleInvokeAsync(object? x) {
            InvokeAsyncInfo info = (InvokeAsyncInfo) x!;
            TaskCompletionSource tcs = info.TaskCompletionSource;

            if (info.Token.IsCancellationRequested) {
                tcs.TrySetCanceled(info.Token);
                return;
            }

            if (info.CapturedContext != null) {
                ExecutionContext.Run(info.CapturedContext, s_RunCallback, info);
            }
            else {
                try {
                    info.Action();
                    tcs.SetResult();
                }
                catch (Exception e) {
                    tcs.SetException(e);
                }
            }
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
}