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
using PFXToolKitUI.Utils;
using PFXToolKitUI.Utils.Collections.Observable;

namespace PFXToolKitUI.Interactivity.Selections;

/// <summary>
/// Manages a one dimensional list of selected indices
/// </summary>
/// <typeparam name="T">The type of item that is selectable</typeparam>
public sealed class ListSelectionModel<T> : IListSelectionModel<T> {
    private readonly IntRangeUnion selectedIndices;

    public ObservableList<T> SourceList { get; }

    /// <summary>
    /// An event fired when this list's indices change
    /// </summary>
    public event EventHandler<ListSelectionModelChangedEventArgs>? SelectionChanged;

    /// <summary>
    /// Returns the number of selected items
    /// </summary>
    public int Count => this.selectedIndices.GrandTotal;

    /// <summary>
    /// Enumerates the selected items
    /// </summary>
    public IReadOnlyList<T> SelectedItems { get; }

    public IEnumerable<KeyValuePair<int, T>> SelectedEntries {
        get {
            foreach (IntRange range in this.selectedIndices) {
                for (int i = range.Start; i < range.End; i++) {
                    yield return new KeyValuePair<int, T>(i, this.SourceList[i]);
                }
            }
        }
    }

    /// <summary>
    /// Enumerates the selected indices
    /// </summary>
    public IEnumerable<IntRange> SelectedIndices => this.selectedIndices;

    public T this[int index] {
        get {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), index, "Index must be non-negative");
            if (index >= this.Count)
                throw new ArgumentOutOfRangeException(nameof(index), index, "Index out of range: beyond indexable selected item");

            int skip = 0, length;
            foreach (IntRange range in this.selectedIndices) {
                if (index < skip + (length = range.Length))
                    return this.SourceList[range.Start + (index - skip)];
                skip += length;
            }

            throw new InvalidOperationException("Internal error: selection index not resolved");
        }
    }

    public ListSelectionModel(ObservableList<T> sourceList) {
        this.SourceList = sourceList ?? throw new ArgumentNullException(nameof(sourceList));
        this.selectedIndices = new IntRangeUnion();
        this.SelectedItems = new ReadOnlyListDelegate(this);
        this.SourceList.BeforeItemsRemoved += this.SourceListBeforeItemsRemoved;
        this.SourceList.BeforeItemReplace += this.SourceListBeforeItemReplaced;
        this.SourceList.ItemMoved += this.SourceListOnItemMoved;
    }

    private void SourceListBeforeItemsRemoved(IObservableList<T> list, int index, int count) {
        this.DeselectRange(index, count);
    }

    private void SourceListBeforeItemReplaced(IObservableList<T> list, int index, T olditem, T newitem) {
        this.Deselect(index);
    }

    private void SourceListOnItemMoved(IObservableList<T> list, int oldindex, int newindex, T item) {
        this.Deselect(oldindex);
        this.Select(newindex);
    }

    public void Select(int index) => this.SelectRange(index, 1);

    public void SelectItem(T item) {
        int index = this.SourceList.IndexOf(item);
        if (index != -1) {
            this.Select(index);
        }
    }

    public void SelectItems(IEnumerable<T> items) => this.SelectSelectionForItems(items, true);
    public void SelectRange(int index, int count) => this.SetSelection(index, count, true);
    public void SelectRanges(IntRangeUnion union) => this.SetSelectionForRanges(union, true);
    public void Deselect(int index) => this.DeselectRange(index, 1);

    public void DeselectItem(T item) {
        int index = this.SourceList.IndexOf(item);
        if (index != -1) {
            this.Deselect(index);
        }
    }

    public void DeselectRange(int index, int count) => this.SetSelection(index, count, false);

    public void DeselectRanges(IntRangeUnion union) => this.SetSelectionForRanges(union, false);

    public void DeselectItems(IEnumerable<T> items) => this.SelectSelectionForItems(items, false);

    public bool IsSelected(int index) => this.selectedIndices.Contains(index);

    public bool? IsItemSelected(T item) {
        int index = this.SourceList.IndexOf(item);
        return index == -1 ? null : this.IsSelected(index);
    }

    public void SelectAll() => this.SelectRange(0, this.SourceList.Count);

    public void Clear() {
        if (this.SelectionChanged != null) {
            IList<IntRange> changedList = this.selectedIndices.ToList();
            this.selectedIndices.Clear();
            this.SelectionChanged?.Invoke(this, new ListSelectionModelChangedEventArgs(ReadOnlyCollection<IntRange>.Empty, changedList));
        }
        else {
            this.selectedIndices.Clear();
        }
    }

    public IntRangeUnion ToIntRangeUnion() {
        return this.selectedIndices.Clone();
    }

    private void SetSelection(int index, int count, bool select) {
        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(index), index, "Index must be non-negative");
        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count), count, "Count must be non-negative");
        if ((index + count) < index)
            throw new ArgumentException("Index+Count overflow");
        if ((index + count) > this.SourceList.Count)
            throw new ArgumentException("Index+Count exceeds maximum index within the source list");

        IntRange range = IntRange.FromLength(index, count);
        if (this.SelectionChanged == null) {
            if (select) {
                this.selectedIndices.Add(range);
            }
            else {
                this.selectedIndices.Remove(range);
            }
        }
        else {
            // variable name based on state: true = whatIsNotThere, false = whatIsThere 
            IntRangeUnion changedIndices = this.selectedIndices.GetPresenceUnion(range, !select);
            if (select) {
                this.selectedIndices.Add(range);
            }
            else {
                this.selectedIndices.Remove(range);
            }

            if (changedIndices.RangeCount > 0) {
                IList<IntRange> empty = ReadOnlyCollection<IntRange>.Empty;
                this.SelectionChanged?.Invoke(this, new ListSelectionModelChangedEventArgs(select ? changedIndices.ToList() : empty, select ? empty : changedIndices.ToList()));
            }
        }
    }

    private void SelectSelectionForItems(IEnumerable<T> items, bool select) {
        IntRangeUnion indices = new IntRangeUnion();
        foreach (T item in items) {
            int index = this.SourceList.IndexOf(item);
            if (index != -1) {
                indices.Add(index);
            }
        }

        this.SetSelectionForRanges(indices, select);
    }

    private void SetSelectionForRanges(IntRangeUnion ranges, bool select) {
        IntRangeUnion changedIndices = new IntRangeUnion();
        if (this.SelectionChanged != null) {
            foreach (IntRange range in ranges) {
                this.selectedIndices.GetPresenceUnion(changedIndices, range, !select);
                if (select) {
                    this.selectedIndices.Add(range);
                }
                else {
                    this.selectedIndices.Remove(range);
                }
            }

            if (changedIndices.RangeCount > 0) {
                IList<IntRange> empty = ReadOnlyCollection<IntRange>.Empty;
                this.SelectionChanged?.Invoke(this, new ListSelectionModelChangedEventArgs(select ? changedIndices.ToList() : empty, select ? empty : changedIndices.ToList()));
            }
        }
        else {
            foreach (IntRange range in ranges) {
                if (select) {
                    this.selectedIndices.Add(range);
                }
                else {
                    this.selectedIndices.Remove(range);
                }
            }
        }
    }

    private sealed class ReadOnlyListDelegate(ListSelectionModel<T> selection) : IReadOnlyList<T> {
        public int Count => selection.Count;

        public T this[int index] => selection[index];
        
        public IEnumerator<T> GetEnumerator() {
            foreach (IntRange range in selection.selectedIndices) {
                for (int i = range.Start; i < range.End; i++) {
                    yield return selection.SourceList[i];
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }
    }
}