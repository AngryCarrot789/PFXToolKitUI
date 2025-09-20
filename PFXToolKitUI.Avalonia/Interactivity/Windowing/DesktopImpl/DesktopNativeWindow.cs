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
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Threading;
using PFXToolKitUI.AdvancedMenuService;
using PFXToolKitUI.Avalonia.AvControls;
using PFXToolKitUI.Avalonia.Themes.Converters;
using PFXToolKitUI.Avalonia.Utils;
using PFXToolKitUI.Icons;

namespace PFXToolKitUI.Avalonia.Interactivity.Windowing.DesktopImpl;

/// <summary>
/// A derived window type that is only used for the <see cref="DesktopWindowManager"/>
/// </summary>
public sealed class DesktopNativeWindow : Window {
    public static readonly StyledProperty<IBrush?> TitleBarBrushProperty = AvaloniaProperty.Register<DesktopNativeWindow, IBrush?>(nameof(TitleBarBrush));
    public static readonly StyledProperty<TextAlignment> TitleBarTextAlignmentProperty = AvaloniaProperty.Register<DesktopNativeWindow, TextAlignment>(nameof(TitleBarTextAlignment));

    public IBrush? TitleBarBrush {
        get => this.GetValue(TitleBarBrushProperty);
        set => this.SetValue(TitleBarBrushProperty, value);
    }

    public TextAlignment TitleBarTextAlignment {
        get => this.GetValue(TitleBarTextAlignmentProperty);
        set => this.SetValue(TitleBarTextAlignmentProperty, value);
    }

    protected override Type StyleKeyOverride => typeof(DesktopNativeWindow);
    
    public WindowIcon? WindowIcon {
        get => base.Icon;
        set => base.Icon = value;
    }

    public Icon? TitleBarIcon {
        get => this.titleBarIcon;
        set {
            if (!Equals(this.titleBarIcon, value)) {
                this.titleBarIcon = value;
                this.OnIconStateChanged(true, false);
            }
        }
    }

    public bool ShowTitleBarIcon {
        get => this.showTitleBarIcon;
        set {
            if (this.showTitleBarIcon != value) {
                this.showTitleBarIcon = value;
                this.OnIconStateChanged(false, false);
            }
        }
    }

    public bool IsToolWindow { get; set; }

    public TopLevelMenuRegistry? Menu { get; set; }

    public DesktopWindowImpl Window { get; }

    private VisualLayerManager? PART_VisualLayerManager;
    private Panel? PART_TitleBarPanel;
    private Button? PART_ButtonMinimize, PART_ButtonRestore, PART_ButtonMaximize, PART_ButtonClose;
    private Image? PART_IconImage;
    private IconControl? PART_IconControl;
    private DockPanel? PART_TitleBar;
    private DesktopNativeWindow? myPlacedCenteredToOverride;
    private WindowCloseReason closingReason;
    private bool isClosingFromCode;
    internal bool doNotModifySizeToContent, isFullyOpened;
    private bool showTitleBarIcon;
    private Icon? titleBarIcon;

    public DesktopNativeWindow(DesktopWindowImpl impl) {
        this.Window = impl;
        this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        this.VerticalContentAlignment = VerticalAlignment.Stretch;
        this.HorizontalContentAlignment = HorizontalAlignment.Stretch;
        this.AddHandler(WindowOpenedEvent, this.OnOpening, RoutingStrategies.Direct, handledEventsToo: true);

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

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.PART_VisualLayerManager = e.NameScope.GetTemplateChild<VisualLayerManager>("PART_VisualLayerManager");
        this.PART_TitleBarPanel = e.NameScope.GetTemplateChild<Panel>("PART_TitleBarPanel");
        (this.PART_ButtonMinimize = e.NameScope.GetTemplateChild<Button>("PART_ButtonMinimize")).Click += OnMinimizeButtonClick;
        (this.PART_ButtonRestore = e.NameScope.GetTemplateChild<Button>("PART_ButtonRestore")).Click += OnRestoreButtonClick;
        (this.PART_ButtonMaximize = e.NameScope.GetTemplateChild<Button>("PART_ButtonMaximize")).Click += OnMaximizeButtonClick;
        (this.PART_ButtonClose = e.NameScope.GetTemplateChild<Button>("PART_ButtonClose")).Click += OnCloseButtonClick;
        this.PART_TitleBar = e.NameScope.GetTemplateChild<DockPanel>("PART_TitleBar");
        this.PART_IconImage = e.NameScope.GetTemplateChild<Image>("PART_IconImage");
        this.PART_IconControl = e.NameScope.GetTemplateChild<IconControl>("PART_IconControl");
        this.PART_IconControl.Icon = this.TitleBarIcon;
        this.UpdateTitleBarButtonVisibility();
        this.OnIconStateChanged(null, null);
        if (this.IsToolWindow) {
            this.PART_TitleBarPanel.Height = 24;
            this.ExtendClientAreaTitleBarHeightHint = 24;
            this.PART_ButtonMinimize.Width = 27;
            this.PART_ButtonRestore.Width = 27;
            this.PART_ButtonMaximize.Width = 27;
            this.PART_ButtonClose.Width = 27;
        }
    }

    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
        this.UpdateTitleBarButtonVisibility();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change) {
        base.OnPropertyChanged(change);
        if (this.PART_TitleBar != null && (change.Property == WindowStateProperty || change.Property == CanResizeProperty)) {
            this.UpdateTitleBarButtonVisibility();
        }

        if (change.Property == IconProperty) {
            this.OnIconStateChanged(false, true);
        }
    }

    /// <summary>
    /// Sets this window to be placed centric to the given window. If we are not open yet, this will be processed when we become shown.
    /// </summary>
    /// <param name="window"></param>
    public void PlaceCenteredTo(DesktopNativeWindow window) {
        if (this.isFullyOpened) {
            // Thickness vlmMargin = this.PART_VisualLayerManager!.Margin;
            Size size = this.Bounds.Size.Inflate(default);
            Size parentSize = window.Bounds.Size;
            double x = (parentSize.Width - size.Width) / 2;
            double y = (parentSize.Height - size.Height) / 2;
            PixelPoint newPosition = window.Position + new PixelPoint((int) x, (int) y);

            Screen? screen = this.Screens.ScreenFromWindow(window) ?? this.Screens.ScreenFromPoint(window.Position);
            if (screen != null) {
                PixelRect constraint = screen.WorkingArea;
                double maxX = constraint.Right - size.Width;
                double maxY = constraint.Bottom - size.Height;
                if (constraint.X <= maxX)
                    newPosition = newPosition.WithX((int) Math.Floor(Math.Clamp(newPosition.X, constraint.X, maxX)));
                if (constraint.Y <= maxY)
                    newPosition = newPosition.WithY((int) Math.Floor(Math.Clamp(newPosition.Y, constraint.Y, maxY)));
            }

            this.Position = newPosition;
        }
        else {
            this.myPlacedCenteredToOverride = window;
        }
    }

    private void UpdatePlacement() {
        if (this.myPlacedCenteredToOverride != null) {
            this.PlaceCenteredTo(this.myPlacedCenteredToOverride);
            this.myPlacedCenteredToOverride = null;
        }
        else if (this.WindowStartupLocation == WindowStartupLocation.CenterOwner && this.Window.parentWindow != null) {
            this.PlaceCenteredTo(this.Window.parentWindow.myNativeWindow);
        }
    }

    private void OnOpening(object? sender, RoutedEventArgs e) {
        this.doNotModifySizeToContent = true;
        this.Window.OnNativeWindowOpening();
    }

    protected override void OnOpened(EventArgs e) {
        // this.StopRendering();

        base.OnOpened(e);
        this.Window.OnNativeWindowOpened();

        // this.InvalidateMeasure();
        // this.InvalidateArrange();
        // this.UpdateLayout();

        // this.UpdatePlacement();
        // this.StartRendering();

        Dispatcher.UIThread.Invoke(void () => {
            this.isFullyOpened = true;
            this.doNotModifySizeToContent = false;
            this.SizeToContent = this.Window.SizingInfo.SizeToContent;
            this.UpdatePlacement();
        }, DispatcherPriority.Loaded);
    }

    protected override void OnClosing(WindowClosingEventArgs e) {
        base.OnClosing(e);
        WindowCancelCloseEventArgs args = this.Window.OnNativeWindowClosing(this.closingReason = e.CloseReason, this.isClosingFromCode = e.IsProgrammatic);
        if (e.Cancel && !args.IsCancelled) {
            Debug.Fail("!!!");
        }

        e.Cancel = args.IsCancelled;
    }

    protected override void OnClosed(EventArgs e) {
        base.OnClosed(e);
        this.isFullyOpened = false;
        this.Window.OnNativeWindowClosed(this.closingReason, this.isClosingFromCode);
    }

    private void OnIconStateChanged(bool? pfxIconChange, bool? winIconChange) {
        if (this.ShowTitleBarIcon) {
            if (this.PART_IconControl != null && this.TitleBarIcon != null) {
                if (!pfxIconChange.HasValue || pfxIconChange.Value || this.PART_IconControl.Icon == null)
                    this.PART_IconControl.Icon = this.TitleBarIcon;
                
                this.PART_IconImage!.IsVisible = false;
                this.PART_IconControl!.IsVisible = true;
                return;
            }
            else if (this.PART_IconImage != null && this.WindowIcon != null) {
                if (!winIconChange.HasValue || winIconChange.Value || this.PART_IconImage.Source == null)
                    this.PART_IconImage.Source = IconImageConverter.WindowIconToBitmap(this.WindowIcon);
                
                this.PART_IconControl!.IsVisible = false;
                this.PART_IconImage!.IsVisible = true;
                return;
            }
        }

        if (this.PART_IconControl != null)
            this.PART_IconControl!.IsVisible = false;
        if (this.PART_IconImage != null)
            this.PART_IconImage!.IsVisible = false;
    }

    private void UpdateTitleBarButtonVisibility() {
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

    private static void OnMinimizeButtonClick(object? sender, RoutedEventArgs e) {
        if (GetTopLevel(sender as Button) is DesktopNativeWindow window)
            window.WindowState = WindowState.Minimized;
    }

    private static void OnRestoreButtonClick(object? sender, RoutedEventArgs e) {
        if (GetTopLevel(sender as Button) is DesktopNativeWindow window)
            window.WindowState = WindowState.Normal;
    }

    private static void OnMaximizeButtonClick(object? sender, RoutedEventArgs e) {
        if (GetTopLevel(sender as Button) is DesktopNativeWindow window)
            window.WindowState = WindowState.Maximized;
    }

    private static void OnCloseButtonClick(object? sender, RoutedEventArgs e) {
        if (GetTopLevel(sender as Button) is DesktopNativeWindow window)
            window.Close();
    }
}