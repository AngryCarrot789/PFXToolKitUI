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
using Avalonia;
using Avalonia.Controls;
using PFXToolKitUI.Interactivity.Windowing;

namespace PFXToolKitUI.Avalonia.Interactivity.Windowing.Desktop;

/// <summary>
/// Manages <see cref="IDesktopWindow"/> instances and facilitates creating windows
/// </summary>
public interface IWindowManager : ITopLevelManager {
    /// <summary>
    /// Enumerates all top-level windows, as in, windows that have no <see cref="IDesktopWindow.Owner"/> set
    /// </summary>
    IEnumerable<IDesktopWindow> TopLevelWindows { get; }

    /// <summary>
    /// Enumerates all visible windows, recursively.
    /// </summary>
    IEnumerable<IDesktopWindow> AllWindows {
        get {
            foreach (IDesktopWindow window in this.TopLevelWindows) {
                yield return window;
                foreach (IDesktopWindow child in window.OwnedWindows) {
                    yield return child;
                }
            }
        }
    }

    /// <summary>
    /// An event fired when a <see cref="IDesktopWindow"/> is shown via <see cref="IDesktopWindow.ShowAsync"/> or <see cref="IDesktopWindow.ShowDialogAsync"/>.
    /// Note -- this is fired after all of the window's <see cref="IDesktopWindow.Opened"/> event handlers are invoked
    /// <para>
    /// Handlers of this event can use this to for example attach a <see cref="IDesktopWindow.Closed"/> event handler
    /// </para>
    /// </summary>
    event EventHandler<WindowEventArgs>? WindowOpened;

    // /// <summary>
    // /// An event fired when a <see cref="IWindow"/> becomes activated
    // /// </summary>
    // event EventHandler<WindowEventArgs>? WindowActivated;

    /// <summary>
    /// Creates a window from the builder object. Note, this does not actually show the window
    /// </summary>
    /// <param name="builder">The builder</param>
    /// <returns>The newly created window</returns>
    IDesktopWindow CreateWindow(WindowBuilder builder);

    /// <summary>
    /// Tries to get the window that is currently activated (as in, has control focus).
    /// </summary>
    /// <param name="window">The active or main window</param>
    /// <returns>True if an active window was found</returns>
    bool TryGetActiveOrMainWindow([NotNullWhen(true)] out IDesktopWindow? window);

    bool ITopLevelManager.TryGetActiveOrMainTopLevel([NotNullWhen(true)] out ITopLevel? topLevel) {
        return (topLevel = this.GetActiveOrMainWindow()) != null;
    }

    /// <summary>
    /// Returns the window produced by <see cref="TryGetActiveOrMainWindow"/> or returns null
    /// </summary>
    /// <returns>The active window or null</returns>
    IDesktopWindow? GetActiveOrMainWindow() => this.TryGetActiveOrMainWindow(out IDesktopWindow? window) ? window : null;
    
    /// <summary>
    /// Tries to get the <see cref="IDesktopWindow"/> instance that a specific visual control exists in.
    /// This method always returns true when passing <see cref="IDesktopWindow.Control"/> as the visual.
    /// <para>
    /// This is effectively equivalent to <see cref="TopLevel.GetTopLevel"/>, except this method
    /// may work in cases where that method will not
    /// </para>
    /// </summary>
    /// <param name="visual">The visual</param>
    /// <param name="window">The window that the visual exists in</param>
    /// <returns>True if the window was found</returns>
    bool TryGetWindowFromVisual(Visual visual, [NotNullWhen(true)] out IDesktopWindow? window);

    /// <summary>
    /// Tries to get the window manager service
    /// </summary>
    /// <param name="windowManager">The manager, or null</param>
    /// <returns>True if a window manager exists</returns>
    public static bool TryGetInstance([NotNullWhen(true)] out IWindowManager? windowManager) {
        return ApplicationPFX.TryGetComponent(out windowManager);
    }

    /// <summary>
    /// Tries to get the <see cref="IDesktopWindow"/> instance that a visual exists in. This is effectively
    /// equivalent to <see cref="TopLevel.GetTopLevel"/>, except this method may work in cases where
    /// that method will not
    /// </summary>
    /// <param name="visual">The visual to get the window of</param>
    /// <param name="window">The window the visual exists in</param>
    /// <returns>
    /// True if the visual existed in a window. False if either no <see cref="IWindowManager"/>
    /// existed or <see cref="TryGetWindowFromVisual"/> returned false
    /// </returns>
    static bool TryGetWindow(Visual visual, [NotNullWhen(true)] out IDesktopWindow? window) {
        if (!TryGetInstance(out IWindowManager? manager)) {
            window = null;
            return false;
        }

        return manager.TryGetWindowFromVisual(visual, out window);
    }
}