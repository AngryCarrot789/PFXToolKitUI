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
    private SimpleMonitor? _monitor;
    private readonly bool isDerivedType;
    protected int blockReentrancyCount;

    public event ObservableListMultipleItemsEventHandler<T>? BeforeItemsAdded;
    public event ObservableListBeforeRemovedEventHandler<T>? BeforeItemsRemoved;
    public event ObservableListReplaceEventHandler<T>? BeforeItemReplace;
    public event ObservableListMoveEventHandler<T>? BeforeItemMoved;

    public event ObservableListMultipleItemsEventHandler<T>? ItemsAdded;
    public event ObservableListMultipleItemsEventHandler<T>? ItemsRemoved;
    public event ObservableListReplaceEventHandler<T>? ItemReplaced;
    public event ObservableListMoveEventHandler<T>? ItemMoved;

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
    /// Fired when items are added to or removed from this list, or an item is replaced or moved.
    /// <para>
    /// When this list is cleared, this is fired with <see cref="NotifyCollectionChangedAction.Reset"/>
    /// </para>
    /// </summary>
    public event NotifyCollectionChangedEventHandler? CollectionChanged;

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

        // Optimisation
        // MethodInfo info = this.GetType().GetMethod(nameof(this.OnItemsRemoved), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy)!;
        // this.IsItemsRemovedOverridden = info.GetBaseDefinition().DeclaringType != info.DeclaringType;
        // If we are a derived type, then disable optimisations that avoid invoking specific
        // notification methods since they may involve copying the lots of items when it's unnecessary
        this.isDerivedType = this.GetType() != typeof(ObservableList<T>);

        this.ClearBehavior = clearBehavior;
    }

    protected override void InsertItem(int index, T item) {
        this.CheckReentrancy();
        if (index < 0)
            throw new IndexOutOfRangeException("Negative index: " + index);
        if ((uint) index > (uint) this.myItems.Count)
            throw new IndexOutOfRangeException($"Index beyond length of this list: {index} > {this.myItems.Count}");

        SingletonList<T>? itemList = null;
        this.BeforeItemsAdded?.Invoke(this, index, itemList = new SingletonList<T>(item));
        this.myItems.Insert(index, item);

        // Invoke base method when derived or if we have an ItemsAdded or CC handler
        if (this.isDerivedType || this.ItemsAdded != null || this.CollectionChanged != null)
            this.OnItemsAdded(index, itemList ?? new SingletonList<T>(item));
    }

    public void AddRange(IEnumerable<T> items) => this.InsertRange(this.Count, items);

    public void InsertRange(int index, IEnumerable<T> items) {
        this.CheckReentrancy();
        if (index < 0)
            throw new IndexOutOfRangeException("Negative index: " + index);
        if ((uint) index > (uint) this.myItems.Count)
            throw new IndexOutOfRangeException($"Index beyond length of this list: {index} > {this.myItems.Count}");

        // Slight risk in passing list to the ItemsAdded event in case items mutates asynchronously... meh
        if (!(items is IList<T> list)) {
            // Probably enumerator method or something along those lines, convert to list for speedy insertion
            list = items.ToList();
        }

        if (list.Count > 0) {            
            this.BeforeItemsAdded?.Invoke(this, index, list);
            this.myItems.InsertRange(index, list);

            if (this.isDerivedType || this.ItemsAdded != null || this.CollectionChanged != null) // Stops the event handler modifying the list
                list = list.AsReadOnly();

            this.OnItemsAdded(index, list);
        }
    }

    protected override void RemoveItem(int index) {
        this.CheckReentrancy();
        T removedItem = this[index];

        this.BeforeItemsRemoved?.Invoke(this, index, 1);
        this.myItems.RemoveAt(index);

        // Invoke base method when derived or we have an ItemsRemoved handler
        if (this.isDerivedType || this.ItemsRemoved != null || this.CollectionChanged != null)
            this.OnItemsRemoved(index, new SingletonList<T>(removedItem));
    }

    public void RemoveRange(int index, int count) {
        if (index < 0)
            throw new IndexOutOfRangeException("Negative index: " + index);
        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count), count, "Count is negative");
        if (this.myItems.Count - index < count)
            throw new IndexOutOfRangeException("The index and count exceed the range of this list");
        if (count == 0)
            return;

        this.CheckReentrancy();

        this.BeforeItemsRemoved?.Invoke(this, index, count);
        if (!this.isDerivedType && this.ItemsRemoved == null && this.CollectionChanged == null) {
            // We are not a derived type, and we have no ItemsRemoved or CC handler,
            // so we don't need to create any pointless sub-lists

            this.myItems.RemoveRange(index, count);
        }
        else {
            List<T> items = this.myItems.Slice(index, count);
            this.myItems.RemoveRange(index, count);
            this.OnItemsRemoved(index, items.AsReadOnly());
        }
    }

    public LongRangeUnion RemoveRange(IEnumerable<T> items) {
        this.CheckReentrancy();
        if (!(items is IList<T> list)) {
            list = items.ToList();
        }

        // Calculate a union of ranges to remove
        LongRangeUnion union = new LongRangeUnion();
        foreach (T item in list) {
            int index = this.myItems.IndexOf(item);
            if (index != -1) {
                union.Add(index);
            }
        }

        if (union.RangeCount > 0) {
            // We could iterate front to back and increase start offset by length each time,
            // however it's more efficient to remove back to front
            List<LongRange> indices = union.ToList();
            for (int i = indices.Count - 1; i >= 0; i--) {
                LongRange range = indices[i];
                if (range.Length == 1) {
                    this.RemoveItem((int) range.Start);
                }
                else {
                    this.RemoveRange((int) range.Start, (int) range.Length);
                }
            }
        }

        return union;
    }

    protected override void SetItem(int index, T newItem) {
        this.CheckReentrancy();
        T oldItem = this[index];

        this.BeforeItemReplace?.Invoke(this, index, oldItem, newItem);
        base.SetItem(index, newItem);
        this.OnItemReplaced(index, oldItem, newItem);
    }

    public void Move(int oldIndex, int newIndex) => this.MoveItem(oldIndex, newIndex);

    protected virtual void MoveItem(int oldIndex, int newIndex) {
        this.CheckReentrancy();
        if (newIndex < 0)
            throw new IndexOutOfRangeException("Negative newIndex: " + newIndex);
        if ((uint) newIndex >= (uint) this.myItems.Count)
            throw new IndexOutOfRangeException($"newIndex beyond length of this list: {newIndex} >= {this.myItems.Count}");

        T item = this[oldIndex];

        this.BeforeItemMoved?.Invoke(this, oldIndex, newIndex, item);
        base.RemoveItem(oldIndex);
        base.InsertItem(newIndex, item);
        this.OnItemMoved(oldIndex, newIndex, item);
    }

    protected override void ClearItems() {
        this.CheckReentrancy();
        if (this.myItems.Count < 1) {
            return;
        }

        this.BeforeItemsRemoved?.Invoke(this, 0, this.myItems.Count);
        if (!this.isDerivedType && this.ItemsRemoved == null && this.CollectionChanged == null) {
            // We are not a derived type, and we have no ItemsRemoved or CC handler,
            // so we don't need to create any pointless sub-lists
            this.myItems.Clear();
        }
        else {
            ReadOnlyCollection<T> items = this.myItems.ToList().AsReadOnly();
            this.myItems.Clear();
            this.OnItemsRemoved(0, items);
        }
    }

    protected virtual void OnItemsAdded(int index, IList<T> items) {
        try {
            this.blockReentrancyCount++;
            this.ItemsAdded?.Invoke(this, index, items);
            this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, items as IList ?? items.ToList(), index));
        }
        finally {
            this.blockReentrancyCount--;
        }
    }

    protected virtual void OnItemsRemoved(int index, IList<T> items) {
        try {
            this.blockReentrancyCount++;
            this.ItemsRemoved?.Invoke(this, index, items);
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
            this.ItemReplaced?.Invoke(this, index, oldItem, newItem);
            this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItem, oldItem, index));
        }
        finally {
            this.blockReentrancyCount--;
        }
    }

    protected virtual void OnItemMoved(int oldIndex, int newIndex, T item) {
        try {
            this.blockReentrancyCount++;
            this.ItemMoved?.Invoke(this, oldIndex, newIndex, item);
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

    public static void Test() {
        ObservableList<int> list = new ObservableList<int>();

        // Indexable processor removes back to front as an optimisation, can disable in constructor
        ObservableItemProcessor.MakeIndexable(list, (s, i, o) => {
            Console.WriteLine($"Added '{o}' at {i}");
        }, (s, i, o) => {
            Console.WriteLine($"Removed '{o}' at {i}");
        }, (s, oldI, newI, o) => {
            Console.WriteLine($"Moved '{o}' from {oldI} to {newI}");
        });

        list.Add(0);
        list.Add(1);
        list.Add(2);
        list.Add(3);
        list.Add(4);
        list.Add(5);
        list.Add(6);
        list.Add(7);
        list.Add(8);

        // list = 0,1,2,3,4,5,6,7,8
        // Removing 4 items at index 2 removes 2,3,4,5
        // Remaining list = 0,1,6,7,8
        list.RemoveRange(2, 4);

        // assert list.Count == 5
        // assert list == [0,1,6,7,8]
    }
}