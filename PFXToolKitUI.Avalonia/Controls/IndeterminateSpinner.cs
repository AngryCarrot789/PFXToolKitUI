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
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Threading;
using Avalonia.VisualTree;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.Avalonia.Controls;

public sealed class IndeterminateSpinner : Control {
    public static readonly StyledProperty<bool> IsSpinningProperty = AvaloniaProperty.Register<IndeterminateSpinner, bool>(nameof(IsSpinning), defaultValue: false);
    public static readonly StyledProperty<Color> SegmentFillColourProperty = AvaloniaProperty.Register<IndeterminateSpinner, Color>(nameof(SegmentFillColour));
    public static readonly StyledProperty<double> RadiusProperty = AvaloniaProperty.Register<IndeterminateSpinner, double>(nameof(Radius), defaultValue: 7.0);
    public static readonly StyledProperty<double> ThicknessProperty = AvaloniaProperty.Register<IndeterminateSpinner, double>(nameof(Thickness), defaultValue: 3.0);
    public static readonly StyledProperty<int> SegmentsProperty = AvaloniaProperty.Register<IndeterminateSpinner, int>(nameof(Segments), defaultValue: 8);

    public Color SegmentFillColour {
        get => this.GetValue(SegmentFillColourProperty);
        set => this.SetValue(SegmentFillColourProperty, value);
    }

    public double Radius {
        get => this.GetValue(RadiusProperty);
        set => this.SetValue(RadiusProperty, value);
    }

    public double Thickness {
        get => this.GetValue(ThicknessProperty);
        set => this.SetValue(ThicknessProperty, value);
    }

    public int Segments {
        get => this.GetValue(SegmentsProperty);
        set => this.SetValue(SegmentsProperty, value);
    }

    public bool IsSpinning {
        get => this.GetValue(IsSpinningProperty);
        set => this.SetValue(IsSpinningProperty, value);
    }

    private int currentIndex = 0;
    private readonly DispatcherTimer myTimer;

    public IndeterminateSpinner() {
        this.myTimer = new DispatcherTimer {
            Interval = TimeSpan.FromSeconds(1.0 / 8.0)
        };

        this.myTimer.Tick += (s, e) => {
            if (this.currentIndex >= (this.Segments - 1)) {
                this.currentIndex = 0;
            }
            else {
                this.currentIndex++;
            }

            this.InvalidateVisual();
        };

        this.myTimer.Start();
    }

    static IndeterminateSpinner() {
        AffectsRender<IndeterminateSpinner>(IsSpinningProperty, SegmentFillColourProperty, RadiusProperty, ThicknessProperty, SegmentsProperty);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e) {
        base.OnAttachedToVisualTree(e);
        this.UpdateCanSpin();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e) {
        base.OnDetachedFromVisualTree(e);
        this.UpdateCanSpin();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change) {
        base.OnPropertyChanged(change);
        if (change.Property == IsSpinningProperty) {
            this.currentIndex = 0;
            this.UpdateCanSpin();
        }
    }

    private void UpdateCanSpin() {
        if (this.IsSpinning && this.IsAttachedToVisualTree()) {
            this.myTimer.Start();
        }
        else {
            this.myTimer.Stop();
        }
    }

    protected override Size MeasureOverride(Size availableSize) {
        Size size = base.MeasureOverride(availableSize);

        double diameter = (this.Radius * 2);
        diameter += (diameter / 6.0);
        
        return new Size(Math.Max(size.Width, diameter), Math.Max(size.Height, diameter));
    }

    public override void Render(DrawingContext context) {
        base.Render(context);
        double radius = this.Radius;
        if (DoubleUtils.LessThanOrClose(radius, 1.0) || !this.IsSpinning) {
            return;
        }

        Rect bounds = this.Bounds;
        Point center = new Point(bounds.Width / 2, bounds.Height / 2);
        int segments = this.Segments;
        double anglePerSegment = 360.0 / segments;
        double segmentSize = radius / 3.0;

        Color fillColour = this.SegmentFillColour;
        for (int i = 0; i < segments; i++) {
            // Create fading effect: leading segment is lighter
            int index = this.currentIndex + i;
            if (index > (segments - 1)) {
                index -= segments;
            }

            byte alpha = i == 0 ? (byte) 255 : (byte) (255 * i / (segments + 1));
            ImmutableSolidColorBrush fillBrush = new ImmutableSolidColorBrush(Color.FromArgb(alpha, fillColour.R, fillColour.G, fillColour.B));

            double angleRadians = (index * anglePerSegment) * (Math.PI / 180);
            Point p1 = new Point(
                center.X + Math.Cos(angleRadians) * (radius - segmentSize),
                center.Y + Math.Sin(angleRadians) * (radius - segmentSize)
            );

            Point p2 = new Point(
                center.X + Math.Cos(angleRadians) * radius,
                center.Y + Math.Sin(angleRadians) * radius
            );

            ImmutablePen pen = new ImmutablePen(fillBrush, this.Thickness, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round);
            context.DrawLine(pen, p1, p2);
        }
    }
}