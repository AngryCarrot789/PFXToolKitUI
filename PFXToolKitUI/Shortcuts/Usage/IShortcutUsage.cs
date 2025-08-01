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

using PFXToolKitUI.Shortcuts.Inputs;

namespace PFXToolKitUI.Shortcuts.Usage;

/// <summary>
/// An interface for all shortcut "usages". Usages are used when a shortcut requires
/// more than 1 input stroke to activate it
/// </summary>
public interface IShortcutUsage {
    /// <summary>
    /// A reference to the shortcut that created this usage instance
    /// </summary>
    IShortcut Shortcut { get; }

    /// <summary>
    /// Whether this input usage has been completed and is ready to be activated or cancelled (<see cref="CurrentStroke"/> will be null
    /// and <see cref="PreviousStroke"/> will be the last input stroke for this shortcut usage)
    /// </summary>
    bool IsCompleted { get; }

    /// <summary>
    /// The input stroke that was previously the <see cref="CurrentStroke"/> before it was progressed
    /// <para>
    /// Given a shortcut with 3 inputs: A, B and C, <see cref="CurrentStroke"/> and <see cref="PreviousStroke"/> will never be A, and <see cref="PreviousStroke"/> will be null if <see cref="CurrentStroke"/> is B, but <see cref="PreviousStroke"/> will be B if <see cref="CurrentStroke"/> is C
    /// </para>
    /// </summary>
    IInputStroke PreviousStroke { get; }

    /// <summary>
    /// The input stroke that is required to be input next in order for this usage to be progressed successfully
    /// <para>
    /// Given a shortcut with 3 inputs: A, B and C, <see cref="CurrentStroke"/> will never be A, but it may be B or C
    /// </para>
    /// </summary>
    IInputStroke CurrentStroke { get; }

    /// <summary>
    /// Returns an enumerable of the remaining input strokes required for this usage to be completed
    /// </summary>
    IEnumerable<IInputStroke> RemainingStrokes { get; }

    /// <summary>
    /// Whether the current stroke is a mouse stroke or not
    /// </summary>
    bool IsCurrentStrokeMouseBased { get; }

    /// <summary>
    /// Whether the current stroke is a key stroke or not
    /// </summary>
    bool IsCurrentStrokeKeyBased { get; }

    /// <summary>
    /// Attempts to progress current stroke to the next stroke in the sequence. If this usage is already
    /// completed (<see cref="IShortcutUsage.IsCompleted"/>), then true is returned. If the current stroke matches
    /// the input stroke, then the current stroke is progressed and true is returned. Otherwise, false is returned
    /// </summary>
    /// <param name="stroke"></param>
    /// <returns>
    /// Whether the stroke matches the current stroke, or if this usage is already completed
    /// </returns>
    bool OnInputStroke(IInputStroke stroke);
}