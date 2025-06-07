// 
// Copyright (c) 2023-2025 REghZy
// 
// This file is part of FramePFX.
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
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
// 

using System.Diagnostics;
using Avalonia.Controls;
using PFXToolKitUI.Utils.Collections.Observable;

namespace PFXToolKitUI.Avalonia.AvControls.Trees;

public abstract class ModelBasedTreeView : TreeView {
}

/// <summary>
/// The base class for a model-based tree view
/// </summary>
/// <typeparam name="TModel"></typeparam>
public abstract class ModelBasedTreeView<TModel> : ModelBasedTreeView where TModel : class {
    private readonly Stack<ModelBasedTreeViewItem<TModel>> itemCache;
    internal readonly ModelControlDictionary<TModel, ModelBasedTreeViewItem<TModel>> itemMap;
    private IObservableList<TModel>? observableList;

    /// <summary>
    /// Gets the maximum amount of cached tree view items allowed
    /// </summary>
    public int MaxCacheSize { get; }

    protected ModelBasedTreeView(int maxCacheSize = 64) {
        this.MaxCacheSize = maxCacheSize;
        this.itemCache = new Stack<ModelBasedTreeViewItem<TModel>>();
        this.itemMap = new ModelControlDictionary<TModel, ModelBasedTreeViewItem<TModel>>();
    }

    public ModelBasedTreeViewItem<TModel> GetNodeAt(int index) {
        return (ModelBasedTreeViewItem<TModel>) this.Items[index]!;
    }
    
    protected void AddModels(IEnumerable<TModel> models) {
        int i = this.Items.Count;
        foreach (TModel model in models) {
            this.InsertNodeAt(i++, model);
        }
    }

    public void InsertNodeAt(int index, TModel item) => this.InsertNodeAt(this.GetCachedItemOrCreate(), item, index);

    public void InsertNodeAt(ModelBasedTreeViewItem<TModel> control, TModel model, int index) {
        Debug.Assert(control.Model == null);
        control.InternalOnAdding(this, null, model);
        Debug.Assert(control.Model == model);

        this.Items.Insert(index, control);
        this.itemMap.AddMapping(model, control);
        control.ApplyStyling();
        control.ApplyTemplate();
        control.InternalOnAdded();
    }

    public void RemoveNodeAt(int index, bool canCache = true) {
        ModelBasedTreeViewItem<TModel> control = (ModelBasedTreeViewItem<TModel>) this.Items[index]!;
        TModel model = control.Model ?? throw new Exception("Expected node to have a resource");

        Debug.Assert(control.Model == model);
        control.InternalOnRemoving();
        Debug.Assert(control.Model == null);

        this.Items.RemoveAt(index);
        this.itemMap.RemoveMapping(model, control);
        control.InternalOnRemoved();
        if (canCache)
            this.PushCachedItem(control);
    }

    public void MoveNode(int oldIndex, int newIndex) {
        ModelBasedTreeViewItem<TModel> control = (ModelBasedTreeViewItem<TModel>) this.Items[oldIndex]!;
        TModel model = control.Model ?? throw new Exception("Expected node to have a model");
        this.RemoveNodeAt(oldIndex, false);
        this.InsertNodeAt(control, model, newIndex);
    }

    protected void ClearModels() {
        for (int i = this.Items.Count - 1; i >= 0; i--) {
            this.RemoveNodeAt(i);
        }
    }
    
    /// <summary>
    /// Sets up event handlers for the list to automatically add/remove/replace/move models and then adds all the models to this list box
    /// </summary>
    /// <param name="list">The list to observe</param>
    public void SetItemsSource(IObservableList<TModel>? list) {
        if (this.observableList != null) {
            this.observableList.ItemsAdded -= this.OnItemsAdded;
            this.observableList.ItemsRemoved -= this.OnItemsRemoved;
            this.observableList.ItemReplaced -= this.OnItemReplaced;
            this.observableList.ItemMoved -= this.OnItemMoved;
            this.observableList = null;
            this.ClearModels();
        }

        if ((this.observableList = list) != null) {
            this.AddModels(list!);
            list!.ItemsAdded += this.OnItemsAdded;
            list.ItemsRemoved += this.OnItemsRemoved;
            list.ItemReplaced += this.OnItemReplaced;
            list.ItemMoved += this.OnItemMoved;
        }
    }
    
    private void OnItemsAdded(IObservableList<TModel> list, IList<TModel> items, int index) {
        foreach (TModel model in items) {
            this.InsertNodeAt(index++, model);
        }
    }

    private void OnItemsRemoved(IObservableList<TModel> list, IList<TModel> items, int index) {
        for (int i = index + items.Count - 1; i >= index; i--) {
            this.RemoveNodeAt(i);
        }
    }

    private void OnItemReplaced(IObservableList<TModel> list, TModel oldItem, TModel newItem, int index) {
        this.RemoveNodeAt(index);
        this.InsertNodeAt(index, newItem);
    }

    private void OnItemMoved(IObservableList<TModel> list, TModel item, int oldIdx, int newIdx) {
        this.MoveNode(oldIdx, newIdx);
    }

    protected internal ModelBasedTreeViewItem<TModel> GetCachedItemOrCreate() {
        return this.itemCache.TryPop(out ModelBasedTreeViewItem<TModel>? item)
            ? item
            : this.CreateItem();
    }

    protected internal bool PushCachedItem(ModelBasedTreeViewItem<TModel> item) {
        if (this.itemCache.Count >= this.MaxCacheSize)
            return false;
        this.itemCache.Push(item);
        return true;
    }

    /// <summary>
    /// Creates a new node 
    /// </summary>
    /// <returns></returns>
    protected abstract ModelBasedTreeViewItem<TModel> CreateItem();

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey) => this.CreateItem();
}