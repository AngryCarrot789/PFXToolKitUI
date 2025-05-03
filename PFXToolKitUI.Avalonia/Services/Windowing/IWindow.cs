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

using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using PFXToolKitUI.Interactivity;
using PFXToolKitUI.Interactivity.Contexts;

namespace PFXToolKitUI.Avalonia.Services.Windowing;

/// <summary>
/// The base interface for a modal or non-modal window. On single view applications
/// this wraps a control placed ontop of the content of a single view
/// </summary>
public interface IWindow : ITopLevel {
    public static readonly DataKey<IWindow> WindowDataKey = DataKey<IWindow>.Create("PFXWindow");
    
    /// <summary>
    /// Gets or sets the text alignment for the window's titlebar
    /// </summary>
    TextAlignment TitleBarTextAlignment { get; set; }
    
    /// <summary>
    /// Sets or sets the title bar background
    /// </summary>
    IBrush? TitleBarBrush { get; set; }    
    
    /// <summary>
    /// Gets or sets the window's border brush
    /// </summary>
    IBrush? BorderBrush { get; set; }
    
    /// <summary>
    /// Gets or sets the width of the window
    /// </summary>
    double Width { get; set; }

    /// <summary>
    /// Gets or sets the height of the window
    /// </summary>
    double Height { get; set; }

    /// <summary>
    /// Gets or sets the title of the window
    /// </summary>
    string? Title { get; set; }

    /// <summary>
    /// Returns true when the window is open
    /// </summary>
    bool IsOpen { get; }

    /// <summary>
    ///  Gets whether the window has been closed after opening. False by default.
    /// Windows cannot be re-opened once closed, they must be re-created
    /// </summary>
    bool IsClosed { get; }

    /// <summary>
    /// Gets whether the window was opened as a dialog (modal). When true, it shouldn't really be
    /// closed via <see cref="Close()"/>, instead give a result via <see cref="Close(bool?)"/>
    /// </summary>
    bool IsOpenAsDialog { get; }

    /// <summary>
    /// Gets or sets if the window is resizable. Typically false for dialogs
    /// </summary>
    bool IsResizable { get; set; }

    /// <summary>
    /// Gets or sets if auto-sizing the window to the content minimum size is allowed. False by default
    /// </summary>
    bool CanAutoSizeToContent { get; set; }

    /// <summary>
    /// Gets the underlying root control represented by this window. For desktop, typically returns
    /// the <see cref="Window"/> instance unless the app forces a single-view windowing system is used
    /// (therefore, never cast this object to a specific derived type unless absolutely certain)
    /// </summary>
    ContentControl Control { get; }

    /// <summary>
    /// Gets the storage provider service this window is linked to
    /// </summary>
    IStorageProvider StorageProvider { get; }

    /// <summary>
    /// Fired when the user is trying to close the window. Async is supported, and the return
    /// value is the 'isCloseCancelled' state, as in, return true to prevent the window closing.
    /// <para>
    /// Be weary of the passed <see cref="WindowCloseReason"/>, since cancellation may not
    /// actually prevent the window from closing
    /// </para>
    /// </summary>
    event WindowClosingAsyncEventHandler? WindowClosing;
    
    /// <summary>
    /// Fired when the window actually closes
    /// </summary>
    event WindowClosedEventHandler? WindowClosed;
    
    /// <summary>
    /// Shows the dialog and returns the result
    /// </summary>
    /// <param name="parent"></param>
    /// <returns></returns>
    Task<TResult?> ShowDialog<TResult>(IWindow parent);

    /// <summary>
    /// Shows the dialog and returns the result
    /// </summary>
    /// <param name="parent"></param>
    /// <returns></returns>
    async Task ShowDialog(IWindow parent) {
        await this.ShowDialog<object>(parent);
    }
    
    /// <summary>
    /// Shows this window
    /// </summary>
    /// <param name="parent">The optional parent of this window. When specified, we are always on top of the parent window</param>
    /// <exception cref="InvalidOperationException">Attempt to open after already closed (<see cref="IWindow.IsClosed"/> is true)</exception>
    void Show(IWindow? parent);

    /// <summary>
    /// Closes the window as a non-modal (regular window)
    /// </summary>
    /// <exception cref="InvalidOperationException">Window is not open or is already closed (<see cref="IWindow.IsOpen"/> is false or <see cref="IWindow.IsClosed"/> is true)</exception>
    void Close();
    
    /// <summary>
    /// Closes the window as modal (dialog window)
    /// </summary>
    /// <exception cref="InvalidOperationException">Window is not open or is already closed (<see cref="IWindow.IsOpen"/> is false or <see cref="IWindow.IsClosed"/> is true)</exception>
    void Close(object? dialogResult);
}