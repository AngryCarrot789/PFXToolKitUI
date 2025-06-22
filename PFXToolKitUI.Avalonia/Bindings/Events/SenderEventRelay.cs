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

using System.Diagnostics;
using System.Reflection;
using PFXToolKitUI.AdvancedMenuService;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.Avalonia.Bindings.Events;

/// <summary>
/// An event relay that is similar to <see cref="SimpleEventRelay"/> but treats a parameter as the sender
/// instance, which would be the same instance passed to <see cref="SimpleEventRelay.AddEventHandler"/>
/// </summary>
public readonly struct SenderEventRelay {
    public readonly EventInfo EventInfo;
    public readonly MethodInfo AddMethod, RemoveMethod;
    public readonly string EventName;
    private readonly object?[] HandlerDelegateInArray;

    public Delegate HandlerDelegate => (Delegate) this.HandlerDelegateInArray[0]!;

    private SenderEventRelay(EventInfo eventInfo, Delegate handlerDelegate) {
        this.EventInfo = eventInfo;
        this.EventName = eventInfo.Name;
        this.HandlerDelegateInArray = [handlerDelegate];

        this.AddMethod = eventInfo.GetAddMethod(nonPublic: false) ?? throw new Exception("Missing add method");
        this.RemoveMethod = eventInfo.GetRemoveMethod(nonPublic: false) ?? throw new Exception("Missing remove method");
    }

    public static SenderEventRelay Create(string eventName, Type senderType, Action<object> callback) {
        return CreateInternal(eventName, senderType, callback, null /* no extra parameter */);
    }

    public static SenderEventRelay Create(string eventName, Type senderType, Action<object, object> callback, object extraParameter) {
        return CreateInternal(eventName, senderType, callback, extraParameter);
    }

    public static SenderEventRelay Create<T>(string eventName, Action<T> callback) where T : class {
        return CreateInternal(eventName, typeof(T), callback, null /* no extra parameter */);
    }
    
    public static SenderEventRelay Create<T>(string eventName, Action<T, object> callback, object extraParameter) where T : class {
        return CreateInternal(eventName, typeof(T), callback, extraParameter);
    }
    
    private static SenderEventRelay CreateInternal(string eventName, Type senderType, Delegate callback, object? extraParameter) {
        ArgumentNullException.ThrowIfNull(eventName);
        EventInfo? info = senderType.GetEvent(eventName, BindingFlags.Public | BindingFlags.Instance);
        if (info == null)
            throw new Exception("Could not find event by name: " + senderType.Name + "." + eventName);

        Type handlerType = info.EventHandlerType ?? throw new Exception("Missing event handler type");
        return new SenderEventRelay(info, EventUtils.CreateDelegateToInvokeActionFromEvent(handlerType, callback, senderType, extraParameter));
    }

    public void AddEventHandler(object model) => this.AddMethod.Invoke(model, this.HandlerDelegateInArray);

    public void RemoveEventHandler(object model) => this.RemoveMethod.Invoke(model, this.HandlerDelegateInArray);
}

public static class TestShit {
    public static void Main() {
        SenderEventRelay relay1 = SenderEventRelay.Create<CommandContextEntry>(nameof(CommandContextEntry.DescriptionChanged), OnDescriptionChangedForInstance1);
        SenderEventRelay relay2 = SenderEventRelay.Create(nameof(CommandContextEntry.DescriptionChanged), typeof(CommandContextEntry), OnDescriptionChangedForInstance2);
        SenderEventRelay relay3 = SenderEventRelay.Create(nameof(CommandContextEntry.DescriptionChanged), typeof(CommandContextEntry), OnDescriptionChangedForInstance3, "test hello");

        CommandContextEntry entry = new CommandContextEntry("oka");
        relay1.AddEventHandler(entry);
        relay2.AddEventHandler(entry);
        relay3.AddEventHandler(entry);

        entry.Description = "mr sexy!";
    }


    private static void OnDescriptionChangedForInstance1(CommandContextEntry obj) {
    }

    private static void OnDescriptionChangedForInstance2(object obj) {
    }

    private static void OnDescriptionChangedForInstance3(object arg1, object arg2) {
        Debug.Assert(arg2 is string s && s == "test hello");
    }
}