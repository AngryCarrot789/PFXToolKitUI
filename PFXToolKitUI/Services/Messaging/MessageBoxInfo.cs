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

using PFXToolKitUI.Icons;
using PFXToolKitUI.Utils.Events;

namespace PFXToolKitUI.Services.Messaging;

/// <summary>
/// A class for a basic message box class with a maximum of 3 buttons; Yes/OK, No and Cancel
/// </summary>
public class MessageBoxInfo {
    public const string DefaultCaption = "Message";
    public const string DefaultShowDetailsText = "Show details";
    public const string DefaultHideDetailsText = "Hide details";
    public const string DefaultAlwaysUseThisResultText = "Always use this option";

    private string? caption = DefaultCaption;
    private string? header;
    private string? message;
    private MessageBoxButtons buttons;

    /// <summary>
    /// Gets or sets the message caption, aka window title
    /// </summary>
    public string? Caption {
        get => this.caption;
        set => PropertyHelper.SetAndRaiseINE(ref this.caption, value, this, this.CaptionChanged);
    }

    /// <summary>
    /// Gets or sets the optional message header, displayed separated above the <see cref="Message"/>
    /// </summary>
    public string? Header {
        get => this.header;
        set => PropertyHelper.SetAndRaiseINE(ref this.header, value, this, this.HeaderChanged);
    }

    /// <summary>
    /// Gets or sets the main body text
    /// </summary>
    public string? Message {
        get => this.message;
        set => PropertyHelper.SetAndRaiseINE(ref this.message, value, this, this.MessageChanged);
    }

    /// <summary>
    /// Gets or sets the extra details text. When this value contains characters (excluding whitespaces),
    /// a "Show Details" button will expand the dialog and show this text in a scrollable panel.
    /// <para>
    /// This could, for example, be the <see cref="Exception.ToString"/> of an exception
    /// </para>
    /// </summary>
    public string? ExtraDetails {
        get => field;
        set => PropertyHelper.SetAndRaiseINE(ref field, value, this, this.ExtraDetailsChanged);
    }

    /// <summary>
    /// Gets or sets which buttons are shown
    /// </summary>
    public MessageBoxButtons Buttons {
        get => this.buttons;
        set => PropertyHelper.SetAndRaiseINE(ref this.buttons, value, this, this.ButtonsChanged);
    }

    /// <summary>
    /// Gets or sets the icon displayed in either the header area (if the header has text) or the message body area 
    /// </summary>
    public Icon? Icon {
        get => field;
        set => PropertyHelper.SetAndRaiseINE(ref field, value, this, this.IconChanged);
    }

    #region Buttons Text

    /// <summary>
    /// Gets or sets the explicit text in the button that yields <see cref="MessageBoxResult.Yes"/> <see cref="MessageBoxResult.OK"/>. 
    /// Default is null, meaning <see cref="ActualYesOkText"/> will return a default value based on <see cref="Buttons"/>.
    /// </summary>
    public string? YesOkText {
        get => field;
        set => PropertyHelper.SetAndRaiseINE(ref field, value, this, this.YesOkTextChanged);
    }

    /// <summary>
    /// Gets or sets the explicit text in the button that yields <see cref="MessageBoxResult.No"/>. 
    /// Default is null, meaning <see cref="ActualNoText"/> will return a default value based on <see cref="Buttons"/>.
    /// </summary>
    public string? NoText {
        get => field;
        set => PropertyHelper.SetAndRaiseINE(ref field, value, this, this.NoTextChanged);
    }

    /// <summary>
    /// Gets or sets the explicit text in the button that yields <see cref="MessageBoxResult.Cancel"/>. 
    /// Default is null, meaning <see cref="ActualCancelText"/> will return a default value based on <see cref="Buttons"/>.
    /// </summary>
    public string? CancelText {
        get => field;
        set => PropertyHelper.SetAndRaiseINE(ref field, value, this, this.CancelTextChanged);
    }

    /// <summary>
    /// Gets or sets the text displayed in the toggle button when the extra details are not visible
    /// </summary>
    public string? ShowDetailsText {
        get => field;
        set => PropertyHelper.SetAndRaiseINE(ref field, value, this, this.ShowDetailsTextChanged);
    } = DefaultShowDetailsText;

    /// <summary>
    /// Gets or sets the text displayed in the toggle button when the extra details are visible
    /// </summary>
    public string? HideDetailsText {
        get => field;
        set => PropertyHelper.SetAndRaiseINE(ref field, value, this, this.HideDetailsTextChanged);
    } = DefaultHideDetailsText;

    /// <summary>
    /// Gets the value of <see cref="YesOkText"/> if it has any non-whitespace chars, otherwise, returns a string based on <see cref="Buttons"/>
    /// </summary>
    public string ActualYesOkText => !string.IsNullOrWhiteSpace(this.YesOkText) ? this.YesOkText : GetDefaultYesOkText(this.buttons);

    /// <summary>
    /// Gets the value of <see cref="NoText"/> if it has any non-whitespace chars, otherwise, <c>"No"</c>
    /// </summary>
    public string ActualNoText => !string.IsNullOrWhiteSpace(this.NoText) ? this.NoText : "No";

    /// <summary>
    /// Gets the value of <see cref="CancelText"/> if it has any non-whitespace chars, otherwise, <c>"Cancel"</c>
    /// </summary>
    public string ActualCancelText => !string.IsNullOrWhiteSpace(this.CancelText) ? this.CancelText : "Cancel";

    #endregion

    #region Persistent Results

    /// <summary>
    /// Gets or sets the name of this dialog. This is used to maintain the message box result when the user checked
    /// the "Always use this option" box (and if they didn't check "Only until app closes", will also be saved to the config)
    /// <para>
    /// When this value is empty or whitespaces only, it is treated as null. A valid dialog name must have characters.
    /// </para>
    /// </summary>
    public string? PersistentDialogName { get; init; }

    /// <summary>
    /// Gets or sets the option for persistently using the user's result. Only used when <see cref="PersistentDialogName"/> is valid
    /// </summary>
    public bool AlwaysUseThisResult {
        get => field;
        set => PropertyHelper.SetAndRaiseINE(ref field, value, this, this.AlwaysUseThisResultChanged);
    }

    /// <summary>
    /// Gets or sets the option for if <see cref="AlwaysUseThisResult"/> applies only until the app closes
    /// </summary>
    public bool AlwaysUseThisResultUntilAppCloses {
        get => field;
        set => PropertyHelper.SetAndRaiseINE(ref field, value, this, this.AlwaysUseThisResultUntilAppClosesChanged);
    } = true;

    /// <summary>
    /// Gets or sets the text in the check box that toggles <see cref="AlwaysUseThisResult"/>
    /// </summary>
    public string? AlwaysUseThisResultText {
        get => field;
        set => PropertyHelper.SetAndRaiseINE(ref field, value, this, this.AlwaysUseThisResultTextChanged);
    } = DefaultAlwaysUseThisResultText;

    #endregion

    /// <summary>
    /// Gets or sets the type of button to automatically focus in the UI. Default is none
    /// </summary>
    public MessageBoxResult DefaultButton { get; init; }

    /// <summary>
    /// Gets or sets the cancellation token used to notify the window to close. This will cause the
    /// result of <see cref="IMessageDialogService.ShowMessage(MessageBoxInfo)"/> to return <see cref="MessageBoxResult.None"/>
    /// </summary>
    public CancellationToken DialogCancellation { get; init; }

    public event EventHandler? CaptionChanged;
    public event EventHandler? HeaderChanged;
    public event EventHandler? MessageChanged;
    public event EventHandler? ExtraDetailsChanged;
    public event EventHandler? YesOkTextChanged;
    public event EventHandler? NoTextChanged;
    public event EventHandler? CancelTextChanged;
    public event EventHandler? ShowDetailsTextChanged;
    public event EventHandler? HideDetailsTextChanged;
    public event EventHandler? ButtonsChanged;
    public event EventHandler? IconChanged;
    public event EventHandler? AlwaysUseThisResultTextChanged;
    public event EventHandler? AlwaysUseThisResultChanged;
    public event EventHandler? AlwaysUseThisResultUntilAppClosesChanged;

    public MessageBoxInfo() {
    }

    public MessageBoxInfo(string? message) : this() {
        this.message = message;
    }

    public MessageBoxInfo(string? caption, string? message) : this() {
        this.caption = caption;
        this.message = message;
    }

    public MessageBoxInfo(string? caption, string? header, string? message) : this() {
        this.caption = caption;
        this.header = header;
        this.message = message;
    }

    public MessageBoxInfo(MessageBoxTemplate template, CancellationToken dialogCancellation = default) : this() {
        this.Caption = template.Caption;
        this.Header = template.Header;
        this.Message = template.Message;
        this.ExtraDetails = template.ExtraDetails;
        this.Buttons = template.Buttons;
        this.Icon = template.Icon;
        this.YesOkText = template.YesOkText;
        this.NoText = template.NoText;
        this.CancelText = template.CancelText;
        this.ShowDetailsText = template.ShowDetailsText;
        this.HideDetailsText = template.HideDetailsText;
        this.PersistentDialogName = template.PersistentDialogName;
        this.AlwaysUseThisResult = template.AlwaysUseThisResult;
        this.AlwaysUseThisResultUntilAppCloses = template.AlwaysUseThisResultUntilAppCloses;
        this.AlwaysUseThisResultText = template.AlwaysUseThisResultText;
        this.DefaultButton = template.DefaultButton;
        this.DialogCancellation = dialogCancellation;
    }

    private static string GetDefaultYesOkText(MessageBoxButtons buttons) {
        switch (buttons) {
            case MessageBoxButtons.OK:
            case MessageBoxButtons.OKCancel:
                return "OK";
            case MessageBoxButtons.YesNoCancel:
            case MessageBoxButtons.YesNo:
                return "Yes";
            default: throw new ArgumentOutOfRangeException();
        }
    }
}