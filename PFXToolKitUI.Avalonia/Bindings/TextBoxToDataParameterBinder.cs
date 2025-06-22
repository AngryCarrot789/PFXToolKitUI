// 
// Copyright (c) 2024-2025 REghZy
// 
// This file is part of FramePFX.
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
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
// 

using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using PFXToolKitUI.Avalonia.Utils;
using PFXToolKitUI.DataTransfer;

namespace PFXToolKitUI.Avalonia.Bindings;

/// <summary>
/// Same as <see cref="AvaloniaPropertyToDataParameterAutoBinder{TModel}"/> but does not map the control property
/// back to the model value directly, but instead tries to parse the control value to a model compatible value
/// </summary>
/// <typeparam name="TModel">Type of model</typeparam>
/// <typeparam name="T">The type of value</typeparam>
public class TextBoxToDataParameterBinder<TModel, T> : BaseAvaloniaPropertyBinder<TModel> where TModel : class, ITransferableData {
    public DataParameter? Parameter { get; }

    private readonly Func<T, string?>? ParamToProp;
    private readonly Func<TextBoxToDataParameterBinder<TModel, T>, string, Task<Optional<T>>> Convert;
    private bool isHandlingChangeModel;

    /// <summary>
    /// Fired when <see cref="BaseBinder{TModel}.UpdateControl"/> is invoked (which is called
    /// when we become fully attached, or when it is invoked manually or the model value changes)
    /// </summary>
    public event Action<TextBoxToDataParameterBinder<TModel, T>>? PostUpdateControl;

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

    /// <summary>
    /// Creates a new data parameter property binder
    /// </summary>
    /// <param name="parameter">The data parameter, used to listen to model value changes</param>
    /// <param name="parameterToProperty">Converts the parameter value to an appropriate property value (e.g. double to string)</param>
    /// <param name="convert">
    /// Converts the text box string value back to the parameter value. This function
    /// can show dialogs and then return default on failure to convert back, and also
    /// re-selects the text box when <see cref="FocusTextBoxOnError"/> is true
    /// </param>
    public TextBoxToDataParameterBinder(DataParameter<T> parameter, Func<T, string?>? parameterToProperty, Func<TextBoxToDataParameterBinder<TModel, T>, string, Task<Optional<T>>> convert, Action<TextBoxToDataParameterBinder<TModel, T>>? postUpdateControl = null) : base(null) {
        this.Parameter = parameter;
        this.ParamToProp = parameterToProperty;
        this.Convert = convert;
        this.PostUpdateControl = postUpdateControl;
    }

    protected override void UpdateModelOverride() {
    }

    protected override void UpdateControlOverride() {
        if (this.IsFullyAttached) {
            T newValue = ((DataParameter<T>) this.Parameter!).GetValue(this.Model);
            ((TextBox) this.myControl!).Text = this.ParamToProp != null ? this.ParamToProp(newValue) : newValue?.ToString();
            BugFix.TextBox_UpdateSelection((TextBox) this.myControl!);
            this.PostUpdateControl?.Invoke(this);
        }
    }

    private void OnDataParameterValueChanged(DataParameter parameter, ITransferableData owner) => this.UpdateControl();

    protected override void CheckAttachControl(Control control) {
        base.CheckAttachControl(control);
        if (!(control is TextBox))
            throw new InvalidOperationException("Attempt to attach non-textbox");
    }

    protected override void OnAttached() {
        base.OnAttached();
        this.Parameter?.AddValueChangedHandler(this.Model, this.OnDataParameterValueChanged);

        TextBox tb = (TextBox) this.Control;
        tb.LostFocus += this.OnLostFocus;
        tb.KeyDown += this.OnKeyDown;
    }

    protected override void OnDetached() {
        base.OnDetached();
        this.Parameter?.RemoveValueChangedHandler(this.Model, this.OnDataParameterValueChanged);

        TextBox tb = (TextBox) this.Control;
        tb.LostFocus -= this.OnLostFocus;
        tb.KeyDown -= this.OnKeyDown;
    }

    private void OnLostFocus(object? sender, RoutedEventArgs e) {
        if (this.CanChangeOnLostFocus) {
            if (!this.isHandlingChangeModel) {
                ApplicationPFX.Instance.Dispatcher.Post(this.HandleChangeModel, DispatchPriority.Input);
            }
        }
        else {
            this.UpdateControl();
        }
    }

    private void OnKeyDown(object? sender, KeyEventArgs e) {
        if (e.Key == Key.Escape) {
            TextBox tb = (TextBox) sender!;

            tb.LostFocus -= this.OnLostFocus;
            this.UpdateControl();

            VisualTreeUtils.TryMoveFocusUpwards(tb);

            ApplicationPFX.Instance.Dispatcher.Invoke(() => tb.LostFocus += this.OnLostFocus, DispatchPriority.Loaded);

            // invoke callback to allow user code to maybe reverse some changes
            // this.OnEscapePressed();
        }
        else if (e.Key == Key.Enter) {
            if (!this.isHandlingChangeModel) {
                this.HandleChangeModel();
            }
        }
    }

    private async void HandleChangeModel() {
        Optional<T> value;
        try {
            if (!base.IsFullyAttached) {
                return;
            }

            TextBox control = (TextBox) this.myControl!;
            this.isHandlingChangeModel = true;
            bool oldIsEnabled = control.IsEnabled;
            control.IsEnabled = false;
            value = await this.Convert(this, control.Text ?? "");
            control.IsEnabled = oldIsEnabled;
            if (!value.HasValue) {
                if (this.FocusTextBoxOnError)
                    await ApplicationPFX.Instance.Dispatcher.InvokeAsync(() => BugFix.TextBox_FocusSelectAll(control));
                return;
            }
        }
        catch (Exception e) {
            ApplicationPFX.Instance.Dispatcher.Post(() => throw e, DispatchPriority.Send);
            return;
        }
        finally {
            this.isHandlingChangeModel = false;
        }

        try {
            if (this.IsFullyAttached) {
                ((DataParameter<T>) this.Parameter!).SetValue(this.Model, value.Value);
            }

            this.UpdateControl();
        }
        catch (Exception e) {
            ApplicationPFX.Instance.Dispatcher.Post(() => throw e, DispatchPriority.Send);
        }
    }
}