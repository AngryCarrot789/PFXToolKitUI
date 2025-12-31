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

using PFXToolKitUI.EventHelpers;

namespace PFXToolKitUI.Avalonia.Bindings.Enums;

/// <summary>
/// A class which helps bind radio buttons to an enum property
/// </summary>
public class EventPropertyEnumBinder<TEnum> : BaseEnumBinder<TEnum>, IRelayEventHandler where TEnum : unmanaged, Enum {
    private readonly Action<object, TEnum> setter;
    private readonly Func<object, TEnum> getter;
    private readonly EventWrapper eventRelay;

    /// <summary>
    /// Gets or sets the active transferable data owner
    /// </summary>
    public object? Model { get; private set; }
    
    public override bool IsAttached => this.Model != null;

    public EventPropertyEnumBinder(Type modelType, string eventName, Func<object, TEnum> getter, Action<object, TEnum> setter) {
        this.eventRelay = EventRelayStorage.UIStorage.GetEventRelay(modelType, eventName);
        this.setter = setter;
        this.getter = getter;
    }
    
    void IRelayEventHandler.OnEvent(object sender) => this.OnModelEnumChanged();
    
    private void OnModelEnumChanged() {
        if (this.Model == null)
            throw new Exception("Fatal application bug");
        
        this.UpdateControls(this.getter(this.Model));
    }

    public void Attach(object model) {
        ArgumentNullException.ThrowIfNull(model);
        if (this.Model != null)
            throw new InvalidOperationException("Already attached");
        
        EventRelayStorage.UIStorage.AddHandler(this.Model = model, this, this.eventRelay);
        this.UpdateControls(this.getter(model));
    }

    public void Detach() {
        if (this.Model == null)
            throw new InvalidOperationException("Not attached");

        EventRelayStorage.UIStorage.RemoveHandler(this.Model!, this, this.eventRelay);
        this.Model = null;
    }

    protected override void SetValue(TEnum value) {
        this.setter(this.Model!, value);
    }

    protected override TEnum GetValue() {
        return this.getter(this.Model!);
    }
}