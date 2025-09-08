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
using System.Collections.Specialized;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Controls.Selection;
using PFXToolKitUI.Utils.Collections.Observable;

namespace PFXToolKitUI.Avalonia.Utils;

public sealed class ObservableListBoxSelectionHandler<T> where T : class {
    private readonly ObservableList<T> sourceList;
    private readonly ListBox listBox;
    private readonly Func<ListBoxItem, T> toModel;
    private readonly Func<T, ListBoxItem> fromModel;
    private bool isUpdatingControl, isUpdatingModel;

    public ObservableListBoxSelectionHandler(ObservableList<T> sourceList, ListBox listBox, Func<ListBoxItem, T> toModel, Func<T, ListBoxItem> fromModel) {
        this.sourceList = sourceList;
        this.listBox = listBox;
        this.toModel = toModel;
        this.fromModel = fromModel;

        this.listBox.Selection.Clear();
        this.OnModelItemsAdded(sourceList, 0, sourceList);

        this.listBox.SelectionChanged += this.OnListBoxSelectionChanged;
        this.listBox.Items.CollectionChanged += this.OnListBoxCollectionChanged;
        this.sourceList.ItemsAdded += this.OnModelItemsAdded;
        this.sourceList.ItemsRemoved += this.OnModelItemsRemoved;
        this.sourceList.ItemReplaced += this.OnModelItemReplaced;

        // Indices within the selected models list changed. Since no actual items changed, this is fine.
        // this.sourceList.ItemMoved += this.OnModelItemMoved;
    }

    private void OnListBoxCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
        Debug.Assert(!this.isUpdatingModel && !this.isUpdatingControl, "Reentrancy");
        if (e.Action != NotifyCollectionChangedAction.Add && e.Action != NotifyCollectionChangedAction.Replace) {
            return;
        }

        this.isUpdatingControl = true;

        ISelectionModel selection = this.listBox.Selection;
        ItemCollection itemList = this.listBox.Items;

        IList list = e.NewItems!;
        for (int i = 0; i < list.Count; i++) {
            ListBoxItem item = (ListBoxItem) list[i]!;
            int idx = itemList.IndexOf(item);
            Debug.Assert(idx == e.NewStartingIndex + i);
            if (this.sourceList.Contains(this.toModel(item))) {
                if (!selection.IsSelected(idx))
                    selection.Select(idx);
            }
            else if (selection.IsSelected(idx)) {
                selection.Deselect(idx);
            }
        }

        this.isUpdatingControl = false;
    }

    private void OnModelItemsAdded(IObservableList<T> list, int index, IList<T> items) {
        if (!this.isUpdatingModel) {
            ItemCollection itemList = this.listBox.Items;
            if (itemList.Count <= 0) {
                return; // SelectionHandler created before items initialized. Not an issue, just worse perf
            }

            this.isUpdatingControl = true;

            ISelectionModel selection = this.listBox.Selection;
            foreach (T item in items) {
                int idx = itemList.IndexOf(this.fromModel(item));
                if (idx != -1 && !selection.IsSelected(idx)) {
                    selection.Select(idx);
                }
            }

            this.isUpdatingControl = false;
        }
    }

    private void OnModelItemsRemoved(IObservableList<T> list, int index, IList<T> items) {
        if (!this.isUpdatingModel) {
            this.isUpdatingControl = true;

            ISelectionModel selection = this.listBox.Selection;
            if (list.Count == 0) {
                selection.Clear();
            }
            else {
                ItemCollection itemList = this.listBox.Items;
                foreach (T item in items) {
                    int idx = itemList.IndexOf(this.fromModel(item));
                    if (idx != -1 && selection.IsSelected(idx)) {
                        selection.Deselect(idx);
                    }
                }
            }

            this.isUpdatingControl = false;
        }
    }

    private void OnModelItemReplaced(IObservableList<T> list, int index, T oldItem, T newItem) {
        if (!this.isUpdatingModel) {
            this.isUpdatingControl = true;

            ISelectionModel selection = this.listBox.Selection;
            ItemCollection itemList = this.listBox.Items;

            int oldIdx = itemList.IndexOf(this.fromModel(oldItem));
            if (oldIdx != -1 && selection.IsSelected(oldIdx))
                selection.Deselect(oldIdx);

            int newIdx = itemList.IndexOf(this.fromModel(newItem));
            if (newIdx != -1 && !selection.IsSelected(newIdx))
                selection.Select(newIdx);

            this.isUpdatingControl = false;
        }
    }

    private void OnListBoxSelectionChanged(object? sender, SelectionChangedEventArgs e) {
        if (this.isUpdatingControl || sender != e.Source /* SelectionChanged is not a direct event */) {
            return;
        }

        this.isUpdatingModel = true;

        foreach (object item in e.RemovedItems)
            this.sourceList.Remove(this.toModel((ListBoxItem) item));

        if (e.AddedItems.Count > 0)
            this.sourceList.AddRange(e.AddedItems.Cast<ListBoxItem>().Select(this.toModel));

        this.isUpdatingModel = false;
    }

    public void Dispose() {
        this.listBox.SelectionChanged -= this.OnListBoxSelectionChanged;
        this.listBox.Items.CollectionChanged -= this.OnListBoxCollectionChanged;
        this.sourceList.ItemsAdded -= this.OnModelItemsAdded;
        this.sourceList.ItemsRemoved -= this.OnModelItemsRemoved;
        this.sourceList.ItemReplaced -= this.OnModelItemReplaced;
        // this.sourceList.ItemMoved -= this.OnModelItemMoved;
    }
}