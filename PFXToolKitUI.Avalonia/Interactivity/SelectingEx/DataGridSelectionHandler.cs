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

using Avalonia.Controls;
using PFXToolKitUI.Utils.Collections.Observable;

namespace PFXToolKitUI.Avalonia.Interactivity.SelectingEx;

public class DataGridSelectionHandler<T> where T : class {
    private readonly ObservableList<T> sourceItems, selectedItems;
    protected bool IsUpdatingControl { get; private set; }
    protected bool IsUpdatingModel { get; private set; }

    protected DataGrid DataGrid { get; }

    protected DataGridSelectionHandler(ObservableList<T> sourceItems, ObservableList<T> selectedItems, DataGrid dataGrid) {
        this.sourceItems = sourceItems;
        this.selectedItems = selectedItems;
        this.DataGrid = dataGrid;
        this.sourceItems.ItemsAdded += this.OnSourceItemsAdded;
        this.sourceItems.ItemReplaced += this.OnSourceItemReplaced;
        this.selectedItems.ItemsAdded += this.OnSelectedItemsAdded;
        this.selectedItems.ItemsRemoved += this.OnSelectedItemsRemoved;
        this.selectedItems.ItemReplaced += this.OnSelectedItemReplaced;

        this.SetupInitialControlSelection(selectedItems);

        this.DataGrid.SelectionChanged += this.DataGridOnSelectionChanged;

        // Indices within the selected models list changed.
        // But since no actual items will change, we don't need to hook.
        // this.sourceList.ItemMoved += this.OnModelItemMoved;
    }

    private void DataGridOnSelectionChanged(object? sender, SelectionChangedEventArgs e) {
        if (!this.IsUpdatingControl && sender == e.Source) {
            this.OnControlSelectionChanged(e.RemovedItems.Cast<T>(), e.AddedItems.Cast<T>());
        }
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

        this.DataGrid.SelectedItems.Clear();
        this.InternalSelectControls(items);

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

    private void OnSourceItemsAdded(IObservableList<T> list, int index, IList<T> items) {
        if (this.IsUpdatingModel || this.IsUpdatingControl)
            throw new InvalidOperationException("Reentrancy");

        this.IsUpdatingControl = true;

        if (this.DataGrid.SelectionMode == DataGridSelectionMode.Single) {
            this.DataGrid.SelectedItem = items[items.Count - 1];
        }
        else {
            foreach (T item in items) {
                if (this.selectedItems.Contains(item)) {
                    this.DataGrid.SelectedItems.Add(item);
                }
            }
        }

        this.IsUpdatingControl = false;
    }

    private void OnSourceItemReplaced(IObservableList<T> list, int index, T olditem, T newitem) {
        if (this.IsUpdatingModel || this.IsUpdatingControl)
            throw new InvalidOperationException("Reentrancy");

        this.IsUpdatingControl = true;

        if (this.selectedItems.Contains(newitem)) {
            if (this.DataGrid.SelectionMode == DataGridSelectionMode.Single) {
                this.DataGrid.SelectedItem = newitem;
            }
            else {
                this.DataGrid.SelectedItems.Add(newitem);
            }
        }

        this.IsUpdatingControl = false;
    }

    private void OnSelectedItemsAdded(IObservableList<T> list, int index, IList<T> items) {
        if (!this.IsUpdatingModel && this.sourceItems.Count > 0 /* check if we were created before items initialized. Not an issue, just worse perf */) {
            this.IsUpdatingControl = true;
            this.InternalSelectControls(items);
            this.IsUpdatingControl = false;
        }
    }

    private void InternalSelectControls(IEnumerable<T> items) {
        if (this.DataGrid.SelectionMode == DataGridSelectionMode.Single) {
            this.DataGrid.SelectedItem = items.FirstOrDefault();
            return;
        }

        foreach (T item in items) {
            if (this.sourceItems.IndexOf(item) != -1) {
                this.DataGrid.SelectedItems.Add(item);
            }
        }
    }

    private void OnSelectedItemsRemoved(IObservableList<T> list, int index, IList<T> items) {
        if (!this.IsUpdatingModel) {
            this.IsUpdatingControl = true;

            if (list.Count == 0) {
                if (this.DataGrid.SelectionMode == DataGridSelectionMode.Single) {
                    this.DataGrid.SelectedItem = null;
                }
                else {
                    this.DataGrid.SelectedItems.Clear();
                }
            }
            else if (this.sourceItems.Count > 0) {
                if (this.DataGrid.SelectionMode == DataGridSelectionMode.Single) {
                    this.DataGrid.SelectedItem = items.FirstOrDefault();
                }
                else {
                    foreach (T item in items) {
                        if (this.sourceItems.IndexOf(item) != -1) {
                            this.DataGrid.SelectedItems.Add(item);
                        }
                    }

                    this.DataGrid.SelectedItems.Clear();
                }
            }

            this.IsUpdatingControl = false;
        }
    }

    private void OnSelectedItemReplaced(IObservableList<T> list, int index, T oldItem, T newItem) {
        if (!this.IsUpdatingModel) {
            this.IsUpdatingControl = true;

            if (this.DataGrid.SelectionMode == DataGridSelectionMode.Single) {
                this.DataGrid.SelectedItem = newItem;
            }
            else {
                if (this.sourceItems.IndexOf(oldItem) != -1)
                    this.DataGrid.SelectedItems.Remove(oldItem);

                if (this.sourceItems.IndexOf(newItem) != -1)
                    this.DataGrid.SelectedItems.Add(newItem);
            }

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
        this.DataGrid.SelectionChanged -= this.DataGridOnSelectionChanged;
    }
}