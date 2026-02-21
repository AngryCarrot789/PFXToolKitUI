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

using System.Diagnostics;
using PFXToolKitUI.EventHelpers;
using PFXToolKitUI.Interactivity.Contexts;
using PFXToolKitUI.Utils.Events;
using PFXToolKitUI.Utils.RDA;

namespace PFXToolKitUI.AdvancedMenuService;

public static class ContextEntryExtensions {
    public readonly struct UpdateEventArgs<T>(T value) {
        /// <summary>Gets the current value</summary>
        public T Value { get; } = value;
    }

    public readonly struct UpdateEventArgs<T1, T2>(T1 value1, T2 value2) {
        /// <summary>Gets the current value of type 1</summary>
        public T1 Value1 { get; } = value1;

        /// <summary>Gets the current value of type 2</summary>
        public T2 Value2 { get; } = value2;
    }

    public readonly struct UpdateEventArgs<T1, T2, T3>(T1 value1, T2 value2, T3 value3) {
        /// <summary>Gets the current value of type 1</summary>
        public T1 Value1 { get; } = value1;

        /// <summary>Gets the current value of type 2</summary>
        public T2 Value2 { get; } = value2;

        /// <summary>Gets the current value of type 3</summary>
        public T3 Value3 { get; } = value3;
    }

    /// <param name="this">The menu entry to add the change handler to</param>
    extension(BaseMenuEntry @this) {
        /// <summary>
        /// Adds a handler to <see cref="BaseMenuEntry.CapturedContextChanged"/> that fires the update callback with the value keyed by the data key, or null if there is no value by that key.
        /// </summary>
        /// <param name="key">The data key used to get the value of <see cref="T"/></param>
        /// <param name="update">Invoked when the context changes</param>
        /// <typeparam name="T">The type of value to pass to the context change callback</typeparam>
        /// <returns>The entry, for chained calling</returns>
        public BaseMenuEntry AddContextUpdateHandler<T>(DataKey<T> key, EventHandler<BaseMenuEntry, UpdateEventArgs<T?>> update) where T : class {
            @this.CapturedContextChanged += (sender, e) => update((BaseMenuEntry) sender!, new UpdateEventArgs<T?>(e.NewValue != null && key.TryGetContext(e.NewValue, out T? value) ? value : null));
            return @this;
        }

        /// <summary>
        /// Same as <see cref="AddContextUpdateHandler{T}"/> except supports two values.
        /// The callback is either called with all non-null parameters or all null.
        /// </summary>
        /// <returns>The entry</returns>
        public BaseMenuEntry AddContextUpdateHandler<T1, T2>(DataKey<T1> key1, DataKey<T2> key2, EventHandler<BaseMenuEntry, UpdateEventArgs<T1?, T2?>> update) where T1 : class where T2 : class {
            @this.CapturedContextChanged += (sender, e) => {
                if (e.NewValue != null && key1.TryGetContext(e.NewValue, out T1? val1) && key2.TryGetContext(e.NewValue, out T2? val2))
                    update((BaseMenuEntry) sender!, new UpdateEventArgs<T1?, T2?>(val1, val2));
                else
                    update((BaseMenuEntry) sender!, new UpdateEventArgs<T1?, T2?>(null, null));
            };
            
            return @this;
        }

        /// <summary>
        /// Same as <see cref="AddContextUpdateHandler{T}"/> except supports three values.
        /// The callback is either called with all non-null parameters or all null.
        /// </summary>
        /// <returns>The entry</returns>
        public BaseMenuEntry AddContextUpdateHandler<T1, T2, T3>(DataKey<T1> key1, DataKey<T2> key2, DataKey<T3> key3, EventHandler<BaseMenuEntry, UpdateEventArgs<T1?, T2?, T3?>> update) where T1 : class where T2 : class where T3 : class {
            @this.CapturedContextChanged += (sender, e) => {
                if (e.NewValue != null && key1.TryGetContext(e.NewValue, out T1? val1) && key2.TryGetContext(e.NewValue, out T2? val2) && key3.TryGetContext(e.NewValue, out T3? val3))
                    update((BaseMenuEntry) sender!, new UpdateEventArgs<T1?, T2?, T3?>(val1, val2, val3));
                else
                    update((BaseMenuEntry) sender!, new UpdateEventArgs<T1?, T2?, T3?>(null, null, null));
            };
            
            return @this;
        }

        /// <summary>
        /// Adds a handler to <see cref="BaseMenuEntry.CapturedContextChanged"/> that fires the update callback with previous and new value keyed by the data key.
        /// </summary>
        /// <param name="key">The data key used to get the value of <see cref="T"/></param>
        /// <param name="onDataChanged">The callback invoked with the old and new value, tracked with a field inside a lambda class</param>
        /// <typeparam name="T">The type of value to pass to the context change callback</typeparam>
        /// <returns>The entry, for chained calling</returns>
        public BaseMenuEntry AddContextChangedHandler<T>(DataKey<T> key, EventHandler<BaseMenuEntry, ValueChangedEventArgs<T?>> onDataChanged) where T : class {
            T? field_CurrentValue = null;
            @this.CapturedContextChanged += (sender, e) => {
                if (e.NewValue == null || !key.TryGetContext(e.NewValue, out T? newValue)) {
                    if (field_CurrentValue != null) {
                        onDataChanged((BaseMenuEntry) sender!, new ValueChangedEventArgs<T?>(field_CurrentValue, null));
                        field_CurrentValue = null;
                    }
                }
                else if (!Equals(field_CurrentValue, newValue)) {
                    onDataChanged((BaseMenuEntry) sender!, new ValueChangedEventArgs<T?>(field_CurrentValue, newValue));
                    field_CurrentValue = newValue;
                }
            };
            
            return @this;
        }

        public BaseMenuEntry AddContextUpdateHandlerWithEvent<T>(DataKey<T> key, string eventName, EventHandler<BaseMenuEntry, UpdateEventArgs<T?>> onUpdate) where T : class {
            @this.CapturedContextChanged += new SpecializedChangeHandler_Callback<T, T>(@this, key, [eventName], x => x, onUpdate).OnCapturedContextChanged;
            return @this;
        }

        public BaseMenuEntry AddContextUpdateHandlerWithEvents<T>(DataKey<T> key, string[] eventNames, EventHandler<BaseMenuEntry, UpdateEventArgs<T?>> onUpdate) where T : class {
            @this.CapturedContextChanged += new SpecializedChangeHandler_Callback<T, T>(@this, key, eventNames, x => x, onUpdate).OnCapturedContextChanged;
            return @this;
        }

        public BaseMenuEntry AddContextUpdateHandlerWithEventsEx<TKey, T>(DataKey<TKey> key, Func<TKey, T> keyValueToDstValue, string eventNames, EventHandler<BaseMenuEntry, UpdateEventArgs<T?>> onUpdate) where TKey : class where T : class {
            @this.CapturedContextChanged += new SpecializedChangeHandler_Callback<TKey, T>(@this, key, [eventNames], keyValueToDstValue, onUpdate).OnCapturedContextChanged;
            return @this;
        }

        public BaseMenuEntry AddContextUpdateHandlerWithEventsEx<TKey, T>(DataKey<TKey> key, Func<TKey, T> keyValueToDstValue, string[] eventNames, EventHandler<BaseMenuEntry, UpdateEventArgs<T?>> onUpdate) where TKey : class where T : class {
            @this.CapturedContextChanged += new SpecializedChangeHandler_Callback<TKey, T>(@this, key, eventNames, keyValueToDstValue, onUpdate).OnCapturedContextChanged;
            return @this;
        }

        public BaseMenuEntry AddCanExecuteChangeUpdaterForEvent<T>(DataKey<T> key, string eventName) where T : class {
            @this.CapturedContextChanged += new SpecializedChangeHandler_Callback<T, T>(@this, key, [eventName], x => x, static (e, t) => e.RaiseCanExecuteChanged()).OnCapturedContextChanged;
            return @this;
        }

        public BaseMenuEntry AddCanExecuteChangeUpdaterForEvents<T>(DataKey<T> key, string[] eventNames) where T : class {
            @this.CapturedContextChanged += new SpecializedChangeHandler_Callback<T, T>(@this, key, eventNames, x => x, static (e, t) => e.RaiseCanExecuteChanged()).OnCapturedContextChanged;
            return @this;
        }

        public BaseMenuEntry AddCanExecuteChangeUpdaterForEventsEx<TKey, T>(DataKey<TKey> key, Func<TKey, T> keyValueToDstValue, string eventNames) where TKey : class where T : class {
            @this.CapturedContextChanged += new SpecializedChangeHandler_Callback<TKey, T>(@this, key, [eventNames], keyValueToDstValue, static (e, t) => e.RaiseCanExecuteChanged()).OnCapturedContextChanged;
            return @this;
        }

        public BaseMenuEntry AddCanExecuteChangeUpdaterForEventsEx<TKey, T>(DataKey<TKey> key, Func<TKey, T> keyValueToDstValue, string[] eventNames) where TKey : class where T : class {
            @this.CapturedContextChanged += new SpecializedChangeHandler_Callback<TKey, T>(@this, key, eventNames, keyValueToDstValue, static (e, t) => e.RaiseCanExecuteChanged()).OnCapturedContextChanged;
            return @this;
        }

        public BaseMenuEntry AddIsCheckedChangeUpdaterForEvent<T>(DataKey<T> key, string eventName) where T : class {
            @this.CapturedContextChanged += new SpecializedChangeHandler_Callback<T, T>(@this, key, [eventName], x => x, static (e, t) => e.RaiseIsCheckedChanged()).OnCapturedContextChanged;
            return @this;
        }

        public BaseMenuEntry AddIsCheckedChangeUpdaterForEvents<T>(DataKey<T> key, string[] eventNames) where T : class {
            @this.CapturedContextChanged += new SpecializedChangeHandler_Callback<T, T>(@this, key, eventNames, x => x, static (e, t) => e.RaiseIsCheckedChanged()).OnCapturedContextChanged;
            return @this;
        }

        public BaseMenuEntry AddIsCheckedChangeUpdaterForEventsEx<TKey, T>(DataKey<TKey> key, Func<TKey, T> keyValueToDstValue, string eventNames) where TKey : class where T : class {
            @this.CapturedContextChanged += new SpecializedChangeHandler_Callback<TKey, T>(@this, key, [eventNames], keyValueToDstValue, static (e, t) => e.RaiseIsCheckedChanged()).OnCapturedContextChanged;
            return @this;
        }

        public BaseMenuEntry AddIsCheckedChangeUpdaterForEventsEx<TKey, T>(DataKey<TKey> key, Func<TKey, T> keyValueToDstValue, string[] eventNames) where TKey : class where T : class {
            @this.CapturedContextChanged += new SpecializedChangeHandler_Callback<TKey, T>(@this, key, eventNames, keyValueToDstValue, static (e, t) => e.RaiseIsCheckedChanged()).OnCapturedContextChanged;
            return @this;
        }
    }

    private abstract class BaseSpecializedChangeHandler<TKey, T> : IRelayEventHandler where TKey : class where T : class {
        private readonly DataKey<TKey> key;
        private readonly string[] eventNames;
        private readonly Func<TKey, T> keyValueToDstValue;
        private EventWrapper[]? relays;
        private RapidDispatchActionEx? rda;

        public T? CurrentValue { get; private set; }

        public BaseMenuEntry Entry { get; }
        
        protected BaseSpecializedChangeHandler(BaseMenuEntry entry, DataKey<TKey> key, string[] eventNames, Func<TKey, T> keyValueToDstValue) {
            this.Entry = entry;
            this.key = key;
            this.eventNames = eventNames;
            this.keyValueToDstValue = keyValueToDstValue;
        }

        internal void OnCapturedContextChanged(object? o, ValueChangedEventArgs<IContextData?> e) {
            Debug.Assert(this.Entry == o);
            T newValue;
            if (e.NewValue == null || !this.key.TryGetContext(e.NewValue, out TKey? newSrcValue)) {
                if (this.CurrentValue != null) {
                    foreach (EventWrapper relay in this.relays!)
                        EventRelayStorage.UIStorage.RemoveHandler(this.CurrentValue, this, relay);
                    this.CurrentValue = null;
                    this.Update();
                }
            }
            else if (!Equals(this.CurrentValue, newValue = this.keyValueToDstValue(newSrcValue))) {
                if (this.relays != null && this.CurrentValue != null) {
                    foreach (EventWrapper relay in this.relays!)
                        EventRelayStorage.UIStorage.RemoveHandler(this.CurrentValue, this, relay);
                }

                if (this.relays == null) {
                    this.relays = new EventWrapper[this.eventNames.Length];
                    for (int i = 0; i < this.eventNames.Length; i++) {
                        this.relays[i] = EventRelayStorage.UIStorage.GetEventRelay(typeof(T), this.eventNames[i]);
                    }
                }

                this.CurrentValue = newValue;
                this.Update();
                foreach (EventWrapper relay in this.relays!)
                    EventRelayStorage.UIStorage.AddHandler(newValue, this, relay);
            }
        }

        public void OnEvent(object sender) {
            if (ApplicationPFX.Instance.Dispatcher.CheckAccess()) {
                this.Update();
            }
            else {
                if (this.rda == null)
                    Interlocked.Exchange(ref this.rda, RapidDispatchActionEx.ForSync(this.Update, "SpecializedChangeHandler"));

                this.rda.InvokeAsync();
            }
        }

        protected abstract void Update();
    }

    private class SpecializedChangeHandler_Callback<TKey, T> : BaseSpecializedChangeHandler<TKey, T> where TKey : class where T : class {
        private readonly EventHandler<BaseMenuEntry, UpdateEventArgs<T?>> onUpdate;

        public SpecializedChangeHandler_Callback(BaseMenuEntry entry, DataKey<TKey> key, string[] eventNames, Func<TKey, T> keyValueToDstValue, EventHandler<BaseMenuEntry, UpdateEventArgs<T?>> onUpdate) : base(entry, key, eventNames, keyValueToDstValue) {
            this.onUpdate = onUpdate;
        }

        protected override void Update() => this.onUpdate(this.Entry, new UpdateEventArgs<T?>(this.CurrentValue));
    }
}