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

using System.Collections.Specialized;

namespace PFXToolKitUI.Utils.Collections.Observable;

public delegate void ObservableListBeforeAddedEventHandler<T>(IObservableList<T> list, int index, T item);

public delegate void ObservableListBeforeRemovedEventHandler<T>(IObservableList<T> list, int index, int count);

public delegate void ObservableListMultipleItemsEventHandler<T>(IObservableList<T> list, int index, IList<T> items);

public delegate void ObservableListMoveEventHandler<T>(IObservableList<T> list, int oldIndex, int newIndex, T item);

public delegate void ObservableListReplaceEventHandler<T>(IObservableList<T> list, int index, T oldItem, T newItem);

/// <summary>
/// A list implementation that invokes a series of events when the collection changes
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IObservableList<T> : IList<T>, INotifyCollectionChanged {
    /// <summary>
    /// An event fired when items are about to be added to this collection
    /// </summary>
    public event ObservableListMultipleItemsEventHandler<T>? BeforeItemsAdded;
    
    /// <summary>
    /// An event fired when one or more items are about to be removed.
    /// </summary>
    public event ObservableListBeforeRemovedEventHandler<T>? BeforeItemsRemoved;
    
    
    /// <summary>
    /// An event fired when an item is about to be replaced by another item
    /// </summary>
    public event ObservableListReplaceEventHandler<T>? BeforeItemReplace;
    
    /// <summary>
    /// An event fired when an item is about to be moved from one index to another
    /// </summary>
    public event ObservableListMoveEventHandler<T>? BeforeItemMoved;
    
    /// <summary>
    /// An event fired when one or more items are inserted into the list.
    /// <para>
    /// The event args are the list of items inserted, and the index of the insertion
    /// </para>
    /// </summary>
    event ObservableListMultipleItemsEventHandler<T> ItemsAdded;

    /// <summary>
    /// An event fired when one or more items are removed from this list, at the given index. This is also fired
    /// when the list is cleared, where the index will equal 0 and the args contains all the items that were cleared
    /// <para>
    /// The event args are the list of items that were removed (which remain in the same order as
    /// they were in the list) and the original index of the first item in the removed items list
    /// </para>
    /// </summary>
    event ObservableListMultipleItemsEventHandler<T> ItemsRemoved;

    /// <summary>
    /// An event fired when an item is replaced. Only a single item is supported
    /// </summary>
    event ObservableListReplaceEventHandler<T> ItemReplaced;

    /// <summary>
    /// An event fired when an item is moved from one index to another
    /// </summary>
    event ObservableListMoveEventHandler<T> ItemMoved;

    /// <summary>
    /// Specifies the behaviour that determines which <see cref="NotifyCollectionChangedAction"/> is used when this list becomes empty
    /// </summary>
    ResetBehavior ClearBehavior { get; }

    /// <summary>
    /// Adds multiple items to this observable list
    /// </summary>
    /// <param name="items">The items to add</param>
    void AddRange(IEnumerable<T> items);
    
    /// <summary>
    /// Inserts multiple items into the list at the specific index
    /// </summary>
    /// <param name="index">The index to insert the item(s) at</param>
    /// <param name="items">The items to add</param>
    void InsertRange(int index, IEnumerable<T> items);
    
    /// <summary>
    /// Removes multiple items at the given index
    /// </summary>
    /// <param name="index">The index to remove the items at</param>
    /// <param name="count">The amount of items to remove</param>
    void RemoveRange(int index, int count);

    /// <summary>
    /// Removes a collection of items from this list, and returns a union of the indices of the removed items
    /// </summary>
    /// <param name="items">The items to remove</param>
    /// <returns>The indices removed</returns>
    LongRangeUnion RemoveRange(IEnumerable<T> items);
}