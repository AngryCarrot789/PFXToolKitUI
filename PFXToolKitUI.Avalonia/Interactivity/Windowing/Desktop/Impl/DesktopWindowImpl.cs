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

namespace PFXToolKitUI.Avalonia.Interactivity.Windowing.Desktop.Impl;

/// <summary>
/// The implementation of <see cref="IDesktopWindow"/> for a <see cref="DesktopWindowManager"/>
/// </summary>
public sealed class DesktopWindowImpl : IDesktopWindow {
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
        set => PropertyHelper.SetAndRaiseINEEx(this.myNativeWindow, x => x.WindowIcon, (x, y) => x.WindowIcon = y, value, this, (t, o, n) => t.IconChanged?.Invoke(t, o, n));
    }
    
    public Icon? TitleBarIcon {
        get => this.myNativeWindow.TitleBarIcon;
        set => PropertyHelper.SetAndRaiseINEEx(this.myNativeWindow, x => x.TitleBarIcon, (x, y) => x.TitleBarIcon = y, value, this, (t, o, n) => t.TitleBarIconChanged?.Invoke(t, o, n));
    }

    public bool IsTitleBarVisible {
        get => this.myNativeWindow.IsTitleBarVisible;
        set => PropertyHelper.SetAndRaiseINEEx(this.myNativeWindow, x => x.IsTitleBarVisible, (x, y) => x.IsTitleBarVisible = y, value, this, t => t.IsTitleBarVisibleChanged?.Invoke(t, EventArgs.Empty));
    }

    public string? Title {
        get => this.myNativeWindow.Title;
        set => PropertyHelper.SetAndRaiseINEEx(this.myNativeWindow, x => x.Title, (x, y) => x.Title = y, value, this, (t, o, n) => t.TitleChanged?.Invoke(t, o, n));
    }

    public IColourBrush? TitleBarBrush {
        get => this.titleBarBrushHandler.Brush;
        set => PropertyHelper.SetAndRaiseINEEx(this.titleBarBrushHandler, x => x.Brush, (x, y) => x.Brush = y, value, this, (t, o, n) => t.TitleBarBrushChanged?.Invoke(t, o, n));
    }

    public IColourBrush? BorderBrush {
        get => this.borderBrushHandler.Brush;
        set => PropertyHelper.SetAndRaiseINEEx(this.borderBrushHandler, x => x.Brush, (x, y) => x.Brush = y, value, this, (t, o, n) => t.BorderBrushChanged?.Invoke(t, o, n));
    }

    public TextAlignment TitleBarTextAlignment {
        get => this.myNativeWindow.TitleBarTextAlignment;
        set => PropertyHelper.SetAndRaiseINEEx(this.myNativeWindow, x => x.TitleBarTextAlignment, (x, y) => x.TitleBarTextAlignment = y, value, this, (t, o, n) => t.TitleBarTextAlignmentChanged?.Invoke(t, o, n));
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

    public event WindowEventHandler? Opening;
    public event WindowEventHandler? Opened;
    public event WindowEventHandler<WindowCancelCloseEventArgs>? TryClose;
    public event AsyncWindowEventHandler<WindowCancelCloseEventArgs>? TryCloseAsync;
    public event WindowEventHandler<WindowCloseEventArgs>? Closing;
    public event AsyncWindowEventHandler<WindowCloseEventArgs>? ClosingAsync;
    public event WindowEventHandler<WindowCloseEventArgs>? Closed;
    public event WindowIconChangedEventHandler? IconChanged;
    public event WindowTitleBarIconChangedEventHandler? TitleBarIconChanged;
    public event WindowEventHandler? IsTitleBarVisibleChanged;
    public event WindowTitleBarCaptionChangedEventHandler? TitleChanged;
    public event WindowTitleBarBrushChangedEventHandler? TitleBarBrushChanged;
    public event WindowBorderBrushChangedEventHandler? BorderBrushChanged;
    public event WindowTitleBarTextAlignmentChangedEventHandler? TitleBarTextAlignmentChanged;
    
    // owner+owned tracking
    internal readonly DesktopNativeWindow myNativeWindow;
    internal readonly DesktopWindowManager myManager;
    internal readonly DesktopWindowImpl? parentWindow;
    internal readonly List<DesktopWindowImpl> myVisibleChildWindows;

    // open+close operation tracking
    private TaskCompletionSource? tcsShowAsync;
    private TaskCompletionSource? tcsWaitForClosed;
    private readonly List<CancellableTaskCompletionSource> listTcsWaitForClosed;
    private bool isProcessingClosingInternal;
    
    // utils for "binding" brushes from the BrushManager api
    private readonly ColourBrushHandler titleBarBrushHandler;
    private readonly ColourBrushHandler borderBrushHandler;

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
        this.SizingInfo.CanResizeChanged += static info => ((DesktopWindowImpl) info.Window).myNativeWindow.CanResize = info.CanResize;
        this.SizingInfo.SizeToContentChanged += static (info, _, nv) => {
            DesktopWindowImpl window = (DesktopWindowImpl) info.Window;
            if (!window.myNativeWindow.doNotModifySizeToContent) {
                window.myNativeWindow.SizeToContent = nv;
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

    private static void OnSizingInfoPropertyChanged(WindowSizingInfo sender, string propertyName, double? oldValue, double? newValue) {
        DesktopNativeWindow window = ((DesktopWindowImpl) sender.Window).myNativeWindow;
        StyledProperty<double> property;
        switch (propertyName) {
            case nameof(WindowSizingInfo.MinWidth):  property = Layoutable.MinWidthProperty; break;
            case nameof(WindowSizingInfo.MinHeight): property = Layoutable.MinHeightProperty; break;
            case nameof(WindowSizingInfo.MaxWidth):  property = Layoutable.MaxWidthProperty; break;
            case nameof(WindowSizingInfo.MaxHeight): property = Layoutable.MaxHeightProperty; break;
            case nameof(WindowSizingInfo.Width):     property = Layoutable.WidthProperty; break;
            case nameof(WindowSizingInfo.Height):    property = Layoutable.HeightProperty; break;
            default:                                 return;
        }

        window.SetValue(property, newValue ?? AvaloniaProperty.UnsetValue);
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
        // this.requestToCloseTask != null || this.isProcessingClosingInternal
        if (this.isProcessingClosingInternal)
            throw new InvalidOperationException("Reentrancy of " + nameof(this.OnNativeWindowClosing));
        
        // set to TryingToClose by RequestCloseAsync
        if (this.OpenState != OpenState.Open && this.OpenState != OpenState.TryingToClose)
            throw new InvalidOperationException("Window is not in its normal open state");

        this.OpenState = OpenState.TryingToClose;
        this.isProcessingClosingInternal = true;
        WindowCancelCloseEventArgs beforeClosingArgs = new WindowCancelCloseEventArgs(this, reason, isFromCode);
        this.TryClose?.Invoke(this, beforeClosingArgs);

        Delegate[]? beforeClosingAsyncHandlers = this.TryCloseAsync?.GetInvocationList();
        if (beforeClosingAsyncHandlers != null && beforeClosingAsyncHandlers.Length > 0) {
            List<Task> tasks = new List<Task>();
            foreach (Delegate handler in beforeClosingAsyncHandlers) {
                tasks.Add(Task.Run(() => ((AsyncWindowEventHandler<WindowCancelCloseEventArgs>) handler)(this, beforeClosingArgs)));
            }

            try {
                ApplicationPFX.Instance.Dispatcher.AwaitForCompletion(Task.WhenAll(tasks));
            }
            catch (AggregateException ex) {
                List<Exception> errors = ex.InnerExceptions.Where(x => !(x is OperationCanceledException)).ToList();
                if (errors.Count == 1)
                    throw new Exception("Exception invoking async Closing handler", errors[0]);
                if (errors.Count > 0)
                    throw new AggregateException("Exception invoking multiple async Closing handler", errors);
            }
        }

        this.isProcessingClosingInternal = false;
        if (beforeClosingArgs.IsCancelled) {
            this.OpenState = OpenState.Open;
            return beforeClosingArgs;
        }

        this.OpenState = OpenState.Closing;
        WindowCloseEventArgs closingArgs = new WindowCloseEventArgs(this, reason, isFromCode);
        this.Closing?.Invoke(this, closingArgs);
        Delegate[]? closingAsyncHandlers = this.ClosingAsync?.GetInvocationList();
        if (closingAsyncHandlers != null && closingAsyncHandlers.Length > 0) {
            List<Task> tasks = new List<Task>();
            foreach (Delegate handler in closingAsyncHandlers) {
                tasks.Add(Task.Run(() => ((AsyncWindowEventHandler<WindowCloseEventArgs>) handler)(this, closingArgs)));
            }

            try {
                ApplicationPFX.Instance.Dispatcher.AwaitForCompletion(Task.WhenAll(tasks));
            }
            catch (AggregateException ex) {
                List<Exception> errors = ex.InnerExceptions.Where(x => !(x is OperationCanceledException)).ToList();
                if (errors.Count == 1)
                    throw new Exception("Exception invoking async Closing handler", errors[0]);
                if (errors.Count > 0)
                    throw new AggregateException("Exception invoking multiple async Closing handler", errors);
            }
        }

        return beforeClosingArgs;
    }

    internal void OnNativeWindowClosed(WindowCloseReason reason, bool isFromCode) {
        if (this.OpenState != OpenState.Closing)
            throw new InvalidOperationException("Window is not in the closing state");

        this.OpenState = OpenState.Closed;
        this.DisposeResourcesForClosed();

        this.myManager.OnWindowClosed(this);
        this.Closed?.Invoke(this, new WindowCloseEventArgs(this, reason, isFromCode));
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

    private void AllocateResourcesForOpening() {
        this.titleBarBrushHandler.SetTarget(this.myNativeWindow);
        this.borderBrushHandler.SetTarget(this.myNativeWindow);
        this.borderBrushHandler.BrushChanged += this.OnBorderBrushChanged;
    }

    private void DisposeResourcesForClosed() {
        this.titleBarBrushHandler.SetTarget(null);
        this.borderBrushHandler.SetTarget(null);
    }

    private void OnBorderBrushChanged(ColourBrushHandler sender, IColourBrush? oldbrush, IColourBrush? newbrush) {
        this.myNativeWindow.BorderThickness = newbrush == null ? default : new Thickness(1);
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

    public async Task<bool> RequestCloseAsync(object? dialogResult = null) {
        ApplicationPFX.Instance.Dispatcher.VerifyAccess();

        switch (this.OpenState) {
            case < OpenState.Open:        throw new InvalidOperationException("Window has not fully opened yet, it cannot be requested to close yet.");
            case OpenState.TryingToClose: throw new InvalidOperationException("Window has already been requested to close");
            case OpenState.Closing:       throw new InvalidOperationException("Window is already in the process of closing");
            case OpenState.Closed:        throw new InvalidOperationException("Window has already been closed");
        }

        Debug.Assert(this.OpenState == OpenState.Open);
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