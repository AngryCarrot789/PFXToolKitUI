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
using PFXToolKitUI.Utils.Events;
using PFXToolKitUI.Utils.Ranges;

namespace PFXToolKitUI.Utils.Collections.Observable;

/// <summary>
/// A read-only wrapper of <see cref="IObservableList{T}"/>
/// </summary>
/// <typeparam name="T">The type of value we store</typeparam>
public class ReadOnlyObservableList<T> : ReadOnlyCollection<T>, IObservableList<T> {
    private readonly IObservableList<T> delegateList;

    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    public event EventHandler<ItemsAddOrRemoveEventArgs<T>>? ValidateAdd;
    public event EventHandler<ItemsAddOrRemoveEventArgs<T>>? ValidateRemove;
    public event EventHandler<ItemReplaceEventArgs<T>>? ValidateReplace;
    public event EventHandler<ItemMoveEventArgs<T>>? ValidateMove;
    public event EventHandler<ItemsAddOrRemoveEventArgs<T>>? ItemsAdded;
    public event EventHandler<ItemsAddOrRemoveEventArgs<T>>? ItemsRemoved;
    public event EventHandler<ItemReplaceEventArgs<T>>? ItemReplaced;
    public event EventHandler<ItemMoveEventArgs<T>>? ItemMoved;
    public ResetBehavior ClearBehavior => this.delegateList.ClearBehavior;

    public ReadOnlyObservableList(IObservableList<T> list) : base(list) {
        this.delegateList = list;
        list.ValidateAdd += (sender, e) => this.ValidateAdd?.Invoke(this, e);
        list.ValidateRemove += (sender, e) => this.ValidateRemove?.Invoke(this, e);
        list.ValidateReplace += (sender, e) => this.ValidateReplace?.Invoke(this, e);
        list.ValidateMove += (sender, e) => this.ValidateMove?.Invoke(this, e);
        list.ItemsAdded += (sender, e) => this.ItemsAdded?.Invoke(this, e);
        list.ItemsRemoved += (sender, e) => this.ItemsRemoved?.Invoke(this, e);
        list.ItemReplaced += (sender, e) => this.ItemReplaced?.Invoke(this, e);
        list.ItemMoved += (sender, e) => this.ItemMoved?.Invoke(this, e);
        list.CollectionChanged += (sender, e) => this.CollectionChanged?.Invoke(this, e);
    }

    void IObservableList<T>.AddRange(IEnumerable<T> items) => throw new NotSupportedException("Read-only collection");
    void IObservableList<T>.InsertRange(int index, IEnumerable<T> items) => throw new NotSupportedException("Read-only collection");
    void IObservableList<T>.RemoveRange(int index, int count) => throw new NotSupportedException("Read-only collection");
    IntegerSet<int> IObservableList<T>.RemoveRange(IEnumerable<T> items) => throw new NotSupportedException("Read-only collection");
}