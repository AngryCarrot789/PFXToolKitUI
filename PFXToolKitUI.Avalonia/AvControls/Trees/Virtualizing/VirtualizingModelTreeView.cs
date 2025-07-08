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

namespace PFXToolKitUI.Avalonia.AvControls.Trees.Virtualizing;

/// <summary>
/// A tree view that uses a model-view connection, and supports virtualization of tree items. Only visible items will exist
/// </summary>
public class VirtualizingModelTreeView : ListBox {
    public VirtualizingModelTreeView() {
        // VirtualizingStackPanel panel = new VirtualizingStackPanel();
    }

    protected override void ContainerIndexChangedOverride(Control container, int oldIndex, int newIndex) {
        base.ContainerIndexChangedOverride(container, oldIndex, newIndex);
    }

    protected override void ContainerForItemPreparedOverride(Control container, object? item, int index) {
        base.ContainerForItemPreparedOverride(container, item, index);
    }

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey) {
        return base.NeedsContainerOverride(item, index, out recycleKey);
    }

    protected override void ClearContainerForItemOverride(Control element) {
        base.ClearContainerForItemOverride(element);
    }

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey) {
        return base.CreateContainerForItemOverride(item, index, recycleKey);
    }

    protected override void PrepareContainerForItemOverride(Control container, object? item, int index) {
        base.PrepareContainerForItemOverride(container, item, index);
    }
}