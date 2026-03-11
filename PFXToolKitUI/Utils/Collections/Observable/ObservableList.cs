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
using System.Diagnostics;
using PFXToolKitUI.Utils.Events;
using PFXToolKitUI.Utils.Ranges;

namespace PFXToolKitUI.Utils.Collections.Observable;

public enum ResetBehavior {
    /// <summary>
    /// When the list is cleared or all items are removed, we call reset
    /// </summary>
    Reset,

    /// <summary>
    /// When the list is cleared or all items are removed, we call with the cleared/removed items
    /// </summary>
    Remove
}

/// <summary>
/// An optimised observable collection. This class is not thread safe; manual synchronization required
/// </summary>
/// <typeparam name="T">Type of value to store</typeparam>
public class ObservableList<T> : CollectionEx<T>, IObservableList<T> {
    public const ResetBehavior DefaultClearBehavior = ResetBehavior.Reset;

    private readonly List<T> myItems;
    private readonly SingleItemListImpl cachedList;
    private SingleItemListImpl? activeCachedList;
    private SimpleMonitor? _monitor;
    private readonly bool isDerivedType;
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

    public event NotifyCollectionChangedEventHandler? CollectionChanged;
    public event EventHandler<ItemsAddOrRemoveEventArgs<T>>? ValidateAdd;
    public event EventHandler<ItemsAddOrRemoveEventArgs<T>>? ValidateRemove;
    public event EventHandler<ItemReplaceEventArgs<T>>? ValidateReplace;
    public event EventHandler<ItemMoveEventArgs<T>>? ValidateMove;
    public event EventHandler<ItemsAddOrRemoveEventArgs<T>>? ItemsAdded;
    public event EventHandler<ItemsAddOrRemoveEventArgs<T>>? ItemsRemoved;
    public event EventHandler<ItemReplaceEventArgs<T>>? ItemReplaced;
    public event EventHandler<ItemMoveEventArgs<T>>? ItemMoved;

    public ResetBehavior ClearBehavior { get; }

    public ObservableList() : this(DefaultClearBehavior) {
    }

    public ObservableList(ResetBehavior clearBehavior) : this([], clearBehavior) {
    }

    public ObservableList(IEnumerable<T> collection) : this(collection.ToList(), DefaultClearBehavior) {
    }

    public ObservableList(IEnumerable<T> collection, ResetBehavior clearBehavior) : this(collection.ToList(), clearBehavior) {
    }

    private ObservableList(List<T> list, ResetBehavior clearBehavior) : base(list) {
        this.myItems = (List<T>?) base.Items!;
        this.cachedList = new SingleItemListImpl();
        this.activeCachedList = this.cachedList;

        // Optimisation
        // MethodInfo info = this.GetType().GetMethod(nameof(this.OnItemsRemoved), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy)!;
        // this.IsItemsRemovedOverridden = info.GetBaseDefinition().DeclaringType != info.DeclaringType;
        // If we are a derived type, then disable optimisations that avoid invoking specific
        // notification methods since they may involve copying the lots of items when it's unnecessary
        this.isDerivedType = this.GetType() != typeof(ObservableList<T>);

        this.ClearBehavior = clearBehavior;
    }

    private SingleItemListImpl GetOrCreateList(T item, ref SingleItemListImpl? reference) {
        if (reference == null) {
            if ((reference = this.activeCachedList) != null) {
                this.activeCachedList = null;
            }
            else {
                reference = new SingleItemListImpl();
            }
            
            reference.value = item;
        }
        
        return reference;
    }

    private void TryReturnCachedList(SingleItemListImpl? list) {
        if (list != null && list == this.cachedList) {
            Debug.Assert(this.activeCachedList == null);
            this.activeCachedList = this.cachedList;
        }
    }

    protected override void InsertItem(int index, T item) {
        CollectionUtils.ThrowIfInsertOutOfBounds(this, index);
        this.CheckReentrancy();

        SingleItemListImpl? tmpList = null;
        try {
            this.ValidateAdd?.Invoke(this, new ItemsAddOrRemoveEventArgs<T>(index, this.GetOrCreateList(item, ref tmpList)));
            this.myItems.Insert(index, item);

            // Invoke base method when derived or if we have an ItemsAdded or CC handler
            if (this.isDerivedType || this.ItemsAdded != null || this.CollectionChanged != null)
                this.OnItemsAdded(index, this.GetOrCreateList(item, ref tmpList));
        }
        finally {
            this.TryReturnCachedList(tmpList);
        }
    }

    public void AddRange(IEnumerable<T> items) => this.InsertRange(this.Count, items);

    public void InsertRange(int index, IEnumerable<T> items) {
        CollectionUtils.ThrowIfInsertOutOfBounds(this, index);
        this.CheckReentrancy();

        // Slight risk in passing list to the ItemsAdded event in case items mutates asynchronously... meh
        if (!(items is IList<T> list)) {
            // Probably enumerator method or something along those lines, convert to list for speedy insertion
            list = items.ToList();
        }

        if (list.Count > 0) {
            this.ValidateAdd?.Invoke(this, new ItemsAddOrRemoveEventArgs<T>(index, list));
            this.myItems.InsertRange(index, list);

            if (this.isDerivedType || this.ItemsAdded != null || this.CollectionChanged != null) {
                this.OnItemsAdded(index, list.AsReadOnly());
            }
        }
    }

    protected override void RemoveItem(int index) {
        this.CheckReentrancy();
        T removedItem = this[index];

        SingleItemListImpl? tmpList = null;
        try {
            this.ValidateRemove?.Invoke(this, new ItemsAddOrRemoveEventArgs<T>(index, this.GetOrCreateList(removedItem, ref tmpList)));
            this.myItems.RemoveAt(index);

            // Invoke base method when derived or we have an ItemsRemoved handler
            if (this.isDerivedType || this.ItemsRemoved != null || this.CollectionChanged != null)
                this.OnItemsRemoved(index, this.GetOrCreateList(removedItem, ref tmpList));
        }
        finally {
            this.TryReturnCachedList(tmpList);
        }
    }

    public void RemoveRange(int index, int count) {
        CollectionUtils.ThrowIfOutOfBounds(this, index, count);
        this.CheckReentrancy();

        ReadOnlyCollection<T> items = this.myItems.Slice(index, count).AsReadOnly();
        this.ValidateRemove?.Invoke(this, new ItemsAddOrRemoveEventArgs<T>(index, items));
        this.myItems.RemoveRange(index, count);
        this.OnItemsRemoved(index, items);
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

        this.ValidateReplace?.Invoke(this, new ItemReplaceEventArgs<T>(index, oldItem, newItem));
        base.SetItem(index, newItem);
        this.OnItemReplaced(index, oldItem, newItem);
    }

    public void Move(int oldIndex, int newIndex) => this.MoveItem(oldIndex, newIndex);

    protected virtual void MoveItem(int oldIndex, int newIndex) {
        CollectionUtils.ThrowIfMoveOutOfBounds(this, oldIndex, newIndex);
        this.CheckReentrancy();

        T item = this[oldIndex];
        this.ValidateMove?.Invoke(this, new ItemMoveEventArgs<T>(oldIndex, newIndex, item));

        base.RemoveItem(oldIndex);
        base.InsertItem(newIndex, item);
        this.OnPostItemMoved(oldIndex, newIndex, item);
    }

    protected override void ClearItems() {
        this.CheckReentrancy();
        if (this.myItems.Count < 1) {
            return;
        }

        if (this.isDerivedType || this.ValidateRemove != null || this.ItemsRemoved != null || this.CollectionChanged != null) {
            ReadOnlyCollection<T> items = this.myItems.ToList().AsReadOnly();
            this.ValidateRemove?.Invoke(this, new ItemsAddOrRemoveEventArgs<T>(0, items));

            this.myItems.Clear();
            this.OnItemsRemoved(0, items);
        }
        else {
            // We are not a derived type, and we have no ItemsRemoved or CC handler,
            // so we don't need to create any pointless sub-lists
            this.myItems.Clear();
        }
    }

    protected virtual void OnItemsAdded(int index, IList<T> items) {
        try {
            this.blockReentrancyCount++;
            this.ItemsAdded?.Invoke(this, new ItemsAddOrRemoveEventArgs<T>(index, items));
            this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, items as IList ?? items.ToList(), index));
        }
        finally {
            this.blockReentrancyCount--;
        }
    }

    protected virtual void OnItemsRemoved(int index, IList<T> items) {
        try {
            this.blockReentrancyCount++;
            this.ItemsRemoved?.Invoke(this, new ItemsAddOrRemoveEventArgs<T>(index, items));
            this.CollectionChanged?.Invoke(this,
                this.Count > 0 || this.ClearBehavior == ResetBehavior.Remove // call Remove on reset
                    ? new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, items as IList ?? items.ToList(), index)
                    : new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
        finally {
            this.blockReentrancyCount--;
        }
    }

    protected virtual void OnItemReplaced(int index, T oldItem, T newItem) {
        try {
            this.blockReentrancyCount++;
            this.ItemReplaced?.Invoke(this, new ItemReplaceEventArgs<T>(index, oldItem, newItem));
            this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItem, oldItem, index));
        }
        finally {
            this.blockReentrancyCount--;
        }
    }

    protected virtual void OnPostItemMoved(int oldIndex, int newIndex, T item) {
        try {
            this.blockReentrancyCount++;
            this.ItemMoved?.Invoke(this, new ItemMoveEventArgs<T>(oldIndex, newIndex, item));
            this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, item, newIndex, oldIndex));
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
            if (this.ItemsAdded?.GetInvocationList().Length > 1 ||
                this.ItemsRemoved?.GetInvocationList().Length > 1 ||
                this.ItemReplaced?.GetInvocationList().Length > 1 ||
                this.ItemMoved?.GetInvocationList().Length > 1 ||
                this.CollectionChanged?.GetInvocationList().Length > 1)
                throw new InvalidOperationException("Reentrancy Not Allowed");
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => this.myItems.GetEnumerator();

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => this.myItems.GetEnumerator();

    public new List<T>.Enumerator GetEnumerator() => this.myItems.GetEnumerator();

    private SimpleMonitor EnsureMonitorInitialized() => this._monitor ??= new SimpleMonitor(this);

    private sealed class SimpleMonitor(ObservableList<T> collection) : IDisposable {
        public void Dispose() => collection.blockReentrancyCount--;
    }

    private class SingleItemListImpl : IList<T>, IList {
        public T value;

        public int Count => 1;

        public bool IsReadOnly => true;

        public T this[int index] {
            get {
                ArgumentOutOfRangeException.ThrowIfNotEqual(index, 0);
                return this.value;
            }
            set => throw new NotSupportedException("Read-only list");
        }

        object? IList.this[int index] {
            get => index == 0 ? this.value : throw new IndexOutOfRangeException("Index was out of range: " + index);
            set => throw new NotSupportedException("Read-only list");
        }

        public bool IsSynchronized => false;
        public object SyncRoot => this;
        public bool IsFixedSize => true;

        public void CopyTo(T[] array, int arrayIndex) {
            array[arrayIndex] = this.value;
        }
        
        public bool Contains(T item) => EqualityComparer<T>.Default.Equals(item, this.value);

        public int IndexOf(T item) => this.Contains(item) ? 0 : -1;
        
        public bool Contains(object? val) => EqualityComparer<object>.Default.Equals(val, this.value);
        
        public int IndexOf(object? val) => this.Contains(val) ? 0 : -1;
        
        public IEnumerator<T> GetEnumerator() {
            yield return this.value;
        }

        IEnumerator IEnumerable.GetEnumerator() {
            yield return this.value;
        }

        public void CopyTo(Array array, int index) => array.SetValue(this.value, index);

        public void Add(T item) => throw new NotSupportedException("Read-only list");
        public void Clear() => throw new NotSupportedException("Read-only list");
        public bool Remove(T item) => throw new NotSupportedException("Read-only list");
        public void Insert(int index, T item) => throw new NotSupportedException("Read-only list");
        public void RemoveAt(int index) => throw new NotSupportedException("Read-only list");
        public int Add(object? val) => throw new NotSupportedException("Read-only list");
        public void Insert(int index, object? value) => throw new NotSupportedException("Read-only list");
        public void Remove(object? value) => throw new NotSupportedException("Read-only list");
    }
}