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
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using PFXToolKitUI.Themes;
using PFXToolKitUI.Themes.Gradients;
using SkiaSharp;
using GradientSpreadMethod = PFXToolKitUI.Themes.Gradients.GradientSpreadMethod;
using GradientStop = PFXToolKitUI.Themes.Gradients.GradientStop;
using RelativePoint = PFXToolKitUI.Themes.Gradients.RelativePoint;
using RelativeUnit = Avalonia.RelativeUnit;

namespace PFXToolKitUI.Avalonia.Themes.BrushFactories;

public class BrushManagerImpl : BrushManager {
    private Dictionary<string, DynamicAvaloniaColourBrush>? dynamicBrushes;
    private List<DynamicAvaloniaColourBrush>? listeningDynamicBrushes;

    public BrushManagerImpl() {
        Application app = Application.Current ?? throw new Exception("No app");
        app.ActualThemeVariantChanged += this.OnThemeChanged;
        app.ResourcesChanged += this.OnResourcesChanged;
    }

    private void OnThemeChanged(object? sender, EventArgs e) {
        this.ReevaluateAllBrushes();
    }

    private void OnResourcesChanged(object? sender, ResourcesChangedEventArgs e) {
        this.ReevaluateAllBrushes();
    }

    private void ReevaluateAllBrushes() {
        if (this.listeningDynamicBrushes != null) {
            foreach (DynamicAvaloniaColourBrush brush in this.listeningDynamicBrushes) {
                brush.ReevaluateBrush();
            }
        }
    }

    public override ConstantAvaloniaColourBrush CreateConstant(SKColor colour) {
        // Not really any point to caching an immutable brush
        return new ConstantAvaloniaColourBrush(colour);
    }

    public override ILinearGradientColourBrush CreateConstantLinearGradient(IReadOnlyList<GradientStop> gradientStops, double opacity = 1, RelativePoint? transformOrigin = null, GradientSpreadMethod spreadMethod = GradientSpreadMethod.Pad, RelativePoint? startPoint = null, RelativePoint? endPoint = null) {
        return new ConstantAvaloniaLinearGradientBrush(new ImmutableLinearGradientBrush(gradientStops.Select(x => new ImmutableGradientStop(x.Offset, new Color(x.Color.Alpha, x.Color.Red, x.Color.Green, x.Color.Blue))).ToList(), opacity, null, CastRP(transformOrigin), (global::Avalonia.Media.GradientSpreadMethod) spreadMethod, CastRP(startPoint), CastRP(endPoint)));
    }

    public override IRadialGradientColourBrush CreateConstantRadialGradient(IReadOnlyList<GradientStop> gradientStops, double opacity = 1, RelativePoint? transformOrigin = null, GradientSpreadMethod spreadMethod = GradientSpreadMethod.Pad, RelativePoint? center = null, RelativePoint? gradientOrigin = null, double radius = 0.5) {
        return new ConstantAvaloniaRadialGradientBrush(new ImmutableRadialGradientBrush(gradientStops.Select(x => new ImmutableGradientStop(x.Offset, new Color(x.Color.Alpha, x.Color.Red, x.Color.Green, x.Color.Blue))).ToList(), opacity, null, CastRP(transformOrigin), (global::Avalonia.Media.GradientSpreadMethod) spreadMethod, CastRP(center), CastRP(gradientOrigin), radius));
    }

    public override DynamicAvaloniaColourBrush GetDynamicThemeBrush(string themeKey) {
        if (this.dynamicBrushes == null) {
            this.dynamicBrushes = new Dictionary<string, DynamicAvaloniaColourBrush>();
        }
        else if (this.dynamicBrushes.TryGetValue(themeKey, out DynamicAvaloniaColourBrush? existingBrush)) {
            return existingBrush;
        }

        // Since these brushes can be quite expensive to listen for changes, we want to try and always cache them
        DynamicAvaloniaColourBrush brush = new DynamicAvaloniaColourBrush(this, themeKey);
        this.dynamicBrushes[themeKey] = brush;
        return brush;
    }

    public override IStaticColourBrush GetStaticThemeBrush(string themeKey) {
        if (ThemeManagerImpl.TryFindBrushInApplicationResources(themeKey, out IBrush? brush)) {
            return new StaticAvaloniaColourBrush(themeKey, brush.ToImmutable());
        }

        return new StaticAvaloniaColourBrush(themeKey, null);
    }

    private static global::Avalonia.RelativePoint CastRP(RelativePoint? rp) => rp is RelativePoint rp1 ? new global::Avalonia.RelativePoint(rp1.Point.X, rp1.Point.Y, (RelativeUnit) rp1.Unit) : default;

    internal void AddBrushAsListener(DynamicAvaloniaColourBrush brush) {
        this.listeningDynamicBrushes ??= new List<DynamicAvaloniaColourBrush>(32);
        this.listeningDynamicBrushes.Add(brush);
    }

    internal void RemoveBrushAsListener(DynamicAvaloniaColourBrush brush) {
        this.listeningDynamicBrushes?.Remove(brush);
    }
}