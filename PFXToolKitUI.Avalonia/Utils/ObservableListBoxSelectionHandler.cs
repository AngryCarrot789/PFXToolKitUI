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
    private readonly ObservableList<T> sourceItems, selectedItems;
    private readonly ListBox listBox;
    private readonly Func<ListBoxItem, T> toModel;
    private bool isUpdatingControl, isUpdatingModel;

    public ObservableListBoxSelectionHandler(ObservableList<T> sourceItems, ObservableList<T> selectedItems, ListBox listBox, Func<ListBoxItem, T> toModel) {
        this.sourceItems = sourceItems;
        this.selectedItems = selectedItems;
        this.listBox = listBox;
        this.toModel = toModel;

        this.listBox.Selection.Clear();
        this.OnSelectedItemsAdded(selectedItems, 0, selectedItems);
        
        this.listBox.SelectionChanged += this.OnListBoxSelectionChanged;
        this.sourceItems.ItemsAdded += this.OnSourceItemsAdded;
        this.sourceItems.ItemReplaced += this.OnSourceItemReplaced;
        this.selectedItems.ItemsAdded += this.OnSelectedItemsAdded;
        this.selectedItems.ItemsRemoved += this.OnSelectedItemsRemoved;
        this.selectedItems.ItemReplaced += this.OnSelectedItemReplaced;

        // Indices within the selected models list changed. Since no actual items changed, this is fine.
        // this.sourceList.ItemMoved += this.OnModelItemMoved;
    }

    private void OnListBoxSelectionChanged(object? sender, SelectionChangedEventArgs e) {
        if (this.isUpdatingControl || sender != e.Source /* SelectionChanged is not a direct event */) {
            return;
        }

        this.isUpdatingModel = true;

        foreach (object item in e.RemovedItems)
            this.selectedItems.Remove(this.toModel((ListBoxItem) item));

        if (e.AddedItems.Count > 0)
            this.selectedItems.AddRange(e.AddedItems.Cast<ListBoxItem>().Select(this.toModel));

        this.isUpdatingModel = false;
    }
    
    private void OnSourceItemsAdded(IObservableList<T> list, int index, IList<T> items) {
        if (this.isUpdatingModel || this.isUpdatingControl)
            throw new InvalidOperationException("Reentrancy");
        
        this.isUpdatingControl = true;

        ISelectionModel selection = this.listBox.Selection;
        for (int i = 0; i < items.Count; i++) {
            int idx = index + i;
            if (this.selectedItems.Contains(items[i])) {
                if (!selection.IsSelected(idx))
                    selection.Select(idx);
            }
            else if (selection.IsSelected(idx)) {
                selection.Deselect(idx);
            }
        }

        this.isUpdatingControl = false;
    }

    private void OnSourceItemReplaced(IObservableList<T> list, int index, T olditem, T newitem) {
        if (this.isUpdatingModel || this.isUpdatingControl)
            throw new InvalidOperationException("Reentrancy");
        
        this.isUpdatingControl = true;
        
        ISelectionModel selection = this.listBox.Selection;
        if (this.selectedItems.Contains(newitem)) {
            if (!selection.IsSelected(index))
                selection.Select(index);
        }
        else if (selection.IsSelected(index)) {
            selection.Deselect(index);
        }

        this.isUpdatingControl = false;
    }

    private void OnSelectedItemsAdded(IObservableList<T> list, int index, IList<T> items) {
        if (!this.isUpdatingModel && this.sourceItems.Count > 0 /* check if we were created before items initialized. Not an issue, just worse perf */) {
            this.isUpdatingControl = true;

            ISelectionModel selection = this.listBox.Selection;
            foreach (T item in items) {
                int idx = this.sourceItems.IndexOf(item);
                if (idx != -1 && !selection.IsSelected(idx)) {
                    selection.Select(idx);
                }
            }

            this.isUpdatingControl = false;
        }
    }

    private void OnSelectedItemsRemoved(IObservableList<T> list, int index, IList<T> items) {
        if (!this.isUpdatingModel) {
            this.isUpdatingControl = true;

            ISelectionModel selection = this.listBox.Selection;
            if (list.Count == 0) {
                selection.Clear();
            }
            else {
                foreach (T item in items) {
                    int idx = this.sourceItems.IndexOf(item);
                    if (idx != -1 && selection.IsSelected(idx)) {
                        selection.Deselect(idx);
                    }
                }
            }

            this.isUpdatingControl = false;
        }
    }

    private void OnSelectedItemReplaced(IObservableList<T> list, int index, T oldItem, T newItem) {
        if (!this.isUpdatingModel) {
            this.isUpdatingControl = true;

            ISelectionModel selection = this.listBox.Selection;
            int oldIdx = this.sourceItems.IndexOf(oldItem);
            if (oldIdx != -1 && selection.IsSelected(oldIdx))
                selection.Deselect(oldIdx);

            int newIdx = this.sourceItems.IndexOf(newItem);
            if (newIdx != -1 && !selection.IsSelected(newIdx))
                selection.Select(newIdx);

            this.isUpdatingControl = false;
        }
    }

    public void Dispose() {
        this.listBox.SelectionChanged -= this.OnListBoxSelectionChanged;
        this.sourceItems.ItemsAdded -= this.OnSourceItemsAdded;
        this.sourceItems.ItemReplaced -= this.OnSourceItemReplaced;
        this.selectedItems.ItemsAdded -= this.OnSelectedItemsAdded;
        this.selectedItems.ItemsRemoved -= this.OnSelectedItemsRemoved;
        this.selectedItems.ItemReplaced -= this.OnSelectedItemReplaced;
    }
}