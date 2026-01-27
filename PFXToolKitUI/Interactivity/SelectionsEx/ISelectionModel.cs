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

namespace PFXToolKitUI.Interactivity.SelectionsEx;

/// <summary>
/// An interface for a list-based selection model.
/// </summary>
public interface ISelectionModel {
    /// <summary>
    /// Returns the number of selected items
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Deselects all items
    /// </summary>
    void DeselectAll();
}

/// <summary>
/// A generic implementation of <see cref="ISelectionModel"/>
/// <para>
/// Note about selecting and deselecting items that do not exist (e.g. <see cref="SelectItems"/>): no exception will
/// be thrown, instead, the request will be ignored. It is the job of the caller to check the items beforehand
/// </para>
/// </summary>
/// <typeparam name="T"></typeparam>
public interface ISelectionModel<T> : ISelectionModel {
    /// <summary>
    /// Gets a read-only list of the selected items
    /// </summary>
    IReadOnlySet<T> SelectedItems { get; }

    /// <summary>
    /// An event fired when the selection state changes
    /// </summary>
    public event EventHandler<SelectionModelExChangedEventArgs<T>>? SelectionChanged;

    /// <summary>
    /// Checks if an item is selected
    /// </summary>
    /// <param name="item">The item</param>
    /// <returns>Null if the item does not exist in <see cref="SourceList"/>, otherwise the result of <see cref="ISelectionModel.IsSelected"/></returns>
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
}

public readonly struct SelectionModelExChangedEventArgs<T>(IList<T> addedItems, IList<T> removedItems) {
    /// <summary>
    /// Gets the list containing items that are now selected
    /// </summary>
    public IList<T> AddedItems { get; } = addedItems;

    /// <summary>
    /// Gets the list containing items that are no longer selected
    /// </summary>
    public IList<T> RemovedItems { get; } = removedItems;
}