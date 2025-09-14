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

using Avalonia.Controls;
using Avalonia.Layout;
using PFXToolKitUI.Avalonia.Interactivity.Windowing.DesktopImpl;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.Avalonia.Interactivity.Windowing;

public delegate void WindowSizingInfoChangedEventHandler(WindowSizingInfo sender);

public delegate void WindowSizeInfoDoubleValueChangedEventHandler(WindowSizingInfo sender, string propertyName, double? oldValue, double? newValue);

public delegate void WindowSizingInfoSizeToContentChangedEventHandler(WindowSizingInfo sender, SizeToContent oldSizeToContent, SizeToContent newSizeToContent);

/// <summary>
/// Specifies how a window should be sized. Note - windows do not update the properties of this class, it is one-way
/// </summary>
public sealed class WindowSizingInfo {
    public static readonly double DefaultMinWidth = Layoutable.MinWidthProperty.GetDefaultValue(typeof(DesktopNativeWindow));
    public static readonly double DefaultMinHeight = Layoutable.MinHeightProperty.GetDefaultValue(typeof(DesktopNativeWindow));
    public static readonly double DefaultMaxWidth = Layoutable.MaxWidthProperty.GetDefaultValue(typeof(DesktopNativeWindow));
    public static readonly double DefaultMaxHeight = Layoutable.MaxHeightProperty.GetDefaultValue(typeof(DesktopNativeWindow));
    public static readonly double DefaultWidth = Layoutable.WidthProperty.GetDefaultValue(typeof(DesktopNativeWindow));
    public static readonly double DefaultHeight = Layoutable.HeightProperty.GetDefaultValue(typeof(DesktopNativeWindow));

    private double? minWidth, minHeight;
    private double? maxWidth, maxHeight;
    private double? width, height;
    private bool canResize;
    private SizeToContent sizeToContent;

    public double? MinWidth {
        get => this.minWidth;
        set => PropertyHelper.SetAndRaiseINE(ref this.minWidth, value, this, static (t, o, n) => t.DoubleValueChanged?.Invoke(t, nameof(MinWidth), o, n));
    }

    public double? MinHeight {
        get => this.minHeight;
        set => PropertyHelper.SetAndRaiseINE(ref this.minHeight, value, this, static (t, o, n) => t.DoubleValueChanged?.Invoke(t, nameof(MinHeight), o, n));
    }

    public double? MaxWidth {
        get => this.maxWidth;
        set => PropertyHelper.SetAndRaiseINE(ref this.maxWidth, value, this, static (t, o, n) => t.DoubleValueChanged?.Invoke(t, nameof(MaxWidth), o, n));
    }

    public double? MaxHeight {
        get => this.maxHeight;
        set => PropertyHelper.SetAndRaiseINE(ref this.maxHeight, value, this, static (t, o, n) => t.DoubleValueChanged?.Invoke(t, nameof(MaxHeight), o, n));
    }

    public double? Width {
        get => this.width;
        set => PropertyHelper.SetAndRaiseINE(ref this.width, value, this, static (t, o, n) => t.DoubleValueChanged?.Invoke(t, nameof(Width), o, n));
    }

    public double? Height {
        get => this.height;
        set => PropertyHelper.SetAndRaiseINE(ref this.height, value, this, static (t, o, n) => t.DoubleValueChanged?.Invoke(t, nameof(Height), o, n));
    }

    /// <summary>
    /// Gets or sets if the window can be resized by the user. This also hides the maximize button, if used
    /// </summary>
    public bool CanResize {
        get => this.canResize;
        set => PropertyHelper.SetAndRaiseINE(ref this.canResize, value, this, static t => t.CanResizeChanged?.Invoke(t));
    }

    /// <summary>
    /// Gets or sets how the window should be sized based on its contents. The default is manual, which means the window
    /// initially takes up the framework default window size, unless our width/height or minimum analogues are set
    /// </summary>
    public SizeToContent SizeToContent {
        get => this.sizeToContent;
        set => PropertyHelper.SetAndRaiseINE(ref this.sizeToContent, value, this, static (t, o, n) => t.SizeToContentChanged?.Invoke(t, o, n));
    }

    public event WindowSizeInfoDoubleValueChangedEventHandler? DoubleValueChanged;
    public event WindowSizingInfoChangedEventHandler? CanResizeChanged;
    public event WindowSizingInfoSizeToContentChangedEventHandler? SizeToContentChanged;

    /// <summary>
    /// Gets the window that owns this window size info object
    /// </summary>
    public IWindow Window { get; }

    internal WindowSizingInfo(IWindow window, WindowBuilder builder) {
        this.Window = window;
        this.minWidth = builder.MinWidth;
        this.minHeight = builder.MinHeight;
        this.maxWidth = builder.MaxWidth;
        this.maxHeight = builder.MaxHeight;
        this.width = builder.Width;
        this.height = builder.Height;
        this.canResize = builder.CanResize;
        this.sizeToContent = builder.SizeToContent;
    }
}