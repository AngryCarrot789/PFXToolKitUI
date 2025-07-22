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

public abstract class BaseTextBoxBinder<TModel> : BaseBinder<TModel> where TModel : class {
    public delegate void EscapePressedEventHandler(BaseTextBoxBinder<TModel> sender, string oldText);

    public delegate void ValueConfirmedEventHandler(BaseTextBoxBinder<TModel> sender, string oldText);

    private readonly Func<IBinder<TModel>, string, Task<bool>> updateModel;
    private bool isHandlingChangeModel;

    /// <summary>
    /// Gets or sets if the model value can be set when the text box loses focus.
    /// Default is true, which is the recommended value because the user may not
    /// realise their change was undone since they had to click the Enter key to confirm changes.
    /// </summary>
    public bool CanChangeOnLostFocus { get; set; } = true;

    /// <summary>
    /// Gets or sets if the text box should be re-focused when the update callback returns false.
    /// Default is true, because it's annoying to the user to have to re-click the text box again
    /// </summary>
    public bool FocusTextBoxOnError { get; set; } = true;

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
    /// <param name="updateModel">
    /// A function which tries to update the model's value from the text box's text. Returns true on success,
    /// returns false when the text box contains invalid data (and it's assumed it shows a dialog too).
    /// When this returns false, the text box's text is re-selected (if <see cref="FocusTextBoxOnError"/> is true)
    /// </param>
    public BaseTextBoxBinder(Func<IBinder<TModel>, string, Task<bool>> updateModel) {
        this.updateModel = updateModel;
    }

    protected abstract string GetTextCore();

    protected override void UpdateModelOverride() {
    }

    protected override void UpdateControlOverride() {
        if (this.IsFullyAttached) {
            string newValue = this.GetTextCore();
            ((TextBox) this.myControl!).Text = newValue;
            BugFix.TextBox_UpdateSelection((TextBox) this.myControl!);
        }
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
    }

    protected override void OnDetached() {
        TextBox tb = (TextBox) this.Control;
        tb.LostFocus -= this.OnLostFocus;
        tb.KeyDown -= this.OnKeyDown;
    }

    private void OnLostFocus(object? sender, RoutedEventArgs e) {
        if (this.CanChangeOnLostFocus) {
            if (!this.isHandlingChangeModel) {
                ApplicationPFX.Instance.Dispatcher.Post(this.OnHandleUpdateModel, DispatchPriority.Input);
            }
        }
        else {
            this.UpdateControl();
        }
    }

    private void OnKeyDown(object? sender, KeyEventArgs e) {
        if (e.Key == Key.Escape) {
            if (this.isHandlingChangeModel) {
                return;
            }

            TextBox tb = (TextBox) sender!;

            // When the user clicks escape, we want to temporarily disable lost focus handling and move focus elsewhere.
            // This is to prevent infinite loops of dialogs being shown saying the value is incorrect format or whatever.
            // User inputs bad value, dialog shows, user closes dialog and the text box is re-focused,
            // user clicks away to do something else, lost focus is called and shows the dialog again, and it loops

            tb.LostFocus -= this.OnLostFocus;

            string oldText = tb.Text ?? "";
            this.UpdateControl();

            VisualTreeUtils.TryMoveFocusUpwards(tb);

            ApplicationPFX.Instance.Dispatcher.Invoke(() => tb.LostFocus += this.OnLostFocus, DispatchPriority.Loaded);

            // invoke callback to allow user code to maybe reverse some changes
            this.EscapePressed?.Invoke(this, oldText);
        }
        else if (e.Key == Key.Enter) {
            if (!this.isHandlingChangeModel) {
                this.OnHandleUpdateModel();
            }
        }
    }

    /// <summary>
    /// Updates our model based on what's present in the text block
    /// </summary>
    public async void OnHandleUpdateModel() {
        try {
            if (this.isHandlingChangeModel || !base.IsFullyAttached) {
                return;
            }

            TextBox control = (TextBox) this.myControl!;
            this.isHandlingChangeModel = true;

            // Read text before setting IsEnabled to false, because LostFocus will reset text to underlying value
            string text = control.Text ?? "";

            bool oldIsEnabled = control.IsEnabled;
            control.IsEnabled = false;
            bool success = await this.updateModel(this, text);
            control.IsEnabled = oldIsEnabled;
            this.UpdateControl();
            if (!success && this.FocusTextBoxOnError)
                await ApplicationPFX.Instance.Dispatcher.InvokeAsync(() => BugFix.TextBox_FocusSelectAll(control));

            this.ValueConfirmed?.Invoke(this, text);
        }
        catch (Exception e) {
            ApplicationPFX.Instance.Dispatcher.Post(() => ExceptionDispatchInfo.Throw(e), DispatchPriority.Send);
        }
        finally {
            this.isHandlingChangeModel = false;
        }
    }
}