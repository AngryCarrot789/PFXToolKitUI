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

using PFXToolKitUI.Avalonia.Interactivity.Windowing.Desktop;
using PFXToolKitUI.Avalonia.Interactivity.Windowing.Overlays;

namespace PFXToolKitUI.Avalonia.Interactivity.Windowing;

/// <summary>
/// An enum that specifies the opened state of an <see cref="IDesktopWindow"/> or <see cref="IOverlayWindow"/>
/// </summary>
public enum OpenState {
    /// <summary>
    /// The window or overlay has not been shown yet. This is the default state for a newly created windows or overlays.
    /// </summary>
    NotOpened,

    /// <summary>
    /// The window or overlay is in the process of opening
    /// </summary>
    Opening,

    /// <summary>
    /// The window or overlay is open and visible on screen
    /// </summary>
    Open,

    /// <summary>
    /// The window or overlay has been requested to close. Its "try" close event handlers will be invoked which may wish to cancel the close operation.
    /// <para>
    /// The window or overlay will still be open at this point, and its open state may return back to <see cref="Open"/> if the close operation is cancelled
    /// </para>
    /// <para>
    /// Note, overlay windows may be forced closed, and may not actually enter this state. Therefore, their <see cref="IOverlayWindow.TryClose"/>
    /// and <see cref="IOverlayWindow.TryCloseAsync"/> handlers won't be invoked. It's extremely important that
    /// <see cref="OverlayWindowCloseEventArgs.IsForced"/> is taken into account
    /// </para>
    /// </summary>
    TryingToClose,

    /// <summary>
    /// The window or overlay is actually starting to close because the close operation was not cancelled.
    /// The closing and closed event handlers will be invoked, and once they completed, the window or overlay will finally close
    /// </summary>
    Closing,

    /// <summary>
    /// The window or overlay has fully closed and is no longer visible (although the closing animation might still be playing; platform dependent).
    /// </summary>
    Closed
}

public static class OpenStateExtensions {
    /// <summary>
    /// Returns true when this window is still technically "open" and is not in the actual process of closing (although it may shortly after).
    /// </summary>
    public static bool IsOpenOrTryingToClose(this OpenState openState) {
        return openState == OpenState.Open || openState == OpenState.TryingToClose;
    }
}