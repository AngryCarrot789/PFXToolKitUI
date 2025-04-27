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
using PFXToolKitUI.Avalonia.Services.Colours;
using PFXToolKitUI.Avalonia.Services.Messages.Controls;
using PFXToolKitUI.Avalonia.Services.Windowing;
using PFXToolKitUI.Avalonia.Shortcuts.Dialogs;
using PFXToolKitUI.Avalonia.Utils;
using PFXToolKitUI.Services.ColourPicking;
using PFXToolKitUI.Services.InputStrokes;
using PFXToolKitUI.Services.Messaging;
using PFXToolKitUI.Services.UserInputs;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.Avalonia.Services.UserInputs;

public partial class UserInputDialogView : WindowingContentControl {
    public static readonly SingleUserInputInfo DummySingleInput = new SingleUserInputInfo("Text Input Here") { Message = "A primary message here", ConfirmText = "Confirm", CancelText = "Cancel", Caption = "The caption here", Label = "The label here" };
    public static readonly DoubleUserInputInfo DummyDoubleInput = new DoubleUserInputInfo("Text A Here", "Text B Here") { Message = "A primary message here", ConfirmText = "Confirm", CancelText = "Cancel", Caption = "The caption here", LabelA = "Label A Here:", LabelB = "Label B Here:" };

    public static readonly ModelControlRegistry<UserInputInfo, Control> Registry;

    public static readonly StyledProperty<UserInputInfo?> UserInputInfoProperty = AvaloniaProperty.Register<UserInputDialogView, UserInputInfo?>("UserInputInfo");

    public UserInputInfo? UserInputInfo {
        get => this.GetValue(UserInputInfoProperty);
        set => this.SetValue(UserInputInfoProperty, value);
    }

    public Thickness ContentMargin {
        get => this.PART_InputFieldContent.Margin;
        set => this.PART_InputFieldContent.Margin = value;
    }

    /// <summary>
    /// Gets the dialog result for this user input dialog
    /// </summary>
    public bool? DialogResult { get; private set; }

    private readonly AvaloniaPropertyToDataParameterBinder<UserInputInfo> captionBinder = new AvaloniaPropertyToDataParameterBinder<UserInputInfo>(WindowTitleProperty, UserInputInfo.CaptionParameter);
    private readonly AvaloniaPropertyToDataParameterBinder<UserInputInfo> messageBinder = new AvaloniaPropertyToDataParameterBinder<UserInputInfo>(TextBlock.TextProperty, UserInputInfo.MessageParameter);
    private readonly AvaloniaPropertyToDataParameterBinder<UserInputInfo> confirmTextBinder = new AvaloniaPropertyToDataParameterBinder<UserInputInfo>(ContentProperty, UserInputInfo.ConfirmTextParameter);
    private readonly AvaloniaPropertyToDataParameterBinder<UserInputInfo> cancelTextBinder = new AvaloniaPropertyToDataParameterBinder<UserInputInfo>(ContentProperty, UserInputInfo.CancelTextParameter);

    public UserInputDialogView() {
        this.InitializeComponent();
        this.captionBinder.AttachControl(this);
        this.messageBinder.AttachControl(this.PART_Message);
        this.confirmTextBinder.AttachControl(this.PART_ConfirmButton);
        this.cancelTextBinder.AttachControl(this.PART_CancelButton);
        this.PART_Message.PropertyChanged += this.OnMessageTextBlockPropertyChanged;

        this.PART_ConfirmButton.Click += this.OnConfirmButtonClicked;
        this.PART_CancelButton.Click += this.OnCancelButtonClicked;
    }
    
    static UserInputDialogView() {
        Registry = new ModelControlRegistry<UserInputInfo, Control>();
        Registry.RegisterType<SingleUserInputInfo>(() => new SingleUserInputControl());
        Registry.RegisterType<DoubleUserInputInfo>(() => new DoubleUserInputControl());
        Registry.RegisterType<ColourUserInputInfo>(() => new ColourUserInputControl());
        Registry.RegisterType<KeyStrokeUserInputInfo>(() => new KeyStrokeUserInputControl());
        Registry.RegisterType<MouseStrokeUserInputInfo>(() => new MouseStrokeUserInputControl());
        UserInputInfoProperty.Changed.AddClassHandler<UserInputDialogView, UserInputInfo?>((o, e) => o.OnUserInputDataChanged(e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
    }

    protected override void OnWindowOpened() {
        base.OnWindowOpened();
        this.Window!.Control.AddHandler(KeyDownEvent, this.OnKeyDown, RoutingStrategies.Tunnel);
        this.Window.IsResizable = false;
    }

    protected override void OnWindowClosed() {
        base.OnWindowClosed();
    }
    
    protected override Size MeasureOverride(Size availableSize) {
        Size size = base.MeasureOverride(availableSize);
        size = new Size(size.Width + 2, Math.Max(size.Height, 43));
        return new Size(Math.Max(size.Width, 250), size.Height);
    }

    private void OnMessageTextBlockPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e) {
        if (e.Property == TextBlock.TextProperty) {
            this.PART_MessageContainer.IsVisible = !string.IsNullOrWhiteSpace(e.GetNewValue<string?>());
        }
    }
    
    private void OnKeyDown(object? sender, KeyEventArgs e) {
        if (!e.Handled && e.Key == Key.Escape) {
            this.TryCloseDialog(false);
        }
    }
    
    private void OnConfirmButtonClicked(object? sender, RoutedEventArgs e) => this.TryCloseDialog(true);

    private void OnCancelButtonClicked(object? sender, RoutedEventArgs e) => this.TryCloseDialog(false);

    private void OnUserInputDataChanged(UserInputInfo? oldData, UserInputInfo? newData) {
        if (oldData != null) {
            (this.PART_InputFieldContent.Content as IUserInputContent)?.Disconnect();
            this.PART_InputFieldContent.Content = null;
            oldData.HasErrorsChanged -= this.OnHasErrorsChanged;
        }

        // Create this first just in case there's a problem with no registrations
        Control? control = newData != null ? Registry.NewInstance(newData) : null;

        this.captionBinder.SwitchModel(newData);
        this.messageBinder.SwitchModel(newData);
        this.confirmTextBinder.SwitchModel(newData);
        this.cancelTextBinder.SwitchModel(newData);
        if (control != null) {
            newData!.HasErrorsChanged += this.OnHasErrorsChanged;
            this.PART_InputFieldContent.Content = control;
            control.ApplyStyling();
            control.ApplyTemplate();
            (control as IUserInputContent)?.Connect(this, newData);
        }

        this.UpdateAllErrors();
        Dispatcher.UIThread.InvokeAsync(() => {
            if ((this.PART_InputFieldContent.Content as IUserInputContent)?.FocusPrimaryInput() == true) {
                return;
            }

            if (this.UserInputInfo?.DefaultButton is bool boolean) {
                if (boolean) {
                    this.PART_ConfirmButton.Focus();
                }
                else {
                    this.PART_CancelButton.Focus();
                }
            }
        }, DispatcherPriority.Loaded);
    }

    private void OnHasErrorsChanged(UserInputInfo info) {
        this.PART_ConfirmButton.IsEnabled = !info.HasErrors();
    }

    /// <summary>
    /// Invokes the <see cref="UserInputInfo.UpdateAllErrors"/> on our user input info to forcefully
    /// update all errors, and then updates our confirm button
    /// </summary>
    public void UpdateAllErrors() {
        if (this.UserInputInfo is UserInputInfo info) {
            info.UpdateAllErrors();
            this.PART_ConfirmButton.IsEnabled = !info.HasErrors();
        }
        else {
            this.PART_ConfirmButton.IsEnabled = false;
        }
    }

    /// <summary>
    /// Tries to close the dialog
    /// </summary>
    /// <param name="result">The dialog result wanted</param>
    /// <returns>
    /// True if the dialog was closed (regardless of the dialog result),
    /// false if it could not be closed due to a validation error or other error
    /// </returns>
    public bool TryCloseDialog(bool result) {
        if (this.Window == null) {
            return false;
        }

        if (result) {
            UserInputInfo? data = this.UserInputInfo;
            if (data == null || data.HasErrors()) {
                return false;
            }

            this.Window.Close(this.DialogResult = true);
        }
        else {
            this.Window.Close(this.DialogResult = false);
        }

        return true;
    }
    
    /// <summary>
    /// Shows a new user input dialog using the given user information to create the content
    /// </summary>
    /// <param name="info">The input info</param>
    /// <returns>A task to await the dialog close result</returns>
    public static async Task<bool?> ShowDialogAsync(UserInputInfo info) {
        Validate.NotNull(info);
        
        if (WindowingSystem.TryGetInstance(out WindowingSystem? system)) {
            if (!system.TryGetActiveWindow(out IWindow? activeWindow)) {
                return false;
            }
            
            UserInputDialogView content = new UserInputDialogView() {
                UserInputInfo = info
            };
            
            IWindow dialog = system.CreateWindow(content);
            bool? result = await dialog.ShowDialog<bool?>(activeWindow);
            content.UserInputInfo = null; // unhook models' event handlers
            if (result == true && content.DialogResult == true) {
                return true;
            }
            return result;
        }
        else {
            await IMessageDialogService.Instance.ShowMessage("Desktop", "No windowing library available");
        }

        return null;
    }
}