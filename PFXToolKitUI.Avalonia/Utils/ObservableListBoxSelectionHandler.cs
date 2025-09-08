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
        this.sourceList.ItemsAdded += this.OnModelItemsAdded;
        this.sourceList.ItemsRemoved += this.OnModelItemsRemoved;
        this.sourceList.ItemReplaced += this.OnModelItemReplaced;

        // Indices within the selected models list changed. Since no actual items changed, this is fine.
        // this.sourceList.ItemMoved += this.OnModelItemMoved;
    }

    private void OnModelItemsAdded(IObservableList<T> list, int index, IList<T> items) {
        if (!this.isUpdatingModel) {
            this.isUpdatingControl = true;

            ISelectionModel selection = this.listBox.Selection;
            ItemCollection itemList = this.listBox.Items;
            foreach (T item in items) {
                int idx = itemList.IndexOf(this.fromModel(item));
                Debug.Assert(idx != -1);
                selection.Select(idx);
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
                    Debug.Assert(idx != -1);
                    selection.Deselect(idx);
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
            Debug.Assert(oldIdx != -1);

            selection.Deselect(oldIdx);

            int newIdx = itemList.IndexOf(this.fromModel(newItem));
            Debug.Assert(newIdx != -1);

            selection.Select(newIdx);

            this.isUpdatingControl = false;
        }
    }

    private void OnListBoxSelectionChanged(object? sender, SelectionChangedEventArgs e) {
        if (this.isUpdatingControl || sender != e.Source) {
            return;
        }

        this.isUpdatingModel = true;

        foreach (object item in e.RemovedItems)
            this.sourceList.Remove(this.toModel((ListBoxItem) item));
        foreach (object item in e.AddedItems)
            this.sourceList.Add(this.toModel((ListBoxItem) item));

        this.isUpdatingModel = false;
    }

    public void Dispose() {
        this.listBox.SelectionChanged -= this.OnListBoxSelectionChanged;
        this.sourceList.ItemsAdded -= this.OnModelItemsAdded;
        this.sourceList.ItemsRemoved -= this.OnModelItemsRemoved;
        this.sourceList.ItemReplaced -= this.OnModelItemReplaced;
        // this.sourceList.ItemMoved -= this.OnModelItemMoved;
    }
}