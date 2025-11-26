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

using PFXToolKitUI.Utils.Collections.Observable;
using PFXToolKitUI.Utils.Ranges;

namespace PFXToolKitUI.Interactivity.Selections;

/// <summary>
/// An interface for a list-based selection model.
/// </summary>
public interface IListSelectionModel {
    /// <summary>
    /// Returns the number of selected items
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Selects all items
    /// </summary>
    void SelectAll();

    /// <summary>
    /// Deselects all items
    /// </summary>
    void DeselectAll();
}

/// <summary>
/// A generic implementation of <see cref="IListSelectionModel"/>
/// <para>
/// Note about selecting and deselecting items that do not exist (e.g. <see cref="SelectItems"/>): no exception will
/// be thrown, instead, the request will be ignored. It is the job of the caller to check the items beforehand
/// </para>
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IListSelectionModel<T> : IListSelectionModel {
    /// <summary>
    /// Gets the observable list that stores the items that are selectable.
    /// This list is not the selected items list, instead, use <see cref="SelectedItems"/> to enumerate the items.
    /// </summary>
    ObservableList<T> SourceList { get; }
    
    /// <summary>
    /// Gets a read-only list of the selected items
    /// </summary>
    IReadOnlySet<T> SelectedItems { get; }

    /// <summary>
    /// Gets the first selected item, which is the selected item whose index within <see cref="SourceList"/> is the smallest of all selected items. 
    /// Note, this method is is between O(n) and O(n^2)
    /// </summary>
    T First { get; }
    
    /// <summary>
    /// Gets the last selected item, which is the selected item whose index within <see cref="SourceList"/> is the largest of all selected items. 
    /// Note, this method is is between O(n) and O(n^2)
    /// </summary>
    T Last { get; }

    /// <summary>
    /// Gets the nth selected item, as if indexing into <see cref="GetSelectedIndices"/> but for items.
    /// Note, this method is is between O(n) and O(n^2)
    /// </summary>
    /// <param name="index">The index</param>
    /// <exception cref="ArgumentOutOfRangeException">The index is not within the bounds of the selected range</exception>
    T this[int index] { get; }

    /// <summary>
    /// An event fired when the selection state changes
    /// </summary>
    public event EventHandler<ListSelectionModelChangedEventArgs<T>>? SelectionChanged;

    /// <summary>
    /// Checks if an item is selected
    /// </summary>
    /// <param name="item">The item</param>
    /// <returns>Null if the item does not exist in <see cref="SourceList"/>, otherwise the result of <see cref="IListSelectionModel.IsSelected"/></returns>
    bool IsSelected(T item);
    
    /// <summary>
    /// Selects an item
    /// </summary>
    /// <param name="item">The item to select</param>
    void SelectItem(T item);

    /// <summary>
    /// Selects multiple items
    /// </summary>
    /// <param name="items">The items to select</param>
    void SelectItems(IEnumerable<T> items);
    
    /// <summary>
    /// Deselects an item
    /// </summary>
    /// <param name="item">The item to deselect</param>
    void DeselectItem(T item);

    /// <summary>
    /// Deselects multiple items
    /// </summary>
    /// <param name="items">The items to deselect</param>
    void DeselectItems(IEnumerable<T> items);
    
    /// <summary>
    /// Gets a list of the selected items' indices, ordered from smallest to largest
    /// </summary>
    /// <returns>A new list</returns>
    IntegerSet<int> GetSelectedIndices();
    
    /// <summary>
    /// Gets an <see cref="IEnumerable{T}"/> of selected entries, ordered from smallest index to largest
    /// </summary>
    /// <returns></returns>
    List<KeyValuePair<int, T>> GetSelectedEntries();
}

public readonly struct ListSelectionModelChangedEventArgs<T>(IList<T> addedItems, IList<T> removedItems) {
    /// <summary>
    /// Gets the list containing items that are now selected
    /// </summary>
    public IList<T> AddedItems { get; } = addedItems;

    /// <summary>
    /// Gets the list containing items that are no longer selected
    /// </summary>
    public IList<T> RemovedItems { get; } = removedItems;
}

public static class ListSelectionModelExtensions {
    public static void MoveItemHelper<T>(this IListSelectionModel<T> selectionModel, int oldIndex, int newIndex) {
        selectionModel.SourceList.Move(oldIndex, newIndex);
    }
}