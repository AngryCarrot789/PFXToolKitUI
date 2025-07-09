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
using Avalonia.Collections;
using Avalonia.Controls;
using PFXToolKitUI.Interactivity;

namespace PFXToolKitUI.Avalonia.Interactivity.Selecting;

/// <summary>
/// A selection manager for a tree view
/// </summary>
/// <typeparam name="T">The type of item the tree view makes selectable</typeparam>
public class TreeViewSelectionManager<T> : IListSelectionManager<T> where T : class {
    public TreeView? TreeView {
        get => this.myTree;
        set {
            TreeView? oldTree = this.myTree;
            ReadOnlyCollection<T>? oldItems = null;
            if (oldTree != null) {
                this.castingSelectionList = null;
                if (value == null) {
                    // Tree is being set to null; clear selection first
                    oldTree.SelectedItems.Clear();
                    oldTree.SelectionChanged -= this.OnSelectionChanged;
                    this.myTree = null;
                    return;
                }

                // Since there's an old and new tree, we need to first say cleared then selection
                // changed from old selection to new selection, even if they're the exact same
                if ((oldItems = AsReadOnly(CastSelectedItems(oldTree).ToList())) != null && this.KeepSelectedItemsFromOldTree)
                    oldTree.SelectedItems.Clear();
                oldTree.SelectionChanged -= this.OnSelectionChanged;
            }

            this.myTree = value;
            if (value != null) {
                this.castingSelectionList = new CastingList(value);
                value.SelectionChanged += this.OnSelectionChanged;
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

    public int Count => this.myTree?.SelectedItems.Count ?? 0;

    /// <summary>
    /// Specifies whether to move the old tree's selected items to the new tree when our <see cref="TreeView"/> property changes. True by default.
    /// <br/>
    /// <para>
    /// When true, the old tree's items are saved then the tree is cleared, and the new tree's selection becomes that saved list
    /// </para>
    /// <para>
    /// When false, the <see cref="SelectionCleared"/> event is raised (if the old tree is valid) and then the selection changed event is raised on the new tree's pre-existing selected items.
    /// </para>
    /// </summary>
    public bool KeepSelectedItemsFromOldTree { get; set; } = true;

    public IEnumerable<T> SelectedItems => this.myTree != null ? CastSelectedItems(this.myTree) : ImmutableArray<T>.Empty;

    public IList<T> SelectedItemList => this.castingSelectionList ?? ReadOnlyCollection<T>.Empty;

    public event SelectionChangedEventHandler<T>? SelectionChanged;
    public event SelectionClearedEventHandler<T>? SelectionCleared;
    public event LightSelectionChangedEventHandler<T>? LightSelectionChanged;

    private IList<T>? castingSelectionList;
    private TreeView? myTree;

    public TreeViewSelectionManager() {
    }

    public TreeViewSelectionManager(TreeView treeView) {
        this.TreeView = treeView;
    }

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e) {
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

        this.SelectionChanged?.Invoke(this, oldList, newList);
        this.LightSelectionChanged?.Invoke(this);
    }

    public bool IsSelected(T item) {
        if (this.myTree == null)
            return false;
        return this.myTree.SelectedItems.Contains(item);
    }

    private void OnSelectionCleared() {
        this.SelectionCleared?.Invoke(this);
        this.LightSelectionChanged?.Invoke(this);
    }

    public void SetSelection(T item) {
        if (this.myTree == null) {
            return;
        }

        this.myTree.SelectedItems.Clear();
        this.Select(item);
    }

    public void SetSelection(IEnumerable<T> items) {
        if (this.myTree == null) {
            return;
        }

        this.myTree.SelectedItems.Clear();
        this.Select(items);
    }

    public void Select(T item) {
        if (this.myTree == null)
            return;
        if (!this.myTree.SelectedItems.Contains(item))
            this.myTree.SelectedItems.Add(item);
    }

    public void Select(IEnumerable<T> items) {
        if (this.myTree == null) {
            return;
        }

        if (this.myTree.SelectedItems is AvaloniaList<T> selectedItems) {
            selectedItems.AddRange(items.ToList());
        }
        else {
            foreach (T item in items.ToList()) {
                this.Select(item);
            }
        }
    }

    public void Unselect(T item) {
        if (this.myTree == null) {
            return;
        }

        this.myTree.SelectedItems.Remove(item);
    }

    public void Unselect(IEnumerable<T> items) {
        if (this.myTree == null) {
            return;
        }

        if (this.myTree.SelectedItems is AvaloniaList<T> selectedItems) {
            selectedItems.RemoveAll(items.ToList());
        }
        else {
            foreach (T item in items.ToList()) {
                this.Unselect(item);
            }
        }
    }

    public void ToggleSelected(T item) {
        if (this.IsSelected(item))
            this.Unselect(item);
        else
            this.Select(item);
    }

    public void Clear() {
        this.myTree?.SelectedItems.Clear();
    }

    public void SelectAll() {
        this.myTree?.SelectAll();
    }

    private static IEnumerable<T> CastSelectedItems(TreeView tree) => tree.SelectedItems.Cast<T>();
    private static ReadOnlyCollection<T>? AsReadOnly(List<T>? list) => list != null && list.Count > 0 ? list.AsReadOnly() : null;

    private class CastingList : IList<T> {
        private readonly TreeView dataGrid;

        public int Count => this.dataGrid.SelectedItems.Count;
        public bool IsReadOnly => this.dataGrid.SelectedItems.IsReadOnly;

        public T this[int index] {
            get => (T) this.dataGrid.SelectedItems[index]!;
            set => this.dataGrid.SelectedItems[index] = value;
        }

        public CastingList(TreeView dataGrid) {
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