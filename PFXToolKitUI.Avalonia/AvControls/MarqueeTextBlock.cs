// 
// Copyright (c) 2026-2026 REghZy
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
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using PFXToolKitUI.Utils.Events;

namespace PFXToolKitUI.Avalonia.AvControls;

public class MarqueeTextBlock : TextBlock {
    public static readonly StyledProperty<Orientation> AutoScrollOrientationProperty =
        AvaloniaProperty.Register<MarqueeTextBlock, Orientation>(
            nameof(AutoScrollOrientation),
            defaultValue: Orientation.Horizontal);

    public static readonly StyledProperty<bool> IsAutoScrollEnabledProperty =
        AvaloniaProperty.Register<MarqueeTextBlock, bool>(
            nameof(IsAutoScrollEnabled),
            defaultValue: true);

    public Orientation AutoScrollOrientation {
        get => this.GetValue(AutoScrollOrientationProperty);
        set => this.SetValue(AutoScrollOrientationProperty, value);
    }

    public bool IsAutoScrollEnabled {
        get => this.GetValue(IsAutoScrollEnabledProperty);
        set => this.SetValue(IsAutoScrollEnabledProperty, value);
    }
    
    // Timer to start scrolling
    private DispatcherTimer StartScrollTimer {
        get {
            if (this.myStartScrollTimer == null) {
                this.myStartScrollTimer = new DispatcherTimer(DispatcherPriority.Default) {
                    Interval = TimeSpan.FromSeconds(2.0)
                };
                
                this.myStartScrollTimer.Tick += this.OnTickStartScroll;
            }
            
            return this.myStartScrollTimer;
        }
    }

    // Timer to reset scroll position and try to restart auto scrolling
    private DispatcherTimer ResetScrollTimer {
        get {
            if (this.myResetScrollTimer == null) {
                this.myResetScrollTimer = new DispatcherTimer(DispatcherPriority.Default) {
                    Interval = TimeSpan.FromSeconds(2.0)
                };
                
                this.myResetScrollTimer.Tick += this.OnTickResetScroll;
            }
            
            return this.myResetScrollTimer;
        }
    }

    // Timer to actuall scroll
    private DispatcherTimer ScrollTimer {
        get {
            if (this.myScrollTimer == null) {
                this.myScrollTimer = new DispatcherTimer(DispatcherPriority.Render) {
                    Interval = TimeSpan.FromSeconds(1.0 / 20.0 /* 20 fps */)
                };
                
                this.myScrollTimer.Tick += this.OnTickScroll;
            }
            
            return this.myScrollTimer;
        }
    }
    
    private Point OffsetPoint {
        get => field;
        set => PropertyHelper.SetAndRaiseINE(ref field, value, static (x, y) => x == y, this, static @this => @this.InvalidateVisual());
    }

    private double targetOffset;
    private Size myTextSize;
    private DispatcherTimer? myStartScrollTimer, myResetScrollTimer, myScrollTimer;

    public MarqueeTextBlock() {
    }

    private void OnTickStartScroll(object? sender, EventArgs e) {
        if (!this.IsAutoScrollEnabled || string.IsNullOrWhiteSpace(this.Text)) {
            this.ResetAll();
            return;
        }

        double overlap = this.GetOverflowAmount();
        if (overlap <= 0.0) {
            this.ResetAll();
            return;
        }

        this.targetOffset = overlap;
        this.myStartScrollTimer?.Stop();
        this.ScrollTimer.Start();
    }

    private void OnTickResetScroll(object? sender, EventArgs e) {
        this.ResetAll();
        this.StartScrollTimer.Start();
    }

    protected override void RenderTextLayout(DrawingContext context, Point origin) {
        base.RenderTextLayout(context, origin + this.OffsetPoint);
    }

    private void OnTickScroll(object? sender, EventArgs e) {
        if (!this.IsLoaded) {
            this.ResetAll();
            return;
        }

        double offset;
        if (this.AutoScrollOrientation == Orientation.Horizontal) {
            this.OffsetPoint = new Point(offset = this.OffsetPoint.X - 1.0, this.OffsetPoint.Y);
        }
        else {
            this.OffsetPoint = new Point(this.OffsetPoint.X, offset = this.OffsetPoint.Y - 1.0);
        }

        if (offset <= -this.targetOffset) {
            this.ScrollTimer.Stop();
            this.ResetScrollTimer.Start();
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e) {
        this.ResetAll();
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change) {
        base.OnPropertyChanged(change);

        if (this.IsLoaded && (change.Property == AutoScrollOrientationProperty || change.Property == IsAutoScrollEnabledProperty)) {
            this.ResetAll();
            this.StartScrollTimer.Start();
        }
    }

    protected override Size MeasureOverride(Size availableSize) {
        this.myTextSize = base.MeasureOverride(availableSize);
        this.OnTextSizeChanged();
        return this.myTextSize;
    }

    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
        this.ResetAll();
        if (!this.IsAutoScrollEnabled || string.IsNullOrWhiteSpace(this.Text)) {
            return;
        }

        double overlap = this.GetOverflowAmount();
        if (overlap > 0.0) {
            this.targetOffset = overlap;
            this.StartScrollTimer.Start();
        }
    }

    protected override void OnUnloaded(RoutedEventArgs e) {
        base.OnUnloaded(e);
        this.ResetAll();
    }

    private void OnTextSizeChanged() {
        if (!this.IsLoaded || !this.IsAutoScrollEnabled || string.IsNullOrWhiteSpace(this.Text)) {
            this.ResetAll();
            return;
        }

        double overlap = this.GetOverflowAmount();
        if (overlap <= 0.0) {
            this.ResetAll();
            return;
        }

        this.targetOffset = overlap;
        if (!this.ScrollTimer.IsEnabled) {
            this.StartScrollTimer.Start();
        }
    }

    private double GetOverflowAmount() {
        if (this.AutoScrollOrientation == Orientation.Horizontal)
            return Math.Max(0, this.myTextSize.Width - this.Bounds.Width);
        else
            return Math.Max(0, this.myTextSize.Height - this.Bounds.Height);
    }

    private void ResetAll() {
        this.myStartScrollTimer?.Stop();
        this.myResetScrollTimer?.Stop();
        this.myScrollTimer?.Stop();
        this.targetOffset = 0.0;
        this.OffsetPoint = default;
    }
}