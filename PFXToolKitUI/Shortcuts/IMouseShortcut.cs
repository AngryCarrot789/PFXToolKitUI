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
using PFXToolKitUI.Shortcuts.Usage;

namespace PFXToolKitUI.Shortcuts;

/// <summary>
/// An interface for shortcuts that accept mouse inputs
/// </summary>
public interface IMouseShortcut : IShortcut {
    /// <summary>
    /// All of the Mouse Strokes that this shortcut contains
    /// </summary>
    IEnumerable<MouseStroke> MouseStrokes { get; }

    /// <summary>
    /// This can be used in order to track the usage of <see cref="IShortcut.InputStrokes"/>. If
    /// the list is empty, then the return value of this function is effectively pointless
    /// </summary>
    /// <returns></returns>
    IMouseShortcutUsage CreateMouseUsage();
}