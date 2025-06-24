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

namespace PFXToolKitUI.Avalonia.Bindings;

/// <summary>
/// A binder that inherits <see cref="BaseAvaloniaPropertyToEventPropertyBinder{TModel}"/> that
/// uses two action events for updating the control and the model
/// </summary>
/// <typeparam name="TModel">The type of model</typeparam>
public class AvaloniaPropertyToEventPropertyBinder<TModel> : BaseAvaloniaPropertyToEventPropertyBinder<TModel> where TModel : class {
    private readonly Action<IBinder<TModel>>? updateControl, updateModel;

    public AvaloniaPropertyToEventPropertyBinder(string eventName, Action<IBinder<TModel>>? updateControl, Action<IBinder<TModel>>? updateModel) : this(null, eventName, updateControl, updateModel) {
    }

    public AvaloniaPropertyToEventPropertyBinder(AvaloniaProperty? property, string eventName, Action<IBinder<TModel>>? updateControl, Action<IBinder<TModel>>? updateModel) : base(property, eventName) {
        this.updateControl = updateControl;
        this.updateModel = updateModel;
    }

    protected override void UpdateModelOverride() => this.updateModel?.Invoke(this);

    protected override void UpdateControlOverride() => this.updateControl?.Invoke(this);
}