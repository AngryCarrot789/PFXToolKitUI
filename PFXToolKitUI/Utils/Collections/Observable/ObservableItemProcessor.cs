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

using PFXToolKitUI.Utils.Events;

namespace PFXToolKitUI.Utils.Collections.Observable;

/// <summary>
/// A helper class for creating observable item processors
/// </summary>
public static class ObservableItemProcessor {
    /// <summary>
    /// Creates a simple item processor that invokes two callbacks when an item is added to or removed from the collection 
    /// </summary>
    /// <param name="list"></param>
    /// <param name="onItemAdded"></param>
    /// <param name="onItemRemoved"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static ObservableItemProcessorSimple<T> MakeSimple<T>(IObservableList<T> list, Action<T>? onItemAdded, Action<T>? onItemRemoved) {
        return new ObservableItemProcessorSimple<T>(list, onItemAdded, onItemRemoved);
    }

    /// <summary>
    /// Creates an item processor that provides the index of added and removed items
    /// </summary>
    /// <param name="list"></param>
    /// <param name="onItemAdded"></param>
    /// <param name="onItemRemoved"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static ObservableItemProcessorIndexing<T> MakeIndexable<T>(IObservableList<T> list, Action<ItemAddOrRemoveEventArgs<T>>? onItemAdded, Action<ItemAddOrRemoveEventArgs<T>>? onItemRemoved, Action<ItemMoveEventArgs<T>>? onItemMoved, bool useOptimisedRemovalProcessing = true) {
        return new ObservableItemProcessorIndexing<T>(list, onItemAdded, onItemRemoved, onItemMoved, useOptimisedRemovalProcessing);
    }
}

public sealed class ObservableItemProcessorSimple<T> : IDisposable {
    private readonly IObservableList<T> list;

    public event Action<T>? OnItemAdded;
    public event Action<T>? OnItemRemoved;

    public ObservableItemProcessorSimple(IObservableList<T> list, Action<T>? itemAdded, Action<T>? itemRemoved) {
        this.list = list ?? throw new ArgumentNullException(nameof(list));
        list.ItemsAdded += this.OnItemsAdded;
        list.ItemsRemoved += this.OnItemsRemoved;
        list.ItemReplaced += this.OnItemReplaced;

        this.OnItemAdded = itemAdded;
        this.OnItemRemoved = itemRemoved;
    }

    private void OnItemsAdded(object? sender, ItemsAddOrRemoveEventArgs<T> e) {
        Action<T>? handler = this.OnItemAdded;
        if (handler != null) {
            foreach (T item in e.Items) {
                handler(item);
            }
        }
    }

    private void OnItemsRemoved(object? sender, ItemsAddOrRemoveEventArgs<T> e) {
        Action<T>? handler = this.OnItemRemoved;
        if (handler != null) {
            foreach (T item in e.Items) {
                handler(item);
            }
        }
    }

    private void OnItemReplaced(object? sender, ItemReplaceEventArgs<T> e) {
        this.OnItemRemoved?.Invoke(e.OldItem);
        this.OnItemAdded?.Invoke(e.NewItem);
    }

    public void Dispose() {
        this.list.ItemsAdded -= this.OnItemsAdded;
        this.list.ItemsRemoved -= this.OnItemsRemoved;
        this.list.ItemReplaced -= this.OnItemReplaced;
    }
}

public sealed class ObservableItemProcessorIndexing<T> : IDisposable {
    private readonly IObservableList<T> list;
    private readonly bool useOptimisedRemovalProcessing;

    public event Action<ItemAddOrRemoveEventArgs<T>>? OnItemAdded;
    public event Action<ItemAddOrRemoveEventArgs<T>>? OnItemRemoved;
    public event Action<ItemMoveEventArgs<T>>? OnItemMoved;

    public ObservableItemProcessorIndexing(IObservableList<T> list, Action<ItemAddOrRemoveEventArgs<T>>? itemAdded, Action<ItemAddOrRemoveEventArgs<T>>? itemRemoved, Action<ItemMoveEventArgs<T>>? itemMoved, bool useOptimisedRemovalProcessing) {
        this.list = list ?? throw new ArgumentNullException(nameof(list));
        this.useOptimisedRemovalProcessing = useOptimisedRemovalProcessing;
        list.ItemsAdded += this.ItemsAdded;
        list.ItemsRemoved += this.ItemsRemoved;
        list.ItemReplaced += this.ItemReplaced;
        list.ItemMoved += this.ItemMoved;

        this.OnItemAdded = itemAdded;
        this.OnItemRemoved = itemRemoved;
        this.OnItemMoved = itemMoved;
    }

    private void ItemsAdded(object? sender, ItemsAddOrRemoveEventArgs<T> e) {
        Action<ItemAddOrRemoveEventArgs<T>>? handler = this.OnItemAdded;
        if (handler == null)
            return;

        int i = e.Index - 1;
        foreach (T item in e.Items)
            handler(new ItemAddOrRemoveEventArgs<T>(++i, item));
    }

    private void ItemsRemoved(object? sender, ItemsAddOrRemoveEventArgs<T> e) {
        Action<ItemAddOrRemoveEventArgs<T>>? handler = this.OnItemRemoved;
        if (handler != null) {
            if (this.useOptimisedRemovalProcessing) {
                // Remove back to front, as it's usually the most performant for array-based
                // list implementations that may be modified by the handler,
                // since they will do overall less copying
                for (int j = e.Items.Count, i = e.Index + j - 1; i >= e.Index; i--) {
                    // i = index of last item in unaware listeners' list, points to junk in real list
                    // j = index in items parameter list
                    handler(new ItemAddOrRemoveEventArgs<T>(i, e.Items[--j]));
                }
            }
            else {
                foreach (T item in e.Items)
                    handler(new ItemAddOrRemoveEventArgs<T>(e.Index, item));
            }
        }
    }

    private void ItemReplaced(object? sender, ItemReplaceEventArgs<T> e) {
        this.OnItemRemoved?.Invoke(new ItemAddOrRemoveEventArgs<T>(e.Index, e.OldItem));
        this.OnItemAdded?.Invoke(new ItemAddOrRemoveEventArgs<T>(e.Index, e.NewItem));
    }

    private void ItemMoved(object? sender, ItemMoveEventArgs<T> e) {
        this.OnItemMoved?.Invoke(new ItemMoveEventArgs<T>(e.OldIndex, e.NewIndex, e.Item));
    }

    public void Dispose() {
        this.list.ItemsAdded -= this.ItemsAdded;
        this.list.ItemsRemoved -= this.ItemsRemoved;
        this.list.ItemReplaced -= this.ItemReplaced;
        this.list.ItemMoved -= this.ItemMoved;
    }

    /// <summary>
    /// Invokes our item add handler(s) on all items for the list
    /// </summary>
    public ObservableItemProcessorIndexing<T> AddExistingItems() {
        Action<ItemAddOrRemoveEventArgs<T>>? handler = this.OnItemAdded;
        if (handler != null) {
            int i = -1;
            foreach (T item in this.list)
                handler(new ItemAddOrRemoveEventArgs<T>(++i, item));
        }

        return this;
    }

    /// <summary>
    /// Invokes our item remove handler(s) on all items for the list
    /// </summary>
    public ObservableItemProcessorIndexing<T> RemoveExistingItems(bool backToFront = true) {
        Action<ItemAddOrRemoveEventArgs<T>>? handler = this.OnItemRemoved;
        if (handler != null) {
            if (backToFront) {
                for (int i = this.list.Count - 1; i >= 0; i--) {
                    handler(new ItemAddOrRemoveEventArgs<T>(i, this.list[i]));
                }
            }
            else {
                for (int i = 0; i < this.list.Count; i++) {
                    handler(new ItemAddOrRemoveEventArgs<T>(i, this.list[i]));
                }
            }
        }

        return this;
    }
}
