﻿// 
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
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Layout;
using Avalonia.Platform;
using Avalonia.Threading;
using PFXToolKitUI.Avalonia.Interactivity;
using PFXToolKitUI.Avalonia.Themes.Controls;
using PFXToolKitUI.Avalonia.Utils;
using PFXToolKitUI.Interactivity;

namespace PFXToolKitUI.Avalonia.Services.Windowing;

/// <summary>
/// The base class for windows supported by a <see cref="WindowingSystem"/>
/// </summary>
public class DesktopWindow : WindowEx, IDesktopWindow {
    /// <summary>
    /// Gets whether the window is actually open or not. False when not opened or <see cref="IsClosed"/> is true
    /// </summary>
    public bool IsOpen { get; private set; }

    /// <summary>
    /// Gets whether the window has closed after being opened. False when <see cref="IsOpen"/> is true
    /// </summary>
    public bool IsClosed { get; private set; }

    /// <summary>
    /// Gets whether this window is open as a modal dialog
    /// </summary>
    public bool IsOpenAsDialog { get; private set; }

    public event DesktopWindowClosingAsyncEventHandler? WindowClosing;
    public event DesktopWindowClosedEventHandler? WindowClosed;

    private VisualLayerManager? PART_VisualLayerManager;
    private Panel? PART_TitleBarPanel;
    private Window? myPlacedCenteredToOverride;

    public IClipboardService? ClipboardService { get; }

    public DesktopWindow() : base() {
        this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        this.VerticalContentAlignment = VerticalAlignment.Stretch;
        this.HorizontalContentAlignment = HorizontalAlignment.Stretch;

        IClipboard? clip = this.Clipboard;
        this.ClipboardService = clip != null ? new ClipboardServiceImpl(clip) : null;
        DataManager.GetContextData(this).Set(IDesktopWindow.DataKey, this);
    }

    /// <summary>
    /// Sets this window to be placed centric to the given window. If we are not open yet, this will be processed when we become shown.
    /// </summary>
    /// <param name="window"></param>
    public void PlaceCenteredTo(Window window) {
        if (this.IsOpen) {
            Thickness vlmMargin = this.PART_VisualLayerManager!.Margin;
            Size size = this.Bounds.Size.Inflate(vlmMargin);
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


    public void Show(DesktopWindow? parent) {
        if (parent == null) {
            base.Show();
        }
        else {
            base.Show(parent);
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.PART_VisualLayerManager = e.NameScope.GetTemplateChild<VisualLayerManager>("PART_VisualLayerManager");
        this.PART_TitleBarPanel = e.NameScope.GetTemplateChild<Panel>("PART_TitleBarPanel");
    }

    protected virtual void OnOpenedCore() {
    }

    protected sealed override void OnOpened(EventArgs e) {
        if (this.IsClosed || this.IsOpen) {
            throw new InvalidOperationException($"Invalid state. IsClosed = {this.IsClosed}, IsOpen = {this.IsOpen}");
        }

        this.StopRendering();
        this.IsClosed = false;
        this.IsOpen = true;
        
        this.OnOpenedCore();
        base.OnOpened(e);
        this.InvalidateMeasure();
        this.InvalidateArrange();
        this.UpdateLayout();

        this.UpdatePlacement();

        this.StartRendering();

        Dispatcher.UIThread.Invoke(void () => {
            this.SizeToContent = SizeToContent.Manual;
            this.UpdatePlacement();
        }, DispatcherPriority.Loaded);
    }

    private void UpdatePlacement() {
        if (this.myPlacedCenteredToOverride != null) {
            this.PlaceCenteredTo(this.myPlacedCenteredToOverride);
            this.myPlacedCenteredToOverride = null;
        }
        else if (this.WindowStartupLocation == WindowStartupLocation.CenterOwner && this.Owner is Window owner) {
            this.PlaceCenteredTo(owner);
        }
    }

    protected override async Task<bool> OnClosingAsync(WindowCloseReason reason) {
        bool isCancelled = false;
        Delegate[]? handlers = this.WindowClosing?.GetInvocationList();
        if (handlers != null) {
            foreach (Delegate handler in handlers) {
                isCancelled |= await ((DesktopWindowClosingAsyncEventHandler) handler)(this, reason, isCancelled);
            }
        }

        return isCancelled;
    }

    protected override void OnClosed(EventArgs e) {
        if (this.IsClosed || !this.IsOpen) {
            throw new InvalidOperationException($"Invalid state. IsClosed = {this.IsClosed}, IsOpen = {this.IsOpen}");
        }

        this.IsOpen = false;
        this.IsClosed = true;

        base.OnClosed(e);

        this.WindowClosed?.Invoke(this);
    }

    public new Task<TResult> ShowDialog<TResult>(Window owner) {
        this.IsOpenAsDialog = true;
        return base.ShowDialog<TResult>(owner)!;
    }

    private class ClipboardServiceImpl : IClipboardService {
        private readonly IClipboard clipboard;

        public ClipboardServiceImpl(IClipboard clipboard) {
            this.clipboard = clipboard;
        }

        public async Task<string?> GetTextAsync() => await this.clipboard.GetTextAsync();

        public Task SetTextAsync(string? text) => this.clipboard.SetTextAsync(text);

        public Task ClearAsync() => this.clipboard.ClearAsync();

        public Task SetDataObjectAsync(IDataObjekt data) => this.clipboard.SetDataObjectAsync(((DataObjectWrapper) data).RawDataObject);

        public Task FlushAsync() => this.clipboard.FlushAsync();

        public async Task<string[]> GetFormatsAsync() => await this.clipboard.GetFormatsAsync();

        public async Task<object?> GetDataAsync(string format) => await this.clipboard.GetDataAsync(format);

        public async Task<IDataObjekt?> TryGetInProcessDataObjectAsync() {
            IDataObject? obj = await this.clipboard.TryGetInProcessDataObjectAsync();
            return obj != null ? new DataObjectWrapper(obj) : null;
        }
    }
}