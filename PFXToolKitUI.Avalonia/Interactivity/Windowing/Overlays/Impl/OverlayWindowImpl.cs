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
using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Media;
using PFXToolKitUI.Avalonia.Interactivity.Windowing.Desktop.Impl;
using PFXToolKitUI.Avalonia.Utils;
using PFXToolKitUI.Composition;
using PFXToolKitUI.Interactivity;
using PFXToolKitUI.Interactivity.Contexts;
using PFXToolKitUI.Interactivity.Windowing;
using PFXToolKitUI.Logging;
using PFXToolKitUI.Themes;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.Avalonia.Interactivity.Windowing.Overlays.Impl;

public sealed class OverlayWindowImpl : IOverlayWindow {
    public ComponentStorage ComponentStorage { get; }

    public IMutableContextData LocalContextData => DataManager.GetContextData(this.myControl);

    public IOverlayWindowManager OverlayManager => this.myManager;

    public IOverlayWindow? Owner => this.myParent;

    public IEnumerable<IOverlayWindow> OwnedPopups => this.myOwnedPopups;

    public OverlayWindowTitleBarInfo? TitleBarInfo { get; }

    public IColourBrush? BorderBrush {
        get => this.borderBrushHandler.Brush;
        set => PropertyHelper.SetAndRaiseINEEx(this.borderBrushHandler, x => x.Brush, (x, y) => x.Brush = y, value, this, (t, o, n) => t.BorderBrushChanged?.Invoke(t, o, n));
    }

    public object? Content {
        get => this.myControl.Content;
        set => this.myControl.Content = value;
    }

    public Interactive Control => this.myControl;

    public OpenState OpenState { get; private set; }

    public event OverlayWindowEventHandler? Opening;
    public event OverlayWindowEventHandler? Opened;
    public event OverlayWindowEventHandler<OverlayWindowCancelCloseEventArgs>? TryClose;
    public event AsyncOverlayWindowEventHandler<OverlayWindowCancelCloseEventArgs>? TryCloseAsync;
    public event OverlayWindowEventHandler<OverlayWindowCloseEventArgs>? Closing;
    public event AsyncOverlayWindowEventHandler<OverlayWindowCloseEventArgs>? ClosingAsync;
    public event OverlayWindowEventHandler<OverlayWindowCloseEventArgs>? Closed;
    public event OverlayWindowBorderBrushChangedEventHandler? BorderBrushChanged;

    internal readonly OverlayWindowManagerImpl myManager;
    internal readonly OverlayControl myControl;
    internal readonly OverlayWindowImpl? myParent;
    internal readonly List<OverlayWindowImpl> myOwnedPopups;
    internal readonly SizeToContent AutoSizeToContent;

    // open+close operation tracking
    private TaskCompletionSource? tcsShowAsync;
    private TaskCompletionSource? tcsWaitForClosed;
    private readonly List<CancellableTaskCompletionSource> listTcsWaitForClosed;
    private bool isProcessingClosingInternal;

    private object? myDialogResult;

    // utils for "binding" brushes from the BrushManager api
    private readonly ColourBrushHandler borderBrushHandler;

    public OverlayWindowImpl(OverlayWindowManagerImpl overlayManager, OverlayWindowImpl? parent, OverlayWindowBuilder builder) {
        this.myManager = overlayManager;
        this.myParent = parent;
        this.ComponentStorage = new ComponentStorage(this);
        this.myOwnedPopups = new List<OverlayWindowImpl>();
        this.listTcsWaitForClosed = new List<CancellableTaskCompletionSource>();
        this.myControl = new OverlayControl(this);
        this.borderBrushHandler = new ColourBrushHandler(TemplatedControl.BorderBrushProperty);
        this.TitleBarInfo = builder.TitleBar;
        this.Content = builder.Content;
        this.BorderBrush = builder.BorderBrush;

        if (builder.MinWidth is double minW)
            this.myControl.MinWidth = minW;
        if (builder.MinHeight is double minH)
            this.myControl.MinHeight = minH;
        if (builder.MaxWidth is double maxW)
            this.myControl.MaxWidth = maxW;
        if (builder.MaxHeight is double maxH)
            this.myControl.MaxHeight = maxH;
        if (builder.Width is double w)
            this.myControl.Width = w;
        if (builder.Height is double h)
            this.myControl.Height = h;

        this.AutoSizeToContent = builder.SizeToContent;

        DataManager.GetContextData(this.myControl).Set(ITopLevel.TopLevelDataKey, this);

        if (this.TryGetTopLevel(out TopLevel? topLevel)) {
            IClipboard? clipboard = topLevel.Clipboard;
            if (clipboard != null)
                this.ComponentStorage.AddComponent<IClipboardService>(new DesktopWindowImpl.ClipboardServiceImpl(clipboard));
            this.ComponentStorage.AddComponent<IWebLauncher>(new DesktopWindowImpl.WebLauncherImpl(topLevel.Launcher));
        }
    }

    public bool TryGetTopLevel([NotNullWhen(true)] out TopLevel? topLevel) {
        return this.myManager.TryGetTopLevel(out topLevel);
    }

    public Task ShowAsync() {
        ApplicationPFX.Instance.Dispatcher.VerifyAccess();
        
        if (this.OpenState != OpenState.NotOpened)
            throw new InvalidOperationException("Popup has already started to open");
        
        return ApplicationPFX.Instance.Dispatcher.InvokeAsync(this.ShowImpl);
    }

    public async Task<object?> ShowDialogAsync() {
        ApplicationPFX.Instance.Dispatcher.VerifyAccess();
        
        if (this.OpenState != OpenState.NotOpened)
            throw new InvalidOperationException("Popup has already started to open");
        
        await ApplicationPFX.Instance.Dispatcher.InvokeAsync(this.ShowImpl);

        await this.WaitForClosedAsync(CancellationToken.None);
        return this.myDialogResult;
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

        ApplicationPFX.Instance.Dispatcher.Post(CloseInternalAsync);
        return;

        void CloseInternalAsync() {
            Task task = this.CloseDialogImpl(dialogResult, WindowCloseReason.WindowClosing, true, false);

            // This may be a bad idea... but it makes tracking issues easier
            if (ApplicationPFX.Instance.Dispatcher.TryGetFrameManager(out IDispatcherFrameManager? frameManager)) {
                frameManager.AwaitForCompletion(task);
            }
        }
    }

    public async Task<bool> RequestCloseAsync(object? dialogResult = null) {
        ApplicationPFX.Instance.Dispatcher.VerifyAccess();
        this.CheckStateForRequestClose();
        this.OpenState = OpenState.TryingToClose;

        await ApplicationPFX.Instance.Dispatcher.InvokeAsync(() => this.CloseDialogImpl(dialogResult, WindowCloseReason.WindowClosing, true, false)).Unwrap();

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

    private void ShowImpl() {
        // Stage 1 - pre-show
        if (this.OpenState != OpenState.NotOpened)
            throw new InvalidOperationException("Popup has already started to open");

        this.OpenState = OpenState.Opening;
        this.myManager.OnPopupOpening(this);
        this.Opening?.Invoke(this, EventArgs.Empty);

        // Stage 2 - show in UI
        this.myManager.AddPopupToVisualTree(this);

        this.OpenState = OpenState.Open;
        this.AllocateResourcesForOpening();
        this.Opened?.Invoke(this, EventArgs.Empty);
        this.myManager.OnPopupOpened(this);
        this.tcsShowAsync?.SetResult();
        this.tcsShowAsync = null;
    }

    internal async Task CloseDialogImpl(object? dialogResult, WindowCloseReason reason, bool isFromCode, bool forced) {
        // this.requestToCloseTask != null || this.isProcessingClosingInternal
        if (this.isProcessingClosingInternal)
            throw new InvalidOperationException("Reentrancy of " + nameof(this.CloseDialogImpl));

        if (this.OpenState != OpenState.Open && this.OpenState != OpenState.TryingToClose)
            throw new InvalidOperationException("Popup is not in its normal open state");

        foreach (OverlayWindowImpl win in this.myOwnedPopups) {
            if (win.OpenState == OpenState.Open && !win.isProcessingClosingInternal) {
                await win.CloseDialogImpl(null, reason, isFromCode, forced);
                if (win.OpenState != OpenState.Closed && !forced) {
                    return;
                }
            }
        }

        // A child window may have closed this window for some reason, or the app shut down?
        if (this.OpenState != OpenState.Open && this.OpenState != OpenState.TryingToClose)
            return;

        if (!forced) {
            this.OpenState = OpenState.TryingToClose;
            this.isProcessingClosingInternal = true;
            OverlayWindowCancelCloseEventArgs beforeClosingArgs = new OverlayWindowCancelCloseEventArgs(this, reason, isFromCode);

            try {
                this.TryClose?.Invoke(this, beforeClosingArgs);
            }
            catch (Exception e) {
                Debugger.Break();
                AppLogger.Instance.WriteLine("Failed to invoke a handler to " + nameof(this.Closing));
                AppLogger.Instance.WriteLine(e.GetToString());
            }

            Delegate[]? beforeClosingAsyncHandlers = this.TryCloseAsync?.GetInvocationList();
            if (beforeClosingAsyncHandlers != null && beforeClosingAsyncHandlers.Length > 0) {
                List<Task> tasks = new List<Task>();
                foreach (Delegate handler in beforeClosingAsyncHandlers) {
                    tasks.Add(Task.Run(() => ((AsyncOverlayWindowEventHandler<OverlayWindowCancelCloseEventArgs>) handler)(this, beforeClosingArgs)));
                }

                await Task.WhenAll(tasks);

                // Get all task exceptions that are not primarily OCE
                List<Exception> errors = tasks.Select(x => x.Exception).NonNull().SelectMany(x => x.InnerExceptions.Where(y => !(y is OperationCanceledException))).ToList();
                if (errors.Count > 0) {
                    Debugger.Break();
                    AppLogger.Instance.WriteLine("Failed to invoke one or more handlers to " + nameof(this.TryCloseAsync));
                    foreach (Exception e in errors) {
                        AppLogger.Instance.WriteLine(e.GetToString());
                    }
                }
            }

            this.isProcessingClosingInternal = false;
            if (beforeClosingArgs.IsCancelled) {
                this.OpenState = OpenState.Open;
                return;
            }
        }

        this.myDialogResult = dialogResult;
        this.OpenState = OpenState.Closing;
        OverlayWindowCloseEventArgs closingArgs = new OverlayWindowCloseEventArgs(this, reason, isFromCode, forced);

        try {
            this.Closing?.Invoke(this, closingArgs);
        }
        catch (Exception e) {
            Debugger.Break();
            AppLogger.Instance.WriteLine("Failed to invoke a handler to " + nameof(this.Closing));
            AppLogger.Instance.WriteLine(e.GetToString());
        }

        Delegate[]? closingAsyncHandlers = this.ClosingAsync?.GetInvocationList();
        if (closingAsyncHandlers != null && closingAsyncHandlers.Length > 0) {
            List<Task> tasks = new List<Task>();
            foreach (Delegate handler in closingAsyncHandlers) {
                tasks.Add(Task.Run(() => ((AsyncOverlayWindowEventHandler<OverlayWindowCloseEventArgs>) handler)(this, closingArgs)));
            }

            await Task.WhenAll(tasks);

            // Get all task exceptions that are not primarily OCE
            List<Exception> errors = tasks.Select(x => x.Exception).NonNull().SelectMany(x => x.InnerExceptions.Where(y => !(y is OperationCanceledException))).ToList();
            if (errors.Count > 0) {
                Debugger.Break();
                AppLogger.Instance.WriteLine("Failed to invoke one or more handlers to " + nameof(this.ClosingAsync));
                foreach (Exception e in errors) {
                    AppLogger.Instance.WriteLine(e.GetToString());
                }
            }
        }

        // Remove this popup dialog from the UI
        this.myManager.RemovePopupFromVisualTree(this);

        this.OpenState = OpenState.Closed;
        this.DisposeResourcesForClosed();

        this.myManager.OnPopupClosed(this);
        this.Closed?.Invoke(this, closingArgs);
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
        this.borderBrushHandler.CurrentBrushChanged += this.OnBorderBrushChanged;
        this.borderBrushHandler.SetTarget(this.myControl);
    }

    private void DisposeResourcesForClosed() {
        this.borderBrushHandler.SetTarget(null);
    }

    private void OnBorderBrushChanged(ColourBrushHandler sender, IBrush? oldCurrentBrush, IBrush? newCurrentBrush) {
        this.myControl.BorderThickness = newCurrentBrush == null ? default : new Thickness(1);
    }
}