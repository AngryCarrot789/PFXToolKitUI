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
using PFXToolKitUI.AdvancedMenuService;
using PFXToolKitUI.Avalonia.Interactivity.Contexts;
using PFXToolKitUI.Avalonia.Utils;
using PFXToolKitUI.Composition;
using PFXToolKitUI.Icons;
using PFXToolKitUI.Interactivity;
using PFXToolKitUI.Interactivity.Windowing;
using PFXToolKitUI.Logging;
using PFXToolKitUI.Themes;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.Avalonia.Interactivity.Windowing.DesktopImpl;

/// <summary>
/// The implementation of <see cref="IWindow"/> for a <see cref="DesktopWindowManager"/>
/// </summary>
public sealed class DesktopWindowImpl : IWindow {
    internal readonly DesktopNativeWindow myNativeWindow;
    internal readonly DesktopWindowManager myManager;
    internal readonly DesktopWindowImpl? parentWindow;
    internal readonly List<DesktopWindowImpl> myVisibleChildWindows;

    private TaskCompletionSource? tcsShowAsync;
    private TaskCompletionSource? tcsWaitForClosed;
    private readonly List<CancellableTaskCompletionSource> listTcsWaitForClosed;

    private bool isProcessingBeforeClosingEvents; // protect against reentrancy
    private bool isClosingSynchronously; // Close() called when already being processed

    public ComponentStorage ComponentStorage { get; }

    public IWindowManager WindowMyManager => this.myManager;

    public IWindow? Owner => this.parentWindow;

    public IEnumerable<IWindow> OwnedWindows => this.myVisibleChildWindows; // this.myNativeWindow.OwnedWindows.Select(x => ((DesktopNativeWindow) x).Window);

    public bool IsMainWindow { get; }
    public bool IsClosing { get; private set; }
    public bool IsClosed { get; private set; }
    public bool IsOpening { get; private set; }
    public bool IsOpen { get; private set; }
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

    public string? Title {
        get => this.myNativeWindow.Title;
        set => PropertyHelper.SetAndRaiseINEEx(this.myNativeWindow, x => x.Title, (x, y) => x.Title = y, value, this, (t, o, n) => t.TitleChanged?.Invoke(t, o, n));
    }

    public TopLevelMenuRegistry? Menu {
        get => this.myNativeWindow.Menu;
        set => PropertyHelper.SetAndRaiseINEEx(this.myNativeWindow, x => x.Menu, (x, y) => x.Menu = y, value, this, (t, o, n) => t.MenuChanged?.Invoke(t, o, n));
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

    public event WindowEventHandler? WindowOpening;
    public event WindowEventHandler? WindowOpened;
    public event WindowEventHandler<WindowCancelCloseEventArgs>? BeforeClosing;
    public event AsyncWindowEventHandler<WindowCancelCloseEventArgs>? BeforeClosingAsync;
    public event WindowEventHandler<WindowCloseEventArgs>? WindowClosing;
    public event AsyncWindowEventHandler<WindowCloseEventArgs>? WindowClosingAsync;
    public event WindowEventHandler<WindowCloseEventArgs>? WindowClosed;

    public event WindowIconChangedEventHandler? IconChanged;
    public event WindowTitleBarIconChangedEventHandler? TitleBarIconChanged;
    public event WindowTitleBarTitleChangedEventHandler? TitleChanged;
    public event WindowMenuChangedEventHandler? MenuChanged;
    public event WindowTitleBarBrushChangedEventHandler? TitleBarBrushChanged;
    public event WindowBorderBrushChangedEventHandler? BorderBrushChanged;
    public event WindowTitleBarTextAlignmentChangedEventHandler? TitleBarTextAlignmentChanged;

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
        this.SizingInfo.SizeToContentChanged += static (info, ov, nv) => {
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

        this.ComponentStorage.AddComponent(new WebLauncherImpl(this.myNativeWindow));

        this.myNativeWindow.ShowTitleBarIcon = builder.ShowTitleBarIcon;
        this.myNativeWindow.CanResize = builder.CanResize;
        this.myNativeWindow.SizeToContent = builder.SizeToContent;
        this.ApplyBuilderSizing(builder);

        this.Icon = builder.Icon.HasValue ? builder.Icon.Value : this.myManager.GetDefaultWindowIcon();
        this.TitleBarIcon = builder.TitleBarIcon;
        this.Title = builder.Title ?? "Window";
        this.TitleBarBrush = builder.TitleBarBrush;
        this.BorderBrush = builder.BorderBrush;
        this.Menu = builder.Menu;
        this.Content = builder.Content;

        using MultiChangeToken change = DataManager.GetContextData(this.myNativeWindow).BeginChange();
        change.Context.Set(IWindow.DataKey, this);
        change.Context.Set(ITopLevelComponentManager.DataKey, new DesktopTopLevelComponentManagerImpl(this));
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
        if (this.IsClosing)
            throw new InvalidOperationException("Already closing the window");
        if (this.IsClosed)
            throw new InvalidOperationException("Window is already closed");
        if (this.IsOpen)
            throw new InvalidOperationException("Window is already open");

        this.IsClosing = this.IsClosed = false;
        this.IsOpening = true;
        this.IsDialog = this.myNativeWindow.IsDialog;
        this.myManager.OnWindowOpening(this);
        this.WindowOpening?.Invoke(this, EventArgs.Empty);
    }

    internal void OnNativeWindowOpened() {
        if (this.IsClosing)
            throw new InvalidOperationException("Already closing the window");
        if (this.IsClosed)
            throw new InvalidOperationException("Window is already closed");
        if (this.IsOpen)
            throw new InvalidOperationException("Window is already open");

        this.IsOpen = true;
        this.IsOpening = false;
        this.AllocateResourcesForOpening();
        this.WindowOpened?.Invoke(this, EventArgs.Empty);
        this.myManager.OnWindowOpened(this);
        this.tcsShowAsync?.SetResult();
        this.tcsShowAsync = null;
    }

    internal WindowCancelCloseEventArgs OnNativeWindowClosing(WindowCloseReason reason, bool isFromCode) {
        if (this.IsClosing)
            throw new InvalidOperationException("Already closing the window");
        if (this.IsClosed)
            throw new InvalidOperationException("Window is already closed");
        if (!this.IsOpen)
            throw new InvalidOperationException("Window is not open");

        this.IsClosing = true;
        this.IsClosed = false;
        if (this.isProcessingBeforeClosingEvents) {
            Debug.Fail("Reentrancy of OnNativeWindowClosing");
            throw new InvalidOperationException("Reentrancy of OnNativeWindowClosing");
        }

        this.isProcessingBeforeClosingEvents = true;

        WindowCancelCloseEventArgs beforeClosingArgs = new WindowCancelCloseEventArgs(this, reason, isFromCode);
        this.BeforeClosing?.Invoke(this, beforeClosingArgs);

        Delegate[]? beforeClosingAsyncHandlers = this.BeforeClosingAsync?.GetInvocationList();
        if (beforeClosingAsyncHandlers != null && beforeClosingAsyncHandlers.Length > 0) {
            List<Task> tasks = new List<Task>();
            foreach (Delegate handler in beforeClosingAsyncHandlers) {
                tasks.Add(Task.Run(() => ((AsyncWindowEventHandler<WindowCancelCloseEventArgs>) handler)(this, beforeClosingArgs)));
            }

            if (tasks.Any(x => !x.IsCompleted)) {
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
        }

        this.isProcessingBeforeClosingEvents = false;

        if (beforeClosingArgs.IsCancelled) {
            this.IsClosing = false;
            return beforeClosingArgs;
        }

        WindowCloseEventArgs closingArgs = new WindowCloseEventArgs(this, reason, isFromCode);
        this.WindowClosing?.Invoke(this, closingArgs);
        Delegate[]? closingAsyncHandlers = this.WindowClosingAsync?.GetInvocationList();
        if (closingAsyncHandlers != null && closingAsyncHandlers.Length > 0) {
            List<Task> tasks = new List<Task>();
            foreach (Delegate handler in closingAsyncHandlers) {
                tasks.Add(Task.Run(() => ((AsyncWindowEventHandler<WindowCloseEventArgs>) handler)(this, closingArgs)));
            }

            if (tasks.Any(x => !x.IsCompleted)) {
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
        }

        return beforeClosingArgs;
    }

    internal void OnNativeWindowClosed(WindowCloseReason reason, bool isFromCode) {
        if (!this.IsClosing)
            throw new InvalidOperationException("Should be closing");
        if (this.IsClosed)
            throw new InvalidOperationException("Should not already be closed");
        if (!this.IsOpen)
            throw new InvalidOperationException("Window is not open");

        this.IsClosing = false;
        this.IsClosed = true;
        this.IsOpen = false;
        this.DisposeResourcesForClosed();

        this.myManager.OnWindowClosed(this);
        this.WindowClosed?.Invoke(this, new WindowCancelCloseEventArgs(this, reason, isFromCode));
        this.tcsWaitForClosed?.SetResult();
        this.tcsWaitForClosed = null;

        if (this.listTcsWaitForClosed.Count > 0) {
            foreach (CancellableTaskCompletionSource tcs in this.listTcsWaitForClosed) {
                tcs.SetResult();
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

    public void Show() {
        if (this.IsClosed)
            throw new InvalidOperationException("Cannot show an already closed window");
        if (this.IsClosing)
            throw new InvalidOperationException("Cannot show because we are currently closing");

        DesktopNativeWindow? parent = this.parentWindow?.myNativeWindow;
        if (parent == null)
            this.myNativeWindow.Show();
        else
            this.myNativeWindow.Show(parent);
    }

    public Task ShowAsync() {
        if (this.IsClosed)
            throw new InvalidOperationException("Cannot show an already closed window");
        if (this.IsClosing)
            throw new InvalidOperationException("Cannot show because we are currently closing");

        if (this.tcsShowAsync != null) {
            Debugger.Break();
            AppLogger.Instance.WriteLine("Warning: call to " + nameof(this.ShowAsync) + " when already in the process of showing");
            return this.tcsShowAsync.Task;
        }

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

    public Task<object?> ShowDialog() {
        if (this.IsClosed)
            throw new InvalidOperationException("Cannot show an already closed window");
        if (this.IsClosing)
            throw new InvalidOperationException("Cannot show because we are currently closing");
        if (this.tcsShowAsync != null)
            throw new InvalidOperationException("Cannot show as dialog because we are currently in the process of showing as a non-dialog window");

        DesktopNativeWindow? parent = this.parentWindow?.myNativeWindow;
        if (parent == null)
            throw new InvalidOperationException("Cannot show as dialog because we have no parent window");

        return this.myNativeWindow.ShowDialog<object?>(parent);
    }

    private const string ErrorMessage_ClosingWhenClosed = "Window is already closed. This is not a fatal error but closing a window " +
                                                          "multiple times is a logic/state bug, because the dialogResult given will be ignored";

    public bool Close(object? dialogResult = null) {
        if (this.IsClosed) {
            AppLogger.Instance.WriteLine(ErrorMessage_ClosingWhenClosed);
            Debug.Fail(ErrorMessage_ClosingWhenClosed);
            return true;
        }

        if (this.IsClosing)
            throw new InvalidOperationException("Window is currently closing");
        if (this.isClosingSynchronously)
            throw new InvalidOperationException("Reentrancy -- window is currently closing");

        this.isClosingSynchronously = true;
        this.myNativeWindow.Close(dialogResult);
        this.isClosingSynchronously = false;
        return this.IsClosed;
    }

    public async Task<bool> CloseAsync(object? dialogResult = null) {
        if (this.IsClosed) {
            AppLogger.Instance.WriteLine(ErrorMessage_ClosingWhenClosed);
            Debug.Fail(ErrorMessage_ClosingWhenClosed);
            return true;
        }

        if (this.IsClosing)
            throw new InvalidOperationException("Window is currently closing");

        if (!this.isClosingSynchronously) {
            using CancellationTokenSource cts = new CancellationTokenSource();
            ApplicationPFX.Instance.Dispatcher.Post(() => {
                try {
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

            await this.WaitForClosedAsync(cts.Token);
        }
        else {
            Debug.Fail("Non fatal bug but a very bad issue -- CloseAsync called from Close");
            await this.WaitForClosedAsync();
        }

        return this.IsClosed;
    }

    public Task WaitForClosedAsync(CancellationToken cancellationToken = default) {
        if (this.IsClosed)
            return Task.CompletedTask;

        TaskCompletionSource tcs;
        if (cancellationToken.CanBeCanceled) {
            tcs = new CancellableTaskCompletionSource(cancellationToken);
            this.listTcsWaitForClosed.Add((CancellableTaskCompletionSource) tcs);
        }
        else {
            tcs = this.tcsWaitForClosed ??= new TaskCompletionSource();
        }

        return tcs.Task;
    }

    public void Activate() => this.myNativeWindow.Activate();

    private class WebLauncherImpl(Window window) : IWebLauncher {
        public Task<bool> LaunchUriAsync(Uri uri) => window.Launcher.LaunchUriAsync(uri);
    }

    private class ClipboardServiceImpl(IClipboard clipboard) : IClipboardService {
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