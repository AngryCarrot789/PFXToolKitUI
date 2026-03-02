// 
// Copyright (c) 2026-2026 REghZy
// 
// This file is part of MemoryEngine360.
// 
// MemoryEngine360 is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// MemoryEngine360 is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MemoryEngine360. If not, see <https://www.gnu.org/licenses/>.
// 

using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using PFXToolKitUI.Utils.Events;

namespace PFXToolKitUI.Avalonia.AvControls.Trees.Virtualizing;

[TemplatePart("PART_Header", typeof(Control))]
[PseudoClasses(":pressed", ":selected")]
public class VirtualizingTreeViewItem<TModel> : BaseVirtualizingTreeViewItem where TModel : class {
    private VirtualizingTreeView<TModel>? parentTreeView;
    private VirtualizingTreeView<TModel>.FlatNode? flatNode;
    
    public TModel? Model {
        get => field;
        set => PropertyHelper.SetAndRaiseINE(ref field, value, this, static (t, o, n) => t.OnModelChanged(o, n));
    }

    public event EventHandler<ValueChangedEventArgs<TModel?>>? ModelChanged;
    
    public VirtualizingTreeViewItem() {
    }

    static VirtualizingTreeViewItem() {
    }

    protected override void OnIsExpandedChanged() {
        base.OnIsExpandedChanged();
        
        if (this.IsExpanded) {
            this.parentTreeView?.Expand(this.flatNode!);
        }
        else {
            this.parentTreeView?.Collapse(this.flatNode!);
        }
    }

    internal void InternalOnUse(VirtualizingTreeView<TModel> parentTree, VirtualizingTreeView<TModel>.FlatNode node) {
        this.parentTreeView = parentTree;
        this.flatNode = node;
        this.Model = node.Model;
        this.Level = node.Depth;

        this.SetCurrentValue(IsSelectedProperty, false);
        this.SetCurrentValue(IsExpandedProperty, node.IsExpanded);
    }
    
    internal void InternalOnRecycle() {
        this.parentTreeView = null;
        this.flatNode = null;
        this.Model = null;
        this.Level = 0;
        
        this.ClearValue(IsSelectedProperty);
        this.ClearValue(IsExpandedProperty);
    }

    protected virtual void OnModelChanged(TModel? oldModel, TModel? newModel) {
        this.ModelChanged?.Invoke(this,new ValueChangedEventArgs<TModel?>(oldModel, newModel));
        this.Header = newModel?.ToString();
    }
}