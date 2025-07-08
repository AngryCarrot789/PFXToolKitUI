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

using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.Diagnostics;
using PFXToolKitUI.Interactivity.Contexts;
using PFXToolKitUI.Utils;
using PFXToolKitUI.Utils.Events;
using PFXToolKitUI.Utils.RDA;

namespace PFXToolKitUI.AdvancedMenuService;

public static class ContextEntryExtensions {
    /// <summary>
    /// Adds a handler to <see cref="BaseContextEntry.CapturedContextChanged"/> that fires the update callback with the value keyed by the data key, or null if there is no value by that key.
    /// </summary>
    /// <param name="entry">The context entry to add the change handler to</param>
    /// <param name="key">The data key used to get the value of <see cref="T"/></param>
    /// <param name="update">Invoked when the context changes</param>
    /// <typeparam name="T">The type of value to pass to the context change callback</typeparam>
    /// <returns>The entry, for chained calling</returns>
    public static BaseContextEntry AddSimpleContextUpdate<T>(this BaseContextEntry entry, DataKey<T> key, Action<BaseContextEntry, T?> update) where T : class {
        entry.CapturedContextChanged += (sender, oldCtx, newCtx) => update(sender, newCtx != null && key.TryGetContext(newCtx, out T? value) ? value : null);
        return entry;
    }

    /// <summary>
    /// Same as <see cref="AddSimpleContextUpdate{T}"/> except supports two values.
    /// The callback is either called with all non-null parameters or all null.
    /// </summary>
    /// <returns>The entry</returns>
    public static BaseContextEntry AddSimpleContextUpdate<T1, T2>(this BaseContextEntry entry, DataKey<T1> key1, DataKey<T2> key2, Action<BaseContextEntry, T1?, T2?> update) where T1 : class where T2 : class {
        entry.CapturedContextChanged += (sender, oldCtx, newCtx) => {
            if (newCtx != null && key1.TryGetContext(newCtx, out T1? val1) && key2.TryGetContext(newCtx, out T2? val2))
                update(sender, val1, val2);
            else
                update(sender, null, null);
        };
        return entry;
    }

    /// <summary>
    /// Same as <see cref="AddSimpleContextUpdate{T}"/> except supports three values.
    /// The callback is either called with all non-null parameters or all null.
    /// </summary>
    /// <returns>The entry</returns>
    public static BaseContextEntry AddSimpleContextUpdate<T1, T2, T3>(this BaseContextEntry entry, DataKey<T1> key1, DataKey<T2> key2, DataKey<T3> key3, Action<BaseContextEntry, T1?, T2?, T3?> update) where T1 : class where T2 : class where T3 : class {
        entry.CapturedContextChanged += (sender, oldCtx, newCtx) => {
            if (newCtx != null && key1.TryGetContext(newCtx, out T1? val1) && key2.TryGetContext(newCtx, out T2? val2) && key3.TryGetContext(newCtx, out T3? val3))
                update(sender, val1, val2, val3);
            else
                update(sender, null, null, null);
        };
        return entry;
    }

    /// <summary>
    /// Adds a handler to <see cref="BaseContextEntry.CapturedContextChanged"/> that fires the update callback with previous and new value keyed by the data key.
    /// </summary>
    /// <param name="entry">The context entry to add the change handler to</param>
    /// <param name="key">The data key used to get the value of <see cref="T"/></param>
    /// <param name="onDataChanged">The callback invoked with the old and new value, tracked with a field inside a lambda class</param>
    /// <typeparam name="T">The type of value to pass to the context change callback</typeparam>
    /// <returns>The entry, for chained calling</returns>
    public static BaseContextEntry AddContextChangeHandler<T>(this BaseContextEntry entry, DataKey<T> key, Action<BaseContextEntry, T?, T?> onDataChanged) where T : class {
        T? field_CurrentValue = null;
        entry.CapturedContextChanged += (sender, oldCtx, newCtx) => {
            if (newCtx == null || !key.TryGetContext(newCtx, out T? newValue)) {
                if (field_CurrentValue != null) {
                    onDataChanged(sender, field_CurrentValue, null);
                    field_CurrentValue = null;
                }
            }
            else if (!Equals(field_CurrentValue, newValue)) {
                onDataChanged(sender, field_CurrentValue, newValue);
                field_CurrentValue = newValue;
            }
        };
        return entry;
    }

    public static BaseContextEntry AddContextValueChangeHandlerWithEvent<T>(this BaseContextEntry entry, DataKey<T> key, string eventName, Action<BaseContextEntry, T?> onUpdate) where T : class {
        entry.CapturedContextChanged += new SpecializedChangeHandler_Callback<T>(entry, key, [eventName], onUpdate).OnCapturedContextChanged;
        return entry;
    }
    
    public static BaseContextEntry AddContextValueChangeHandlerWithEvents<T>(this BaseContextEntry entry, DataKey<T> key, string[] eventNames, Action<BaseContextEntry, T?> onUpdate) where T : class {
        entry.CapturedContextChanged += new SpecializedChangeHandler_Callback<T>(entry, key, eventNames, onUpdate).OnCapturedContextChanged;
        return entry;
    }
    
    public static BaseContextEntry AddCanExecuteChangeUpdaterForEvent<T>(this BaseContextEntry entry, DataKey<T> key, string eventName) where T : class {
        entry.CapturedContextChanged += new SpecializedChangeHandler_UpdateCanExecute<T>(entry, key, [eventName]).OnCapturedContextChanged;
        return entry;
    }
    
    public static BaseContextEntry AddCanExecuteChangeUpdaterForEvents<T>(this BaseContextEntry entry, DataKey<T> key, string[] eventNames) where T : class {
        entry.CapturedContextChanged += new SpecializedChangeHandler_UpdateCanExecute<T>(entry, key, eventNames).OnCapturedContextChanged;
        return entry;
    }

    private interface ISpecializedChangeHandler {
        static readonly Dictionary<EventInfoKey, SenderEventRelay> cachedEventRelayMap = new Dictionary<EventInfoKey, SenderEventRelay>();
        static readonly ConcurrentDictionary<object, HybridDictionary> attachedInstanceMap = new ConcurrentDictionary<object, HybridDictionary>();

        void OnContextValueEventRaised();
    }

    private abstract class BaseSpecializedChangeHandler<T> : ISpecializedChangeHandler where T : class {
        protected readonly BaseContextEntry Entry;
        protected T? CurrentValue => this.currentValue;
        
        private T? currentValue;
        private readonly DataKey<T> key;
        private readonly string[] eventNames;
        private SenderEventRelay[]? relays;
        private RapidDispatchActionEx? rda;
        
        protected BaseSpecializedChangeHandler(BaseContextEntry entry, DataKey<T> key, string[] eventNames) {
            this.Entry = entry;
            this.key = key;
            this.eventNames = eventNames;
        }

        internal void OnCapturedContextChanged(BaseContextEntry sender, IContextData? oldCtx, IContextData? newCtx) {
            Debug.Assert(this.Entry == sender);
            if (newCtx == null || !this.key.TryGetContext(newCtx, out T? newValue)) {
                if (this.currentValue != null) {
                    foreach (SenderEventRelay relay in this.relays!) 
                        OnDetached(this.currentValue, this, relay);
                    this.currentValue = null;
                    this.Update();
                }
            }
            else if (!Equals(this.currentValue, newValue)) {
                if (this.relays != null && this.currentValue != null) {
                    foreach (SenderEventRelay relay in this.relays!) 
                        OnDetached(this.currentValue, this, relay);
                }

                if (this.relays == null) {
                    this.relays = new SenderEventRelay[this.eventNames.Length];
                    for (int i = 0; i < this.eventNames.Length; i++) {
                        this.relays[i] = GetEventRelay(this.key.DataType, this.eventNames[i]);
                    }
                }
                
                this.currentValue = newValue;
                this.Update();
                foreach (SenderEventRelay relay in this.relays!) 
                    OnAttached(newValue, this, relay);
            }
        }

        public void OnContextValueEventRaised() {
            if (ApplicationPFX.Instance.Dispatcher.CheckAccess()) {
                this.Update();
            }
            else {
                this.rda ??= RapidDispatchActionEx.ForSync(this.Update, "SpecializedChangeHandler");
                this.rda.InvokeAsync();
            }
        }

        protected abstract void Update();
    }
    
    private class SpecializedChangeHandler_Callback<T> : BaseSpecializedChangeHandler<T> where T : class {
        private readonly Action<BaseContextEntry, T?> onUpdate;

        public SpecializedChangeHandler_Callback(BaseContextEntry entry, DataKey<T> key, string[] eventNames, Action<BaseContextEntry, T?> onUpdate) 
            : base(entry, key, eventNames) {
            this.onUpdate = onUpdate;
        }

        protected override void Update() {
            this.onUpdate(this.Entry, this.CurrentValue);
        }
    }
    
    private class SpecializedChangeHandler_UpdateCanExecute<T> : BaseSpecializedChangeHandler<T> where T : class {
        public SpecializedChangeHandler_UpdateCanExecute(BaseContextEntry entry, DataKey<T> key, string[] eventNames) : base(entry, key, eventNames) {
        }

        protected override void Update() => this.Entry.RaiseCanExecuteChanged();
    }

    private static SenderEventRelay GetEventRelay(Type modelType, string eventName) {
        Debug.Assert(ApplicationPFX.Instance.Dispatcher.CheckAccess());

        EventInfoKey infoKey = new EventInfoKey(modelType, eventName);
        if (ISpecializedChangeHandler.cachedEventRelayMap.TryGetValue(infoKey, out SenderEventRelay relay)) {
            return relay;
        }

        return ISpecializedChangeHandler.cachedEventRelayMap[infoKey] = SenderEventRelay.Create(eventName, modelType, OnModelEventFired, eventName);
    }

    private static void OnAttached(object model, ISpecializedChangeHandler binder, SenderEventRelay relay) {
        Debug.Assert(ApplicationPFX.Instance.Dispatcher.CheckAccess());

        HybridDictionary eventToHandlerList = ISpecializedChangeHandler.attachedInstanceMap.GetOrAdd(model, _ => new HybridDictionary(caseInsensitive: false));
        ISpecializedChangeHandler[]? array = (ISpecializedChangeHandler[]?) eventToHandlerList[relay.EventName];
        if (array == null) {
            relay.AddEventHandler(model);
            eventToHandlerList[relay.EventName] = new[] { binder };
        }
        else {
            eventToHandlerList[relay.EventName] = ArrayUtils.Add(array, binder);
        }
    }

    private static void OnDetached(object model, ISpecializedChangeHandler binder, SenderEventRelay relay) {
        Debug.Assert(ApplicationPFX.Instance.Dispatcher.CheckAccess());

        HybridDictionary eventToHandlerList = ISpecializedChangeHandler.attachedInstanceMap[model];
        ISpecializedChangeHandler[] array = (ISpecializedChangeHandler[]) eventToHandlerList[relay.EventName]!;
        Debug.Assert(array != null);

        int idx = ArrayUtils.IndexOf_RefType(array, binder);
        Debug.Assert(idx != -1);

        if (array.Length == 1) {
            relay.RemoveEventHandler(model); // no more binders listen to event, so remove handler
            eventToHandlerList.Remove(relay.EventName); // remove the list to open possibility to remove model from attachedInstanceMap
            if (eventToHandlerList.Count == 0) {
                // prevent memory leak. the model should always be removed as soon as all binders using it are detached.
                bool removed = ISpecializedChangeHandler.attachedInstanceMap.TryRemove(model, out HybridDictionary? removedInnerMap);
                Debug.Assert(removed && removedInnerMap == eventToHandlerList);
            }
        }
        else {
            eventToHandlerList[relay.EventName] = ArrayUtils.RemoveAt(array, idx);
        }
    }

    // Note: this can be invoked from any thread
    private static void OnModelEventFired(object _sender, object _eventName) {
        // assert _eventName is string;
        if (ISpecializedChangeHandler.attachedInstanceMap.TryGetValue(_sender, out HybridDictionary? dictionary)) {
            ISpecializedChangeHandler[]? handlers = (ISpecializedChangeHandler[]?) dictionary[_eventName];
            if (handlers != null) {
                Debug.Assert(handlers.Length > 0);
                foreach (ISpecializedChangeHandler handler in handlers) {
                    handler.OnContextValueEventRaised();
                }
            }
        }
    }
}