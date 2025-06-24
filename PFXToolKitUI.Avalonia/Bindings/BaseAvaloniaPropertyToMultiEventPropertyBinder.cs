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
using PFXToolKitUI.Avalonia.Bindings.Events;

namespace PFXToolKitUI.Avalonia.Bindings;

/// <summary>
/// A base class which inherits <see cref="BaseAvaloniaPropertyBinder{TModel}"/> and also implements
/// an event handler for the model which fires the <see cref="IBinder.UpdateControl"/> method
/// </summary>
/// <typeparam name="TModel">The model type</typeparam>
public abstract class BaseAvaloniaPropertyToMultiEventPropertyBinder<TModel> : BaseAvaloniaPropertyBinder<TModel>, IRelayEventHandler where TModel : class {
    private readonly SenderEventRelay[] autoEventHelpers;

    protected BaseAvaloniaPropertyToMultiEventPropertyBinder(string eventName) : this(null, eventName) {
    }

    protected BaseAvaloniaPropertyToMultiEventPropertyBinder(AvaloniaProperty? property, params string[] eventNames) : base(property) {
        this.autoEventHelpers = new SenderEventRelay[eventNames.Length];
        for (int i = 0; i < eventNames.Length; i++) {
            this.autoEventHelpers[i] = EventRelayBinderUtils.GetEventRelay(typeof(TModel), eventNames[i]);
        }
    }

    /// <summary>
    /// Invoked by the model's value changed event handler. By default, this method invokes <see cref="IBinder.UpdateControl"/>
    /// </summary>
    protected virtual void OnModelValueChanged() => this.UpdateControl();

    protected override void OnAttached() {
        base.OnAttached();
        foreach (SenderEventRelay aeh in this.autoEventHelpers)
            EventRelayBinderUtils.OnAttached(this.myModel!, this, aeh);
    }

    protected override void OnDetached() {
        base.OnDetached();
        foreach (SenderEventRelay aeh in this.autoEventHelpers)
            EventRelayBinderUtils.OnDetached(this.myModel!, this, aeh);
    }

    void IRelayEventHandler.OnEventFired() => this.OnModelValueChanged();
}