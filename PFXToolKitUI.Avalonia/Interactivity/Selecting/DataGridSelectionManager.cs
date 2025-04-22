// 
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

using System.Collections;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using PFXToolKitUI.Interactivity;

namespace PFXToolKitUI.Avalonia.Interactivity.Selecting;

public class DataGridSelectionManager<TModel> : IListSelectionManager<TModel> where TModel : class {
    public DataGrid? DataGrid {
        get => this.dataGrid;
        set {
            DataGrid? oldGrid = this.dataGrid;
            ReadOnlyCollection<TModel>? oldItems = null;
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
                    ReadOnlyCollection<TModel>? newItems = AsReadOnly(CastSelectedItems(value).ToList());
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

    public IEnumerable<TModel> SelectedItems => this.dataGrid != null ? CastSelectedItems(this.dataGrid) : ImmutableArray<TModel>.Empty;

    public IList<TModel> SelectedItemList => this.castingSelectionList ?? ReadOnlyCollection<TModel>.Empty;

    public event SelectionChangedEventHandler<TModel>? SelectionChanged;
    public event SelectionClearedEventHandler<TModel>? SelectionCleared;
    public event LightSelectionChangedEventHandler<TModel>? LightSelectionChanged;

    private IList<TModel>? castingSelectionList;
    private DataGrid? dataGrid;

    public DataGridSelectionManager() {
    }

    public DataGridSelectionManager(DataGrid dataGridView) {
        this.DataGrid = dataGridView;
    }
    
    private void OnDataGridSelectionChanged(object? sender, SelectionChangedEventArgs e) {
        this.ProcessTreeSelection(e.RemovedItems, e.AddedItems);
    }

    internal void ProcessTreeSelection(IList? oldItems, IList? newItems) {
        ReadOnlyCollection<TModel>? oldList = oldItems?.Cast<TModel>().ToList().AsReadOnly();
        ReadOnlyCollection<TModel>? newList = newItems?.Cast<TModel>().ToList().AsReadOnly();
        if (oldList?.Count > 0 || newList?.Count > 0) {
            this.OnSelectionChanged(oldList, newList);
        }
    }

    private void OnSelectionChanged(ReadOnlyCollection<TModel>? oldList, ReadOnlyCollection<TModel>? newList) {
        if (ReferenceEquals(oldList, newList) || (oldList?.Count < 1 && newList?.Count < 1)) {
            return;
        }

        this.SelectionChanged?.Invoke(this, oldList, newList);
        this.LightSelectionChanged?.Invoke(this);
    }

    public bool IsSelected(TModel item) {
        if (this.dataGrid == null)
            return false;
        return this.dataGrid.SelectedItems.Contains(item);
    }

    private void OnSelectionCleared() {
        this.SelectionCleared?.Invoke(this);
        this.LightSelectionChanged?.Invoke(this);
    }

    public void SetSelection(TModel item) {
        if (this.dataGrid == null) {
            return;
        }

        this.dataGrid.SelectedItems.Clear();
        this.Select(item);
    }

    public void SetSelection(IEnumerable<TModel> items) {
        if (this.dataGrid == null) {
            return;
        }

        this.dataGrid.SelectedItems.Clear();
        this.Select(items);
    }

    public void Select(TModel item) {
        if (this.dataGrid == null)
            return;
        if (!this.dataGrid.SelectedItems.Contains(item))
            this.dataGrid.SelectedItems.Add(item);
    }

    public void Select(IEnumerable<TModel> items) {
        if (this.dataGrid == null) {
            return;
        }

        foreach (TModel item in items.ToList()) {
            this.Select(item);
        }
    }

    public void Unselect(TModel item) {
        if (this.dataGrid == null) {
            return;
        }

        this.dataGrid.SelectedItems.Remove(item);
    }

    public void Unselect(IEnumerable<TModel> items) {
        if (this.dataGrid == null) {
            return;
        }

        foreach (TModel item in items) {
            this.Unselect(item);
        }
    }

    public void ToggleSelected(TModel item) {
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

    private static IEnumerable<TModel> CastSelectedItems(DataGrid tree) => tree.SelectedItems.Cast<TModel>();
    private static ReadOnlyCollection<TModel>? AsReadOnly(List<TModel>? list) => list != null && list.Count > 0 ? list.AsReadOnly() : null;

    private class CastingList : IList<TModel> {
        private readonly DataGrid dataGrid;

        public int Count => this.dataGrid.SelectedItems.Count;
        public bool IsReadOnly => this.dataGrid.SelectedItems.IsReadOnly;

        public TModel this[int index] {
            get => (TModel) this.dataGrid.SelectedItems[index]!;
            set => this.dataGrid.SelectedItems[index] = value;
        }
        
        public CastingList(DataGrid dataGrid) {
            this.dataGrid = dataGrid;
        }

        public IEnumerator<TModel> GetEnumerator() => CastSelectedItems(this.dataGrid).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public void Add(TModel item) {
            if (!this.dataGrid.SelectedItems.Contains(item))
                this.dataGrid.SelectedItems.Add(item);
        }

        public void Clear() {
            this.dataGrid.SelectedItems.Clear();
        }

        public bool Contains(TModel item) {
            return this.dataGrid.SelectedItems.Contains(item);
        }

        public void CopyTo(TModel[] array, int arrayIndex) {
            foreach (TModel item in this.dataGrid.SelectedItems)
                array[arrayIndex++] = item;
        }

        public bool Remove(TModel item) {
            if (!this.dataGrid.SelectedItems.Contains(item))
                return false;
            this.dataGrid.SelectedItems.Remove(item);
            return true;
        }
        
        public int IndexOf(TModel item) {
            return this.dataGrid.SelectedItems.IndexOf(item);
        }

        public void Insert(int index, TModel item) {
            this.dataGrid.SelectedItems.Insert(index, item);
        }

        public void RemoveAt(int index) {
            this.dataGrid.SelectedItems.RemoveAt(index);
        }
    }
}