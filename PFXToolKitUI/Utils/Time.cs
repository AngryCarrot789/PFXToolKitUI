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
    public static async Task DelayForAsync(TimeSpan delay, CancellationToken cancellation, int osThreadPeriod = 18) {
        double nextTick = Stopwatch.GetTimestamp() / (double) TimeSpan.TicksPerMillisecond + delay.TotalMilliseconds;
        int ms = (int) delay.TotalMilliseconds;
        
        if (ms >= osThreadPeriod) {
            // average windows thread-slice time == 15~ millis
            await Task.Delay(ms - osThreadPeriod, cancellation);
        }
        
        double temp;
        while ((temp = GetSystemMillis() - nextTick) < -0.05 /* say 50 micros to do continuations to reschedule */) {
            // do nothing but loop for the rest of the duration, for precise timing
            cancellation.ThrowIfCancellationRequested();
            if (temp < 2.0)
                await Task.Yield();
        }
    }
}