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
using PFXToolKitUI.Avalonia.Themes.BrushFactories;
using PFXToolKitUI.Themes;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.Avalonia.Utils;

public delegate void ColourBrushHandlerBrushChangedEventHandler(ColourBrushHandler sender, IColourBrush? oldBrush, IColourBrush? newBrush);

public delegate void ColourBrushHandlerCurrentBrushChangedEventHandler(ColourBrushHandler sender, IBrush? oldCurrentBrush, IBrush? newCurrentBrush);

/// <summary>
/// Manages a <see cref="PFXToolKitUI.Themes.IColourBrush"/> applied to a property of a control, while also managing dynamic brush changes
/// </summary>
public sealed class ColourBrushHandler {
    private AvaloniaObject? myTarget;
    private IColourBrush? myBrush;
    private IDisposable? myBrushSubscription;
    private IBrush? currentBrush;

    /// <summary>
    /// Gets or sets the brush
    /// </summary>
    public IColourBrush? Brush {
        get => this.myBrush;
        set {
            IColourBrush? oldBrush = this.myBrush;
            if (value != oldBrush) {
                this.DisposeSubscription();
                this.myBrush = value;
                this.BrushChanged?.Invoke(this, oldBrush, value);
                this.OnBrushOrTargetChanged();
            }
        }
    }

    public AvaloniaProperty<IBrush?> Property { get; }

    /// <summary>
    /// Gets the brush currently assigned to the target control. Null when <see cref="Brush"/> is null or there is no target control
    /// </summary>
    public IBrush? CurrentBrush {
        get => this.currentBrush;
        private set => PropertyHelper.SetAndRaiseINE(ref this.currentBrush, value, this, static (t, o, n) => t.CurrentBrushChanged?.Invoke(t, o, n));
    }

    /// <summary>
    /// Note this is fired before <see cref="CurrentBrush"/> is updated, and we
    /// may have no target set so it might not get updated at all
    /// </summary>
    public event ColourBrushHandlerBrushChangedEventHandler? BrushChanged;
    
    public event ColourBrushHandlerCurrentBrushChangedEventHandler? CurrentBrushChanged;

    public ColourBrushHandler(AvaloniaProperty<IBrush?> property) {
        this.Property = property;
    }

    public void SetTarget(AvaloniaObject? target) {
        if (this.myTarget != target) {
            this.myTarget = target;
            this.OnBrushOrTargetChanged();
        }
    }

    private void OnBrushOrTargetChanged() {
        if (this.myTarget != null && this.myBrush != null) {
            if (this.myBrushSubscription == null && this.myBrush is DynamicAvaloniaColourBrush dynBrush) {
                this.myBrushSubscription = dynBrush.Subscribe((b, s) => ((ColourBrushHandler) s!).SetTargetBrushValue(b.CurrentBrush), this);
            }

            this.SetTargetBrushValue(((AvaloniaColourBrush) this.myBrush).Brush);
        }
        else {
            this.DisposeSubscription();
            this.CurrentBrush = null;
        }
    }

    private void SetTargetBrushValue(IBrush? brush) {
        this.myTarget!.SetValue(this.Property, brush);
        this.CurrentBrush = brush;
    }
    
    private void DisposeSubscription() {
        this.myBrushSubscription?.Dispose();
        this.myBrushSubscription = null;
    }
}