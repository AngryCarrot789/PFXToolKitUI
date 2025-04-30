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
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Media;
using PFXToolKitUI.Avalonia.Interactivity;
using PFXToolKitUI.Avalonia.Themes.Controls;
using PFXToolKitUI.Avalonia.Utils;
using PFXToolKitUI.Interactivity;

namespace PFXToolKitUI.Avalonia.Services.Windowing;

public partial class DesktopWindowImpl : WindowEx, IWindow {
    StyledProperty<string?> IWindow.TitleProperty => WindowEx.TitleProperty;
    StyledProperty<TextAlignment> IWindow.TitleBarTextAlignmentProperty => WindowEx.TitleBarTextAlignmentProperty;
    StyledProperty<IBrush?> IWindow.TitleBarBrushProperty => WindowEx.TitleBarBrushProperty;
    StyledProperty<IBrush?> IWindow.BorderBrushProperty => WindowEx.BorderBrushProperty;

    public bool IsOpen { get; private set; }

    public bool IsClosed { get; private set; }

    public bool IsOpenAsDialog { get; private set; }

    public bool IsResizable {
        get => this.CanResize;
        set => this.CanResize = value;
    }

    public bool CanAutoSizeToContent { get; set; }

    ContentControl IWindow.Control => this;

    private readonly List<WindowClosingAsyncEventHandler> closingHandlers = new List<WindowClosingAsyncEventHandler>();

    public event WindowClosingAsyncEventHandler? WindowClosing {
        add {
            if (value != null) {
                lock (this.closingHandlers) {
                    if (!this.closingHandlers.Contains(value))
                        this.closingHandlers.Add(value);
                }
            }
        }
        remove {
            if (value != null) {
                lock (this.closingHandlers) {
                    this.closingHandlers.Remove(value);
                }
            }
        }
    }

    public event WindowClosedEventHandler? WindowClosed;

    private bool isCallingOnOpening, widthChangeDuringOpen, heightChangeDuringOpen;
    private VisualLayerManager? PART_VisualLayerManager;
    private Panel? PART_TitleBarPanel;

    public IClipboardService? ClipboardService { get; }

    public DesktopWindowImpl() {
        this.InitializeComponent();
        this.CanAutoSizeToContent = true;
        IClipboard? clip = this.Clipboard;
        this.ClipboardService = clip != null ? new ClipboardServiceImpl(clip) : null;
        using (var token = DataManager.GetContextData(this).BeginChange())
            token.Context.Set(ITopLevel.DataKey, this).Set(IWindow.WindowDataKey, this);
    }

    public DesktopWindowImpl(WindowingContentControl content) : this() {
        this.PART_Content.Content = content;
        // First we measure the initial minimum size the content takes up
        this.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        Size dsSize = content.DesiredSize;
        this.Width = dsSize.Width;
        this.Height = dsSize.Height;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.PART_VisualLayerManager = e.NameScope.GetTemplateChild<VisualLayerManager>("PART_VisualLayerManager");
        this.PART_TitleBarPanel = e.NameScope.GetTemplateChild<Panel>("PART_TitleBarPanel");
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change) {
        base.OnPropertyChanged(change);
        if (this.isCallingOnOpening) {
            if (change.Property == WidthProperty)
                this.widthChangeDuringOpen = true;
            else if (change.Property == HeightProperty)
                this.heightChangeDuringOpen = true;
        }
    }

    protected override void OnOpened(EventArgs e) {
        if (this.IsClosed || this.IsOpen) {
            throw new InvalidOperationException($"Invalid state. IsClosed = {this.IsClosed}, IsOpen = {this.IsOpen}");
        }

        base.OnOpened(e);

        this.StopRendering();
        this.IsClosed = false;
        this.IsOpen = true;

        this.isCallingOnOpening = true;
        ((WindowingContentControl) this.PART_Content.Content!).OnOpened(this);
        this.isCallingOnOpening = false;

        if (this.CanAutoSizeToContent) {
            SizeToContent sizeToContent = SizeToContent.Manual;
            if (!this.widthChangeDuringOpen)
                sizeToContent |= SizeToContent.Width;
            if (!this.heightChangeDuringOpen)
                sizeToContent |= SizeToContent.Height;

            this.SizeToContent = sizeToContent;
        }

        this.InvalidateMeasure();
        this.InvalidateArrange();
        this.UpdateLayout();

        if (this.WindowStartupLocation == WindowStartupLocation.CenterOwner && this.Owner is Window owner) {
            Thickness vtlMargin = this.PART_VisualLayerManager!.Margin;
            Size desiredSize = this.DesiredSize;
            desiredSize = new Size(
                desiredSize.Width + vtlMargin.Left + vtlMargin.Right,
                desiredSize.Height + vtlMargin.Top + vtlMargin.Bottom + this.PART_TitleBarPanel!.Height);

            Size ownerSize = owner.Bounds.Size;
            Size size = (ownerSize / 2) - (desiredSize / 2);
            this.Position = owner.Position + new PixelPoint((int) size.Width, (int) size.Height);
        }

        this.StartRendering();
    }

    protected override async Task<bool> OnClosingAsync(WindowCloseReason reason) {
        bool isCancelled = false;
        List<WindowClosingAsyncEventHandler> list;
        lock (this.closingHandlers) {
            list = this.closingHandlers.ToList();
        }

        foreach (WindowClosingAsyncEventHandler handler in list) {
            isCancelled |= await handler(this, reason, isCancelled);
        }

        return isCancelled;
    }

    protected override void OnClosed(EventArgs e) {
        if (this.IsClosed || !this.IsOpen) {
            throw new InvalidOperationException($"Invalid state. IsClosed = {this.IsClosed}, IsOpen = {this.IsOpen}");
        }

        base.OnClosed(e);
        this.IsOpen = false;
        this.IsClosed = true;

        if (this.PART_Content.Content is WindowingContentControl content) {
            content.OnClosed(this);
        }

        this.WindowClosed?.Invoke(this);
    }

    public async Task<TResult?> ShowDialog<TResult>(IWindow parent) {
        this.IsOpenAsDialog = true;
        return await base.ShowDialog<TResult?>((DesktopWindowImpl) parent);
    }

    public void Show(IWindow? parent) {
        if (parent == null) {
            base.Show();
        }
        else {
            base.Show((DesktopWindowImpl) parent);
        }
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