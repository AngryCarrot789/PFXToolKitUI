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

using System.Diagnostics.CodeAnalysis;

namespace PFXToolKitUI.Interactivity.Windowing;

/// <summary>
/// Manages <see cref="ITopLevel"/> instances
/// </summary>
public interface ITopLevelManager {
    /// <summary>
    /// Tries to get the current active top-level, or at worst, a main top level.
    /// This method is not recommended since it can result in dialogs showing relative to top levels
    /// it has no business with. Instead, use <see cref="ITopLevel.FromContext"/>
    /// </summary>
    /// <param name="topLevel">The active or main top level</param>
    /// <returns>True if a top level was found</returns>
    bool TryGetActiveOrMainTopLevel([NotNullWhen(true)] out ITopLevel? topLevel);
    
    /// <summary>
    /// Tries to get the top level manager service
    /// </summary>
    /// <param name="topLevelManager">The manager, or null</param>
    /// <returns>True if a top level manager exists</returns>
    public static bool TryGetInstance([NotNullWhen(true)] out ITopLevelManager? topLevelManager) {
        return ApplicationPFX.TryGetComponent(out topLevelManager);
    }
}