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
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using PFXToolKitUI.Avalonia.Bindings;
using PFXToolKitUI.Avalonia.Interactivity.Windowing;
using PFXToolKitUI.Avalonia.Services.Colours;
using PFXToolKitUI.Avalonia.Services.Messages.Controls;
using PFXToolKitUI.Avalonia.Shortcuts.Dialogs;
using PFXToolKitUI.Avalonia.Utils;
using PFXToolKitUI.Services.ColourPicking;
using PFXToolKitUI.Services.InputStrokes;
using PFXToolKitUI.Services.UserInputs;
using PFXToolKitUI.Themes;
using PFXToolKitUI.Utils;
using SkiaSharp;

namespace PFXToolKitUI.Avalonia.Services.UserInputs;

public partial class UserInputDialogView : UserControl {
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
    /// Gets the window we are opened in
    /// </summary>
    public IWindow? Window { get; private set; }

    private readonly IBinder<UserInputInfo> captionBinder = new EventUpdateBinder<UserInputInfo>(nameof(UserInputInfo.CaptionChanged), b => ((UserInputDialogView) b.Control).Window!.Title = b.Model.Caption);

    private readonly IBinder<UserInputInfo> messageBinder = new EventUpdateBinder<UserInputInfo>(nameof(UserInputInfo.MessageChanged), b => {
        b.Control.SetValue(IsVisibleProperty, !string.IsNullOrWhiteSpace(b.Model.Message));
        b.Control.SetValue(TextBlock.TextProperty, b.Model.Message);
    });

    private readonly IBinder<UserInputInfo> confirmTextBinder = new EventUpdateBinder<UserInputInfo>(nameof(UserInputInfo.ConfirmTextChanged), b => b.Control.SetValue(ContentProperty, b.Model.ConfirmText));
    private readonly IBinder<UserInputInfo> cancelTextBinder = new EventUpdateBinder<UserInputInfo>(nameof(UserInputInfo.CancelTextChanged), b => b.Control.SetValue(ContentProperty, b.Model.CancelText));

    public UserInputDialogView() {
        this.InitializeComponent();
        this.messageBinder.AttachControl(this.PART_Message);
        this.confirmTextBinder.AttachControl(this.PART_ConfirmButton);
        this.cancelTextBinder.AttachControl(this.PART_CancelButton);
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

    internal void OnWindowOpening(IWindow window) {
        this.Window = window;
        if (this.PART_InputFieldContent.Content is IUserInputContent content) {
            content.OnWindowOpening();
            PixelSize bounds = content.GetMinimumBounds();
            if (bounds.Width > 0)
                this.Window.SizingInfo.MinWidth = bounds.Width;
            if (bounds.Height > 0)
                this.Window.SizingInfo.MinHeight = bounds.Height;
        }
    }

    internal void OnWindowOpened(IWindow window) {
        this.captionBinder.AttachControl(this);
        if (this.PART_InputFieldContent.Content is IUserInputContent content) {
            content.OnWindowOpened();
        }
    }

    internal void OnWindowClosed(IWindow window) {
        this.captionBinder.DetachControl();
        if (this.PART_InputFieldContent.Content is IUserInputContent content) {
            content.OnWindowClosed();
        }

        this.Window = null;
    }

    protected override Size MeasureOverride(Size availableSize) {
        Size size = base.MeasureOverride(availableSize);
        size = new Size(size.Width + 2, Math.Max(size.Height, 43));
        return new Size(Math.Max(size.Width, 250), size.Height);
    }

    private void OnConfirmButtonClicked(object? sender, RoutedEventArgs e) => this.RequestClose(true);

    private void OnCancelButtonClicked(object? sender, RoutedEventArgs e) => this.RequestClose(false);

    private void OnUserInputDataChanged(UserInputInfo? oldData, UserInputInfo? newData) {
        if (oldData != null) {
            (this.PART_InputFieldContent.Content as IUserInputContent)?.Disconnect();
            this.PART_InputFieldContent.Content = null;
            oldData.HasErrorsChanged -= this.UpdateConfirmButton;
        }

        // Create this first just in case there's a problem with no registrations
        Control? control = newData != null ? Registry.NewInstance(newData) : null;

        this.captionBinder.SwitchModel(newData);
        this.messageBinder.SwitchModel(newData);
        this.confirmTextBinder.SwitchModel(newData);
        this.cancelTextBinder.SwitchModel(newData);
        if (control != null) {
            newData!.HasErrorsChanged += this.UpdateConfirmButton;
            this.PART_InputFieldContent.Content = control;
            control.ApplyStyling();
            control.ApplyTemplate();
            (control as IUserInputContent)?.Connect(this, newData);
        }

        this.DoUpdateAllErrors();
        Dispatcher.UIThread.Post(() => {
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

    private void UpdateConfirmButton(UserInputInfo info) {
        this.PART_ConfirmButton.IsEnabled = !info.HasErrors();
    }

    /// <summary>
    /// Invokes the <see cref="UserInputInfo.UpdateAllErrors"/> on our user input info to forcefully
    /// update all errors, and then updates our confirm button
    /// </summary>
    public void DoUpdateAllErrors() {
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
    public void RequestClose(bool result) {
        if (this.Window != null && this.Window.OpenState == OpenState.Open) {
            if (!result) {
                _ = this.Window!.RequestCloseAsync(BoolBox.False);
                return;
            }

            UserInputInfo? data = this.UserInputInfo;
            if (data == null || data.HasErrors()) {
                return;
            }

            _ = this.Window!.RequestCloseAsync(BoolBox.True);
        }
    }

    /// <summary>
    /// Shows a new user input dialog using the given user information to create the content
    /// </summary>
    /// <param name="info">The input info</param>
    /// <param name="parentWindow">The parent window for the dialog</param>
    /// <returns>A task to await the dialog close result</returns>
    public static async Task<bool?> ShowDialogAsync(UserInputInfo info, IWindow parentWindow) {
        ArgumentNullException.ThrowIfNull(info);
        ArgumentNullException.ThrowIfNull(parentWindow);
        
        UserInputDialogView view = new UserInputDialogView() {
            UserInputInfo = info
        };

        IWindow window = parentWindow.WindowManager.CreateWindow(new WindowBuilder() {
            Title = info.Caption,
            Parent = parentWindow,
            Content = view,
            SizeToContent = SizeToContent.WidthAndHeight,
            CanResize = false,
            TitleBarBrush = BrushManager.Instance.GetDynamicThemeBrush("ABrush.Tone4.Background.Static"),
            BorderBrush = BrushManager.Instance.CreateConstant(SKColors.DodgerBlue),
        });

        window.Control.AddHandler(KeyDownEvent, WindowOnKeyDown);
        window.WindowOpening += WindowOnWindowOpening;
        window.WindowOpened += WindowOnWindowOpened;
        window.WindowClosed += WindowOnWindowClosed;

        bool? result = await window.ShowDialogAsync() as bool?;
        view.UserInputInfo = null; // unhook models' event handlers
        return result;

        void WindowOnWindowOpening(IWindow s, EventArgs e) {
            s.SizingInfo.SizeToContent = SizeToContent.Manual;
            view.OnWindowOpening(s);
        }

        void WindowOnWindowOpened(IWindow s, EventArgs e) => view.OnWindowOpened(s);
        void WindowOnWindowClosed(IWindow s, WindowCloseEventArgs e) => view.OnWindowClosed(s);

        void WindowOnKeyDown(object? s, KeyEventArgs e) {
            if (!e.Handled && e.Key == Key.Escape && view.Window != null) {
                view.RequestClose(false);
            }
        }
    }

    /// <summary>
    /// Shows a new user input dialog using the given user information to create the content
    /// </summary>
    /// <param name="info">The input info</param>
    /// <returns>A task to await the dialog close result</returns>
    public static async Task<bool?> ShowDialogAsync(UserInputInfo info) {
        ArgumentNullException.ThrowIfNull(info);
        IWindow? parentWindow = WindowContextUtils.GetUsefulWindow();
        if (parentWindow == null) {
            return null;
        }

        return await ShowDialogAsync(info, parentWindow);
    }
}