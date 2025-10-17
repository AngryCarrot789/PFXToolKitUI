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

using System.Collections.Concurrent;
using System.Diagnostics;

namespace PFXToolKitUI;

public sealed class MessagePump {
    private delegate void InternalPostCallback(object? state1, object? state2, CancellationToken cancellationToken);
    
    private readonly struct PumpOperation(InternalPostCallback callback, object? state1, object? state2, CancellationToken cancellation) {
        public readonly InternalPostCallback callback = callback;
        public readonly object? state1 = state1;
        public readonly object? state2 = state2;
        public readonly CancellationToken cancellation = cancellation;
    }

    private AutoResetEvent? msgEvent;
    private readonly ConcurrentQueue<PumpOperation> callbacks;
    private CancellationToken requestExitToken;
    private bool isRunning;

    /// <summary>
    /// Gets whether exiting the pump loop is requested
    /// </summary>
    public bool IsExitRequested => this.requestExitToken.IsCancellationRequested;

    public bool IsRunning => this.isRunning;
    
    public MessagePump() {
        this.callbacks = new ConcurrentQueue<PumpOperation>();
        this.msgEvent = new AutoResetEvent(false);
    }

    public static MessagePump RunInThread(Action<Thread>? setupAction = null, CancellationToken requestExitToken = default) {
        MessagePump pump = new MessagePump();
        Thread thread = new Thread(() => pump.Run(requestExitToken));
        setupAction?.Invoke(thread);
        thread.Start();
        return pump;
    }

    /// <summary>
    /// Runs this message pump in the current thread.
    /// </summary>
    /// <param name="requestExitToken">
    /// A token that, when cancelled, will notify us to exit the message pump once all callbacks are fired.
    /// </param>
    public void Run(CancellationToken requestExitToken) {
        AutoResetEvent? mre = this.msgEvent;
        ObjectDisposedException.ThrowIf(mre == null, this);
        this.requestExitToken = requestExitToken;
        
        using CancellationTokenRegistration registration = requestExitToken.Register(static t => ((MessagePump) t!).RequestProcessing(), this);
        while (!requestExitToken.IsCancellationRequested) {
            this.ExecuteAllCallbacks();
            mre.WaitOne();
        }
        
        this.ExecuteAllCallbacks();
        Volatile.Write(ref this.isRunning, true);
        
        AutoResetEvent? expectedMre = Interlocked.Exchange(ref this.msgEvent, null);
        Debug.Assert(expectedMre == mre);
        expectedMre?.Dispose();
        mre?.Dispose();
    }

    private void RequestProcessing() {
        this.msgEvent?.Set();
    }

    private void ExecuteAllCallbacks() {
        while (this.callbacks.TryDequeue(out PumpOperation operation)) {
            operation.callback(operation.state1, operation.state2, operation.cancellation);
        }
    }

    public Task InvokeAsync(Action<CancellationToken> action, CancellationToken cancellationToken = default) {
        if (cancellationToken.IsCancellationRequested)
            return Task.FromCanceled(cancellationToken);

        TaskCompletionSource tcs = new TaskCompletionSource();
        this.PostInternal(HandleInvokeAsync, action, tcs, cancellationToken);
        return tcs.Task;
    }
    
    public void Post(SendOrPostCallback callback, object? state, CancellationToken cancellationToken = default) {
        this.PostInternal(static (a, b, _) => ((SendOrPostCallback) a!)(b), callback, state, cancellationToken);
    }
    
    private void PostInternal(InternalPostCallback callback, object? state1, object? state2, CancellationToken cancellationToken = default) {
        this.callbacks.Enqueue(new PumpOperation(callback, state1, state2, cancellationToken));
        this.RequestProcessing();
    }

    private static void HandleInvokeAsync(object? s1, object? s2, CancellationToken c) {
        TaskCompletionSource tcs = (TaskCompletionSource) s2!;
        if (c.IsCancellationRequested) {
            tcs.SetCanceled(c);
            return;
        }

        try {
            ((Action<CancellationToken>) s1!)(c);
        }
        catch (OperationCanceledException e) {
            tcs.SetCanceled(e.CancellationToken);
        }
        catch (Exception e) {
            tcs.SetException(e);
        }
    }
}