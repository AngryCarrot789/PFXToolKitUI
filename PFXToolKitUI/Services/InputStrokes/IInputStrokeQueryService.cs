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

using PFXToolKitUI.Interactivity.Windowing;
using PFXToolKitUI.Shortcuts.Inputs;

namespace PFXToolKitUI.Services.InputStrokes;

/// <summary>
/// A service that lets the user specify an input stroke, e.g. a key stroke or mouse clic
/// </summary>
public interface IInputStrokeQueryService {
    public static IInputStrokeQueryService Instance => ApplicationPFX.GetComponent<IInputStrokeQueryService>();

    /// <summary>
    /// Returns a user-specified KeyStroke. Simple implementation shows a dialog that the user types into
    /// </summary>
    /// <param name="initialKeyStroke">The KeyStroke to use as default.</param>
    /// <param name="parentTopLevel">
    /// The top level that should be the parent of the dialog window. When null, either the best window from
    /// the current command context is used, or at worst, the active window or a main window will be used
    /// </param>
    /// <returns>A task that provides the KeyStroke pressed, or the initial KeyStroke</returns>
    Task<KeyStroke?> GetKeyStrokeInput(KeyStroke? initialKeyStroke, ITopLevel? parentTopLevel = null);

    /// <summary>
    /// Returns a user-specified MouseStroke. Simple implementation shows a dialog that the user types into
    /// </summary>
    /// <param name="initialMouseStroke">The MouseStroke to use as default.</param>
    /// <param name="parentTopLevel">
    /// The top level that should be the parent of the dialog window. When null, either the best window from
    /// the current command context is used, or at worst, the active window or a main window will be used
    /// </param>
    /// <returns>A task that provides the MouseStroke pressed, or the initial MouseStroke</returns>
    Task<MouseStroke?> GetMouseStroke(MouseStroke? initialMouseStroke, ITopLevel? parentTopLevel = null);
}