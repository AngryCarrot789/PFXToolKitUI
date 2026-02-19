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

using System.Diagnostics;
using System.Runtime.ExceptionServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using PFXToolKitUI.Avalonia.Bindings;
using PFXToolKitUI.Avalonia.Interactivity.Windowing;
using PFXToolKitUI.Avalonia.Interactivity.Windowing.Desktop;
using PFXToolKitUI.Avalonia.Interactivity.Windowing.Overlays;
using PFXToolKitUI.Avalonia.Services.Colours;
using PFXToolKitUI.Avalonia.Services.Messages.Controls;
using PFXToolKitUI.Avalonia.Shortcuts.Dialogs;
using PFXToolKitUI.Avalonia.Utils;
using PFXToolKitUI.Interactivity.Windowing;
using PFXToolKitUI.Services.ColourPicking;
using PFXToolKitUI.Services.InputStrokes;
using PFXToolKitUI.Services.Messaging;
using PFXToolKitUI.Services.UserInputs;
using PFXToolKitUI.Themes;
using PFXToolKitUI.Utils;
using SkiaSharp;

namespace PFXToolKitUI.Avalonia.Services.UserInputs;

public partial class UserInputDialogView : UserControl {
    public static readonly SingleUserInputInfo DummySingleInput = new SingleUserInputInfo() {
        Caption = "The caption here", Message = "A primary message here",
        Text = "Text Input Here",
        Label = "The label here",
        ConfirmText = "Confirm", CancelText = "Cancel"
    };

    public static readonly DoubleUserInputInfo DummyDoubleInput = new DoubleUserInputInfo() {
        Caption = "The caption here", Message = "A primary message here",
        TextA = "Text A Here", TextB = "Text B Here",
        LabelA = "Label A Here:", LabelB = "Label B Here:",
        ConfirmText = "Confirm", CancelText = "Cancel"
    };

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
    /// Gets the top level we exist in
    /// </summary>
    public IWindowBase? OwnerWindow { get; private set; }

    private readonly IBinder<UserInputInfo> captionBinder = new EventUpdateBinder<UserInputInfo>(nameof(UserInputInfo.CaptionChanged), b => {
        IWindowBase? owner = ((UserInputDialogView) b.Control).OwnerWindow;
        if (owner is IDesktopWindow dw)
            dw.Title = b.Model.Caption;
        if (owner is IOverlayWindow ow && ow.TitleBarInfo != null)
            ow.TitleBarInfo.Title = b.Model.Caption;
    });

    private readonly IBinder<UserInputInfo> messageBinder = new EventUpdateBinder<UserInputInfo>(nameof(UserInputInfo.MessageChanged), b => {
        b.Control.SetValue(IsVisibleProperty, !string.IsNullOrWhiteSpace(b.Model.Message));
        b.Control.SetValue(TextBlock.TextProperty, b.Model.Message);
    });

    private readonly IBinder<UserInputInfo> confirmTextBinder = new EventUpdateBinder<UserInputInfo>(nameof(UserInputInfo.ConfirmTextChanged), b => b.Control.SetValue(ContentProperty, b.Model.ConfirmText));
    private readonly IBinder<UserInputInfo> cancelTextBinder = new EventUpdateBinder<UserInputInfo>(nameof(UserInputInfo.CancelTextChanged), b => b.Control.SetValue(ContentProperty, b.Model.CancelText));
    private bool hasErrors, isAwaitingTryConfirm;
    private CancellationTokenSource? ctsTryConfirm;

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

    internal void OnWindowOpening(IWindowBase view) {
        this.OwnerWindow = view;
        if (this.OwnerWindow is IDesktopWindow window && this.PART_InputFieldContent.Content is IUserInputContent content) {
            content.OnWindowOpening();
            PixelSize bounds = content.GetMinimumBounds();
            if (bounds.Width > 0)
                window.SizingInfo.MinWidth = bounds.Width;
            if (bounds.Height > 0)
                window.SizingInfo.MinHeight = bounds.Height;
        }
    }

    internal void OnWindowOpened(IWindowBase view) {
        this.captionBinder.AttachControl(this);
        if (this.PART_InputFieldContent.Content is IUserInputContent content) {
            content.OnWindowOpened();
        }
    }

    internal void OnWindowClosed(IWindowBase view) {
        this.captionBinder.DetachControl();
        if (this.PART_InputFieldContent.Content is IUserInputContent content) {
            content.OnWindowClosed();
        }

        this.OwnerWindow = null;
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

        this.DoUpdateAllErrors();
        ApplicationPFX.Instance.Dispatcher.Post(() => {
            if ((this.PART_InputFieldContent.Content as IUserInputContent)?.FocusPrimaryInput() == true) {
                return;
            }

            if (newData != null) {
                switch (newData.DefaultButton) {
                    case UserInputInfo.ButtonType.None:    break;
                    case UserInputInfo.ButtonType.Confirm: this.PART_ConfirmButton.Focus(); break;
                    case UserInputInfo.ButtonType.Cancel:  this.PART_CancelButton.Focus(); break;
                }
            }
        }, DispatchPriority.Loaded);
    }

    private void OnHasErrorsChanged(object? sender, EventArgs eventArgs) {
        this.hasErrors = this.UserInputInfo!.HasErrors();
        this.UpdateIsConfirmEnabled();
    }

    /// <summary>
    /// Invokes the <see cref="UserInputInfo.UpdateAllErrors"/> on our user input info to forcefully
    /// update all errors, and then updates our confirm button
    /// </summary>
    public void DoUpdateAllErrors() {
        UserInputInfo? info = this.UserInputInfo;
        if (info != null) {
            info.UpdateAllErrors();
            this.hasErrors = info.HasErrors();
        }

        this.UpdateIsConfirmEnabled();
    }

    private void UpdateIsConfirmEnabled() {
        this.PART_ConfirmButton.IsEnabled = this.UserInputInfo != null && !this.hasErrors && !this.isAwaitingTryConfirm;
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
        // If anyone tries to close this view, cancel the try confirm callback
        this.ctsTryConfirm?.Cancel();

        if (this.OwnerWindow == null) {
            return;
        }

        if (this.OwnerWindow.OpenState != OpenState.Open) {
            return;
        }

        if (!result) {
            this.OwnerWindow.RequestClose(BoolBox.False);
            return;
        }

        // Force update errors in case there's a delay between input change and errors updated
        this.DoUpdateAllErrors();

        UserInputInfo? data = this.UserInputInfo;
        if (data == null) {
            if (this.OwnerWindow != null)
                Debug.Assert(this.OwnerWindow.OpenState == OpenState.Closing || this.OwnerWindow.OpenState == OpenState.Closed);
            return;
        }

        // Do nothing if there's errors present.
        if (data.HasErrors()) {
            return;
        }

        this.OwnerWindow.RequestClose(BoolBox.True);
    }

    private void BeforeAwaitTryConfirm() {
        this.isAwaitingTryConfirm = true;
        this.UpdateIsConfirmEnabled();
    }

    private void AfterAwaitTryConfirm() {
        this.isAwaitingTryConfirm = false;
        this.UpdateIsConfirmEnabled();
    }

    private static async Task<bool> RunTryConfirmAsync(UserInputInfo info, UserInputDialogView view) {
        Debug.Assert(view.ctsTryConfirm == null);

        view.ctsTryConfirm = new CancellationTokenSource();
        view.BeforeAwaitTryConfirm();

        try {
            return await info.TryConfirmAsync!(info, view.ctsTryConfirm.Token);
        }
        catch (OperationCanceledException) {
            return false;
        }
        catch (Exception exception) {
            if (!Debugger.IsAttached) {
                await IMessageDialogService.Instance.ShowExceptionMessage("Internal Error", "An error occurred while closing dialog", exception);
            }
            else {
                ApplicationPFX.Instance.Dispatcher.Post(static s => ((ExceptionDispatchInfo) s!).Throw(), ExceptionDispatchInfo.Capture(exception));
            }

            return false;
        }
        finally {
            view.AfterAwaitTryConfirm();

            bool isCloseRequested = view.ctsTryConfirm.IsCancellationRequested;
            view.ctsTryConfirm.Dispose();
            view.ctsTryConfirm = null;

            if (isCloseRequested) {
                ApplicationPFX.Instance.Dispatcher.Post(static (state) => {
                    UserInputDialogView v = (UserInputDialogView) state!;
                    if (v.OwnerWindow != null && v.OwnerWindow.OpenState == OpenState.Open) {
                        v.RequestClose(false);
                    }
                    
                    // Priority below Default is required, because async callbacks use Default.
                    // When RunTryConfirmAsync finishes, it posts the continuation to finalize
                    // the TryCloseAsync procedure, but this post callback here would run first
                    // without < Default priority, meaning OpenState will still be TryingToClose
                }, view, DispatchPriority.Input);
            }
        }
    }

    /// <summary>
    /// Shows a new user input dialog using the given user information to create the content
    /// </summary>
    /// <param name="info">The input info</param>
    /// <param name="parentWindow">The parent window for the dialog</param>
    /// <returns>A task to await the dialog close result</returns>
    public static async Task<bool?> ShowDialogWindowAsync(UserInputInfo info, IDesktopWindow parentWindow) {
        ArgumentNullException.ThrowIfNull(info);
        ArgumentNullException.ThrowIfNull(parentWindow);

        UserInputDialogView view = new UserInputDialogView() {
            UserInputInfo = info
        };

        IDesktopWindow window = parentWindow.WindowManager.CreateWindow(new WindowBuilder() {
            Title = info.Caption,
            Parent = parentWindow,
            Content = view,
            SizeToContent = SizeToContent.WidthAndHeight,
            CanResize = false,
            CanMinimize = false,
            CanMaximize = false,
            TitleBarBrush = BrushManager.Instance.GetDynamicThemeBrush("ABrush.Tone4.Background.Static"),
            BorderBrush = BrushManager.Instance.CreateConstant(SKColors.DodgerBlue)
        });

        window.Control.AddHandler(KeyDownEvent, (s, e) => {
            if (!e.Handled && e.Key == Key.Escape && view.OwnerWindow != null) {
                view.RequestClose(false);
            }
        });

        window.Opening += (s, e) => {
            window.SizingInfo.SizeToContent = SizeToContent.Manual;
            view.OnWindowOpening(window);
        };

        window.Opened += (s, e) => view.OnWindowOpened(window);
        window.Closed += (s, e) => view.OnWindowClosed(window);

        window.TryCloseAsync += (s, e) => ApplicationPFX.Instance.Dispatcher.InvokeAsync(async () => {
            if (e.DialogResult == BoolBox.True && info.TryConfirmAsync != null) {
                if (!await RunTryConfirmAsync(info, view)) {
                    e.SetCancelled();
                }
            }
        }).Unwrap();

        bool? result = await window.ShowDialogAsync() as bool?;
        view.UserInputInfo = null; // unhook models' event handlers
        return result;
    }

    public static async Task<bool?> ShowDialogOverlayAsync(UserInputInfo info, IOverlayWindowManager overlayManager, IOverlayWindow? parent) {
        ArgumentNullException.ThrowIfNull(info);
        ArgumentNullException.ThrowIfNull(overlayManager);

        UserInputDialogView view = new UserInputDialogView() {
            UserInputInfo = info
        };

        IOverlayWindow overlayWindow = overlayManager.CreateWindow(new OverlayWindowBuilder() {
            TitleBar = new OverlayWindowTitleBarInfo() {
                Title = info.Caption,
                TitleBarBrush = BrushManager.Instance.GetDynamicThemeBrush("ABrush.Tone4.Background.Static"),
            },
            Parent = parent,
            Content = view,
            BorderBrush = BrushManager.Instance.CreateConstant(SKColors.DodgerBlue),
            CloseOnLostFocus = true
        });

        overlayWindow.Control.AddHandler(KeyDownEvent, (s, e) => {
            if (!e.Handled && e.Key == Key.Escape && view.OwnerWindow != null) {
                view.RequestClose(false);
            }
        }); // just in case on desktop

        overlayWindow.Opening += (s, e) => view.OnWindowOpening(overlayWindow);
        overlayWindow.Opened += (s, e) => view.OnWindowOpened(overlayWindow);
        overlayWindow.Closed += (s, e) => view.OnWindowClosed(overlayWindow);

        overlayWindow.TryCloseAsync += (s, e) => ApplicationPFX.Instance.Dispatcher.InvokeAsync(async () => {
            if (e.DialogResult == BoolBox.True && info.TryConfirmAsync != null) {
                if (!await RunTryConfirmAsync(info, view)) {
                    e.SetCancelled();
                }
            }
        }).Unwrap();

        bool? result = await overlayWindow.ShowDialogAsync() as bool?;
        view.UserInputInfo = null; // unhook models' event handlers
        return result;
    }

    /// <summary>
    /// Shows a new user input dialog using the given user information to create the content
    /// </summary>
    /// <param name="info">The input info</param>
    /// <returns>A task to await the dialog close result</returns>
    public static async Task<bool?> ShowDialogWindowOrPopup(UserInputInfo info) {
        ITopLevel? parent = TopLevelContextUtils.GetTopLevelFromContext();
        return parent == null ? null : await ShowDialogWindowOrPopup(info, parent);
    }

    public static async Task<bool?> ShowDialogWindowOrPopup(UserInputInfo info, ITopLevel parent) {
        ArgumentNullException.ThrowIfNull(info);
        ArgumentNullException.ThrowIfNull(parent);

        // if (IOverlayWindowManager.TryGetInstance(out IOverlayWindowManager? instance)) {
        //     return await ShowDialogOverlayAsync(info, instance, instance.GetActiveWindow());
        // }

        Optional<bool?> window = await WindowContextUtils.UseWindowAsync(parent, w => ShowDialogWindowAsync(info, w), (m, w) => ShowDialogOverlayAsync(info, m, w));
        return window.GetValueOrDefault();
    }
}