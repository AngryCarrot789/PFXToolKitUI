// 
// Copyright (c) 2025-2025 REghZy
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

using PFXToolKitUI.Icons;

namespace PFXToolKitUI.Services.Messaging;

/// <summary>
/// A reusable and readonly template for creating <see cref="MessageBoxInfo"/> instances
/// </summary>
public sealed class MessageBoxTemplate {
    public string? Caption { get; init; } = MessageBoxInfo.DefaultCaption;
    public string? Header { get; init; }
    public string? Message { get; init; }
    public string? ExtraDetails { get; init; }
    public MessageBoxButtons Buttons { get; init; }
    public Icon? Icon { get; init; }
    public string? YesOkText { get; init; }
    public string? NoText { get; init; }
    public string? CancelText { get; init; }
    public string? ShowDetailsText { get; init; } = MessageBoxInfo.DefaultShowDetailsText;
    public string? HideDetailsText { get; init; } = MessageBoxInfo.DefaultHideDetailsText;
    public string? PersistentDialogName { get; init; }
    public bool AlwaysUseThisResult { get; init; }
    public bool AlwaysUseThisResultUntilAppCloses { get; init; }
    public string? AlwaysUseThisResultText { get; init; } = MessageBoxInfo.DefaultAlwaysUseThisResultText;
    public MessageBoxResult DefaultButton { get; init; }

    public MessageBoxTemplate() {
    }

    public MessageBoxTemplate(string caption, string message, MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxResult defaultButton = MessageBoxResult.None, Icon? icon = null, string? persistentDialogName = null) {
        this.Caption = caption;
        this.Message = message;
        this.Buttons = buttons;
        this.DefaultButton = defaultButton;
        this.Icon = icon;
        this.PersistentDialogName = persistentDialogName;
    }

    public MessageBoxTemplate(string caption, string header, string message, MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxResult defaultButton = MessageBoxResult.None, Icon? icon = null, string? persistentDialogName = null) {
        this.Caption = caption;
        this.Header = header;
        this.Message = message;
        this.Buttons = buttons;
        this.DefaultButton = defaultButton;
        this.Icon = icon;
        this.PersistentDialogName = persistentDialogName;
    }

    /// <summary>
    /// Creates an instance of <see cref="MessageBoxInfo"/> from this template
    /// </summary>
    /// <param name="dialogCancellation">Cancellation token used to notify the window to close</param>
    /// <returns>The new message box info instance</returns>
    public MessageBoxInfo CreateInfo(CancellationToken dialogCancellation = default) {
        return new MessageBoxInfo(this, dialogCancellation);
    }

    /// <summary>
    /// Shows a message box using the info created by <see cref="CreateInfo"/>
    /// </summary>
    /// <param name="dialogCancellation">Cancellation token used to notify the window to close</param>
    /// <returns></returns>
    public Task<MessageBoxResult> ShowMessage(CancellationToken dialogCancellation = default) => IMessageDialogService.Instance.ShowMessage(new MessageBoxInfo(this, dialogCancellation));
}