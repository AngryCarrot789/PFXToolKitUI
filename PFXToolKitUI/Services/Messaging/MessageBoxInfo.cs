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

using PFXToolKitUI.Utils;

namespace PFXToolKitUI.Services.Messaging;

public delegate void MessageBoxDataButtonsChangedEventHandler(MessageBoxInfo sender);

/// <summary>
/// A class for a basic message box class with a maximum of 3 buttons; Yes/OK, No and Cancel
/// </summary>
public class MessageBoxInfo {
    private string? caption = "Alert";
    private string? header = null;
    private string? message = "Message";
    private string? yesOkText = "OK";
    private string? noText = "No";
    private string? cancelText = "Cancel";
    private string? alwaysUseThisResultText = "Always use this option";
    private bool alwaysUseThisResult = false;
    private bool alwaysUseThisResultUntilAppCloses = true;
    private MessageBoxButton buttons;

    public string? Caption {
        get => this.caption;
        set => PropertyHelper.SetAndRaiseINE(ref this.caption, value, this, static t => t.CaptionChanged?.Invoke(t));
    }

    public string? Header {
        get => this.header;
        set => PropertyHelper.SetAndRaiseINE(ref this.header, value, this, static t => t.HeaderChanged?.Invoke(t));
    }

    public string? Message {
        get => this.message;
        set => PropertyHelper.SetAndRaiseINE(ref this.message, value, this, static t => t.MessageChanged?.Invoke(t));
    }

    public string? YesOkText {
        get => this.yesOkText;
        set => PropertyHelper.SetAndRaiseINE(ref this.yesOkText, value, this, static t => t.YesOkTextChanged?.Invoke(t));
    }

    public string? NoText {
        get => this.noText;
        set => PropertyHelper.SetAndRaiseINE(ref this.noText, value, this, static t => t.NoTextChanged?.Invoke(t));
    }

    public string? CancelText {
        get => this.cancelText;
        set => PropertyHelper.SetAndRaiseINE(ref this.cancelText, value, this, static t => t.CancelTextChanged?.Invoke(t));
    }

    public string? AlwaysUseThisResultText {
        get => this.alwaysUseThisResultText;
        set => PropertyHelper.SetAndRaiseINE(ref this.alwaysUseThisResultText, value, this, static t => t.AlwaysUseThisResultTextChanged?.Invoke(t));
    }

    public bool AlwaysUseThisResult {
        get => this.alwaysUseThisResult;
        set => PropertyHelper.SetAndRaiseINE(ref this.alwaysUseThisResult, value, this, static t => t.AlwaysUseThisResultChanged?.Invoke(t));
    }

    public bool AlwaysUseThisResultUntilAppCloses {
        get => this.alwaysUseThisResultUntilAppCloses;
        set => PropertyHelper.SetAndRaiseINE(ref this.alwaysUseThisResultUntilAppCloses, value, this, static t => t.AlwaysUseThisResultUntilAppClosesChanged?.Invoke(t));
    }

    /// <summary>
    /// Gets or sets which buttons are shown
    /// </summary>
    public MessageBoxButton Buttons {
        get => this.buttons;
        set => PropertyHelper.SetAndRaiseINE(ref this.buttons, value, this, static t => t.ButtonsChanged?.Invoke(t));
    }

    /// <summary>
    /// Gets or sets the name of this dialog. This is used to maintain the message box result when the user checked
    /// the "Always use this option" box (and if they didn't check "Only until app closes", will also be saved to the config)
    /// </summary>
    public string? PersistentDialogName { get; init; }

    /// <summary>
    /// Gets or sets the type of button to automatically focus in the UI. Default is none
    /// </summary>
    public MessageBoxResult DefaultButton { get; init; }
    
    /// <summary>
    /// Gets or sets the cancellation token used to notify the window to close.
    /// This will cause the result of <see cref="IMessageDialogService.ShowMessage(MessageBoxInfo)"/>
    /// to return <see cref="MessageBoxResult.None"/>
    /// </summary>
    public CancellationToken DialogCancellation { get; init; }

    public event MessageBoxDataButtonsChangedEventHandler? ButtonsChanged;
    public event MessageBoxDataButtonsChangedEventHandler? CaptionChanged;
    public event MessageBoxDataButtonsChangedEventHandler? HeaderChanged;
    public event MessageBoxDataButtonsChangedEventHandler? MessageChanged;
    public event MessageBoxDataButtonsChangedEventHandler? YesOkTextChanged;
    public event MessageBoxDataButtonsChangedEventHandler? NoTextChanged;
    public event MessageBoxDataButtonsChangedEventHandler? CancelTextChanged;
    public event MessageBoxDataButtonsChangedEventHandler? AlwaysUseThisResultTextChanged;
    public event MessageBoxDataButtonsChangedEventHandler? AlwaysUseThisResultChanged;
    public event MessageBoxDataButtonsChangedEventHandler? AlwaysUseThisResultUntilAppClosesChanged;

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

    public void SetDefaultButtonText() {
        switch (this.buttons) {
            case MessageBoxButton.OK: this.YesOkText = "OK"; break;
            case MessageBoxButton.OKCancel:
                this.YesOkText = "OK";
                this.CancelText = "Cancel";
                break;
            case MessageBoxButton.YesNoCancel:
                this.YesOkText = "Yes";
                this.NoText = "No";
                this.CancelText = "Cancel";
                break;
            case MessageBoxButton.YesNo:
                this.YesOkText = "Yes";
                this.NoText = "No";
                break;
            default: throw new ArgumentOutOfRangeException();
        }
    }
}