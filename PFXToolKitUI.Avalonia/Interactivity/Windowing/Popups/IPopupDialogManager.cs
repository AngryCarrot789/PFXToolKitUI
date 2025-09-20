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
using Avalonia;
using Avalonia.Controls;

namespace PFXToolKitUI.Avalonia.Interactivity.Windowing.Popups;

/// <summary>
/// Manages popups.
/// <para>
/// Single-view applications have a single piece of content. Therefore, to support showing
/// windows without native interop, the single view content must support showing controls on-top of
/// the application's main content (which will likely be a page control to present pages).
/// </para>
/// <para>
/// The application's content isn't a <see cref="IPopupDialog"/>; by default, no windows exists.
/// </para>
/// </summary>
public interface IPopupDialogManager {
    /// <summary>
    /// Enumerates all top-level windows, as in, windows that have no <see cref="IPopupDialog.Owner"/> set
    /// </summary>
    IEnumerable<IPopupDialog> TopLevelWindows { get; }

    /// <summary>
    /// Enumerates all visible windows, recursively.
    /// </summary>
    IEnumerable<IPopupDialog> AllWindows {
        get {
            foreach (IPopupDialog window in this.TopLevelWindows) {
                yield return window;
                foreach (IPopupDialog child in window.OwnedWindows) {
                    yield return child;
                }
            }
        }
    }

    /// <summary>
    /// An event fired when a <see cref="IPopupDialog"/> is shown via <see cref="IPopupDialog.ShowAsync"/> or <see cref="IPopupDialog.ShowDialog"/>.
    /// Note -- this is fired after all of the window's <see cref="IPopupDialog.WindowOpened"/> event handlers are invoked
    /// <para>
    /// Handlers of this event can use this to for example attach a <see cref="IPopupDialog.WindowClosed"/> event handler
    /// </para>
    /// </summary>
    event EventHandler<PopupEventArgs>? WindowOpened;

    // /// <summary>
    // /// An event fired when a <see cref="IWindow"/> becomes activated
    // /// </summary>
    // event EventHandler<WindowEventArgs>? WindowActivated;

    /// <summary>
    /// Creates a window from the builder object. Note, this does not actually show the window
    /// </summary>
    /// <param name="builder">The builder</param>
    /// <returns>The newly created window</returns>
    IPopupDialog CreatePopup(PopupDialogBuilder builder);

    /// <summary>
    /// Tries to get the window that is currently activated (as in, has control focus).
    /// </summary>
    /// <param name="window">The active window</param>
    /// <returns>True if an active window was found</returns>
    bool TryGetActiveOrMainWindow([NotNullWhen(true)] out IPopupDialog? window);

    /// <summary>
    /// Returns the window produced by <see cref="TryGetActiveOrMainWindow"/> or returns null
    /// </summary>
    /// <returns>The active window or null</returns>
    IPopupDialog? GetActiveWindowOrNull() => this.TryGetActiveOrMainWindow(out IPopupDialog? window) ? window : null;

    /// <summary>
    /// Tries to get the <see cref="IPopupDialog"/> instance that a specific visual control exists in.
    /// This method always returns true when passing <see cref="IPopupDialog.Control"/> as the visual.
    /// <para>
    /// This is effectively equivalent to <see cref="TopLevel.GetTopLevel"/>, except this method
    /// may work in cases where that method will not
    /// </para>
    /// </summary>
    /// <param name="visual">The visual</param>
    /// <param name="window">The window that the visual exists in</param>
    /// <returns>True if the window was found</returns>
    bool TryGetWindowFromVisual(Visual visual, [NotNullWhen(true)] out IPopupDialog? window);

    /// <summary>
    /// Tries to get the window manager service
    /// </summary>
    /// <param name="windowManager">The manager, or null</param>
    /// <returns>True if a window manager exists</returns>
    public static bool TryGetInstance([NotNullWhen(true)] out IPopupDialogManager? windowManager) {
        return ApplicationPFX.TryGetComponent(out windowManager);
    }

    /// <summary>
    /// Tries to get the <see cref="IPopupDialog"/> instance that a visual exists in. This is effectively
    /// equivalent to <see cref="TopLevel.GetTopLevel"/>, except this method may work in cases where
    /// that method will not
    /// </summary>
    /// <param name="visual">The visual to get the window of</param>
    /// <param name="popup">The window the visual exists in</param>
    /// <returns>
    /// True if the visual existed in a window. False if either no <see cref="IPopupDialogManager"/>
    /// existed or <see cref="TryGetWindowFromVisual"/> returned false
    /// </returns>
    static bool TryGetWindow(Visual visual, [NotNullWhen(true)] out IPopupDialog? popup) {
        if (TryGetInstance(out IPopupDialogManager? manager)) {
            return manager.TryGetWindowFromVisual(visual, out popup);
        }

        popup = null;
        return false;
    }
}