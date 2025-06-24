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

using PFXToolKitUI.Interactivity.Contexts;

namespace PFXToolKitUI.Interactivity;

/// <summary>
/// An interface that represents a window in the windowing system
/// </summary>
public interface IDesktopWindow {
    public static readonly DataKey<IDesktopWindow> DataKey = DataKey<IDesktopWindow>.Create("TopLevel_DesktopWindow");
    
    /// <summary>
    /// Gets this window's clipboard service
    /// </summary>
    IClipboardService? ClipboardService { get; }
}