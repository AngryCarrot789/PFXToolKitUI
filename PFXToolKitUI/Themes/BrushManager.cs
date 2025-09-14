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

using PFXToolKitUI.Themes.Gradients;
using SkiaSharp;

namespace PFXToolKitUI.Themes;

/// <summary>
/// A factory used to create brushes
/// </summary>
public abstract class BrushManager {
    public static BrushManager Instance => ApplicationPFX.GetComponent<BrushManager>();

    /// <summary>
    /// Creates a brush whose underlying colour is constant and does not change
    /// </summary>
    /// <param name="colour">The colour</param>
    /// <returns>The brush</returns>
    public abstract IConstantColourBrush CreateConstant(SKColor colour);

    /// <summary>
    /// Creates a constant unchanging linear gradient brush
    /// </summary>
    public abstract ILinearGradientColourBrush CreateConstantLinearGradient(IReadOnlyList<GradientStop> gradientStops,
                                                                            double opacity = 1,
                                                                            RelativePoint? transformOrigin = null,
                                                                            GradientSpreadMethod spreadMethod = GradientSpreadMethod.Pad,
                                                                            RelativePoint? startPoint = null,
                                                                            RelativePoint? endPoint = null);

    /// <summary>
    /// Creates a constant unchanging radial gradient brush
    /// </summary>
    public abstract IRadialGradientColourBrush CreateConstantRadialGradient(IReadOnlyList<GradientStop> gradientStops, 
                                                                            double opacity = 1, 
                                                                            RelativePoint? transformOrigin = null, 
                                                                            GradientSpreadMethod spreadMethod = GradientSpreadMethod.Pad, 
                                                                            RelativePoint? center = null, 
                                                                            RelativePoint? gradientOrigin = null, 
                                                                            double radius = 0.5);
    
    /// <summary>
    /// Gets a known theme brush that may change at any time
    /// </summary>
    /// <param name="themeKey">The key</param>
    /// <returns>The brush</returns>
    public abstract IDynamicColourBrush GetDynamicThemeBrush(string themeKey);

    /// <summary>
    /// Gets a known theme brush as a constant (the returned value's underlying UI brush does not change)
    /// </summary>
    /// <param name="themeKey"></param>
    /// <returns></returns>
    public abstract IStaticColourBrush GetStaticThemeBrush(string themeKey);
}