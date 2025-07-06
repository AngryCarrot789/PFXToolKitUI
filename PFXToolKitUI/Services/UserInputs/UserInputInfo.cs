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

using PFXToolKitUI.DataTransfer;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.Services.UserInputs;

public delegate void UserInputInfoEventHandler(UserInputInfo info);

/// <summary>
/// The base class for a user input dialog's model, which contains generic
/// properties suitable across any type of two-buttoned titlebar and message dialog
/// </summary>
public abstract class UserInputInfo : ITransferableData {
    private string? caption, message;
    private string? confirmText = "OK";
    private string? cancelText = "Cancel";

    public TransferableData TransferableData { get; }

    /// <summary>
    /// Gets or sets the dialog's caption, displayed usually in the titlebar
    /// </summary>
    public string? Caption {
        get => this.caption;
        set => PropertyHelper.SetAndRaiseINE(ref this.caption, value, this, static t => t.CaptionChanged?.Invoke(t));
    }

    /// <summary>
    /// Gets or sets the dialog's message, displayed above the input field(s) at the top of the dialog's content,
    /// typically in bolder text. This could be some general information about what the fields do or maybe some rules.
    /// See derived classes for properties such as labels or field descriptions, which may be more specific
    /// </summary>
    public string? Message {
        get => this.message;
        set => PropertyHelper.SetAndRaiseINE(ref this.message, value, this, static t => t.MessageChanged?.Invoke(t));
    }

    /// <summary>
    /// Gets or sets the text in the confirm button
    /// </summary>
    public string? ConfirmText {
        get => this.confirmText;
        set => PropertyHelper.SetAndRaiseINE(ref this.confirmText, value, this, static t => t.ConfirmTextChanged?.Invoke(t));
    }

    /// <summary>
    /// Gets or sets the text in the cancel button
    /// </summary>
    public string? CancelText {
        get => this.cancelText;
        set => PropertyHelper.SetAndRaiseINE(ref this.cancelText, value, this, static t => t.CancelTextChanged?.Invoke(t));
    }

    /// <summary>
    /// Gets or sets the button that is focused by default. True for the confirm
    /// button, False for the cancel button, null for none. Null is default
    /// </summary>
    public bool? DefaultButton { get; init; }
    
    /// <summary>
    /// Fired when one or more errors change in this user input info. This is listened to by
    /// the GUI to invoke <see cref="HasErrors"/> and update the confirm button
    /// </summary>
    public event UserInputInfoEventHandler? HasErrorsChanged;
    public event UserInputInfoEventHandler? CaptionChanged, MessageChanged;
    public event UserInputInfoEventHandler? ConfirmTextChanged, CancelTextChanged;

    protected UserInputInfo() {
        this.TransferableData = new TransferableData(this);
    }

    protected UserInputInfo(string? caption, string? message) : this() {
        this.caption = caption;
        this.message = message;
    }

    /// <summary>
    /// Raises the <see cref="HasErrorsChanged"/> event
    /// </summary>
    public void RaiseHasErrorsChanged() => this.HasErrorsChanged?.Invoke(this);

    /// <summary>
    /// Checks if there are no errors present. This is used to enable or disable the confirm button
    /// </summary>
    public abstract bool HasErrors();

    /// <summary>
    /// Forces any errors to be re-calculated. This is because this object will be in its
    /// initialised state and only just connected to the UI, so this method should set any
    /// errors forcefully.
    /// <para>
    /// This method does not need to call <see cref="RaiseHasErrorsChanged"/>, because this
    /// method is only typically used just before calling <see cref="HasErrors"/>
    /// </para>
    /// </summary>
    public abstract void UpdateAllErrors();
}