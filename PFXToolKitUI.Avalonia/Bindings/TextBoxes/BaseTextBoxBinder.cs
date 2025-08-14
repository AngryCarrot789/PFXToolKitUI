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

using System.Runtime.ExceptionServices;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using PFXToolKitUI.Avalonia.Utils;

namespace PFXToolKitUI.Avalonia.Bindings.TextBoxes;

/// <summary>
/// The base class for binding a text box's text to some sort of value within a model
/// </summary>
/// <typeparam name="TModel">The type of model</typeparam>
public abstract class BaseTextBoxBinder<TModel> : BaseBinder<TModel> where TModel : class {
    public delegate void EscapePressedEventHandler(BaseTextBoxBinder<TModel> sender, string oldText);

    public delegate void ValueConfirmedEventHandler(BaseTextBoxBinder<TModel> sender, string oldText);

    private readonly Func<IBinder<TModel>, string, Task<bool>> parseAndUpdate;
    private bool isHandlingChangeModel;

    /// <summary>
    /// Gets or sets if the model value can be updated when the text box loses focus too.
    /// Default is true, which is the recommended value because the user may not
    /// realise their change was undone since they had to click the Enter key to confirm changes.
    /// </summary>
    public bool CanApplyValueOnLostFocus { get; set; } = true;

    /// <summary>
    /// Gets or sets if the text box should be re-focused when the update callback returns false.
    /// Default is true, because it's annoying to the user to have to re-click the text box again
    /// </summary>
    public bool FocusTextBoxOnError { get; set; } = true;

    public bool CanMoveFocusUpwardsOnEscape { get; set; } = true;

    /// <summary>
    /// Fired when the user presses the escape button. Before being fired, focus 
    /// </summary>
    public event EscapePressedEventHandler? EscapePressed;

    /// <summary>
    /// Fired when the user confirms their value, typically by pressing ENTER
    /// </summary>
    public event ValueConfirmedEventHandler? ValueConfirmed;

    /// <summary>
    /// Initialises the <see cref="TextBoxToEventPropertyBinder{TModel}"/> object
    /// </summary>
    /// <param name="eventName">The name of the model's event</param>
    /// <param name="getText">A getter to fetch the model's value as raw text for the text box</param>
    /// <param name="parseAndUpdate">
    /// A function which tries to update the model's value from the text box's text. Returns true on success,
    /// returns false when the text box contains invalid data (and it's assumed it shows a dialog too).
    /// When this returns false, the text box's text is re-selected (if <see cref="FocusTextBoxOnError"/> is true)
    /// </param>
    public BaseTextBoxBinder(Func<IBinder<TModel>, string, Task<bool>> parseAndUpdate) {
        this.parseAndUpdate = parseAndUpdate;
    }

    protected abstract string GetTextCore();

    protected override void UpdateModelOverride() {
    }

    protected override void UpdateControlOverride() {
        TextBox tb = (TextBox) this.myControl!;
        if (this.IsFullyAttached) {
            string newValue = this.GetTextCore();
            tb.Text = newValue;
            BugFix.TextBox_UpdateSelection(tb);
        }
        
        AttachedTextBoxBinding.SetIsValueDifferent(tb, false);
    }

    protected override void CheckAttachControl(Control control) {
        base.CheckAttachControl(control);
        if (!(control is TextBox))
            throw new InvalidOperationException("Attempt to attach non-textbox");
    }

    protected override void OnAttached() {
        TextBox tb = (TextBox) this.Control;
        tb.LostFocus += this.OnLostFocus;
        tb.KeyDown += this.OnKeyDown;
        tb.AddHandler(InputElement.KeyDownEvent, OnKeyDown_Tunnel, RoutingStrategies.Tunnel, handledEventsToo: true);
        AttachedTextBoxBinding.SetIsValueDifferent(tb, false);
    }

    protected override void OnDetached() {
        TextBox tb = (TextBox) this.Control;
        tb.LostFocus -= this.OnLostFocus;
        tb.KeyDown -= this.OnKeyDown;
        tb.RemoveHandler(InputElement.KeyDownEvent, OnKeyDown_Tunnel);
        AttachedTextBoxBinding.SetIsValueDifferent(tb, false);
    }

    private void OnLostFocus(object? sender, RoutedEventArgs e) {
        if (this.CanApplyValueOnLostFocus) {
            if (!this.isHandlingChangeModel) {
                ApplicationPFX.Instance.Dispatcher.Post(this.HandleUpdateModelFromText, DispatchPriority.Input);
            }
        }
        else {
            this.UpdateControl();
        }
    }

    private void OnKeyDown(object? sender, KeyEventArgs e) {
        TextBox tb = (TextBox) sender!;
        if (e.Key == Key.Escape) {
            if (this.isHandlingChangeModel) {
                return;
            }

            // When the user clicks escape, we want to temporarily disable lost focus handling and move focus elsewhere.
            // This is to prevent infinite loops of dialogs being shown saying the value is incorrect format or whatever.
            // User inputs bad value, dialog shows, user closes dialog and the text box is re-focused,
            // user clicks away to do something else, lost focus is called and shows the dialog again, and it loops

            tb.LostFocus -= this.OnLostFocus;

            string oldText = tb.Text ?? "";
            
            bool canMoveFocus = this.CanMoveFocusUpwardsOnEscape && !AttachedTextBoxBinding.GetIsValueDifferent(tb);
            
            this.UpdateControl(); // sets IsValueDifferent to false

            // wasValueDifferent == true:  User just pressed escape to cancel their changes when
            // wasValueDifferent == false: User wants to un-focus the text box, probably
            if (canMoveFocus) {
                VisualTreeUtils.TryMoveFocusUpwards(tb);
            }

            ApplicationPFX.Instance.Dispatcher.Invoke(() => tb.LostFocus += this.OnLostFocus, DispatchPriority.Loaded);

            // invoke callback to allow user code to maybe reverse some changes
            this.EscapePressed?.Invoke(this, oldText);
            
            e.Handled = true;
        }
        else if (e.Key == Key.Enter) {
            if (!this.isHandlingChangeModel) {
                this.HandleUpdateModelFromText();
            }
            
            e.Handled = true;
        }
    }
    
    private static void OnKeyDown_Tunnel(object? sender, KeyEventArgs e) {
        if (e.Key != Key.Enter && e.Key != Key.Escape) {
            AttachedTextBoxBinding.SetIsValueDifferent((TextBox) sender!, true);
        }
    }

    /// <summary>
    /// Updates our model based on what's present in the text block
    /// </summary>
    private async void HandleUpdateModelFromText() {
        TextBox? tb = null;
        try {
            if (this.isHandlingChangeModel || !base.IsFullyAttached) {
                return;
            }

            tb = (TextBox) this.myControl!;
            this.isHandlingChangeModel = true;

            // Read text before setting IsEnabled to false, because LostFocus will reset text to underlying value
            string text = tb.Text ?? "";

            bool oldIsEnabled = tb.IsEnabled;
            tb.IsEnabled = false;
            AttachedTextBoxBinding.SetIsValueDifferent(tb, false);
            bool success = await this.parseAndUpdate(this, text);
            tb.IsEnabled = oldIsEnabled;
            this.UpdateControl();
            if (!success && this.FocusTextBoxOnError)
                await ApplicationPFX.Instance.Dispatcher.InvokeAsync(() => BugFix.TextBox_FocusSelectAll(tb));

            this.ValueConfirmed?.Invoke(this, text);
        }
        catch (Exception e) {
            ApplicationPFX.Instance.Dispatcher.Post(() => ExceptionDispatchInfo.Throw(e), DispatchPriority.Send);
        }
        finally {
            this.isHandlingChangeModel = false;
            if (tb != null)
                AttachedTextBoxBinding.SetIsValueDifferent(tb, false);
        }
    }

    // private sealed class TextBoxBrushFlipFlopTimer : FlipFlopTimer {
    //     private readonly IColourBrush? highBrush;
    //
    //     private AvaloniaObject? targetObject;
    //     private AvaloniaProperty? targetProperty;
    //
    //     private IDisposable? highBrushSubscription;
    //
    //     public TextBoxBrushFlipFlopTimer(TimeSpan interval, IColourBrush? highBrush) : base(interval) {
    //         this.highBrush = highBrush;
    //     }
    //
    //     /// <summary>
    //     /// Sets the control that we update the property of. This method will subscribe to changes
    //     /// of dynamic brushes if that is what the low and/or high brushes are
    //     /// </summary>
    //     /// <param name="target"></param>
    //     /// <param name="property"></param>
    //     /// <exception cref="InvalidOperationException"></exception>
    //     public void SetTarget(AvaloniaObject target, AvaloniaProperty property) {
    //         if (this.targetObject != null)
    //             throw new InvalidOperationException("Target already set. Use " + nameof(this.ClearTarget));
    //
    //         this.targetObject = target;
    //         this.targetProperty = property;
    //
    //         if (this.lowBrush is DynamicAvaloniaColourBrush dLowBrush)
    //             this.lowBrushSubscription = dLowBrush.Subscribe(this.OnLowBrushValueChanged);
    //
    //         if (this.highBrush is DynamicAvaloniaColourBrush dHighBrush)
    //             this.highBrushSubscription = dHighBrush.Subscribe(this.OnHighBrushValueChanged);
    //
    //         this.UpdateBrush(this.IsHigh);
    //     }
    //
    //     /// <summary>
    //     /// Clears the target object, if present. This will also unsubscribe from dynamic brush
    //     /// changes if previously subscribed in <see cref="SetTarget"/>
    //     /// </summary>
    //     public void ClearTarget() {
    //         DisposableUtils.Dispose(ref this.lowBrushSubscription);
    //         DisposableUtils.Dispose(ref this.highBrushSubscription);
    //         this.targetObject = null;
    //         this.targetProperty = null;
    //     }
    //
    //     private void OnLowBrushValueChanged(IBrush? obj) {
    //         if (this.targetObject != null && !this.IsHigh) {
    //             this.targetObject.SetValue(this.targetProperty!, obj);
    //         }
    //     }
    //
    //     private void OnHighBrushValueChanged(IBrush? obj) {
    //         if (this.targetObject != null && this.IsHigh) {
    //             this.targetObject.SetValue(this.targetProperty!, obj);
    //         }
    //     }
    //
    //     protected override void OnIsHighChanged(bool isHigh) {
    //         base.OnIsHighChanged(isHigh);
    //         this.UpdateBrush(isHigh);
    //     }
    //
    //     private void UpdateBrush(bool isHigh) {
    //         this.targetObject?.SetValue(this.targetProperty!, isHigh ? ((AvaloniaColourBrush?) this.highBrush)?.Brush : ((AvaloniaColourBrush?) this.lowBrush)?.Brush);
    //     }
    // }
}