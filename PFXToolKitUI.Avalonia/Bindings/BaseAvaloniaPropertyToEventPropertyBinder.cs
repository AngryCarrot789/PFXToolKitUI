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
using PFXToolKitUI.Utils.Events;

namespace PFXToolKitUI.Avalonia.Bindings;

/// <summary>
/// A base class which inherits <see cref="BaseAvaloniaPropertyBinder{TModel}"/> and also implements
/// an event handler for the model which fires the <see cref="IBinder.UpdateControl"/> method
/// </summary>
/// <typeparam name="TModel">The model type</typeparam>
public abstract class BaseAvaloniaPropertyToEventPropertyBinder<TModel> : BaseAvaloniaPropertyBinder<TModel>, IRelayEventHandler where TModel : class {
    private readonly SenderEventRelay eventRelay;

    protected BaseAvaloniaPropertyToEventPropertyBinder(string eventName) : this(null, eventName) {
    }

    protected BaseAvaloniaPropertyToEventPropertyBinder(AvaloniaProperty? property, string eventName) : base(property) {
        this.eventRelay = EventRelayStorage.UIStorage.GetEventRelay(typeof(TModel), eventName);
    }

    /// <summary>
    /// Invoked by the model's value changed event handler. By default this method invokes <see cref="IBinder.UpdateControl"/>
    /// </summary>
    protected virtual void OnModelValueChanged() => this.UpdateControl();

    void IRelayEventHandler.OnEvent(object sender) => this.OnModelValueChanged();
    
    protected override void OnAttached() {
        base.OnAttached();
        EventRelayStorage.UIStorage.AddHandler(this.myModel!, this, this.eventRelay);
    }

    protected override void OnDetached() {
        base.OnDetached();
        EventRelayStorage.UIStorage.RemoveHandler(this.myModel!, this, this.eventRelay);
    }
}