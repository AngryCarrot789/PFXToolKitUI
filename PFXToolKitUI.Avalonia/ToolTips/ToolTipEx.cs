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

// #define MEASURE_INHERITANCE_CACHE_HITS_MISSES

using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using PFXToolKitUI.Avalonia.Interactivity;

namespace PFXToolKitUI.Avalonia.ToolTips;

public static class ToolTipEx {
    private static readonly Dictionary<Type, Control> toolTipControlCache = new Dictionary<Type, Control>();

    public static readonly AttachedProperty<Type?> TipTypeProperty = AvaloniaProperty.RegisterAttached<Control, Type?>("TipType", typeof(ToolTipEx));

    public static void SetTipType(Control obj, Type? value) => obj.SetValue(TipTypeProperty, value);
    public static Type? GetTipType(Control obj) => obj.GetValue(TipTypeProperty);

    static ToolTipEx() {
        TipTypeProperty.Changed.AddClassHandler<Control, Type?>(static (s, e) => OnTipTypeChanged(s, e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
        ToolTip.IsOpenProperty.Changed.AddClassHandler<Control, bool>(OnIsToolTipOpenChanged);

        // These events are unbelievably unreliable. ToolTipOpeningEvent is called before ToolTipClosingEvent,
        // because some sort of Close method is called before attempting to open again
        // ToolTip.ToolTipOpeningEvent.AddClassHandler(typeof(Control), OnToolTipOpening);
        // ToolTip.ToolTipClosingEvent.AddClassHandler(typeof(Control), OnToolTipClosing);
    }

    private static void OnTipTypeChanged(Control control, Type? oldTipType, Type? newTipType) {
        if (newTipType != null) {
            if (!toolTipControlCache.TryGetValue(newTipType, out Control? rip)) {
                rip = (Control) Activator.CreateInstance(newTipType)! ?? throw new InvalidOperationException("Wut");
                toolTipControlCache[newTipType] = rip = new ToolTipControlEx(rip);
            }

            bool isOpen = ToolTip.GetIsOpen(control);
            if (isOpen)
                ToolTip.SetIsOpen(control, false);

            ToolTip.SetTip(control, rip);

            if (isOpen)
                ToolTip.SetIsOpen(control, true);
        }
        else {
            if (ToolTip.GetIsOpen(control)) {
                ToolTip.SetIsOpen(control, false);
            }

            ToolTip.SetTip(control, null);
        }
    }

    private static void OnIsToolTipOpenChanged(Control control, AvaloniaPropertyChangedEventArgs<bool> e) {
    }

    private static Control GetOrCreateTipControl(Type tipType) {
        if (!toolTipControlCache.TryGetValue(tipType, out Control? tip)) {
            tip = (Control) Activator.CreateInstance(tipType)! ?? throw new InvalidOperationException("Wut");
            toolTipControlCache[tipType] = tip = new ToolTipControlEx(tip);
        }

        return tip;
    }

    private class ToolTipControlEx : ToolTip {
        private static readonly PropertyInfo AdornedControlProperty = typeof(ToolTip).GetProperty("AdornedControl", BindingFlags.Instance | BindingFlags.NonPublic)!;
        // private static readonly MethodInfo UpdatePositionMethod = typeof(Popup).GetMethod("HandlePositionChange", BindingFlags.Instance | BindingFlags.NonPublic)!;

        protected override Type StyleKeyOverride => typeof(ToolTip);

        private Control? AdornedControl => (Control?) AdornedControlProperty.GetValue(this);

        private Control? lastAdornedControl;

        public ToolTipControlEx(Control content) {
            this.Content = content;
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e) {
            base.OnAttachedToVisualTree(e);
            this.EnsureOnClosedInvoked();

            if (this.Content is IToolTipControl tipControl) {
                Control? adorned = this.AdornedControl;
                if (adorned != null) {
                    this.lastAdornedControl = adorned;
                    tipControl.OnOpened(adorned, DataManager.GetFullContextData(adorned));
                }
            }
            
            // ApplicationPFX.Instance.Dispatcher.Post(() => {
            //     if (this.Parent is Popup popup)
            //         UpdatePositionMethod.Invoke(popup, []);
            // }, DispatchPriority.ApplicationIdle);
        }

        private void EnsureOnClosedInvoked() {
            if (this.lastAdornedControl != null) {
                if (this.Content is IToolTipControl tipControl)
                    tipControl.OnClosed(this.lastAdornedControl);

                this.lastAdornedControl = null;
            }
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e) {
            base.OnDetachedFromVisualTree(e);
            this.EnsureOnClosedInvoked();
        }
    }
}