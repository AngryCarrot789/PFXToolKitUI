// 
// Copyright (c) 2024-2025 REghZy
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
using PFXToolKitUI.Avalonia.Bindings;

namespace PFXToolKitUI.Avalonia.AvControls.ListBoxes;

public abstract class ModelBasedListBoxItem<TModel> : BaseModelBasedListBoxItem where TModel : class {
    public new ModelBasedListBox<TModel>? ListBox {
        get => (ModelBasedListBox<TModel>?) base.ListBox;
        internal set => base.ListBox = value;
    }

    public TModel? Model { get; private set; }

    private List<IBinder<TModel>>? modelBinderList;

    protected ModelBasedListBoxItem() {
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
                binder.AttachModel(this.Model!);
        }
    }

    public bool RemoveBinderForModel(IBinder<TModel> binder) {
        if (this.modelBinderList == null || !this.modelBinderList.Remove(binder)) {
            return false;
        }

        if (this.Model != null && binder.HasModel && binder.Model == this.Model) {
            binder.DetachModel();
        }

        return true;
    }

    internal void InternalOnAddingToList(ModelBasedListBox<TModel> explorerList, TModel resource) {
        this.ListBox = explorerList;
        this.Model = resource;
        this.OnAddingToList();
    }

    internal void InternalOnAddedToList() {
        if (this.modelBinderList != null) {
            foreach (IBinder<TModel> binder in this.modelBinderList) {
                binder.AttachModel(this.Model!);
            }
        }

        this.OnAddedToList();
    }

    internal void InternalOnRemovingFromList() {
        if (this.modelBinderList != null) {
            foreach (IBinder<TModel> binder in this.modelBinderList) {
                binder.DetachModel();
            }
        }

        this.OnRemovingFromList();
    }

    internal void InternalOnRemovedFromList() {
        this.OnRemovedFromList();
        this.ListBox = null;
        this.Model = null;
    }

    /// <summary>
    /// Called before this item is added to the <see cref="ListBox"/> but after the <see cref="ListBox"/> and <see cref="Model"/> references are set
    /// </summary>
    protected abstract void OnAddingToList();

    /// <summary>
    /// Invoked once the styling and template is applied and we're added to the <see cref="ListBox"/>.
    /// This can connect binders and add event handlers
    /// </summary>
    protected abstract void OnAddedToList();

    /// <summary>
    /// Invoked when we're about to be removed from our <see cref="ListBox"/>.
    /// This can disconnect binders and remove event handlers
    /// </summary>
    protected abstract void OnRemovingFromList();

    /// <summary>
    /// Invoked once removed from <see cref="ListBox"/> but before the <see cref="ListBox"/> and <see cref="Model"/> references are cleared
    /// </summary>
    protected abstract void OnRemovedFromList();
}