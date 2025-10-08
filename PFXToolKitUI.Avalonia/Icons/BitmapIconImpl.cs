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
using Avalonia.Media;
using Avalonia.Media.Imaging;
using PFXToolKitUI.Icons;
using PFXToolKitUI.Logging;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.Avalonia.Icons;

public class BitmapIconImpl : AbstractAvaloniaIcon {
    private readonly Func<Task<Bitmap>>? loadBitmapFunc;
    private int stateBitmapLoading; // 0 = not loading, 1 = loading, 2 = loaded, 3 = failed

    public Bitmap? Bitmap { get; private set; }

    public BitmapIconImpl(string name, Bitmap bitmap) : base(name) {
        this.Bitmap = bitmap ?? throw new ArgumentNullException(nameof(bitmap));
    }

    public BitmapIconImpl(string name, Func<Task<Bitmap>> loadBitmapFunc) : base(name) {
        this.loadBitmapFunc = loadBitmapFunc ?? throw new ArgumentNullException(nameof(loadBitmapFunc));
    }

    public override void Render(DrawingContext context, Rect bounds, StretchMode stretch) {
        this.EnsureBitmapLoadedOrTryingToLoad();
        if (bounds.Width > 0 && bounds.Height > 0 && this.Bitmap != null) {
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
        this.EnsureBitmapLoadedOrTryingToLoad();
        return this.Bitmap == null ? default : stretch.CalculateSize(availableSize, this.Bitmap.Size);
    }

    private void EnsureBitmapLoadedOrTryingToLoad() {
        if (this.stateBitmapLoading == 0 && this.Bitmap == null && this.loadBitmapFunc != null) {
            this.stateBitmapLoading = 1;
            this.LoadBitmapLazyAsync();
        }
    }

    private async void LoadBitmapLazyAsync() {
        Debug.Assert(this.stateBitmapLoading == 1);
        Debug.Assert(this.loadBitmapFunc != null);

        Bitmap bitmap;
        try {
            bitmap = await this.loadBitmapFunc!();
        }
        catch (Exception e) {
            this.stateBitmapLoading = 3;
            AppLogger.Instance.WriteLine("[Icon] Failed to lazily load bitmap for " + this.Name + ": " + e.GetToString());
            return;
        }
            
        ApplicationPFX.Instance.Dispatcher.Post(() => {
            this.stateBitmapLoading = 2;
            this.Bitmap = bitmap;
            this.OnRenderInvalidated();
        });
    }
}