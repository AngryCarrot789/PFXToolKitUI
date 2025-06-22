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
using Avalonia.Platform;
using PFXToolKitUI.Logging;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.Avalonia.Services.Windowing;

/// <summary>
/// An abstraction around windows. This service allows support for multiple windows and dialogs on single-view application
/// </summary>
public abstract class WindowingSystem {
    /// <summary>
    /// Gets the current main window
    /// </summary>
    public abstract DesktopWindow? MainWindow { get; }
    
    /// <summary>
    /// Gets or sets the shutdown behaviour of the application. Default value is <see cref="global::Avalonia.Controls.ShutdownMode.OnLastWindowClose"/>
    /// </summary>
    public abstract ShutdownMode ShutdownMode { get; set; }

    /// <summary>
    /// Tries to get the windowing system for the application
    /// </summary>
    /// <param name="system"></param>
    /// <returns></returns>
    public static bool TryGetInstance([NotNullWhen(true)] out WindowingSystem? system) {
        return ApplicationPFX.Instance.ServiceManager.TryGetService(out system);
    }

    /// <summary>
    /// Registers the window with this windowing system. The window cannot be open.
    /// It is automatically unregistered when the window closes
    /// </summary>
    /// <param name="window">The window</param>
    /// <param name="isMainWindowable">True to allow the window to be elegible for main window status</param>
    /// <returns>The parameter, for chain calling</returns>
    public abstract DesktopWindow Register(DesktopWindow window, bool isMainWindowable = false);

    /// <summary>
    /// Tries to get the window that is currently focused.
    /// </summary>
    /// <param name="window">The active window</param>
    /// <returns>True if an active window was found</returns>
    public abstract bool TryGetActiveWindow([NotNullWhen(true)] out DesktopWindow? window);

    public DesktopWindow? GetActiveWindowOrNull() {
        return this.TryGetActiveWindow(out DesktopWindow? window) ? window : null;
    }
}

public sealed class WindowingSystemImpl : WindowingSystem {
    private readonly IDesktopService desktop;
    private readonly List<DesktopWindow> openWindows;
    private readonly Uri? defaultIconUri;
    private DesktopWindow? lastActivated;
    private WindowIcon? defaultIcon;
    private bool failedToLoadDefaultIcon;

    public override DesktopWindow? MainWindow => this.desktop.ApplicationLifetime.MainWindow as DesktopWindow;

    public override ShutdownMode ShutdownMode {
        get => this.desktop.ApplicationLifetime.ShutdownMode;
        set => this.desktop.ApplicationLifetime.ShutdownMode = value;
    }

    public WindowingSystemImpl(Uri? defaultIconUri = null) {
        this.desktop = ApplicationPFX.Instance.ServiceManager.GetService<IDesktopService>();
        this.openWindows = new List<DesktopWindow>();
        this.defaultIconUri = defaultIconUri;
    }

    private WindowIcon? GetDefaultWindowIcon() {
        if (this.defaultIcon != null || this.failedToLoadDefaultIcon || this.defaultIconUri == null) {
            return this.defaultIcon;
        }

        try {
            using Stream stream = AssetLoader.Open(this.defaultIconUri);
            return this.defaultIcon = new WindowIcon(stream);
        }
        catch (Exception e) {
            // ignored
            this.failedToLoadDefaultIcon = true;
            AppLogger.Instance.WriteLine("Failed to load default icon: " + e.GetToString());
            return null;
        }
    }

    private void OnWindowOpened(object? sender, EventArgs e) {
        DesktopWindow window = (DesktopWindow) sender!;
        this.openWindows.Add(window);
        this.lastActivated = window;
        window.Activated += this.OnWindowActivated;
        window.Deactivated += this.OnWindowDeactivated;
    }

    private void OnWindowClosed(object? sender, EventArgs e) {
        DesktopWindow window = (DesktopWindow) sender!;
        bool isMainWindow = this.MainWindow == window;
        IClassicDesktopStyleApplicationLifetime lifetime = this.desktop.ApplicationLifetime;
        if (isMainWindow) {
            lifetime.MainWindow = null;
        }

        if (window == this.lastActivated) {
            this.lastActivated = null;
        }

        window.Activated -= this.OnWindowActivated;
        window.Deactivated -= this.OnWindowDeactivated;

        ShutdownMode mode = lifetime.ShutdownMode;
        if (isMainWindow && mode == ShutdownMode.OnMainWindowClose) {
            ApplicationPFX.Instance.Shutdown();
        }
        else if (mode == ShutdownMode.OnLastWindowClose && lifetime.Windows.Count < 1) {
            ApplicationPFX.Instance.Shutdown();
        }
    }

    private void OnWindowActivated(object? sender, EventArgs e) {
        this.lastActivated = (DesktopWindow) sender!;
    }

    private void OnWindowDeactivated(object? sender, EventArgs e) {
    }

    public override DesktopWindow Register(DesktopWindow window, bool isMainWindowable = false) {
        window.Opened += this.OnWindowOpened;
        window.Closed += this.OnWindowClosed;
        if (isMainWindowable) {
            DesktopWindow? myMainWindow = this.MainWindow;
            if (myMainWindow != null && myMainWindow.IsOpen)
                throw new InvalidOperationException("Current main window is still open. Cannot show a new window as a main window");
            this.desktop.ApplicationLifetime.MainWindow = window;
        }

        window.Icon ??= this.GetDefaultWindowIcon();
        return window;
    }

    public override bool TryGetActiveWindow([NotNullWhen(true)] out DesktopWindow? window) {
        // Get last activated, or find the first that is activated
        window = this.lastActivated ?? this.openWindows.FirstOrDefault(x => x.IsActive);

        // No windows are activated. App is completely unfocused. So find the main window
        window ??= this.MainWindow;
        if (window != null && !window.IsOpen) {
            window = null;
        }

        return window != null;
    }
}