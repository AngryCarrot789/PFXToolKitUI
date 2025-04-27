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
using Avalonia.Media;
using Avalonia.Skia;
using PFXToolKitUI.Avalonia.Themes.BrushFactories;
using PFXToolKitUI.Avalonia.Utils;
using PFXToolKitUI.Icons;
using PFXToolKitUI.Logging;
using PFXToolKitUI.Utils.Destroying;
using SkiaSharp;

namespace PFXToolKitUI.Avalonia.Icons;

public class GeometryIconImpl : AbstractAvaloniaIcon {
    private readonly GeometryEntryRef[] Elements;
    public readonly StretchMode Stretch;
    private Geometry?[]? geometries;

    private class GeometryEntryRef : IDisposable {
        private readonly GeometryIconImpl iconImpl;
        public readonly GeometryEntry entry;
        public IDisposable? disposeFillBrush, disposeStrokeBrush;
        public IBrush? myFillBrush, myPenBrush;
        public IPen? myPen;
        
        public GeometryEntryRef(GeometryIconImpl iconImpl, GeometryEntry entry) {
            this.iconImpl = iconImpl;
            this.entry = entry;
            if (entry.Fill is DynamicAvaloniaColourBrush b) {
                this.disposeFillBrush = b.Subscribe(this.OnFillBrushInvalidated);
            }
            else if (entry.Fill != null) {
                this.myFillBrush = ((AvaloniaColourBrush) entry.Fill).Brush;
            }

            if (entry.Stroke is DynamicAvaloniaColourBrush s) {
                this.disposeStrokeBrush = s.Subscribe(this.OnStrokeBrushInvalidated);
            }
            else if (entry.Stroke != null) {
                this.myPenBrush = ((AvaloniaColourBrush) entry.Stroke).Brush;
            }
        }

        private void OnFillBrushInvalidated(IBrush? brush) {
            this.myFillBrush = brush;
            this.iconImpl.OnRenderInvalidated();
        }

        private void OnStrokeBrushInvalidated(IBrush? brush) {
            this.myPenBrush = brush;
            this.myPen = null;
            this.iconImpl.OnRenderInvalidated();
        }

        public void Dispose() {
            DisposableUtils.Dispose(ref this.disposeFillBrush);
            DisposableUtils.Dispose(ref this.disposeStrokeBrush);
        }
    }

    public Geometry?[] Geometries {
        get {
            if (this.geometries == null) {
                this.geometries = new Geometry[this.Elements.Length];
                for (int i = 0; i < this.Elements.Length; i++) {
                    try {
                        this.geometries[i] = Geometry.Parse(this.Elements[i].entry.Geometry);
                    }
                    catch (Exception e) {
                        AppLogger.Instance.WriteLine("Error parsing SVG for svg icon: \n" + e);
                    }
                }
            }

            return this.geometries!;
        }
    }

    public GeometryIconImpl(string name, GeometryEntry[] geometry, StretchMode stretch) : base(name) {
        this.Elements = geometry.Select(e => new GeometryEntryRef(this, e)).ToArray();
        this.Stretch = stretch;
    }

    public override void Render(DrawingContext context, Rect size, SKMatrix transform) {
        using DrawingContext.PushedState? state = transform != SKMatrix.Identity ? context.PushTransform(transform.ToAvMatrix()) : null;

        Geometry?[] geoArray = this.Geometries;
        for (int i = 0; i < geoArray.Length; i++) {
            if (geoArray[i] != null) {
                GeometryEntryRef geo = this.Elements[i];
                if (geo.myPen == null && geo.myPenBrush != null) {
                    geo.myPen = new Pen(geo.myPenBrush, geo.entry.StrokeThickness);
                }

                context.DrawGeometry(geo.myFillBrush, geo.myPen, geoArray[i]!);
            }
        }
    }

    public Rect GetBounds() {
        int count = 0;
        double l = double.MaxValue, t = double.MaxValue, r = double.MinValue, b = double.MinValue;
        foreach (Geometry? g in this.Geometries) {
            if (g == null) {
                continue;
            }

            Rect a = g.Bounds;
            l = Math.Min(a.Left, l);
            t = Math.Min(a.Top, t);
            r = Math.Max(a.Right, r);
            b = Math.Max(a.Bottom, b);
            count++;
        }

        return count > 0 ? new Rect(l, t, r - l, b - t) : default;
    }

    public override (Size Size, SKMatrix Transform) Measure(Size availableSize, StretchMode stretch) {
        (Size size, Matrix t) = SkiaAvUtils.CalculateSizeAndTransform(availableSize, this.GetBounds(), (Stretch) this.Stretch);
        return (size, t.ToSKMatrix());
    }
}