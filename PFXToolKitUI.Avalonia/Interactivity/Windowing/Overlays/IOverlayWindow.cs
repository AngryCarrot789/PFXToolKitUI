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

using Avalonia.Controls;
using PFXToolKitUI.Themes;

namespace PFXToolKitUI.Avalonia.Interactivity.Windowing.Overlays;

public delegate void OverlayWindowEventHandler(IOverlayWindow sender, EventArgs e);

public delegate void OverlayWindowEventHandler<in TEventArgs>(IOverlayWindow sender, TEventArgs e) where TEventArgs : EventArgs;

public delegate Task AsyncOverlayWindowEventHandler<in TEventArgs>(IOverlayWindow sender, TEventArgs e) where TEventArgs : EventArgs;

public delegate void OverlayWindowBorderBrushChangedEventHandler(IOverlayWindow sender, IColourBrush? oldValue, IColourBrush? newValue);

/// <summary>
/// An overlay window within a single-view application. Typically only used on mobile platforms
/// </summary>
public interface IOverlayWindow : IWindowBase {
    /// <summary>
    /// Gets the overlay window manager associated with this window
    /// </summary>
    IOverlayWindowManager OverlayManager { get; }

    /// <summary>
    /// Gets the overlay window that owns this window.
    /// </summary>
    IOverlayWindow? Owner { get; }

    /// <summary>
    /// Enumerates the child overlay windows of this window. 
    /// </summary>
    IEnumerable<IOverlayWindow> OwnedPopups { get; }

    /// <summary>
    /// Gets the title bar info object that this overlay window was created with. If non-null, this can be used to change the title bar text, icon, etc.
    /// </summary>
    OverlayWindowTitleBarInfo? TitleBarInfo { get; }

    /// <summary>
    /// Gets or sets the brush that colours the border of the overlay window. Setting this to null will disable the border
    /// </summary>
    IColourBrush? BorderBrush { get; set; }

    /// <summary>
    /// Gets or sets the content of this overlay window. This is equivalent to <see cref="ContentControl.Content"/>
    /// </summary>
    object? Content { get; set; }

    /// <summary>
    /// An event fired when the overlay window is in the process of opening but has not been shown on screen yet.
    /// </summary>
    event OverlayWindowEventHandler? Opening;

    /// <summary>
    /// An event fired when the overlay window is fully opening. This is fired before the task returned by <see cref="ShowAsync"/> is completed
    /// </summary>
    event OverlayWindowEventHandler? Opened;

    /// <summary>
    /// An event fired when the overlay window is requested to close. <see cref="IsClosing"/> and <see cref="IsClosed"/>
    /// will both be false at this time.
    /// <para>
    /// This and <see cref="TryCloseAsync"/> are the only times that cancelling overlay window closure is possible
    /// </para>
    /// </summary>
    event OverlayWindowEventHandler<OverlayWindowCancelCloseEventArgs>? TryClose;

    /// <summary>
    /// An asynchronous event fired when the overlay window is requested to close. The handlers are invoked
    /// in their own tasks once all handlers of <see cref="TryClose"/> are invoked.
    /// <para>
    /// This and <see cref="TryClose"/> are the only times that cancelling overlay window closure is possible
    /// </para>
    /// </summary>
    event AsyncOverlayWindowEventHandler<OverlayWindowCancelCloseEventArgs>? TryCloseAsync;

    /// <summary>
    /// An event fired when the overlay window is actually about to close. This is fired after <see cref="IsClosing"/> is set as true,
    /// but before <see cref="IsClosed"/> is set as true.
    /// </summary>
    event OverlayWindowEventHandler<OverlayWindowCloseEventArgs>? Closing;

    /// <summary>
    /// An event fired when the overlay window is actually about to close. The handlers are invoked in their own tasks once all handlers
    /// of <see cref="Closing"/> are invoked.
    /// </summary>
    event AsyncOverlayWindowEventHandler<OverlayWindowCloseEventArgs>? ClosingAsync;

    /// <summary>
    /// An event fired when the overlay window is fully closed. <see cref="IsClosing"/> is set to false and <see cref="IsClosed"/>
    /// is set as true prior to this event.
    /// <para>
    /// This is fired before the task returned by <see cref="CloseAsync"/> or <see cref="WaitForClosedAsync"/> becomes completed
    /// </para>
    /// </summary>
    event OverlayWindowEventHandler<OverlayWindowCloseEventArgs>? Closed;

    event OverlayWindowBorderBrushChangedEventHandler BorderBrushChanged;
}

/// <summary>
/// Event args for when a <see cref="IOverlayWindow"/> is shown
/// </summary>
public class OverlayWindowEventArgs(IOverlayWindow dialog) : EventArgs {
    public IOverlayWindow Dialog { get; } = dialog;
}

/// <summary>
/// Event args for when an overlay window is closing and when closed
/// </summary>
public class OverlayWindowCloseEventArgs(IOverlayWindow dialog, WindowCloseReason reason, bool isFromCode) : OverlayWindowEventArgs(dialog) {
    /// <summary>
    /// Gets the reason for why the overlay window is closing 
    /// </summary>
    public WindowCloseReason Reason { get; } = reason;

    /// <summary>
    /// Gets whether the closing operation was caused by user code (i.e. called from <see cref="IOverlayWindow.CloseAsync"/>)
    /// </summary>
    public bool IsFromCode { get; } = isFromCode;
}

/// <summary>
/// Event args for when trying to close an overlay window
/// </summary>
public class OverlayWindowCancelCloseEventArgs(IOverlayWindow dialog, WindowCloseReason reason, bool isFromCode) : OverlayWindowCloseEventArgs(dialog, reason, isFromCode) {
    // We don't allow de-cancelling the close because it could lead to unexpected behaviour and issues
    private volatile bool isCancelled;

    /// <summary>
    /// Gets whether the closing of the overlay window should be cancelled
    /// </summary>
    public bool IsCancelled => this.isCancelled;

    /// <summary>
    /// Sets <see cref="IsCancelled"/> to true
    /// </summary>
    public void SetCancelled() => this.isCancelled = true;
}