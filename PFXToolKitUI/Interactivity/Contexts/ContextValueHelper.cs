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
    private static void OnContextChangedImpl<TSender, TKey, TValue>(ref TValue? field, DataKey<TKey> dataKey, IContextData? newContext, Func<TKey, TValue> keyValueToDstValue, TSender sender, object? state, Action<TSender, object?, ValueChangedEventArgs<TValue?>> onValueChanged) {
        if (newContext != null && dataKey.TryGetContext(newContext, out TKey? newKeyValue)) {
            TValue newValue = keyValueToDstValue(newKeyValue);
            if (!EqualityComparer<TValue>.Default.Equals(field, newValue)) {
                onValueChanged(sender, state, new ValueChangedEventArgs<TValue?>(field, newValue));
                field = newValue;
            }
        }
        else if (EqualityComparer<TValue>.Default.Equals(field, default)) {
            onValueChanged(sender, state, new ValueChangedEventArgs<TValue?>(field, default));
            field = default;
        }
    }
    
    public static void SetAndRaiseINE<T>(ref T? field, DataKey<T> dataKey, IContextData? newContext, object sender, EventHandler onValueChanged) {
        OnContextChangedImpl(ref field, dataKey, newContext, static x => x, sender, onValueChanged, static (_sender, _state, e) => {
            ((EventHandler) _state!)(_sender, EventArgs.Empty);
        });
    }

    public static void SetAndRaiseINE<T>(ref T? field, DataKey<T> dataKey, IContextData? newContext, object sender, EventHandler<ValueChangedEventArgs<T?>> onValueChanged) {
        OnContextChangedImpl(ref field, dataKey, newContext, static x => x, sender, onValueChanged, static (_sender, _state, e) => {
            ((EventHandler<ValueChangedEventArgs<T?>>) _state!)(_sender, e);
        });
    }

    public static void SetAndRaiseINE<TSender, T>(ref T? field, DataKey<T> dataKey, IContextData? newContext, TSender sender, EventHandler<TSender, ValueChangedEventArgs<T?>> onValueChanged) {
        OnContextChangedImpl(ref field, dataKey, newContext, static x => x, sender, onValueChanged, static (_sender, _state, e) => {
            ((EventHandler<TSender, ValueChangedEventArgs<T?>>) _state!)(_sender, e);
        });
    }
    
    public static void SetAndRaiseINEEx<TKey, TValue>(ref TValue? field, DataKey<TKey> dataKey, IContextData? newContext, Func<TKey, TValue> keyValueToDstValue, object sender, EventHandler onValueChanged) {
        OnContextChangedImpl(ref field, dataKey, newContext, keyValueToDstValue, sender, onValueChanged, static (_sender, _state, e) => {
            ((EventHandler) _state!)(_sender, EventArgs.Empty);
        });
    }

    public static void SetAndRaiseINEEx<TKey, TValue>(ref TValue? field, DataKey<TKey> dataKey, IContextData? newContext, Func<TKey, TValue> keyValueToDstValue, object sender, EventHandler<ValueChangedEventArgs<TValue?>> onValueChanged) {
        OnContextChangedImpl(ref field, dataKey, newContext, keyValueToDstValue, sender, onValueChanged, static (_sender, _state, e) => {
            ((EventHandler<ValueChangedEventArgs<TValue?>>) _state!)(_sender, e);
        });
    }

    public static void SetAndRaiseINEEx<TSender, TKey, TValue>(ref TValue? field, DataKey<TKey> dataKey, IContextData? newContext, Func<TKey, TValue> keyValueToDstValue, TSender sender, EventHandler<TSender, ValueChangedEventArgs<TValue?>> onValueChanged) {
        OnContextChangedImpl(ref field, dataKey, newContext, keyValueToDstValue, sender, onValueChanged, static (_sender, _state, e) => {
            ((EventHandler<TSender, ValueChangedEventArgs<TValue?>>) _state!)(_sender, e);
        });
    }

    extension(BaseMenuEntry @this) {
        public void SetAndRaiseINE<T>(ref T? field, DataKey<T> dataKey, EventHandler onValueChanged) {
            SetAndRaiseINE(ref field, dataKey, @this.CapturedContext, @this, onValueChanged);
        }

        public void SetAndRaiseINE<T>(ref T? field, DataKey<T> dataKey, EventHandler<ValueChangedEventArgs<T?>> onValueChanged) {
            SetAndRaiseINE(ref field, dataKey, @this.CapturedContext, @this, onValueChanged);
        }

        public void SetAndRaiseINEEx<TKey, TValue>(ref TValue? field, DataKey<TKey> dataKey, Func<TKey, TValue> keyValueToDstValue, EventHandler onValueChanged) {
            SetAndRaiseINEEx(ref field, dataKey, @this.CapturedContext, keyValueToDstValue, @this, onValueChanged);
        }

        public void SetAndRaiseINEEx<TKey, TValue>(ref TValue? field, DataKey<TKey> dataKey, Func<TKey, TValue> keyValueToDstValue, EventHandler<ValueChangedEventArgs<TValue?>> onValueChanged) {
            SetAndRaiseINEEx(ref field, dataKey, @this.CapturedContext, keyValueToDstValue, @this, onValueChanged);
        }
    }

    extension<TSender>(TSender @this) where TSender : BaseMenuEntry {
        public void SetAndRaiseINE<T>(ref T? field, DataKey<T> dataKey, EventHandler<TSender, ValueChangedEventArgs<T?>> onValueChanged) {
            SetAndRaiseINE(ref field, dataKey, @this.CapturedContext, @this, onValueChanged);
        }

        public void SetAndRaiseINEEx<TKey, TValue>(ref TValue? field, DataKey<TKey> dataKey, Func<TKey, TValue> keyValueToDstValue, EventHandler<TSender, ValueChangedEventArgs<TValue?>> onValueChanged) {
            SetAndRaiseINEEx(ref field, dataKey, @this.CapturedContext, keyValueToDstValue, @this, onValueChanged);
        }
    }

    extension(IContextData? @this) {
        public void SetAndRaiseINE<TSender, T>(ref T? field, DataKey<T> dataKey, TSender sender, EventHandler onValueChanged) where TSender : class {
            SetAndRaiseINE(ref field, dataKey, @this, sender, onValueChanged);
        }

        public void SetAndRaiseINE<TSender, T>(ref T? field, DataKey<T> dataKey, TSender sender, EventHandler<TSender, ValueChangedEventArgs<T?>> onValueChanged) where TSender : class {
            SetAndRaiseINE(ref field, dataKey, @this, sender, onValueChanged);
        }

        public void SetAndRaiseINEEx<TSender, TKey, TValue>(ref TValue? field, DataKey<TKey> dataKey, Func<TKey, TValue> keyValueToDstValue, TSender sender, EventHandler onValueChanged) where TSender : class {
            SetAndRaiseINEEx(ref field, dataKey, @this, keyValueToDstValue, sender, onValueChanged);
        }

        public void SetAndRaiseINEEx<TSender, TKey, TValue>(ref TValue? field, DataKey<TKey> dataKey, Func<TKey, TValue> keyValueToDstValue, TSender sender, EventHandler<TSender, ValueChangedEventArgs<TValue?>> onValueChanged) where TSender : class {
            SetAndRaiseINEEx(ref field, dataKey, @this, keyValueToDstValue, sender, onValueChanged);
        }
    }
}