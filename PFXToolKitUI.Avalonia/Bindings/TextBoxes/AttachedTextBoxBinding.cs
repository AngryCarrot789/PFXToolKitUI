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
using Avalonia.Reactive;
using PFXToolKitUI.Avalonia.Utils;
using PFXToolKitUI.Themes;

namespace PFXToolKitUI.Avalonia.Bindings.TextBoxes;

public static class AttachedTextBoxBinding {
    private static IDynamicColourBrush? lowBrush, highBrush;

    /// <summary>
    /// A property used to tell whether the text in the text box is effectively different from the model's text.
    /// This is not an absolute truth, since it will be true when a text box's text is modified but not applied,
    /// even if the user presses CTRL+Z to undo their change.
    /// <para>
    /// This is just used for visual feedback to the user that they haven't confirmed their changes
    /// </para>
    /// </summary>
    public static readonly AttachedProperty<bool> IsValueDifferentProperty = AvaloniaProperty.RegisterAttached<AvaloniaObject, bool>("IsValueDifferent",  typeof(AttachedTextBoxBinding));
    public static readonly AttachedProperty<IBrush?> OverlayBorderBrushProperty = AvaloniaProperty.RegisterAttached<AvaloniaObject, IBrush?>("OverlayBrush",  typeof(AttachedTextBoxBinding));
    public static readonly AttachedProperty<Thickness> OverlayBorderThicknessProperty = AvaloniaProperty.RegisterAttached<AvaloniaObject, Thickness>("OverlayBorderThickness",  typeof(AttachedTextBoxBinding), new Thickness(1));
    private static readonly AttachedProperty<BrushFlipFlopTimer?> FlipFlopProperty = AvaloniaProperty.RegisterAttached<TextBox, BrushFlipFlopTimer?>("FlipFlop", typeof(AttachedTextBoxBinding));

    // Re-use flipflops to save on some allocations
    private static readonly List<BrushFlipFlopTimer> CachedFlipFlops = new List<BrushFlipFlopTimer>();
    private const int MaxCachedFlipFlops = 10;
    
    static AttachedTextBoxBinding() {
        IsValueDifferentProperty.Changed.Subscribe(new AnonymousObserver<AvaloniaPropertyChangedEventArgs<bool>>(OnIsValueDifferentChanged));
    }

    private static void OnIsValueDifferentChanged(AvaloniaPropertyChangedEventArgs<bool> e) {
        if (e.NewValue.GetValueOrDefault()) {
            BrushFlipFlopTimer? flipFlop = e.Sender.GetValue(FlipFlopProperty);
            if (flipFlop == null) {
                if (CachedFlipFlops.Count > 0) {
                    flipFlop = CachedFlipFlops[CachedFlipFlops.Count - 1];
                    CachedFlipFlops.RemoveAt(CachedFlipFlops.Count - 1);
                }
                else {
                    IColourBrush brushLow = lowBrush ??= BrushManager.Instance.GetDynamicThemeBrush("TextBox.ValueChanged.Low.Border");
                    IColourBrush brushHigh = highBrush ??= BrushManager.Instance.GetDynamicThemeBrush("TextBox.ValueChanged.High.Border");
                    flipFlop = new BrushFlipFlopTimer(TimeSpan.FromSeconds(0.4), brushLow, brushHigh) { StartHigh = true };
                }
                
                e.Sender.SetValue(FlipFlopProperty, flipFlop);
            }
            
            flipFlop.SetTarget(e.Sender, OverlayBorderBrushProperty);
            flipFlop.IsEnabled = true;
            
            // ((TextBox) e.Sender).DetachedFromVisualTree += OnDetachedFromVisualTree;
        }
        else {
            if (e.OldValue.HasValue) {
                BrushFlipFlopTimer flipFlop = e.Sender.GetValue(FlipFlopProperty)!;
                Debug.Assert(flipFlop != null);
                flipFlop.IsEnabled = false;
                flipFlop.ClearTarget();
                if (CachedFlipFlops.Count < MaxCachedFlipFlops) {
                    e.Sender.SetValue(FlipFlopProperty, null);
                    CachedFlipFlops.Add(flipFlop);
                }
            }
            else {
                Debug.Assert(e.Sender.GetValue(FlipFlopProperty) == null);
            }
            
            e.Sender.ClearValue(OverlayBorderBrushProperty);
        }
    }

    // private static void OnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e) {
    //     ((TextBox) sender!).DetachedFromVisualTree -= OnDetachedFromVisualTree;
    //     SetIsValueDifferent((TextBox) sender, false);
    // }
    
    public static void SetIsValueDifferent(AvaloniaObject obj, bool value) => obj.SetValue(IsValueDifferentProperty, value);
    public static bool GetIsValueDifferent(AvaloniaObject obj) => obj.GetValue(IsValueDifferentProperty);
    public static void SetOverlayBorderBrush(AvaloniaObject obj, IBrush? value) => obj.SetValue(OverlayBorderBrushProperty, value);
    public static IBrush? GetOverlayBorderBrush(AvaloniaObject obj) => obj.GetValue(OverlayBorderBrushProperty);
    public static void SetOverlayBorderThickness(AvaloniaObject obj, Thickness value) => obj.SetValue(OverlayBorderThicknessProperty, value);
    public static Thickness GetOverlayBorderThickness(AvaloniaObject obj) => obj.GetValue(OverlayBorderThicknessProperty);
}