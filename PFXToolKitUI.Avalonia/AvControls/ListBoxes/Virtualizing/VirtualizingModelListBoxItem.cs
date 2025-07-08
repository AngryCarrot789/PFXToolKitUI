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
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.Avalonia.AvControls.ListBoxes.Virtualizing;

public abstract class VirtualizingModelListBoxItem : ListBoxItem {
    private object? model;

    /// <summary>
    /// Gets the model currently assigned to this list box item
    /// </summary>
    public object? Model {
        get => this.model;
        internal set => PropertyHelper.SetAndRaiseINE(ref this.model, value, this, static (t, o, n) => t.OnModelChanged(o, n));
    }

    protected VirtualizingModelListBoxItem() {
    }

    protected virtual void OnModelChanged(object? oldModel, object? newModel) {
    }

    internal void InternalOnAddingToList() => this.OnAddingToList();
    internal void InternalOnAddedToList() => this.OnAddedToList();
    internal void InternalOnRemovingFromList() => this.OnRemovingFromList();
    internal void InternalOnRemovedFromList() => this.OnRemovedFromList();
    internal void InternalOnContainerIndexChanged(int oldIndex, int newIndex) => this.OnContainerIndexChanged(oldIndex, newIndex);
    
    protected abstract void OnAddingToList();
    
    protected abstract void OnAddedToList();
    
    protected abstract void OnRemovingFromList();
    
    protected abstract void OnRemovedFromList();

    protected virtual void OnContainerIndexChanged(int oldIndex, int newIndex) {
        
    }
}

public abstract class VirtualizingModelListBoxItem<T> : VirtualizingModelListBoxItem where T : class {
    public new T? Model => (T?) base.Model;

    protected VirtualizingModelListBoxItem() {
    }

    protected sealed override void OnModelChanged(object? oldModel, object? newModel) {
        base.OnModelChanged(oldModel, newModel);
        this.OnModelChanged((T?) oldModel, (T?) newModel);
    }

    protected virtual void OnModelChanged(T? oldModel, T? newModel) {
    }
}