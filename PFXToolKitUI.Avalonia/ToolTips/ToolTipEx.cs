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
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using PFXToolKitUI.Avalonia.Interactivity;

namespace PFXToolKitUI.Avalonia.ToolTips;

public static class ToolTipEx {
    private static readonly Dictionary<Type, Control> toolTipControlCache = new Dictionary<Type, Control>();

    public static readonly AttachedProperty<Type?> TipTypeProperty = AvaloniaProperty.RegisterAttached<Control, Type?>("TipType", typeof(ToolTipEx));
    public static readonly AttachedProperty<object?> TipProperty = AvaloniaProperty.RegisterAttached<Control, object?>("Tip", typeof(ToolTipEx));
    public static readonly AttachedProperty<bool> MoveWithCursorProperty = AvaloniaProperty.RegisterAttached<Control, bool>("MoveWithCursor", typeof(ToolTipEx), defaultValue: true);

    private static ToolTipControlEx? currentShownToolTip;

    static ToolTipEx() {
        TipTypeProperty.Changed.AddClassHandler<Control, Type?>(static (s, e) => OnTipTypeChanged(s, e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
        TipProperty.Changed.AddClassHandler<Control, object?>(static (s, e) => OnTipChanged(s, e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
        InputElement.PointerMovedEvent.AddClassHandler<TopLevel>(OnPreviewPointerMoved, RoutingStrategies.Tunnel, handledEventsToo: true);

        // These events are unbelievably unreliable. ToolTipOpeningEvent is called before ToolTipClosingEvent,
        // because some sort of Close method is called before attempting to open again
        // ToolTip.ToolTipOpeningEvent.AddClassHandler(typeof(Control), OnToolTipOpening);
        // ToolTip.ToolTipClosingEvent.AddClassHandler(typeof(Control), OnToolTipClosing);
    }

    public static void SetTipType(Control obj, Type? value) => obj.SetValue(TipTypeProperty, value);
    public static Type? GetTipType(Control obj) => obj.GetValue(TipTypeProperty);
    public static void SetTip(Control obj, object? value) => obj.SetValue(TipProperty, value);
    public static object? GetTip(Control obj) => obj.GetValue(TipProperty);

    public static void SetMoveWithCursor(Control obj, bool value) => obj.SetValue(MoveWithCursorProperty, value);
    public static bool GetMoveWithCursor(Control obj) => obj.GetValue(MoveWithCursorProperty);

    private static void OnPreviewPointerMoved(TopLevel topLevel, PointerEventArgs e) {
        if (currentShownToolTip != null) {
            Control? control = currentShownToolTip.CurrentlyAdornedControl;
            Debug.Assert(control != null);

            if (GetMoveWithCursor(control!)) {
                currentShownToolTip.UpdatePosition();
            }
        }
    }

    private static void OnTipTypeChanged(Control control, Type? oldTipType, Type? newTipType) {
        if (newTipType != null) {
            if (!toolTipControlCache.TryGetValue(newTipType, out Control? tip)) {
                tip = (Control) Activator.CreateInstance(newTipType)! ?? throw new InvalidOperationException("Wut");
                toolTipControlCache[newTipType] = tip = new ToolTipControlEx(tip);
            }

            ToolTip.AddToolTipOpeningHandler(control, OnToolTipOpening);
            bool isOpen = ToolTip.GetIsOpen(control);
            if (isOpen)
                ToolTip.SetIsOpen(control, false);

            ToolTip.SetTip(control, tip);

            if (isOpen)
                ToolTip.SetIsOpen(control, true);
        }
        else {
            ToolTip.RemoveToolTipOpeningHandler(control, OnToolTipOpening);
            if (ToolTip.GetIsOpen(control)) {
                ToolTip.SetIsOpen(control, false);
            }

            ToolTip.SetTip(control, null);
        }
    }

    private static void OnToolTipOpening(object? sender, CancelRoutedEventArgs e) {
        if (ToolTip.GetTip((Control) sender!) is ToolTipControlEx ex) {
            ex.OnOpening((Control) sender!, e);
        }
    }

    private static void OnTipChanged(Control control, object? oldTip, object? newTip) {
        if (newTip != null) {
            if (ToolTip.GetTip(control) is ToolTipControlEx tip) {
                tip.Content = newTip;
            }
            else {
                bool isOpen = ToolTip.GetIsOpen(control);
                if (isOpen)
                    ToolTip.SetIsOpen(control, false);
                
                tip = new ToolTipControlEx(newTip);
                ToolTip.SetTip(control, tip);

                if (isOpen)
                    ToolTip.SetIsOpen(control, true);
            }
        }
        else {
            if (ToolTip.GetIsOpen(control)) {
                ToolTip.SetIsOpen(control, false);
            }

            ToolTip.SetTip(control, null);
        }
    }

    private class ToolTipControlEx : ToolTip {
        private static readonly PropertyInfo AdornedControlProperty = typeof(ToolTip).GetProperty("AdornedControl", BindingFlags.Instance | BindingFlags.NonPublic)!;
        private static readonly MethodInfo UpdatePositionMethod = typeof(Popup).GetMethod("HandlePositionChange", BindingFlags.Instance | BindingFlags.NonPublic)!;

        protected override Type StyleKeyOverride => typeof(ToolTip);

        private Control? AdornedControl => (Control?) AdornedControlProperty.GetValue(this);

        public Control? CurrentlyAdornedControl { get; private set; }

        public ToolTipControlEx(object content) {
            this.Content = content;
            Style style = new Style((x) => x.OfType<TextBlock>()) {
                Setters = { new Setter(TextBlock.TextWrappingProperty, TextWrapping.Wrap) }
            };
            
            this.Styles.Add(style);
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change) {
            base.OnPropertyChanged(change);
            if (change.Property == ContentProperty) {
                this.EnsureOnClosedInvoked();
                this.TryOpen();
            }
        }

        public void OnOpening(Control sender, CancelRoutedEventArgs e) {
            if (this.Content is IToolTipControl tipControl) {
                tipControl.OnOpening(sender, e);
            }
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e) {
            base.OnAttachedToVisualTree(e);
            this.EnsureOnClosedInvoked();
            this.TryOpen();

            // ApplicationPFX.Instance.Dispatcher.Post(() => {
            //     if (this.Parent is Popup popup)
            //         UpdatePositionMethod.Invoke(popup, []);
            // }, DispatchPriority.ApplicationIdle);
        }

        private void TryOpen() {
            Debug.Assert(currentShownToolTip == null);

            Control? adorned = this.AdornedControl;
            if (adorned != null) {
                currentShownToolTip = this;
                
                this.CurrentlyAdornedControl = adorned;
                if (this.Content is IToolTipControl tipControl) {
                    tipControl.OnOpened(adorned, DataManager.GetFullContextData(adorned));
                }
            }
        }

        public void UpdatePosition() {
            if (this.Parent is Popup popup)
                UpdatePositionMethod.Invoke(popup, []);
        }

        private void EnsureOnClosedInvoked() {
            if (this.CurrentlyAdornedControl != null) {
                Debug.Assert(currentShownToolTip == this);
                currentShownToolTip = null;

                if (this.Content is IToolTipControl tipControl)
                    tipControl.OnClosed(this.CurrentlyAdornedControl);

                this.CurrentlyAdornedControl = null;
            }
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e) {
            base.OnDetachedFromVisualTree(e);
            this.EnsureOnClosedInvoked();
        }
    }
}