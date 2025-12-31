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
using PFXToolKitUI.Avalonia.Interactivity.Windowing.Desktop.Impl;
using PFXToolKitUI.Utils.Events;

namespace PFXToolKitUI.Avalonia.Interactivity.Windowing.Desktop;

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

    public double? MinWidth {
        get => field;
        set => PropertyHelper.SetAndRaise(ref field, value, this, static t => t.DoubleValueChanged?.Invoke(t, nameof(MinWidth)));
    }

    public double? MinHeight {
        get => field;
        set => PropertyHelper.SetAndRaise(ref field, value, this, static t => t.DoubleValueChanged?.Invoke(t, nameof(MinHeight)));
    }

    public double? MaxWidth {
        get => field;
        set => PropertyHelper.SetAndRaise(ref field, value, this, static t => t.DoubleValueChanged?.Invoke(t, nameof(MaxWidth)));
    }

    public double? MaxHeight {
        get => field;
        set => PropertyHelper.SetAndRaise(ref field, value, this, static t => t.DoubleValueChanged?.Invoke(t, nameof(MaxHeight)));
    }

    public double? Width {
        get => field;
        set => PropertyHelper.SetAndRaise(ref field, value, this, static t => t.DoubleValueChanged?.Invoke(t, nameof(Width)));
    }

    public double? Height {
        get => field;
        set => PropertyHelper.SetAndRaise(ref field, value, this, static t => t.DoubleValueChanged?.Invoke(t, nameof(Height)));
    }

    /// <summary>
    /// Gets or sets if the window can be resized by the user. This also hides the maximize button, if used
    /// </summary>
    public bool CanResize {
        get => field;
        set => PropertyHelper.SetAndRaiseINE(ref field, value, this, this.CanResizeChanged);
    }

    /// <summary>
    /// Gets or sets how the window should be sized based on its contents. The default is manual, which means the window
    /// initially takes up the framework default window size, unless our width/height or minimum analogues are set
    /// </summary>
    public SizeToContent SizeToContent {
        get => field;
        set => PropertyHelper.SetAndRaiseINE(ref field, value, this, this.SizeToContentChanged);
    }

    public event EventHandler<string>? DoubleValueChanged;
    public event EventHandler? CanResizeChanged;
    public event EventHandler<ValueChangedEventArgs<SizeToContent>>? SizeToContentChanged;

    /// <summary>
    /// Gets the window that owns this window size info object
    /// </summary>
    public IDesktopWindow Window { get; }

    internal WindowSizingInfo(IDesktopWindow window, WindowBuilder builder) {
        this.Window = window;
        this.MinWidth = builder.MinWidth;
        this.MinHeight = builder.MinHeight;
        this.MaxWidth = builder.MaxWidth;
        this.MaxHeight = builder.MaxHeight;
        this.Width = builder.Width;
        this.Height = builder.Height;
        this.CanResize = builder.CanResize;
        this.SizeToContent = builder.SizeToContent;
    }
}