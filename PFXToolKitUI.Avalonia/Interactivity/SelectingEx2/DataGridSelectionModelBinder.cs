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
using System.Diagnostics;
using Avalonia.Controls;
using PFXToolKitUI.Interactivity.Selections;

namespace PFXToolKitUI.Avalonia.Interactivity.SelectingEx2;

public sealed class DataGridSelectionModelBinder<T> {
    private bool isUpdatingModel, isUpdatingControl;

    public DataGrid DataGrid { get; }

    public ListSelectionModel<T> Selection { get; }

    public DataGridSelectionModelBinder(DataGrid dataGrid, ListSelectionModel<T> selection) {
        this.DataGrid = dataGrid;
        this.Selection = selection;

        this.TrySetInitialSelection();
        dataGrid.SelectionChanged += this.OnDataGridSelectionChanged;
        selection.SelectionChanged += this.OnModelSelectionChanged;
    }

    private void TrySetInitialSelection() {
        Debug.Assert(!this.isUpdatingControl);
        this.isUpdatingControl = true;

        IList list = this.DataGrid.SelectedItems;
        list.Clear();

        if (this.Selection.Count > 0) {
            List<T> toAdd = this.Selection.SelectedItems.ToList();
            foreach (T item in toAdd) {
                list.Add(item);
            }
        }

        this.isUpdatingControl = false;
    }

    private void OnModelSelectionChanged(object? o, ListSelectionModelChangedEventArgs<T> e) {
        if (this.isUpdatingModel)
            return;

        Debug.Assert(!this.isUpdatingControl);
        this.isUpdatingControl = true;

        IList list = this.DataGrid.SelectedItems;
        foreach (T item in e.RemovedItems) {
            list.Remove(item);
        }

        foreach (T item in e.AddedItems) {
            list.Add(item);
        }

        this.isUpdatingControl = false;
    }

    private void OnDataGridSelectionChanged(object? sender, SelectionChangedEventArgs e) {
        if (this.isUpdatingControl)
            return;

        Debug.Assert(!this.isUpdatingModel);
        this.isUpdatingModel = true;

        this.Selection.DeselectItems(e.RemovedItems.Cast<T>());
        this.Selection.SelectItems(e.AddedItems.Cast<T>());

        this.isUpdatingModel = false;
    }

    public void Dispose() {
        this.DataGrid.SelectionChanged -= this.OnDataGridSelectionChanged;
        this.Selection.SelectionChanged -= this.OnModelSelectionChanged;
    }
}