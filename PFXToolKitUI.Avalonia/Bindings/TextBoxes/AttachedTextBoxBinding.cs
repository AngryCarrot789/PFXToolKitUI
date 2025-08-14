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
    public static readonly AttachedProperty<IBrush?> OverlayBrushProperty = AvaloniaProperty.RegisterAttached<AvaloniaObject, IBrush?>("OverlayBrush",  typeof(AttachedTextBoxBinding));
    private static readonly AttachedProperty<BrushFlipFlopTimer?> FlipFlopProperty = AvaloniaProperty.RegisterAttached<TextBox, BrushFlipFlopTimer?>("FlipFlop", typeof(AttachedTextBoxBinding));

    static AttachedTextBoxBinding() {
        IsValueDifferentProperty.Changed.Subscribe(new AnonymousObserver<AvaloniaPropertyChangedEventArgs<bool>>(OnIsValueDifferentChanged));
    }

    private static void OnIsValueDifferentChanged(AvaloniaPropertyChangedEventArgs<bool> e) {
        if (e.NewValue.GetValueOrDefault()) {
            BrushFlipFlopTimer? flipFlop = e.Sender.GetValue(FlipFlopProperty);
            if (flipFlop == null) {
                IColourBrush brushLow = lowBrush ??= BrushManager.Instance.GetDynamicThemeBrush("TextBox.ValueChanged.Low.Border");
                IColourBrush brushHigh = highBrush ??= BrushManager.Instance.GetDynamicThemeBrush("TextBox.ValueChanged.High.Border");
                
                e.Sender.SetValue(FlipFlopProperty, flipFlop = new BrushFlipFlopTimer(TimeSpan.FromSeconds(0.4), brushLow, brushHigh) {StartHigh = true});
            }
            
            flipFlop.SetTarget(e.Sender, OverlayBrushProperty);
            flipFlop.IsEnabled = true;
            
            // ((TextBox) e.Sender).DetachedFromVisualTree += OnDetachedFromVisualTree;
        }
        else {
            if (e.OldValue.HasValue) {
                BrushFlipFlopTimer flipFlop = e.Sender.GetValue(FlipFlopProperty)!;
                Debug.Assert(flipFlop != null);
                flipFlop.IsEnabled = false;
                flipFlop.ClearTarget();
            }
            else {
                Debug.Assert(e.Sender.GetValue(FlipFlopProperty) == null);
            }
            
            e.Sender.ClearValue(OverlayBrushProperty);
        }
    }

    // private static void OnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e) {
    //     ((TextBox) sender!).DetachedFromVisualTree -= OnDetachedFromVisualTree;
    //     SetIsValueDifferent((TextBox) sender, false);
    // }
    
    public static void SetIsValueDifferent(AvaloniaObject obj, bool value) => obj.SetValue(IsValueDifferentProperty, value);
    public static bool GetIsValueDifferent(AvaloniaObject obj) => obj.GetValue(IsValueDifferentProperty);
    public static void SetOverlayBrush(AvaloniaObject obj, IBrush? value) => obj.SetValue(OverlayBrushProperty, value);
    public static IBrush? GetOverlayBrush(AvaloniaObject obj) => obj.GetValue(OverlayBrushProperty);
}