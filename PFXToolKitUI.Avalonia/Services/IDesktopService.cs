// 
// Copyright (c) 2024-2025 REghZy
// 
// This file is part of FramePFX.
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
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
// 

using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls.ApplicationLifetimes;

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
        return ApplicationPFX.Instance.ServiceManager.TryGetService(out service);
    }

    /// <summary>
    /// Gets the classic desktop application lifetime
    /// </summary>
    IClassicDesktopStyleApplicationLifetime ApplicationLifetime { get; }

    /// <summary>
    /// Sets the absolute mouse cursor position
    /// </summary>
    /// <param name="x">New Mouse X position</param>
    /// <param name="y">New Mouse X position</param>
    bool SetCursorPosition(int x, int y);
    
    /// <summary>
    /// Gets the absolute mouse cursor position
    /// </summary>
    /// <param name="x">[out] The X position</param>
    /// <param name="y">[out] The Y position</param>
    /// <returns>True if the cursor position was accessed successfully</returns>
    bool GetCursorPosition(out int x, out int y);
}