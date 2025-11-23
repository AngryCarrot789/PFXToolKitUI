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

using Avalonia;
using PFXToolKitUI.DataTransfer;

namespace PFXToolKitUI.Avalonia.Bindings;

/// <summary>
/// The most basic fully automatic data parameter binder. This uses the object getter/setter of both
/// the control and the parameter's accessor. It also provides converter functions for easy conversion
/// between different types (e.g. string to double and vice versa)
/// </summary>
/// <typeparam name="TModel">The type of model which implements <see cref="ITransferableData"/> and can store property values</typeparam>
public class AvaloniaPropertyToDataParameterAutoBinder<TModel> : BaseAvaloniaPropertyBinder<TModel> where TModel : class, ITransferableData {
    private readonly Func<object?, object?>? ToProperty, ToParameter;
    
    public DataParameter? Parameter { get; }

    public bool CanUpdateModel { get; init; } = true;

    /// <summary>
    /// Creates a new data parameter property binder
    /// </summary>
    /// <param name="property">The avalonia property, which is used to listen to property changes</param>
    /// <param name="parameter">The data parameter, used to listen to model value changes</param>
    /// <param name="toProperty">Converts the parameter value to an appropriate property value (e.g. double to string)</param>
    /// <param name="toParameter">Converts the property value back to the parameter value (e.g. string to double, or returns validation error)</param>
    public AvaloniaPropertyToDataParameterAutoBinder(AvaloniaProperty? property, DataParameter? parameter, Func<object?, object?>? toProperty = null, Func<object?, object?>? toParameter = null) : base(property) {
        this.Parameter = parameter;
        this.ToProperty = toProperty;
        this.ToParameter = toParameter;
    }

    protected override void UpdateModelOverride() {
        if (this.CanUpdateModel && this.IsFullyAttached && this.Property != null && this.Parameter != null) {
            object? newValue = this.Control.GetValue(this.Property);
            this.Parameter.SetObjectValue(this.Model, this.ToParameter != null ? this.ToParameter(newValue) : newValue);
        }
    }

    protected override void UpdateControlOverride(bool hasJustAttached) {
        if (this.IsFullyAttached && this.Property != null && this.Parameter != null) {
            object? newValue = this.Parameter.GetObjectValue(this.Model);
            this.Control.SetValue(this.Property, this.ToProperty != null ? this.ToProperty(newValue) : newValue);
        }
    }

    private void OnDataParameterValueChanged(object? sender, DataParameterValueChangedEventArgs e) => this.UpdateControl();

    protected override void OnAttached() {
        base.OnAttached();
        this.Parameter?.AddValueChangedHandler(this.Model, this.OnDataParameterValueChanged);
    }

    protected override void OnDetached() {
        base.OnDetached();
        this.Parameter?.RemoveValueChangedHandler(this.Model, this.OnDataParameterValueChanged);
    }
}