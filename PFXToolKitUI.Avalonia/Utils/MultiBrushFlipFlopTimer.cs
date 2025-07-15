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
using PFXToolKitUI.Avalonia.Themes.BrushFactories;
using PFXToolKitUI.Themes;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.Avalonia.Utils;

public readonly struct BrushExchange(AvaloniaObject target, AvaloniaProperty property, IColourBrush? lowBrush, IColourBrush? highBrush) {
    public readonly AvaloniaObject Target = target;
    public readonly AvaloniaProperty Property = property;
    public readonly IColourBrush? LowBrush = lowBrush;
    public readonly IColourBrush? HighBrush = highBrush;
}

public class MultiBrushFlipFlopTimer : FlipFlopTimer {
    private readonly BrushExchange[] exchanges;
    private (IDisposable?, IDisposable?)[]? subscriptions;

    public MultiBrushFlipFlopTimer(TimeSpan interval, IEnumerable<BrushExchange> exchanges) : base(interval) {
        this.exchanges = exchanges.ToArray();
        foreach (BrushExchange exchange in this.exchanges) {
            if (exchange.Target == null)
                throw new ArgumentException("One of the target objects were null");
            if (exchange.Property == null)
                throw new ArgumentException("One of the target properties were null");
        }
    }

    /// <summary>
    /// Sets the control that we update the property of. This method will subscribe to changes
    /// of dynamic brushes if that is what the low and/or high brushes are
    /// </summary>
    /// <param name="target"></param>
    /// <param name="property"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public void EnableTargets() {
        if (this.subscriptions != null)
            throw new InvalidOperationException("Targets already enabled. Use " + nameof(this.ClearTarget) + " first");

        this.subscriptions = new (IDisposable?, IDisposable?)[this.exchanges.Length];
        for (int i = 0; i < this.exchanges.Length; i++) {
            BrushExchange exchange = this.exchanges[i];
            IDisposable? lowBrushSubscription = null;
            IDisposable? highBrushSubscription = null;
            if (exchange.LowBrush is DynamicAvaloniaColourBrush dLowBrush)
                lowBrushSubscription = dLowBrush.Subscribe(obj => {
                    Debug.Assert(this.subscriptions != null);
                    if (!this.IsHigh)
                        exchange.Target.SetValue(exchange.Property, obj);
                });

            if (exchange.HighBrush is DynamicAvaloniaColourBrush dHighBrush)
                highBrushSubscription = dHighBrush.Subscribe(obj => {
                    Debug.Assert(this.subscriptions != null);
                    if (this.IsHigh)
                        exchange.Target.SetValue(exchange.Property, obj);
                });

            this.subscriptions[i] = (lowBrushSubscription, highBrushSubscription);
        }

        this.UpdateBrushes(this.IsHigh);
    }

    /// <summary>
    /// Clears the target object, if present. This will also unsubscribe from dynamic brush
    /// changes if previously subscribed in <see cref="SetTarget"/>
    /// </summary>
    public void ClearTarget() {
        if (this.subscriptions == null)
            return;

        foreach ((IDisposable?, IDisposable?) entry in this.subscriptions) {
            entry.Item1?.Dispose();
            entry.Item2?.Dispose();
        }

        this.subscriptions = null;
    }

    protected override void OnIsHighChanged(bool isHigh) {
        base.OnIsHighChanged(isHigh);
        this.UpdateBrushes(isHigh);
    }

    private void UpdateBrushes(bool isHigh) {
        if (this.subscriptions == null)
            return;

        foreach (BrushExchange exchange in this.exchanges) {
            exchange.Target.SetValue(exchange.Property, isHigh ? ((AvaloniaColourBrush?) exchange.HighBrush)?.Brush : ((AvaloniaColourBrush?) exchange.LowBrush)?.Brush);
        }
    }
}