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
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace PFXToolKitUI.Utils;

/// <summary>
/// Provides access to the system time
/// </summary>
public static class Time {
    /// <summary>
    /// Gets the system's performance counter ticks
    /// </summary>
    public static long GetSystemTicks() => Stopwatch.GetTimestamp();

    /// <summary>
    /// Gets the system's performance counter ticks and converts them to milliseconds
    /// </summary>
    public static long GetSystemMillis() {
        return Stopwatch.GetTimestamp() / TimeSpan.TicksPerMillisecond;
    }

    /// <summary>
    /// Gets the system's performance counter ticks and converts them to milliseconds, as a double instead
    /// </summary>
    public static double GetSystemMillisD() {
        return (double) Stopwatch.GetTimestamp() / TimeSpan.TicksPerMillisecond;
    }

    /// <summary>
    /// Gets the system's performance counter ticks and converts them to seconds
    /// </summary>
    /// <returns></returns>
    public static long GetSystemSeconds() {
        return Stopwatch.GetTimestamp() / TimeSpan.TicksPerSecond;
    }

    /// <summary>
    /// Gets the system's performance counter ticks and converts them to decimal seconds
    /// </summary>
    /// <returns></returns>
    public static double GetSystemSecondsD() {
        return (double) Stopwatch.GetTimestamp() / TimeSpan.TicksPerSecond;
    }

    /// <summary>
    /// Precision thread sleep timing
    /// <para>
    /// This should, in most cases, guarantee the exact amount of time wanted
    /// </para>
    /// </summary>
    /// <param name="delay">The exact number of milliseconds to sleep for</param>
    /// <param name="osThreadPeriod">The maximum amount of milliseconds between os thread time slices</param>
    public static void SleepFor(double delay, int osThreadPeriod = 18) {
        if (delay >= osThreadPeriod) {
            // average windows thread-slice time == 15~ millis
            Thread.Sleep((int) (delay - osThreadPeriod));
        }

        double nextTick = GetSystemMillisD() + delay;
        while (GetSystemMillisD() < nextTick) {
            // do nothing but loop for the rest of the duration, for precise timing
        }
    }

    /// <summary>
    /// Precision thread sleep timing
    /// <para>
    /// This should, in most cases, guarantee the exact amount of time wanted
    /// </para>
    /// </summary>
    /// <param name="delay">The exact number of milliseconds to sleep for</param>
    /// <param name="osThreadPeriod">The maximum amount of milliseconds between os thread time slices</param>
    public static void SleepFor(long delay, int osThreadPeriod = 18) {
        if (delay >= osThreadPeriod) {
            // average windows thread-slice time == 15~ millis
            Thread.Sleep((int) (delay - osThreadPeriod));
        }

        long nextTick = GetSystemMillis() + delay;
        while (GetSystemMillis() < nextTick) {
            // do nothing but loop for the rest of the duration, for precise timing
        }
    }

    /// <summary>
    /// Asynchronous delay for a precision amount of time. When sleeping for anything less than 18 milliseconds,
    /// <see cref="Task.Delay(int)"/> isn't used, therefore, this method is effectively blocking
    /// </summary>
    /// <param name="delay">The amount of time to delay for</param>
    /// <param name="cancellation">Signals the delay to become cancelled</param>
    /// <param name="osThreadPeriod">The maximum amount of milliseconds between os thread time slices</param>
    public static Task DelayForAsync(TimeSpan delay, CancellationToken cancellation, int osThreadPeriod = 18) {
        return DelayForAsyncImpl(delay.TotalMilliseconds, true, cancellation, osThreadPeriod);
    }

    private static async Task DelayForAsyncImpl(double milliseconds, bool canSpin, CancellationToken cancellation, int osThreadPeriod = 18) {
        double nextTick = Stopwatch.GetTimestamp() / (double) TimeSpan.TicksPerMillisecond + milliseconds;
        int ms = (int) milliseconds;

        if (ms >= osThreadPeriod) {
            // average windows thread-slice time == 15~ millis
            await Task.Delay(ms - osThreadPeriod, cancellation);
        }

        if (canSpin) {
            double temp;
            while ((temp = GetSystemMillis() - nextTick) < -0.05 /* say 50 micros to do continuations to reschedule */) {
                // do nothing but loop for the rest of the duration, for precise timing
                cancellation.ThrowIfCancellationRequested();
                if (temp < 2.0)
                    await Task.Yield();
            }
        }
    }

    /// <summary>
    /// Returns a task that completes once the delay has elapsed using the OS's high-resolution timer mechanisms
    /// </summary>
    /// <param name="delay">The delay</param>
    /// <param name="canSpin">
    /// Whether spin-waiting the final amount of time is allowed. When false, the task may complete too early.
    /// </param>
    /// <param name="cancellation">A cancellation token to cancel the delay operation</param>
    /// <returns></returns>
    /// <exception cref="OperationCanceledException">The cancellation token was cancelled</exception>
    public static Task DelayForAsyncEx(TimeSpan delay, bool canSpin = true, CancellationToken cancellation = default) {
        if (OperatingSystem.IsWindows()) {
            // if (OperatingSystem.IsWindowsVersionAtLeast(1803)) {
            //     return Win32HighResolutionTimer.DelayForAsync(delay, cancellation);
            // }
            // else {
            return Win32Natives.DelayForAsync(delay, canSpin, cancellation);
            // }
        }
        else {
            return DelayForAsync(delay, cancellation);
        }
    }

    [SupportedOSPlatform("windows")]
    private static class Win32Natives {
        [DllImport("winmm.dll")]
        private static extern uint timeBeginPeriod(uint uPeriod /* millis */);

        [DllImport("winmm.dll")]
        private static extern uint timeEndPeriod(uint uPeriod);

        // SEE: https://blog.bearcats.nl/perfect-sleep-function/
        public static async Task DelayForAsync(TimeSpan delay, bool canSpin, CancellationToken cancellation) {
            uint result = timeBeginPeriod(1);
            try {
                if (result == 0 /* success */) {
                    await Task.Run(async () => {
                        long targetTick = Stopwatch.GetTimestamp() + delay.Ticks;
                        int delayMillis = (int) (delay.TotalMilliseconds - (canSpin ? 1.1 /* 0.1 tolerance */ : 0));
                        if (delayMillis > 0)
                            await Task.Delay(delayMillis, cancellation).ConfigureAwait(false);

                        if (canSpin) {
                            while (Stopwatch.GetTimestamp() < targetTick)
                                await Task.Yield();
                        }
                    }, cancellation);
                }
                else {
                    await DelayForAsyncImpl(delay.TotalMilliseconds, canSpin, cancellation).ConfigureAwait(false);
                }
            }
            finally {
                _ = timeEndPeriod(1);
            }
        }
    }

    // [SupportedOSPlatform("windows")]
    // private static class Win32HighResolutionTimer {
    //     public static async Task DelayForAsync(TimeSpan delay, CancellationToken cancellation) {
    //     }
    // }
}