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
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using PFXToolKitUI.Avalonia.Icons;
using PFXToolKitUI.Icons;

namespace PFXToolKitUI.Avalonia.AvControls;

/// <summary>
/// A control which presents an <see cref="PFXToolKitUI.Icons.Icon"/>
/// </summary>
public class IconControl : Control, IIconButton {
    private static readonly RenderOptions s_AliasRenderOptions = new RenderOptions() { EdgeMode = EdgeMode.Aliased, BitmapInterpolationMode = BitmapInterpolationMode.HighQuality };
    public static readonly StyledProperty<Icon?> IconProperty = AvaloniaProperty.Register<IconControl, Icon?>(nameof(Icon));
    public static readonly StyledProperty<StretchMode> StretchProperty = AvaloniaProperty.Register<IconControl, StretchMode>(nameof(Stretch), StretchMode.UniformNoUpscale);
    public static readonly StyledProperty<bool> UseBoundsHitTestProperty = AvaloniaProperty.Register<IconControl, bool>(nameof(UseBoundsHitTest), true);

    /// <summary>
    /// Gets or sets the icon we use for drawing this control
    /// </summary>
    public Icon? Icon {
        get => this.GetValue(IconProperty);
        set => this.SetValue(IconProperty, value);
    }
    
    public StretchMode Stretch {
        get => this.GetValue(StretchProperty);
        set => this.SetValue(StretchProperty, value);
    }
    
    /// <summary>
    /// Gets or sets if the icon should be hit-test visible by its visual bounds rather than
    /// rendering data. This is implemented by drawing a transparent background before the icon itself
    /// </summary>
    public bool UseBoundsHitTest {
        get => this.GetValue(UseBoundsHitTestProperty);
        set => this.SetValue(UseBoundsHitTestProperty, value);
    }

    public double? IconMaxWidth { get; set; }
    
    public double? IconMaxHeight { get; set; }

    private bool isAttachedToVt;
    private AbstractAvaloniaIcon? attachedIcon;

    public IconControl() {
    }

    static IconControl() {
        IconProperty.Changed.AddClassHandler<IconControl, Icon?>((d, e) => d.OnIconChanged(e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
        AffectsMeasure<IconControl>(IconProperty, StretchProperty);
        AffectsRender<IconControl>(IconProperty, StretchProperty);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e) {
        base.OnAttachedToVisualTree(e);
        this.DetachIcon(); // just in case
        if (this.Icon is AbstractAvaloniaIcon icon) {
            this.attachedIcon = icon;
            icon.RenderInvalidated += this.OnIconInvalidated;
        }

        this.isAttachedToVt = true;
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e) {
        base.OnDetachedFromVisualTree(e);
        this.DetachIcon();
        this.isAttachedToVt = false;
    }

    private void OnIconChanged(Icon? oldValue, Icon? newValue) {
        Debug.Assert(oldValue == null || !this.isAttachedToVt || ReferenceEquals(oldValue, this.attachedIcon));
        this.DetachIcon();
        if (newValue is AbstractAvaloniaIcon newIcon && this.isAttachedToVt) {
            this.attachedIcon = newIcon;
            newIcon.RenderInvalidated += this.OnIconInvalidated;
        }
    }

    private void DetachIcon() {
        if (this.attachedIcon != null) {
            this.attachedIcon.RenderInvalidated -= this.OnIconInvalidated;
            this.attachedIcon = null;
        }
    }

    private void OnIconInvalidated(object? sender, EventArgs e) {
        this.InvalidateMeasure();
        this.InvalidateVisual();
    }

    public override void Render(DrawingContext context) {
        base.Render(context);

        if (this.UseBoundsHitTest) {
            context.DrawRectangle(Brushes.Transparent, null, this.Bounds);
        }

        if (this.Icon is AbstractAvaloniaIcon icon) {
            if (IIconPreferences.TryGetInstance(out IIconPreferences? prefs) && !prefs.UseAntiAliasing) {
                using (context.PushRenderOptions(s_AliasRenderOptions)) {
                    icon.Render(context, this.Bounds, this.Stretch);
                }
            }
            else {
                icon.Render(context, this.Bounds, this.Stretch);
            }
        }
    }

    protected override Size MeasureOverride(Size availableSize) {
        Size size = default;
        if (this.Icon is AbstractAvaloniaIcon icon) {
            size = icon.Measure(availableSize, (StretchMode) (int) this.Stretch);
        }

        return size;
    }

    protected override Size ArrangeOverride(Size finalSize) {
        Size size = default;
        
        if (this.Icon is AbstractAvaloniaIcon icon) {
            size = icon.Measure(finalSize, (StretchMode) (int) this.Stretch);
        }
        
        return size;
    }
}