// 
// Copyright (c) 2025-2025 REghZy
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
using PFXToolKitUI.Composition;
using PFXToolKitUI.Interactivity.Contexts;

namespace PFXToolKitUI.Interactivity.Windowing;

/// <summary>
/// An interface that represents a top-level UI component like a window or overlay.
/// </summary>
public interface ITopLevel : IComponentManager {
    /// <summary>
    /// Gets the data key for a top level
    /// </summary>
    public static readonly DataKey<ITopLevel> TopLevelDataKey = DataKeys.Create<ITopLevel>("WindowingTopLevel");

    /// <summary>
    /// Gets the local context data of this top level (not the fully inherited context). This
    /// </summary>
    IMutableContextData LocalContextData { get; }

    /// <summary>
    /// Tries to get the clipboard component
    /// </summary>
    /// <param name="clipboard">The clipboard</param>
    /// <returns>True if the top level supports a clipboard</returns>
    bool TryGetClipboard([NotNullWhen(true)] out IClipboardService? clipboard) => this.TryGetComponent(out clipboard);

    /// <summary>
    /// Tries to get the web launcher
    /// </summary>
    /// <param name="launcer">The launcher</param>
    /// <returns>True if the top level supports launching things in a web browser</returns>
    bool TryGetWebLauncher([NotNullWhen(true)] out IWebLauncher? launcher) => this.TryGetComponent(out launcher);

    /// <summary>
    /// Gets the top level from the context data, or null, if there is no top level available
    /// </summary>
    /// <param name="context">The context</param>
    /// <returns>The top level</returns>
    static ITopLevel? FromContext(IContextData context) => TopLevelDataKey.GetContext(context);
    
    /// <summary>
    /// Tries to get the top level from the context data.
    /// </summary>
    /// <param name="context">The context</param>
    /// <param name="topLevel">The top level</param>
    /// <returns>True if a top level was available</returns>
    static bool TryGetFromContext(IContextData context, [NotNullWhen(true)] out ITopLevel? topLevel) => TopLevelDataKey.TryGetContext(context, out topLevel);
}