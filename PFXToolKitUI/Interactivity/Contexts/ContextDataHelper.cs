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

using System.Diagnostics.CodeAnalysis;

namespace PFXToolKitUI.Interactivity.Contexts;

public static class ContextDataHelper {
    public static bool ContainsKey(this IContextData data, DataKey key) {
        ArgumentNullException.ThrowIfNull(key, nameof(key));
        return data.ContainsKey(key.Id);
    }

    public static bool ContainsAll(this IContextData data, params DataKey[] keys) {
        foreach (DataKey key in keys)
            if (!data.ContainsKey(key))
                return false;
        return true;
    }

    public static bool ContainsAll(this IContextData data, IEnumerable<DataKey> keys) {
        foreach (DataKey key in keys)
            if (!data.ContainsKey(key))
                return false;
        return true;
    }

    public static bool ContainsAll(this IContextData data, DataKey keyA, DataKey keyB) {
        return data.ContainsKey(keyA) && data.ContainsKey(keyB);
    }

    public static bool ContainsAll(this IContextData data, DataKey keyA, DataKey keyB, DataKey keyC) {
        return data.ContainsKey(keyA) && data.ContainsKey(keyB) && data.ContainsKey(keyC);
    }

    public static bool ContainsAll(this IContextData data, params string[] keys) {
        foreach (string key in keys)
            if (!data.ContainsKey(key))
                return false;
        return true;
    }

    public static bool ContainsAll(this IContextData data, IEnumerable<string> keys) {
        foreach (string key in keys)
            if (!data.ContainsKey(key))
                return false;
        return true;
    }

    public static bool ContainsAll(this IContextData data, string keyA, string keyB) {
        return data.ContainsKey(keyA) && data.ContainsKey(keyB);
    }

    public static bool ContainsAll(this IContextData data, string keyA, string keyB, string keyC) {
        return data.ContainsKey(keyA) && data.ContainsKey(keyB) && data.ContainsKey(keyC);
    }


    public static bool ContainsAny(this IContextData data, DataKey keyA, DataKey keyB) {
        return data.ContainsKey(keyA) || data.ContainsKey(keyB);
    }

    public static bool ContainsAny(this IContextData data, DataKey keyA, DataKey keyB, DataKey keyC) {
        return data.ContainsKey(keyA) || data.ContainsKey(keyB) || data.ContainsKey(keyC);
    }

    public static bool ContainsAny(this IContextData data, params DataKey[] keys) {
        foreach (DataKey key in keys)
            if (data.ContainsKey(key))
                return true;
        return keys.Length == 0;
    }

    public static bool ContainsAny(this IContextData data, IEnumerable<DataKey> keys, bool resultWhenEmpty = true) {
        using (IEnumerator<DataKey> enumerator = keys.GetEnumerator()) {
            if (!enumerator.MoveNext())
                return resultWhenEmpty;
            do {
                if (data.ContainsKey(enumerator.Current))
                    return true;
            } while (enumerator.MoveNext());

            return false;
        }
    }

    public static bool ContainsAny(this IContextData data, string keyA, string keyB) {
        return data.ContainsKey(keyA) || data.ContainsKey(keyB);
    }

    public static bool ContainsAny(this IContextData data, string keyA, string keyB, string keyC) {
        return data.ContainsKey(keyA) || data.ContainsKey(keyB) || data.ContainsKey(keyC);
    }

    public static bool ContainsAny(this IContextData data, params string[] keys) {
        foreach (string key in keys)
            if (data.ContainsKey(key))
                return true;
        return keys.Length == 0;
    }

    public static bool ContainsAny(this IContextData data, IEnumerable<string> keys, bool resultWhenEmpty = true) {
        using IEnumerator<string> enumerator = keys.GetEnumerator();
        if (!enumerator.MoveNext())
            return resultWhenEmpty;
        do {
            if (data.ContainsKey(enumerator.Current))
                return true;
        } while (enumerator.MoveNext());

        return false;
    }

    public static bool TryGetAll<T1, T2>(this IContextData data, DataKey<T1> keyA, DataKey<T2> keyB, [NotNullWhen(true)] out T1? a, [NotNullWhen(true)] out T2? b) {
        if (keyA.TryGetContext(data, out a) && keyB.TryGetContext(data, out b))
            return true;
        a = default;
        b = default;
        return false;
    }

    public static bool TryGetAll<T1, T2, T3>(this IContextData data, DataKey<T1> keyA, DataKey<T2> keyB, DataKey<T3> keyC, [NotNullWhen(true)] out T1? a, [NotNullWhen(true)] out T2? b, [NotNullWhen(true)] out T3? c) {
        if (keyA.TryGetContext(data, out a) && keyB.TryGetContext(data, out b) && keyC.TryGetContext(data, out c))
            return true;
        a = default;
        b = default;
        c = default;
        return false;
    }
}