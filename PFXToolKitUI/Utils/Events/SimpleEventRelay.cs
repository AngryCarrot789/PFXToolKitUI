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

using System.Reflection;

namespace PFXToolKitUI.Utils.Events;

/// <summary>
/// A struct that generates an event handler (in the form of an action) from almost any event
/// </summary>
public readonly struct SimpleEventRelay {
    public readonly EventInfo EventInfo;
    public readonly Delegate HandlerDelegate;

    public SimpleEventRelay(string eventName, Type modelType, Action callback) {
        ArgumentNullException.ThrowIfNull(eventName);

        EventInfo? info = modelType.GetEvent(eventName, BindingFlags.Public | BindingFlags.Instance);
        if (info == null)
            throw new Exception("Could not find event by name: " + modelType.Name + "." + eventName);

        Type handlerType = info.EventHandlerType ?? throw new Exception("Missing event handler type");

        this.EventInfo = info;
        this.HandlerDelegate = EventReflectionUtils.CreateDelegateToInvokeActionFromEvent(handlerType, callback);
    }
    
    public void AddEventHandler(object model) {
        this.EventInfo.AddEventHandler(model, this.HandlerDelegate);
    }

    public void RemoveEventHandler(object model) {
        this.EventInfo.RemoveEventHandler(model, this.HandlerDelegate);
    }
}