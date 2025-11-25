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

namespace PFXToolKitUI.EventHelpers;

/// <summary>
/// An event relay that is similar to <see cref="SimpleEventRelay"/> but treats a parameter as the sender
/// instance, which would be the same instance passed to <see cref="SimpleEventRelay.AddEventHandler"/>
/// </summary>
public readonly struct SenderEventRelay {
    public readonly EventInfo EventInfo;
    public readonly MethodInfo AddMethod, RemoveMethod;
    public readonly string EventName;
    private readonly object[] HandlerDelegateInArray;

    public Delegate HandlerDelegate => (Delegate) this.HandlerDelegateInArray[0]!;

    private SenderEventRelay(EventInfo eventInfo, Delegate handlerDelegate, MethodInfo addMethod, MethodInfo removeMethod) {
        this.EventInfo = eventInfo;
        this.EventName = eventInfo.Name;
        this.HandlerDelegateInArray = [handlerDelegate];
        this.AddMethod = addMethod;
        this.RemoveMethod = removeMethod;
    }

    public static SenderEventRelay Create(string eventName, Type senderType, Action<object> callback) {
        return CreateInternal(eventName, senderType, callback, null /* no extra parameter */);
    }

    public static SenderEventRelay Create(string eventName, Type senderType, Action<object, object> callback, object state) {
        return CreateInternal(eventName, senderType, callback, state);
    }

    public static SenderEventRelay Create<T>(string eventName, Action<T> callback) where T : class {
        return CreateInternal(eventName, typeof(T), callback, null /* no extra parameter */);
    }
    
    public static SenderEventRelay Create<T>(string eventName, Action<T, object> callback, object state) where T : class {
        return CreateInternal(eventName, typeof(T), callback, state);
    }
    
    public static SenderEventRelay Create<T>(EventInfo eventInfo, Action<T> callback) where T : class {
        return CreateInternal(eventInfo, typeof(T), callback, null /* no extra parameter */);
    }
    
    public static SenderEventRelay Create<T>(EventInfo eventInfo, Action<T, object> callback, object state) where T : class {
        return CreateInternal(eventInfo, typeof(T), callback, state);
    }
    
    private static SenderEventRelay CreateInternal(string eventName, Type senderType, Delegate callback, object? state) {
        ArgumentNullException.ThrowIfNull(senderType);
        return CreateInternal(EventReflectionUtils.GetEventInfoForName(senderType, eventName), senderType, callback, state);
    }
    
    internal static SenderEventRelay CreateInternal(EventInfo eventInfo, Type senderType, Delegate callback, object? state) {
        ArgumentNullException.ThrowIfNull(eventInfo);
        Type handlerType = eventInfo.EventHandlerType ?? throw new Exception("Missing event handler type");
        GetAddAndRemoveMethods(eventInfo, out MethodInfo addMethod, out MethodInfo removeMethod);
        return new SenderEventRelay(eventInfo, EventReflectionUtils.CreateDelegateToInvokeActionFromEvent(handlerType, callback, senderType, state), addMethod, removeMethod);
    }
    
    internal static SenderEventRelay CreateUnsafe(EventInfo eventInfo, Delegate eventHandlerCallback) {
        ArgumentNullException.ThrowIfNull(eventInfo);
        ArgumentNullException.ThrowIfNull(eventHandlerCallback);
        GetAddAndRemoveMethods(eventInfo, out MethodInfo addMethod, out MethodInfo removeMethod);
        return new SenderEventRelay(eventInfo, eventHandlerCallback, addMethod, removeMethod);
    }

    private static void GetAddAndRemoveMethods(EventInfo eventInfo, out MethodInfo addMethod, out MethodInfo removeMethod) {
        addMethod = eventInfo.GetAddMethod(nonPublic: false) ?? throw new Exception("Missing add method");
        removeMethod = eventInfo.GetRemoveMethod(nonPublic: false) ?? throw new Exception("Missing remove method");
    }

    public void AddEventHandler(object model) => this.AddMethod.Invoke(model, this.HandlerDelegateInArray);

    public void RemoveEventHandler(object model) => this.RemoveMethod.Invoke(model, this.HandlerDelegateInArray);
}