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

using Avalonia;
using Avalonia.Media;
using PFXToolKitUI.Avalonia.Themes.BrushFactories;
using PFXToolKitUI.Icons;
using PFXToolKitUI.Themes;

namespace PFXToolKitUI.Avalonia.Icons;

public class EllipseIconImpl : AbstractAvaloniaIcon {
    public readonly IColourBrush? TheFillBrush;
    public readonly IColourBrush? TheStrokeBrush;
    public readonly double StrokeThickness;
    public readonly double RadiusX;
    public readonly double RadiusY;

    private IBrush? myFillBrush, myPenBrush;
    private IPen? myPen;
    private readonly IDisposable? disposeFillBrush, disposeStrokeBrush;

    public EllipseIconImpl(string name, IColourBrush? fill, IColourBrush? stroke, double radiusX, double radiusY, double strokeThickness = 0) : base(name) {
        this.RadiusX = radiusX;
        this.RadiusY = radiusY;
        this.TheFillBrush = fill;
        this.TheStrokeBrush = stroke;
        this.StrokeThickness = strokeThickness;
        if (fill is DynamicAvaloniaColourBrush b)
            this.disposeFillBrush = b.Subscribe(static (b, s) => ((EllipseIconImpl) s!).OnFillBrushInvalidated(b.CurrentBrush), this, invokeHandlerImmediately: false);
        if (stroke is DynamicAvaloniaColourBrush s)
            this.disposeStrokeBrush = s.Subscribe(static (b, s) => ((EllipseIconImpl) s!).OnStrokeBrushInvalidated(b.CurrentBrush), this, invokeHandlerImmediately: false);

        if (fill != null)
            this.myFillBrush = ((AvaloniaColourBrush) fill).Brush;
        if (stroke != null)
            this.myPenBrush = ((AvaloniaColourBrush) stroke).Brush;
    }

    private void OnFillBrushInvalidated(IBrush? brush) {
        this.myFillBrush = brush;
        this.OnRenderInvalidated();
    }

    private void OnStrokeBrushInvalidated(IBrush? brush) {
        this.myPenBrush = brush;
        this.myPen = null;
        this.OnRenderInvalidated();
    }

    public override void Render(DrawingContext context, Rect bounds, StretchMode stretch) {
        Rect fakeBounds = new Rect(bounds.Size);
        Rect combinedBounds = this.GetBounds();
        if (combinedBounds.Width <= 0 || combinedBounds.Height <= 0) {
            return;
        }

        double scaleX = fakeBounds.Width / combinedBounds.Width;
        double scaleY = fakeBounds.Height / combinedBounds.Height;

        double scale = stretch switch {
            StretchMode.Fill => Math.Max(scaleX, scaleY),
            StretchMode.Uniform => Math.Min(scaleX, scaleY),
            StretchMode.UniformNoUpscale => Math.Min(Math.Min(scaleX, scaleY), 1.0),
            StretchMode.UniformToFill => Math.Max(scaleX, scaleY),
            StretchMode.None => 1.0,
            _ => 1.0
        };

        Point offset = fakeBounds.Center - combinedBounds.Center * scale;
        Matrix transform = Matrix.CreateScale(scale, scale) * Matrix.CreateTranslation(offset.X, offset.Y);

        using (context.PushTransform(transform)) {
            if (this.myPen == null && this.myPenBrush != null) {
                this.myPen = new Pen(this.myPenBrush, this.StrokeThickness);
            }
            
            context.DrawEllipse(this.myFillBrush, this.myPen, new Point(bounds.Width / 2.0, bounds.Height / 2.0), this.RadiusX, this.RadiusY);
        }
    }

    public Rect GetBounds() {
        return new Rect(0, 0, this.RadiusX * 2.0, this.RadiusY * 2.0);
    }

    public override Size Measure(Size availableSize, StretchMode stretch) {
        return stretch.CalculateSize(availableSize, this.GetBounds().Size);
    }
}