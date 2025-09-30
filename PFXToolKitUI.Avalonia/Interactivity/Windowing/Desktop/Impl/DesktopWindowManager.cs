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

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform;
using PFXToolKitUI.Avalonia.Services;
using PFXToolKitUI.Logging;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.Avalonia.Interactivity.Windowing.Desktop.Impl;

/// <summary>
/// An implementation of <see cref="IWindowManager"/> that uses avalonia <see cref="global::Avalonia.Controls.Window"/> objects
/// </summary>
public sealed class DesktopWindowManager : IWindowManager {
    private readonly IDesktopService desktop;
    private readonly List<DesktopWindowImpl> topLevelWindows;
    private readonly List<DesktopWindowImpl> mainWindows;
    private readonly List<DesktopWindowImpl> allWindows;
    private DesktopWindowImpl? lastActivated;

    // Default Icon stuff
    private readonly Uri? defaultWindowIconUri;
    private WindowIcon? defaultWindowIcon;
    private bool failedToLoadDefaultIcon;

    private DesktopNativeWindow? DesktopMainWindow {
        get => (DesktopNativeWindow?) this.desktop.ApplicationLifetime.MainWindow;
        set => this.desktop.ApplicationLifetime.MainWindow = value;
    }

    public IEnumerable<IDesktopWindow> TopLevelWindows { get; }

    public IEnumerable<IDesktopWindow> AllWindows { get; }

    public event EventHandler<WindowEventArgs>? WindowOpened;

    public DesktopWindowManager(Uri? defaultWindowIconUri) {
        this.desktop = ApplicationPFX.GetComponent<IDesktopService>();
        this.topLevelWindows = new List<DesktopWindowImpl>();
        this.mainWindows = new List<DesktopWindowImpl>();
        this.allWindows = new List<DesktopWindowImpl>();
        this.TopLevelWindows = this.topLevelWindows.AsReadOnly();
        this.AllWindows = this.allWindows.AsReadOnly();
        this.defaultWindowIconUri = defaultWindowIconUri;
    }

    public IDesktopWindow CreateWindow(WindowBuilder builder) {
        IDesktopWindow? parent = builder.Parent;
        if (parent != null && !(parent is DesktopWindowImpl)) {
            throw new InvalidOperationException("Attempt to create window using a non-desktop parent window");
        }

        return new DesktopWindowImpl(this, parent as DesktopWindowImpl, builder);
    }

    public bool TryGetActiveOrMainWindow([NotNullWhen(true)] out IDesktopWindow? window) {
        // Get last activated, or find the first that is activated
        if ((window = this.lastActivated) != null && window.OpenState == OpenState.Open)
            return true;

        // lastActivated not set somehow, so find an activated window
        if ((window = this.mainWindows.FirstOrDefault(x => x.OpenState == OpenState.Open && x.IsActivated)) != null)
            return true;
        if ((window = this.allWindows.LastOrDefault(x => x.OpenState == OpenState.Open && x.IsActivated)) != null)
            return true;

        // bad case of the software bugs. Find an open window
        if ((window = this.mainWindows.FirstOrDefault(x => x.OpenState == OpenState.Open)) != null)
            return true;
        return (window = this.allWindows.LastOrDefault(x => x.OpenState == OpenState.Open)) != null;
    }

    public bool TryGetWindowFromVisual(Visual visual, [NotNullWhen(true)] out IDesktopWindow? window) {
        if (visual is DesktopNativeWindow nativeWindow) {
            window = nativeWindow.Window;
            return true;
        }

        TopLevel? topLevel = TopLevel.GetTopLevel(visual);
        if (topLevel is DesktopNativeWindow nativeWindow2) {
            window = nativeWindow2.Window;
            return true;
        }

        window = null;
        return false;
    }

    /// <summary>
    /// Gets the default icon. Note this may lazily load the icon so do not call from a method that must return quickly
    /// </summary>
    /// <returns>
    /// The icon, or null if the uri provided by to the <see cref="DesktopWindowManager"/> could not
    /// be found, or an error was encountered while loading the icon
    /// </returns>
    public WindowIcon? GetDefaultWindowIcon() {
        if (this.defaultWindowIcon != null || this.failedToLoadDefaultIcon || this.defaultWindowIconUri == null) {
            return this.defaultWindowIcon;
        }

        try {
            using Stream stream = AssetLoader.Open(this.defaultWindowIconUri);
            return this.defaultWindowIcon = new WindowIcon(stream);
        }
        catch (Exception e) {
            this.failedToLoadDefaultIcon = true;
            AppLogger.Instance.WriteLine("Failed to load default icon: " + e.GetToString());
            return null;
        }
    }

    private void OnWindowActivated(object? sender, EventArgs e) {
        DesktopWindowImpl window = ((DesktopNativeWindow) sender!).Window;
        this.lastActivated = window.OpenState == OpenState.Open || window.OpenState == OpenState.TryingToClose ? window : null;
    }

    #region Internal Show/Close Handling

    // Invoked before WindowOpening
    internal void OnWindowOpening(DesktopWindowImpl window) {
        Debug.Assert(window.myManager == this);
        if (window.parentWindow != null)
            window.parentWindow.myVisibleChildWindows.Add(window);
        else
            this.topLevelWindows.Add(window);

        this.allWindows.Add(window);
        if (window.IsMainWindow) {
            this.mainWindows.Add(window);
            this.DesktopMainWindow = window.myNativeWindow;
        }

        window.myNativeWindow.Activated += this.OnWindowActivated;
    }

    // Invoked after WindowOpened
    internal void OnWindowOpened(DesktopWindowImpl window) {
        Debug.Assert(window.myManager == this);
        this.WindowOpened?.Invoke(this, new WindowEventArgs(window));
    }

    // Invoked before WindowClosed
    internal void OnWindowClosed(DesktopWindowImpl window) {
        Debug.Assert(window.myManager == this);
        IClassicDesktopStyleApplicationLifetime lifetime = this.desktop.ApplicationLifetime;
        if (window.IsMainWindow) {
            int index = this.mainWindows.IndexOf(window);
            if (index == -1) {
                Debug.Fail("Failed to remove window from main window list that was a MainWindow");
            }
            else {
                this.mainWindows.RemoveAt(index);
                if (index == this.mainWindows.Count /* we removed the window at the end of the list */) {
                    lifetime.MainWindow = this.mainWindows.Count > 0 ? this.mainWindows[this.mainWindows.Count - 1].myNativeWindow : null;
                }
            }
        }

        window.myNativeWindow.Activated -= this.OnWindowActivated;

        bool debugRemoved;
        if (window.parentWindow != null) {
            debugRemoved = window.parentWindow.myVisibleChildWindows.Remove(window);
            Debug.Assert(debugRemoved, "Failed to remove self from parent's owners list");
        }
        else {
            debugRemoved = this.topLevelWindows.Remove(window);
            Debug.Assert(debugRemoved, "Failed to remove self from window manager's top-level window list");
        }

        debugRemoved = this.allWindows.Remove(window);
        Debug.Assert(debugRemoved, "Failed to remove self from window manager's window list");

        if (this.lastActivated == window)
            this.lastActivated = null;

        switch (lifetime.ShutdownMode) {
            case ShutdownMode.OnLastWindowClose when this.allWindows.Count < 1:
            case ShutdownMode.OnMainWindowClose when this.mainWindows.Count < 1: {
                // We need to post the shutdown request, because due to how the avalonia
                // application handles stuff, the window we just processed isn't removed from
                // the app's internal window list, so it will try to close it, which results
                // in reentrancy (and an Exception because IsClosed is true).
                ApplicationPFX.Instance.Dispatcher.Post(ApplicationPFX.Instance.Shutdown);
                break;
            }
        }
    }

    #endregion
}