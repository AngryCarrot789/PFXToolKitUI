// 
// Copyright (c) 2025-2025 REghZy
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

namespace PFXToolKitUI.EventHelpers;

public readonly struct EventWrapper {
    private readonly object[] handlerInArray;

    /// <summary>
    /// Gets the event info
    /// </summary>
    public EventInfo EventInfo { get; }

    /// <summary>
    /// Gets the handlers that is added/removed as an event handler
    /// </summary>
    public Delegate Handler => (Delegate) this.handlerInArray[0];

    public EventWrapper(EventInfo eventInfo, Delegate handlerDelegate) {
        this.EventInfo = eventInfo;
        this.handlerInArray = [handlerDelegate];
    }

    public static EventWrapper CreateSimple(string eventName, Type ownerType, Action callback) {
        ArgumentNullException.ThrowIfNull(eventName);
        EventInfo? info = ownerType.GetEvent(eventName, BindingFlags.Public | BindingFlags.Instance);
        if (info == null)
            throw new Exception("Could not find event by name: " + ownerType.Name + "." + eventName);

        return CreateSimple(info, callback);
    }
    
    public static EventWrapper CreateSimple(EventInfo eventInfo, Action callback) {
        ArgumentNullException.ThrowIfNull(eventInfo);
        Type handlerType = eventInfo.EventHandlerType ?? throw new Exception("Missing event handler type");
        return new EventWrapper(eventInfo, EventReflectionUtils.CreateDelegateToInvokeActionFromEvent(handlerType, callback));
    }

    public static EventWrapper CreateWithSender(string eventName, Type senderType, Action<object> callback) {
        return CreateInternal(eventName, senderType, callback, null /* no extra parameter */);
    }

    public static EventWrapper CreateWithSenderAndState(string eventName, Type senderType, Action<object, object> callback, object state) {
        return CreateInternal(eventName, senderType, callback, state);
    }

    public static EventWrapper CreateWithSender<T>(string eventName, Action<T> callback) where T : class {
        return CreateInternal(eventName, typeof(T), callback, null /* no extra parameter */);
    }

    public static EventWrapper CreateWithSenderAndState<T>(string eventName, Action<T, object> callback, object state) where T : class {
        return CreateInternal(eventName, typeof(T), callback, state);
    }

    public static EventWrapper CreateWithSender<T>(EventInfo eventInfo, Action<T> callback) where T : class {
        return CreateInternal(eventInfo, typeof(T), callback, null /* no extra parameter */);
    }

    public static EventWrapper CreateWithSenderAndState<T>(EventInfo eventInfo, Action<T, object> callback, object state) where T : class {
        return CreateInternal(eventInfo, typeof(T), callback, state);
    }

    private static EventWrapper CreateInternal(string eventName, Type senderType, Delegate callback, object? state) {
        ArgumentNullException.ThrowIfNull(senderType);
        return CreateInternal(EventReflectionUtils.GetEventInfoForName(senderType, eventName), senderType, callback, state);
    }

    internal static EventWrapper CreateInternal(EventInfo eventInfo, Type senderType, Delegate callback, object? state) {
        ArgumentNullException.ThrowIfNull(eventInfo);
        Type handlerType = eventInfo.EventHandlerType ?? throw new Exception("Missing event handler type");
        return new EventWrapper(eventInfo, EventReflectionUtils.CreateDelegateToInvokeActionFromEvent(handlerType, callback, senderType, state));
    }

    internal static EventWrapper CreateUnsafe(EventInfo eventInfo, Delegate eventHandlerCallback) {
        ArgumentNullException.ThrowIfNull(eventInfo);
        ArgumentNullException.ThrowIfNull(eventHandlerCallback);
        return new EventWrapper(eventInfo, eventHandlerCallback);
    }

    public void AddEventHandler(object model) {
        this.EventInfo.GetAddMethod()!.Invoke(model, this.handlerInArray);
    }

    public void RemoveEventHandler(object model) {
        this.EventInfo.GetRemoveMethod()!.Invoke(model, this.handlerInArray);
    }
}