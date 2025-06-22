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

using PFXToolKitUI.Interactivity.Contexts;

namespace PFXToolKitUI.AdvancedMenuService;

public static class ContextEntryExtensions {
    public static void AddSimpleContextUpdate<T>(this BaseContextEntry entry, DataKey<T> key, Action<BaseContextEntry, T?> update) where T : class {
        entry.CapturedContextChanged += (sender, oldCtx, newCtx) => update(sender, newCtx != null && key.TryGetContext(newCtx, out T? value) ? value : null);
    }
    
    public static void AddSimpleContextUpdate<T1, T2>(this BaseContextEntry entry, DataKey<T1> key1, DataKey<T2> key2, Action<BaseContextEntry, T1?, T2?> update) where T1 : class where T2 : class {
        entry.CapturedContextChanged += (sender, oldCtx, newCtx) => {
            if (newCtx != null && key1.TryGetContext(newCtx, out T1? val1) && key2.TryGetContext(newCtx, out T2? val2))
                update(sender, val1, val2);
            else
                update(sender, null, null);
        };
    }
    
    public static void AddSimpleContextUpdate<T1, T2, T3>(this BaseContextEntry entry, DataKey<T1> key1, DataKey<T2> key2, DataKey<T3> key3, Action<BaseContextEntry, T1?, T2?, T3?> update) where T1 : class where T2 : class where T3 : class {
        entry.CapturedContextChanged += (sender, oldCtx, newCtx) => {
            if (newCtx != null && key1.TryGetContext(newCtx, out T1? val1) && key2.TryGetContext(newCtx, out T2? val2) && key3.TryGetContext(newCtx, out T3? val3))
                update(sender, val1, val2, val3);
            else
                update(sender, null, null, null);
        };
    }

    public static void AddContextChangeHandler<T>(this BaseContextEntry entry, DataKey<T> key, Action<BaseContextEntry, T?, T?> onDataChanged) where T : class {
        T? field_CurrentValue = null;
        entry.CapturedContextChanged += (sender, oldCtx, newCtx) => {
            if (newCtx == null || !key.TryGetContext(newCtx, out T? newValue)) {
                if (field_CurrentValue != null)
                    onDataChanged(sender, field_CurrentValue, null);
            }
            else if (!Equals(field_CurrentValue, newValue)) {
                onDataChanged(sender, field_CurrentValue, newValue);
            }
        };
    }
}