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

namespace PFXToolKitUI.Avalonia.Bindings;

/// <summary>
/// A binder that does not listen to property changes or listen to any kind of event.
/// Useful for binding immutable models to a UI.
/// </summary>
/// <typeparam name="TModel"></typeparam>
public class ManualBinder<TModel> : BaseBinder<TModel> where TModel : class {
    private readonly Action<IBinder<TModel>> onAttached;
    private readonly Action<IBinder<TModel>>? onDetached;

    public ManualBinder(Action<IBinder<TModel>> onAttached, Action<IBinder<TModel>>? onDetached = null) {
        this.onAttached = onAttached;
        this.onDetached = onDetached;
    }

    protected override void UpdateModelOverride() {
    }

    protected override void UpdateControlOverride() {
    }

    protected override void OnAttached() {
        this.onAttached.Invoke(this);
    }

    protected override void OnDetached() {
        this.onDetached?.Invoke(this);
    }
}