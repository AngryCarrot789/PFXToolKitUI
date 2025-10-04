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
    private readonly Dispatcher dispatcher;
    private readonly DispatcherFrameManagerImpl? myFrameManager;

    public AvaloniaDispatcherDelegate(Dispatcher dispatcher) {
        this.dispatcher = dispatcher;
        this.myFrameManager = this.dispatcher.SupportsRunLoops ? new DispatcherFrameManagerImpl(this) : null;
    }

    public bool CheckAccess() => this.dispatcher.CheckAccess();

    public void VerifyAccess() => this.dispatcher.VerifyAccess();

    public void Invoke(Action action, DispatchPriority priority) {
        if (priority == DispatchPriority.Send && this.dispatcher.CheckAccess()) {
            action();
        }
        else {
            this.dispatcher.Invoke(action, ToAvaloniaPriority(priority));
        }
    }

    public T Invoke<T>(Func<T> function, DispatchPriority priority) {
        if (priority == DispatchPriority.Send && this.dispatcher.CheckAccess())
            return function();
        return this.dispatcher.Invoke(function, ToAvaloniaPriority(priority));
    }

    public async Task InvokeAsync(Action action, DispatchPriority priority, CancellationToken token = default) {
        await this.dispatcher.InvokeAsync(action, ToAvaloniaPriority(priority), token);
    }

    public async Task<T> InvokeAsync<T>(Func<T> function, DispatchPriority priority, CancellationToken token = default) {
        return await this.dispatcher.InvokeAsync(function, ToAvaloniaPriority(priority), token);
    }

    public void Post(Action action, DispatchPriority priority = DispatchPriority.Default) {
        this.dispatcher.Post(action, ToAvaloniaPriority(priority));
    }
    
    public void Post(SendOrPostCallback action, object? state, DispatchPriority priority = DispatchPriority.Default) {
        this.dispatcher.Post(action, state, ToAvaloniaPriority(priority));
    }
    
    public void Shutdown() => this.dispatcher.InvokeShutdown();

    public IDispatcherTimer CreateTimer(DispatchPriority priority) {
        ConstructorInfo ctor = typeof(DispatcherTimer).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, [typeof(DispatcherPriority), typeof(Dispatcher)], null)
                               ?? throw new Exception("Missing constructor for Avalonia DispatcherTimer");
        DispatcherTimer timer = (DispatcherTimer) ctor.Invoke([ToAvaloniaPriority(priority), this.dispatcher]);
        return new DispatcherTimerDelegate(timer, this);
    }

    public bool TryGetFrameManager([NotNullWhen(true)] out IDispatcherFrameManager? frameManager) {
        return (frameManager = this.myFrameManager) != null;
    }

    private static DispatcherPriority ToAvaloniaPriority(DispatchPriority priority) {
        return Unsafe.As<DispatchPriority, DispatcherPriority>(ref priority);
    }

    private class DispatcherTimerDelegate : IDispatcherTimer {
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

        public DispatcherTimerDelegate(DispatcherTimer timer, AvaloniaDispatcherDelegate dispatcher) {
            this.timer = timer;
            this.dispatcher = dispatcher;
        }

        public void Start() => this.timer.Start();

        public void Stop() => this.timer.Stop();
    }

    private sealed class DispatcherFrameManagerImpl(AvaloniaDispatcherDelegate dispatcher) : IDispatcherFrameManager {
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
        
        static DispatcherFrameManagerImpl() {
            // defined to force DisabledProcessingCountInfo to be evaluated
        }
    }
}