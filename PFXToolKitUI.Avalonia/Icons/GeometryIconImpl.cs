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
using PFXToolKitUI.Logging;
using PFXToolKitUI.Utils.Destroying;

namespace PFXToolKitUI.Avalonia.Icons;

public class GeometryIconImpl : AbstractAvaloniaIcon, IGeometryIcon {
    public IEnumerable<GeometryEntry> GeometryEntries => this.myGeometryEntries;

    private GeometryEntryWrapper[] GeometryEntryRefs {
        get {
            if (this.myGeometryEntryRefs == null) {
                this.myGeometryEntryRefs = this.myGeometryEntries.Select(e => new GeometryEntryWrapper(this, e)).ToArray();
                AppLogger.Instance.WriteLine($"[Icon] Generated SVG for '{this.Name}'. Bounds = {this.GetBounds()}");
            }

            return this.myGeometryEntryRefs;
        }
    }

    private readonly GeometryEntry[] myGeometryEntries;
    private GeometryEntryWrapper[]? myGeometryEntryRefs;

    public GeometryIconImpl(string name, GeometryEntry[] geometry) : base(name) {
        this.myGeometryEntries = geometry;
    }

    public override void Render(DrawingContext context, Rect bounds, StretchMode stretch) {
        // {
        //     using DrawingContext.PushedState? state = transform != SKMatrix.Identity ? context.PushTransform(transform.ToAvMatrix()) : null;
        //
        //     foreach (GeometryEntryRef geo in this.GeometryEntryRefs) {
        //         if (geo.geometry != null) {
        //             if (geo.myPen == null && geo.myPenBrush != null) {
        //                 geo.myPen = new Pen(geo.myPenBrush, geo.entry.StrokeThickness);
        //             }
        //
        //             context.DrawGeometry(geo.myFillBrush, geo.myPen, geo.geometry);
        //         }
        //     }
        // }

        {
            GeometryEntryWrapper[] entries = this.GeometryEntryRefs;
            if (entries.Length < 1) {
                return;
            }

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
                foreach (GeometryEntryWrapper geo in entries) {
                    if (geo.geometry != null) {
                        if (geo.myPen == null && geo.myPenBrush != null) {
                            geo.myPen = new Pen(geo.myPenBrush, geo.entry.StrokeThickness);
                        }

                        context.DrawGeometry(geo.myFillBrush, geo.myPen, geo.geometry);
                    }
                }
            }
        }
    }

    public Rect GetBounds() {
        int count = 0;
        double l = double.MaxValue, t = double.MaxValue, r = double.MinValue, b = double.MinValue;
        foreach (GeometryEntryWrapper? geometry in this.GeometryEntryRefs) {
            if (geometry.geometry == null) {
                continue;
            }

            Rect a = geometry.geometry.Bounds;
            l = Math.Min(a.Left, l);
            t = Math.Min(a.Top, t);
            r = Math.Max(a.Right, r);
            b = Math.Max(a.Bottom, b);
            count++;
        }

        return count > 0 ? new Rect(l, t, r - l, b - t) : default;
    }

    public override Size Measure(Size availableSize, StretchMode stretch) {
        return stretch.CalculateSize(availableSize, this.GetBounds().Size);
        // (Size size, Matrix t) = SkiaAvUtils.CalculateSizeAndTransform(availableSize, this.GetBounds(), (Stretch) stretch);
        // return (size, t.ToSKMatrix());
    }

    internal class GeometryEntryWrapper {
        private readonly GeometryIconImpl iconImpl;
        public readonly GeometryEntry entry;
        public readonly Geometry? geometry;
        private IDisposable? disposeFillBrush, disposeStrokeBrush;
        public IBrush? myFillBrush, myPenBrush;
        public IPen? myPen;

        public GeometryEntryWrapper(GeometryIconImpl iconImpl, GeometryEntry entry) {
            this.iconImpl = iconImpl;
            this.entry = entry;

            if (entry.Fill is DynamicAvaloniaColourBrush b)
                this.disposeFillBrush = b.Subscribe(static (b, s) => ((GeometryEntryWrapper) s!).OnFillBrushInvalidated(b.CurrentBrush), this, invokeHandlerImmediately: false);
            if (entry.Stroke is DynamicAvaloniaColourBrush s)
                this.disposeStrokeBrush = s.Subscribe(static (b, s) => ((GeometryEntryWrapper) s!).OnStrokeBrushInvalidated(b.CurrentBrush), this, invokeHandlerImmediately: false);

            if (entry.Fill != null)
                this.myFillBrush = ((AvaloniaColourBrush) entry.Fill).Brush;
            if (entry.Stroke != null)
                this.myPenBrush = ((AvaloniaColourBrush) entry.Stroke).Brush;

            try {
                this.geometry = Geometry.Parse(this.entry.Geometry);
            }
            catch (Exception e) {
                AppLogger.Instance.WriteLine("Error parsing SVG for svg icon: " + e);
            }
        }

        public Rect Bounds => this.geometry != null ? this.geometry.Bounds : default;

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
}