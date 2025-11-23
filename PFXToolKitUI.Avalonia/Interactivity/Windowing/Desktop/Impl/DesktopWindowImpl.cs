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

using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using PFXToolKitUI.Avalonia.Shortcuts.Avalonia;
using PFXToolKitUI.Avalonia.Utils;
using PFXToolKitUI.Composition;
using PFXToolKitUI.Icons;
using PFXToolKitUI.Interactivity;
using PFXToolKitUI.Interactivity.Contexts;
using PFXToolKitUI.Interactivity.Windowing;
using PFXToolKitUI.Logging;
using PFXToolKitUI.Themes;
using PFXToolKitUI.Utils;
using PFXToolKitUI.Utils.Events;

namespace PFXToolKitUI.Avalonia.Interactivity.Windowing.Desktop.Impl;

/// <summary>
/// The implementation of <see cref="IDesktopWindow"/> for a <see cref="DesktopWindowManager"/>
/// </summary>
public sealed class DesktopWindowImpl : IDesktopWindow {
    private static readonly IEnumerable<Exception> s_EmptyEL = ReadOnlyCollection<Exception>.Empty;

    public IMutableContextData LocalContextData => DataManager.GetContextData(this.myNativeWindow);

    public ComponentStorage ComponentStorage { get; }

    public IWindowManager WindowManager => this.myManager;

    public IDesktopWindow? Owner => this.parentWindow;

    public IEnumerable<IDesktopWindow> OwnedWindows => this.myVisibleChildWindows; // this.myNativeWindow.OwnedWindows.Select(x => ((DesktopNativeWindow) x).Window);

    public bool IsMainWindow { get; }
    public OpenState OpenState { get; private set; }
    public bool IsDialog { get; private set; } // we set this after showing
    public bool IsActivated => this.myNativeWindow.IsActive;

    public WindowIcon? Icon {
        get => this.myNativeWindow.WindowIcon;
        set => PropertyHelper.SetAndRaiseINEEx(this.myNativeWindow, x => x.WindowIcon, (x, y) => x.WindowIcon = y, value, this, this.IconChanged);
    }

    public Icon? TitleBarIcon {
        get => this.myNativeWindow.TitleBarIcon;
        set => PropertyHelper.SetAndRaiseINEEx(this.myNativeWindow, x => x.TitleBarIcon, (x, y) => x.TitleBarIcon = y, value, this, this.TitleBarIconChanged);
    }

    public bool IsTitleBarVisible {
        get => this.myNativeWindow.IsTitleBarVisible;
        set => PropertyHelper.SetAndRaiseINEEx(this.myNativeWindow, x => x.IsTitleBarVisible, (x, y) => x.IsTitleBarVisible = y, value, this, this.IsTitleBarVisibleChanged);
    }

    public string? Title {
        get => this.myNativeWindow.Title;
        set => PropertyHelper.SetAndRaiseINEEx(this.myNativeWindow, x => x.Title, (x, y) => x.Title = y, value, this, this.TitleChanged);
    }

    public IColourBrush? TitleBarBrush {
        get => this.titleBarBrushHandler.Brush;
        set => PropertyHelper.SetAndRaiseINEEx(this.titleBarBrushHandler, x => x.Brush, (x, y) => x.Brush = y, value, this, this.TitleBarBrushChanged);
    }

    public IColourBrush? BorderBrush {
        get => this.borderBrushHandler.Brush;
        set => PropertyHelper.SetAndRaiseINEEx(this.borderBrushHandler, x => x.Brush, (x, y) => x.Brush = y, value, this, this.BorderBrushChanged);
    }

    public TextAlignment TitleBarTextAlignment {
        get => this.myNativeWindow.TitleBarTextAlignment;
        set => PropertyHelper.SetAndRaiseINEEx(this.myNativeWindow, x => x.TitleBarTextAlignment, (x, y) => x.TitleBarTextAlignment = y, value, this, this.TitleBarTextAlignmentChanged);
    }

    public object? Content {
        get => this.myNativeWindow.Content;
        set => this.myNativeWindow.Content = value;
    }

    public Interactive Control => this.myNativeWindow;

    public WindowSizingInfo SizingInfo { get; }

    public Size ActualSize => this.myNativeWindow.Bounds.Size;

    public PixelPoint Position {
        get => this.myNativeWindow.Position;
        set => this.myNativeWindow.Position = value;
    }

    public event EventHandler? Opening;
    public event EventHandler? Opened;
    public event EventHandler<WindowCancelCloseEventArgs>? TryClose;
    public event AsyncEventHandler<WindowCancelCloseEventArgs>? TryCloseAsync;
    public event EventHandler<WindowCloseEventArgs>? Closing;
    public event AsyncEventHandler<WindowCloseEventArgs>? ClosingAsync;
    public event EventHandler<WindowCloseEventArgs>? Closed;
    public event EventHandler<ValueChangedEventArgs<WindowIcon?>>? IconChanged;
    public event EventHandler<ValueChangedEventArgs<Icon?>>? TitleBarIconChanged;
    public event EventHandler? IsTitleBarVisibleChanged;
    public event EventHandler<ValueChangedEventArgs<string?>>? TitleChanged;
    public event EventHandler<ValueChangedEventArgs<IColourBrush?>>? TitleBarBrushChanged;
    public event EventHandler<ValueChangedEventArgs<IColourBrush?>>? BorderBrushChanged;
    public event EventHandler<ValueChangedEventArgs<TextAlignment>>? TitleBarTextAlignmentChanged;

    // owner+owned tracking
    internal readonly DesktopNativeWindow myNativeWindow;
    internal readonly DesktopWindowManager myManager;
    internal readonly DesktopWindowImpl? parentWindow;
    internal readonly List<DesktopWindowImpl> myVisibleChildWindows;

    // open+close operation tracking
    private TaskCompletionSource? tcsShowAsync;
    private TaskCompletionSource? tcsWaitForClosed;
    private readonly List<CancellableTaskCompletionSource> listTcsWaitForClosed;

    // utils for "binding" brushes from the BrushManager api
    private readonly ColourBrushHandler titleBarBrushHandler;
    private readonly ColourBrushHandler borderBrushHandler;

    internal bool internalIsProcessingClose;

    public DesktopWindowImpl(DesktopWindowManager myManager, DesktopWindowImpl? parentWindow, WindowBuilder builder) {
        Debug.Assert(ReferenceEquals(parentWindow, builder.Parent));
        this.myManager = myManager;
        this.parentWindow = parentWindow;
        this.myNativeWindow = new DesktopNativeWindow(this);
        this.myVisibleChildWindows = new List<DesktopWindowImpl>();
        this.listTcsWaitForClosed = new List<CancellableTaskCompletionSource>();
        this.ComponentStorage = new ComponentStorage(this);
        this.IsMainWindow = builder.MainWindow;
        this.SizingInfo = new WindowSizingInfo(this, builder);
        this.SizingInfo.DoubleValueChanged += OnSizingInfoPropertyChanged;
        this.SizingInfo.CanResizeChanged += static (s, _) => ((DesktopWindowImpl) ((WindowSizingInfo) s!).Window).myNativeWindow.CanResize = ((WindowSizingInfo) s!).CanResize;
        this.SizingInfo.SizeToContentChanged += static (s, e) => {
            DesktopWindowImpl window = (DesktopWindowImpl) ((WindowSizingInfo) s!).Window;
            if (!window.myNativeWindow.doNotModifySizeToContent) {
                window.myNativeWindow.SizeToContent = e.NewValue;
            }
        };

        this.titleBarBrushHandler = new ColourBrushHandler(DesktopNativeWindow.TitleBarBrushProperty);
        this.borderBrushHandler = new ColourBrushHandler(TemplatedControl.BorderBrushProperty);

        IClipboard? clip = this.myNativeWindow.Clipboard;
        if (clip != null) {
            this.ComponentStorage.AddComponent<IClipboardService>(new ClipboardServiceImpl(clip));
        }

        this.ComponentStorage.AddComponent<IWebLauncher>(new WebLauncherImpl(this.myNativeWindow.Launcher));

        this.myNativeWindow.IsToolWindow = builder.IsToolWindow;
        this.myNativeWindow.ShowTitleBarIcon = builder.ShowTitleBarIcon;
        this.myNativeWindow.CanResize = builder.CanResize;
        this.myNativeWindow.Builder_CanMinimize = builder.CanMinimize;
        this.myNativeWindow.Builder_CanMaximize = builder.CanMaximize;
        this.myNativeWindow.SizeToContent = builder.SizeToContent;
        this.ApplyBuilderSizing(builder);

        this.IsTitleBarVisible = builder.IsTitleBarVisible;
        this.Icon = builder.Icon.HasValue ? builder.Icon.Value : this.myManager.GetDefaultWindowIcon();
        this.TitleBarIcon = builder.TitleBarIcon;
        this.Title = builder.Title ?? "Window";
        this.TitleBarBrush = builder.TitleBarBrush;
        this.BorderBrush = builder.BorderBrush;
        this.Content = builder.Content;

        UIInputManager.SetFocusPath(this.myNativeWindow, builder.FocusPath);

        DataManager.GetContextData(this.myNativeWindow).Set(ITopLevel.TopLevelDataKey, this);
    }

    private void ApplyBuilderSizing(WindowBuilder builder) {
        if (builder.MinWidth is double d1)
            this.myNativeWindow.MinWidth = d1;
        if (builder.MinHeight is double d2)
            this.myNativeWindow.MinHeight = d2;
        if (builder.MaxWidth is double d3)
            this.myNativeWindow.MaxWidth = d3;
        if (builder.MaxHeight is double d4)
            this.myNativeWindow.MaxHeight = d4;
        if (builder.Width is double d5)
            this.myNativeWindow.Width = d5;
        if (builder.Height is double d6)
            this.myNativeWindow.Height = d6;
    }

    private static void OnSizingInfoPropertyChanged(object? o, string propertyName) {
        WindowSizingInfo sender = (WindowSizingInfo) o!;
        DesktopNativeWindow window = ((DesktopWindowImpl) sender.Window).myNativeWindow;
        switch (propertyName) {
            case nameof(WindowSizingInfo.MinWidth):  window.SetValue(Layoutable.MinWidthProperty, sender.MinWidth ?? AvaloniaProperty.UnsetValue); break;
            case nameof(WindowSizingInfo.MinHeight): window.SetValue(Layoutable.MinHeightProperty, sender.MinHeight ?? AvaloniaProperty.UnsetValue); break;
            case nameof(WindowSizingInfo.MaxWidth):  window.SetValue(Layoutable.MaxWidthProperty, sender.MaxWidth ?? AvaloniaProperty.UnsetValue); break;
            case nameof(WindowSizingInfo.MaxHeight): window.SetValue(Layoutable.MaxHeightProperty, sender.MaxHeight ?? AvaloniaProperty.UnsetValue); break;
            case nameof(WindowSizingInfo.Width):     window.SetValue(Layoutable.WidthProperty, sender.Width ?? AvaloniaProperty.UnsetValue); break;
            case nameof(WindowSizingInfo.Height):    window.SetValue(Layoutable.HeightProperty, sender.Height ?? AvaloniaProperty.UnsetValue); break;
            default:                                 return;
        }
    }

    internal void OnNativeWindowOpening() {
        if (this.OpenState != OpenState.NotOpened)
            throw new InvalidOperationException("Window has already started to open");

        this.OpenState = OpenState.Opening;
        this.IsDialog = this.myNativeWindow.IsDialog;
        this.myManager.OnWindowOpening(this);
        this.Opening?.Invoke(this, EventArgs.Empty);
    }

    internal void OnNativeWindowOpened() {
        if (this.OpenState != OpenState.Opening)
            throw new InvalidOperationException("Window has not started to open yet");

        this.OpenState = OpenState.Open;
        this.AllocateResourcesForOpening();
        this.Opened?.Invoke(this, EventArgs.Empty);
        this.myManager.OnWindowOpened(this);
        this.tcsShowAsync?.TrySetResult();
        this.tcsShowAsync = null;
    }

    internal WindowCancelCloseEventArgs OnNativeWindowClosing(WindowCloseReason reason, bool isFromCode) {
        if (this.internalIsProcessingClose)
            throw new InvalidOperationException("Reentry");
        if (this.OpenState != OpenState.Open && this.OpenState != OpenState.TryingToClose)
            throw new InvalidOperationException("Window is not in its normal open state");

        try {
            this.internalIsProcessingClose = true;
            this.OpenState = OpenState.TryingToClose;
            WindowCancelCloseEventArgs cancelCloseArgs = new WindowCancelCloseEventArgs(this, reason, isFromCode);
            foreach (DesktopWindowImpl window in this.myVisibleChildWindows.ToList()) {
                window.myNativeWindow.Close();
            }

            if (this.myVisibleChildWindows.Count > 0) {
                cancelCloseArgs.SetCancelled();
            }
            else {
                this.TryClose?.Invoke(this, cancelCloseArgs);

                try {
                    this.myManager.myFrameManager.AwaitForCompletion(AsyncEventUtils.InvokeAsync(this.TryCloseAsync, this, cancelCloseArgs, ignoreCancelled: true));
                }
                catch (AggregateException e) {
                    List<Exception> errors = e.InnerExceptions.Where(x => !(x is OperationCanceledException)).ToList();
                    if (errors.Count > 0) {
                        Debugger.Break();
                        AppLogger.Instance.WriteLine("Failed to invoke one or more handlers to " + nameof(this.TryCloseAsync));
                        foreach (Exception ex in errors) {
                            AppLogger.Instance.WriteLine(ex.GetToString());
                        }
                    }
                }
            }

            if (cancelCloseArgs.IsCancelled) {
                this.OpenState = OpenState.Open;
                return cancelCloseArgs;
            }

            this.OpenState = OpenState.Closing;
            WindowCloseEventArgs closingArgs = new WindowCloseEventArgs(this, reason, isFromCode);
            this.Closing?.Invoke(this, closingArgs);

            try {
                this.myManager.myFrameManager.AwaitForCompletion(AsyncEventUtils.InvokeAsync(this.ClosingAsync, this, closingArgs, ignoreCancelled: true));
            }
            catch (AggregateException e) {
                Debugger.Break();
                AppLogger.Instance.WriteLine("Failed to invoke one or more handlers to " + nameof(this.TryCloseAsync));
                foreach (Exception ex in e.InnerExceptions) {
                    AppLogger.Instance.WriteLine(ex.GetToString());
                }
            }

            return cancelCloseArgs;
        }
        finally {
            this.internalIsProcessingClose = false;
        }
    }

    internal void OnNativeWindowClosed(WindowCloseReason reason, bool isFromCode) {
        if (this.OpenState != OpenState.Closing)
            throw new InvalidOperationException("Window is not in the closing state");

        this.OpenState = OpenState.Closed;
        this.DisposeResourcesForClosed();

        this.myManager.OnWindowClosed(this);

        try {
            this.Closed?.Invoke(this, new WindowCloseEventArgs(this, reason, isFromCode));
        }
        finally {
            this.tcsWaitForClosed?.TrySetResult();
            this.tcsWaitForClosed = null;

            if (this.listTcsWaitForClosed.Count > 0) {
                foreach (CancellableTaskCompletionSource tcs in this.listTcsWaitForClosed) {
                    tcs.TrySetResult();
                    tcs.Dispose();
                }

                this.listTcsWaitForClosed.Clear();
            }
        }
    }

    private void AllocateResourcesForOpening() {
        this.titleBarBrushHandler.SetTarget(this.myNativeWindow);
        this.borderBrushHandler.SetTarget(this.myNativeWindow);
        this.borderBrushHandler.BrushChanged += this.OnBorderBrushChanged;
    }

    private void DisposeResourcesForClosed() {
        this.titleBarBrushHandler.SetTarget(null);
        this.borderBrushHandler.SetTarget(null);
    }

    private void OnBorderBrushChanged(object? o, ValueChangedEventArgs<IColourBrush?> e) {
        this.myNativeWindow.BorderThickness = e.NewValue == null ? default : new Thickness(1);
    }

    public bool TryGetTopLevel([NotNullWhen(true)] out TopLevel? topLevel) {
        topLevel = this.myNativeWindow;
        return true;
    }

    public Task ShowAsync() {
        ApplicationPFX.Instance.Dispatcher.VerifyAccess();

        if (this.tcsShowAsync != null) {
            Debug.Assert(this.OpenState == OpenState.Opening);

            Debugger.Break();
            AppLogger.Instance.WriteLine("Warning: call to " + nameof(this.ShowAsync) + " when already in the process of showing");
            return this.tcsShowAsync.Task;
        }

        if (this.OpenState == OpenState.Closed) // clarity exception message
            throw new InvalidOperationException("Window has been closed; it cannot be opened again.");
        if (this.OpenState != OpenState.NotOpened)
            throw new InvalidOperationException("Window has already been opened");

        this.tcsShowAsync = new TaskCompletionSource();
        ApplicationPFX.Instance.Dispatcher.Post(() => {
            try {
                DesktopNativeWindow? parent = this.parentWindow?.myNativeWindow;
                if (parent == null)
                    this.myNativeWindow.Show();
                else
                    this.myNativeWindow.Show(parent);
            }
            catch (Exception e) {
                this.tcsShowAsync.SetException(new Exception("Failed to show window", e));
                this.tcsShowAsync = null;
                throw;
            }
        });

        return this.tcsShowAsync.Task;
    }

    public Task<object?> ShowDialogAsync() {
        ApplicationPFX.Instance.Dispatcher.VerifyAccess();

        if (this.OpenState == OpenState.Closed) // clarity exception message
            throw new InvalidOperationException("Window has been closed; it cannot be opened again.");
        if (this.OpenState != OpenState.NotOpened)
            throw new InvalidOperationException("Window has already been opened");

        if (this.tcsShowAsync != null)
            throw new InvalidOperationException("Cannot show as dialog because we are currently in the process of showing as a non-dialog window");

        DesktopNativeWindow? parent = this.parentWindow?.myNativeWindow;
        if (parent == null)
            throw new InvalidOperationException("Cannot show as dialog because we have no parent window");

        return this.myNativeWindow.ShowDialog<object?>(parent);
    }

    private void CheckStateForRequestClose() {
        switch (this.OpenState) {
            case < OpenState.Open:        throw new InvalidOperationException("Window has not fully opened yet, it cannot be requested to close yet.");
            case OpenState.TryingToClose: throw new InvalidOperationException("Window has already been requested to close");
            case OpenState.Closing:       throw new InvalidOperationException("Window is already in the process of closing");
            case OpenState.Closed:        throw new InvalidOperationException("Window has already been closed");
        }

        Debug.Assert(this.OpenState == OpenState.Open);
    }

    public void RequestClose(object? dialogResult = null) {
        ApplicationPFX.Instance.Dispatcher.VerifyAccess();
        this.CheckStateForRequestClose();
        this.OpenState = OpenState.TryingToClose;

        ApplicationPFX.Instance.Dispatcher.Post(() => this.myNativeWindow.Close(dialogResult));
    }

    public async Task<bool> RequestCloseAsync(object? dialogResult = null) {
        ApplicationPFX.Instance.Dispatcher.VerifyAccess();
        this.CheckStateForRequestClose();
        this.OpenState = OpenState.TryingToClose;

        using CancellationTokenSource cts = new CancellationTokenSource();
        ApplicationPFX.Instance.Dispatcher.Post(() => {
            try {
                // Due to how we implemented our OnClosing logic, Close() will block even
                // while waiting for async operations (TryCloseAsync) because we
                // use dispatcher frames.
                this.myNativeWindow.Close(dialogResult);
            }
            finally {
                try {
                    // ReSharper disable once AccessToDisposedClosure
                    // assert !cts.IsDisposed
                    cts.Cancel();
                }
                catch {
                    // ignored
                }
            }
        });

        // We either wait for the window to close, or for the CTS to become cancelled (due to cancelled window close).
        await this.WaitForClosedAsync(cts.Token);
        return this.OpenState == OpenState.Closed;
    }

    public async Task WaitForClosedAsync(CancellationToken cancellationToken = default) {
        ApplicationPFX.Instance.Dispatcher.VerifyAccess();

        if (this.OpenState != OpenState.Closed) {
            TaskCompletionSource tcs;
            if (cancellationToken.CanBeCanceled) {
                tcs = new CancellableTaskCompletionSource(cancellationToken);
                this.listTcsWaitForClosed.Add((CancellableTaskCompletionSource) tcs);
            }
            else {
                tcs = this.tcsWaitForClosed ??= new TaskCompletionSource();
            }

            try {
                await tcs.Task;
            }
            catch (OperationCanceledException) {
                // ignored
            }
        }
    }

    public void Activate() {
        ApplicationPFX.Instance.Dispatcher.VerifyAccess();
        this.myNativeWindow.Activate();
    }

    internal class WebLauncherImpl(ILauncher launcher) : IWebLauncher {
        public Task<bool> LaunchUriAsync(Uri uri) => launcher.LaunchUriAsync(uri);
    }

    internal class ClipboardServiceImpl(IClipboard clipboard) : IClipboardService {
        public async Task<string?> GetTextAsync() => await clipboard.GetTextAsync();
        public Task SetTextAsync(string? text) => clipboard.SetTextAsync(text);
        public Task ClearAsync() => clipboard.ClearAsync();
        public Task SetDataObjectAsync(IDataObjekt data) => clipboard.SetDataObjectAsync(((DataObjectWrapper) data).RawDataObject);
        public Task FlushAsync() => clipboard.FlushAsync();
        public async Task<string[]> GetFormatsAsync() => await clipboard.GetFormatsAsync();
        public async Task<object?> GetDataAsync(string format) => await clipboard.GetDataAsync(format);

        public async Task<IDataObjekt?> TryGetInProcessDataObjectAsync() {
            IDataObject? obj = await clipboard.TryGetInProcessDataObjectAsync();
            return obj != null ? new DataObjectWrapper(obj) : null;
        }
    }
}