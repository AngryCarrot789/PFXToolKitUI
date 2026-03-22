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

using System.Collections;
using PFXToolKitUI.Utils.Events;

namespace PFXToolKitUI.Avalonia.Controls.Trees.Virtualizing;

public abstract class VirtualizingController<TModel> where TModel : class {
    /// <summary>
    /// Gets the rood node of the model tree
    /// </summary>
    public abstract TModel RootNode { get; }

    protected VirtualizingController() {
    }

    #region Hierarchy

    public virtual IEnumerable<TModel> GetChildren(TModel parent) {
        return new ChildrenEnumerable(this, parent);
    }

    public ChildrenEnumerable GetChildrenEx(TModel parent) {
        return new ChildrenEnumerable(this, parent);
    }

    /// <summary>
    /// Gets the number of children in a node.
    /// </summary>
    public abstract int GetChildrenCount(TModel parent);

    /// <summary>
    /// Gets the child node of a parent at the given index
    /// </summary>
    public abstract TModel GetChildAt(TModel parent, int index);

    #endregion

    #region Events

    public abstract void AddNodeAddedHandler(TModel model, EventHandler<ItemsAddOrRemoveEventArgs<TModel>> handler);
    public abstract void RemoveNodeAddedHandler(TModel model, EventHandler<ItemsAddOrRemoveEventArgs<TModel>> handler);

    public abstract void AddNodeRemovedHandler(TModel model, EventHandler<ItemsAddOrRemoveEventArgs<TModel>> handler);
    public abstract void RemoveNodeRemovedHandler(TModel model, EventHandler<ItemsAddOrRemoveEventArgs<TModel>> handler);

    public abstract void AddNodeMovedHandler(TModel model, EventHandler<ItemMoveEventArgs<TModel>> handler);
    public abstract void RemoveNodeMovedHandler(TModel model, EventHandler<ItemMoveEventArgs<TModel>> handler);

    #endregion

    public readonly struct ChildrenEnumerable(VirtualizingController<TModel> controller, TModel parent) : IEnumerable<TModel> {
        public VirtualizingController<TModel> Controller { get; } = controller;
        public TModel Parent { get; } = parent;

        public ChildrenEnumerator GetEnumerator() => new ChildrenEnumerator(this.Controller, this.Parent);

        IEnumerator<TModel> IEnumerable<TModel>.GetEnumerator() => this.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public struct ChildrenEnumerator : IEnumerator<TModel> {
            private readonly VirtualizingController<TModel> controller;
            private readonly TModel parent;
            private int index = -1;

            public ChildrenEnumerator(VirtualizingController<TModel> controller, TModel parent) {
                this.controller = controller;
                this.parent = parent;
            }

            public TModel Current => this.index == -1 ? throw new InvalidOperationException("Reached end or not moved yet") : this.controller.GetChildAt(this.parent, this.index);

            object IEnumerator.Current => this.Current;

            public void Dispose() {
            }

            public bool MoveNext() {
                return (++this.index) < this.controller.GetChildrenCount(this.parent);
            }

            public void Reset() {
                this.index = -1;
            }
        }
    }
}