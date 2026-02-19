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
using PFXToolKitUI.Utils.Events;
using PFXToolKitUI.Utils.Reactive;

namespace PFXToolKitUI.Services.UserInputs;

/// <summary>
/// The base class for a user input dialog's model, which contains generic
/// properties suitable across any type of two-buttoned titlebar and message dialog
/// </summary>
public abstract class UserInputInfo : ITransferableData {
    public static IEventObservable<UserInputInfo> CaptionObservable => field ??= Observable.ForEvent<UserInputInfo>((s, e) => s.CaptionChanged += e, (s, e) => s.CaptionChanged -= e);
    public static IEventObservable<UserInputInfo> MessageObservable => field ??= Observable.ForEvent<UserInputInfo>((s, e) => s.MessageChanged += e, (s, e) => s.MessageChanged -= e);
    public static IEventObservable<UserInputInfo> ConfirmTextObservable => field ??= Observable.ForEvent<UserInputInfo>((s, e) => s.ConfirmTextChanged += e, (s, e) => s.ConfirmTextChanged -= e);
    public static IEventObservable<UserInputInfo> CancelTextObservable => field ??= Observable.ForEvent<UserInputInfo>((s, e) => s.CancelTextChanged += e, (s, e) => s.CancelTextChanged -= e);
    public static IEventObservable<UserInputInfo> HasErrorsObservable => field ??= Observable.ForEvent<UserInputInfo>((s, e) => s.HasErrorsChanged += e, (s, e) => s.HasErrorsChanged -= e);
    
    private string? caption, message;

    public TransferableData TransferableData { get; }

    /// <summary>
    /// Gets or sets the dialog's caption, displayed usually in the titlebar
    /// </summary>
    public string? Caption {
        get => this.caption;
        set => PropertyHelper.SetAndRaiseINE(ref this.caption, value, this, this.CaptionChanged);
    }

    /// <summary>
    /// Gets or sets the dialog's message, displayed above the input field(s) at the top of the dialog's content,
    /// typically in bolder text. This could be some general information about what the fields do or maybe some rules.
    /// See derived classes for properties such as labels or field descriptions, which may be more specific
    /// </summary>
    public string? Message {
        get => this.message;
        set => PropertyHelper.SetAndRaiseINE(ref this.message, value, this, this.MessageChanged);
    }

    /// <summary>
    /// Gets or sets the text in the confirm button
    /// </summary>
    public string? ConfirmText {
        get => field;
        set => PropertyHelper.SetAndRaiseINE(ref field, value, this, this.ConfirmTextChanged);
    } = "OK";

    /// <summary>
    /// Gets or sets the text in the cancel button
    /// </summary>
    public string? CancelText {
        get => field;
        set => PropertyHelper.SetAndRaiseINE(ref field, value, this, this.CancelTextChanged);
    } = "Cancel";

    /// <summary>
    /// Gets or sets an async callback that is invoked and awaited when the dialog window is about to close.
    /// This is only invoked when there are no text errors.
    /// <para>
    /// If the result of the awaited task returns false, the dialog will not be closed.
    /// </para>
    /// </summary>
    /// <remarks>Do not use <see cref="Delegate.Combine(Delegate?, Delegate?)"/> on this value, because it will not be handled correctly</remarks>
    public TryConfirmAsyncEventHandler? TryConfirmAsync { get; set; }

    /// <summary>
    /// Gets or sets the button that is focused by default. The default is <see cref="ButtonType.None"/>, meaning no button is focused
    /// </summary>
    public ButtonType DefaultButton { get; init; }
    
    public event EventHandler? CaptionChanged, MessageChanged;
    public event EventHandler? ConfirmTextChanged, CancelTextChanged;
    
    /// <summary>
    /// Fired when one or more errors change in this user input info. This is listened to by
    /// the GUI to invoke <see cref="HasErrors"/> and update the confirm button
    /// </summary>
    public event EventHandler? HasErrorsChanged;

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
    public void RaiseHasErrorsChanged() => this.HasErrorsChanged?.Invoke(this, EventArgs.Empty);

    /// <summary>
    /// Checks if there are no errors present. This is used to enable or disable the confirm button
    /// </summary>
    public abstract bool HasErrors();

    /// <summary>
    /// Forces any errors to be re-calculated. This is called when the dialog first opens because
    /// this object will be in its initial state and only just connected to the UI, so this method
    /// should set any errors forcefully.
    /// <para>
    /// This method is also invoked just before the dialog closes, to ensure <see cref="HasErrors"/>
    /// returns the absolutely true case, in case implementers have some sort of delay between input
    /// change and errors updated
    /// </para>
    /// </summary>
    public abstract void UpdateAllErrors();
    
    /// <summary>
    /// Specifies the type of button the user can click to cause the dialog to produce a result
    /// </summary>
    public enum ButtonType {
        /// <summary>
        /// No button
        /// </summary>
        None,
        /// <summary>
        /// The confirm button
        /// </summary>
        Confirm,
        /// <summary>
        /// The cancel button
        /// </summary>
        Cancel
    }
}

/// <summary>
/// A delegate for the try confirm handler
/// </summary>
/// <param name="info">The sender info</param>
/// <param name="token">
/// The cancellation token that becomes cancelled when the user forces the window to be closed,
/// e.g. by clicking cancel or the X button, when the try confirm callback is already running.
/// </param>
public delegate Task<bool> TryConfirmAsyncEventHandler(UserInputInfo info, CancellationToken token);