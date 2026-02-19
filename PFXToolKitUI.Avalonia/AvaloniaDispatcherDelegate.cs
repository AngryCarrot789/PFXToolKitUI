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
public sealed class AvaloniaDispatcherDelegate : BaseDispatcher {
    private static readonly ConstructorInfo TimerCTOR = typeof(DispatcherTimer).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, [typeof(DispatcherPriority), typeof(Dispatcher)], null) ?? throw new Exception("Missing constructor for Avalonia DispatcherTimer");

    private readonly Dispatcher dispatcher;
    private readonly FrameManagerImpl? myFrameManager;

    public AvaloniaDispatcherDelegate(Dispatcher dispatcher) {
        this.dispatcher = dispatcher;
        this.myFrameManager = this.dispatcher.SupportsRunLoops ? new FrameManagerImpl(this) : null;
    }

    public override bool CheckAccess() => this.dispatcher.CheckAccess();

    public override void VerifyAccess() => this.dispatcher.VerifyAccess();

    protected override void ShutdownCore() => this.dispatcher.InvokeShutdown();

    public override IDispatcherTimer CreateTimer(DispatchPriority priority) {
        DispatcherTimer timer = (DispatcherTimer) TimerCTOR.Invoke([ToAvaloniaPriority(priority), this.dispatcher]);
        return new TimerDelegate(timer, this);
    }

    public override bool TryGetFrameManager([NotNullWhen(true)] out IDispatcherFrameManager? frameManager) {
        return (frameManager = this.myFrameManager) != null;
    }

    protected override void PostCore(SendOrPostCallback callback, object? state, DispatchPriority priority) {
        this.dispatcher.Post(callback, state, ToAvaloniaPriority(priority));
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
}