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
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Skia;
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
        if (colour == SKColors.Transparent)
            this.Brush = (ImmutableSolidColorBrush) Brushes.Transparent;
        else
            this.Brush = new ImmutableSolidColorBrush(new Color(colour.Alpha, colour.Red, colour.Green, colour.Blue));
    }
    
    public ConstantAvaloniaColourBrush(IImmutableSolidColorBrush brush) {
        this.Color = brush.Color.ToSKColor();
        this.Brush = (ImmutableSolidColorBrush) brush;
    }
}

public sealed class ConstantAvaloniaLinearGradientBrush : AvaloniaColourBrush, ILinearGradientColourBrush {
    public override ImmutableLinearGradientBrush Brush { get; }

    public ConstantAvaloniaLinearGradientBrush(ImmutableLinearGradientBrush brush) {
        this.Brush = brush;
    }
}

public sealed class ConstantAvaloniaRadialGradientBrush : AvaloniaColourBrush, IRadialGradientColourBrush {
    public override ImmutableRadialGradientBrush Brush { get; }

    public ConstantAvaloniaRadialGradientBrush(ImmutableRadialGradientBrush brush) {
        this.Brush = brush;
    }
}

public class StaticAvaloniaColourBrush : AvaloniaColourBrush, IStaticColourBrush {
    public string ThemeKey { get; }

    public override IImmutableBrush? Brush { get; }

    public StaticAvaloniaColourBrush(string themeKey, IImmutableBrush? brush) {
        this.ThemeKey = themeKey;
        this.Brush = brush;
    }
}

public sealed class DynamicAvaloniaColourBrush : AvaloniaColourBrush, IDynamicColourBrush {
    private int usageCounter;
    private List<Action<IBrush?>>? handlers;
    
    public string ThemeKey { get; }

    public override IBrush? Brush => this.CurrentBrush;

    public int ReferenceCount => this.usageCounter;
    
    /// <summary>
    /// Gets the fully resolved brush
    /// </summary>
    public IBrush? CurrentBrush { get; private set; }

    public event DynamicColourBrushChangedEventHandler? BrushChanged;

    public DynamicAvaloniaColourBrush(string themeKey) {
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
    /// <param name="onBrushChanged">An optional handler for when our internal brush changes for any reason</param>
    /// <param name="invokeHandlerImmediately">True to invoke the given handler in this method if we currently have a valid brush</param>
    /// <returns>A disposable to unsubscribe</returns>
    public IDisposable Subscribe(Action<IBrush?>? onBrushChanged, bool invokeHandlerImmediately = true) {
        ApplicationPFX.Instance.Dispatcher.VerifyAccess();

        if (onBrushChanged != null)
            (this.handlers ??= new List<Action<IBrush?>>()).Add(onBrushChanged);

        if (this.usageCounter++ == 0) {
            // We expect the handler list to be empty due to the logic in Unsubscribe
            Debug.Assert(onBrushChanged != null ? this.handlers?.Count == 1 : (this.handlers == null || this.handlers.Count < 1));

            global::Avalonia.Application.Current!.ActualThemeVariantChanged += this.OnApplicationThemeChanged;
            global::Avalonia.Application.Current.ResourcesChanged += this.CurrentOnResourcesChanged;
            this.FindBrush();
        }
        else if (invokeHandlerImmediately && onBrushChanged != null && this.CurrentBrush != null) {
            onBrushChanged(this.CurrentBrush);
        }

        return new UsageToken(this, onBrushChanged);
    }

    private void OnApplicationThemeChanged(object? sender, EventArgs e) {
        this.FindBrush();
    }

    private void Unsubscribe(UsageToken token) {
        ApplicationPFX.Instance.Dispatcher.VerifyAccess();

        if (this.usageCounter == 0) {
            throw new InvalidOperationException("Excessive unsubscribe count");
        }

        if (token.invalidatedHandler != null) {
            Debug.Assert(this.handlers != null);
            this.handlers.Remove(token.invalidatedHandler);
        }

        if (--this.usageCounter == 0) {
            // Since token.invalidatedHandler cannot change and is readonly,
            // it should be impossible for it to not get removed
            Debug.Assert(this.handlers == null || this.handlers.Count < 1);

            global::Avalonia.Application.Current!.ActualThemeVariantChanged -= this.OnApplicationThemeChanged;
            global::Avalonia.Application.Current.ResourcesChanged -= this.CurrentOnResourcesChanged;

            if (this.CurrentBrush != null) {
                this.CurrentBrush = null;
                this.NotifyHandlersBrushChanged();
            }
        }
    }

    private void CurrentOnResourcesChanged(object? sender, ResourcesChangedEventArgs e) => this.FindBrush();

    private void FindBrush() {
        if (global::Avalonia.Application.Current!.TryGetResource(this.ThemeKey, global::Avalonia.Application.Current.ActualThemeVariant, out object? value)) {
            if (value is IBrush brush) {
                if (!ReferenceEquals(this.CurrentBrush, brush)) {
                    this.CurrentBrush = brush;
                    this.NotifyHandlersBrushChanged();
                }

                return;
            }
            else {
                // Try to convert to an immutable brush in case the user specified a colour key
                if (value is Color colour) {
                    // Already have the same colour, and we have an immutable brush,
                    // so no need to notify handlers of changes
                    if (this.CurrentBrush is ImmutableSolidColorBrush currBrush && currBrush.Color == colour) {
                        return;
                    }

                    AppLogger.Instance.WriteLine($"[{nameof(DynamicAvaloniaColourBrush)}] Found resource with key '{this.ThemeKey}', converted to brush");
                    this.CurrentBrush = new ImmutableSolidColorBrush(colour);
                    this.NotifyHandlersBrushChanged();
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
            foreach (Action<IBrush?> action in this.handlers) {
                action(this.CurrentBrush);
            }
        }

        this.BrushChanged?.Invoke(this);
    }

    private class UsageToken : IDisposable {
        private DynamicAvaloniaColourBrush? brush;
        public readonly Action<IBrush?>? invalidatedHandler;

        public UsageToken(DynamicAvaloniaColourBrush brush, Action<IBrush?>? invalidatedHandler) {
            this.brush = brush;
            this.invalidatedHandler = invalidatedHandler;
        }

        public void Dispose() {
            if (this.brush == null)
                throw new ObjectDisposedException("this", "Already disposed");

            this.brush.Unsubscribe(this);
            this.brush = null;
        }
    }
}