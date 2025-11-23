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

namespace PFXToolKitUI.Activities;

/// <summary>
/// An implementation of <see cref="EmptyActivityProgress"/> that does nothing (no events, get/set values return default values, etc.)
/// </summary>
public class EmptyActivityProgress : IActivityProgress {
    public static readonly IActivityProgress Instance = new EmptyActivityProgress();

    bool IActivityProgress.IsIndeterminate { get => false; set { } }
    string? IActivityProgress.Caption { get => null; set { } }
    string? IActivityProgress.Text { get => null; set { } }
    CompletionState IActivityProgress.CompletionState { get; } = new EmptyCompletionState();

    event EventHandler IActivityProgress.IsIndeterminateChanged { add { } remove { } }
    event EventHandler IActivityProgress.CaptionChanged { add { } remove { } }
    event EventHandler IActivityProgress.TextChanged { add { } remove { } }

    public EmptyActivityProgress() { }
}