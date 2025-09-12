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
using PFXToolKitUI.Utils;
using PFXToolKitUI.Utils.Collections.Observable;

namespace PFXToolKitUI.Avalonia.Interactivity.SelectingEx;

public sealed class DataGridSelectionModelBinder<T> {
    private bool isUpdatingModel, isUpdatingControl;

    public DataGrid DataGrid { get; }
    
    public ListSelectionModel<T> Selection { get; }

    public DataGridSelectionModelBinder(DataGrid dataGrid, ListSelectionModel<T> selection) {
        this.DataGrid = dataGrid;
        this.Selection = selection;

        dataGrid.SelectionChanged += this.OnDataGridSelectionChanged;
        selection.SelectionChanged += this.OnModelSelectionChanged;
    }

    private void OnModelSelectionChanged(ListSelectionModel<T> sender, IList<IntRange> addedIndices, IList<IntRange> removedIndices) {
        if (this.isUpdatingModel)
            return;

        Debug.Assert(!this.isUpdatingControl);
        this.isUpdatingControl = true;

        ObservableList<T> srcList = this.Selection.SourceList;
        IList? list = this.DataGrid.SelectedItems;
        foreach (IntRange range in removedIndices) {
            for (int i = range.Start; i < range.End; i++) {
                list.Remove(srcList[i]);
            }
        }

        foreach (IntRange range in addedIndices) {
            for (int i = range.Start; i < range.End; i++) {
                list.Add(srcList[i]);
            }
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
        this.DataGrid.SelectionChanged += this.OnDataGridSelectionChanged;
        this.Selection.SelectionChanged += this.OnModelSelectionChanged;
    }
}