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
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.Services.Messaging;

public delegate void MessageBoxInfoEventHandler(MessageBoxInfo sender);

public delegate void MessageBoxInfoIconChangedEventHandler(MessageBoxInfo sender, Icon? oldIcon, Icon? newIcon);

/// <summary>
/// A class for a basic message box class with a maximum of 3 buttons; Yes/OK, No and Cancel
/// </summary>
public class MessageBoxInfo {
    private string? caption = "Alert";
    private string? header;
    private string? message = "Message";
    private string? extraDetails;
    private string? yesOkText;
    private string? noText;
    private string? cancelText;
    private string? showDetailsText = "Show details", hideDetailsText = "Hide details";
    private string? alwaysUseThisResultText = "Always use this option";
    private bool alwaysUseThisResult;
    private bool alwaysUseThisResultUntilAppCloses = true;
    private MessageBoxButtons buttons;
    private Icon? icon;

    /// <summary>
    /// Gets or sets the message caption, aka window title
    /// </summary>
    public string? Caption {
        get => this.caption;
        set => PropertyHelper.SetAndRaiseINE(ref this.caption, value, this, static t => t.CaptionChanged?.Invoke(t));
    }

    /// <summary>
    /// Gets or sets the optional message header, displayed separated above the <see cref="Message"/>
    /// </summary>
    public string? Header {
        get => this.header;
        set => PropertyHelper.SetAndRaiseINE(ref this.header, value, this, static t => t.HeaderChanged?.Invoke(t));
    }

    /// <summary>
    /// Gets or sets the main body text
    /// </summary>
    public string? Message {
        get => this.message;
        set => PropertyHelper.SetAndRaiseINE(ref this.message, value, this, static t => t.MessageChanged?.Invoke(t));
    }

    /// <summary>
    /// Gets or sets the extra details text. When this value contains characters (excluding whitespaces),
    /// a "Show Details" button will expand the dialog and show this text in a scrollable panel.
    /// <para>
    /// This could, for example, be the <see cref="Exception.ToString"/> of an exception
    /// </para>
    /// </summary>
    public string? ExtraDetails {
        get => this.extraDetails;
        set => PropertyHelper.SetAndRaiseINE(ref this.extraDetails, value, this, static t => t.ExtraDetailsChanged?.Invoke(t));
    }

    /// <summary>
    /// Gets or sets which buttons are shown
    /// </summary>
    public MessageBoxButtons Buttons {
        get => this.buttons;
        set => PropertyHelper.SetAndRaiseINE(ref this.buttons, value, this, static t => t.ButtonsChanged?.Invoke(t));
    }

    /// <summary>
    /// Gets or sets the icon displayed in either the header area (if the header has text) or the message body area 
    /// </summary>
    public Icon? Icon {
        get => this.icon;
        set => PropertyHelper.SetAndRaiseINE(ref this.icon, value, this, static (t, o, n) => t.IconChanged?.Invoke(t, o, n));
    }

    #region Buttons Text

    /// <summary>
    /// Gets or sets the explicit text in the button that yields <see cref="MessageBoxResult.Yes"/> <see cref="MessageBoxResult.OK"/>. 
    /// Default is null, meaning <see cref="ActualYesOkText"/> will return a default value based on <see cref="Buttons"/>.
    /// </summary>
    public string? YesOkText {
        get => this.yesOkText;
        set => PropertyHelper.SetAndRaiseINE(ref this.yesOkText, value, this, static t => t.YesOkTextChanged?.Invoke(t));
    }

    /// <summary>
    /// Gets or sets the explicit text in the button that yields <see cref="MessageBoxResult.No"/>. 
    /// Default is null, meaning <see cref="ActualNoText"/> will return a default value based on <see cref="Buttons"/>.
    /// </summary>
    public string? NoText {
        get => this.noText;
        set => PropertyHelper.SetAndRaiseINE(ref this.noText, value, this, static t => t.NoTextChanged?.Invoke(t));
    }

    /// <summary>
    /// Gets or sets the explicit text in the button that yields <see cref="MessageBoxResult.Cancel"/>. 
    /// Default is null, meaning <see cref="ActualCancelText"/> will return a default value based on <see cref="Buttons"/>.
    /// </summary>
    public string? CancelText {
        get => this.cancelText;
        set => PropertyHelper.SetAndRaiseINE(ref this.cancelText, value, this, static t => t.CancelTextChanged?.Invoke(t));
    }

    /// <summary>
    /// Gets or sets the text displayed in the toggle button when the extra details are not visible
    /// </summary>
    public string? ShowDetailsText {
        get => this.showDetailsText;
        set => PropertyHelper.SetAndRaiseINE(ref this.showDetailsText, value, this, static t => t.ShowDetailsTextChanged?.Invoke(t));
    }
    
    /// <summary>
    /// Gets or sets the text displayed in the toggle button when the extra details are visible
    /// </summary>
    public string? HideDetailsText {
        get => this.hideDetailsText;
        set => PropertyHelper.SetAndRaiseINE(ref this.hideDetailsText, value, this, static t => t.HideDetailsTextChanged?.Invoke(t));
    }
    
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
        get => this.alwaysUseThisResult;
        set => PropertyHelper.SetAndRaiseINE(ref this.alwaysUseThisResult, value, this, static t => t.AlwaysUseThisResultChanged?.Invoke(t));
    }

    /// <summary>
    /// Gets or sets the option for if <see cref="AlwaysUseThisResult"/> applies only until the app closes
    /// </summary>
    public bool AlwaysUseThisResultUntilAppCloses {
        get => this.alwaysUseThisResultUntilAppCloses;
        set => PropertyHelper.SetAndRaiseINE(ref this.alwaysUseThisResultUntilAppCloses, value, this, static t => t.AlwaysUseThisResultUntilAppClosesChanged?.Invoke(t));
    }

    /// <summary>
    /// Gets or sets the text in the check box that toggles <see cref="AlwaysUseThisResult"/>
    /// </summary>
    public string? AlwaysUseThisResultText {
        get => this.alwaysUseThisResultText;
        set => PropertyHelper.SetAndRaiseINE(ref this.alwaysUseThisResultText, value, this, static t => t.AlwaysUseThisResultTextChanged?.Invoke(t));
    }

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

    public event MessageBoxInfoEventHandler? CaptionChanged;
    public event MessageBoxInfoEventHandler? HeaderChanged;
    public event MessageBoxInfoEventHandler? MessageChanged;
    public event MessageBoxInfoEventHandler? ExtraDetailsChanged;
    public event MessageBoxInfoEventHandler? YesOkTextChanged;
    public event MessageBoxInfoEventHandler? NoTextChanged;
    public event MessageBoxInfoEventHandler? CancelTextChanged;
    public event MessageBoxInfoEventHandler? ShowDetailsTextChanged;
    public event MessageBoxInfoEventHandler? HideDetailsTextChanged;
    public event MessageBoxInfoEventHandler? ButtonsChanged;
    public event MessageBoxInfoIconChangedEventHandler? IconChanged;
    public event MessageBoxInfoEventHandler? AlwaysUseThisResultTextChanged;
    public event MessageBoxInfoEventHandler? AlwaysUseThisResultChanged;
    public event MessageBoxInfoEventHandler? AlwaysUseThisResultUntilAppClosesChanged;

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