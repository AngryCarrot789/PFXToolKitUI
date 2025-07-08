// 
// Copyright (c) 2023-2025 REghZy
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
using PFXToolKitUI.Utils.Collections.Observable;
using PFXToolKitUI.Utils.RDA;

namespace PFXToolKitUI.Logging;

/// <summary>
/// A thread-safe logger that contains a list of log entries. Logged entries are temporarily stored 
/// </summary>
public class AppLogger {
    public static AppLogger Instance { get; } = new AppLogger();

    private readonly ConcurrentQueue<LogEntry> queuedEntries;
    private readonly ObservableList<LogEntry> entries;
    private readonly RateLimitedDispatchAction delayedFlush;

    public ReadOnlyObservableList<LogEntry> Entries { get; }

    public AppLogger() {
        this.entries = new ObservableList<LogEntry>();
        this.Entries = new ReadOnlyObservableList<LogEntry>(this.entries);

        // We use a delayed flushing mechanism in order to reduce complete UI stall if
        // some random thread is dumping 10000s of log entries into the UI.
        this.queuedEntries = new ConcurrentQueue<LogEntry>();
        this.delayedFlush = RateLimitedDispatchActionBase.ForDispatcherSync(this.FlushEntriesAMT, TimeSpan.FromMilliseconds(50), DispatchPriority.BeforeRender);
        this.delayedFlush.DebugName = nameof(AppLogger);
    }

    /// <summary>
    /// Flushes cached entries to our <see cref="entries"/> collection
    /// </summary>
    /// <returns></returns>
    public void FlushEntriesAMT() {
        List<LogEntry> newEntries = new List<LogEntry>();
        for (int i = 0; i < 50 && this.queuedEntries.TryDequeue(out LogEntry? entry); i++) {
            newEntries.Add(entry);
        }

        if (newEntries.Count < 1) return;

        const int EntryLimit = 500;
        int excess = this.entries.Count + newEntries.Count;
        if (excess > EntryLimit) // remove (505-500)=5
            this.entries.RemoveRange(0, excess - EntryLimit);

        this.entries.AddRange(newEntries);
        
        if (!this.queuedEntries.IsEmpty)
            this.delayedFlush.InvokeAsync();
    }

    static AppLogger() {
    }

    private static bool CanonicalizeLine(ref string line) {
        if (string.IsNullOrEmpty(line))
            return false;

        if (line[line.Length - 1] == '\n') {
            int offset = line.Length > 1 && line[line.Length - 2] == '\r' ? 2 : 1;
            line = line.AsSpan(0, line.Length - offset).Trim().ToString();
        }
        else {
            line = line.Trim();
        }

        return true;
    }

    /// <summary>
    /// Writes an entry to the application logger. Writes nothing if the line is empty
    /// </summary>
    /// <param name="line"></param>
    public void WriteLine(string line) {
        if (!CanonicalizeLine(ref line))
            return;

        LogEntry entry = new LogEntry(DateTime.Now, Environment.StackTrace, line);
        this.queuedEntries.Enqueue(entry);
        this.delayedFlush.InvokeAsync();

        string text = $"[{entry.LogTime:hh:mm:ss}] {entry.Content}";
        Console.WriteLine(text);
        System.Diagnostics.Debug.WriteLine(text);
    }
}