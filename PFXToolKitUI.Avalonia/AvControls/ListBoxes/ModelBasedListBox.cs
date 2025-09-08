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
using PFXToolKitUI.Utils.Collections.Observable;

namespace PFXToolKitUI.Avalonia.AvControls.ListBoxes;

/// <summary>
/// A list box that implements the pre/post add/remove architecture
/// </summary>
/// <typeparam name="TModel">The model type</typeparam>
public abstract class ModelBasedListBox<TModel> : BaseModelBasedListBox where TModel : class {
    private readonly Stack<ModelBasedListBoxItem<TModel>>? itemCache;
    private readonly ModelControlMap<TModel, ModelBasedListBoxItem<TModel>> itemMap;
    private IObservableList<TModel>? observableList;

    /// <summary>
    /// Gets the item map for this list box. This is used to map models to the list box items and vice versa
    /// </summary>
    public IModelControlMap<TModel, ModelBasedListBoxItem<TModel>> ItemMap => this.itemMap;

    /// <summary>
    /// Gets the selected item's model, or null, if there's no selected item
    /// </summary>
    public TModel? SelectedModel {
        get => ((ModelBasedListBoxItem<TModel>?) this.SelectedItem)?.Model;
        set => this.SelectedItem = value != null ? this.itemMap.GetControl(value) : null;
    }

    public TModel this[int index] => ((ModelBasedListBoxItem<TModel>) this.Items[index]!).Model!;

    /// <summary>
    /// Gets the size of our item cache. When zero, caching is disabled
    /// </summary>
    public int MaxCacheSize { get; }

    protected ModelBasedListBox() : this(0) {
    }

    protected ModelBasedListBox(int cacheSize) {
        this.itemCache = cacheSize > 0 ? new Stack<ModelBasedListBoxItem<TModel>>(cacheSize) : null;
        this.MaxCacheSize = cacheSize;
        this.itemMap = new ModelControlMap<TModel, ModelBasedListBoxItem<TModel>>();
    }

    protected void AddModel(TModel model) => this.InsertModelAt(this.Items.Count, model);

    protected void InsertModelAt(int index, TModel model) {
        this.InsertModelAtInternal(index, model, this.itemCache?.Count > 0 ? this.itemCache.Pop() : this.CreateItem());
    }
    
    private void InsertModelAtInternal(int index, TModel model, ModelBasedListBoxItem<TModel> control) {
        this.itemMap.AddMapping(model, control);
        this.OnAddingItemToList(control, model);
        this.Items.Insert(index, control);
        this.OnAddedItemToList(control, model);
    }

    protected void RemoveModelAt(int index) {
        ModelBasedListBoxItem<TModel> control = (ModelBasedListBoxItem<TModel>) this.Items[index]!;
        this.RemoveModelAtInternal(index, control);
        if (this.itemCache != null && this.itemCache.Count < this.MaxCacheSize) {
            this.itemCache.Push(control);
        }
    }
    
    private void RemoveModelAtInternal(int index, ModelBasedListBoxItem<TModel> control) {
        TModel model = control.Model!;
        this.OnRemovingItemFromList(control, model);
        this.itemMap.RemoveMapping(model, control);
        this.Items.RemoveAt(index);
        this.OnRemovedItemFromList(control, model);
    }

    protected void MoveModel(int oldIndex, int newIndex) {
        ModelBasedListBoxItem<TModel> control = (ModelBasedListBoxItem<TModel>) this.Items[oldIndex]!;
        TModel model = control.Model!;
        bool isSelected = control.IsSelected;
        this.RemoveModelAtInternal(oldIndex, control);
        this.InsertModelAtInternal(newIndex, model, control);
        if (isSelected)
            control.IsSelected = true;
    }

    protected void ClearModels() {
        for (int i = this.Items.Count - 1; i >= 0; i--) {
            this.RemoveModelAt(i);
        }
    }

    protected void AddModels(IEnumerable<TModel> models) {
        foreach (TModel model in models) {
            this.AddModel(model);
        }
    }

    private void OnItemsAdded(IObservableList<TModel> list, int index, IList<TModel> items) {
        foreach (TModel model in items) {
            this.InsertModelAt(index++, model);
        }
    }

    private void OnItemsRemoved(IObservableList<TModel> list, int index, IList<TModel> items) {
        for (int i = index + items.Count - 1; i >= index; i--) {
            this.RemoveModelAt(i);
        }
    }

    private void OnItemReplaced(IObservableList<TModel> list, int index, TModel oldItem, TModel newItem) {
        this.RemoveModelAt(index);
        this.InsertModelAt(index, newItem);
    }

    private void OnItemMoved(IObservableList<TModel> list, int oldIdx, int newIdx, TModel item) {
        this.MoveModel(oldIdx, newIdx);
    }

    /// <summary>
    /// Sets up event handlers for the list to automatically add/remove/replace/move models and then adds all the models to this list box
    /// </summary>
    /// <param name="newList">The list to observe</param>
    public void SetItemsSource(IObservableList<TModel>? newList) {
        IObservableList<TModel>? oldList = this.observableList;
        this.observableList = null;
        
        if (oldList != null) {
            oldList.ItemsAdded -= this.OnItemsAdded;
            oldList.ItemsRemoved -= this.OnItemsRemoved;
            oldList.ItemReplaced -= this.OnItemReplaced;
            oldList.ItemMoved -= this.OnItemMoved;
            this.observableList = null;
            this.ClearModels();
        }

        if (newList != null) {
            this.AddModels(this.observableList = newList);
            newList.ItemsAdded += this.OnItemsAdded;
            newList.ItemsRemoved += this.OnItemsRemoved;
            newList.ItemReplaced += this.OnItemReplaced;
            newList.ItemMoved += this.OnItemMoved;
        }
    }

    protected virtual void OnAddingItemToList(ModelBasedListBoxItem<TModel> control, TModel model) {
        Debug.Assert(control.Model == null);
        control.InternalOnAddingToList(this, model);
    }

    protected virtual void OnAddedItemToList(ModelBasedListBoxItem<TModel> control, TModel model) {
        Debug.Assert(control.Model == model);
        control.InternalOnAddedToList();
    }

    protected virtual void OnRemovingItemFromList(ModelBasedListBoxItem<TModel> control, TModel model) {
        Debug.Assert(control.Model == model);
        base.InternalOnRemovingItem(control);
        control.InternalOnRemovingFromList();
    }

    protected virtual void OnRemovedItemFromList(ModelBasedListBoxItem<TModel> control, TModel model) {
        Debug.Assert(control.Model == model);
        control.InternalOnRemovedFromList();
        Debug.Assert(control.Model == null);
        
        control.ClearValue(IsSelectedProperty);
    }

    protected abstract ModelBasedListBoxItem<TModel> CreateItem();

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey) => this.CreateItem();
}