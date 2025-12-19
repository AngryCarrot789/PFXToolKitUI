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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using PFXToolKitUI.Utils.Ranges;

namespace PFXToolKitUI.Utils.Collections;

/// <summary>
/// An optimised observable collection. This class is not thread safe; manual synchronization required
/// </summary>
/// <typeparam name="T">Type of value to store</typeparam>
public class NewObservableList<T> : CollectionEx<T> {
    private readonly List<T> myItems;
    private SimpleMonitor? _monitor;
    protected int blockReentrancyCount;

    /*
        Actions:
            Add:
                OldIndex = -1,    OldItems = null,
                NewIndex = Valid, NewItems = Added Items
            Remove:
                OldIndex = Valid, OldItems = Removed Items
                NewIndex = -1,    NewItems = null,
            Replace:
                OldIndex = NewIndex = Valid,
                OldItems = Removed, NewItems = Added
            Move:
                OldItems = NewItems = Moved Item,
                OldIndex = Valid, NewIndex = Valid,
            Reset:
                OldIndex = -1, OldItems = null
                NewIndex = -1, NewItems = null,
     */

    /// <summary>
    /// Occurs when the collection is about to change, either by adding or removing an item.
    /// </summary>
    public event NotifyCollectionChangedEventHandler? CollectionChanging;

    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    public NewObservableList() : this(ReadOnlyCollection<T>.Empty) {
    }

    public NewObservableList(IEnumerable<T> collection) : this(collection.ToList()) {
    }

    private NewObservableList(List<T> list) : base(list) {
        this.myItems = (List<T>?) base.Items!;
    }

    private void ThrowIfIndexOutOfBounds(int index, int count) {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        int newIndex = index + count;
        if (newIndex < 0 || newIndex > this.myItems.Count) {
            throw new IndexOutOfRangeException("Integer overflow adding items");
        }
    }

    protected override void InsertItem(int index, T item) {
        this.CheckReentrancy();
        this.ThrowIfIndexOutOfBounds(index, 1);

        NotifyCollectionChangedEventArgs args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index);
        this.OnCollectionChanging(args);
        this.myItems.Insert(index, item);
        this.OnCollectionChanged(args);
    }

    public void InsertRange(int index, IEnumerable<T> items) {
        this.CheckReentrancy();
        if (!(items is IList<T> list)) {
            list = items.ToList();
        }

        this.ThrowIfIndexOutOfBounds(index, list.Count);

        NotifyCollectionChangedEventArgs args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, (IList) list, index);
        this.OnCollectionChanging(args);
        this.myItems.InsertRange(index, list);
        this.OnCollectionChanged(args);
    }

    public void AddRange(IEnumerable<T> items) => this.InsertRange(this.Count, items);

    protected override void RemoveItem(int index) {
        this.CheckReentrancy();
        this.ThrowIfIndexOutOfBounds(index, -1);
        T item = this[index];

        NotifyCollectionChangedEventArgs args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index);
        this.OnCollectionChanging(args);
        this.myItems.RemoveAt(index);
        this.OnCollectionChanged(args);
    }

    public void RemoveRange(int index, int count) {
        this.CheckReentrancy();
        this.ThrowIfIndexOutOfBounds(index, count);

        if (this.CollectionChanging == null && this.CollectionChanged == null) {
            this.myItems.RemoveRange(index, count);
        }
        else {
            List<T> items = this.myItems.Slice(index, count);
            NotifyCollectionChangedEventArgs args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, items, index);
            this.OnCollectionChanging(args);
            this.myItems.RemoveRange(index, count);
            this.OnCollectionChanged(args);
        }
    }

    public IntegerSet<int> RemoveRange(IEnumerable<T> items) {
        this.CheckReentrancy();
        if (!(items is IList<T> list)) {
            list = items.ToList();
        }

        // Calculate a union of ranges to remove
        IntegerSet<int> union = new IntegerSet<int>();
        foreach (T item in list) {
            int index = this.myItems.IndexOf(item);
            if (index != -1) {
                union.Add(index);
            }
        }

        if (union.Ranges.Count > 0) {
            // We could iterate front to back and increase start offset by length each time,
            // however it's more efficient to remove back to front
            List<IntegerRange<int>> indices = union.ToList();
            for (int i = indices.Count - 1; i >= 0; i--) {
                IntegerRange<int> range = indices[i];
                if (range.Length == 1) {
                    this.RemoveItem(range.Start);
                }
                else {
                    this.RemoveRange(range.Start, range.Length);
                }
            }
        }

        return union;
    }

    protected override void SetItem(int index, T newItem) {
        this.CheckReentrancy();
        T oldItem = this[index];

        NotifyCollectionChangedEventArgs args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItem, oldItem, index);
        this.OnCollectionChanging(args);
        this.myItems[index] = newItem;
        this.OnCollectionChanged(args);
    }

    public void Move(int oldIndex, int newIndex) => this.MoveItem(oldIndex, newIndex);

    protected virtual void MoveItem(int oldIndex, int newIndex) {
        this.CheckReentrancy();
        if (newIndex < 0)
            throw new IndexOutOfRangeException("Negative newIndex: " + newIndex);
        if ((uint) newIndex >= (uint) this.myItems.Count)
            throw new IndexOutOfRangeException($"newIndex beyond length of this list: {newIndex} >= {this.myItems.Count}");

        T item = this[oldIndex];

        NotifyCollectionChangedEventArgs args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, item, newIndex, oldIndex);
        this.OnCollectionChanging(args);
        this.myItems.RemoveAt(oldIndex);
        this.myItems.Insert(newIndex, item);
        this.OnCollectionChanged(args);
    }

    protected override void ClearItems() {
        this.CheckReentrancy();
        if (this.myItems.Count > 0) {
            this.OnCollectionChanging(ObservableListUtils.ResetArgs);
            this.myItems.Clear();
            this.OnCollectionChanged(ObservableListUtils.ResetArgs);
        }
    }

    private void OnCollectionChanging(NotifyCollectionChangedEventArgs args) {
        try {
            this.blockReentrancyCount++;
            this.CollectionChanging?.Invoke(this, args);
        }
        finally {
            this.blockReentrancyCount--;
        }
    }

    private void OnCollectionChanged(NotifyCollectionChangedEventArgs args) {
        try {
            this.blockReentrancyCount++;
            this.CollectionChanged?.Invoke(this, args);
        }
        finally {
            this.blockReentrancyCount--;
        }
    }

    public IDisposable BlockReentrancy() {
        this.blockReentrancyCount++;
        return this.EnsureMonitorInitialized();
    }

    protected void CheckReentrancy() {
        if (this.blockReentrancyCount > 0) {
            // we can allow changes if there's only one listener - the problem
            // only arises if reentrant changes make the original event args
            // invalid for later listeners.  This keeps existing code working
            // (e.g. Selector.SelectedItems).

            NotifyCollectionChangedEventHandler? changing = this.CollectionChanging;
            NotifyCollectionChangedEventHandler? changed = this.CollectionChanged;
            if (changing != null && !changing.HasSingleTarget)
                Throw();
            if (changed != null && !changed.HasSingleTarget)
                Throw();

            return;

            [DoesNotReturn]
            [MethodImpl(MethodImplOptions.NoInlining)]
            static void Throw() {
                throw new InvalidOperationException("Reentrancy Not Allowed");
            }
        }
    }

    public new List<T>.Enumerator GetEnumerator() => this.myItems.GetEnumerator();

    private SimpleMonitor EnsureMonitorInitialized() => this._monitor ??= new SimpleMonitor(this);

    private sealed class SimpleMonitor(NewObservableList<T> collection) : IDisposable {
        public void Dispose() => collection.blockReentrancyCount--;
    }
}

internal static class ObservableListUtils {
    public static readonly NotifyCollectionChangedEventArgs ResetArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
}