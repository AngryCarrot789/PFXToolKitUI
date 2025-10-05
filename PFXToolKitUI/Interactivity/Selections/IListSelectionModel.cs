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

/// <summary>
/// An interface for a list-based selection model.
/// </summary>
public interface IListSelectionModel {
    /// <summary>
    /// Returns the number of selected items
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Enumerates the selected indices
    /// </summary>
    IEnumerable<LongRange> SelectedIndices { get; }

    /// <summary>
    /// An event fired when the selection state changes
    /// </summary>
    public event EventHandler<ListSelectionModelChangedEventArgs>? SelectionChanged;

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
    void SelectRanges(LongRangeUnion union);

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
    void DeselectRanges(LongRangeUnion union);

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
    /// Replaces the current selection with the new selected index, deselecting everything else
    /// </summary>
    /// <param name="index">The index to make selected. Anything else will become deselected</param>
    void SetSelection(int index) => this.SetSelection([LongRange.FromStartAndLength(index, 1)]);
    
    /// <summary>
    /// Replaces the current selection with the new selection, deselecting everything else
    /// </summary>
    /// <param name="range">The range to make selected. Anything else will become deselected</param>
    void SetSelection(LongRange range) => this.SetSelection([range]);
    
    /// <summary>
    /// Replaces the current selection with the provided selection, deselecting everything else
    /// </summary>
    /// <param name="indices">The indices to make selected. Anything else will become deselected</param>
    void SetSelection(IEnumerable<int> indices) => this.SetSelection(new LongRangeUnion(indices.Select(t => LongRange.FromStartAndLength(t, 1))));

    /// <summary>
    /// Replaces the current selection with the provided selection, deselecting everything else
    /// </summary>
    /// <param name="ranges">The ranges to make selected. Anything outside the ranges will become deselected</param>
    void SetSelection(LongRangeUnion ranges);

    /// <summary>
    /// Deselects all items
    /// </summary>
    void Clear();

    /// <summary>
    /// Returns a new <see cref="LongRangeUnion"/> containing the selected indices
    /// </summary>
    /// <returns>The selected indices</returns>
    LongRangeUnion ToLongRangeUnion();
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
    /// Gets the nth selected item
    /// </summary>
    /// <param name="index">The nth selected index</param>
    T this[int index] { get; }

    /// <summary>
    /// Enumerates the selected items
    /// </summary>
    IReadOnlyList<T> SelectedItems { get; }

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
    void SelectItem(T item);

    /// <summary>
    /// Selects multiple items
    /// </summary>
    /// <param name="items"></param>
    void SelectItems(IEnumerable<T> items);
    
    /// <summary>
    /// Deselects an item
    /// </summary>
    /// <param name="item"></param>
    void DeselectItem(T item);

    /// <summary>
    /// Deselects multiple items
    /// </summary>
    /// <param name="items"></param>
    void DeselectItems(IEnumerable<T> items);

    /// <summary>
    /// Checks if an item is selected
    /// </summary>
    /// <param name="item">The item</param>
    /// <returns>Null if the item does not exist in <see cref="SourceList"/>, otherwise the result of <see cref="IListSelectionModel.IsSelected"/></returns>
    bool? IsItemSelected(T item);
}

public readonly struct ListSelectionModelChangedEventArgs(IList<LongRange> addedIndices, IList<LongRange> removedIndices) {
    /// <summary>
    /// The ranges containing indices that are now selected
    /// </summary>
    public IList<LongRange> AddedIndices { get; } = addedIndices;

    /// <summary>
    /// The ranges containing indices that are no longer selected
    /// </summary>
    public IList<LongRange> RemovedIndices { get; } = removedIndices;
}