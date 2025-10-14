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
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
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
    private bool isChangingModel, isResettingTextToModel;

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
    /// Fired when the user confirms their value, either by pressing ENTER or when the text box loses focus when <see cref="CanApplyValueOnLostFocus"/>
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

    /// <summary>
    /// Creates a (or gets a cached) string from the underlying model value, not the text box control itself
    /// </summary>
    /// <returns></returns>
    protected abstract string GetTextCore();

    protected override void UpdateModelOverride() {
    }

    protected override void UpdateControlOverride(bool hasJustAttached) {
        TextBox tb = (TextBox) this.myControl!;
        if (this.IsFullyAttached) {
            string newValue = this.GetTextCore();
            
            this.isResettingTextToModel = true;
            tb.Text = newValue;
            BugFix.TextBox_UpdateSelection(tb);
            if (hasJustAttached && tb.IsUndoEnabled) {
                // goofy way to clear the undo stack, so that the user cannot undo what
                // this class applied for the first time since attaching the control
                tb.IsUndoEnabled = false;
                tb.IsUndoEnabled = true;
            }
            
            this.isResettingTextToModel = false;
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
        tb.TextChanged += this.OnTextChanged;
        AttachedTextBoxBinding.SetIsValueDifferent(tb, false);
    }

    protected override void OnDetached() {
        TextBox tb = (TextBox) this.Control;
        tb.LostFocus -= this.OnLostFocus;
        tb.KeyDown -= this.OnKeyDown;
        tb.TextChanged -= this.OnTextChanged;
        AttachedTextBoxBinding.SetIsValueDifferent(tb, false);
    }

    private void OnLostFocus(object? sender, RoutedEventArgs e) {
        if (this.CanApplyValueOnLostFocus) {
            if (!this.isChangingModel) {
                ApplicationPFX.Instance.Dispatcher.Post(this.HandleUpdateModelFromText, DispatchPriority.Input);
            }
        }
        else {
            this.UpdateControl();
        }
    }

    private void OnKeyDown(object? sender, KeyEventArgs e) {
        TextBox textBox = (TextBox) sender!;
        switch (e.Key) {
            case Key.Escape: {
                if (this.isChangingModel || this.isResettingTextToModel) {
                    return;
                }

                // When the user clicks escape, we want to temporarily disable lost focus handling and move focus elsewhere.
                // This is to prevent infinite loops of dialogs being shown saying the value is incorrect format or whatever.
                // User inputs bad value, dialog shows, user closes dialog and the text box is re-focused,
                // user clicks away to do something else, lost focus is called and shows the dialog again, and it loops

                textBox.LostFocus -= this.OnLostFocus;
                string oldText = textBox.Text ?? "";
                bool canMoveFocus = this.CanMoveFocusUpwardsOnEscape && !AttachedTextBoxBinding.GetIsValueDifferent(textBox);
            
                this.UpdateControl(); // sets IsValueDifferent to false
                
                Debug.Assert(!this.isResettingTextToModel);
                this.isResettingTextToModel = true;
                
                if (canMoveFocus) {
                    VisualTreeUtils.TryMoveFocusUpwards(textBox);
                }
                
                // using Invoke here can potentially cause TextChanged event to be invoked since it
                // has a higher priority so we wrap it with isResettingTextToModel to be sure,
                // since we're still technically in the process of resetting text
                Dispatcher.UIThread.Invoke(() => textBox.LostFocus += this.OnLostFocus, DispatcherPriority.Loaded);
                
                this.isResettingTextToModel = false;

                // invoke callback to allow user code to maybe reverse some changes
                this.EscapePressed?.Invoke(this, oldText);
            
                e.Handled = true;
                break;
            }
            case Key.Enter: {
                if (!this.isChangingModel) {
                    this.HandleUpdateModelFromText();
                }
            
                e.Handled = true;
                break;
            }
        }
    }
    
    private void OnTextChanged(object? sender, TextChangedEventArgs e) {
        TextBox textBox = (TextBox) sender!;
        if (!this.isChangingModel && !this.isResettingTextToModel && textBox.IsKeyboardFocusWithin) {
            AttachedTextBoxBinding.SetIsValueDifferent(textBox, true);
        }
    }
    
    /// <summary>
    /// Updates our model based on what's present in the text block
    /// </summary>
    private async void HandleUpdateModelFromText() {
        try {
            if (this.isChangingModel || !base.IsFullyAttached) {
                return;
            }

            TextBox textBox = (TextBox) this.myControl!;
            this.isChangingModel = true;

            // Read text before setting IsEnabled to false, because LostFocus will reset text to underlying value
            string text = textBox.Text ?? "";
            
            bool oldIsEnabled = textBox.IsEnabled;
            textBox.IsEnabled = false;
            
            AttachedTextBoxBinding.SetIsValueDifferent(textBox, false);
            bool success = await this.parseAndUpdate(this, text);
            
            textBox.IsEnabled = oldIsEnabled;
            this.UpdateControl();
            if (!success && this.FocusTextBoxOnError)
                await ApplicationPFX.Instance.Dispatcher.InvokeAsync(() => BugFix.TextBox_FocusSelectAll(textBox));

            this.ValueConfirmed?.Invoke(this, text);
        }
        finally {
            this.isChangingModel = false;
        }
    }
}