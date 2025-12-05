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
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using PFXToolKitUI.Utils;
using PFXToolKitUI.Utils.Collections.Observable;
using PFXToolKitUI.Utils.Ranges;

namespace PFXToolKitUI.Interactivity.Selections;

/// <summary>
/// Manages a one dimensional list of selected indices
/// </summary>
/// <typeparam name="T">The type of item that is selectable</typeparam>
public sealed class ListSelectionModel<T> : IListSelectionModel<T> {
    private static readonly IList<T> EmptyList = ReadOnlyCollection<T>.Empty;
    private readonly HashSet<T> selectedItems;

    public int Count => this.selectedItems.Count;

    public IObservableList<T> SourceList { get; }

    public IReadOnlySet<T> SelectedItems => this.selectedItems;

    public T First {
        get {
            if (this.Count < 1)
                ThrowForEmptySelectionModel();
            return this.SourceList[SelectionUtils.GetIndexOfFirstSelectedItem(this.selectedItems, this.SourceList)];
        }
    }

    public T Last {
        get {
            if (this.Count < 1)
                ThrowForEmptySelectionModel();
            return this.SourceList[SelectionUtils.GetIndexOfLastSelectedItem(this.selectedItems, this.SourceList)];
        }
    }

    public T this[int index] => this.SourceList[this.GetRealIndex(index)];

    public event EventHandler<ListSelectionModelChangedEventArgs<T>>? SelectionChanged;

    public ListSelectionModel(IObservableList<T> sourceList) {
        this.SourceList = sourceList ?? throw new ArgumentNullException(nameof(sourceList));
        this.selectedItems = new HashSet<T>();

        // To ensure the selection model never contains non-existent items, we hook into the pre-remove
        // and pre-replace events, and deselect the item being removed or replaced
        this.SourceList.ValidateRemove += this.SourceListValidateRemove;
        this.SourceList.ValidateReplace += this.SourceListValidateReplaced;
    }

    public void SelectItem(T item) {
        if (this.selectedItems.Add(item)) {
            this.SelectionChanged?.Invoke(this, new ListSelectionModelChangedEventArgs<T>([item], EmptyList));
        }
    }

    public void SelectItems(IEnumerable<T> items) {
        EventHandler<ListSelectionModelChangedEventArgs<T>>? handlers = this.SelectionChanged;
        if (handlers != null) {
            List<T> added = this.selectedItems.UnionAddEx(items);
            if (added.Count > 0) {
                handlers(this, new ListSelectionModelChangedEventArgs<T>(added.AsReadOnly(), EmptyList));
            }
        }
        else {
            // Optimized path with no SelectionChanged handlers (i.e. initial setup)
            foreach (T item in items) {
                this.selectedItems.Add(item);
            }
        }
    }

    public void SelectItems(IList<T> items, int index, int count) {
        EventHandler<ListSelectionModelChangedEventArgs<T>>? handlers = this.SelectionChanged;
        if (handlers != null) {
            List<T> added = new List<T>(count);
            for (int i = 0; i < count; i++) {
                T item = items[index + i];
                if (this.selectedItems.Add(item)) {
                    added.Add(item);
                }
            }

            if (added.Count > 0) {
                handlers(this, new ListSelectionModelChangedEventArgs<T>(added.AsReadOnly(), EmptyList));
            }
        }
        else {
            // Optimized path with no SelectionChanged handlers (i.e. initial setup)
            for (int i = 0; i < count; i++) {
                this.selectedItems.Add(items[index + i]);
            }
        }
    }

    public void DeselectItem(T item) {
        if (this.selectedItems.Remove(item)) {
            this.SelectionChanged?.Invoke(this, new ListSelectionModelChangedEventArgs<T>(EmptyList, [item]));
        }
    }

    public void DeselectItems(IEnumerable<T> items) {
        EventHandler<ListSelectionModelChangedEventArgs<T>>? handlers = this.SelectionChanged;
        if (handlers != null) {
            List<T> removed = this.selectedItems.UnionRemoveEx(items);
            if (removed.Count > 0) {
                handlers(this, new ListSelectionModelChangedEventArgs<T>(EmptyList, removed.AsReadOnly()));
            }
        }
        else {
            // Optimized path with no SelectionChanged handlers (i.e. initial setup)
            foreach (T item in items) {
                this.selectedItems.Remove(item);
            }
        }
    }

    public IntegerSet<int> GetSelectedIndices() {
        IntegerSet<int> set = new IntegerSet<int>();
        foreach (T item in this.selectedItems) {
            int index = this.SourceList.IndexOf(item);
            if (index != -1) {
                set.Add(index);
            }
        }

        return set;
    }

    public List<KeyValuePair<int, T>> GetSelectedEntries() {
        IntegerSet<int> indices = this.GetSelectedIndices();
        List<KeyValuePair<int, T>> entries = new List<KeyValuePair<int, T>>(this.selectedItems.Count);
        foreach (IntegerRange<int> range in indices) {
            for (int i = range.Start; i < range.End; i++) {
                entries.Add(new KeyValuePair<int, T>(i, this.SourceList[i]));
            }
        }

        return entries;
    }

    public void DeselectItems(IList<T> items, int index, int count) {
        EventHandler<ListSelectionModelChangedEventArgs<T>>? handlers = this.SelectionChanged;
        if (handlers != null) {
            List<T> removed = new List<T>(count);
            for (int i = 0; i < count; i++) {
                T item = items[index + i];
                if (this.selectedItems.Remove(item)) {
                    removed.Add(item);
                }
            }

            if (removed.Count > 0) {
                handlers(this, new ListSelectionModelChangedEventArgs<T>(EmptyList, removed.AsReadOnly()));
            }
        }
        else {
            // Optimized path with no SelectionChanged handlers (i.e. initial setup)
            for (int i = 0; i < count; i++) {
                this.selectedItems.Remove(items[index + i]);
            }
        }
    }

    public bool IsSelected(T item) => this.selectedItems.Contains(item);

    public void SelectAll() => this.SelectItems(this.SourceList, 0, this.SourceList.Count);

    public void DeselectAll() {
        if (this.selectedItems.Count > 0) {
            EventHandler<ListSelectionModelChangedEventArgs<T>>? handlers = this.SelectionChanged;
            if (handlers != null) {
                List<T> changedList = this.selectedItems.ToList();
                this.selectedItems.Clear();
                handlers(this, new ListSelectionModelChangedEventArgs<T>(EmptyList, changedList.AsReadOnly()));
            }
            else {
                this.selectedItems.Clear();
            }
        }
    }

    public void SetSelection(T item) {
        this.DeselectAll();
        this.SelectItem(item);
    }

    public void SetSelection(IEnumerable<T> items) {
        this.DeselectAll();
        this.SelectItems(items);
    }

    /// <summary>
    /// Gets an index within <see cref="SourceList"/> from the given nth selected index
    /// </summary>
    /// <param name="index">The nth selected item index</param>
    /// <returns>The real index</returns>
    public int GetRealIndex(int index) {
        int count = this.Count;
        if (count == 0)
            ThrowForEmptySelectionModel();
        
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, count);
        return SelectionUtils.GetIndexOfNthSelectedItem(index, this.selectedItems, this.SourceList);
    }

    private void SourceListValidateRemove(IObservableList<T> list, int index, int count) {
        if (count == 1) {
            this.DeselectItem(list[index]);
        }
        else {
            this.DeselectItems(list, index, count);
        }
    }

    private void SourceListValidateReplaced(IObservableList<T> list, int index, T oldItem, T newItem) {
        this.DeselectItem(oldItem);
    }
    
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowForEmptySelectionModel() => throw new InvalidOperationException("Selection model is empty");
}