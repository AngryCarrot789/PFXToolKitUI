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
using PFXToolKitUI.Utils.Events;

namespace PFXToolKitUI.Avalonia.Interactivity.Windowing.Overlays;

/// <summary>
/// An overlay window within a single-view application. Typically only used on mobile platforms
/// </summary>
public interface IOverlayWindow : IWindowBase, IBaseOverlayOrContentHost {
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
    /// An event fired when the overlay window is in the process of opening but has not been shown on screen yet.
    /// </summary>
    event EventHandler? Opening;

    /// <summary>
    /// An event fired when the overlay window is fully opening. This is fired before the task returned by <see cref="ShowAsync"/> is completed
    /// </summary>
    event EventHandler? Opened;

    /// <summary>
    /// An asynchronous event fired when the overlay window is requested to close. The handlers are invoked
    /// in their own tasks once all handlers of <see cref="TryClose"/> are invoked.
    /// <para>
    /// This and <see cref="TryClose"/> are the only times that cancelling overlay window closure is possible
    /// </para>
    /// <para>
    /// Note, this event is not fired if the overlay is being force closed. You must handle <see cref="ClosingAsync"/>
    /// to account for this possibility (see <see cref="OverlayWindowCloseEventArgs.IsForced"/>)
    /// </para>
    /// </summary>
    event AsyncEventHandler<OverlayWindowCancelCloseEventArgs>? TryCloseAsync;

    /// <summary>
    /// An event fired when the overlay window is actually about to close. The handlers are invoked in their own tasks once all handlers
    /// of <see cref="Closing"/> are invoked.
    /// </summary>
    event AsyncEventHandler<OverlayWindowCloseEventArgs>? ClosingAsync;

    /// <summary>
    /// An event fired when the overlay window is fully closed. <see cref="IsClosing"/> is set to false and <see cref="IsClosed"/>
    /// is set as true prior to this event.
    /// <para>
    /// This is fired before the task returned by <see cref="CloseAsync"/> or <see cref="WaitForClosedAsync"/> becomes completed
    /// </para>
    /// </summary>
    event EventHandler<OverlayWindowCloseEventArgs>? Closed;

    /// <summary>
    /// An event fired when <see cref="BorderBrush"/> changes
    /// </summary>
    event EventHandler<ValueChangedEventArgs<IColourBrush?>> BorderBrushChanged;
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
public class OverlayWindowCloseEventArgs : OverlayWindowEventArgs {
    /// <summary>
    /// Gets the reason for why the overlay window is closing 
    /// </summary>
    public WindowCloseReason Reason { get; }

    /// <summary>
    /// Gets whether the closing operation was caused by user code (i.e. called from <see cref="IOverlayWindow.CloseAsync"/>)
    /// </summary>
    public bool IsFromCode { get; }

    /// <summary>
    /// Gets whether the close operation is being forced. For example, the app or OS is shutting down,
    /// or a window or control which contains a <see cref="IOverlayWindowHost"/> is being removed from the visual tree soon.
    /// In these case, the overlay may not be visible when <see cref="IOverlayWindow.ClosingAsync"/> is raised (since it
    /// would require pausing the UI thread until all handlers have been invoked).
    /// <para>
    /// Failing to acknowledge the forced mode could mean a loss of data or the application locking up
    /// (e.g. a <see cref="TaskCompletionSource"/> does not get completed because the logic did not expect
    /// <see cref="IOverlayWindow.TryClose"/> and <see cref="IOverlayWindow.TryCloseAsync"/> to not be called, because
    /// they are not called when this property is true)
    /// </para>
    /// </summary>
    public bool IsForced { get; }
    
    /// <summary>
    /// Gets the dialog result passed to <see cref="IOverlayWindow.RequestClose"/> or <see cref="IOverlayWindow.RequestCloseAsync"/>
    /// </summary>
    public object? DialogResult { get; }
    
    /// <summary>
    /// Event args for when an overlay window is closing and when closed
    /// </summary>
    public OverlayWindowCloseEventArgs(IOverlayWindow dialog, WindowCloseReason reason, bool isFromCode, bool forced, object? dialogResult) : base(dialog) {
        this.Reason = reason;
        this.IsFromCode = isFromCode;
        this.IsForced = forced;
        this.DialogResult = dialogResult;
    }
}

/// <summary>
/// Event args for when trying to close an overlay window
/// </summary>
public class OverlayWindowCancelCloseEventArgs : OverlayWindowCloseEventArgs {
    // We don't allow de-cancelling the close because it could lead to unexpected behaviour and issues
    private volatile bool isCancelled;

    /// <summary>
    /// Gets whether the closing of the overlay window should be cancelled
    /// </summary>
    public bool IsCancelled => this.isCancelled;
    
    /// <summary>
    /// Event args for when trying to close an overlay window
    /// </summary>
    public OverlayWindowCancelCloseEventArgs(IOverlayWindow dialog, WindowCloseReason reason, bool isFromCode, object? dialogResult) : base(dialog, reason, isFromCode, false, dialogResult) {
    }

    /// <summary>
    /// Sets <see cref="IsCancelled"/> to true, preventing the overlay window from closing
    /// </summary>
    public void SetCancelled() => this.isCancelled = true;
}