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
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

namespace PFXToolKitUI.Avalonia.Services.Windowing;

/// <summary>
/// An abstraction around windows. This service allows support for multiple windows and dialogs on single-view application
/// </summary>
public abstract class WindowingSystem {
    /// <summary>
    /// Tries to get the windowing system for the application
    /// </summary>
    /// <param name="system"></param>
    /// <returns></returns>
    public static bool TryGetInstance([NotNullWhen(true)] out WindowingSystem? system) {
        return ApplicationPFX.Instance.ServiceManager.TryGetService(out system);
    }
    
    /// <summary>
    /// Creates a window object using the given content object
    /// </summary>
    /// <param name="content">The content of the window</param>
    /// <param name="isMainWindowable">True to allow the window to be elegible for main window status</param>
    /// <returns>The new window instance</returns>
    public abstract IWindow CreateWindow(WindowingContentControl content, bool isMainWindowable = false);

    /// <summary>
    /// Tries to get the window that is currently focused.
    /// </summary>
    /// <param name="window">The active window</param>
    /// <param name="fallbackToMainWindow">True to use the main window if no focused window is found</param>
    /// <returns>True if an active window was found</returns>
    public abstract bool TryGetActiveWindow([NotNullWhen(true)] out IWindow? window, bool fallbackToMainWindow = true);

    public IWindow? GetActiveWindowOrNull() {
        return this.TryGetActiveWindow(out IWindow? window) ? window : null;
    }
}

public sealed class WindowingSystemDesktop : WindowingSystem {
    private readonly IDesktopService desktop;
    private readonly List<BaseDesktopWindowImpl> mainWindows;

    public WindowingSystemDesktop() {
        this.desktop = ApplicationPFX.Instance.ServiceManager.GetService<IDesktopService>();
        this.mainWindows = new List<BaseDesktopWindowImpl>();
    }

    public override IWindow CreateWindow(WindowingContentControl content, bool isMainWindowable = false) {
        BaseDesktopWindowImpl window = new BaseDesktopWindowImpl(content);
        window.Opened += this.OnWindowOpened;
        window.Closed += this.OnWindowClosed;
        if (isMainWindowable) {
            this.mainWindows.Add(window);
        }
        
        return window;
    }
    
    private void OnWindowOpened(object? sender, EventArgs e) {
        IClassicDesktopStyleApplicationLifetime lifetime = this.desktop.ApplicationLifetime;
        if (lifetime.MainWindow == null) {
            lifetime.MainWindow = (BaseDesktopWindowImpl) sender!;
        }
    }

    private void OnWindowClosed(object? sender, EventArgs e) {
        BaseDesktopWindowImpl window = (BaseDesktopWindowImpl) sender!;
        this.mainWindows.Remove(window);
        
        IClassicDesktopStyleApplicationLifetime lifetime = this.desktop.ApplicationLifetime;
        if (lifetime.MainWindow == window) {
            if (window.Owner is BaseDesktopWindowImpl owner && this.mainWindows.Contains(owner) && owner.IsOpen) {
                lifetime.MainWindow = owner;
            }
            else {
                lifetime.MainWindow = this.mainWindows.FirstOrDefault(x => x.IsOpen);
            }

            lifetime.MainWindow?.Activate();
        }
    }

    public override bool TryGetActiveWindow([NotNullWhen(true)] out IWindow? window, bool fallbackToMainWindow = true) {
        if (this.desktop.TryGetActiveWindow(out Window? wnd, fallbackToMainWindow)) {
            window = (IWindow) wnd;
            return true;
        }

        window = null;
        return false;
    }
}