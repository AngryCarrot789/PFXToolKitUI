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

namespace PFXToolKitUI.Utils;

public static class CollectionUtils {
    public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> consumer) {
        if (enumerable is List<T> list1) {
            list1.ForEach(consumer);
        }
        else if (enumerable is IList<T> list2) {
            for (int i = 0; i < list2.Count; i++) {
                consumer(list2[i]);
            }
        }
        else {
            foreach (T value in enumerable) {
                consumer(value);
            }
        }
    }

    public static void ForEach<T>(this IEnumerable enumerable, Action<T> consumer) {
        foreach (object value in enumerable) {
            if (value is T t) {
                consumer(t);
            }
        }
    }

    public static void ForEach(this IEnumerable enumerable, Action<object> consumer) {
        foreach (object value in enumerable) {
            consumer(value);
        }
    }

    public static void ForEach<T, TState>(this IEnumerable<T> enumerable, TState state, Action<T, TState> consumer) {
        foreach (T value in enumerable) {
            consumer(value, state);
        }
    }

    public static void ForEachThenClear<T>(this ICollection<T> list, Action<T> consumer) {
        using (ErrorList stack = new ErrorList("An exception occurred while enumerating one or more items before clearing the collection")) {
            int i = 0;
            foreach (T item in list) {
                try {
                    consumer(item);
                }
                catch (Exception e) {
                    stack.Add(new Exception($"Failed to dispose Item[{i}]", e));
                }

                i++;
            }

            list.Clear();
        }
    }

    public static void ClearAndAdd<T>(this ICollection<T> list, T value) {
        list.Clear();
        list.Add(value);
    }

    public static bool TryAdd<T>(this IList<T> list, T value) {
        if (list.Contains(value))
            return false;

        list.Add(value);
        return true;
    }

    public static void AddCollectionRange<T>(this ICollection<T> list, IEnumerable<T> values) {
        foreach (T t in values) {
            list.Add(t);
        }
    }

    public static void EnsureLength<T>(T[] array, int count) {
        if (array == null || array.Length != count) {
            throw new Exception("Expected an array of size " + count + ". Got: " + (array != null ? array.Length.ToString() : "null"));
        }
    }

    public static void MoveItemInArray<T>(T[] array, int oldIndex, int newIndex) {
        if (newIndex < 0 || newIndex >= array.Length)
            throw new IndexOutOfRangeException($"{nameof(newIndex)} is not within range: {(newIndex < 0 ? "less than zero" : "greater than list length")} ({newIndex})");
        List<T> list = array.ToList();
        T removedItem = list[oldIndex];
        list.RemoveAt(oldIndex);
        list.Insert(newIndex, removedItem);
        for (int i = 0; i < list.Count; i++) {
            array[i] = list[i];
        }
    }

    public static void MoveItem<T>(this IList<T> list, int oldIndex, int newIndex) {
        if (newIndex < 0 || newIndex >= list.Count)
            throw new IndexOutOfRangeException($"{nameof(newIndex)} is not within range: {(newIndex < 0 ? "less than zero" : "greater than list length")} ({newIndex})");
        T removedItem = list[oldIndex];
        list.RemoveAt(oldIndex);
        list.Insert(newIndex, removedItem);
    }

    public static void MoveItem(IList list, int oldIndex, int newIndex) {
        object? removedItem = list[oldIndex];
        list.RemoveAt(oldIndex);
        list.Insert(newIndex, removedItem);
    }

    public static int IndexOf<T>(this IReadOnlyList<T> list, T value) {
        EqualityComparer<T> comparer = EqualityComparer<T>.Default;
        for (int i = 0; i < list.Count; i++) {
            if (comparer.Equals(value, list[i])) {
                return i;
            }
        }

        return -1;
    }

    public static bool Contains<T>(this IReadOnlyList<T> list, T value) {
        return IndexOf(list, value) != -1;
    }

    public static SingletonReadOnlyList<T> Singleton<T>(in T value) {
        return new SingletonReadOnlyList<T>(value);
    }

    public static List<T> SingleItem<T>(in T value) {
        return new List<T>() { value };
    }

    public static int CountAll<T>(this IEnumerable<T> source, Func<T, int> func) {
        int count = 0;
        foreach (T value in source)
            count += func(value);
        return count;
    }

    public static bool HasAtleast<T>(this IEnumerable<T> source, int count) {
        int i = 0;
        using (IEnumerator<T> enumerator = source.GetEnumerator()) {
            while (enumerator.MoveNext()) {
                if (++i >= count) {
                    return true;
                }
            }
        }

        return false;
    }

    public static int GetSortInsertionIndex<T>(IList<T> list, T item, Comparison<T> comparer) {
        int left = 0;
        int right = list.Count - 1;
        while (left <= right) {
            int middle = left + (right - left) / 2;
            int comparison = comparer(item, list[middle]);
            if (comparison < 0)
                right = middle - 1;
            else if (comparison > 0)
                left = middle + 1;
            else
                return middle;
        }

        return left;
    }

    public static int GetSortInsertionIndex<T>(T[] array, int left, int right, T item, Comparison<T> comparer) {
        while (left <= right) {
            int middle = left + (right - left) / 2;
            int comparison = comparer(item, array[middle]);
            if (comparison < 0)
                right = middle - 1;
            else if (comparison > 0)
                left = middle + 1;
            else
                return middle;
        }

        return left;
    }

    public static T[] AddToArray<T>(this T[] array, T value, int additionalLength = 1) {
        if (additionalLength < 1) {
            throw new ArgumentOutOfRangeException(nameof(additionalLength), "Value must be greater than 0");
        }

        int oldLen = array.Length;
        int newLen = oldLen + additionalLength;
        if (additionalLength == 1)
            array = array.CloneArrayMax(newLen);
        array[oldLen + 1] = value;
        return array;
    }

    public static void InsertSorted<T>(this IList<T> list, T value, Comparison<T> comparison) {
        int index = GetSortInsertionIndex(list, value, comparison);
        list.Insert(index, value);
    }

    /// <summary>
    /// Gets either the next index or the previous index, ensuring it does not equal the given index, or returns -1 if the list is empty or has only 1 element
    /// </summary>
    /// <param name="list"></param>
    /// <param name="index"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static int GetNeighbourIndex<T>(IReadOnlyList<T> list, int index) {
        int count = list.Count;
        if (index < 0 || index >= count)
            throw new IndexOutOfRangeException("Index was not within the bounds of the list");

        // ABCD

        if (count <= 1)
            return -1;

        return index == (count - 1) ? (index - 1) : (index + 1);
    }

    /// <summary>
    /// Adds all items in the given enumerable, and returns a list of items that actually added
    /// </summary>
    /// <param name="set">Target set</param>
    /// <param name="itemsToAdd">Items to add</param>
    /// <typeparam name="T">Type of element</typeparam>
    /// <returns>The items that were actually added</returns>
    public static List<T> UnionAddEx<T>(this ISet<T> set, IEnumerable<T> itemsToAdd) {
        List<T> addedItems = new List<T>();
        foreach (T item in itemsToAdd) {
            if (set.Add(item)) {
                addedItems.Add(item);
            }
        }

        return addedItems;
    }

    /// <summary>
    /// Removes all items in the given enumerable, and returns a list of items that actually removes
    /// </summary>
    /// <param name="set">Target set</param>
    /// <param name="itemsToRemove">Items to remove</param>
    /// <typeparam name="T">Type of element</typeparam>
    /// <returns>The items that were actually removed</returns>
    public static List<T> UnionRemoveEx<T>(this ISet<T> set, IEnumerable<T> itemsToRemove) {
        List<T> removedItems = new List<T>();
        foreach (T item in itemsToRemove) {
            if (set.Remove(item)) {
                removedItems.Add(item);
            }
        }

        return removedItems;
    }

    /// <summary>
    /// Attempts to extract a value which is equal across all objects, using the given getter function
    /// </summary>
    /// <param name="objects">Input objects</param>
    /// <param name="getter">Getter function</param>
    /// <param name="equal">
    /// The value that is equal across all objects (will be set to <see cref="objects"/>[0]'s value)
    /// </param>
    /// <typeparam name="T">Type of object to get</typeparam>
    /// <returns>True if there is 1 object, or more than 1 and they have the same value, otherwise false</returns>
    public static bool GetEqualValue<T>(IReadOnlyList<object>? objects, Func<object, T> getter, out T? equal) {
        int count;
        if (objects == null || (count = objects.Count) < 1) {
            equal = default;
            return false;
        }

        equal = getter(objects[0]);
        if (count == 1) {
            return true;
        }

        EqualityComparer<T> comparator = EqualityComparer<T>.Default;
        for (int i = 1; i < count; i++) {
            if (!comparator.Equals(getter(objects[i]), equal)) {
                equal = default;
                return false;
            }
        }

        return true;
    }

    public static bool GetEqualValue<TSrc, TValue>(IReadOnlyList<TSrc> items, Func<TSrc, TValue> func, out TValue? equal) {
        int count = items.Count;
        if (count < 1) {
            equal = default;
            return false;
        }

        equal = func(items[0]);
        if (count == 1) {
            return true;
        }

        EqualityComparer<TValue> comparator = EqualityComparer<TValue>.Default;
        for (int i = 1; i < count; i++) {
            if (!comparator.Equals(func(items[i]), equal)) {
                equal = default;
                return false;
            }
        }

        return true;
    }

    public static bool GetEqualValue<TSrc, TValue>(IEnumerable<TSrc> items, Func<TSrc, TValue> func, out TValue? equal) {
        using IEnumerator<TSrc> enumerator = items.GetEnumerator();
        if (!enumerator.MoveNext()) {
            equal = default;
            return false;
        }

        equal = func(enumerator.Current);
        if (!enumerator.MoveNext()) {
            return true;
        }

        EqualityComparer<TValue> comparator = EqualityComparer<TValue>.Default;
        do {
            if (!comparator.Equals(func(enumerator.Current), equal)) {
                equal = default;
                return false;
            }
        } while (enumerator.MoveNext());

        return true;
    }

    public static void UpdateOrInitialiseValue<TKey, TValue>(this SortedList<TKey, TValue> list, TKey key, Func<TValue, TValue> updater, Func<TValue> defaultProvider) where TKey : notnull {
        int index = list.IndexOfKey(key);
        if (index == -1) {
            list[key] = defaultProvider();
        }
        else {
            list.SetValueAtIndex(index, updater(list.GetValueAtIndex(index)));
        }
    }

    public static void UpdateOrInitialiseValue<TKey, TValue>(this SortedList<TKey, TValue> list, TKey key, Func<TValue, TValue> updater, TValue defaultValue) where TKey : notnull {
        int index = list.IndexOfKey(key);
        if (index == -1) {
            list[key] = defaultValue;
        }
        else {
            list.SetValueAtIndex(index, updater(list.GetValueAtIndex(index)));
        }
    }

    public static List<(int, T)> CreateIndexMap<T>(IList<T> sourceList, IEnumerable<T> items) {
        SortedList<int, T> list = new SortedList<int, T>();
        foreach (T item in items) {
            int index = sourceList.IndexOf(item);
            if (index != -1) {
                list.Add(index, item);
            }
        }

        return list.Select(x => (x.Key, x.Value)).ToList();
    }
}