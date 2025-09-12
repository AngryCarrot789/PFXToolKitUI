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
using Avalonia.Controls.Selection;
using PFXToolKitUI.Interactivity.Selections;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.Avalonia.Interactivity.SelectingEx2;

public sealed class SelectionModelBinder<T> {
    private bool isUpdatingModel, isUpdatingControl;

    public ISelectionModel SelectionModel { get; }

    public ListSelectionModel<T> Selection { get; }

    public SelectionModelBinder(ISelectionModel selectionModel, ListSelectionModel<T> selection) {
        this.SelectionModel = selectionModel;
        this.Selection = selection;

        this.OnModelSelectionChanged(selection, new SelectionModelChangedEventArgs(selection.ToIntRangeUnion().ToList(), ReadOnlyCollection<IntRange>.Empty));
        selectionModel.SelectionChanged += this.OnSelectionModelSelectionChanged;
        selection.SelectionChanged += this.OnModelSelectionChanged;
    }

    private void OnModelSelectionChanged(object? o, SelectionModelChangedEventArgs e) {
        if (this.isUpdatingModel)
            return;

        Debug.Assert(!this.isUpdatingControl);
        this.isUpdatingControl = true;

        this.SelectionModel.BeginBatchUpdate();
        
        foreach (IntRange range in e.RemovedIndices) {
            Debug.Assert(range.Length > 0);
            this.SelectionModel.DeselectRange(range.Start, range.End - 1);
        }

        foreach (IntRange range in e.AddedIndices) {
            Debug.Assert(range.Length > 0);
            this.SelectionModel.SelectRange(range.Start, range.End - 1);
        }
        
        this.SelectionModel.EndBatchUpdate();

        this.isUpdatingControl = false;
    }

    private void OnSelectionModelSelectionChanged(object? sender, SelectionModelSelectionChangedEventArgs e) {
        if (this.isUpdatingControl)
            return;

        Debug.Assert(!this.isUpdatingModel);
        this.isUpdatingModel = true;

        if (e.DeselectedIndexes.Count > 0) {
            IntRangeUnion union = new IntRangeUnion();
            foreach (int i in e.DeselectedIndexes) {
                union.Add(i);
            }

            this.Selection.DeselectRanges(union);
        }
        
        if (e.SelectedIndexes.Count > 0) {
            IntRangeUnion union = new IntRangeUnion();
            foreach (int i in e.SelectedIndexes) {
                union.Add(i);
            }

            this.Selection.SelectRanges(union);
        }

        this.isUpdatingModel = false;
    }

    public void Dispose() {
        this.SelectionModel.SelectionChanged -= this.OnSelectionModelSelectionChanged;
        this.Selection.SelectionChanged -= this.OnModelSelectionChanged;
    }
}