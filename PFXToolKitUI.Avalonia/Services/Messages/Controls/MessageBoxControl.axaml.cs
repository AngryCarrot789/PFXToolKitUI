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

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using PFXToolKitUI.Avalonia.Bindings;
using PFXToolKitUI.Services.Messaging;

namespace PFXToolKitUI.Avalonia.Services.Messages.Controls;

public partial class MessageBoxControl : WindowingContentControl {
    public static readonly StyledProperty<MessageBoxInfo?> MessageBoxDataProperty = AvaloniaProperty.Register<MessageBoxControl, MessageBoxInfo?>("MessageBoxData");

    public MessageBoxInfo? MessageBoxData {
        get => this.GetValue(MessageBoxDataProperty);
        set => this.SetValue(MessageBoxDataProperty, value);
    }

    private readonly AvaloniaPropertyToDataParameterBinder<MessageBoxInfo> captionBinder = new AvaloniaPropertyToDataParameterBinder<MessageBoxInfo>(WindowTitleProperty, MessageBoxInfo.CaptionParameter);
    private readonly AvaloniaPropertyToDataParameterBinder<MessageBoxInfo> headerBinder = new AvaloniaPropertyToDataParameterBinder<MessageBoxInfo>(TextBlock.TextProperty, MessageBoxInfo.HeaderParameter);
    private readonly AvaloniaPropertyToDataParameterBinder<MessageBoxInfo> messageBinder = new AvaloniaPropertyToDataParameterBinder<MessageBoxInfo>(TextBlock.TextProperty, MessageBoxInfo.MessageParameter);
    private readonly AvaloniaPropertyToDataParameterBinder<MessageBoxInfo> yesOkTextBinder = new AvaloniaPropertyToDataParameterBinder<MessageBoxInfo>(ContentProperty, MessageBoxInfo.YesOkTextParameter);
    private readonly AvaloniaPropertyToDataParameterBinder<MessageBoxInfo> noTextBinder = new AvaloniaPropertyToDataParameterBinder<MessageBoxInfo>(ContentProperty, MessageBoxInfo.NoTextParameter);
    private readonly AvaloniaPropertyToDataParameterBinder<MessageBoxInfo> cancelTextBinder = new AvaloniaPropertyToDataParameterBinder<MessageBoxInfo>(ContentProperty, MessageBoxInfo.CancelTextParameter);

    public MessageBoxControl() {
        this.InitializeComponent();
        this.captionBinder.AttachControl(this);
        this.headerBinder.AttachControl(this.PART_Header);
        this.messageBinder.AttachControl(this.PART_Message);
        this.yesOkTextBinder.AttachControl(this.PART_YesOkButton);
        this.noTextBinder.AttachControl(this.PART_NoButton);
        this.cancelTextBinder.AttachControl(this.PART_CancelButton);
        this.PART_Header.PropertyChanged += this.OnHeaderTextBlockPropertyChanged;

        this.PART_YesOkButton.Click += this.OnConfirmButtonClicked;
        this.PART_NoButton.Click += this.OnNoButtonClicked;
        this.PART_CancelButton.Click += this.OnCancelButtonClicked;
    }

    protected override void OnWindowOpened() {
        base.OnWindowOpened();

        this.Window!.Control.MinHeight = 100;
        this.Window!.Control.MinWidth = 300;
        this.Window!.Control.MaxWidth = 800;
        this.Window!.Control.MaxHeight = 800;
        this.Window.CanAutoSizeToContent = true;
        // this.Window!.IsResizable = false;
    }

    protected override void OnWindowClosed() {
        base.OnWindowClosed();
    }

    private void OnHeaderTextBlockPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e) {
        if (e.Property == TextBlock.TextProperty) {
            this.PART_MessageContainer.IsVisible = !string.IsNullOrWhiteSpace(e.GetNewValue<string?>());
        }
    }

    static MessageBoxControl() {
        MessageBoxDataProperty.Changed.AddClassHandler<MessageBoxControl, MessageBoxInfo?>((o, e) => o.OnMessageBoxDataChanged(e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
    }

    protected override void OnKeyDown(KeyEventArgs e) {
        base.OnKeyDown(e);
        if (e.Key == Key.Escape) {
            this.CancelDialog();
        }
    }

    private void OnConfirmButtonClicked(object? sender, RoutedEventArgs e) {
        MessageBoxInfo? data = this.MessageBoxData;
        if (data == null) {
            return;
        }

        switch (data.Buttons) {
            case MessageBoxButton.OK:
            case MessageBoxButton.OKCancel:
                this.Close(MessageBoxResult.OK);
                return;
            case MessageBoxButton.YesNoCancel:
            case MessageBoxButton.YesNo:
                this.Close(MessageBoxResult.Yes);
                return;
            default:
                this.Close(MessageBoxResult.None);
                return;
        }
    }

    private void OnNoButtonClicked(object? sender, RoutedEventArgs e) {
        MessageBoxInfo? data = this.MessageBoxData;
        if (data == null) {
            return;
        }

        if ((data.Buttons == MessageBoxButton.YesNo || data.Buttons == MessageBoxButton.YesNoCancel)) {
            this.Close(MessageBoxResult.No);
        }
        else {
            this.Close(MessageBoxResult.None);
        }
    }

    private void OnCancelButtonClicked(object? sender, RoutedEventArgs e) {
        MessageBoxInfo? data = this.MessageBoxData;
        if (data == null) {
            return;
        }

        if ((data.Buttons == MessageBoxButton.OKCancel || data.Buttons == MessageBoxButton.YesNoCancel)) {
            this.Close(MessageBoxResult.Cancel);
        }
        else {
            this.Close(MessageBoxResult.None);
        }
    }

    private void CancelDialog() => base.Window!.Close(null);

    private void OnMessageBoxDataChanged(MessageBoxInfo? oldData, MessageBoxInfo? newData) {
        if (oldData != null)
            oldData.ButtonsChanged -= this.OnActiveButtonsChanged;
        if (newData != null)
            newData.ButtonsChanged += this.OnActiveButtonsChanged;

        // Create this first just in case there's a problem with no registrations
        this.captionBinder.SwitchModel(newData);
        this.headerBinder.SwitchModel(newData);
        this.messageBinder.SwitchModel(newData);
        this.yesOkTextBinder.SwitchModel(newData);
        this.noTextBinder.SwitchModel(newData);
        this.cancelTextBinder.SwitchModel(newData);
        this.UpdateVisibleButtons();
        if (newData != null) {
            Dispatcher.UIThread.InvokeAsync(() => {
                switch (newData.DefaultButton) {
                    case MessageBoxResult.None: break;
                    case MessageBoxResult.Yes:
                    case MessageBoxResult.OK:
                        if (this.PART_YesOkButton.IsVisible)
                            this.PART_YesOkButton.Focus();
                    break;
                    case MessageBoxResult.Cancel:
                        if (this.PART_CancelButton.IsVisible)
                            this.PART_CancelButton.Focus();
                    break;
                    case MessageBoxResult.No:
                        if (this.PART_NoButton.IsVisible)
                            this.PART_NoButton.Focus();
                    break;
                }
            }, DispatcherPriority.Loaded);
        }
    }

    private void OnActiveButtonsChanged(MessageBoxInfo sender) {
        this.UpdateVisibleButtons();
    }

    /// <summary>
    /// Updates which buttons are visible based on our message box data's <see cref="MessageBoxInfo.Buttons"/> property
    /// </summary>
    public void UpdateVisibleButtons() {
        MessageBoxInfo? data = this.MessageBoxData;
        if (data == null) {
            return;
        }

        switch (data.Buttons) {
            case MessageBoxButton.OK:
                this.PART_YesOkButton.IsVisible = true;
                this.PART_NoButton.IsVisible = false;
                this.PART_CancelButton.IsVisible = false;
            break;
            case MessageBoxButton.OKCancel:
                this.PART_YesOkButton.IsVisible = true;
                this.PART_NoButton.IsVisible = false;
                this.PART_CancelButton.IsVisible = true;
            break;
            case MessageBoxButton.YesNoCancel:
                this.PART_YesOkButton.IsVisible = true;
                this.PART_NoButton.IsVisible = true;
                this.PART_CancelButton.IsVisible = true;
            break;
            case MessageBoxButton.YesNo:
                this.PART_YesOkButton.IsVisible = true;
                this.PART_NoButton.IsVisible = true;
                this.PART_CancelButton.IsVisible = false;
            break;
            default: throw new ArgumentOutOfRangeException();
        }
    }

    /// <summary>
    /// Tries to close the dialog
    /// </summary>
    /// <param name="result">The dialog result wanted</param>
    /// <returns>True if the dialog was closed, false if it could not be closed due to a validation error or other error</returns>
    public void Close(MessageBoxResult result) {
        base.Window!.Close(result);
    }
}