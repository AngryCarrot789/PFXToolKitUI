﻿// 
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
    public event ObservableListMultipleItemsEventHandler<T>? ItemsAdded;
    public event ObservableListMultipleItemsEventHandler<T>? ItemsRemoved;
    public event ObservableListReplaceEventHandler<T>? ItemReplaced;
    public event ObservableListSingleItemEventHandler<T>? ItemMoved;

    public ReadOnlyObservableList(IObservableList<T> list) : base(list) {
        list.ItemsAdded += this.ListOnItemsAdded;
        list.ItemsRemoved += this.ListOnItemsRemoved;
        list.ItemReplaced += this.ListOnItemReplaced;
        list.ItemMoved += this.ListOnItemMoved;
    }

    private void ListOnItemsAdded(IObservableList<T> list, IList<T> items, int index) => this.ItemsAdded?.Invoke(this, items, index);

    private void ListOnItemsRemoved(IObservableList<T> list, IList<T> items, int index) => this.ItemsRemoved?.Invoke(this, items, index);

    private void ListOnItemReplaced(IObservableList<T> list, T oldItem, T newItem, int index) => this.ItemReplaced?.Invoke(this, oldItem, newItem, index);

    private void ListOnItemMoved(IObservableList<T> list, T item, int oldIndex, int newIndex) => this.ItemMoved?.Invoke(this, item, oldIndex, newIndex);
}