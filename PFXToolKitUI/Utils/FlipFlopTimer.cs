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

namespace PFXToolKitUI.Utils;

public delegate void FlipFlopTimerEventHandler(FlipFlopTimer sender);

public delegate void FlipFlopTimerStateChangedEventHandler(FlipFlopTimer sender, bool isHigh);

public class FlipFlopTimer {
    private TimeSpan interval;
    private int levelChangesToStop;
    private int highState = -1;
    private long totalChangesSinceStart;
    private bool isEnabled, isDisabledForLevelChangeLimit;
    private bool startHigh;
    private IDispatcherTimer? timer;

    /// <summary>
    /// Gets or sets the time between level changes
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Interval is invalid</exception>
    public TimeSpan Interval {
        get => this.interval;
        set {
            double totalMs = value.TotalMilliseconds;
            if (totalMs < 1.0)
                throw new ArgumentOutOfRangeException(nameof(value), value, "Total milliseconds must be >= 1");
            if (totalMs > int.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(value), value, "Total milliseconds too large");

            PropertyHelper.SetAndRaiseINE(ref this.interval, value, this, static t => t.IntervalChanged?.Invoke(t));

            if (this.timer != null)
                this.timer.Interval = value;
        }
    }

    /// <summary>
    /// Gets or sets the amount of level changes that must happen for the internal timer to be disabled automatically.
    /// Default is 0, meaning auto-disable is not processed.
    /// <para>
    /// Note, when the timer is stopped, <see cref="IsEnabled"/> will still be true, and the current state will be held.
    /// When <see cref="IsEnabled"/> is set to false, the state is set to low.
    /// </para>
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Value is negative</exception>
    public int LevelChangesToStop {
        get => this.levelChangesToStop;
        set {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), value, "Must cannot be negative");
            PropertyHelper.SetAndRaiseINE(ref this.levelChangesToStop, value, this, static t => t.IntervalToAutoStopChanged?.Invoke(t));

            if (this.IsEnabled) {
                if (this.totalChangesSinceStart >= value && value != 0) {
                    this.DisableForLevelLimitReached();
                }
                else if (this.isDisabledForLevelChangeLimit) {
                    this.isDisabledForLevelChangeLimit = false;
                    this.timer?.Start();
                }
            }
        }
    }

    /// <summary>
    /// Gets or sets if the flip-flop is enabled. When set to false, we switch back to the first exchange. When true, the timer is
    /// started, and we start switching between brushes which fires <see cref="IsHighChanged"/>
    /// </summary>
    public bool IsEnabled {
        get => this.isEnabled;
        set {
            if (this.isEnabled != value) {
                this.isEnabled = value;
                this.IsEnabledChanged?.Invoke(this);
                this.OnIsEnabledChanged(value);
            }
        }
    }

    /// <summary>
    /// Gets or sets whether the state should start out as high when <see cref="IsEnabled"/> is set to true. Default value is false.
    /// <para>
    /// When set to true, setting <see cref="IsEnabled"/> to true counts as a state chance, whereas when this value is false it does not.
    /// Therefore, this value does not affect the behaviour of <see cref="LevelChangesToStop"/>
    /// </para>
    /// </summary>
    public bool StartHigh {
        get => this.startHigh;
        set => PropertyHelper.SetAndRaiseINE(ref this.startHigh, value, this, static t => t.StartHighChanged?.Invoke(t));
    }
    
    public bool IsHigh => this.highState > 0;

    public event FlipFlopTimerEventHandler? IntervalChanged;
    public event FlipFlopTimerEventHandler? IntervalToAutoStopChanged;
    public event FlipFlopTimerEventHandler? IsEnabledChanged;
    public event FlipFlopTimerEventHandler? StartHighChanged;

    /// <summary>
    /// Fired when the high state changes.
    /// </summary>
    public event FlipFlopTimerStateChangedEventHandler? IsHighChanged;

    public FlipFlopTimer(TimeSpan interval) {
        this.Interval = interval;
    }

    protected virtual void OnIsHighChanged(bool isHigh) {
        this.IsHighChanged?.Invoke(this, isHigh);
    }

    private void OnIsEnabledChanged(bool value) {
        bool newState = this.StartHigh;
        this.isDisabledForLevelChangeLimit = false;
        this.totalChangesSinceStart = newState ? 1 : 0;
        
        if (value) {
            if (this.timer == null) {
                this.timer = ApplicationPFX.Instance.Dispatcher.CreateTimer(DispatchPriority.Normal);
                this.timer.Interval = this.Interval;
                this.timer.Tick += this.OnTimerTicked;
            }

            if (this.highState == -1) this.highState = newState ? 1 : 0;

            if (!this.timer.IsEnabled)
                this.timer.Start();
        }
        else {
            this.highState = -1;
            this.timer?.Stop();
        }

        this.OnIsHighChanged(this.IsHigh);
    }

    private void OnTimerTicked(object? sender, EventArgs e) {
        if (this.highState == -1) {
            return; // disabled... somehow
        }

        this.highState = this.highState == 0 ? 1 : 0;
        this.OnIsHighChanged(this.IsHigh);
            
        // We process the change, even if levelChangesToStop is 1
        if (++this.totalChangesSinceStart >= this.levelChangesToStop && this.levelChangesToStop != 0) {
            this.DisableForLevelLimitReached();
        }
    }

    private void DisableForLevelLimitReached() {
        this.isDisabledForLevelChangeLimit = true;
        this.timer?.Stop();
    }
}