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
using Avalonia.Media;
using PFXToolKitUI.Icons;
using PFXToolKitUI.Interactivity.Contexts;
using PFXToolKitUI.Themes;

namespace PFXToolKitUI.Avalonia.Interactivity.Windowing.Desktop;

public delegate void WindowEventHandler(IDesktopWindow sender, EventArgs e);

public delegate void WindowEventHandler<in TEventArgs>(IDesktopWindow sender, TEventArgs e) where TEventArgs : EventArgs;

public delegate Task AsyncWindowEventHandler(IDesktopWindow sender, EventArgs e);

public delegate Task AsyncWindowEventHandler<in TEventArgs>(IDesktopWindow sender, TEventArgs e) where TEventArgs : EventArgs;

public delegate void WindowIconChangedEventHandler(IDesktopWindow window, WindowIcon? oldValue, WindowIcon? newValue);

public delegate void WindowTitleBarIconChangedEventHandler(IDesktopWindow window, Icon? oldValue, Icon? newValue);

public delegate void WindowTitleBarCaptionChangedEventHandler(IDesktopWindow window, string? oldValue, string? newValue);

public delegate void WindowTitleBarBrushChangedEventHandler(IDesktopWindow window, IColourBrush? oldValue, IColourBrush? newValue);

public delegate void WindowBorderBrushChangedEventHandler(IDesktopWindow window, IColourBrush? oldValue, IColourBrush? newValue);

public delegate void WindowTitleBarTextAlignmentChangedEventHandler(IDesktopWindow window, TextAlignment oldValue, TextAlignment newValue);

/// <summary>
/// A window that can be positioned and sized and dragged around by the user.
/// Supports any number of owned windows and can be parented to any other window.
/// </summary>
public interface IDesktopWindow : IWindowBase {
    /// <summary>
    /// Gets the window manager associated with this window
    /// </summary>
    IWindowManager WindowManager { get; }

    /// <summary>
    /// Gets the parent window. This is usually only set for dialogs, however, it might be
    /// set for windows that want to stay on-top of another window without being a dialog.
    /// </summary>
    IDesktopWindow? Owner { get; }

    /// <summary>
    /// Enumerates the child windows of this window. Windows are added to this before
    /// <see cref="Opening"/> is fired, and removed before <see cref="Closed"/> is fired 
    /// </summary>
    IEnumerable<IDesktopWindow> OwnedWindows { get; }

    /// <summary>
    /// Gets whether this window is a main window. When shown, it will replace the main
    /// window status of the previous main window until this window closes.
    /// </summary>
    bool IsMainWindow { get; }

    /// <summary>
    /// Returns whether this window was shown as a dialog. Note, this may return true even when closing or closed
    /// </summary>
    bool IsDialog { get; }

    /// <summary>
    /// Gets whether this window is the application-wide active window
    /// </summary>
    bool IsActivated { get; }

    /// <summary>
    /// Gets or sets the window icon. This is shown in the task bar on supported operating systems,
    /// and is the default icon for the title bar (if shown and the icon isn't set as hidden).
    /// <para>
    /// See <see cref="WindowBuilder.Icon"/> as for why this cannot be set as icons from the <see cref="IconManager"/>
    /// </para>
    /// </summary>
    WindowIcon? Icon { get; set; }

    /// <summary>
    /// Gets or sets the icon shown in the title bar
    /// </summary>
    Icon? TitleBarIcon { get; set; }

    /// <summary>
    /// Gets or sets if the title bar should be visible. Note, when invisible, the window cannot be dragged around.
    /// Resizing may still work, unless our <see cref="SizingInfo"/>'s <see cref="WindowSizingInfo.CanResize"/> property is false
    /// </summary>
    bool IsTitleBarVisible { get; set; }

    /// <summary>
    /// Gets or sets the title bar text, aka the caption
    /// </summary>
    string? Title { get; set; }

    /// <summary>
    /// Gets or sets the brush that colours the title bar background
    /// </summary>
    IColourBrush? TitleBarBrush { get; set; }

    /// <summary>
    /// Gets or sets the brush that colours the border of the window. Setting this to null will disable the border
    /// </summary>
    IColourBrush? BorderBrush { get; set; }

    /// <summary>
    /// Gets or sets the text alignment for the title bar text
    /// </summary>
    TextAlignment TitleBarTextAlignment { get; set; }

    /// <summary>
    /// Gets the object that allows for changing the window size and constraints
    /// </summary>
    WindowSizingInfo SizingInfo { get; }

    /// <summary>
    /// Gets the size of the window
    /// </summary>
    Size ActualSize { get; }

    /// <summary>
    /// Gets or sets the position of the window
    /// </summary>
    PixelPoint Position { get; set; }

    /// <summary>
    /// An event fired when the window is in the process of opening but has not been shown on screen yet.
    /// </summary>
    event WindowEventHandler? Opening;

    /// <summary>
    /// An event fired when the window is fully opening.
    /// </summary>
    event WindowEventHandler? Opened;

    /// <summary>
    /// An event fired when the window is requested to close.
    /// <para>
    /// This and <see cref="TryCloseAsync"/> are the only times that cancelling window closure is possible
    /// </para>
    /// </summary>
    event WindowEventHandler<WindowCancelCloseEventArgs>? TryClose;

    /// <summary>
    /// An asynchronous event fired when the window is requested to close. The handlers are invoked
    /// in their own tasks once all handlers of <see cref="TryClose"/> are invoked.
    /// <para>
    /// This and <see cref="TryClose"/> are the only times that cancelling window closure is possible
    /// </para>
    /// </summary>
    event AsyncWindowEventHandler<WindowCancelCloseEventArgs>? TryCloseAsync;

    /// <summary>
    /// An event fired when the window is actually about to close.
    /// </summary>
    event WindowEventHandler<WindowCloseEventArgs>? Closing;

    /// <summary>
    /// An event fired when the window is actually about to close.
    /// The handlers are invoked in their own tasks once all handlers
    /// of <see cref="Closing"/> are invoked.
    /// </summary>
    event AsyncWindowEventHandler<WindowCloseEventArgs>? ClosingAsync;

    /// <summary>
    /// An event fired when the window is fully closed.
    /// <para>
    /// This is fired before the task returned by <see cref="RequestCloseAsync"/> or <see cref="WaitForClosedAsync"/> becomes completed
    /// </para>
    /// </summary>
    event WindowEventHandler<WindowCloseEventArgs>? Closed;

    event WindowIconChangedEventHandler? IconChanged;
    event WindowTitleBarIconChangedEventHandler? TitleBarIconChanged;
    event WindowEventHandler? IsTitleBarVisibleChanged;
    event WindowTitleBarCaptionChangedEventHandler? TitleChanged;
    event WindowTitleBarBrushChangedEventHandler? TitleBarBrushChanged;
    event WindowBorderBrushChangedEventHandler? BorderBrushChanged;
    event WindowTitleBarTextAlignmentChangedEventHandler? TitleBarTextAlignmentChanged;

    /// <summary>
    /// Force-activates this window, making <see cref="IsActivated"/> and it becomes the focused control
    /// </summary>
    void Activate();

    /// <summary>
    /// Gets the window from the context data, or null, if there is no window available
    /// </summary>
    /// <param name="context">The context</param>
    /// <returns>The window</returns>
    static IDesktopWindow? WindowFromContext(IContextData context) {
        return TopLevelDataKey.GetContext(context) as IDesktopWindow;
    }

    /// <summary>
    /// Tries to get the window from the context data.
    /// </summary>
    /// <param name="context">The context</param>
    /// <param name="window">The window</param>
    /// <returns>True if a window was available</returns>
    static bool TryGetWindowFromContext(IContextData context, [NotNullWhen(true)] out IDesktopWindow? window) {
        return (window = WindowFromContext(context)) != null;
    }

    /// <summary>
    /// Tries to get the window from the visual, or returns null, if the visual isn't in a <see cref="IDesktopWindow"/>
    /// </summary>
    /// <param name="visual">The visual to get the window of</param>
    /// <returns>The window, or null</returns>
    static IDesktopWindow? FromVisual(Visual visual) {
        return IWindowManager.TryGetWindow(visual, out IDesktopWindow? window) ? window : null;
    }

    /// <summary>
    /// Tries to get the window from the visual
    /// </summary>
    /// <param name="visual">The visual to get the window of</param>
    /// <param name="window">The window the visual exists in</param>
    /// <returns>
    /// True if the visual existed in a window. False if either no <see cref="IWindowManager"/>
    /// existed or <see cref="TryGetWindowFromVisual"/> returned false
    /// </returns>
    static bool TryGetFromVisual(Visual visual, [NotNullWhen(true)] out IDesktopWindow? window) {
        return IWindowManager.TryGetWindow(visual, out window);
    }
}

/// <summary>
/// Event args for when a <see cref="IDesktopWindow"/> is shown
/// </summary>
public class WindowEventArgs(IDesktopWindow window) : EventArgs {
    public IDesktopWindow Window { get; } = window;
}

/// <summary>
/// Event args for when a window is closing and when closed
/// </summary>
public class WindowCloseEventArgs(IDesktopWindow window, WindowCloseReason reason, bool isFromCode) : WindowEventArgs(window) {
    /// <summary>
    /// Gets the reason for why the window is closing 
    /// </summary>
    public WindowCloseReason Reason { get; } = reason;

    /// <summary>
    /// Gets whether the closing operation was caused by user code (i.e. called from <see cref="IDesktopWindow.RequestCloseAsync"/>)
    /// </summary>
    public bool IsFromCode { get; } = isFromCode;
}

/// <summary>
/// Event args for when trying to close a window
/// </summary>
public class WindowCancelCloseEventArgs(IDesktopWindow window, WindowCloseReason reason, bool isFromCode) : WindowCloseEventArgs(window, reason, isFromCode) {
    // We don't allow de-cancelling the close because it could lead to unexpected behaviour and issues
    private volatile bool isCancelled;

    /// <summary>
    /// Gets whether the closing of the window should be cancelled
    /// </summary>
    public bool IsCancelled => this.isCancelled;

    /// <summary>
    /// Sets <see cref="IsCancelled"/> to true
    /// </summary>
    public void SetCancelled() => this.isCancelled = true;
}