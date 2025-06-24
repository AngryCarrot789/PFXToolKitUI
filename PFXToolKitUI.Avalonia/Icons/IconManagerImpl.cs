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

using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Media.Immutable;
using Avalonia.Platform;
using Avalonia.Skia;
using PFXToolKitUI.Icons;
using PFXToolKitUI.Logging;
using PFXToolKitUI.Themes;
using PFXToolKitUI.Utils;
using SkiaSharp;

namespace PFXToolKitUI.Avalonia.Icons;

/// <summary>
/// A class that manages a set of registered icons throughout the application. This is used to simply icon usage
/// </summary>
public class IconManagerImpl : IconManager {
    // Try to find an existing icon with the same file path. Share pixel data, maybe using a wrapper, because icons are lazily loaded

    public IconManagerImpl() {
    }

    public Icon RegisterIconUsingStream(string name, Stream stream) {
        Bitmap? bmp = null;
        try {
            return this.RegisterCore(new BitmapIconImpl(name, bmp = new Bitmap(stream)));
        }
        catch (Exception e) {
            AppLogger.Instance.WriteLine($"Exception while loading bitmap from stream:{Environment.NewLine}{e.GetToString()}");
            bmp?.Dispose();
            return this.RegisterCore(new EmptyIcon(name));
        }
    }
    
    public override Icon RegisterIconByFilePath(string name, string filePath, bool lazilyLoad = true) {
        using BufferedStream stream = new BufferedStream(File.OpenRead(filePath), 4096);
        return this.RegisterIconUsingStream(name, stream);
    }
    
    public override Icon RegisterIconByUri(string name, Uri uri, bool lazilyLoad = true) {
        Stream stream;
        try {
            stream = AssetLoader.Open(uri);
        }
        catch (Exception e) {
            AppLogger.Instance.WriteLine($"Exception while opening Uri '{uri}' as stream:{Environment.NewLine}{e.GetToString()}");
            return this.RegisterCore(new EmptyIcon(name));
        }

        try {
            return this.RegisterIconUsingStream(name, stream);
        }
        finally {
            stream.Dispose();
        }
    }

    public override Icon RegisterIconUsingBitmap(string name, SKBitmap bitmap) {
        Bitmap? bmp = null;
        try {
            SKImageInfo info = bitmap.Info;
            PixelFormat? fmt = info.ColorType.ToAvalonia();
            bmp = new Bitmap(fmt ?? PixelFormat.Bgra8888, info.AlphaType.ToAlphaFormat(), bitmap.GetPixels(), new PixelSize(info.Width, info.Height), new Vector(96, 96), info.RowBytes);
            return this.RegisterCore(new BitmapIconImpl(name, bmp));
        }
        catch (Exception e) {
            AppLogger.Instance.WriteLine($"Exception while creating avalonia bitmap from skia bitmap:\n{e}");
            bmp?.Dispose();
            return this.RegisterCore(new EmptyIcon(name));
        }
    }

    public override Icon RegisterGeometryIcon(string name, GeometryEntry[] geometry, StretchMode stretch = StretchMode.None) {
        return this.RegisterCore(new GeometryIconImpl(name, geometry, stretch));
    }

    public override Icon RegisterEllipseIcon(string name, IColourBrush? fill, IColourBrush? stroke, double radiusX, double radiusY, double strokeThickness = 0) {
        return this.RegisterCore(new EllipseIconImpl(name, fill, stroke, radiusX, radiusY, strokeThickness));
    }

    private static ImmutableSolidColorBrush? ColourToBrush(SKColor? colour) {
        return colour is SKColor b ? new ImmutableSolidColorBrush(Color.FromArgb(b.Alpha, b.Red, b.Green, b.Blue)) : null;
    }
}