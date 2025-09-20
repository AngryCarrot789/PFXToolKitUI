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

namespace PFXToolKitUI.Avalonia.Interactivity.Windowing;

/// <summary>
/// An enum that specifies the opened state of an <see cref="IWindow"/>
/// </summary>
public enum OpenState {
    /// <summary>
    /// The window has not been shown yet. This is the default state for created windows.
    /// </summary>
    NotOpened,
    
    /// <summary>
    /// The window is in the process of opening; <see cref="IWindow.ShowAsync"/> or <see cref="IWindow.ShowDialogAsync"/>
    /// has been invoked, and <see cref="IWindow.WindowOpening"/> handlers will be invoked.
    /// </summary>
    Opening,
    
    /// <summary>
    /// The window is open and visible on screen. <see cref="IWindow.WindowOpened"/> handlers will be invoked.
    /// </summary>
    Open,
    
    /// <summary>
    /// The window has been requested to close; <see cref="IWindow.TryClose"/> and <see cref="IWindow.TryCloseAsync"/>
    /// handlers are invoked to try and cancel the close operation.
    /// <para>
    /// The window will still be open at this point, and the window's <see cref="IWindow.OpenState"/> may
    /// return back to <see cref="Open"/> if the close operation is cancelled
    /// </para>
    /// </summary>
    TryingToClose,
    
    /// <summary>
    /// The window is actually starting to close because the close operation was not cancelled. <see cref="IWindow.WindowClosing"/>
    /// and <see cref="IWindow.WindowClosingAsync"/> handlers will be invoked, and afterwards, the actual window UI will be closed.
    /// </summary>
    Closing,
    
    /// <summary>
    /// The window has fully closed and is no longer visible (although the closing animation might still be playing; platform dependent).
    /// <see cref="IWindow.WindowClosed"/> handlers will be invoked.
    /// </summary>
    Closed
}