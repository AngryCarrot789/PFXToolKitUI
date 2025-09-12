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

using PFXToolKitUI.Utils;
using PFXToolKitUI.Utils.Collections.Observable;

namespace PFXToolKitUI.Interactivity.Selections;

public interface IListSelectionModel {
    /// <summary>
    /// Returns the number of selected items
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Enumerates the selected indices
    /// </summary>
    IEnumerable<IntRange> SelectedIndices { get; }

    /// <summary>
    /// An event fired when the selection state changes
    /// </summary>
    public event EventHandler<SelectionModelChangedEventArgs>? SelectionChanged;

    /// <summary>
    /// Select a single item
    /// </summary>
    /// <param name="index"></param>
    void Select(int index);

    /// <summary>
    /// Select a range of items
    /// </summary>
    /// <param name="index"></param>
    /// <param name="count"></param>
    void SelectRange(int index, int count);

    /// <summary>
    /// Select a list of ranges of items
    /// </summary>
    /// <param name="union"></param>
    void SelectRanges(IntRangeUnion union);

    /// <summary>
    /// Deselect a single item
    /// </summary>
    /// <param name="index"></param>
    void Deselect(int index);

    /// <summary>
    /// Deselect a range of items
    /// </summary>
    /// <param name="index"></param>
    /// <param name="count"></param>
    void DeselectRange(int index, int count);

    /// <summary>
    /// Deselect a list of ranges of items
    /// </summary>
    /// <param name="union"></param>
    void DeselectRanges(IntRangeUnion union);

    /// <summary>
    /// Check if the item at the index is selected
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    bool IsSelected(int index);

    /// <summary>
    /// Selects all items
    /// </summary>
    void SelectAll();

    /// <summary>
    /// Deselects all items
    /// </summary>
    void Clear();

    /// <summary>
    /// Returns a new <see cref="IntRangeUnion"/> containing the selected indices
    /// </summary>
    /// <returns>The selected indices</returns>
    IntRangeUnion ToIntRangeUnion();
}

public interface IListSelectionModel<T> : IListSelectionModel {
    /// <summary>
    /// Gets the nth selected item
    /// </summary>
    /// <param name="index">The nth selected index</param>
    T this[int index] { get; }

    /// <summary>
    /// Enumerates the selected items
    /// </summary>
    IEnumerable<T> SelectedItems { get; }

    /// <summary>
    /// Enumerates the selected indices and pairs them with the item itself
    /// </summary>
    IEnumerable<KeyValuePair<int, T>> SelectedEntries { get; }

    /// <summary>
    /// Gets the observable list that stores the items of a mode that may be selectable.
    /// This list is not the selected items list, instead, use <see cref="SelectedItems"/> to enumerate the items.
    /// </summary>
    ObservableList<T> SourceList { get; }

    /// <summary>
    /// Selects an item
    /// </summary>
    /// <param name="item"></param>
    void Select(T item);

    /// <summary>
    /// Selects multiple items
    /// </summary>
    /// <param name="items"></param>
    void Select(IEnumerable<T> items);
    
    /// <summary>
    /// Deselects an item
    /// </summary>
    /// <param name="item"></param>
    void Deselect(T item);

    /// <summary>
    /// Deselects multiple items
    /// </summary>
    /// <param name="items"></param>
    void Deselect(IEnumerable<T> items);

    /// <summary>
    /// Checks if an item is selected
    /// </summary>
    /// <param name="item">The item</param>
    /// <returns>Null if the item does not exist in <see cref="SourceList"/>, otherwise the result of <see cref="IListSelectionModel.IsSelected"/></returns>
    bool? IsItemSelected(T item);
}

public readonly struct SelectionModelChangedEventArgs(IList<IntRange> addedIndices, IList<IntRange> removedIndices) {
    /// <summary>
    /// The ranges containing indices that are now selected
    /// </summary>
    public IList<IntRange> AddedIndices { get; } = addedIndices;

    /// <summary>
    /// The ranges containing indices that are no longer selected
    /// </summary>
    public IList<IntRange> RemovedIndices { get; } = removedIndices;
}