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

using PFXToolKitUI.Utils.RDA;

namespace PFXToolKitUI.Activities;

/// <summary>
/// A concurrent implementation of <see cref="IActivityProgress"/> that
/// dispatches property change events to the main UI thread dispatcher
/// </summary>
public class DispatcherActivityProgress : IActivityProgress {
    private readonly Lock dataLock = new Lock(); // only really used as a memory barrier
    private volatile bool isTextClean = true;

    public bool IsIndeterminate {
        get => field;
        set {
            lock (this.dataLock) {
                if (field == value)
                    return;
                field = value;
            }

            this.updateIsIndeterminateRda.InvokeAsync();
        }
    }

    public string? Caption {
        get => field;
        set {
            lock (this.dataLock) {
                if (field == value)
                    return;
                field = value;
            }

            this.updateCaptionRda.InvokeAsync();
        }
    }

    public string? Text {
        get => field;
        set {
            lock (this.dataLock) {
                if (field == value)
                    return;
                field = value;
                this.isTextClean = false;
            }

            this.updateTextRda.InvokeAsync();
        }
    }

    /// <summary>
    /// Returns true when the text change has been processed on the main thread
    /// </summary>
    public bool IsTextClean => this.isTextClean;

    public event EventHandler? IsIndeterminateChanged;
    public event EventHandler? CaptionChanged;
    public event EventHandler? TextChanged;

    private readonly RapidDispatchActionEx updateIsIndeterminateRda;
    private readonly RapidDispatchActionEx updateCaptionRda;
    private readonly RapidDispatchActionEx updateTextRda;
    private readonly DispatchPriority eventDispatchPriority;

    public CompletionState CompletionState { get; }
    
    public DispatcherActivityProgress() : this(DispatchPriority.Loaded) {
    }

    public DispatcherActivityProgress(DispatchPriority eventDispatchPriority) {
        this.eventDispatchPriority = eventDispatchPriority;
        this.updateIsIndeterminateRda = RapidDispatchActionEx.ForSync(() => this.IsIndeterminateChanged?.Invoke(this, EventArgs.Empty), eventDispatchPriority);
        this.updateCaptionRda = RapidDispatchActionEx.ForSync(() => this.CaptionChanged?.Invoke(this, EventArgs.Empty), eventDispatchPriority);
        this.updateTextRda = RapidDispatchActionEx.ForSync(() => {
            this.isTextClean = true;
            this.TextChanged?.Invoke(this, EventArgs.Empty);
        }, eventDispatchPriority);
        this.CompletionState = new ConcurrentCompletionState(eventDispatchPriority);
    }
}