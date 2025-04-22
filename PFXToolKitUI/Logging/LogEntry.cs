// 
// Copyright (c) 2024-2025 REghZy
// 
// This file is part of FramePFX.
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
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
// 

namespace PFXToolKitUI.Logging;

public class LogEntry {
    /// <summary>
    /// Gets the time at which this log entry was created
    /// </summary>
    public DateTime LogTime { get; }

    /// <summary>
    /// Gets the stack trace of the log entry's creation, which contains the call stack up to something being logged
    /// </summary>
    public string StackTrace { get; }

    /// <summary>
    /// Gets the string content containing the log message
    /// </summary>
    public string Content { get; }

    public LogEntry(DateTime logTime, string stackTrace, string content) {
        this.LogTime = logTime;
        this.StackTrace = stackTrace;
        this.Content = content;
    }
}