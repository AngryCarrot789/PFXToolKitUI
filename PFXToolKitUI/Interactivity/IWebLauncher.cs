// 
// Copyright (c) 2023-2025 REghZy
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
using PFXToolKitUI.Interactivity.Windowing;

namespace PFXToolKitUI.Interactivity;

/// <summary>
/// A service that allows launching a URL in a web browser
/// </summary>
public interface IWebLauncher {
    /// <summary>Tries to get the web launcher service for a window</summary>
    public static bool TryGet(ITopLevelComponentManager window, [NotNullWhen(true)] out IWebLauncher? launcher) => window.TryGetWebLauncher(out launcher);
    
    /// <summary>
    /// Tries to launch the uri in a web browser
    /// </summary>
    /// <param name="uri">The address</param>
    /// <returns>A task returns true when successful</returns>
    Task<bool> LaunchUriAsync(Uri uri);
}