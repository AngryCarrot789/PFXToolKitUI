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
using PFXToolKitUI.Utils;
using PFXToolKitUI.Utils.Collections.Observable;

namespace PFXToolKitUI.Interactivity.Selections;

/// <summary>
/// Manages a hierarchy of selected items. Items must be equal by reference
/// </summary>
/// <typeparam name="T">The item type</typeparam>
public sealed class TreeSelectionModel<T> where T : class {
    private static readonly IList<T> EmptyList = ReadOnlyCollection<T>.Empty;

    private readonly HashSet<T> selectedItems;

    /// <summary>Gets the list of top-level items</summary>
    public ObservableList<T> TopLevelItems { get; }

    /// <summary>Gets a function that gets the parent of a node</summary>
    public Func<T, T?> GetParentFunction { get; }

    /// <summary>Gets a function that enumerates the child nodes of an node</summary>
    public Func<T, IEnumerable<T>> GetChildrenFunction { get; }

    /// <summary>Enumerates the selected items</summary>
    public IEnumerable<T> SelectedItems => this.selectedItems;

    /// <summary>Returns the amount of selected items</summary>
    public int Count => this.selectedItems.Count;

    /// <summary>An event fired when the selection state changes</summary>
    public event EventHandler<ChangedEventArgs>? SelectionChanged;

    public TreeSelectionModel(ObservableList<T> topLevelItems, Func<T, T?> getParentFunction, Func<T, IEnumerable<T>> getChildrenFunction) {
        this.TopLevelItems = topLevelItems ?? throw new ArgumentNullException(nameof(topLevelItems));
        this.GetParentFunction = getParentFunction ?? throw new ArgumentNullException(nameof(getParentFunction));
        this.GetChildrenFunction = getChildrenFunction ?? throw new ArgumentNullException(nameof(getChildrenFunction));
        this.selectedItems = new HashSet<T>(EqualityComparer<T>.Create(ReferenceEquals, t => t.GetHashCode()));
    }

    /// <summary>
    /// Enumerates every single items in the tree, selected or not
    /// </summary>
    /// <returns></returns>
    public List<T> GetItemsRecursive() {
        List<T> list = new List<T>();
        foreach (T item in this.TopLevelItems) {
            list.Add(item);
            this.GetItemsRecursive(item, list);
        }

        return list;
    }

    public void GetItemsRecursive(T item, List<T> list) {
        foreach (T node in this.GetChildrenFunction(item)) {
            list.Add(node);
            this.GetItemsRecursive(node, list);
        }
    }

    /// <summary>
    /// Select a single item
    /// </summary>
    public void Select(T item) {
        if (this.selectedItems.Add(item)) {
            this.SelectionChanged?.Invoke(this, new ChangedEventArgs(new SingletonList<T>(item), EmptyList));
        }
    }

    /// <summary>
    /// Select multiple items
    /// </summary>
    public void SelectItems(IEnumerable<T> items) {
        List<T> added = this.selectedItems.UnionAddEx(items);
        if (added.Count > 0) {
            this.SelectionChanged?.Invoke(this, new ChangedEventArgs(added.AsReadOnly(), EmptyList));
        }
    }

    /// <summary>
    /// Deselect a single item
    /// </summary>
    public void Deselect(T item) {
        if (this.selectedItems.Remove(item)) {
            this.SelectionChanged?.Invoke(this, new ChangedEventArgs(EmptyList, new SingletonList<T>(item)));
        }
    }

    /// <summary>
    /// Deselects multiple items
    /// </summary>
    public void DeselectItems(IEnumerable<T> items) {
        List<T> removed = this.selectedItems.UnionRemoveEx(items);
        if (removed.Count > 0) {
            this.SelectionChanged?.Invoke(this, new ChangedEventArgs(EmptyList, removed.AsReadOnly()));
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
        List<T> items = this.GetItemsRecursive();
        List<T> added = this.selectedItems.UnionAddEx(items);
        if (added.Count > 0) {
            this.SelectionChanged?.Invoke(this, new ChangedEventArgs(added.AsReadOnly(), EmptyList));
        }
    }

    /// <summary>
    /// Selects all items
    /// </summary>
    public void SelectTopLevelItems() {
        this.SelectItems(this.TopLevelItems);
    }

    /// <summary>
    /// Clears the selection and selects the given item, as a single operation
    /// </summary>
    /// <param name="item">The item to select</param>
    public void SetSelection(T item) {
        bool isSelected = this.selectedItems.Remove(item);
        List<T> removed = this.selectedItems.ToList();
        this.selectedItems.Clear();
        this.selectedItems.Add(item);

        this.SelectionChanged?.Invoke(this, new ChangedEventArgs(isSelected ? EmptyList : new SingletonList<T>(item), removed));
    }

    /// <summary>
    /// Clears the selection and selects the given items, as a single operation
    /// </summary>
    /// <param name="items">The items to select</param>
    public void SetSelection(IEnumerable<T> items) {
        HashSet<T> newSet = new HashSet<T>(items);
        List<T> removed = this.selectedItems.Except(newSet).ToList();
        List<T> added = newSet.Except(this.selectedItems).ToList();

        foreach (T t in removed)
            this.selectedItems.Remove(t);
        foreach (T t in added)
            this.selectedItems.Add(t);

        if (added.Count > 0 || removed.Count > 0)
            this.SelectionChanged?.Invoke(this, new ChangedEventArgs(added.AsReadOnly(), removed.AsReadOnly()));
    }

    /// <summary>
    /// Deselects all items
    /// </summary>
    public void Clear() {
        if (this.selectedItems.Count > 0) {
            IList<T> items = new ReadOnlyCollection<T>(this.selectedItems.ToList());
            this.selectedItems.Clear();
            this.SelectionChanged?.Invoke(this, new ChangedEventArgs(EmptyList, items));
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