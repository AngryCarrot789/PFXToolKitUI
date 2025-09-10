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
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;

namespace PFXToolKitUI.Avalonia.Services;

/// <summary>
/// A service available on desktop platforms, e.g. windows. Provides utilities for accessing windows, absolute cursor positions, etc.
/// </summary>
public interface IDesktopService {
    /// <summary>
    /// Tries to get the application's desktop service instance. Returns false if the app isn't running on desktop
    /// </summary>
    /// <param name="service"></param>
    /// <returns></returns>
    public static bool TryGetInstance([NotNullWhen(true)] out IDesktopService? service) {
        return ApplicationPFX.TryGetService(out service);
    }

    /// <summary>
    /// Gets the classic desktop application lifetime
    /// </summary>
    IClassicDesktopStyleApplicationLifetime ApplicationLifetime { get; }

    /// <summary>
    /// Sets the absolute mouse cursor position
    /// </summary>
    /// <param name="pointer"></param>
    /// <param name="x">New Mouse X position</param>
    /// <param name="y">New Mouse X position</param>
    bool SetCursorPosition(IPointer pointer, int x, int y);

    /// <summary>
    /// Gets the absolute mouse cursor position
    /// </summary>
    /// <param name="pointer"></param>
    /// <param name="x">[out] The X position</param>
    /// <param name="y">[out] The Y position</param>
    /// <returns>True if the cursor position was accessed successfully</returns>
    bool GetCursorPosition(IPointer pointer, out int x, out int y);
}