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
using System.Diagnostics;
using PFXToolKitUI.Utils;
using PFXToolKitUI.Utils.Collections.Observable;

namespace PFXToolKitUI.Interactivity.Selections;

/// <summary>
/// Manages a hierarchy of selected items. Items must be equal by reference.
/// <para>
/// The tree structure should be such that:
/// <list type="bullet">
/// <item><description>There's a singular "root" item, which only acts as a container object and cannot be seen at all</description></item>
/// <item><description>The <see cref="GetParentFunction"/> returns the direct parent, which may be compared to <see cref="RootItem"/></description></item>
/// </list>
/// </para>
/// </summary>
/// <typeparam name="T">The item type</typeparam>
public sealed class TreeSelectionModel<T> where T : class {
    private static readonly IList<T> EmptyList = ReadOnlyCollection<T>.Empty;

    private readonly HashSet<T> selectedItems;

    /// <summary>
    /// Gets the root item
    /// </summary>
    public T RootItem { get; }

    /// <summary>
    /// Gets a function that returns true when an item exists in the tree, even if it
    /// has a parent. E.g. the item's tree "manager" reference is not set, so it's not valid)
    /// </summary>
    public Func<T, bool> IsItemInTreeFunction { get; }

    /// <summary>
    /// Gets a function that gets the parent of a node
    /// </summary>
    public Func<T, T?> GetParentFunction { get; }

    /// <summary>
    /// Gets a function that enumerates the child nodes of an node
    /// </summary>
    public Func<T, IObservableList<T>?> GetChildrenFunction { get; }

    /// <summary>
    /// Enumerates the selected items.
    /// </summary>
    public IEnumerable<T> SelectedItems => this.selectedItems;

    /// <summary>
    /// Returns the amount of selected items
    /// </summary>
    public int Count => this.selectedItems.Count;

    /// <summary>
    /// Returns true when we only have a single selected item
    /// </summary>
    public bool HasOneSelectedItem => this.Count == 1;

    /// <summary>
    /// An event fired when the selection state changes
    /// </summary>
    public event EventHandler<ChangedEventArgs>? SelectionChanged;

    private readonly ObservableListMultipleItemsEventHandler<T> ItemsRemovedHandler;
    private readonly ObservableListReplaceEventHandler<T> ItemReplacedHandler;
    private readonly Func<T, bool> CanSelectItem;

    public TreeSelectionModel(T rootItem, Func<T, bool> isItemInTree, Func<T, T?> getParentFunction, Func<T, IObservableList<T>?> getChildrenFunction) {
        this.RootItem = rootItem ?? throw new ArgumentNullException(nameof(rootItem));
        this.IsItemInTreeFunction = isItemInTree ?? throw new ArgumentNullException(nameof(isItemInTree));
        this.GetParentFunction = getParentFunction ?? throw new ArgumentNullException(nameof(getParentFunction));
        this.GetChildrenFunction = getChildrenFunction ?? throw new ArgumentNullException(nameof(getChildrenFunction));
        this.CanSelectItem = item => {
            Debug.Assert(item != null, "Attempt to use null item in " + nameof(TreeSelectionModel<T>));
            return item != null! && item != this.RootItem && this.IsItemInTreeFunction(item);
        };

        this.ItemsRemovedHandler = this.OnItemsRemoved;
        this.ItemReplacedHandler = this.OnItemReplaced;
        this.selectedItems = new HashSet<T>(EqualityComparer<T>.Create(ReferenceEquals, t => t.GetHashCode()));

        IObservableList<T>? list = getChildrenFunction(rootItem)!;
        list.ItemsRemoved += this.ItemsRemovedHandler;
        list.ItemReplaced += this.ItemReplacedHandler;
    }

    /// <summary>
    /// Select a single item
    /// </summary>
    public void Select(T item) {
        if (this.CanSelectItem(item) && this.selectedItems.Add(item)) {
            this.RaiseSelectionChanged(new SingletonList<T>(item), EmptyList);
        }
    }

    /// <summary>
    /// Select multiple items
    /// </summary>
    public void SelectItems(IEnumerable<T> items) {
        List<T> added = this.selectedItems.UnionAddEx(items.Where(this.CanSelectItem));
        if (added.Count > 0) {
            this.RaiseSelectionChanged(added.AsReadOnly(), EmptyList);
        }
    }

    /// <summary>
    /// Deselect a single item
    /// </summary>
    public void Deselect(T item) {
        if (this.selectedItems.Remove(item)) {
            this.RaiseSelectionChanged(EmptyList, new SingletonList<T>(item));
        }
    }

    /// <summary>
    /// Deselects multiple items
    /// </summary>
    public void DeselectItems(IEnumerable<T> items) {
        List<T> removed = this.selectedItems.UnionRemoveEx(items);
        if (removed.Count > 0) {
            this.RaiseSelectionChanged(EmptyList, removed.AsReadOnly());
        }
    }

    /// <summary>
    /// Check if the item at the index is selected
    /// </summary>
    /// <returns></returns>
    public bool IsSelected(T item) => this.selectedItems.Contains(item);

    /// <summary>
    /// Selects all items
    /// </summary>
    public void SelectAll() {
        List<T> allItems = new List<T>();
        Recurse(this.RootItem, allItems);
        Debug.Assert(allItems.All(this.CanSelectItem));

        List<T> added = this.selectedItems.UnionAddEx(allItems);
        if (added.Count > 0) {
            this.RaiseSelectionChanged(added.AsReadOnly(), EmptyList);
        }

        return;

        void Recurse(T item, List<T> sink) {
            IObservableList<T>? items = this.GetChildrenFunction(item);
            if (items != null) {
                for (int i = 0; i < items.Count; i++) {
                    T node = items[i];
                    sink.Add(node);
                    Recurse(node, sink);
                }
            }
        }
    }

    /// <summary>
    /// Selects all items
    /// </summary>
    public void SelectTopLevelItems() {
        this.SelectItems(this.GetChildrenFunction(this.RootItem)!);
    }

    /// <summary>
    /// Clears the selection and selects the given item, as a single operation
    /// </summary>
    /// <param name="item">The item to select</param>
    public void SetSelection(T item) {
        bool wasSelected = this.selectedItems.Remove(item);
        List<T> removed = this.selectedItems.ToList();
        this.selectedItems.Clear();

        if (this.CanSelectItem(item)) {
            this.selectedItems.Add(item);
        }

        IList<T> added = wasSelected ? EmptyList : this.selectedItems.ToList();
        if (added.Count > 0 || removed.Count > 0) {
            this.RaiseSelectionChanged(added, removed);
        }
    }

    /// <summary>
    /// Clears the selection and selects the given items, as a single operation
    /// </summary>
    /// <param name="items">The items to select</param>
    public void SetSelection(IEnumerable<T> items) {
        HashSet<T> newSet = new HashSet<T>(items.Where(this.CanSelectItem));
        List<T> removed = this.selectedItems.Except(newSet).ToList();
        List<T> added = newSet.Except(this.selectedItems).ToList();

        foreach (T t in removed)
            this.selectedItems.Remove(t);
        foreach (T t in added)
            this.selectedItems.Add(t);

        if (added.Count > 0 || removed.Count > 0) {
            this.RaiseSelectionChanged(added.AsReadOnly(), removed.AsReadOnly());
        }
    }

    /// <summary>
    /// Deselects all items
    /// </summary>
    public void Clear() {
        if (this.selectedItems.Count > 0) {
            IList<T> items = new ReadOnlyCollection<T>(this.selectedItems.ToList());
            this.selectedItems.Clear();
            this.RaiseSelectionChanged(EmptyList, items);
        }
    }

    /// <summary>
    /// Deselects all selected items that exist in the item's hierarchy
    /// </summary>
    /// <param name="item">The item whose hierarchy should be processed</param>
    /// <param name="deselectSelf">True to deselect the item itself, False to only deselect its descendents</param>
    public void DeselectHierarchy(T item, bool deselectSelf = false) {
        List<T> deselected;
        if (deselectSelf) {
            deselected = new List<T>(32);
            this.RecursiveDeselect(item, deselected);
        }
        else {
            IObservableList<T>? items = this.GetChildrenFunction(item);
            if (items == null) {
                return;
            }

            deselected = new List<T>(32);
            foreach (T node in items) {
                this.RecursiveDeselect(node, deselected);
            }
        }

        if (deselected.Count > 0) {
            this.RaiseSelectionChanged(EmptyList, deselected);
        }
    }

    private void RaiseSelectionChanged(IList<T> addedItems, IList<T> removedItems) {
        Debug.Assert(addedItems.Count > 0 || removedItems.Count > 0);
        for (int i = 0; i < addedItems.Count; i++) {
            IObservableList<T>? list = this.GetChildrenFunction(addedItems[i]);
            if (list != null) {
                list.ItemsRemoved += this.ItemsRemovedHandler;
                list.ItemReplaced += this.ItemReplacedHandler;
            }
        }

        for (int i = 0; i < removedItems.Count; i++) {
            IObservableList<T>? list = this.GetChildrenFunction(removedItems[i]);
            if (list != null) {
                list.ItemsRemoved -= this.ItemsRemovedHandler;
                list.ItemReplaced -= this.ItemReplacedHandler;
            }
        }

        this.SelectionChanged?.Invoke(this, new ChangedEventArgs(addedItems, removedItems));
    }

    private void OnItemsRemoved(IObservableList<T> list, int index, IList<T> items) {
        if (this.selectedItems.Count > 0) {
            List<T> deselect = new List<T>(32);
            for (int i = 0; i < items.Count; i++) {
                this.RecursiveDeselect(items[i], deselect);
            }

            if (deselect.Count > 0) {
                this.RaiseSelectionChanged(EmptyList, deselect);
            }
        }
    }

    private void OnItemReplaced(IObservableList<T> list, int index, T olditem, T newitem) {
        if (this.selectedItems.Count > 0) {
            List<T> deselect = new List<T>(32);
            this.RecursiveDeselect(olditem, deselect);
            if (deselect.Count > 0) {
                this.RaiseSelectionChanged(EmptyList, deselect);
            }
        }
    }

    private void RecursiveDeselect(T removedItem, List<T> sink) {
        if (this.selectedItems.Remove(removedItem)) {
            sink.Add(removedItem);
        }

        IObservableList<T>? children = this.GetChildrenFunction(removedItem);
        if (children != null) {
            foreach (T node in children) {
                this.RecursiveDeselect(node, sink);
            }
        }
    }

    /// <summary>
    /// Gets an enumerator for the selected items
    /// </summary>
    /// <returns></returns>
    public HashSet<T>.Enumerator GetEnumerator() => this.selectedItems.GetEnumerator();

    public readonly struct ChangedEventArgs(IList<T> addedItems, IList<T> removedItems) {
        /// <summary>
        /// The ranges containing models that are now selected
        /// </summary>
        public IList<T> AddedItems { get; } = addedItems;

        /// <summary>
        /// The ranges containing models that are no longer selected
        /// </summary>
        public IList<T> RemovedItems { get; } = removedItems;
    }
}