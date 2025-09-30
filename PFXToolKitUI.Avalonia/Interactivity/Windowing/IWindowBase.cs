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
using PFXToolKitUI.Avalonia.Interactivity.Windowing.Desktop;
using PFXToolKitUI.Avalonia.Interactivity.Windowing.Overlays;
using PFXToolKitUI.Interactivity.Windowing;

namespace PFXToolKitUI.Avalonia.Interactivity.Windowing;

/// <summary>
/// An object that represents some kind of window, either a <see cref="IDesktopWindow"/> or a <see cref="IOverlayWindow"/>
/// </summary>
public interface IWindowBase : ITopLevel {
    /// <summary>
    /// Returns an avalonia object that represents the window itself in the visual tree.
    /// This will be a visual parent of <see cref="Content"/>, but not necessarily a direct parent.
    /// <para>
    /// This can be used to access the <see cref="IControlContextData"/> via the <see cref="DataManager"/>
    /// </para>
    /// <para>
    /// When the current instance is <see cref="IDesktopWindow"/>, this control will be an instance of <see cref="Window"/>.
    /// When the current instance is <see cref="IOverlayWindow"/>, this control will typically be a special internal control
    /// that exists as a visual child to the root content of the single view application, or a specific control that has its own
    /// popup manager associated with it.
    /// </para>
    /// </summary>
    Interactive Control { get; }
    
    /// <summary>
    /// Gets or sets the content of this window. This is equivalent to <see cref="ContentControl.Content"/>
    /// </summary>
    object? Content { get; set; }

    /// <summary>
    /// Gets the open state for this window. This represents whether the window is unopened, trying to close, closing, etc.
    /// </summary>
    OpenState OpenState { get; }
    
    /// <summary>
    /// Tries to get a <see cref="TopLevel"/> object from this window. The top level found might not be
    /// reference equal to the <see cref="Control"/> property, but instead, may be some sort of delegate instance
    /// in the case of a single-view application.
    /// </summary>
    /// <param name="topLevel">The found top level</param>
    /// <returns>True if a top level was found</returns>
    bool TryGetTopLevel([NotNullWhen(true)] out TopLevel? topLevel);
    
    /// <summary>
    /// Shows this window in a non-modal mode.
    /// </summary>
    /// <returns>A task that completes once the window has opened</returns>
    /// <exception cref="InvalidOperationException">The <see cref="OpenState"/> is not <see cref="Windowing.OpenState.NotOpened"/></exception>
    Task ShowAsync();

    /// <summary>
    /// Shows this window in a modal mode.
    /// </summary>
    /// <returns>
    /// A task that completes when the window closes, and whose result is the first value passed
    /// to <see cref="RequestCloseAsync"/> when the window was not already trying to close.
    /// </returns>
    /// <exception cref="InvalidOperationException">The <see cref="OpenState"/> is not <see cref="Windowing.OpenState.NotOpened"/></exception>
    Task<object?> ShowDialogAsync();

    /// <summary>
    /// Requests this window to close. The returned task is completed when this window either closes
    /// or the close operation was cancelled, where the bool result represents the closed state.
    /// </summary>
    /// <param name="dialogResult">The dialog result. Ignored when not showing as a dialog</param>
    /// <returns>True if the window was actually closed, False if the close attempt was cancelled</returns>
    /// <exception cref="InvalidOperationException">The <see cref="OpenState"/> is not <see cref="OpenState.Open"/></exception>
    Task<bool> RequestCloseAsync(object? dialogResult = null);

    /// <summary>
    /// Returns a task that completes when this window's <see cref="OpenState"/> becomes <see cref="Windowing.OpenState.Closed"/>.
    /// Note, this does not actually tell the window to close.
    /// <para>
    /// If this window is already closed then this method returns <see cref="Task.CompletedTask"/>
    /// </para>
    /// <para>
    /// This method does not throw <see cref="OperationCanceledException"/>
    /// </para>
    /// </summary>
    /// <param name="cancellationToken">Allows to stop waiting for the window to close</param>
    /// <returns>A task that completes before the task returned by <see cref="RequestCloseAsync"/> becomes completed</returns>
    Task WaitForClosedAsync(CancellationToken cancellationToken = default);
}