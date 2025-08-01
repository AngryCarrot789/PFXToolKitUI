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
using PFXToolKitUI.Icons;
using SkiaSharp;

namespace PFXToolKitUI.Avalonia.Icons;

/// <summary>
/// The main abstract class for icons implemented for AvaloniaUI
/// </summary>
public abstract class AbstractAvaloniaIcon : Icon {
    /// <summary>
    /// An event fired when this icon's visual becomes invalid (e.g. a dynamic resource value changes)
    /// </summary>
    public event EventHandler? RenderInvalidated;

    protected AbstractAvaloniaIcon(string name) : base(name) {
    }

    protected void OnRenderInvalidated() => this.RenderInvalidated?.Invoke(this, EventArgs.Empty);

    /// <summary>
    /// Renders this avalonia icon into the given drawing context
    /// </summary>
    /// <param name="context">The drawing context</param>
    /// <param name="size">The drawing area size</param>
    public abstract void Render(DrawingContext context, Rect size, SKMatrix transform);

    public abstract (Size Size, SKMatrix Transform) Measure(Size availableSize, StretchMode stretch);

    public abstract Rect GetBounds();
}