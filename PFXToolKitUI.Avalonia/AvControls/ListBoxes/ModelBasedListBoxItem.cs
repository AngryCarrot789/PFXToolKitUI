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

namespace PFXToolKitUI.Avalonia.AvControls.ListBoxes;

public abstract class ModelBasedListBoxItem<TModel> : BaseModelBasedListBoxItem where TModel : class {
    public new ModelBasedListBox<TModel>? ListBox {
        get => (ModelBasedListBox<TModel>?) base.ListBox;
        internal set => base.ListBox = value;
    }

    public TModel? Model { get; private set; }

    protected ModelBasedListBoxItem() {
    }

    internal void InternalOnAddingToList(ModelBasedListBox<TModel> explorerList, TModel resource) {
        this.ListBox = explorerList;
        this.Model = resource;
        this.OnAddingToList();
    }

    internal void InternalOnAddedToList() => this.OnAddedToList();

    internal void InternalOnRemovingFromList() => this.OnRemovingFromList();

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