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

using Avalonia.Data;
using PFXToolKitUI.DataTransfer;

namespace PFXToolKitUI.Avalonia.Bindings.TextBoxes;

/// <summary>
/// Same as <see cref="AvaloniaPropertyToDataParameterAutoBinder{TModel}"/> but does not map the control property
/// back to the model value directly, but instead tries to parse the control value to a model compatible value
/// </summary>
/// <typeparam name="TModel">Type of model</typeparam>
/// <typeparam name="T">The type of value</typeparam>
public class TextBoxToDataParameterBinder<TModel, T> : BaseTextBoxBinder<TModel> where TModel : class, ITransferableData {
    private static readonly Func<IBinder<TModel>, string, Task<bool>> ParseAndUpdateValue = async (binder, text) => {
        TextBoxToDataParameterBinder<TModel, T> instance = (TextBoxToDataParameterBinder<TModel, T>) binder;
        Optional<T> result = await instance.parseValue(instance, text);
        if (result.HasValue) {
            instance.Parameter.SetValue(instance.Model, result.Value);
            return true;
        }

        return false;
    };
    
    public DataParameter<T> Parameter { get; }

    private readonly Func<T, string?>? convertToString;
    private readonly Func<TextBoxToDataParameterBinder<TModel, T>, string, Task<Optional<T>>> parseValue;

    /// <summary>
    /// Fired when <see cref="BaseBinder{TModel}.UpdateControl"/> is invoked (which is called
    /// when we become fully attached, or when it is invoked manually or the model value changes)
    /// </summary>
    public event Action<TextBoxToDataParameterBinder<TModel, T>>? ControlUpdated;

    /// <summary>
    /// Creates a new data parameter property binder
    /// </summary>
    /// <param name="parameter">The data parameter, used to listen to model value changes</param>
    /// <param name="convertToString">Converts the parameter value to an appropriate property value (e.g. double to string)</param>
    /// <param name="parseValue">
    /// Converts the text box string value back to the parameter value. This function
    /// can show dialogs and then return default on failure to convert back, and also
    /// re-selects the text box when <see cref="FocusTextBoxOnError"/> is true
    /// </param>
    public TextBoxToDataParameterBinder(DataParameter<T> parameter, Func<T, string?>? convertToString, Func<TextBoxToDataParameterBinder<TModel, T>, string, Task<Optional<T>>> parseValue, Action<TextBoxToDataParameterBinder<TModel, T>>? postUpdateControl = null)
        : base(ParseAndUpdateValue) {
        this.Parameter = parameter;
        this.convertToString = convertToString;
        this.parseValue = parseValue;
        this.ControlUpdated = postUpdateControl;
    }

    protected override string GetTextCore() {
        T newValue = this.Parameter.GetValue(this.Model);
        return (this.convertToString != null ? this.convertToString(newValue) : newValue?.ToString()) ?? "";
    }

    protected override void UpdateControlOverride() {
        base.UpdateControlOverride();
        if (this.IsFullyAttached) {
            this.ControlUpdated?.Invoke(this);
        }
    }

    private void OnDataParameterValueChanged(DataParameter parameter, ITransferableData owner) => this.UpdateControl();

    protected override void OnAttached() {
        base.OnAttached();
        this.Parameter.AddValueChangedHandler(this.Model, this.OnDataParameterValueChanged);
    }

    protected override void OnDetached() {
        base.OnDetached();
        this.Parameter.RemoveValueChangedHandler(this.Model, this.OnDataParameterValueChanged);
    }
}