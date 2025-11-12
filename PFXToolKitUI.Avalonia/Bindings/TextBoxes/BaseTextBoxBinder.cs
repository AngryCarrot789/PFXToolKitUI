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
    public delegate void ValueCancelledEventHandler(BaseTextBoxBinder<TModel> sender, ValueCancelledEventArgs e);

    public delegate void ValueConfirmedEventHandler(BaseTextBoxBinder<TModel> sender, ValueConfirmedEventArgs e);

    private readonly Func<IBinder<TModel>, string, Task<bool>> parseAndUpdate;
    private bool isChangingModel, isResettingTextToModel;
    private bool hasUserModifiedValueSinceUpdate;
    private EventHandler<RoutedEventArgs>? onLostFocus;
    private EventHandler<KeyEventArgs>? onKeyDown;
    private EventHandler<TextChangedEventArgs>? onTextChanged;

    /// <summary>
    /// Gets or sets if the model value can be updated when the text box loses focus.
    /// </summary>
    public bool CanApplyValueOnLostFocus { get; set; } = true;

    /// <summary>
    /// Gets or sets if the text box should be re-focused when the update callback returns false.
    /// Default is true, because it's annoying to the user to have to re-click the text box again.
    /// </summary>
    public bool FocusTextBoxOnError { get; set; } = true;

    /// <summary>
    /// Gets or sets if the text box is a multi-line text box.
    /// Setting this as true will prevent the model from being updated when the user clicks the enter key
    /// </summary>
    public bool IsMultiLineTextBox { get; set; }

    public bool CanMoveFocusUpwardsOnEscape { get; set; } = true;

    protected bool HasUserModifiedValueSinceUpdate {
        get => this.hasUserModifiedValueSinceUpdate;
        set {
            this.hasUserModifiedValueSinceUpdate = value;
            if (this.myControl != null)
                AttachedTextBoxBinding.SetIsValueDifferent((TextBox) this.myControl, value);
        }
    }

    /// <summary>
    /// Fired when the user presses the escape button or the text box loses focus when <see cref="CanApplyValueOnLostFocus"/> is false.
    /// </summary>
    public event ValueCancelledEventHandler? ValueCancelled;

    /// <summary>
    /// Fired when the user confirms their value, either by pressing ENTER or when the text box loses focus when <see cref="CanApplyValueOnLostFocus"/> is true.
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
        TextBox tb = (TextBox) this.Control;
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

        this.HasUserModifiedValueSinceUpdate = false;
    }

    protected override void CheckAttachControl(Control control) {
        base.CheckAttachControl(control);
        if (!(control is TextBox tb))
            throw new InvalidOperationException("Attempt to attach non-textbox");
        if (TextBoxBinderUtils.GetIsAttachedToBTTB(tb))
            throw new InvalidOperationException("Text box is already attached via another text box binder");
    }

    protected override void OnAttached() {
        TextBox tb = (TextBox) this.Control;
        tb.LostFocus += this.onLostFocus ??= this.OnLostFocus;
        tb.KeyDown += this.onKeyDown ??= this.OnKeyDown;
        tb.TextChanged += this.onTextChanged ??= this.OnTextChanged;
        this.HasUserModifiedValueSinceUpdate = false;
        TextBoxBinderUtils.SetIsAttachedToBTTB(tb, true);
    }

    protected override void OnDetached() {
        Debug.Assert(this.onLostFocus != null && this.onKeyDown != null && this.onTextChanged != null);
        TextBox tb = (TextBox) this.Control;
        tb.LostFocus -= this.onLostFocus;
        tb.KeyDown -= this.onKeyDown;
        tb.TextChanged -= this.onTextChanged;
        this.HasUserModifiedValueSinceUpdate = false;
        TextBoxBinderUtils.SetIsAttachedToBTTB(tb, false);
    }

    private void OnLostFocus(object? sender, RoutedEventArgs e) {
        Debug.Assert(this.myControl == sender);

        // Only apply value when the user has typed something, otherwise we risk issues with message box loops.
        // For example, two TBs with binders that are mis-configured such that GetTextCore() returns
        // a value that cannot be converted back (which is a bug anyway)
        // - The user clicks one TB, then the other.
        // - Loss of focus on TB 1 shows a message box, and if that MSG box has a default button,
        //   will focus that and cause the other TB to lose focus, and now there's two message boxes
        // - Then, closing one dialog re-focuses a TB but user has to click the other
        //   message box to close it, causing loss of focus and showing another message box.
        bool hasValueChangedSinceUpdate = this.HasUserModifiedValueSinceUpdate;
        if (this.CanApplyValueOnLostFocus && hasValueChangedSinceUpdate) {
            if (!this.isChangingModel) {
                ApplicationPFX.Instance.Dispatcher.Post(this.HandleUpdateModelFromText, DispatchPriority.Input);
            }
        }
        else {
            string? text = hasValueChangedSinceUpdate ? ((TextBox) sender!).Text : null;
            this.UpdateControl();

            // Only fire when the user has actually changed the value
            if (hasValueChangedSinceUpdate) {
                this.ValueCancelled?.Invoke(this, new ValueCancelledEventArgs(text ?? ""));
            }
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
                // - User inputs bad value, dialog shows, user closes dialog and the text box is re-focused.
                // - Then, user clicks away to do something else, lost focus is called and shows the dialog again, and it loops

                textBox.LostFocus -= this.OnLostFocus;
                string text = textBox.Text ?? "";
                bool canMoveFocus = this.CanMoveFocusUpwardsOnEscape && !this.HasUserModifiedValueSinceUpdate;

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
                this.ValueCancelled?.Invoke(this, new ValueCancelledEventArgs(text));

                e.Handled = true;
                break;
            }
            case Key.Enter: {
                if (!this.IsMultiLineTextBox) {
                    if (!this.isChangingModel) {
                        this.HandleUpdateModelFromText();
                    }

                    e.Handled = true;
                }

                break;
            }
        }
    }

    private void OnTextChanged(object? sender, TextChangedEventArgs e) {
        TextBox textBox = (TextBox) sender!;
        if (!this.isChangingModel && !this.isResettingTextToModel && textBox.IsKeyboardFocusWithin) {
            // Only mark as user-modified when there's focus within. It's very unlikely that
            // the user can make the text change without keyboard focus; undo/redo requires focus,
            // which will therefore count as a user modification. External changes are not supported.
            this.HasUserModifiedValueSinceUpdate = true;
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

            TextBox textBox = (TextBox) this.Control;
            this.isChangingModel = true;

            // Read text before setting IsEnabled to false, because LostFocus will reset text to underlying value
            string text = textBox.Text ?? "";

            bool oldIsEnabled = textBox.IsEnabled;
            textBox.IsEnabled = false;

            this.HasUserModifiedValueSinceUpdate = false;
            bool success = await this.parseAndUpdate(this, text);

            textBox.IsEnabled = oldIsEnabled;
            this.UpdateControl();
            if (!success && this.FocusTextBoxOnError)
                await ApplicationPFX.Instance.Dispatcher.InvokeAsync(() => BugFix.TextBox_FocusSelectAll(textBox));

            this.ValueConfirmed?.Invoke(this, new ValueConfirmedEventArgs(text, success));
        }
        finally {
            this.isChangingModel = false;
        }
    }
}

internal static class TextBoxBinderUtils {
    public static readonly AttachedProperty<bool> IsAttachedToBTTBProperty = AvaloniaProperty.RegisterAttached<TextBox, bool>("IsAttachedToBTTB", typeof(TextBoxBinderUtils));
    public static void SetIsAttachedToBTTB(TextBox obj, bool value) => obj.SetValue(IsAttachedToBTTBProperty, value);
    public static bool GetIsAttachedToBTTB(TextBox obj) => obj.GetValue(IsAttachedToBTTBProperty);
}

public readonly struct ValueCancelledEventArgs(string text) {
    /// <summary>
    /// Gets the text that was in the text box when the user pressed escape
    /// </summary>
    public string Text { get; } = text;
}

public readonly struct ValueConfirmedEventArgs(string text, bool isSuccess) {
    /// <summary>
    /// Gets the text that was in the text box when the user confirmed the value (the same value passed to the parse and update callback)
    /// </summary>
    public string Text { get; } = text;

    /// <summary>
    /// Gets whether the value confirmation was successful and resulted in the model being updated (the parse and update callback for the binder returned true)
    /// </summary>
    public bool IsSuccess { get; } = isSuccess;
}