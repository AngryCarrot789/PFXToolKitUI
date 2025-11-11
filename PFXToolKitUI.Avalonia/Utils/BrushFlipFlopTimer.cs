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
using PFXToolKitUI.Avalonia.Themes.BrushFactories;
using PFXToolKitUI.Themes;
using PFXToolKitUI.Utils;
using PFXToolKitUI.Utils.Destroying;

namespace PFXToolKitUI.Avalonia.Utils;

public class BrushFlipFlopTimer : FlipFlopTimer {
    private readonly IColourBrush? lowBrush;
    private readonly IColourBrush? highBrush;

    private AvaloniaObject? targetObject;
    private AvaloniaProperty? targetProperty;

    private IDisposable? lowBrushSubscription;
    private IDisposable? highBrushSubscription;

    public BrushFlipFlopTimer(TimeSpan interval, IColourBrush? lowBrush, IColourBrush? highBrush) : base(interval) {
        this.lowBrush = lowBrush;
        this.highBrush = highBrush;
    }

    /// <summary>
    /// Sets the control that we update the property of. This method will subscribe to changes
    /// of dynamic brushes if that is what the low and/or high brushes are
    /// </summary>
    /// <param name="target"></param>
    /// <param name="property"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public void SetTarget(AvaloniaObject target, AvaloniaProperty property) {
        if (this.targetObject != null)
            throw new InvalidOperationException("Target already set. Use " + nameof(this.ClearTarget));

        this.targetObject = target;
        this.targetProperty = property;

        if (this.lowBrush is DynamicAvaloniaColourBrush dLowBrush)
            this.lowBrushSubscription = dLowBrush.Subscribe(static (b, s) => ((BrushFlipFlopTimer) s!).OnLowBrushValueChanged(b.CurrentBrush), this);

        if (this.highBrush is DynamicAvaloniaColourBrush dHighBrush)
            this.highBrushSubscription = dHighBrush.Subscribe(static (b, s) => ((BrushFlipFlopTimer) s!).OnHighBrushValueChanged(b.CurrentBrush), this);
        
        this.UpdateBrush(this.IsHigh);
    }

    /// <summary>
    /// Clears the target object, if present. This will also unsubscribe from dynamic brush
    /// changes if previously subscribed in <see cref="SetTarget"/>
    /// </summary>
    public void ClearTarget() {
        DisposableUtils.Dispose(ref this.lowBrushSubscription);
        DisposableUtils.Dispose(ref this.highBrushSubscription);
        this.targetObject = null;
        this.targetProperty = null;
    }

    private void OnLowBrushValueChanged(IBrush? obj) {
        if (this.targetObject != null && !this.IsHigh) {
            this.targetObject.SetValue(this.targetProperty!, obj);
        }
    }

    private void OnHighBrushValueChanged(IBrush? obj) {
        if (this.targetObject != null && this.IsHigh) {
            this.targetObject.SetValue(this.targetProperty!, obj);
        }
    }

    protected override void OnIsHighChanged(bool isHigh) {
        base.OnIsHighChanged(isHigh);
        this.UpdateBrush(isHigh);
    }

    private void UpdateBrush(bool isHigh) {
        this.targetObject?.SetValue(this.targetProperty!, isHigh ? ((AvaloniaColourBrush?) this.highBrush)?.Brush : ((AvaloniaColourBrush?) this.lowBrush)?.Brush);
    }
}