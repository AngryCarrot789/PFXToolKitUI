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
using Avalonia.Controls.Selection;
using PFXToolKitUI.Interactivity.Selections;
using PFXToolKitUI.Utils.Collections.Observable;
using PFXToolKitUI.Utils.Ranges;

namespace PFXToolKitUI.Avalonia.Interactivity.SelectingEx2;

public sealed class SelectionModelBinder<T> {
    private bool isUpdatingModel, isUpdatingControl;

    public ISelectionModel SelectionModel { get; }

    public ListSelectionModel<T> Selection { get; }

    public SelectionModelBinder(ISelectionModel selectionModel, ListSelectionModel<T> selection) {
        this.SelectionModel = selectionModel;
        this.Selection = selection;

        this.TrySetInitialSelection();
        selectionModel.SelectionChanged += this.OnSelectionModelSelectionChanged;
        selection.SelectionChanged += this.OnModelSelectionChanged;
    }

    private void TrySetInitialSelection() {
        Debug.Assert(!this.isUpdatingControl);
        this.isUpdatingControl = true;

        this.SelectionModel.BeginBatchUpdate();
        this.SelectionModel.Clear();

        if (this.Selection.Count > 0) {
            IntegerSet<int> addedIndices = this.Selection.GetSelectedIndices();
            foreach (IntegerRange<int> range in addedIndices) {
                this.SelectionModel.SelectRange(range.Start, range.End - 1);
            }
        }

        this.SelectionModel.EndBatchUpdate();
        this.isUpdatingControl = false;
    }

    private void OnModelSelectionChanged(object? o, ListSelectionModelChangedEventArgs<T> e) {
        if (!this.isUpdatingModel) {
            Debug.Assert(!this.isUpdatingControl);
            this.isUpdatingControl = true;

            this.SelectionModel.BeginBatchUpdate();

            ObservableList<T> srcList = this.Selection.SourceList;
            IntegerSet<int> removedIndices = new IntegerSet<int>();
            foreach (T item in e.RemovedItems) {
                int index = srcList.IndexOf(item);
                if (index != -1) {
                    removedIndices.Add(index);
                }
            }

            foreach (IntegerRange<int> range in removedIndices) {
                this.SelectionModel.DeselectRange(range.Start, range.End - 1);
            }

            IntegerSet<int> addedIndices = new IntegerSet<int>();
            foreach (T item in e.AddedItems) {
                int index = srcList.IndexOf(item);
                if (index != -1) {
                    addedIndices.Add(index);
                }
            }

            foreach (IntegerRange<int> range in addedIndices) {
                this.SelectionModel.SelectRange(range.Start, range.End - 1);
            }

            this.SelectionModel.EndBatchUpdate();

            this.isUpdatingControl = false;
        }
    }

    private void OnSelectionModelSelectionChanged(object? sender, SelectionModelSelectionChangedEventArgs e) {
        if (!this.isUpdatingControl) {
            Debug.Assert(!this.isUpdatingModel);
            this.isUpdatingModel = true;

            ObservableList<T> srcList = this.Selection.SourceList;
            if (e.DeselectedIndexes.Count > 0) {
                List<T> items = new List<T>(e.DeselectedIndexes.Count);
                foreach (int i in e.DeselectedIndexes) {
                    items.Add(srcList[i]);
                }

                this.Selection.DeselectItems(items);
            }

            if (e.SelectedIndexes.Count > 0) {
                List<T> items = new List<T>(e.SelectedIndexes.Count);
                foreach (int i in e.SelectedIndexes) {
                    items.Add(srcList[i]);
                }

                this.Selection.SelectItems(items);
            }

            this.isUpdatingModel = false;
        }
    }

    public void Dispose() {
        this.SelectionModel.SelectionChanged -= this.OnSelectionModelSelectionChanged;
        this.Selection.SelectionChanged -= this.OnModelSelectionChanged;
    }
}