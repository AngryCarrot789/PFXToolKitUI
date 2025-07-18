﻿// 
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

namespace PFXToolKitUI.Avalonia.Bindings;

/// <summary>
/// A binder which inherits <see cref="BaseEventBinder{TModel}"/> that
/// uses two action events for updating the control and the model.
/// <para>
/// This is basically <see cref="AvaloniaPropertyToEventPropertyBinder{TModel}"/> but without the avalonia property change handling to call <see cref="BaseBinder{TModel}.UpdateModel"/>
/// </para>
/// </summary>
/// <typeparam name="TModel">Model type</typeparam>
public class MultiEventUpdateBinder<TModel> : BaseMultiEventPropertyBinder<TModel> where TModel : class {
    public event Action<IBinder<TModel>>? DoUpdateControl;
    public event Action<IBinder<TModel>>? DoUpdateModel;

    public MultiEventUpdateBinder(string[] eventNames, Action<IBinder<TModel>>? updateControl, Action<IBinder<TModel>>? updateModel = null) : base(eventNames) {
        this.DoUpdateControl = updateControl;
        this.DoUpdateModel = updateModel;
    }

    protected override void UpdateModelOverride() => this.DoUpdateModel?.Invoke(this);

    protected override void UpdateControlOverride() => this.DoUpdateControl?.Invoke(this);
}