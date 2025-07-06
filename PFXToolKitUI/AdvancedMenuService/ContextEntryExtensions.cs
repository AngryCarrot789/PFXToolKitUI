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

using PFXToolKitUI.Interactivity.Contexts;

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
                if (field_CurrentValue != null)
                    onDataChanged(sender, field_CurrentValue, null);
            }
            else if (!Equals(field_CurrentValue, newValue)) {
                onDataChanged(sender, field_CurrentValue, newValue);
            }
        };
        return entry;
    }
}