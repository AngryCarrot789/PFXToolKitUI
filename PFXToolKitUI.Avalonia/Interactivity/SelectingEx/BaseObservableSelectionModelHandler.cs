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

using Avalonia.Controls.Selection;
using PFXToolKitUI.Utils.Collections.Observable;
using PFXToolKitUI.Utils.Events;

namespace PFXToolKitUI.Avalonia.Interactivity.SelectingEx;

public abstract class BaseObservableSelectionModelHandler<T> where T : class {
    private readonly ObservableList<T> sourceItems, selectedItems;
    protected bool IsUpdatingControl { get; private set; }
    protected bool IsUpdatingModel { get; private set; }

    protected abstract ISelectionModel SelectionModel { get; }

    protected BaseObservableSelectionModelHandler(ObservableList<T> sourceItems, ObservableList<T> selectedItems) {
        this.sourceItems = sourceItems;
        this.selectedItems = selectedItems;

        this.sourceItems.ItemsAdded += this.OnSourceItemsAdded;
        this.sourceItems.ItemReplaced += this.OnSourceItemReplaced;
        this.selectedItems.ItemsAdded += this.OnSelectedItemsAdded;
        this.selectedItems.ItemsRemoved += this.OnSelectedItemsRemoved;
        this.selectedItems.ItemReplaced += this.OnSelectedItemReplaced;

        // Indices within the selected models list changed.
        // But since no actual items will change, we don't need to hook.
        // this.sourceList.ItemMoved += this.OnModelItemMoved;
    }
    
    /// <summary>
    /// Reflects the model selection to the control selection. Typically called in the constructor by derived types once initialized 
    /// </summary>
    /// <param name="items">The items to set as the selection</param>
    /// <exception cref="InvalidOperationException">Currently updating mode or control</exception>
    protected void SetupInitialControlSelection(IEnumerable<T> items) {
        if (this.IsUpdatingModel)
            throw new InvalidOperationException("Currently updating model selection");
        if (this.IsUpdatingControl)
            throw new InvalidOperationException("Currently updating control selection");

        this.IsUpdatingControl = true;
        
        ISelectionModel selection = this.SelectionModel;
        selection.Clear();
        this.InternalSelectControls(items, selection);
        
        this.IsUpdatingControl = false;
    }

    protected void OnControlSelectionChanged(IEnumerable<T>? removedItems, IEnumerable<T>? addedItems) {
        if (this.IsUpdatingControl) {
            return;
        }

        this.IsUpdatingModel = true;

        if (removedItems != null) {
            foreach (T item in removedItems)
                this.selectedItems.Remove(item);
        }

        if (addedItems != null) {
            this.selectedItems.AddRange(addedItems);
        }

        this.IsUpdatingModel = false;
    }

    private void OnSourceItemsAdded(object? sender, ItemsAddOrRemoveEventArgs<T> e) {
        if (this.IsUpdatingModel || this.IsUpdatingControl)
            throw new InvalidOperationException("Reentrancy");

        this.IsUpdatingControl = true;

        ISelectionModel selection = this.SelectionModel;
        for (int i = 0; i < e.Items.Count; i++) {
            int idx = e.Index + i;
            if (this.selectedItems.Contains(e.Items[i])) {
                if (!selection.IsSelected(idx))
                    selection.Select(idx);
            }
            else if (selection.IsSelected(idx)) {
                selection.Deselect(idx);
            }
        }

        this.IsUpdatingControl = false;
    }

    private void OnSourceItemReplaced(object? sender, ItemReplaceEventArgs<T> e) {
        if (this.IsUpdatingModel || this.IsUpdatingControl)
            throw new InvalidOperationException("Reentrancy");

        this.IsUpdatingControl = true;

        ISelectionModel selection = this.SelectionModel;
        if (this.selectedItems.Contains(e.NewItem)) {
            if (!selection.IsSelected(e.Index))
                selection.Select(e.Index);
        }
        else if (selection.IsSelected(e.Index)) {
            selection.Deselect(e.Index);
        }

        this.IsUpdatingControl = false;
    }

    private void OnSelectedItemsAdded(object? sender, ItemsAddOrRemoveEventArgs<T> e) {
        if (!this.IsUpdatingModel && this.sourceItems.Count > 0 /* check if we were created before items initialized. Not an issue, just worse perf */) {
            this.IsUpdatingControl = true;
            this.InternalSelectControls(e.Items, this.SelectionModel);
            this.IsUpdatingControl = false;
        }
    }

    private void InternalSelectControls(IEnumerable<T> items, ISelectionModel selection) {
        if (this.sourceItems.Count > 0) {
            foreach (T item in items) {
                int idx = this.sourceItems.IndexOf(item);
                if (idx != -1 && !selection.IsSelected(idx)) {
                    selection.Select(idx);
                }
            }
        }
    }

    private void OnSelectedItemsRemoved(object? sender, ItemsAddOrRemoveEventArgs<T> e) {
        if (!this.IsUpdatingModel) {
            this.IsUpdatingControl = true;

            ISelectionModel selection = this.SelectionModel;
            if (this.selectedItems.Count == 0) {
                selection.Clear();
            }
            else if (this.sourceItems.Count > 0) {
                foreach (T item in e.Items) {
                    int idx = this.sourceItems.IndexOf(item);
                    if (idx != -1 && selection.IsSelected(idx)) {
                        selection.Deselect(idx);
                    }
                }
            }

            this.IsUpdatingControl = false;
        }
    }

    private void OnSelectedItemReplaced(object? sender, ItemReplaceEventArgs<T> e) {
        if (!this.IsUpdatingModel) {
            this.IsUpdatingControl = true;

            ISelectionModel selection = this.SelectionModel;
            int oldIdx = this.sourceItems.IndexOf(e.OldItem);
            if (oldIdx != -1 && selection.IsSelected(oldIdx))
                selection.Deselect(oldIdx);

            int newIdx = this.sourceItems.IndexOf(e.NewItem);
            if (newIdx != -1 && !selection.IsSelected(newIdx))
                selection.Select(newIdx);

            this.IsUpdatingControl = false;
        }
    }

    public void Dispose() {
        this.sourceItems.ItemsAdded -= this.OnSourceItemsAdded;
        this.sourceItems.ItemReplaced -= this.OnSourceItemReplaced;
        this.selectedItems.ItemsAdded -= this.OnSelectedItemsAdded;
        this.selectedItems.ItemsRemoved -= this.OnSelectedItemsRemoved;
        this.selectedItems.ItemReplaced -= this.OnSelectedItemReplaced;
        this.OnDisposed();
    }

    protected virtual void OnDisposed() {
    }
}