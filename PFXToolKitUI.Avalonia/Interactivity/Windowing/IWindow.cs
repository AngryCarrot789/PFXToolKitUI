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
using Avalonia.Interactivity;
using Avalonia.Media;
using PFXToolKitUI.AdvancedMenuService;
using PFXToolKitUI.Avalonia.Interactivity.Contexts;
using PFXToolKitUI.Icons;
using PFXToolKitUI.Interactivity.Contexts;
using PFXToolKitUI.Interactivity.Windowing;
using PFXToolKitUI.Themes;

namespace PFXToolKitUI.Avalonia.Interactivity.Windowing;

public delegate void WindowEventHandler(IWindow sender, EventArgs e);

public delegate void WindowEventHandler<in TEventArgs>(IWindow sender, TEventArgs e) where TEventArgs : EventArgs;

public delegate Task AsyncWindowEventHandler(IWindow sender, EventArgs e);

public delegate Task AsyncWindowEventHandler<in TEventArgs>(IWindow sender, TEventArgs e) where TEventArgs : EventArgs;

public delegate void WindowIconChangedEventHandler(IWindow window, WindowIcon? oldValue, WindowIcon? newValue);

public delegate void WindowTitleBarIconChangedEventHandler(IWindow window, Icon? oldValue, Icon? newValue);

public delegate void WindowTitleBarTitleChangedEventHandler(IWindow window, string? oldValue, string? newValue);

public delegate void WindowMenuChangedEventHandler(IWindow window, TopLevelMenuRegistry? oldValue, TopLevelMenuRegistry? newValue);

public delegate void WindowTitleBarBrushChangedEventHandler(IWindow window, IColourBrush? oldValue, IColourBrush? newValue);

public delegate void WindowBorderBrushChangedEventHandler(IWindow window, IColourBrush? oldValue, IColourBrush? newValue);

public delegate void WindowTitleBarTextAlignmentChangedEventHandler(IWindow window, TextAlignment oldValue, TextAlignment newValue);

/// <summary>
/// Exposes window-like properties that delegate to either a native OS window or a single-view overlay (typically only used on mobile or rasp pi).
/// </summary>
public interface IWindow : ITopLevelComponentManager {
    /// <summary>
    /// The data key used to access the window from <see cref="IContextData"/> in, for example, a command
    /// </summary>
    public static readonly DataKey<IWindow> WindowDataKey = DataKey<IWindow>.Create("WindowingWindow");

    /// <summary>
    /// Gets the window manager associated with this window
    /// </summary>
    IWindowManager WindowMyManager { get; }

    /// <summary>
    /// Gets the parent window. This is usually only set for dialogs, however, it might be
    /// set for windows that want to stay on-top of another window without being a dialog.
    /// </summary>
    IWindow? Owner { get; }

    /// <summary>
    /// Enumerates the child windows of this window. Windows are added to this before
    /// <see cref="WindowOpening"/> is fired, and removed before <see cref="WindowClosed"/> is fired 
    /// </summary>
    IEnumerable<IWindow> OwnedWindows { get; }

    /// <summary>
    /// Gets whether this window is a main window. When shown, it will replace the main
    /// window status of the previous main window until this window closes.
    /// </summary>
    bool IsMainWindow { get; }

    /// <summary>
    /// Gets whether the window is in the process of closing. This property is false when <see cref="IsClosed"/> is true.
    /// </summary>
    bool IsClosing { get; }

    /// <summary>
    /// Gets whether the window is actually closed. This differs from <see cref="IsClosing"/> because
    /// on platforms that use a closing animation, <see cref="IsClosed"/> will be set as true once
    /// the window is fully closed, whereas <see cref="IsClosing"/> is set after invoking <see cref="CloseAsync"/>
    /// and is then set to false when <see cref="IsClosed"/> becomes true
    /// </summary>
    bool IsClosed { get; }

    /// <summary>
    /// Gets whether the window is in the process of being opened. This is set to tru before <see cref="WindowOpening"/>
    /// and is set to false before <see cref="WindowOpened"/> (which is also when <see cref="IsOpen"/> is set to true)
    /// </summary>
    bool IsOpening { get; }

    /// <summary>
    /// Gets whether this window is actually open. This is set to true before <see cref="WindowOpened"/>
    /// is fired and is set to false before <see cref="WindowClosed"/> is fired
    /// </summary>
    bool IsOpen { get; }

    /// <summary>
    /// Returns true when <see cref="WindowOpened"/> has been fired
    /// </summary>
    bool HasOpenedBefore => this.IsOpen || this.IsClosing || this.IsClosed;

    /// <summary>
    /// Returns true when <see cref="IsOpen"/> is true and <see cref="IsClosed"/> is false
    /// </summary>
    bool IsOpenAndNotClosing => this.IsOpen && !this.IsClosing;

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
    /// Gets or sets the title bar text
    /// </summary>
    string? Title { get; set; }

    /// <summary>
    /// Gets or sets this window's main menu. Setting as null will remove the visual menu
    /// </summary>
    TopLevelMenuRegistry? Menu { get; set; }

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
    /// Gets or sets the content of this window. This is equivalent to <see cref="ContentControl.Content"/>
    /// </summary>
    object? Content { get; set; }

    /// <summary>
    /// Returns an avalonia object that represents the window itself in the visual tree.
    /// This will be a visual parent of <see cref="Content"/>, but not necessarily a direct parent.
    /// <para>
    /// This can be used to access the <see cref="IControlContextData"/> via the <see cref="DataManager"/>
    /// </para>
    /// <para>
    /// When this window is a desktop window, this control will be an instance of <see cref="Window"/>
    /// </para>
    /// </summary>
    Interactive Control { get; }

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
    event WindowEventHandler? WindowOpening;

    /// <summary>
    /// An event fired when the window is fully opening. This is fired before the task returned by <see cref="ShowAsync"/> is completed
    /// </summary>
    event WindowEventHandler? WindowOpened;

    /// <summary>
    /// An event fired when the window is requested to close. <see cref="IsClosing"/> and <see cref="IsClosed"/>
    /// will both be false at this time.
    /// <para>
    /// This and <see cref="BeforeClosingAsync"/> are the only times that cancelling window closure is possible
    /// </para>
    /// </summary>
    event WindowEventHandler<WindowCancelCloseEventArgs>? BeforeClosing;

    /// <summary>
    /// An asynchronous event fired when the window is requested to close. The handlers are invoked
    /// in their own tasks once all handlers of <see cref="BeforeClosing"/> are invoked.
    /// <para>
    /// This and <see cref="BeforeClosing"/> are the only times that cancelling window closure is possible
    /// </para>
    /// </summary>
    event AsyncWindowEventHandler<WindowCancelCloseEventArgs>? BeforeClosingAsync;

    /// <summary>
    /// An event fired when the window is actually about to close. This is fired after <see cref="IsClosing"/> is set as true,
    /// but before <see cref="IsClosed"/> is set as true.
    /// </summary>
    event WindowEventHandler<WindowCloseEventArgs>? WindowClosing;

    /// <summary>
    /// An event fired when the window is actually about to close. The handlers are invoked in their own tasks once all handlers
    /// of <see cref="WindowClosing"/> are invoked.
    /// </summary>
    event AsyncWindowEventHandler<WindowCloseEventArgs>? WindowClosingAsync;

    /// <summary>
    /// An event fired when the window is fully closed. <see cref="IsClosing"/> is set to false and <see cref="IsClosed"/>
    /// is set as true prior to this event.
    /// <para>
    /// This is fired before the task returned by <see cref="CloseAsync"/> or <see cref="WaitForClosedAsync"/> becomes completed
    /// </para>
    /// </summary>
    event WindowEventHandler<WindowCloseEventArgs>? WindowClosed;

    event WindowIconChangedEventHandler IconChanged;
    event WindowTitleBarIconChangedEventHandler TitleBarIconChanged;
    event WindowMenuChangedEventHandler MenuChanged;
    event WindowTitleBarBrushChangedEventHandler TitleBarBrushChanged;
    event WindowBorderBrushChangedEventHandler BorderBrushChanged;
    event WindowTitleBarTextAlignmentChangedEventHandler TitleBarTextAlignmentChanged;

    /// <summary>
    /// Tries to get a top level object from this window. Note, the top level may not actually equal the <see cref="Control"/> instance.
    /// </summary>
    /// <param name="topLevel">The found top level</param>
    /// <returns>True if a top level was found</returns>
    bool TryGetTopLevel([NotNullWhen(true)] out TopLevel? topLevel);
    
    /// <summary>
    /// Shows this window in a non-modal mode and returns once the window is visible on-screen
    /// </summary>
    void Show();

    /// <summary>
    /// Shows this window in a non-modal mode. This method posts an open request
    /// on the dispatcher and returns a task that completes when the window opens.
    /// </summary>
    /// <returns>A task that completes once the window has opened (i.e. once the opening animation has finished)</returns>
    Task ShowAsync();

    /// <summary>
    /// Shows this window in a modal mode. This method will return before the window is
    /// closed, however, the task will be completed once the window actually closes.
    /// </summary>
    /// <returns>A task that completes once the window has opened (i.e. once the opening animation has finished)</returns>
    Task<object?> ShowDialog();

    /// <summary>
    /// Closes this window and waits until it is closed
    /// </summary>
    /// <param name="dialogResult">The dialog result. Ignored when not showing as a dialog</param>
    /// <returns>True if the window was actually closed, False if the close attempt was cancelled</returns>
    bool Close(object? dialogResult = null);

    /// <summary>
    /// Closes this window. This method posts a close request on the dispatcher and
    /// returns a task that completes when the window closes.
    /// </summary>
    /// <param name="dialogResult">The dialog result. Ignored when not showing as a dialog</param>
    /// <returns>True if the window was actually closed, False if the close attempt was cancelled</returns>
    Task<bool> CloseAsync(object? dialogResult = null);

    /// <summary>
    /// Waits for this window to become closed. Note this does not actually tell the window to close.
    /// <para>
    /// If the window is already closed then this method just immediately returns <see cref="Task.CompletedTask"/>
    /// </para>
    /// </summary>
    /// <param name="cancellationToken">Allows to stop waiting for the window to close</param>
    /// <returns>A task that completes before the task returned by <see cref="CloseAsync"/> becomes completed</returns>
    Task WaitForClosedAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Force-activates this window, making <see cref="IsActivated"/> and it becomes the focused control
    /// </summary>
    void Activate();
}

/// <summary>
/// Event args for when a <see cref="IWindow"/> is shown
/// </summary>
public class WindowEventArgs(IWindow window) : EventArgs {
    public IWindow Window { get; } = window;
}

/// <summary>
/// Event args for when a window is closing and when closed
/// </summary>
public class WindowCloseEventArgs(IWindow window, WindowCloseReason reason, bool isFromCode) : WindowEventArgs(window) {
    /// <summary>
    /// Gets the reason for why the window is closing 
    /// </summary>
    public WindowCloseReason Reason { get; } = reason;

    /// <summary>
    /// Gets whether the closing operation was caused by user code (i.e. called from <see cref="IWindow.CloseAsync"/>)
    /// </summary>
    public bool IsFromCode { get; } = isFromCode;
}

/// <summary>
/// Event args for when trying to close a window
/// </summary>
public class WindowCancelCloseEventArgs(IWindow window, WindowCloseReason reason, bool isFromCode) : WindowCloseEventArgs(window, reason, isFromCode) {
    // We don't allow de-cancelling the close because it could lead to unexpected behaviour and issues

    /// <summary>
    /// Gets whether the closing of the window should be cancelled
    /// </summary>
    public bool IsCancelled { get; private set; }

    /// <summary>
    /// Sets <see cref="IsCancelled"/> to true
    /// </summary>
    public void SetCancelled() => this.IsCancelled = true;
}