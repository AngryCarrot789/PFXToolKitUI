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

using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.Avalonia.Bindings.Events;

internal interface IRelayEventHandler {
    void OnEventFired();
}

internal static class EventRelayBinderUtils {
    private readonly struct EventInfoKey(Type ownerType, string eventName) : IEquatable<EventInfoKey> {
        public readonly Type OwnerType = ownerType;
        public readonly string EventName = eventName;
        public bool Equals(EventInfoKey other) => this.OwnerType == other.OwnerType && this.EventName == other.EventName;
        public override bool Equals(object? obj) => obj is EventInfoKey other && this.Equals(other);
        public override int GetHashCode() => HashCode.Combine(this.OwnerType, this.EventName);
        public override string ToString() => $"{this.OwnerType.Name}#{this.EventName}";
    }

    // Only modified on main thread by binder constructors
    private static readonly Dictionary<EventInfoKey, SenderEventRelay> modelTypeToEventRelayMap;

    // This is concurrent because model events are allowed to be fired from any event,
    // and OnAttached/OnDetached only get invoked on the main thread. There's a chance that
    // any model fires their event right as OnAttached or OnDetached is called.
    // Dictionary<Model, Dictionary<EventName, IRelayEventHandler[]>>
    private static readonly ConcurrentDictionary<object, HybridDictionary> attachedInstanceMap;

    static EventRelayBinderUtils() {
        modelTypeToEventRelayMap = new Dictionary<EventInfoKey, SenderEventRelay>();
        attachedInstanceMap = new ConcurrentDictionary<object, HybridDictionary>(ReferenceEqualityComparer.Instance);
    }

    // Both OnAttached and OnDetached can only be called on the main thread (soft assert).
    // The main thread is also the only one that can modify attachedInstanceMap and the inner map
    // But since the event handlers can be fired on any thread, we still need to support concurrent reads

    public static void OnAttached(object model, IRelayEventHandler binder, SenderEventRelay relay) {
        Debug.Assert(ApplicationPFX.Instance.Dispatcher.CheckAccess());

        HybridDictionary eventToHandlerList = attachedInstanceMap.GetOrAdd(model, _ => new HybridDictionary(caseInsensitive: false));
        IRelayEventHandler[]? array = (IRelayEventHandler[]?) eventToHandlerList[relay.EventName];
        if (array == null) {
            relay.AddEventHandler(model);
            eventToHandlerList[relay.EventName] = new[] { binder };
        }
        else {
            eventToHandlerList[relay.EventName] = ArrayUtils.Add(array, binder);
        }
    }

    public static void OnDetached(object model, IRelayEventHandler binder, SenderEventRelay relay) {
        Debug.Assert(ApplicationPFX.Instance.Dispatcher.CheckAccess());

        HybridDictionary eventToHandlerList = attachedInstanceMap[model];
        IRelayEventHandler[] array = (IRelayEventHandler[]) eventToHandlerList[relay.EventName]!;
        Debug.Assert(array != null);

        int idx = ArrayUtils.IndexOf_RefType(array, binder);
        Debug.Assert(idx != -1);

        if (array.Length == 1) {
            relay.RemoveEventHandler(model); // no more binders listen to event, so remove handler
            eventToHandlerList.Remove(relay.EventName); // remove the list to open possibility to remove model from attachedInstanceMap
            if (eventToHandlerList.Count == 0) {
                // prevent memory leak. the model should always be removed as soon as all binders using it are detached.
                bool removed = attachedInstanceMap.TryRemove(model, out HybridDictionary? removedInnerMap);
                Debug.Assert(removed && removedInnerMap == eventToHandlerList);
            }
        }
        else {
            eventToHandlerList[relay.EventName] = ArrayUtils.RemoveAt(array, idx);
        }
    }

    public static SenderEventRelay GetEventRelay(Type modelType, string eventName) {
        Debug.Assert(ApplicationPFX.Instance.Dispatcher.CheckAccess());

        EventInfoKey infoKey = new EventInfoKey(modelType, eventName);
        if (modelTypeToEventRelayMap.TryGetValue(infoKey, out SenderEventRelay relay)) {
            return relay;
        }

        return modelTypeToEventRelayMap[infoKey] = SenderEventRelay.Create(eventName, modelType, OnModelEventFired, eventName);
    }

    // Note: this can be invoked from any thread
    private static void OnModelEventFired(object model, object theEventName) {
        // assert theEventName is string;
        if (attachedInstanceMap.TryGetValue(model, out HybridDictionary? dictionary)) {
            IRelayEventHandler[]? handlers = (IRelayEventHandler[]?) dictionary[theEventName];
            if (handlers != null) {
                Debug.Assert(handlers.Length > 0);
                foreach (IRelayEventHandler handler in handlers) {
                    handler.OnEventFired();
                }
            }
        }
    }

    [Conditional("DEBUG")]
    public static void CheckMemoryLeaksOnAppShutdown() {
        if (!attachedInstanceMap.IsEmpty) {
            Debug.WriteLine("Warning: binder memory leaks present (models still attached)");
            foreach (KeyValuePair<object, HybridDictionary> entry in attachedInstanceMap) {
                Debug.WriteLine($"Model Type: {entry.Key.GetType().FullName}");
                foreach (DictionaryEntry subEntry in entry.Value) {
                    Debug.WriteLine($"   Event Name: {subEntry.Key}");
                    Debug.WriteLine( "   Handler(s) (most likely the binder classes):");
                    foreach (IRelayEventHandler handler in (IRelayEventHandler[]) subEntry.Value!) {
                        Debug.WriteLine($"      - {handler.GetType().FullName}");
                        if (handler is IBinder binder && binder.IsFullyAttached) {
                            Debug.WriteLine($"        (attached to {binder.Debug_Control?.GetType().FullName})");
                        }
                    }
                }
            }

            Debugger.Break();
        }
    }

    // Made this before switching to HybridDictionary. This may be worth a try since it's rare that
    // multiple IBinders listen to the same event on the same model instance, so most the time the mode is 1 (field is handler)
    private readonly struct FastList : IEnumerable<object> {
        private readonly object value;
        private readonly int mode; // 0 = empty, 1 = field is the value, 2 = field is object[]{val1, val2}, 3 = list of vals

        private FastList(object value, int mode) {
            this.value = value;
            this.mode = mode;
        }

        public FastList Add(object newValue) {
            object field;
            switch (this.mode) {
                // new fastlist with value as field
                case 0: return new FastList(newValue, 1);
                // upgrade to array
                case 1: return new FastList(new object[] { this.value, newValue }, 2);
                // upgrade to list
                case 2:
                    field = this.value;
                    object[] array = Unsafe.As<object, object[]>(ref field);
                    return new FastList(new List<object>() { array[0], array[1], newValue }, 2);
                // append value to list
                default:
                    Debug.Assert(this.mode == 3);
                    field = this.value;
                    List<object> list = Unsafe.As<object, List<object>>(ref field);
                    list.Add(newValue);
                    return this;
            }
        }

        [SuppressMessage("ReSharper", "AssignmentInConditionalExpression")]
        public FastList Remove(object valToRem, out bool removed) {
            object field;
            switch (this.mode) {
                // alreay empty so...
                case 0:
                    removed = false;
                    return this;
                // downgrade to empty on match
                case 1: return (removed = Equals(this.value, valToRem)) ? default : this;
                // downgrade to single field on match
                case 2:
                    field = this.value;
                    object[] array = Unsafe.As<object, object[]>(ref field);
                    if (removed = Equals(array[0], valToRem))
                        return new FastList(array[1], 1);
                    if (removed = Equals(array[1], valToRem))
                        return new FastList(array[0], 1);
                    return default;
                // There's 2 options: we either stay as list forever, or downgrade to array.
                // However, the likely chance is that the list will be needed again soon, so we go with that options
                default:
                    Debug.Assert(this.mode == 3);
                    field = this.value;
                    List<object> list = Unsafe.As<object, List<object>>(ref field);
                    removed = list.Remove(valToRem);
                    return this;
            }
        }

        public IEnumerator<object> GetEnumerator() {
            switch (this.mode) {
                case 0: return Enumerable.Empty<object>().GetEnumerator();
                case 1: return Enumerable.Repeat(this.value, 1).GetEnumerator();
                default:
                    Debug.Assert(this.mode == 2 || this.mode == 3);
                    return ((IEnumerable<object>) this.value).GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}