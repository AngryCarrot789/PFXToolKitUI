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
using System.Reflection;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.EventHelpers;

/// <summary>
/// Used as storage space for cached event relays and manages all object instances that may cause event to be fired.
/// The default is <see cref="UIStorage"/>. Whether a new instance is needed is completely empirical.
/// <para>
/// This class is thread safe.
/// </para>
/// </summary>
public sealed class EventRelayStorage {
    /// <summary>
    /// A shared instance for UI related relays
    /// </summary>
    public static readonly EventRelayStorage UIStorage = new EventRelayStorage();

    #region Relay Caching

    // We use a lock instead of concurrent dictionary, because GetEventRelay will potentially
    // mutate eventInfoToRelayMap and maybe cachedDelegateHandlers, and that method needs to
    // be thread safe. Locking is the safer, and probably faster (based on current usage) option
    private readonly Lock eventInfoMapLock = new Lock();
    private readonly Dictionary<EventInfoKey, EventWrapper> eventInfoToRelayMap;

    // A cache of delegates that invoke OnEventRaised. The key is the event name passed to OnEventRaised.
    // This is used because, although it's not common, sometimes there's events of type EventHandler
    // with the same name but with different defining types (e.g. MyObject.NameChanged and MyOtherObject.NameChanged).
    // So, with the current impl, we can reuse the same delegate and same some memory.
    private readonly Dictionary<string, Delegate> cachedHandlerForEventHandlerType = new Dictionary<string, Delegate>();

    #endregion

    #region Attachment Map

    // IDictionary<object, IDictionary<string, IRelayEventHandler[]>>
    // Not yet benchmarked but ConcurrentDictionary here may provide a performance advantage
    // because events could be fired from any thread, 
    private readonly ConcurrentDictionary<object, HybridDictionary> attachedInstanceMap;
    
    // We cache our instance handler to prevent (potentially) creating a
    // new delegate for each new relay created by GetEventRelay()
    private readonly Action<object, object> m_OnEventRaised;

#if DEBUG
    public IEnumerable<KeyValuePair<object, IEnumerable<KeyValuePair<string, IRelayEventHandler[]>>>> DEBUG_AttachmentMap {
        get { return this.attachedInstanceMap.Select(x => new KeyValuePair<object, IEnumerable<KeyValuePair<string, IRelayEventHandler[]>>>(x.Key, x.Value.Cast<DictionaryEntry>().Select(y => new KeyValuePair<string, IRelayEventHandler[]>((string) y.Key, (IRelayEventHandler[]) y.Value!)))); }
    }
#endif

    #endregion

    public EventRelayStorage() {
        this.eventInfoToRelayMap = new Dictionary<EventInfoKey, EventWrapper>();
        this.attachedInstanceMap = new ConcurrentDictionary<object, HybridDictionary>(ReferenceEqualityComparer.Instance);
        this.m_OnEventRaised = this.OnEventRaised;
    }

    public void AddHandler(object instance, IRelayEventHandler handler, EventWrapper relay) {
        HybridDictionary eventToHandlerList = this.attachedInstanceMap.GetOrAdd(instance, _ => new HybridDictionary());
        string name = relay.EventInfo.Name;
        
        lock (eventToHandlerList) {
            IRelayEventHandler[]? array = (IRelayEventHandler[]?) eventToHandlerList[name];
            if (array == null) {
                relay.AddEventHandler(instance);
                eventToHandlerList[name] = new[] { handler };
            }
            else {
                eventToHandlerList[name] = ArrayUtils.Add(array, handler);
            }
        }
    }

    public void RemoveHandler(object instance, IRelayEventHandler handler, EventWrapper relay) {
        HybridDictionary eventToHandlerList = this.attachedInstanceMap[instance];

        string name = relay.EventInfo.Name;
        lock (eventToHandlerList) {
            IRelayEventHandler[]? array = (IRelayEventHandler[]?) eventToHandlerList[name];
            if (array != null) {
                if (array.Length == 1) {
                    relay.RemoveEventHandler(instance); // no more binders listen to event, so remove handler
                    eventToHandlerList.Remove(name); // remove the list to open possibility to remove model from attachedInstanceMap
                    if (eventToHandlerList.Count == 0) {
                        // prevent memory leak. the model should always be removed as soon as all binders using it are detached.
                        bool removed = this.attachedInstanceMap.TryRemove(instance, out HybridDictionary? removedInnerMap);
                        Debug.Assert(removed && removedInnerMap == eventToHandlerList);
                    }
                }
                else {
                    int idx = ArrayUtils.IndexOfRef(array, handler);
                    Debug.Assert(idx != -1);
                    eventToHandlerList[name] = ArrayUtils.RemoveAt(array, idx);
                }
            }
        }
    }

    public EventWrapper GetEventRelay(Type modelType, string eventName) {
        EventInfoKey key = new EventInfoKey(modelType, eventName);

        lock (this.eventInfoMapLock) {
            if (!this.eventInfoToRelayMap.TryGetValue(key, out EventWrapper relay)) {
                EventInfo info = EventReflectionUtils.GetEventInfoForName(modelType, eventName);
                Type handlerType = info.EventHandlerType ?? throw new Exception("Missing event handler type");
                Delegate eventHandler = this.GetOrCreateEventHandler(handlerType, eventName);
                this.eventInfoToRelayMap[key] = relay = EventWrapper.CreateUnsafe(info, eventHandler);
            }

            return relay;
        }
    }

    private Delegate GetOrCreateEventHandler(Type handlerType, string eventName) {
        bool isEventHandlerType = handlerType == typeof(EventHandler);
        
        // Try to get an existing delegate for the event name, otherwise, create and cache one
        if (isEventHandlerType && this.cachedHandlerForEventHandlerType.TryGetValue(eventName, out Delegate? handler)) {
            return handler;
        }
        
        // Most likely case is that EventHandlerType is EventHandler
        handler = EventReflectionUtils.CreateDelegateToInvokeActionFromEvent(handlerType, this.m_OnEventRaised, typeof(object), eventName);
        if (isEventHandlerType) {
            // Only cache EventHandler types
            this.cachedHandlerForEventHandlerType[eventName] = handler;
        }

        return handler;
    }

    // Note: this can be invoked from any thread
    private void OnEventRaised(object model, object theEventName) {
        // assert theEventName is string;
        if (this.attachedInstanceMap.TryGetValue(model, out HybridDictionary? dictionary)) {
            IRelayEventHandler[]? handlers;
            lock (dictionary) {
                handlers = (IRelayEventHandler[]?) dictionary[theEventName];
            }

            if (handlers != null) {
                foreach (IRelayEventHandler handler in handlers) {
                    handler.OnEvent(model);
                }
            }
        }
    }
}

/// <summary>
/// An interface implemented by an object that can receive events when use in conjunction with <see cref="EventRelayStorage"/>
/// </summary>
public interface IRelayEventHandler {
    /// <summary>
    /// Invoked when the sender's event is invoked
    /// </summary>
    /// <param name="sender">The object whose event was invoked</param>
    void OnEvent(object sender);
}