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

namespace PFXToolKitUI.Services.InputStrokes;

/// <summary>
/// A service that lets the user specify an input stroke, e.g. a key stroke or mouse clic
/// </summary>
public interface IInputStrokeQueryService {
    public static IInputStrokeQueryService Instance => ApplicationPFX.GetService<IInputStrokeQueryService>();

    /// <summary>
    /// Returns a user-specified key stroke. Simple implementation shows a dialog that the user types into
    /// </summary>
    /// <param name="initialKeyStroke">The key stroke to use as default.</param>
    /// <returns>A task that provides the key stroke pressed, or the initial key stroke</returns>
    Task<KeyStroke?> GetKeyStrokeInput(KeyStroke? initialKeyStroke);

    /// <summary>
    /// Returns a user-specified mouse stroke. Simple implementation shows a dialog that the user types into
    /// </summary>
    /// <param name="initialMouseStroke">The mouse stroke to use as default.</param>
    /// <returns>A task that provides the mouse stroke pressed, or the initial mouse stroke</returns>
    Task<MouseStroke?> GetMouseStroke(MouseStroke? initialMouseStroke);
}