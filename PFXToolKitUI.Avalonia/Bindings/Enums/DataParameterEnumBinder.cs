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

using PFXToolKitUI.DataTransfer;

namespace PFXToolKitUI.Avalonia.Bindings.Enums;

/// <summary>
/// A class which helps bind radio buttons to an enum data parameter
/// </summary>
public class DataParameterEnumBinder<TEnum> : BaseEnumBinder<TEnum> where TEnum : struct, Enum {
    /// <summary>
    /// Gets or sets the active transferable data owner
    /// </summary>
    public ITransferableData? Model { get; private set; }

    /// <summary>
    /// Gets the parameter used to get/set the enum value in the <see cref="Model"/>
    /// </summary>
    public DataParameter<TEnum> Parameter { get; }

    public override bool IsAttached => this.Model != null;
    
    public DataParameterEnumBinder(DataParameter<TEnum> parameter) {
        this.Parameter = parameter ?? throw new ArgumentNullException(nameof(parameter));
    }

    private void OnDataParameterChanged(DataParameter parameter, ITransferableData owner) {
        if (this.Model == null)
            throw new Exception("Fatal application bug");
        this.UpdateControls(this.GetValue());
    }
    
    public void Attach(ITransferableData model) {
        ArgumentNullException.ThrowIfNull(model);
        if (this.Model != null)
            throw new InvalidOperationException("Already attached");

        this.Model = model;
        this.Parameter.AddValueChangedHandler(model, this.OnDataParameterChanged);
        this.UpdateControls(this.GetValue());
    }

    public void Detach() {
        if (this.Model == null)
            throw new InvalidOperationException("Not attached");

        this.Parameter.RemoveValueChangedHandler(this.Model, this.OnDataParameterChanged);
        this.Model = null;
    }

    protected override void SetValue(TEnum value) {
        this.Parameter.SetValue(this.Model!, value);
    }

    protected override TEnum GetValue() {
        return this.Parameter.GetValue(this.Model!);
    }
}