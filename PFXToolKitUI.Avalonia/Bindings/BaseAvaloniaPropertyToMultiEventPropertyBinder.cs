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

using Avalonia;

namespace PFXToolKitUI.Avalonia.Bindings;

/// <summary>
/// A base class which inherits <see cref="BaseAvaloniaPropertyBinder{TModel}"/> and also implements
/// an event handler for the model which fires the <see cref="IBinder.UpdateControl"/> method
/// </summary>
/// <typeparam name="TModel">The model type</typeparam>
public abstract class BaseAvaloniaPropertyToMultiEventPropertyBinder<TModel> : BaseAvaloniaPropertyBinder<TModel> where TModel : class {
    private readonly AutoEventHelper[] autoEventHelpers;

    protected BaseAvaloniaPropertyToMultiEventPropertyBinder(string eventName) : this(null, eventName) {
    }

    protected BaseAvaloniaPropertyToMultiEventPropertyBinder(AvaloniaProperty? property, params string[] eventNames) : base(property) {
        Action callback = this.OnModelValueChanged;
        this.autoEventHelpers = new AutoEventHelper[eventNames.Length];
        for (int i = 0; i < eventNames.Length; i++) {
            this.autoEventHelpers[i] = new AutoEventHelper(eventNames[i], typeof(TModel), callback);
        }
    }

    /// <summary>
    /// Invoked by the model's value changed event handler. By default this method invokes <see cref="IBinder.UpdateControl"/>
    /// </summary>
    protected virtual void OnModelValueChanged() => this.UpdateControl();

    protected override void OnAttached() {
        base.OnAttached();
        foreach (AutoEventHelper aeh in this.autoEventHelpers)
            aeh.AddEventHandler(this.myModel!);
    }

    protected override void OnDetached() {
        base.OnDetached();
        foreach (AutoEventHelper aeh in this.autoEventHelpers)
            aeh.RemoveEventHandler(this.myModel!);
    }
}