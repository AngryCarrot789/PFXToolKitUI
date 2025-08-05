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

namespace PFXToolKitUI.Tasks;

/// <summary>
/// Concurrent implementation of <see cref="IActivityProgress"/>
/// </summary>
public class ConcurrentActivityProgress : IActivityProgress {
    private readonly Lock dataLock = new Lock(); // only really used as a memory barrier
    private bool isIndeterminate;
    private string? headerText;
    private string? descriptionText;
    private volatile bool isTextClean = true;

    public bool IsIndeterminate {
        get => this.isIndeterminate;
        set {
            lock (this.dataLock) {
                if (this.isIndeterminate == value)
                    return;
                this.isIndeterminate = value;
            }

            this.updateIsIndeterminateRda.InvokeAsync();
        }
    }

    public string? Caption {
        get => this.headerText;
        set {
            lock (this.dataLock) {
                if (this.headerText == value)
                    return;
                this.headerText = value;
            }

            this.updateCaptionRda.InvokeAsync();
        }
    }

    public string? Text {
        get => this.descriptionText;
        set {
            lock (this.dataLock) {
                if (this.descriptionText == value)
                    return;
                this.descriptionText = value;
                this.isTextClean = false;
            }

            this.updateTextRda.InvokeAsync();
        }
    }

    /// <summary>
    /// Returns true when the text change has been processed on the main thread
    /// </summary>
    public bool IsTextClean => this.isTextClean;

    public event ActivityProgressEventHandler? IsIndeterminateChanged;
    public event ActivityProgressEventHandler? CaptionChanged;
    public event ActivityProgressEventHandler? TextChanged;

    private readonly RapidDispatchActionEx updateIsIndeterminateRda;
    private readonly RapidDispatchActionEx updateCaptionRda;
    private readonly RapidDispatchActionEx updateTextRda;
    private readonly DispatchPriority eventDispatchPriority;

    public CompletionState CompletionState { get; }
    
    public ConcurrentActivityProgress() : this(DispatchPriority.Loaded) {
    }

    public ConcurrentActivityProgress(DispatchPriority eventDispatchPriority) {
        this.eventDispatchPriority = eventDispatchPriority;
        this.updateIsIndeterminateRda = RapidDispatchActionEx.ForSync(() => this.IsIndeterminateChanged?.Invoke(this), eventDispatchPriority);
        this.updateCaptionRda = RapidDispatchActionEx.ForSync(() => this.CaptionChanged?.Invoke(this), eventDispatchPriority);
        this.updateTextRda = RapidDispatchActionEx.ForSync(() => {
            this.isTextClean = true;
            this.TextChanged?.Invoke(this);
        }, eventDispatchPriority);
        this.CompletionState = new ConcurrentCompletionState(eventDispatchPriority);
    }
}