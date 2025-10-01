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
using PFXToolKitUI.Avalonia.Interactivity.Windowing.Overlays.Impl;
using PFXToolKitUI.Avalonia.Utils;
using PFXToolKitUI.Interactivity.Windowing;

namespace PFXToolKitUI.Avalonia.Interactivity.Windowing.Overlays;

/// <summary>
/// Manages popups.
/// <para>
/// Single-view applications have a single piece of content. Therefore, to support showing
/// windows without native interop, the single view content must support showing controls on-top of
/// the application's main content (which will likely be a page control to present pages).
/// </para>
/// <para>
/// The application's content isn't a <see cref="IOverlayWindow"/>; by default, no popups exists.
/// </para>
/// </summary>
public interface IOverlayWindowManager : ITopLevelManager {
    /// <summary>
    /// Enumerates all top-level windows, as in, windows that have no <see cref="IOverlayWindow.OwnerPopup"/> set
    /// </summary>
    IEnumerable<IOverlayWindow> TopLevelWindows { get; }

    /// <summary>
    /// Enumerates all visible windows, recursively.
    /// </summary>
    IEnumerable<IOverlayWindow> AllWindows {
        get {
            foreach (IOverlayWindow window in this.TopLevelWindows) {
                yield return window;
                foreach (IOverlayWindow child in window.OwnedPopups) {
                    yield return child;
                }
            }
        }
    }
    
    /// <summary>
    /// Gets the root host of the overlay windows. This is usually the control which has its
    /// own <see cref="IOverlayWindowManager"/>, i.e. the content of a single view application
    /// </summary>
    public IOverlayWindowHost ContentHost { get; }

    /// <summary>
    /// An event fired when a <see cref="IOverlayWindow"/> is shown via <see cref="IOverlayWindow.ShowAsync"/> or <see cref="IOverlayWindow.ShowDialogAsync"/>.
    /// Note -- this is fired after all of the window's <see cref="IOverlayWindow.Opened"/> event handlers are invoked
    /// <para>
    /// Handlers of this event can use this to for example attach a <see cref="IOverlayWindow.Closed"/> event handler
    /// </para>
    /// </summary>
    event EventHandler<OverlayWindowEventArgs>? OverlayOpened;

    /// <summary>
    /// Creates a window from the builder object. Note, this does not actually show the window
    /// </summary>
    /// <param name="builder">The builder</param>
    /// <returns>The newly created window</returns>
    IOverlayWindow CreateWindow(OverlayWindowBuilder builder);

    /// <summary>
    /// Returns the window produced by <see cref="TryGetActiveOverlay"/> or returns null
    /// </summary>
    /// <returns>The active window or null</returns>
    IOverlayWindow? GetActiveOverlay() {
        return this.TopLevelWindows.LastOrDefault(x => x.OpenState.IsOpenOrTryingToClose());
    }

    bool ITopLevelManager.TryGetActiveOrMainTopLevel([NotNullWhen(true)] out ITopLevel? topLevel) {
        if ((topLevel = this.GetActiveOverlay()) == null)
            topLevel = this.ContentHost;
        return true;
    }

    /// <summary>
    /// Tries to get the top level of this overlay window manager
    /// </summary>
    bool TryGetTopLevel([NotNullWhen(true)] out TopLevel? topLevel);

    /// <summary>
    /// Tries to get the <see cref="IOverlayWindow"/> instance that a specific visual control exists in.
    /// This method always returns true when passing <see cref="IOverlayWindow.Control"/> as the visual.
    /// <para>
    /// This is effectively equivalent to <see cref="TopLevel.GetTopLevel"/>, except this method
    /// may work in cases where that method will not
    /// </para>
    /// </summary>
    /// <param name="visual">The visual</param>
    /// <param name="overlay">The window that the visual exists in</param>
    /// <returns>True if the window was found</returns>
    static bool TryGetOverlayFromVisual(Visual visual, [NotNullWhen(true)] out IOverlayWindow? overlay) {
        OverlayControl? d = VisualTreeUtils.FindLogicalParent<OverlayControl>(visual, includeSelf: true);
        return (overlay = d?.OverlayWindow) != null;
    }

    /// <summary>
    /// Tries to get the <see cref="IBaseOverlayOrContentHost"/> instance that a specific visual control exists in.
    /// </summary>
    /// <param name="visual">The visual</param>
    /// <param name="overlayOrHost">The overlay or content host that the visual exists in</param>
    /// <returns>True if the overlay or host was found</returns>
    static bool TryGetOverlayOrContentHostFromVisual(Visual visual, [NotNullWhen(true)] out IBaseOverlayOrContentHost? overlayOrHost) {
        if (TryGetOverlayFromVisual(visual, out IOverlayWindow? overlay)) {
            overlayOrHost = overlay;
            return true;
        }
        
        OverlayContentHostRoot? d = VisualTreeUtils.FindLogicalParent<OverlayContentHostRoot>(visual, includeSelf: true);
        return (overlayOrHost = d?.Manager.ContentHost) != null;
    }
}