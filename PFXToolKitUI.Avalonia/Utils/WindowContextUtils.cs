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
using PFXToolKitUI.Avalonia.Interactivity.Windowing;
using PFXToolKitUI.CommandSystem;
using PFXToolKitUI.Interactivity.Contexts;

namespace PFXToolKitUI.Avalonia.Utils;

public static class WindowContextUtils {
    /// <summary>
    /// Tries to get a useful window from the current command context as well as the optional alternate context.
    /// Failing those, we attempt to access the application's currently active window, or worst case, one of the main windows
    /// </summary>
    public static IWindow? GetUsefulWindow(IContextData? alternateContext = null, bool canUseActiveOrMainWindow = true) {
        IWindow? parentWindow = null;
        if (CommandManager.LocalContextManager.TryGetGlobalContext(out IContextData? context)) {
            parentWindow = IWindow.WindowFromContext(context);
        }

        if (parentWindow == null && alternateContext != null) {
            parentWindow = IWindow.WindowFromContext(alternateContext);
        }

        if (parentWindow == null && canUseActiveOrMainWindow) {
            if (!IWindowManager.TryGetInstance(out IWindowManager? manager))
                return null;
            manager.TryGetActiveOrMainWindow(out parentWindow);
        }

        return parentWindow;
    }
    
    /// <summary>
    /// Tries to find a window manager at least, and optionally tries to find a useful contextual window
    /// </summary>
    /// <returns>True when a manager is found</returns>
    public static bool TryGetWindowManagerWithUsefulWindow([NotNullWhen(true)] out IWindowManager? manager, out IWindow? parentWindow, IContextData? alternateContext = null, bool canUseActiveOrMainWindow = true) {
        parentWindow = null;
        manager = null;
        
        if (CommandManager.LocalContextManager.TryGetGlobalContext(out IContextData? context)) {
            if ((parentWindow = IWindow.WindowFromContext(context)) != null) {
                manager = parentWindow.WindowManager;
                return true;
            }
        }

        if (parentWindow == null && alternateContext != null) {
            if ((parentWindow = IWindow.WindowFromContext(alternateContext)) != null) {
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
}