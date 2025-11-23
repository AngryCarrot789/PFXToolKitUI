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

public class AvaloniaPropertyToDataParameterBinder<TModel> : AvaloniaPropertyBinder<TModel> where TModel : class, ITransferableData {
    public DataParameter Parameter { get; }

    public AvaloniaPropertyToDataParameterBinder(DataParameter parameter, Action<IBinder<TModel>>? updateControl, Action<IBinder<TModel>>? updateModel = null) : base(updateControl, updateModel) {
        this.Parameter = parameter ?? throw new ArgumentNullException(nameof(parameter));
    }

    public AvaloniaPropertyToDataParameterBinder(DataParameter parameter, AvaloniaProperty? property, Action<IBinder<TModel>>? updateControl, Action<IBinder<TModel>>? updateModel = null) : base(property, updateControl, updateModel) {
        this.Parameter = parameter ?? throw new ArgumentNullException(nameof(parameter));
    }

    protected override void OnAttached() {
        base.OnAttached();
        this.Parameter.AddValueChangedHandler(this.Model, this.OnParameterChanged);
    }

    protected override void OnDetached() {
        base.OnDetached();
        this.Parameter.RemoveValueChangedHandler(this.Model, this.OnParameterChanged);
    }

    private void OnParameterChanged(object? sender, DataParameterValueChangedEventArgs e) {
        this.UpdateControl();
    }
}