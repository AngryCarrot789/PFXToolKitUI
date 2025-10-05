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

namespace PFXToolKitUI.Utils.Debouncing;

/// <summary>
/// A class that posts a callback to the dispatcher once a specific amount of time since the last post attempt has ellapsed
/// </summary>
public class DemandDispatcherDebouncer : IDebouncer {
    private static readonly SendOrPostCallback s_CallbackWithAction = static a => ((Action) a!)();
    private readonly SendOrPostCallback callback;
    private readonly object? state;
    private long lastTrigger;
    
    /// <summary>
    /// Gets the interval of this debouncer
    /// </summary>
    public TimeSpan Interval { get; }

    /// <summary>
    /// Gets the dispatcher we post the callback to
    /// </summary>
    public IDispatcher Dispatcher { get; }
    
    /// <summary>
    /// Gets the dispatch priority for the callback
    /// </summary>
    public DispatchPriority Priority { get; }
    
    public bool CanRun => Time.GetSystemTicks() - this.lastTrigger >= this.Interval.Ticks;
    
    public bool HasRunOnce => this.lastTrigger != 0;

    public DemandDispatcherDebouncer(TimeSpan interval, Action callback, DispatchPriority priority = DispatchPriority.Default) : this(interval, callback, ApplicationPFX.Instance.Dispatcher, priority) {
    }

    public DemandDispatcherDebouncer(TimeSpan interval, Action callback, IDispatcher dispatcher, DispatchPriority priority = DispatchPriority.Default) : this(interval, s_CallbackWithAction, callback, dispatcher, priority) {
    }

    public DemandDispatcherDebouncer(TimeSpan interval, SendOrPostCallback callback, object? state, DispatchPriority priority = DispatchPriority.Default) : this(interval, callback, state, ApplicationPFX.Instance.Dispatcher, priority) {
    }

    public DemandDispatcherDebouncer(TimeSpan interval, SendOrPostCallback callback, object? state, IDispatcher dispatcher, DispatchPriority priority = DispatchPriority.Default) {
        this.Dispatcher = dispatcher;
        this.Interval = interval;
        this.Priority = priority;
        this.callback = callback;
        this.state = state;
    }
    
    public void Run() {
        this.lastTrigger = Time.GetSystemTicks();
        this.Dispatcher.Post(this.callback, this.state, this.Priority);
    }

    public bool GetThenRun() => ((IDebouncer) this).GetThenRun();

    public bool GetThenRunIf() => ((IDebouncer) this).GetThenRunIf();

    public void Reset() {
        this.lastTrigger = 0;
    }
}