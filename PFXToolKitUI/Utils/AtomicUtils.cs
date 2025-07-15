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

namespace PFXToolKitUI.Utils;

/// <summary>
/// A helper class for performing atomic operations
/// </summary>
public static class AtomicUtils {
    /// <summary>
    /// Creates a shallow copy of the list of the source collection while the collection itself is under lock
    /// </summary>
    /// <param name="lockObject">The lock to obtain before processing the items</param>
    /// <param name="collection">The item source</param>
    /// <typeparam name="T">The type of item</typeparam>
    /// <returns>A new list containing all items in the source list after the lock is obtained</returns>
    public static List<T> CopyAndClear<T>(ICollection<T> collection) {
        lock (collection) {
            List<T> completions = collection.ToList();
            collection.Clear();
            return completions;
        }
    }

    public static LinkedListNode<T>? AddLastWhen<T>(LinkedList<T> linkedList, Func<bool> canAdd, Func<T> constructor) {
        if (canAdd())
            lock (linkedList)
                return canAdd() ? linkedList.AddLast(constructor()) : null;
        return null;
    }

    public static bool TryAddLastWhen<T>(LinkedList<T> linkedList, Func<bool> canAdd, Func<T> constructor, out LinkedListNode<T>? node) {
        return (node = AddLastWhen(linkedList, canAdd, constructor)) != null;
    }

    public static T? AddWhen<T>(ICollection<T> collection, Func<bool> canAdd, Func<T> constructor) where T : class {
        if (!canAdd())
            return null;
        lock (collection) {
            if (!canAdd())
                return null;

            T result = constructor();
            collection.Add(result);
            return result;
        }
    }

    public static bool TryAddWhen<T>(ICollection<T> collection, Func<bool> canAdd, Func<T> constructor, [MaybeNullWhen(false)] out T result) {
        if (!canAdd()) {
            result = default;
            return false;
        }

        lock (collection) {
            if (canAdd()) {
                collection.Add(result = constructor());
                return true;
            }
            else {
                result = default;
                return false;
            }
        }
    }

    public static bool TryAddWhen<T, TParam>(ICollection<T> collection, TParam predicateParameter, Predicate<TParam> canAdd, Func<T> constructor, [MaybeNullWhen(false)] out T result) {
        if (!canAdd(predicateParameter)) {
            result = default;
            return false;
        }

        lock (collection) {
            if (canAdd(predicateParameter)) {
                collection.Add(result = constructor());
                return true;
            }
            else {
                result = default;
                return false;
            }
        }
    }
}