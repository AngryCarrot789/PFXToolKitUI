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
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using PFXToolKitUI.Utils;
using PFXToolKitUI.Utils.Collections.Observable;

namespace PFXToolKitUI.Avalonia.AvControls.ListBoxes.Virtualizing;

public abstract class VirtualizingModelListBox : ListBox {
    private static readonly FieldInfo FIELD_ignoreContainerSelectionChanged = typeof(SelectingItemsControl).GetField("_ignoreContainerSelectionChanged", BindingFlags.Instance | BindingFlags.NonPublic)!;
    
    protected VirtualizingModelListBox() {
        this.ItemsPanel = new FuncTemplate<Panel?>(() => new VirtualizingStackPanel());
    }

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey) {
        return this.CreateListBoxItem();
    }

    protected override void PrepareContainerForItemOverride(Control container, object? item, int index) {
        VirtualizingModelListBoxItem LBI = (VirtualizingModelListBoxItem) container;
        Debug.Assert(LBI.Model == null);
        Debug.Assert(item != null);
        
        LBI.InternalOnAddingToList();
        base.PrepareContainerForItemOverride(container, item, index);

        LBI.Model = item;
    }

    protected override void ContainerForItemPreparedOverride(Control container, object? item, int index) {
        VirtualizingModelListBoxItem LBI = (VirtualizingModelListBoxItem) container;
        Debug.Assert(LBI.Model == item);
        
        base.ContainerForItemPreparedOverride(container, item, index);
        LBI.InternalOnAddedToList();
    }

    protected override void ContainerIndexChangedOverride(Control container, int oldIndex, int newIndex) {
        VirtualizingModelListBoxItem LBI = (VirtualizingModelListBoxItem) container;
        Debug.Assert(LBI.Model != null);
        
        base.ContainerIndexChangedOverride(container, oldIndex, newIndex);
        LBI.InternalOnContainerIndexChanged(oldIndex, newIndex);
    }

    protected override void ClearContainerForItemOverride(Control element) {
        VirtualizingModelListBoxItem LBI = (VirtualizingModelListBoxItem) element;
        Debug.Assert(LBI.Model != null);

        try
        {
            FIELD_ignoreContainerSelectionChanged.SetValue(this, BoolBox.True);
            element.ClearValue(IsSelectedProperty);
        }
        finally
        {
            FIELD_ignoreContainerSelectionChanged.SetValue(this, BoolBox.False);
        }
        
        LBI.InternalOnRemovingFromList();
        LBI.InternalOnRemovedFromList();
        LBI.Model = null;

        // object? content = LBI.ContentTemplate == null ? null : LBI.Content;
        // base.ClearContainerForItemOverride(element);
        // if (content != null) LBI.Content = content;
    }

    protected abstract VirtualizingModelListBoxItem CreateListBoxItem();
}

public abstract class VirtualizingModelListBox<T> : VirtualizingModelListBox where T : class {
    public T? SelectedModel => this.SelectedItem as T;

    protected VirtualizingModelListBox() {
    }

    public void SetItemsSource(IObservableList<T>? source) {
        this.ItemsSource = source;
    }

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey) {
        return this.CreateListBoxItem();
    }

    protected sealed override VirtualizingModelListBoxItem CreateListBoxItem() => this.CreateListBoxItemGeneric();

    protected abstract VirtualizingModelListBoxItem<T> CreateListBoxItemGeneric();
}