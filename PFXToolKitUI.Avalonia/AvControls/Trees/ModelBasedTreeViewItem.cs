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
using PFXToolKitUI.Avalonia.Bindings;
using PFXToolKitUI.Utils.Collections.Observable;

namespace PFXToolKitUI.Avalonia.AvControls.Trees;

public abstract class ModelBasedTreeViewItem : TreeViewItem {
    /// <summary>
    /// Called before this item is added to the <see cref="TreeView"/> but after the <see cref="TreeView"/> and <see cref="Model"/> references are set
    /// </summary>
    protected abstract void OnAdding();

    /// <summary>
    /// Invoked once the styling and template is applied and we're added to the <see cref="TreeView"/>.
    /// This can connect binders and add event handlers
    /// </summary>
    protected abstract void OnAdded();

    /// <summary>
    /// Invoked when we're about to be removed from our <see cref="TreeView"/>.
    /// This can disconnect binders and remove event handlers
    /// </summary>
    protected abstract void OnRemoving();

    /// <summary>
    /// Invoked once removed from <see cref="TreeView"/> but before the <see cref="TreeView"/> and <see cref="Model"/> references are cleared
    /// </summary>
    protected abstract void OnRemoved();
}

public abstract class ModelBasedTreeViewItem<TModel> : ModelBasedTreeViewItem where TModel : class {
    /// <summary>
    /// Gets the tree view this node exists in
    /// </summary>
    public ModelBasedTreeView<TModel>? TreeView { get; private set; }

    /// <summary>
    /// Gets the tree view item this node is parented to. This may be null when <see cref="TreeView"/> is non-null, meaning we're a root level node
    /// </summary>
    public ModelBasedTreeViewItem<TModel>? ParentItem { get; private set; }

    /// <summary>
    /// Gets this node's model
    /// </summary>
    public TModel? Model { get; private set; }

    private List<IBinder<TModel>>? modelBinderList;
    private IObservableList<TModel>? observableList;

    protected ModelBasedTreeViewItem() {
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

    public void InsertNodeAt(int index, TModel item) => this.InsertNodeAt(null, item, index);

    public void InsertNodeAt(ModelBasedTreeViewItem<TModel>? control, TModel model, int index) {
        ModelBasedTreeView<TModel>? tree = this.TreeView;
        if (tree == null)
            throw new InvalidOperationException("Cannot add items when we have no tree associated");
        
        control ??= tree.GetCachedItemOrCreate();
        Debug.Assert(control.Model == null);
        control.InternalOnAdding(tree, this, model);
        Debug.Assert(control.Model == model);

        this.Items.Insert(index, control);
        tree.itemMap.AddMapping(model, control);
        control.ApplyStyling();
        control.ApplyTemplate();
        control.InternalOnAdded();
    }

    public void RemoveNodeAt(int index, bool canCache = true) {
        ModelBasedTreeView<TModel>? tree = this.TreeView;
        if (tree == null)
            throw new InvalidOperationException("Cannot remove items when we have no tree associated");
        
        ModelBasedTreeViewItem<TModel> control = (ModelBasedTreeViewItem<TModel>) this.Items[index]!;
        TModel model = control.Model ?? throw new Exception("Expected node to have a model");

        Debug.Assert(control.Model == model);
        control.InternalOnRemoving();
        Debug.Assert(control.Model == null);

        this.Items.RemoveAt(index);
        tree.itemMap.RemoveMapping(model, control);
        control.InternalOnRemoved();
        if (canCache)
            tree.PushCachedItem(control);
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
    /// Adds a binder to our internal list. When we become connected to or
    /// disconnected from the model, the binder is attached/detached accordingly
    /// </summary>
    /// <param name="binder">The binder</param>
    /// <exception cref="InvalidOperationException">Binder is already attached or already added</exception>
    public void AddBinderForModel(IBinder<TModel> binder) {
        if (binder.HasModel)
            throw new InvalidOperationException("Binder is already attached");

        if (this.modelBinderList == null)
            this.modelBinderList = new List<IBinder<TModel>>();
        else if (this.modelBinderList.Contains(binder))
            throw new InvalidOperationException("Binder already added");

        this.modelBinderList.Add(binder);
        if (this.Model != null)
            binder.AttachModel(this.Model!);
    }

    /// <summary>
    /// Adds multiple binders to our internal list. See <see cref="AddBinderForModel(PFXToolKitUI.Avalonia.Bindings.IBinder{TModel})"/> for more info
    /// </summary>
    /// <param name="binders">The binders</param>
    /// <exception cref="InvalidOperationException">Binder is already attached or already added</exception>
    public void AddBinderForModel(params IBinder<TModel>[] binders) {
        if (binders.Length < 1) {
            return;
        }

        foreach (IBinder<TModel> binder in binders) {
            if (binder.HasModel)
                throw new InvalidOperationException("Binder is already attached");
            if (this.modelBinderList != null && this.modelBinderList.Contains(binder))
                throw new InvalidOperationException("Binder already added");
        }

        this.modelBinderList ??= new List<IBinder<TModel>>(binders.Length);
        foreach (IBinder<TModel> binder in binders) {
            this.modelBinderList.Add(binder);
            if (this.Model != null)
                binder.AttachModel(this.Model);
        }
    }

    public bool RemoveBinderForModel(IBinder<TModel> binder) {
        if (this.modelBinderList == null || !this.modelBinderList.Remove(binder)) {
            return false;
        }

        if (this.Model != null && binder.HasModel && ReferenceEquals(binder.Model, this.Model)) {
            binder.DetachModel();
        }

        return true;
    }

    internal void InternalOnAdding(ModelBasedTreeView<TModel> explorerTree, ModelBasedTreeViewItem<TModel>? parentItem, TModel resource) {
        this.TreeView = explorerTree;
        this.ParentItem = parentItem;
        this.Model = resource;
        this.OnAdding();
    }

    internal void InternalOnAdded() {
        if (this.modelBinderList != null) {
            foreach (IBinder<TModel> binder in this.modelBinderList) {
                binder.AttachModel(this.Model!);
            }
        }

        this.OnAdded();
    }

    internal void InternalOnRemoving() {
        if (this.modelBinderList != null) {
            foreach (IBinder<TModel> binder in this.modelBinderList) {
                binder.DetachModel();
            }
        }

        this.OnRemoving();
    }

    internal void InternalOnRemoved() {
        this.OnRemoved();
        this.TreeView = null;
        this.ParentItem = null;
        this.Model = null;
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
}