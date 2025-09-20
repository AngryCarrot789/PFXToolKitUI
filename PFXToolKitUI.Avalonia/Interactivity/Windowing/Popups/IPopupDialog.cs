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
using Avalonia.Interactivity;
using PFXToolKitUI.Avalonia.Interactivity.Contexts;
using PFXToolKitUI.Interactivity.Contexts;
using PFXToolKitUI.Themes;

namespace PFXToolKitUI.Avalonia.Interactivity.Windowing.Popups;

public delegate void PopupEventHandler(IPopupDialog sender, EventArgs e);

public delegate void PopupEventHandler<in TEventArgs>(IPopupDialog sender, TEventArgs e) where TEventArgs : EventArgs;

public delegate Task AsyncPopupEventHandler<in TEventArgs>(IPopupDialog sender, TEventArgs e) where TEventArgs : EventArgs;

public delegate void PopupBorderBrushChangedEventHandler(IPopupDialog sender, IColourBrush? oldValue, IColourBrush? newValue);

/// <summary>
/// A popup within a single-view application. Typically only used on mobile platforms
/// </summary>
public interface IPopupDialog {
    /// <summary>
    /// The data key used to access the popup from <see cref="IContextData"/> in, for example, a command
    /// </summary>
    public static readonly DataKey<IPopupDialog> PopupDataKey = DataKey<IPopupDialog>.Create("SingleViewWindowingPopup");

    /// <summary>
    /// Gets the popup manager associated with this popup
    /// </summary>
    IPopupDialogManager PopupManager { get; }

    /// <summary>
    /// Gets the parent popup
    /// </summary>
    IPopupDialog? Owner { get; }

    /// <summary>
    /// Enumerates the child popups of this popup. Popups are added to this before
    /// <see cref="PopupOpening"/> is fired, and removed before <see cref="PopupClosed"/> is fired 
    /// </summary>
    IEnumerable<IPopupDialog> OwnedWindows { get; }

    /// <summary>
    /// Gets the title bar info object that this popup was created with. If non-null, this can be used to change the title bar text, icon, etc.
    /// </summary>
    PopupTitleBarInfo? TitleBarInfo { get; }

    /// <summary>
    /// Gets or sets the brush that colours the border of the popup. Setting this to null will disable the border
    /// </summary>
    IColourBrush? BorderBrush { get; set; }

    /// <summary>
    /// Gets or sets the content of this popup. This is equivalent to <see cref="ContentControl.Content"/>
    /// </summary>
    object? Content { get; set; }

    /// <summary>
    /// Returns an avalonia object that represents the popup itself in the visual tree.
    /// This will be a visual parent of <see cref="Content"/>, but not necessarily a direct parent.
    /// <para>
    /// This can be used to access the <see cref="IControlContextData"/> via the <see cref="DataManager"/>
    /// </para>
    /// </summary>
    Interactive Control { get; }

    /// <summary>
    /// Gets whether the popup is in the process of closing. This property is false when <see cref="IsClosed"/> is true.
    /// </summary>
    bool IsClosing { get; }

    /// <summary>
    /// Gets whether the popup is actually closed. This differs from <see cref="IsClosing"/> because
    /// on platforms that use a closing animation, <see cref="IsClosed"/> will be set as true once
    /// the popup is fully closed, whereas <see cref="IsClosing"/> is set after invoking <see cref="CloseAsync"/>
    /// and is then set to false when <see cref="IsClosed"/> becomes true
    /// </summary>
    bool IsClosed { get; }

    /// <summary>
    /// Gets whether the popup is in the process of being opened. This is set to tru before <see cref="PopupOpening"/>
    /// and is set to false before <see cref="PopupOpened"/> (which is also when <see cref="IsOpen"/> is set to true)
    /// </summary>
    bool IsOpening { get; }

    /// <summary>
    /// Gets whether this popup is actually open. This is set to true before <see cref="PopupOpened"/>
    /// is fired and is set to false before <see cref="PopupClosed"/> is fired
    /// </summary>
    bool IsOpen { get; }

    /// <summary>
    /// Returns true when <see cref="PopupOpened"/> has been fired
    /// </summary>
    bool HasOpenedBefore => this.IsOpen || this.IsClosing || this.IsClosed;

    /// <summary>
    /// Returns true when <see cref="IsOpen"/> is true and <see cref="IsClosed"/> is false
    /// </summary>
    bool IsOpenAndNotClosing => this.IsOpen && !this.IsClosing;

    /// <summary>
    /// Returns whether this popup was shown as a dialog. Note, this may return true even when closing or closed
    /// </summary>
    bool IsDialog { get; }

    /// <summary>
    /// Gets whether this popup is the application-wide active popup
    /// </summary>
    bool IsActivated { get; }

    /// <summary>
    /// An event fired when the popup is in the process of opening but has not been shown on screen yet.
    /// </summary>
    event PopupEventHandler? PopupOpening;

    /// <summary>
    /// An event fired when the popup is fully opening. This is fired before the task returned by <see cref="ShowAsync"/> is completed
    /// </summary>
    event PopupEventHandler? PopupOpened;

    /// <summary>
    /// An event fired when the popup is requested to close. <see cref="IsClosing"/> and <see cref="IsClosed"/>
    /// will both be false at this time.
    /// <para>
    /// This and <see cref="BeforeClosingAsync"/> are the only times that cancelling popup closure is possible
    /// </para>
    /// </summary>
    event PopupEventHandler<PopupCancelCloseEventArgs>? BeforeClosing;

    /// <summary>
    /// An asynchronous event fired when the popup is requested to close. The handlers are invoked
    /// in their own tasks once all handlers of <see cref="BeforeClosing"/> are invoked.
    /// <para>
    /// This and <see cref="BeforeClosing"/> are the only times that cancelling popup closure is possible
    /// </para>
    /// </summary>
    event AsyncPopupEventHandler<PopupCancelCloseEventArgs>? BeforeClosingAsync;

    /// <summary>
    /// An event fired when the popup is actually about to close. This is fired after <see cref="IsClosing"/> is set as true,
    /// but before <see cref="IsClosed"/> is set as true.
    /// </summary>
    event PopupEventHandler<PopupCloseEventArgs>? PopupClosing;

    /// <summary>
    /// An event fired when the popup is actually about to close. The handlers are invoked in their own tasks once all handlers
    /// of <see cref="PopupClosing"/> are invoked.
    /// </summary>
    event AsyncPopupEventHandler<PopupCloseEventArgs>? PopupClosingAsync;

    /// <summary>
    /// An event fired when the popup is fully closed. <see cref="IsClosing"/> is set to false and <see cref="IsClosed"/>
    /// is set as true prior to this event.
    /// <para>
    /// This is fired before the task returned by <see cref="CloseAsync"/> or <see cref="WaitForClosedAsync"/> becomes completed
    /// </para>
    /// </summary>
    event PopupEventHandler<PopupCloseEventArgs>? PopupClosed;

    event PopupBorderBrushChangedEventHandler BorderBrushChanged;

    /// <summary>
    /// Tries to get a top level object from this popup. Note, the top level may not actually equal the <see cref="Control"/> instance.
    /// </summary>
    /// <param name="topLevel">The found top level</param>
    /// <returns>True if a top level was found</returns>
    bool TryGetTopLevel([NotNullWhen(true)] out TopLevel? topLevel);

    /// <summary>
    /// Shows this popup in a non-modal mode and returns once the popup is visible on-screen
    /// </summary>
    void Show();

    /// <summary>
    /// Shows this popup in a non-modal mode. This method posts an open request
    /// on the dispatcher and returns a task that completes when the popup opens.
    /// </summary>
    /// <returns>A task that completes once the popup has opened (i.e. once the opening animation has finished)</returns>
    Task ShowAsync();

    /// <summary>
    /// Shows this popup in a modal mode. This method will return before the popup is
    /// closed, however, the task will be completed once the popup actually closes.
    /// </summary>
    /// <returns>A task that completes once the popup has opened (i.e. once the opening animation has finished)</returns>
    Task<object?> ShowDialog();

    /// <summary>
    /// Closes this popup and waits until it is closed
    /// </summary>
    /// <param name="dialogResult">The dialog result. Ignored when not showing as a dialog</param>
    /// <returns>True if the popup was actually closed, False if the close attempt was cancelled</returns>
    bool Close(object? dialogResult = null);

    /// <summary>
    /// Closes this popup. This method posts a close request on the dispatcher and
    /// returns a task that completes when the popup closes.
    /// </summary>
    /// <param name="dialogResult">The dialog result. Ignored when not showing as a dialog</param>
    /// <returns>True if the popup was actually closed, False if the close attempt was cancelled</returns>
    Task<bool> CloseAsync(object? dialogResult = null);

    /// <summary>
    /// Waits for this popup to become closed. Note this does not actually tell the popup to close.
    /// <para>
    /// If the popup is already closed then this method just immediately returns <see cref="Task.CompletedTask"/>
    /// </para>
    /// </summary>
    /// <param name="cancellationToken">Allows to stop waiting for the popup to close</param>
    /// <returns>A task that completes before the task returned by <see cref="CloseAsync"/> becomes completed</returns>
    Task WaitForClosedAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Force-activates this popup, making <see cref="IsActivated"/> and it becomes the focused control
    /// </summary>
    void Activate();
}

/// <summary>
/// Event args for when a <see cref="IPopupDialog"/> is shown
/// </summary>
public class PopupEventArgs(IPopupDialog dialog) : EventArgs {
    public IPopupDialog Dialog { get; } = dialog;
}

/// <summary>
/// Event args for when a popup is closing and when closed
/// </summary>
public class PopupCloseEventArgs(IPopupDialog dialog, WindowCloseReason reason, bool isFromCode) : PopupEventArgs(dialog) {
    /// <summary>
    /// Gets the reason for why the popup is closing 
    /// </summary>
    public WindowCloseReason Reason { get; } = reason;

    /// <summary>
    /// Gets whether the closing operation was caused by user code (i.e. called from <see cref="IPopupDialog.CloseAsync"/>)
    /// </summary>
    public bool IsFromCode { get; } = isFromCode;
}

/// <summary>
/// Event args for when trying to close a popup
/// </summary>
public class PopupCancelCloseEventArgs(IPopupDialog dialog, WindowCloseReason reason, bool isFromCode) : PopupCloseEventArgs(dialog, reason, isFromCode) {
    // We don't allow de-cancelling the close because it could lead to unexpected behaviour and issues
    private volatile bool isCancelled;

    /// <summary>
    /// Gets whether the closing of the popup should be cancelled
    /// </summary>
    public bool IsCancelled => this.isCancelled;

    /// <summary>
    /// Sets <see cref="IsCancelled"/> to true
    /// </summary>
    public void SetCancelled() => this.isCancelled = true;
}