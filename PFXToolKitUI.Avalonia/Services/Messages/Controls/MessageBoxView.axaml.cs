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

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using PFXToolKitUI.Avalonia.Bindings;
using PFXToolKitUI.Avalonia.Interactivity.Windowing;
using PFXToolKitUI.Avalonia.Interactivity.Windowing.Desktop;
using PFXToolKitUI.Avalonia.Interactivity.Windowing.Overlays;
using PFXToolKitUI.Logging;
using PFXToolKitUI.Services.Messaging;

namespace PFXToolKitUI.Avalonia.Services.Messages.Controls;

public partial class MessageBoxView : UserControl {
    public static readonly StyledProperty<MessageBoxInfo?> MessageBoxDataProperty = AvaloniaProperty.Register<MessageBoxView, MessageBoxInfo?>("MessageBoxData");

    public MessageBoxInfo? MessageBoxData {
        get => this.GetValue(MessageBoxDataProperty);
        set => this.SetValue(MessageBoxDataProperty, value);
    }

    /// <summary>
    /// Gets the window this view is currently open in
    /// </summary>
    public IWindowBase? OwnerWindow { get; private set; }

    private readonly IBinder<MessageBoxInfo> captionBinder = new EventUpdateBinder<MessageBoxInfo>(nameof(MessageBoxInfo.CaptionChanged), (b) => {
        IWindowBase window = ((MessageBoxView) b.Control).OwnerWindow!;
        if (window is IDesktopWindow desktop)
            desktop.Title = b.Model.Caption;
        if (window is IOverlayWindow overlay && overlay.TitleBarInfo != null)
            overlay.TitleBarInfo.Title = b.Model.Caption;
    });

    private readonly IBinder<MessageBoxInfo> headerBinder = new EventUpdateBinder<MessageBoxInfo>(nameof(MessageBoxInfo.HeaderChanged), (b) => ((TextBlock) b.Control).Text = b.Model.Header);
    private readonly IBinder<MessageBoxInfo> messageBinder = new EventUpdateBinder<MessageBoxInfo>(nameof(MessageBoxInfo.MessageChanged), (b) => ((TextBox) b.Control).Text = b.Model.Message);
    private readonly IBinder<MessageBoxInfo> yesOkTextBinder = new EventUpdateBinder<MessageBoxInfo>(nameof(MessageBoxInfo.YesOkTextChanged), (b) => ((Button) b.Control).Content = b.Model.YesOkText);
    private readonly IBinder<MessageBoxInfo> noTextBinder = new EventUpdateBinder<MessageBoxInfo>(nameof(MessageBoxInfo.NoTextChanged), (b) => ((Button) b.Control).Content = b.Model.NoText);
    private readonly IBinder<MessageBoxInfo> cancelTextBinder = new EventUpdateBinder<MessageBoxInfo>(nameof(MessageBoxInfo.CancelTextChanged), (b) => ((Button) b.Control).Content = b.Model.CancelText);
    private readonly IBinder<MessageBoxInfo> autrTextBinder = new EventUpdateBinder<MessageBoxInfo>(nameof(MessageBoxInfo.AlwaysUseThisResultTextChanged), (b) => ((CheckBox) b.Control).Content = b.Model.AlwaysUseThisResultText);
    private readonly IBinder<MessageBoxInfo> autrBinder = new AvaloniaPropertyToEventPropertyBinder<MessageBoxInfo>(CheckBox.IsCheckedProperty, nameof(MessageBoxInfo.AlwaysUseThisResultChanged), (b) => ((CheckBox) b.Control).IsChecked = b.Model.AlwaysUseThisResult, (b) => b.Model.AlwaysUseThisResult = ((CheckBox) b.Control).IsChecked == true);
    private readonly IBinder<MessageBoxInfo> autrUntilCloseBinder = new AvaloniaPropertyToEventPropertyBinder<MessageBoxInfo>(CheckBox.IsCheckedProperty, nameof(MessageBoxInfo.AlwaysUseThisResultUntilAppClosesChanged), (b) => ((CheckBox) b.Control).IsChecked = b.Model.AlwaysUseThisResultUntilAppCloses, (b) => b.Model.AlwaysUseThisResultUntilAppCloses = ((CheckBox) b.Control).IsChecked == true);

    public MessageBoxView() {
        this.InitializeComponent();
        this.headerBinder.AttachControl(this.PART_Header);
        this.messageBinder.AttachControl(this.PART_Message);
        this.yesOkTextBinder.AttachControl(this.PART_YesOkButton);
        this.noTextBinder.AttachControl(this.PART_NoButton);
        this.cancelTextBinder.AttachControl(this.PART_CancelButton);
        this.autrTextBinder.AttachControl(this.PART_AlwaysUseThisResult);
        this.autrBinder.AttachControl(this.PART_AlwaysUseThisResult);
        this.autrUntilCloseBinder.AttachControl(this.PART_AUTR_UntilAppCloses);
        this.PART_Header.PropertyChanged += this.OnHeaderTextBlockPropertyChanged;

        this.PART_YesOkButton.Click += this.OnConfirmButtonClicked;
        this.PART_NoButton.Click += this.OnNoButtonClicked;
        this.PART_CancelButton.Click += this.OnCancelButtonClicked;
    }

    private void OnHeaderTextBlockPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e) {
        if (e.Property == TextBlock.TextProperty) {
            this.PART_MessageContainer.IsVisible = !string.IsNullOrWhiteSpace(e.GetNewValue<string?>());
        }
    }

    static MessageBoxView() {
        MessageBoxDataProperty.Changed.AddClassHandler<MessageBoxView, MessageBoxInfo?>((o, e) => o.OnMessageBoxDataChanged(e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
    }

    private void OnConfirmButtonClicked(object? sender, RoutedEventArgs e) {
        MessageBoxInfo? data = this.MessageBoxData;
        if (data == null) {
            return;
        }

        switch (data.Buttons) {
            case MessageBoxButton.OK:
            case MessageBoxButton.OKCancel:
                this.RequestClose(MessageBoxResult.OK);
                return;
            case MessageBoxButton.YesNoCancel:
            case MessageBoxButton.YesNo:
                this.RequestClose(MessageBoxResult.Yes);
                return;
            default:
                this.RequestClose(MessageBoxResult.None);
                return;
        }
    }

    private void OnNoButtonClicked(object? sender, RoutedEventArgs e) {
        MessageBoxInfo? data = this.MessageBoxData;
        if (data == null) {
            return;
        }

        if ((data.Buttons == MessageBoxButton.YesNo || data.Buttons == MessageBoxButton.YesNoCancel)) {
            this.RequestClose(MessageBoxResult.No);
        }
        else {
            this.RequestClose(MessageBoxResult.None);
        }
    }

    private void OnCancelButtonClicked(object? sender, RoutedEventArgs e) {
        MessageBoxInfo? data = this.MessageBoxData;
        if (data == null) {
            return;
        }

        if ((data.Buttons == MessageBoxButton.OKCancel || data.Buttons == MessageBoxButton.YesNoCancel)) {
            this.RequestClose(MessageBoxResult.Cancel);
        }
        else {
            this.RequestClose(MessageBoxResult.None);
        }
    }

    private void OnMessageBoxDataChanged(MessageBoxInfo? oldData, MessageBoxInfo? newData) {
        if (oldData != null) {
            oldData.ButtonsChanged -= this.OnActiveButtonsChanged;
            oldData.AlwaysUseThisResultChanged -= this.UpdateAlwaysUseThisResultUntilAppCloses;
        }

        if (newData != null) {
            newData.ButtonsChanged += this.OnActiveButtonsChanged;
            newData.AlwaysUseThisResultChanged += this.UpdateAlwaysUseThisResultUntilAppCloses;
        }

        // Create this first just in case there's a problem with no registrations
        this.captionBinder.SwitchModel(newData);
        this.headerBinder.SwitchModel(newData);
        this.messageBinder.SwitchModel(newData);
        this.yesOkTextBinder.SwitchModel(newData);
        this.noTextBinder.SwitchModel(newData);
        this.cancelTextBinder.SwitchModel(newData);
        this.autrTextBinder.SwitchModel(newData);
        this.autrBinder.SwitchModel(newData);
        this.autrUntilCloseBinder.SwitchModel(newData);

        if (newData != null)
            this.UpdateAlwaysUseThisResultUntilAppCloses(newData);

        // Set visible when data is null, for debugging
        this.PART_AUTRPanel.IsVisible = newData == null || !string.IsNullOrWhiteSpace(newData.PersistentDialogName);

        this.UpdateVisibleButtons();
        if (newData != null) {
            ApplicationPFX.Instance.Dispatcher.Post(() => {
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
            }, DispatchPriority.Loaded);
        }
    }

    private void UpdateAlwaysUseThisResultUntilAppCloses(MessageBoxInfo sender) {
        this.PART_AUTR_UntilAppCloses.IsEnabled = sender.AlwaysUseThisResult;
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
    public void RequestClose(MessageBoxResult result) {
        if (this.OwnerWindow == null) {
            return;
        }

        if (this.OwnerWindow.OpenState != OpenState.Open) {
            AppLogger.Instance.WriteLine($"Warning: attempt to request message box to close again. State = {this.OwnerWindow.OpenState}");
            return;
        }

        this.OwnerWindow.RequestClose(result);
    }

    internal void OnWindowOpening(IWindowBase window) {
        this.OwnerWindow = window;
        this.captionBinder.AttachControl(this);
    }

    internal void OnWindowOpened(IWindowBase window) {
        if (window is IDesktopWindow desktop)
            desktop.SizingInfo.SizeToContent = SizeToContent.Manual;
    }

    internal void OnWindowClosed(IWindowBase window) {
        this.captionBinder.DetachControl();
        this.OwnerWindow = null;
    }
}