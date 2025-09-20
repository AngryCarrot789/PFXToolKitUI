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

namespace PFXToolKitUI.Tasks;

public delegate void ActivityProgressEventHandler(IActivityProgress tracker);

/// <summary>
/// Used to track progression of an activity
/// </summary>
public interface IActivityProgress {
    /// <summary>
    /// Gets or sets if this tracker's completions state is indeterminate. True by default
    /// </summary>
    bool IsIndeterminate { get; set; }

    /// <summary>
    /// Gets or sets the caption text (aka operation caption). This can be a description of what the main operation is
    /// </summary>
    string? Caption { get; set; }

    /// <summary>
    /// Gets or sets the description text (aka operation description). This can be what is currently going on
    /// </summary>
    string? Text { get; set; }

    /// <summary>
    /// An event fired when the <see cref="IsIndeterminate"/> property changes
    /// </summary>
    event ActivityProgressEventHandler IsIndeterminateChanged;

    /// <summary>
    /// An event fired when the <see cref="Caption"/> property changes
    /// </summary>
    event ActivityProgressEventHandler CaptionChanged;

    /// <summary>
    /// An event fired when the <see cref="Text"/> property changes
    /// </summary>
    event ActivityProgressEventHandler TextChanged;

    /// <summary>
    /// Gets this activity's completion state
    /// </summary>
    CompletionState CompletionState { get; }

    /// <summary>
    /// Saves the values of <see cref="IsIndeterminate"/>, <see cref="Caption"/> and
    /// <see cref="Text"/>, which can then be restored by disposing the returned struct
    /// </summary>
    /// <returns>
    /// A struct that stores the current property values and can restore them when disposed
    /// </returns>
    State SaveState() => new(this);

    void SetCaptionAndText(string value) {
        this.Caption = value;
        this.Text = value;
    }

    void SetCaptionAndText(string caption, string text) {
        this.Caption = caption;
        this.Text = text;
    }

    public readonly struct State(IActivityProgress progress) : IDisposable {
        public readonly bool IsIndeterminate = progress.IsIndeterminate;
        public readonly string? Caption = progress.Caption;
        public readonly string? Text = progress.Text;

        public void Dispose() {
            progress.IsIndeterminate = this.IsIndeterminate;
            progress.Caption = this.Caption;
            progress.Text = this.Text;
        }
    }
}