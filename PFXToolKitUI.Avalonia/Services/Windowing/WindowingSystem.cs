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
    /// <returns>True if an active window was found</returns>
    public abstract bool TryGetActiveWindow([NotNullWhen(true)] out IWindow? window);

    public IWindow? GetActiveWindowOrNull() {
        return this.TryGetActiveWindow(out IWindow? window) ? window : null;
    }
}

public sealed class WindowingSystemDesktop : WindowingSystem {
    private readonly IDesktopService desktop;
    private readonly List<DesktopWindowImpl> mainWindowable;
    private readonly List<DesktopWindowImpl> openWindows;
    private DesktopWindowImpl? lastActivated;

    public WindowingSystemDesktop() {
        this.desktop = ApplicationPFX.Instance.ServiceManager.GetService<IDesktopService>();
        this.mainWindowable = new List<DesktopWindowImpl>();
        this.openWindows = new List<DesktopWindowImpl>();
    }

    public override IWindow CreateWindow(WindowingContentControl content, bool isMainWindowable = false) {
        DesktopWindowImpl window = new DesktopWindowImpl(content);
        window.Opened += this.OnWindowOpened;
        window.Closed += this.OnWindowClosed;
        if (isMainWindowable) {
            this.mainWindowable.Add(window);
        }
        
        return window;
    }
    
    private void OnWindowOpened(object? sender, EventArgs e) {
        DesktopWindowImpl window = (DesktopWindowImpl) sender!;
        this.openWindows.Add(window);
        this.lastActivated = window;
        window.Activated += this.OnWindowActivated;
        window.Activated += this.OnWindowDeactivated;
        
        // Try find suitable main window or update main window if current one is not mainwindowable
        IClassicDesktopStyleApplicationLifetime lifetime = this.desktop.ApplicationLifetime;
        if (lifetime.MainWindow is DesktopWindowImpl impl && impl.IsOpen && this.mainWindowable.Contains(impl)) {
            return;
        }

        lifetime.MainWindow = this.mainWindowable.FirstOrDefault(window);
    }

    private void OnWindowClosed(object? sender, EventArgs e) {
        DesktopWindowImpl window = (DesktopWindowImpl) sender!;
        this.mainWindowable.Remove(window);
        if (window == this.lastActivated) {
            this.lastActivated = null;
        }

        window.Activated -= this.OnWindowActivated;
        window.Activated -= this.OnWindowDeactivated;

        // If we closed the main window, then try to find another suitable main window.
        // Otherwise, leave it alone to allow ShutdownMode.OnMainWindowClosed to work
        IClassicDesktopStyleApplicationLifetime lifetime = this.desktop.ApplicationLifetime;
        Window? mainWindow = lifetime.MainWindow;
        if (mainWindow == window || mainWindow == null || !mainWindow.IsLoaded) {
            if (window.Owner is DesktopWindowImpl owner && this.mainWindowable.Contains(owner) && owner.IsOpen) {
                lifetime.MainWindow = owner;
            }
            else if (this.mainWindowable.FirstOrDefault(x => x.IsOpen) is DesktopWindowImpl found) {
                lifetime.MainWindow = found;
            }
            else {
                return;
            }

            lifetime.MainWindow.Activate();
        }
    }
    
    private void OnWindowActivated(object? sender, EventArgs e) {
        this.lastActivated = (DesktopWindowImpl) sender!;
    }
    
    private void OnWindowDeactivated(object? sender, EventArgs e) {
        
    }

    public override bool TryGetActiveWindow([NotNullWhen(true)] out IWindow? window) {
        // Get last activated, or find the first that is activated
        window = this.lastActivated ?? this.openWindows.FirstOrDefault(x => x.IsActive);
        
        if (this.desktop.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime classic) {
            // No windows are activated. App is completely unfocused. So find the main window
            window ??= classic.MainWindow as DesktopWindowImpl;
            if (window != null && !window.IsOpen) {
                window = null;
            }
        }

        // If app has no main window, which ideally should be impossible, find first mainwindowable
        window ??= this.mainWindowable.FirstOrDefault();
        return window != null;
    }
}