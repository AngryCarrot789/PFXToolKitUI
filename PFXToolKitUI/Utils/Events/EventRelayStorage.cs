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

namespace PFXToolKitUI.Utils.Events;

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

/// <summary>
/// Used as storage space for cached event relays and manages all object instances that may cause event to be fired.
/// The default is <see cref="UIStorage"/>. Whether a new instance is needed is completely empirical
/// </summary>
public sealed class EventRelayStorage {
    /// <summary>
    /// A shared instance for UI related relays
    /// </summary>
    public static readonly EventRelayStorage UIStorage = new EventRelayStorage();
    
    private readonly ConcurrentDictionary<EventInfoKey, Lazy<SenderEventRelay>> eventInfoToRelayMap;

    // IDictionary<object, IDictionary<string, IRelayEventHandler[]>>
    private readonly ConcurrentDictionary<object, HybridDictionary> attachedInstanceMap;
    private readonly Action<object, object> cachedModeEventFired;

#if DEBUG
    public IEnumerable<KeyValuePair<object, IEnumerable<KeyValuePair<string, IRelayEventHandler[]>>>> DEBUG_AttachmentMap {
        get { return this.attachedInstanceMap.Select(x => new KeyValuePair<object, IEnumerable<KeyValuePair<string, IRelayEventHandler[]>>>(x.Key, x.Value.Cast<DictionaryEntry>().Select(y => new KeyValuePair<string, IRelayEventHandler[]>((string) y.Key, (IRelayEventHandler[]) y.Value!)))); }
    }
#endif

    public EventRelayStorage() {
        this.eventInfoToRelayMap = new ConcurrentDictionary<EventInfoKey, Lazy<SenderEventRelay>>();
        this.attachedInstanceMap = new ConcurrentDictionary<object, HybridDictionary>(ReferenceEqualityComparer.Instance);
        this.cachedModeEventFired = this.OnEventRaised;
    }

    public void AddHandler(object instance, IRelayEventHandler handler, SenderEventRelay relay) {
        HybridDictionary eventToHandlerList = this.attachedInstanceMap.GetOrAdd(instance, _ => new HybridDictionary());

        lock (eventToHandlerList) {
            IRelayEventHandler[]? array = (IRelayEventHandler[]?) eventToHandlerList[relay.EventName];
            if (array == null) {
                relay.AddEventHandler(instance);
                eventToHandlerList[relay.EventName] = new[] { handler };
            }
            else {
                eventToHandlerList[relay.EventName] = ArrayUtils.Add(array, handler);
            }
        }
    }

    public void RemoveHandler(object instance, IRelayEventHandler handler, SenderEventRelay relay) {
        HybridDictionary eventToHandlerList = this.attachedInstanceMap[instance];

        lock (eventToHandlerList) {
            IRelayEventHandler[]? array = (IRelayEventHandler[]?) eventToHandlerList[relay.EventName];
            if (array == null)
                return;

            if (array.Length == 1) {
                relay.RemoveEventHandler(instance); // no more binders listen to event, so remove handler
                eventToHandlerList.Remove(relay.EventName); // remove the list to open possibility to remove model from attachedInstanceMap
                if (eventToHandlerList.Count == 0) {
                    // prevent memory leak. the model should always be removed as soon as all binders using it are detached.
                    bool removed = this.attachedInstanceMap.TryRemove(instance, out HybridDictionary? removedInnerMap);
                    Debug.Assert(removed && removedInnerMap == eventToHandlerList);
                }
            }
            else {
                int idx = ArrayUtils.IndexOf_RefType(array, handler);
                Debug.Assert(idx != -1);
                eventToHandlerList[relay.EventName] = ArrayUtils.RemoveAt(array, idx);
            }
        }
    }

    public SenderEventRelay GetEventRelay(Type modelType, string eventName) {
        return this.eventInfoToRelayMap.GetOrAdd(new EventInfoKey(modelType, eventName), CreateLazyRelay, this).Value;
    }

    private static Lazy<SenderEventRelay> CreateLazyRelay(EventInfoKey key, EventRelayStorage t) {
        // Lazy is required because we really don't want to create multiple instances of
        // SenderEventRelay for the exact same event, because it's expensive and wasteful.
        return new Lazy<SenderEventRelay>(() => SenderEventRelay.Create(key.EventName, key.OwnerType, t.cachedModeEventFired, key.EventName));
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