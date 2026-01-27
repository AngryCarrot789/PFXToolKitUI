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

using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.Interactivity.SelectionsEx;

/// <summary>
/// Manages a one dimensional list of selected indices
/// </summary>
/// <typeparam name="T">The type of item that is selectable</typeparam>
public sealed class SelectionModel<T> : ISelectionModel<T> {
    private static readonly IList<T> EmptyList = ReadOnlyCollection<T>.Empty;
    private readonly HashSet<T> selectedItems;

    public int Count => this.selectedItems.Count;

    public IReadOnlySet<T> SelectedItems => this.selectedItems;

    public event EventHandler<SelectionModelExChangedEventArgs<T>>? SelectionChanged;

    public SelectionModel() {
        this.selectedItems = new HashSet<T>();
    }

    public void SelectItem(T item) {
        if (this.selectedItems.Add(item)) {
            this.SelectionChanged?.Invoke(this, new SelectionModelExChangedEventArgs<T>([item], EmptyList));
        }
    }

    public void SelectItems(IEnumerable<T> items) {
        EventHandler<SelectionModelExChangedEventArgs<T>>? handlers = this.SelectionChanged;
        if (handlers != null) {
            List<T> added = this.selectedItems.UnionAddEx(items);
            if (added.Count > 0) {
                handlers(this, new SelectionModelExChangedEventArgs<T>(added.AsReadOnly(), EmptyList));
            }
        }
        else {
            // Optimized path with no SelectionChanged handlers (i.e. initial setup)
            foreach (T item in items) {
                this.selectedItems.Add(item);
            }
        }
    }

    public void SelectItems(IReadOnlyList<T> items, int index, int count) {
        EventHandler<SelectionModelExChangedEventArgs<T>>? handlers = this.SelectionChanged;
        if (handlers != null) {
            List<T> added = new List<T>(count);
            for (int i = 0; i < count; i++) {
                T item = items[index + i];
                if (this.selectedItems.Add(item)) {
                    added.Add(item);
                }
            }

            if (added.Count > 0) {
                handlers(this, new SelectionModelExChangedEventArgs<T>(added.AsReadOnly(), EmptyList));
            }
        }
        else {
            // Optimized path with no SelectionChanged handlers (i.e. initial setup)
            for (int i = 0; i < count; i++) {
                this.selectedItems.Add(items[index + i]);
            }
        }
    }

    public void DeselectItem(T item) {
        if (this.selectedItems.Remove(item)) {
            this.SelectionChanged?.Invoke(this, new SelectionModelExChangedEventArgs<T>(EmptyList, [item]));
        }
    }

    public void DeselectItems(IEnumerable<T> items) {
        EventHandler<SelectionModelExChangedEventArgs<T>>? handlers = this.SelectionChanged;
        if (handlers != null) {
            List<T> removed = this.selectedItems.UnionRemoveEx(items);
            if (removed.Count > 0) {
                handlers(this, new SelectionModelExChangedEventArgs<T>(EmptyList, removed.AsReadOnly()));
            }
        }
        else {
            // Optimized path with no SelectionChanged handlers (i.e. initial setup)
            foreach (T item in items) {
                this.selectedItems.Remove(item);
            }
        }
    }

    public bool IsSelected(T item) => this.selectedItems.Contains(item);

    public void DeselectAll() {
        if (this.selectedItems.Count > 0) {
            EventHandler<SelectionModelExChangedEventArgs<T>>? handlers = this.SelectionChanged;
            if (handlers != null) {
                List<T> changedList = this.selectedItems.ToList();
                this.selectedItems.Clear();
                handlers(this, new SelectionModelExChangedEventArgs<T>(EmptyList, changedList.AsReadOnly()));
            }
            else {
                this.selectedItems.Clear();
            }
        }
    }

    public void SetSelection(T item) {
        this.DeselectAll();
        this.SelectItem(item);
    }

    public void SetSelection(IEnumerable<T> items) {
        this.DeselectAll();
        this.SelectItems(items);
    }
    
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowForEmptySelectionModel() => throw new InvalidOperationException("Selection model is empty");
}