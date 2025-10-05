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
using Avalonia.Controls;
using PFXToolKitUI.Avalonia.Interactivity.Windowing;
using PFXToolKitUI.Avalonia.Interactivity.Windowing.Desktop;
using PFXToolKitUI.Avalonia.Interactivity.Windowing.Overlays;
using PFXToolKitUI.CommandSystem;
using PFXToolKitUI.Interactivity.Contexts;
using PFXToolKitUI.Interactivity.Windowing;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.Avalonia.Utils;

public static class WindowContextUtils {
    /// <summary>
    /// Tries to find a window manager at least, and optionally tries to find a useful contextual window
    /// </summary>
    /// <returns>True when a manager is found</returns>
    public static bool TryGetWindowManagerWithUsefulWindow([NotNullWhen(true)] out IWindowManager? manager, out IDesktopWindow? parentWindow, IContextData? context = null, bool canUseActiveOrMainWindow = true) {
        parentWindow = null;
        manager = null;

        if (context != null && (parentWindow = IDesktopWindow.WindowFromContext(context)) != null) {
            manager = parentWindow.WindowManager;
            return true;
        }

        if (parentWindow == null && CommandManager.LocalContextManager.TryGetCurrentContext(out IContextData? localContext)) {
            if ((parentWindow = IDesktopWindow.WindowFromContext(localContext)) != null) {
                manager = parentWindow.WindowManager;
                return true;
            }
        }

        if (parentWindow == null && canUseActiveOrMainWindow) {
            if (!IWindowManager.TryGetInstance(out manager))
                return false;
            manager.TryGetActiveOrMainWindow(out parentWindow);
        }

        return (manager != null);
    }

    public static bool TryGetTopLevel(ITopLevel srcTopLevel, [NotNullWhen(true)] out TopLevel? topLevel) {
        if (srcTopLevel is IDesktopWindow desktop)
            return desktop.TryGetTopLevel(out topLevel);
        if (srcTopLevel is IBaseOverlayOrContentHost overlayOrHost)
            return overlayOrHost.OverlayManager.TryGetTopLevel(out topLevel);

        topLevel = null;
        return false;
    }

    public static IWindowBase? CreateWindow(ITopLevel parentTopLevel, Func<IDesktopWindow, IDesktopWindow> windowFactory, Func<IOverlayWindowManager, IOverlayWindow?, IOverlayWindow> overlayFactory) {
        if (parentTopLevel is IDesktopWindow window1)
            return windowFactory(window1);
        if (parentTopLevel is IBaseOverlayOrContentHost overlayOrHost)
            return overlayFactory(overlayOrHost.OverlayManager, overlayOrHost as IOverlayWindow);
        return null;
    }

    public static async Task<Optional<T>> UseWindowAsync<T>(ITopLevel parentTopLevel, Func<IDesktopWindow, Task<T>> windowFactory, Func<IOverlayWindowManager, IOverlayWindow?, Task<T>> overlayFactory) {
        if (parentTopLevel is IDesktopWindow window)
            return await windowFactory(window);
        if (parentTopLevel is IBaseOverlayOrContentHost overlayOrHost)
            return await overlayFactory(overlayOrHost.OverlayManager, overlayOrHost as IOverlayWindow);
        return default;
    }
}