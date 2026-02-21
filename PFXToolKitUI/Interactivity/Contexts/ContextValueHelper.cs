// 
// Copyright (c) 2026-2026 REghZy
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

using PFXToolKitUI.AdvancedMenuService;
using PFXToolKitUI.Utils.Events;

namespace PFXToolKitUI.Interactivity.Contexts;

public static class ContextValueHelper {
    public static void OnContextChangedHelper<T>(DataKey<T> dataKey, IContextData? newContext, ref T? field, object sender, EventHandler onValueChanged) {
        OnContextChangedHelperImpl(dataKey, newContext, ref field, static x => x, sender, onValueChanged, static (_sender, _state, e) => {
            ((EventHandler) _state!)(_sender, EventArgs.Empty);
        });
    }

    public static void OnContextChangedHelper<T>(DataKey<T> dataKey, IContextData? newContext, ref T? field, object sender, EventHandler<ValueChangedEventArgs<T?>> onValueChanged) {
        OnContextChangedHelperImpl(dataKey, newContext, ref field, static x => x, sender, onValueChanged, static (_sender, _state, e) => {
            ((EventHandler<ValueChangedEventArgs<T?>>) _state!)(_sender, e);
        });
    }

    public static void OnContextChangedHelper<TSender, T>(DataKey<T> dataKey, IContextData? newContext, ref T? field, TSender sender, EventHandler<TSender, ValueChangedEventArgs<T?>> onValueChanged) {
        OnContextChangedHelperImpl(dataKey, newContext, ref field, static x => x, sender, onValueChanged, static (_sender, _state, e) => {
            ((EventHandler<TSender, ValueChangedEventArgs<T?>>) _state!)(_sender, e);
        });
    }
    
    public static void OnContextChangedHelperEx<TKey, TValue>(DataKey<TKey> dataKey, IContextData? newContext, ref TValue? field, Func<TKey, TValue> keyValueToDstValue, object sender, EventHandler onValueChanged) {
        OnContextChangedHelperImpl(dataKey, newContext, ref field, keyValueToDstValue, sender, onValueChanged, static (_sender, _state, e) => {
            ((EventHandler) _state!)(_sender, EventArgs.Empty);
        });
    }

    public static void OnContextChangedHelperEx<TKey, TValue>(DataKey<TKey> dataKey, IContextData? newContext, ref TValue? field, Func<TKey, TValue> keyValueToDstValue, object sender, EventHandler<ValueChangedEventArgs<TValue?>> onValueChanged) {
        OnContextChangedHelperImpl(dataKey, newContext, ref field, keyValueToDstValue, sender, onValueChanged, static (_sender, _state, e) => {
            ((EventHandler<ValueChangedEventArgs<TValue?>>) _state!)(_sender, e);
        });
    }

    public static void OnContextChangedHelperEx<TSender, TKey, TValue>(DataKey<TKey> dataKey, IContextData? newContext, ref TValue? field, Func<TKey, TValue> keyValueToDstValue, TSender sender, EventHandler<TSender, ValueChangedEventArgs<TValue?>> onValueChanged) {
        OnContextChangedHelperImpl(dataKey, newContext, ref field, keyValueToDstValue, sender, onValueChanged, static (_sender, _state, e) => {
            ((EventHandler<TSender, ValueChangedEventArgs<TValue?>>) _state!)(_sender, e);
        });
    }
    
    public static void OnContextChangedHelperImpl<TSender, TKey, TValue>(DataKey<TKey> dataKey, IContextData? newContext, ref TValue? field, Func<TKey, TValue> keyValueToDstValue, TSender sender, object? state, Action<TSender, object?, ValueChangedEventArgs<TValue?>> onValueChanged) {
        TValue? newValue;
        if (newContext != null && dataKey.TryGetContext(newContext, out TKey? newKeyValue)) {
            if (!EqualityComparer<TValue>.Default.Equals(field, newValue = keyValueToDstValue(newKeyValue))) {
                onValueChanged(sender, state, new ValueChangedEventArgs<TValue?>(field, newValue));
                field = newValue;
            }
        }
        else if (EqualityComparer<TValue>.Default.Equals(field, default)) {
            onValueChanged(sender, state, new ValueChangedEventArgs<TValue?>(field, default));
            field = default;
        }
    }

    public static void OnContextChangedHelper<T>(this BaseMenuEntry @this, DataKey<T> dataKey, ref T? field, EventHandler onValueChanged) {
        OnContextChangedHelper(dataKey, @this.CapturedContext, ref field, @this, onValueChanged);
    }

    public static void OnContextChangedHelper<T>(this BaseMenuEntry @this, DataKey<T> dataKey, ref T? field, EventHandler<ValueChangedEventArgs<T?>> onValueChanged) {
        OnContextChangedHelper(dataKey, @this.CapturedContext, ref field, @this, onValueChanged);
    }
    
    public static void OnContextChangedHelperEx<TKey, TValue>(this BaseMenuEntry @this, DataKey<TKey> dataKey, ref TValue? field, Func<TKey, TValue> keyValueToDstValue, EventHandler onValueChanged) {
        OnContextChangedHelperEx(dataKey, @this.CapturedContext, ref field, keyValueToDstValue, @this, onValueChanged);
    }

    public static void OnContextChangedHelperEx<TKey, TValue>(this BaseMenuEntry @this, DataKey<TKey> dataKey, ref TValue? field, Func<TKey, TValue> keyValueToDstValue, EventHandler<ValueChangedEventArgs<TValue?>> onValueChanged) {
        OnContextChangedHelperEx(dataKey, @this.CapturedContext, ref field, keyValueToDstValue, @this, onValueChanged);
    }
    
    public static void OnContextChangedHelper<TSender, T>(this TSender @this, DataKey<T> dataKey, ref T? field, EventHandler onValueChanged) where TSender : BaseMenuEntry {
        OnContextChangedHelper(dataKey, @this.CapturedContext, ref field, @this, onValueChanged);
    }

    public static void OnContextChangedHelper<TSender, T>(this TSender @this, DataKey<T> dataKey, ref T? field, EventHandler<TSender, ValueChangedEventArgs<T?>> onValueChanged) where TSender : BaseMenuEntry {
        OnContextChangedHelper(dataKey, @this.CapturedContext, ref field, @this, onValueChanged);
    }
    
    public static void OnContextChangedHelperEx<TSender, TKey, TValue>(this TSender @this, DataKey<TKey> dataKey, ref TValue? field, Func<TKey, TValue> keyValueToDstValue, EventHandler onValueChanged) where TSender : BaseMenuEntry {
        OnContextChangedHelperEx(dataKey, @this.CapturedContext, ref field, keyValueToDstValue, @this, onValueChanged);
    }

    public static void OnContextChangedHelperEx<TSender, TKey, TValue>(this TSender @this, DataKey<TKey> dataKey, ref TValue? field, Func<TKey, TValue> keyValueToDstValue, EventHandler<TSender, ValueChangedEventArgs<TValue?>> onValueChanged) where TSender : BaseMenuEntry {
        OnContextChangedHelperEx(dataKey, @this.CapturedContext, ref field, keyValueToDstValue, @this, onValueChanged);
    }
}