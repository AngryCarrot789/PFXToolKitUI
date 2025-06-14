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

using System.Collections.ObjectModel;

namespace PFXToolKitUI.Utils.Collections.Observable;

/// <summary>
/// A read-only wrapper of <see cref="IObservableList{T}"/>
/// </summary>
/// <typeparam name="T">The type of value we store</typeparam>
public class ReadOnlyObservableList<T> : ReadOnlyCollection<T>, IObservableList<T> {
    public event ObservableListBeforeAddedEventHandler<T>? BeforeItemAdded;
    public event ObservableListBeforeRemovedEventHandler<T>? BeforeItemsRemoved;
    public event ObservableListReplaceEventHandler<T>? BeforeItemReplace;
    public event ObservableListSingleItemEventHandler<T>? BeforeItemMoved;
    public event ObservableListMultipleItemsEventHandler<T>? ItemsAdded;
    public event ObservableListMultipleItemsEventHandler<T>? ItemsRemoved;
    public event ObservableListReplaceEventHandler<T>? ItemReplaced;
    public event ObservableListSingleItemEventHandler<T>? ItemMoved;

    public ReadOnlyObservableList(IObservableList<T> list) : base(list) {
        list.BeforeItemAdded += this.ListOnBeforeItemAdded;
        list.BeforeItemsRemoved += this.ListOnBeforeItemsRemoved;
        list.BeforeItemReplace += this.ListOnBeforeItemReplace;
        list.BeforeItemMoved += this.ListOnBeforeItemMoved;
        list.ItemsAdded += this.ListOnItemsAdded;
        list.ItemsRemoved += this.ListOnItemsRemoved;
        list.ItemReplaced += this.ListOnItemReplaced;
        list.ItemMoved += this.ListOnItemMoved;
    }

    private void ListOnBeforeItemAdded(IObservableList<T> list, int index, T item) => this.BeforeItemAdded?.Invoke(this, index, item);

    private void ListOnBeforeItemsRemoved(IObservableList<T> list, int index, int count) => this.BeforeItemsRemoved?.Invoke(this, index, count);

    private void ListOnBeforeItemReplace(IObservableList<T> list, int index, T oldItem, T newItem) => this.BeforeItemReplace?.Invoke(this, index, oldItem, newItem);

    private void ListOnBeforeItemMoved(IObservableList<T> list, int oldIndex, int newIndex, T item) => this.BeforeItemMoved?.Invoke(this, oldIndex, newIndex, item);

    private void ListOnItemsAdded(IObservableList<T> list, int index, IList<T> items) => this.ItemsAdded?.Invoke(this, index, items);

    private void ListOnItemsRemoved(IObservableList<T> list, int index, IList<T> items) => this.ItemsRemoved?.Invoke(this, index, items);

    private void ListOnItemReplaced(IObservableList<T> list, int index, T oldItem, T newItem) => this.ItemReplaced?.Invoke(this, index, oldItem, newItem);

    private void ListOnItemMoved(IObservableList<T> list, int oldIndex, int newIndex, T item) => this.ItemMoved?.Invoke(this, oldIndex, newIndex, item);
}