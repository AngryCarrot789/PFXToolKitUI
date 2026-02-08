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
/// A class that posts a callback to the dispatcher once a specific amount of time since the last post attempt has elapsed
/// </summary>
public class TimerDispatcherDebouncer {
    private static readonly SendOrPostCallback s_CallbackWithAction = static a => ((Action) a!)();
    private readonly SendOrPostCallback callback;
    private readonly object? state;
    private readonly IDispatcherTimer timer;
    private long lastTrigger;

    /// <summary>
    /// Gets the interval of this debouncer
    /// </summary>
    public TimeSpan Interval {
        get => this.timer.Interval;
        set => this.timer.Interval = value;
    }

    /// <summary>
    /// Gets the dispatcher we post the callback to
    /// </summary>
    public IDispatcher Dispatcher { get; }

    /// <summary>
    /// Gets the dispatch priority for the callback
    /// </summary>
    public DispatchPriority Priority { get; }

    /// <summary>
    /// Gets whether enough time has elapsed since our callback was last invoked
    /// </summary>
    public bool HasEnoughTimeElapsed => Time.GetSystemTicks() - this.lastTrigger >= this.Interval.Ticks;

    /// <summary>
    /// Gets whether the callback has been postponed, and we're still waiting for it to be invoked
    /// </summary>
    public bool IsWaiting => this.timer.IsEnabled;

    public TimerDispatcherDebouncer(TimeSpan interval, Action callback, DispatchPriority priority = DispatchPriority.Default) : this(interval, callback, ApplicationPFX.Instance.Dispatcher, priority) {
    }

    public TimerDispatcherDebouncer(TimeSpan interval, Action callback, IDispatcher dispatcher, DispatchPriority priority = DispatchPriority.Default) : this(interval, s_CallbackWithAction, callback, dispatcher, priority) {
    }

    public TimerDispatcherDebouncer(TimeSpan interval, SendOrPostCallback callback, object? state, DispatchPriority priority = DispatchPriority.Default) : this(interval, callback, state, ApplicationPFX.Instance.Dispatcher, priority) {
    }

    public TimerDispatcherDebouncer(TimeSpan interval, SendOrPostCallback callback, object? state, IDispatcher dispatcher, DispatchPriority priority = DispatchPriority.Default) {
        this.callback = callback;
        this.state = state;
        this.Dispatcher = dispatcher;
        this.Priority = priority;
        this.timer = dispatcher.CreateTimer(priority);
        this.timer.Interval = interval;
        this.timer.Tick += this.OnTimerTicked;
    }

    /// <summary>
    /// If enough time has elapsed, then invoke the callback. Otherwise, push the timer forward and wait longer to invoke.
    /// </summary>
    /// <returns>True if the callback was invoked. False if it was postponed</returns>
    public bool TryInvokeOrPostpone() {
        if (this.TryInvoke()) {
            return true;
        }

        this.lastTrigger = Time.GetSystemTicks();
        this.timer.Stop();
        this.timer.Start();
        return false;
    }
    
    /// <summary>
    /// Try to invoke the callback. Otherwise, do nothing.
    /// </summary>
    /// <returns>True when the callback was invoked</returns>
    public bool TryInvoke() {
        if (this.lastTrigger == 0) {
            this.lastTrigger = Time.GetSystemTicks();
        }
        
        if (!this.HasEnoughTimeElapsed) {
            return false;
        }

        // Stop the timer in case it's running and its callback is scheduled 
        this.StopTimerAndInvoke();
        return true;
    }

    /// <summary>
    /// Resets the timer, preventing the callback from being dispatched and invoked later.
    /// </summary>
    public void Reset() {
        this.timer.Stop();
        this.lastTrigger = 0;
    }

    private void OnTimerTicked(object? sender, EventArgs e) {
        this.StopTimerAndInvoke();
    }

    private void StopTimerAndInvoke() {
        this.timer.Stop();
        this.lastTrigger = 0;
        this.callback(this.state);
    }
}