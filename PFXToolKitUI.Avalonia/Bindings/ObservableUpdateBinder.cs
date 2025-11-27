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

using PFXToolKitUI.Utils.Reactive;

namespace PFXToolKitUI.Avalonia.Bindings;

/// <summary>
/// A binder which inherits <see cref="BaseEventBinder{TModel}"/> that
/// uses two action events for updating the control and the model.
/// <para>
/// This is basically <see cref="AvaloniaPropertyToEventPropertyBinder{TModel}"/> but without the avalonia property change handling to call <see cref="BaseBinder{TModel}.UpdateModel"/>
/// </para>
/// </summary>
/// <typeparam name="TModel">Model type</typeparam>
public sealed class ObservableUpdateBinder<TModel> : BaseObservableBinder<TModel> where TModel : class {
    public event Action<IBinder<TModel>>? DoUpdateControl;
    public event Action<IBinder<TModel>>? DoUpdateModel;

    public ObservableUpdateBinder(IEventObservable<TModel> observable, Action<IBinder<TModel>>? updateControl, Action<IBinder<TModel>>? updateModel = null) : base(observable) {
        this.DoUpdateControl = updateControl;
        this.DoUpdateModel = updateModel;
    }
    
    public ObservableUpdateBinder(IEventObservable<TModel>[] observables, Action<IBinder<TModel>>? updateControl, Action<IBinder<TModel>>? updateModel = null) : base(observables) {
        this.DoUpdateControl = updateControl;
        this.DoUpdateModel = updateModel;
    }

    protected override void UpdateModelOverride() => this.DoUpdateModel?.Invoke(this);

    protected override void UpdateControlOverride(bool hasJustAttached) => this.DoUpdateControl?.Invoke(this);
}