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
using Avalonia.Controls.Selection;
using PFXToolKitUI.Utils.Collections.Observable;

namespace PFXToolKitUI.Avalonia.Interactivity.SelectingEx;

public sealed class ListBoxSelectionHandler<T> : BaseObservableSelectionModelHandler<T> where T : class {
    private readonly ListBox listBox;
    private readonly Func<ListBoxItem, T> toModel;

    protected override ISelectionModel SelectionModel => this.listBox.Selection;
    
    public ListBoxSelectionHandler(ObservableList<T> sourceItems, ObservableList<T> selectedItems, ListBox listBox, Func<ListBoxItem, T> toModel) : base(sourceItems, selectedItems) {
        this.listBox = listBox;
        this.toModel = toModel;
        this.SetupInitialControlSelection(selectedItems);
        
        // cheat -- we know SetupInitialControlSelection will invoke SelectionChanged,
        // so hook afterward to improve performance slightly 
        this.listBox.SelectionChanged += this.OnListBoxSelectionChanged;
    }

    private void OnListBoxSelectionChanged(object? sender, SelectionChangedEventArgs e) {
        // SelectionChanged is not a direct event, so we could end up getting
        // selection changes from a combo box inside a ListBoxItem. dammit!
        if (!this.IsUpdatingControl && sender == e.Source) {
            this.OnControlSelectionChanged(e.RemovedItems.Cast<ListBoxItem>().Select(this.toModel), e.AddedItems.Cast<ListBoxItem>().Select(this.toModel));
        }
    }

    protected override void OnDisposed() {
        base.OnDisposed();
        this.listBox.SelectionChanged -= this.OnListBoxSelectionChanged;
    }
}