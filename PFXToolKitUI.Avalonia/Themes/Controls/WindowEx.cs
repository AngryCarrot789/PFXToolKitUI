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
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Threading;
using PFXToolKitUI.Avalonia.Utils;

namespace PFXToolKitUI.Avalonia.Themes.Controls;

public class WindowEx : Window {
    public static readonly StyledProperty<IBrush?> TitleBarBrushProperty = AvaloniaProperty.Register<WindowEx, IBrush?>(nameof(TitleBarBrush));
    public static readonly StyledProperty<TextAlignment> TitleBarTextAlignmentProperty = AvaloniaProperty.Register<WindowEx, TextAlignment>(nameof(TitleBarTextAlignment));

    public IBrush? TitleBarBrush {
        get => this.GetValue(TitleBarBrushProperty);
        set => this.SetValue(TitleBarBrushProperty, value);
    }

    public TextAlignment TitleBarTextAlignment {
        get => this.GetValue(TitleBarTextAlignmentProperty);
        set => this.SetValue(TitleBarTextAlignmentProperty, value);
    }

    // Override it here so that any window using WindowEx gets the automatic WindowEx style
    protected override Type StyleKeyOverride => typeof(WindowEx);
    
    private bool isWaitingForClose, isStillWaitingForClose, isCloseCancelledAsync, doFinalClose;
    private Button? PART_ButtonMinimize, PART_ButtonRestore, PART_ButtonMaximize, PART_ButtonClose;
    private DockPanel? PART_TitleBar;

    public WindowEx() : base() {
        if (AvUtils.TryGetService(out Win32PlatformOptions options)) {
            if (options.CompositionMode.Any(x => x == Win32CompositionMode.LowLatencyDxgiSwapChain)) {
                this.ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.PreferSystemChrome;
                this.ExtendClientAreaToDecorationsHint = true;
                this.ExtendClientAreaTitleBarHeightHint = -1;
                return;
            }
        }

        this.ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.SystemChrome;
        this.ExtendClientAreaToDecorationsHint = true;
        this.ExtendClientAreaTitleBarHeightHint = -1;
    }
    
    // [DllImport("user32.dll")]
    // private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
    // 
    // private const int WM_NCLBUTTONDOWN = 0xA1;
    // private const int HTCAPTION = 0x2;
    // 
    // // Call in a pointer down event.
    // public void BeginDrag(int hwnd)
    // {
    //     SendMessage(hwnd, WM_NCLBUTTONDOWN, HTCAPTION, 0);
    // }

    static WindowEx() {
    }

    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
        this.UpdateTitleBar();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change) {
        base.OnPropertyChanged(change);
        if (change.Property == WindowStateProperty || change.Property == CanResizeProperty) {
            this.UpdateTitleBar();
        }
    }

    private void UpdateTitleBar() {
        if (this.CanResize) {
            switch (this.WindowState) {
                case WindowState.Normal:
                    this.PART_TitleBar!.IsVisible = true;
                    this.PART_ButtonMaximize!.IsVisible = true;
                    this.PART_ButtonRestore!.IsVisible = false;
                break;
                case WindowState.Maximized:
                    this.PART_TitleBar!.IsVisible = true;
                    this.PART_ButtonMaximize!.IsVisible = false;
                    this.PART_ButtonRestore!.IsVisible = true;
                break;
                case WindowState.FullScreen: this.PART_TitleBar!.IsVisible = false; break;
                default:                     this.PART_TitleBar!.IsVisible = false; break;
            }
        }
        else {
            switch (this.WindowState) {
                case WindowState.Normal:
                    this.PART_TitleBar!.IsVisible = true;
                    this.PART_ButtonMaximize!.IsVisible = false;
                    this.PART_ButtonRestore!.IsVisible = false;
                break;
                case WindowState.Maximized:
                    this.PART_TitleBar!.IsVisible = true;
                    this.PART_ButtonMaximize!.IsVisible = false;
                    this.PART_ButtonRestore!.IsVisible = true;
                break;
                case WindowState.FullScreen: this.PART_TitleBar!.IsVisible = false; break;
                default:                     this.PART_TitleBar!.IsVisible = false; break;
            }
        }
    }

    protected sealed override void OnClosing(WindowClosingEventArgs e) {
        base.OnClosing(e);
        if (e.Cancel || this.doFinalClose) {
            return;
        }
    
        if (!this.isStillWaitingForClose) {
            if (this.isWaitingForClose) {
                // Someone tried to close the window during OnClosingAsync.
                e.Cancel = true;
            }
            else {
                this.OnClosingAsyncImpl(e.CloseReason);
    
                // If OnClosingAsync is still running, then we cancel the
                // close event, and we set a flag to close on task completed
                if (this.isWaitingForClose) {
                    this.isStillWaitingForClose = true;
                    e.Cancel = true;
                }
            }
        }
    
        this.isCloseCancelledAsync = false;
    }
    
    private async void OnClosingAsyncImpl(WindowCloseReason reason) {
        try {
            this.isWaitingForClose = true;
            this.isCloseCancelledAsync = await this.OnClosingAsync(reason);
        }
        catch (Exception e) {
            Debug.Fail(e.Message);
            Dispatcher.UIThread.Post(void () => ExceptionDispatchInfo.Throw(e));
        }
        finally {
            this.isWaitingForClose = false;
        }
    
        bool postClose = this.isStillWaitingForClose && !this.isCloseCancelledAsync;
        this.isStillWaitingForClose = false;
        this.isCloseCancelledAsync = false;
    
        if (postClose) {
            this.doFinalClose = true;
            Dispatcher.UIThread.Post(this.Close);
        }
    }
    
    /// <summary>
    /// Invoked when this window tries to close. This method supports full async
    /// </summary>
    /// <param name="reason">The close reason</param>
    /// <returns>True to cancel closing (do not close). False to allow the window to close (default value)</returns>
    protected virtual Task<bool> OnClosingAsync(WindowCloseReason reason) {
        return Task.FromResult(false);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        (this.PART_ButtonMinimize = e.NameScope.GetTemplateChild<Button>("PART_ButtonMinimize")).Click += OnMinimizeButtonClick;
        (this.PART_ButtonRestore = e.NameScope.GetTemplateChild<Button>("PART_ButtonRestore")).Click += OnRestoreButtonClick;
        (this.PART_ButtonMaximize = e.NameScope.GetTemplateChild<Button>("PART_ButtonMaximize")).Click += OnMaximizeButtonClick;
        (this.PART_ButtonClose = e.NameScope.GetTemplateChild<Button>("PART_ButtonClose")).Click += OnCloseButtonClick;
        this.PART_TitleBar = e.NameScope.GetTemplateChild<DockPanel>("PART_TitleBar");
    }

    private static void OnMinimizeButtonClick(object? sender, RoutedEventArgs e) {
        if (GetTopLevel(sender as Button) is WindowEx window)
            window.WindowState = WindowState.Minimized;
    }

    private static void OnRestoreButtonClick(object? sender, RoutedEventArgs e) {
        if (GetTopLevel(sender as Button) is WindowEx window)
            window.WindowState = WindowState.Normal;
    }

    private static void OnMaximizeButtonClick(object? sender, RoutedEventArgs e) {
        if (GetTopLevel(sender as Button) is WindowEx window)
            window.WindowState = WindowState.Maximized;
    }

    private static void OnCloseButtonClick(object? sender, RoutedEventArgs e) {
        if (GetTopLevel(sender as Button) is WindowEx window)
            window.Close();
    }
}