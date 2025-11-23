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
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using PFXToolKitUI.Interactivity;
using PFXToolKitUI.Utils.Events;

namespace PFXToolKitUI.Avalonia.Interactivity.Selecting;

/// <summary>
/// A selection manager for a data grid
/// </summary>
/// <typeparam name="T">The type of item the data grid makes selectable</typeparam>
public class DataGridSelectionManager<T> : IListSelectionManager<T> where T : class {
    public DataGrid? DataGrid {
        get => this.dataGrid;
        set {
            DataGrid? oldGrid = this.dataGrid;
            ReadOnlyCollection<T>? oldItems = null;
            if (oldGrid != null) {
                this.castingSelectionList = null;
                if (value == null) {
                    // Tree is being set to null; clear selection first
                    oldGrid.SelectedItems.Clear();
                    oldGrid.SelectionChanged -= this.OnDataGridSelectionChanged;
                    this.dataGrid = null;
                    return;
                }

                // Since there's an old and new tree, we need to first say cleared then selection
                // changed from old selection to new selection, even if they're the exact same
                if ((oldItems = AsReadOnly(CastSelectedItems(oldGrid).ToList())) != null && this.KeepSelectedItemsFromOldTree)
                    oldGrid.SelectedItems.Clear();
                oldGrid.SelectionChanged -= this.OnDataGridSelectionChanged;
            }

            this.dataGrid = value;
            if (value != null) {
                this.castingSelectionList = new CastingList(value);
                value.SelectionChanged += this.OnDataGridSelectionChanged;
                if (this.KeepSelectedItemsFromOldTree) {
                    if (oldItems != null)
                        this.Select(oldItems);
                }
                else {
                    ReadOnlyCollection<T>? newItems = AsReadOnly(CastSelectedItems(value).ToList());
                    this.OnSelectionChanged(oldItems, newItems);
                }
            }
        }
    }

    public int Count => this.dataGrid?.SelectedItems.Count ?? 0;

    /// <summary>
    /// Specifies whether to move the old tree's selected items to the new tree when our <see cref="DataGrid"/> property changes. True by default.
    /// <br/>
    /// <para>
    /// When true, the old tree's items are saved then the tree is cleared, and the new tree's selection becomes that saved list
    /// </para>
    /// <para>
    /// When false, the <see cref="SelectionCleared"/> event is raised (if the old tree is valid) and then the selection changed event is raised on the new tree's pre-existing selected items.
    /// </para>
    /// </summary>
    public bool KeepSelectedItemsFromOldTree { get; set; } = true;

    public IEnumerable<T> SelectedItems => this.dataGrid != null ? CastSelectedItems(this.dataGrid) : ImmutableArray<T>.Empty;

    public IList<T> SelectedItemList => this.castingSelectionList ?? ReadOnlyCollection<T>.Empty;

    public event EventHandler<ValueChangedEventArgs<IList<T>?>>? SelectionChanged;
    public event EventHandler? SelectionCleared;
    public event EventHandler? LightSelectionChanged;

    private IList<T>? castingSelectionList;
    private DataGrid? dataGrid;

    public DataGridSelectionManager() {
    }

    public DataGridSelectionManager(DataGrid dataGridView) {
        this.DataGrid = dataGridView;
    }
    
    private void OnDataGridSelectionChanged(object? sender, SelectionChangedEventArgs e) {
        if (sender == e.Source)
            this.ProcessTreeSelection(e.RemovedItems, e.AddedItems);
    }

    internal void ProcessTreeSelection(IList? oldItems, IList? newItems) {
        ReadOnlyCollection<T>? oldList = oldItems?.Cast<T>().ToList().AsReadOnly();
        ReadOnlyCollection<T>? newList = newItems?.Cast<T>().ToList().AsReadOnly();
        if (oldList?.Count > 0 || newList?.Count > 0) {
            this.OnSelectionChanged(oldList, newList);
        }
    }

    private void OnSelectionChanged(ReadOnlyCollection<T>? oldList, ReadOnlyCollection<T>? newList) {
        if (ReferenceEquals(oldList, newList) || (oldList?.Count < 1 && newList?.Count < 1)) {
            return;
        }

        this.SelectionChanged?.Invoke(this, new ValueChangedEventArgs<IList<T>?>(oldList, newList));
        this.LightSelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool IsSelected(T item) {
        if (this.dataGrid == null)
            return false;
        return this.dataGrid.SelectedItems.Contains(item);
    }

    private void OnSelectionCleared() {
        this.SelectionCleared?.Invoke(this, EventArgs.Empty);
        this.LightSelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetSelection(T item) {
        if (this.dataGrid == null) {
            return;
        }

        this.dataGrid.SelectedItems.Clear();
        this.Select(item);
    }

    public void SetSelection(IEnumerable<T> items) {
        if (this.dataGrid == null) {
            return;
        }

        this.dataGrid.SelectedItems.Clear();
        this.Select(items);
    }

    public void Select(T item) {
        if (this.dataGrid == null)
            return;
        if (!this.dataGrid.SelectedItems.Contains(item))
            this.dataGrid.SelectedItems.Add(item);
    }

    public void Select(IEnumerable<T> items) {
        if (this.dataGrid == null) {
            return;
        }

        foreach (T item in items.ToList()) {
            this.Select(item);
        }
    }

    public void Unselect(T item) {
        if (this.dataGrid == null) {
            return;
        }

        this.dataGrid.SelectedItems.Remove(item);
    }

    public void Unselect(IEnumerable<T> items) {
        if (this.dataGrid == null) {
            return;
        }

        foreach (T item in items) {
            this.Unselect(item);
        }
    }

    public void ToggleSelected(T item) {
        if (this.IsSelected(item))
            this.Unselect(item);
        else
            this.Select(item);
    }

    public void Clear() {
        this.dataGrid?.SelectedItems.Clear();
    }

    public void SelectAll() {
        this.dataGrid?.SelectAll();
    }

    private static IEnumerable<T> CastSelectedItems(DataGrid tree) => tree.SelectedItems.Cast<T>();
    private static ReadOnlyCollection<T>? AsReadOnly(List<T>? list) => list != null && list.Count > 0 ? list.AsReadOnly() : null;

    private class CastingList : IList<T> {
        private readonly DataGrid dataGrid;

        public int Count => this.dataGrid.SelectedItems.Count;
        public bool IsReadOnly => this.dataGrid.SelectedItems.IsReadOnly;

        public T this[int index] {
            get => (T) this.dataGrid.SelectedItems[index]!;
            set => this.dataGrid.SelectedItems[index] = value;
        }
        
        public CastingList(DataGrid dataGrid) {
            this.dataGrid = dataGrid;
        }

        public IEnumerator<T> GetEnumerator() => CastSelectedItems(this.dataGrid).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public void Add(T item) {
            if (!this.dataGrid.SelectedItems.Contains(item))
                this.dataGrid.SelectedItems.Add(item);
        }

        public void Clear() {
            this.dataGrid.SelectedItems.Clear();
        }

        public bool Contains(T item) {
            return this.dataGrid.SelectedItems.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex) {
            foreach (T item in this.dataGrid.SelectedItems)
                array[arrayIndex++] = item;
        }

        public bool Remove(T item) {
            if (!this.dataGrid.SelectedItems.Contains(item))
                return false;
            this.dataGrid.SelectedItems.Remove(item);
            return true;
        }
        
        public int IndexOf(T item) {
            return this.dataGrid.SelectedItems.IndexOf(item);
        }

        public void Insert(int index, T item) {
            this.dataGrid.SelectedItems.Insert(index, item);
        }

        public void RemoveAt(int index) {
            this.dataGrid.SelectedItems.RemoveAt(index);
        }
    }
}