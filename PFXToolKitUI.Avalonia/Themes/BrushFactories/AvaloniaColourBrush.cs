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
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Skia;
using Avalonia.Threading;
using PFXToolKitUI.Logging;
using PFXToolKitUI.Themes;
using PFXToolKitUI.Themes.Gradients;
using SkiaSharp;

namespace PFXToolKitUI.Avalonia.Themes.BrushFactories;

/// <summary>
/// The primary base class for all avalonia implementations of <see cref="IColourBrush"/>
/// </summary>
public abstract class AvaloniaColourBrush : IColourBrush {
    /// <summary>
    /// Gets the brush currently associated with this object. For dynamic brushes (<see cref="DynamicAvaloniaColourBrush"/>),
    /// this value will change if any subscription is active and the application brush it's keyed to changes
    /// </summary>
    public abstract IBrush? Brush { get; }
}

public sealed class ConstantAvaloniaColourBrush : AvaloniaColourBrush, IConstantColourBrush {
    public static IColourBrush Transparent { get; } = new ConstantAvaloniaColourBrush(Brushes.Transparent);

    public override ImmutableSolidColorBrush Brush { get; }

    public SKColor Color { get; }

    public ConstantAvaloniaColourBrush(SKColor colour) {
        this.Color = colour;

        // TODO: maybe try to further save resources by seeing if the colour represents known brushes
        this.Brush = (ImmutableSolidColorBrush?) GetBrushFromKnownColour(colour)
                     ?? new ImmutableSolidColorBrush(new Color(colour.Alpha, colour.Red, colour.Green, colour.Blue));
    }

    public ConstantAvaloniaColourBrush(IImmutableSolidColorBrush brush) {
        this.Color = brush.Color.ToSKColor();
        this.Brush = (ImmutableSolidColorBrush) brush;
    }

    private static IImmutableSolidColorBrush? GetBrushFromKnownColour(SKColor colour) {
        switch ((uint) colour) {
            case 0x00FFFFFF: return Brushes.Transparent;
            case 0xFFF0F8FF: return Brushes.AliceBlue;
            case 0xFFFAEBD7: return Brushes.AntiqueWhite;
            case 0xFF00FFFF: return Brushes.Aqua;
            case 0xFFFF00FF: return Brushes.Fuchsia;
            case 0xFF7FFFD4: return Brushes.Aquamarine;
            case 0xFFF0FFFF: return Brushes.Azure;
            case 0xFFF5F5DC: return Brushes.Beige;
            case 0xFFFFE4C4: return Brushes.Bisque;
            case 0xFF000000: return Brushes.Black;
            case 0xFFFFEBCD: return Brushes.BlanchedAlmond;
            case 0xFF0000FF: return Brushes.Blue;
            case 0xFF8A2BE2: return Brushes.BlueViolet;
            case 0xFFA52A2A: return Brushes.Brown;
            case 0xFFDEB887: return Brushes.BurlyWood;
            case 0xFF5F9EA0: return Brushes.CadetBlue;
            case 0xFF7FFF00: return Brushes.Chartreuse;
            case 0xFFD2691E: return Brushes.Chocolate;
            case 0xFFFF7F50: return Brushes.Coral;
            case 0xFF6495ED: return Brushes.CornflowerBlue;
            case 0xFFFFF8DC: return Brushes.Cornsilk;
            case 0xFFDC143C: return Brushes.Crimson;
            case 0xFF00008B: return Brushes.DarkBlue;
            case 0xFF008B8B: return Brushes.DarkCyan;
            case 0xFFB8860B: return Brushes.DarkGoldenrod;
            case 0xFFA9A9A9: return Brushes.DarkGray;
            case 0xFF006400: return Brushes.DarkGreen;
            case 0xFFBDB76B: return Brushes.DarkKhaki;
            case 0xFF8B008B: return Brushes.DarkMagenta;
            case 0xFF556B2F: return Brushes.DarkOliveGreen;
            case 0xFFFF8C00: return Brushes.DarkOrange;
            case 0xFF9932CC: return Brushes.DarkOrchid;
            case 0xFF8B0000: return Brushes.DarkRed;
            case 0xFFE9967A: return Brushes.DarkSalmon;
            case 0xFF8FBC8B: return Brushes.DarkSeaGreen;
            case 0xFF483D8B: return Brushes.DarkSlateBlue;
            case 0xFF2F4F4F: return Brushes.DarkSlateGray;
            case 0xFF00CED1: return Brushes.DarkTurquoise;
            case 0xFF9400D3: return Brushes.DarkViolet;
            case 0xFFFF1493: return Brushes.DeepPink;
            case 0xFF00BFFF: return Brushes.DeepSkyBlue;
            case 0xFF696969: return Brushes.DimGray;
            case 0xFF1E90FF: return Brushes.DodgerBlue;
            case 0xFFB22222: return Brushes.Firebrick;
            case 0xFFFFFAF0: return Brushes.FloralWhite;
            case 0xFF228B22: return Brushes.ForestGreen;
            case 0xFFDCDCDC: return Brushes.Gainsboro;
            case 0xFFF8F8FF: return Brushes.GhostWhite;
            case 0xFFFFD700: return Brushes.Gold;
            case 0xFFDAA520: return Brushes.Goldenrod;
            case 0xFF808080: return Brushes.Gray;
            case 0xFF008000: return Brushes.Green;
            case 0xFFADFF2F: return Brushes.GreenYellow;
            case 0xFFF0FFF0: return Brushes.Honeydew;
            case 0xFFFF69B4: return Brushes.HotPink;
            case 0xFFCD5C5C: return Brushes.IndianRed;
            case 0xFF4B0082: return Brushes.Indigo;
            case 0xFFFFFFF0: return Brushes.Ivory;
            case 0xFFF0E68C: return Brushes.Khaki;
            case 0xFFE6E6FA: return Brushes.Lavender;
            case 0xFFFFF0F5: return Brushes.LavenderBlush;
            case 0xFF7CFC00: return Brushes.LawnGreen;
            case 0xFFFFFACD: return Brushes.LemonChiffon;
            case 0xFFADD8E6: return Brushes.LightBlue;
            case 0xFFF08080: return Brushes.LightCoral;
            case 0xFFE0FFFF: return Brushes.LightCyan;
            case 0xFFFAFAD2: return Brushes.LightGoldenrodYellow;
            case 0xFFD3D3D3: return Brushes.LightGray;
            case 0xFF90EE90: return Brushes.LightGreen;
            case 0xFFFFB6C1: return Brushes.LightPink;
            case 0xFFFFA07A: return Brushes.LightSalmon;
            case 0xFF20B2AA: return Brushes.LightSeaGreen;
            case 0xFF87CEFA: return Brushes.LightSkyBlue;
            case 0xFF778899: return Brushes.LightSlateGray;
            case 0xFFB0C4DE: return Brushes.LightSteelBlue;
            case 0xFFFFFFE0: return Brushes.LightYellow;
            case 0xFF00FF00: return Brushes.Lime;
            case 0xFF32CD32: return Brushes.LimeGreen;
            case 0xFFFAF0E6: return Brushes.Linen;
            case 0xFF800000: return Brushes.Maroon;
            case 0xFF66CDAA: return Brushes.MediumAquamarine;
            case 0xFF0000CD: return Brushes.MediumBlue;
            case 0xFFBA55D3: return Brushes.MediumOrchid;
            case 0xFF9370DB: return Brushes.MediumPurple;
            case 0xFF3CB371: return Brushes.MediumSeaGreen;
            case 0xFF7B68EE: return Brushes.MediumSlateBlue;
            case 0xFF00FA9A: return Brushes.MediumSpringGreen;
            case 0xFF48D1CC: return Brushes.MediumTurquoise;
            case 0xFFC71585: return Brushes.MediumVioletRed;
            case 0xFF191970: return Brushes.MidnightBlue;
            case 0xFFF5FFFA: return Brushes.MintCream;
            case 0xFFFFE4E1: return Brushes.MistyRose;
            case 0xFFFFE4B5: return Brushes.Moccasin;
            case 0xFFFFDEAD: return Brushes.NavajoWhite;
            case 0xFF000080: return Brushes.Navy;
            case 0xFFFDF5E6: return Brushes.OldLace;
            case 0xFF808000: return Brushes.Olive;
            case 0xFF6B8E23: return Brushes.OliveDrab;
            case 0xFFFFA500: return Brushes.Orange;
            case 0xFFFF4500: return Brushes.OrangeRed;
            case 0xFFDA70D6: return Brushes.Orchid;
            case 0xFFEEE8AA: return Brushes.PaleGoldenrod;
            case 0xFF98FB98: return Brushes.PaleGreen;
            case 0xFFAFEEEE: return Brushes.PaleTurquoise;
            case 0xFFDB7093: return Brushes.PaleVioletRed;
            case 0xFFFFEFD5: return Brushes.PapayaWhip;
            case 0xFFFFDAB9: return Brushes.PeachPuff;
            case 0xFFCD853F: return Brushes.Peru;
            case 0xFFFFC0CB: return Brushes.Pink;
            case 0xFFDDA0DD: return Brushes.Plum;
            case 0xFFB0E0E6: return Brushes.PowderBlue;
            case 0xFF800080: return Brushes.Purple;
            case 0xFFFF0000: return Brushes.Red;
            case 0xFFBC8F8F: return Brushes.RosyBrown;
            case 0xFF4169E1: return Brushes.RoyalBlue;
            case 0xFF8B4513: return Brushes.SaddleBrown;
            case 0xFFFA8072: return Brushes.Salmon;
            case 0xFFF4A460: return Brushes.SandyBrown;
            case 0xFF2E8B57: return Brushes.SeaGreen;
            case 0xFFFFF5EE: return Brushes.SeaShell;
            case 0xFFA0522D: return Brushes.Sienna;
            case 0xFFC0C0C0: return Brushes.Silver;
            case 0xFF87CEEB: return Brushes.SkyBlue;
            case 0xFF6A5ACD: return Brushes.SlateBlue;
            case 0xFF708090: return Brushes.SlateGray;
            case 0xFFFFFAFA: return Brushes.Snow;
            case 0xFF00FF7F: return Brushes.SpringGreen;
            case 0xFF4682B4: return Brushes.SteelBlue;
            case 0xFFD2B48C: return Brushes.Tan;
            case 0xFF008080: return Brushes.Teal;
            case 0xFFD8BFD8: return Brushes.Thistle;
            case 0xFFFF6347: return Brushes.Tomato;
            case 0xFF40E0D0: return Brushes.Turquoise;
            case 0xFFEE82EE: return Brushes.Violet;
            case 0xFFF5DEB3: return Brushes.Wheat;
            case 0xFFFFFFFF: return Brushes.White;
            case 0xFFF5F5F5: return Brushes.WhiteSmoke;
            case 0xFFFFFF00: return Brushes.Yellow;
            case 0xFF9ACD32: return Brushes.YellowGreen;
            default:         return null;
        }
    }
}

public sealed class ConstantAvaloniaLinearGradientBrush : AvaloniaColourBrush, ILinearGradientColourBrush {
    public override ImmutableLinearGradientBrush Brush { get; }

    public ConstantAvaloniaLinearGradientBrush(ImmutableLinearGradientBrush brush) {
        this.Brush = brush ?? throw new ArgumentNullException(nameof(brush));
    }
}

public sealed class ConstantAvaloniaRadialGradientBrush : AvaloniaColourBrush, IRadialGradientColourBrush {
    public override ImmutableRadialGradientBrush Brush { get; }

    public ConstantAvaloniaRadialGradientBrush(ImmutableRadialGradientBrush brush) {
        this.Brush = brush ?? throw new ArgumentNullException(nameof(brush));
    }
}

public class StaticAvaloniaColourBrush : AvaloniaColourBrush, IStaticColourBrush {
    public string ThemeKey { get; }

    public override IImmutableBrush? Brush { get; }

    internal StaticAvaloniaColourBrush(string themeKey, IImmutableBrush? brush) {
        this.ThemeKey = themeKey;
        this.Brush = brush;
    }
}

public sealed class DynamicAvaloniaColourBrush : AvaloniaColourBrush, IDynamicColourBrush {
    public delegate void ChangeHandler(DynamicAvaloniaColourBrush sender, object? state);
    
    private readonly Application App = Application.Current ?? throw new Exception("No app");
    private readonly BrushManagerImpl brushManager;
    private List<UsageToken>? handlers;
    private EventHandler? handleThemeChanged;
    private EventHandler<ResourcesChangedEventArgs>? handleResourcesChanged;

    public string ThemeKey { get; }

    public override IBrush? Brush => this.CurrentBrush;

    public int ReferenceCount { get; private set; }

    /// <summary>
    /// Gets the fully resolved brush
    /// </summary>
    public IBrush? CurrentBrush { get; private set; }

    public event DynamicColourBrushChangedEventHandler? BrushChanged;

    internal DynamicAvaloniaColourBrush(BrushManagerImpl brushManager, string themeKey) {
        this.brushManager = brushManager;
        this.ThemeKey = themeKey;
    }

    /// <summary>
    /// Subscribe to this brush using the given brush handler. This is more like
    /// an "IncrementReferences" method, with an optional brush change handler.
    /// <para>
    /// The returned disposable unsubscribes and MUST be called when this brush is no longer
    /// in use, because otherwise this brush will always be listening to application
    /// theme and resource change events in order to notify listeners of brush changes 
    /// </para>
    /// </summary>
    /// <param name="brushChangeHandler">An optional handler for when our internal brush changes for any reason</param>
    /// <param name="state">An optional parameter passed to the change handler</param>
    /// <param name="invokeHandlerImmediately">True to invoke the given handler in this method if we currently have a valid brush</param>
    /// <returns>A disposable to unsubscribe</returns>
    public IDisposable Subscribe(ChangeHandler brushChangeHandler, object? state, bool invokeHandlerImmediately = true) {
        ApplicationPFX.Instance.Dispatcher.VerifyAccess();

        UsageToken token = new UsageToken(this, brushChangeHandler, state);

        if (this.ReferenceCount++ == 0) {
            // We expect the handler list to be empty due to the logic in Unsubscribe
            Debug.Assert(this.handlers == null || this.handlers.Count < 1);

            if (invokeHandlerImmediately)
                (this.handlers ??= []).Add(token);

            this.HookResourceAndFindBrush();

            if (!invokeHandlerImmediately)
                (this.handlers ??= []).Add(token);
        }
        else {
            Debug.Assert(this.handlers != null && this.handlers.Count > 0);
            this.handlers.Add(token);

            if (invokeHandlerImmediately && this.CurrentBrush != null) {
                brushChangeHandler(this, state);
            }
        }

        return token;
    }

    private void OnThemeChanged(object? sender, EventArgs e) {
        this.FindBrush();
    }

    private void Unsubscribe(UsageToken token) {
        ApplicationPFX.Instance.Dispatcher.VerifyAccess();
        if (this.ReferenceCount == 0) {
            throw new InvalidOperationException("Excessive unsubscribe count");
        }

        Debug.Assert(this.handlers != null);
        bool removed = this.handlers.Remove(token);
        Debug.Assert(removed);

        if (--this.ReferenceCount == 0) {
            this.UnhookResourceAndClearBrush();
        }
    }

    private void OnResourcesChanged(object? sender, ResourcesChangedEventArgs e) => this.FindBrush();

    private void FindBrush() {
        if (this.App.TryGetResource(this.ThemeKey, this.App.ActualThemeVariant, out object? value)) {
            switch (value) {
                case IBrush brush: {
                    if (!ReferenceEquals(this.CurrentBrush, brush)) {
                        this.CurrentBrush = brush;
                        this.NotifyHandlersBrushChanged();
                    }

                    return;
                }
                case Color colour: {
                    // Already have the same colour, and we have an immutable brush, so no need to notify handlers of changes
                    if (this.CurrentBrush is ImmutableSolidColorBrush currBrush && currBrush.Color == colour) {
                        return;
                    }

                    AppLogger.Instance.WriteLine($"[{nameof(DynamicAvaloniaColourBrush)}] Found resource with key '{this.ThemeKey}', converted to brush");
                    this.CurrentBrush = new ImmutableSolidColorBrush(colour);
                    this.NotifyHandlersBrushChanged();
                    return;
                }
            }
        }

        if (this.CurrentBrush != null) {
            this.CurrentBrush = null;
            this.NotifyHandlersBrushChanged();
            AppLogger.Instance.WriteLine($"[{nameof(DynamicAvaloniaColourBrush)}] Failed to find brush resource with key '{this.ThemeKey}'");
        }
    }

    private void NotifyHandlersBrushChanged() {
        if (this.handlers != null) {
            foreach (UsageToken token in this.handlers) {
                token.changeHandler(this, token.state);
            }
        }

        this.BrushChanged?.Invoke(this);
    }

    private void HookResourceAndFindBrush() {
        this.App.ActualThemeVariantChanged += (this.handleThemeChanged ??= this.OnThemeChanged);
        this.App.ResourcesChanged += (this.handleResourcesChanged ??= this.OnResourcesChanged);
        this.FindBrush();
    }

    private void UnhookResourceAndClearBrush() {
        // Since token.invalidatedHandler cannot change and is readonly,
        // it should be impossible for it to not get removed
        Debug.Assert(this.handlers == null || this.handlers.Count < 1);

        this.App.ActualThemeVariantChanged -= this.handleThemeChanged;
        this.App.ResourcesChanged -= this.handleResourcesChanged;

        if (this.CurrentBrush != null) {
            this.CurrentBrush = null;
            this.NotifyHandlersBrushChanged();
        }
    }

    private class UsageToken(DynamicAvaloniaColourBrush owner, ChangeHandler handler, object? state) : IDisposable {
        private DynamicAvaloniaColourBrush? owner = owner;
        public readonly ChangeHandler changeHandler = handler;
        public readonly object? state = state;

        public void Dispose() {
            ApplicationPFX.Instance.Dispatcher.VerifyAccess();
            ObjectDisposedException.ThrowIf(this.owner == null, this);
            this.owner.Unsubscribe(this);
            this.owner = null;
        }
    }
}