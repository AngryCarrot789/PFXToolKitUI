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

using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace PFXToolKitUI.Utils.Collections.Observable;

/// <summary>
/// A read-only wrapper of <see cref="IObservableList{T}"/>
/// </summary>
/// <typeparam name="T">The type of value we store</typeparam>
public class ReadOnlyObservableList<T> : ReadOnlyCollection<T>, IObservableList<T> {
    private readonly IObservableList<T> delegateList;

    public event ObservableListBeforeAddedEventHandler<T>? BeforeItemAdded;
    public event ObservableListBeforeRemovedEventHandler<T>? BeforeItemsRemoved;
    public event ObservableListReplaceEventHandler<T>? BeforeItemReplace;
    public event ObservableListMoveEventHandler<T>? BeforeItemMoved;
    public event ObservableListMultipleItemsEventHandler<T>? ItemsAdded;
    public event ObservableListMultipleItemsEventHandler<T>? ItemsRemoved;
    public event ObservableListReplaceEventHandler<T>? ItemReplaced;
    public event ObservableListMoveEventHandler<T>? ItemMoved;
    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    public ResetBehavior ClearBehavior => this.delegateList.ClearBehavior;

    public ReadOnlyObservableList(IObservableList<T> list) : base(list) {
        this.delegateList = list;
        list.BeforeItemAdded += (sender, index, item) => this.BeforeItemAdded?.Invoke(this, index, item);
        list.BeforeItemsRemoved += (sender, index, count) => this.BeforeItemsRemoved?.Invoke(this, index, count);
        list.BeforeItemReplace += (sender, index, oldItem, newItem) => this.BeforeItemReplace?.Invoke(this, index, oldItem, newItem);
        list.BeforeItemMoved += (sender, oldIndex, newIndex, item) => this.BeforeItemMoved?.Invoke(this, oldIndex, newIndex, item);
        list.ItemsAdded += (sender, index, items) => this.ItemsAdded?.Invoke(this, index, items);
        list.ItemsRemoved += (sender, index, items) => this.ItemsRemoved?.Invoke(this, index, items);
        list.ItemReplaced += (sender, index, oldItem, newItem) => this.ItemReplaced?.Invoke(this, index, oldItem, newItem);
        list.ItemMoved += (sender, oldIndex, newIndex, item) => this.ItemMoved?.Invoke(this, oldIndex, newIndex, item);
        list.CollectionChanged += (sender, e) => this.CollectionChanged?.Invoke(this, e);
    }

    void IObservableList<T>.AddRange(IEnumerable<T> items) => throw new NotSupportedException("Read-only collection");
    void IObservableList<T>.InsertRange(int index, IEnumerable<T> items) => throw new NotSupportedException("Read-only collection");
    void IObservableList<T>.RemoveRange(int index, int count) => throw new NotSupportedException("Read-only collection");
    IntRangeUnion IObservableList<T>.RemoveRange(IEnumerable<T> items) => throw new NotSupportedException("Read-only collection");
}