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

namespace PFXToolKitUI;

/// <summary>
/// A timer that runs the <see cref="Tick"/> callback every <see cref="Interval"/> time, on the dispatcher thread
/// </summary>
public interface IDispatcherTimer {
    /// <summary>
    /// The dispatcher this timer is running on
    /// </summary>
    IDispatcher Dispatcher { get; }

    /// <summary>
    /// Gets or sets if the timer is running.
    /// </summary>
    bool IsEnabled { get; set; }

    /// <summary>
    /// Gets or sets the callback interval. Note, updating this will may cause <see cref="Tick"/> to be invoked sooner or later
    /// </summary>
    TimeSpan Interval { get; set; }

    /// <summary>
    /// The callback fired on the dispatcher's thread
    /// </summary>
    event EventHandler? Tick;

    /// <summary>
    /// Starts the timer. This is equivalent to setting <see cref="IsEnabled"/> to true
    /// </summary>
    public void Start();

    /// <summary>
    /// Stops the timer. This is equivalent to setting <see cref="IsEnabled"/> to false
    /// </summary>
    public void Stop();
}