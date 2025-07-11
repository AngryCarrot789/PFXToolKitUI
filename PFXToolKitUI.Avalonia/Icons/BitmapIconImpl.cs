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
using Avalonia.Media.Imaging;
using PFXToolKitUI.Avalonia.Utils;
using PFXToolKitUI.Icons;
using SkiaSharp;

namespace PFXToolKitUI.Avalonia.Icons;

public class BitmapIconImpl : AbstractAvaloniaIcon {
    public Bitmap Bitmap { get; }

    public BitmapIconImpl(string name, Bitmap bitmap) : base(name) {
        this.Bitmap = bitmap;
    }

    public override void Render(DrawingContext context, Rect size, SKMatrix transform) {
        using DrawingContext.PushedState? state = transform != SKMatrix.Identity ? context.PushTransform(transform.ToAvMatrix()) : null;
        context.DrawImage(this.Bitmap, size);
    }

    public override (Size Size, SKMatrix Transform) Measure(Size availableSize, StretchMode stretchMode) {
        return (((Stretch) (int) stretchMode).CalculateSize(availableSize, this.Bitmap.Size), SKMatrix.Identity);
    }

    public override Rect GetBounds() {
        return new Rect(default, this.Bitmap.Size);
    }
}