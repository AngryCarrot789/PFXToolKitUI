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

using Avalonia.Controls;

namespace PFXToolKitUI.Avalonia.Services.Windowing;

/// <summary>
/// An event handler for when a window is trying to close
/// <param name="window">The sender window</param>
/// <param name="reason">The reason for window close</param>
/// <param name="isCancelled">True when another handler has cancelled the close</param>
/// </summary>
/// <returns>
/// A task which contains the cancellation boolean. True means do not close,
/// False means we don't want to stop it from closing (another handler may cancel though)
/// </returns>
public delegate Task<bool> DesktopWindowClosingAsyncEventHandler(DesktopWindow sender, WindowCloseReason reason, bool isCancelled);
