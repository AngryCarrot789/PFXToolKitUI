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

using Avalonia.Controls;

namespace PFXToolKitUI.Avalonia.Bindings;

/// <summary>
/// A binder that does not listen to property changes or listen to any kind of event.
/// Useful for binding immutable models to a UI.
/// </summary>
/// <typeparam name="TModel"></typeparam>
public class ManualBinderEx<TControl, TModel> : BaseBinder<TModel> where TControl : Control where TModel : class {
    private readonly Action<TControl, TModel> onAttached;
    private readonly Action<TControl, TModel>? onDetached;

    public ManualBinderEx(Action<TControl, TModel> onAttached, Action<TControl, TModel>? onDetached = null) {
        this.onAttached = onAttached;
        this.onDetached = onDetached;
    }

    protected override void UpdateModelOverride() {
    }

    protected override void UpdateControlOverride() {
    }

    protected override void OnAttached() => this.onAttached.Invoke((TControl) this.Control, this.Model);

    protected override void OnDetached() => this.onDetached?.Invoke((TControl) this.Control, this.Model);
}