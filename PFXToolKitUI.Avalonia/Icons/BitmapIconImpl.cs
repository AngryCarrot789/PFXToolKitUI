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
using PFXToolKitUI.Icons;

namespace PFXToolKitUI.Avalonia.Icons;

public class BitmapIconImpl : AbstractAvaloniaIcon {
    public Bitmap Bitmap { get; }

    public BitmapIconImpl(string name, Bitmap bitmap) : base(name) {
        this.Bitmap = bitmap;
    }

    public override void Render(DrawingContext context, Rect bounds, StretchMode stretch) {
        if (bounds.Width > 0 && bounds.Height > 0) {
            Rect viewPort = new Rect(bounds.Size);
            Size sourceSize = this.Bitmap.Size;

            Vector scale = stretch.CalculateScaling(bounds.Size, sourceSize);
            Size scaledSize = sourceSize * scale;
            Rect destRect = viewPort.CenterRect(new Rect(scaledSize)).Intersect(viewPort);
            Rect sourceRect = new Rect(sourceSize).CenterRect(new Rect(destRect.Size / scale));

            context.DrawImage(this.Bitmap, sourceRect, destRect);
        }
    }

    public override Size Measure(Size availableSize, StretchMode stretch) {
        return stretch.CalculateSize(availableSize, this.Bitmap.Size);
    }
}