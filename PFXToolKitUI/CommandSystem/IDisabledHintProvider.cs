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

using PFXToolKitUI.AdvancedMenuService;
using PFXToolKitUI.Icons;
using PFXToolKitUI.Interactivity.Contexts;

namespace PFXToolKitUI.CommandSystem;

/// <summary>
/// An interface implemented by an object that can provide insight into why it is disabled
/// </summary>
public interface IDisabledHintProvider {
    /// <summary>
    /// Provides a hint as to why the command is disabled
    /// </summary>
    /// <param name="context">
    /// The context available when querying the hint info. Should contain the same
    /// information, for example, a context menu item has when trying to execute it
    /// </param>
    /// <param name="sourceContextMenu">
    /// The context menu registry. This is provided when the caller is a context menu entry
    /// </param>
    /// <returns>The hint info</returns>
    DisabledHintInfo? ProvideDisabledHint(IContextData context, ContextRegistry? sourceContextMenu);
}

/// <summary>
/// An object containing information as to why an object is disabled
/// </summary>
public abstract class DisabledHintInfo : IDisposable {
    private bool isDisposed;

    /// <summary>
    /// Dispose this hint info. This is invoked once the info is no longer being used by the caller (i.e. once the tooltip closes)
    /// </summary>
    public void Dispose() {
        if (!this.isDisposed) {
            this.isDisposed = true;
            this.OnDisposed();
        }
    }

    protected virtual void OnDisposed() {
    }
}

/// <summary>
/// Disabled hint info that has a caption and body message.
/// </summary>
public sealed class SimpleDisabledHintInfo : DisabledHintInfo {
    /// <summary>
    /// Gets the caption/header text
    /// </summary>
    public string? Caption { get; }

    /// <summary>
    /// Gets the main body text
    /// </summary>
    public string? Message { get; }
    
    /// <summary>
    /// Gets the icon shown to the side of the caption
    /// </summary>
    public Icon? CaptionIcon { get; }

    public SimpleDisabledHintInfo(string? caption, string? message, Icon? captionIcon = null) {
        this.Caption = caption;
        this.Message = message;
        this.CaptionIcon = captionIcon;
    }
}