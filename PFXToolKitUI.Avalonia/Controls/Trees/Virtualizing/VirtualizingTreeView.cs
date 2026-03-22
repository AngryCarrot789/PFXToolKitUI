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

using System.Collections.ObjectModel;
using Avalonia.Controls;
using PFXToolKitUI.Utils.Events;

namespace PFXToolKitUI.Avalonia.Controls.Trees.Virtualizing;

public abstract class VirtualizingTreeView<TModel> : BaseVirtualizingTreeView where TModel : class {
    public VirtualizingController<TModel>? VirtualizingController {
        get => field;
        set => PropertyHelper.SetAndRaiseINE(ref field, value, this, static (t, o, n) => t.OnVirtualizingControllerChanged(o, n));
    }

    private readonly ObservableCollection<FlatNode> visibleNodes;
    private readonly Dictionary<TModel, FlatNode> tagToNode;

    internal sealed class FlatNode(TModel model) {
        public readonly TModel Model = model;
        public int Depth;
        public bool IsExpanded;
    }

    public event EventHandler<ValueChangedEventArgs<VirtualizingController<TModel>?>>? VirtualizingControllerChanged;

    private readonly EventHandler<ItemsAddOrRemoveEventArgs<TModel>> nodeAddedHandler;
    private readonly EventHandler<ItemsAddOrRemoveEventArgs<TModel>> nodeRemovedHandler;
    private readonly EventHandler<ItemMoveEventArgs<TModel>> nodeMovedHandler;

    protected VirtualizingTreeView() {
        this.tagToNode = new Dictionary<TModel, FlatNode>();
        this.ItemsSource = this.visibleNodes = new ObservableCollection<FlatNode>();

        this.nodeAddedHandler = this.OnNodeAdded;
        this.nodeRemovedHandler = this.OnNodeRemoved;
        this.nodeMovedHandler = this.OnNodeMoved;
    }

    static VirtualizingTreeView() {
    }

    // Do we need a container?
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey) {
        return this.NeedsContainer<VirtualizingTreeViewItem<TModel>>(item, out recycleKey);
    }

    // Create container
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey) {
        return new VirtualizingTreeViewItem<TModel>();
    }

    // Prepare container
    protected override void PrepareContainerForItemOverride(Control container, object? item, int index) {
        base.PrepareContainerForItemOverride(container, item, index);
        ((VirtualizingTreeViewItem<TModel>) container).InternalOnUse(this, (FlatNode) item!);
    }

    // Container is prepared and ready
    protected override void ContainerForItemPreparedOverride(Control container, object? item, int index) {
        base.ContainerForItemPreparedOverride(container, item, index);
    }

    protected override void ContainerIndexChangedOverride(Control container, int oldIndex, int newIndex) {
        base.ContainerIndexChangedOverride(container, oldIndex, newIndex);
    }

    // The container is no longer in use but may be used later; virtualization is happening
    protected override void ClearContainerForItemOverride(Control container) {
        base.ClearContainerForItemOverride(container);
        ((VirtualizingTreeViewItem<TModel>) container).InternalOnRecycle();
    }

    private void OnVirtualizingControllerChanged(VirtualizingController<TModel>? oldController, VirtualizingController<TModel>? newController) {
        if (oldController != null) {
            oldController.RemoveNodeAddedHandler(oldController.RootNode, this.nodeAddedHandler);
            oldController.RemoveNodeRemovedHandler(oldController.RootNode, this.nodeRemovedHandler);
            oldController.RemoveNodeMovedHandler(oldController.RootNode, this.nodeMovedHandler);
            foreach (FlatNode node in this.visibleNodes) {
                this.DetachEvents(node.Model, false);
            }

            this.visibleNodes.Clear();
            this.tagToNode.Clear();
        }

        if (newController != null) {
            newController.AddNodeAddedHandler(newController.RootNode, this.nodeAddedHandler);
            newController.AddNodeRemovedHandler(newController.RootNode, this.nodeRemovedHandler);
            newController.AddNodeMovedHandler(newController.RootNode, this.nodeMovedHandler);

            int insertIndex = 0;
            List<FlatNode> expand = new List<FlatNode>();
            foreach (TModel model in newController.GetChildren(newController.RootNode)) {
                FlatNode childFlat = this.GetOrCreateFlatNode(model);
                childFlat.Depth = 0;
                this.AttachEvents(childFlat.Model);

                this.visibleNodes.Insert(insertIndex++, childFlat);
                if (childFlat.IsExpanded) {
                    expand.Add(childFlat);
                }
            }

            foreach (FlatNode toExpand in expand) {
                this.Expand(toExpand);
            }
        }

        this.VirtualizingControllerChanged?.Invoke(this, new ValueChangedEventArgs<VirtualizingController<TModel>?>(oldController, newController));
    }

    private void OnNodeAdded(object? sender, ItemsAddOrRemoveEventArgs<TModel> e) {
        List<FlatNode>? expand = null;
        FlatNode parentFlat = this.GetOrCreateFlatNode((TModel) sender!);
        if (!parentFlat.IsExpanded) {
            return;
        }

        foreach (TModel node in e.Items) {
            FlatNode flatNode = this.CreateAndAddNode(e.Index, parentFlat, node);
            if (flatNode.IsExpanded) {
                (expand ??= []).Add(flatNode);
            }
        }

        if (expand != null) {
            foreach (FlatNode toExpand in expand) {
                this.Expand(toExpand);
            }
        }
    }

    private FlatNode CreateAndAddNode(int index, FlatNode parent, TModel node) {
        int insertIndex = this.GetInsertionIndex(parent.Model, this.GetInsertionIndex(parent), index);

        FlatNode childFlat = this.GetOrCreateFlatNode(node);
        childFlat.Depth = parent.Depth + 1;
        this.AttachEvents(childFlat.Model);

        this.visibleNodes.Insert(insertIndex, childFlat);
        return childFlat;
    }

    private int GetInsertionIndex(TModel parent, int initialIndex, int countTo) {
        for (int i = 0; i < countTo; i++) {
            TModel sibling = this.VirtualizingController!.GetChildAt(parent, i);
            if (this.tagToNode.TryGetValue(sibling, out FlatNode? siblingFlat))
                initialIndex += this.CountVisibleSubtree(siblingFlat);
        }

        return initialIndex;
    }

    private void OnNodeRemoved(object? sender, ItemsAddOrRemoveEventArgs<TModel> e) {
        foreach (TModel model in e.Items) {
            this.OnNodeRemoved(model);
        }
    }

    private void OnNodeRemoved(TModel node) {
        if (!this.tagToNode.Remove(node, out FlatNode? childFlat)) {
            return;
        }

        int startIndex = this.visibleNodes.IndexOf(childFlat);
        if (startIndex >= 0) {
            int count = this.CountVisibleSubtree(childFlat);
            for (int i = 0; i < count; i++) {
                this.DetachEvents(this.visibleNodes[startIndex].Model, false);
                this.visibleNodes.RemoveAt(startIndex);
            }
        }
    }

    private void OnNodeMoved(object? sender, ItemMoveEventArgs<TModel> e) {
        this.OnNodeRemoved(e.Item);
        FlatNode parentFlat = this.GetOrCreateFlatNode((TModel) sender!);
        if (!parentFlat.IsExpanded) {
            return;
        }

        FlatNode flatNode = this.CreateAndAddNode(e.NewIndex, parentFlat, e.Item);
        if (flatNode.IsExpanded) {
            this.Expand(flatNode);
        }
    }

    private void AttachEvents(TModel node) {
        this.VirtualizingController!.AddNodeAddedHandler(node, this.nodeAddedHandler);
        this.VirtualizingController!.AddNodeRemovedHandler(node, this.nodeRemovedHandler);
        this.VirtualizingController!.AddNodeMovedHandler(node, this.nodeMovedHandler);
    }

    private void DetachEvents(TModel node, bool recursive) {
        this.VirtualizingController!.RemoveNodeAddedHandler(node, this.nodeAddedHandler);
        this.VirtualizingController!.RemoveNodeRemovedHandler(node, this.nodeRemovedHandler);
        this.VirtualizingController!.RemoveNodeMovedHandler(node, this.nodeMovedHandler);
        if (recursive) {
            DetachEventsRecursive(node);
        }

        return;

        void DetachEventsRecursive(TModel parent) {
            int count = this.VirtualizingController!.GetChildrenCount(parent);
            for (int i = 0; i < count; i++) {
                TModel child = this.VirtualizingController!.GetChildAt(parent, i);
                this.VirtualizingController!.RemoveNodeAddedHandler(child, this.nodeAddedHandler);
                this.VirtualizingController!.RemoveNodeRemovedHandler(child, this.nodeRemovedHandler);
                this.VirtualizingController!.RemoveNodeMovedHandler(child, this.nodeMovedHandler);

                DetachEventsRecursive(child);
            }
        }
    }

    private FlatNode GetOrCreateFlatNode(TModel node) {
        if (!this.tagToNode.TryGetValue(node, out FlatNode? flat)) {
            flat = new FlatNode(node);
            this.tagToNode[node] = flat;
        }

        return flat;
    }

    private int GetInsertionIndex(FlatNode node) {
        int depth = node.Depth;

        int i = this.visibleNodes.IndexOf(node) + 1;
        while (i < this.visibleNodes.Count && this.visibleNodes[i].Depth > depth)
            i++;

        return i;
    }

    private int CountVisibleSubtree(FlatNode node) {
        int count = 1; // the node itself
        int index = this.visibleNodes.IndexOf(node) + 1;
        while (index < this.visibleNodes.Count && this.visibleNodes[index].Depth > node.Depth) {
            count++;
            index++;
        }

        return count;
    }

    internal void Expand(FlatNode node) {
        if (node.IsExpanded) {
            return;
        }

        node.IsExpanded = true;
        int insertIndex = this.GetInsertionIndex(node);
        foreach (TModel child in this.VirtualizingController!.GetChildren(node.Model)) {
            FlatNode childFlat = this.GetOrCreateFlatNode(child);
            childFlat.Depth = node.Depth + 1;

            this.AttachEvents(childFlat.Model);
            this.visibleNodes.Insert(insertIndex++, childFlat);
            if (childFlat.IsExpanded) {
                this.Expand(childFlat);
            }
        }
    }

    internal void Collapse(FlatNode node) {
        if (!node.IsExpanded) {
            return;
        }

        node.IsExpanded = false;
        int index = this.visibleNodes.IndexOf(node) + 1;
        while (index < this.visibleNodes.Count && this.visibleNodes[index].Depth > node.Depth) {
            this.DetachEvents(node.Model, false);
            this.visibleNodes.RemoveAt(index);
        }
    }

    // Optional helper for UI toggle
    internal void Toggle(FlatNode node) {
        if (node.IsExpanded)
            this.Collapse(node);
        else
            this.Expand(node);
    }
}