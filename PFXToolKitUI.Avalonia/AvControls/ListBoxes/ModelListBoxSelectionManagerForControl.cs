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

using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia.Collections;
using PFXToolKitUI.Interactivity;
using PFXToolKitUI.Utils.Events;

namespace PFXToolKitUI.Avalonia.AvControls.ListBoxes;

public class ModelListBoxSelectionManagerForControl<TModel, TControl> : IListSelectionManager<TControl> where TModel : class where TControl : class {
    private readonly ModelBasedListBox<TModel> listBox;
    private readonly AvaloniaList<object> selectedControls;
    
    public IList<TControl> SelectedItemList { get; }

    public IEnumerable<TControl> SelectedItems => this.SelectedItemList;
    
    private bool isBatching;
    private List<TControl>? batchResources_old;
    private List<TControl>? batchResources_new;

    public event EventHandler<ValueChangedEventArgs<IList<TControl>?>>? SelectionChanged;
    public event EventHandler? SelectionCleared;
    public event EventHandler? LightSelectionChanged;

    public ModelListBoxSelectionManagerForControl(ModelBasedListBox<TModel> listBox) {
        this.selectedControls = new AvaloniaList<object>();
        this.selectedControls.CollectionChanged += this.OnSelectedItemsChanged;
        this.SelectedItemList = new CastingList(this.selectedControls);
        this.listBox = listBox;
        this.listBox.SelectedItems = this.selectedControls;
    }

    private void OnSelectedItemsChanged(object? sender, NotifyCollectionChangedEventArgs e) {
        switch (e.Action) {
            case NotifyCollectionChangedAction.Add:     this.ProcessTreeSelection(null, e.NewItems ?? null); break;
            case NotifyCollectionChangedAction.Remove:  this.ProcessTreeSelection(e.OldItems, null); break;
            case NotifyCollectionChangedAction.Replace: this.ProcessTreeSelection(e.OldItems, e.NewItems ?? null); break;
            case NotifyCollectionChangedAction.Reset:   this.RaiseSelectionCleared(); break;
            case NotifyCollectionChangedAction.Move:    break;
            default:                                    throw new ArgumentOutOfRangeException();
        }
    }

    internal void ProcessTreeSelection(IList? oldItems, IList? newItems) {
        List<TControl>? oldList = oldItems?.Cast<TControl>().ToList();
        List<TControl>? newList = newItems?.Cast<TControl>().ToList();
        if (this.isBatching) {
            // Batch them into one final event that will get called after isBatching is set to false
            if (newList != null && newList.Count > 0)
                (this.batchResources_new ??= new List<TControl>()).AddRange(newList);
            if (oldList != null && oldList.Count > 0)
                (this.batchResources_old ??= new List<TControl>()).AddRange(oldList);
        }
        else if (oldList?.Count > 0 || newList?.Count > 0) {
            this.RaiseSelectionChanged(GetList(oldList), GetList(newList));
        }
    }

    public bool IsSelected(TControl item) {
        return ((ModelBasedListBoxItem<TModel>) (object) item).IsSelected;
    }

    public void SetSelection(TControl item) {
        this.Clear();
        this.Select(item);
    }

    public void SetSelection(IEnumerable<TControl> items) {
        this.Clear();
        this.Select(items);
    }

    public void Select(TControl item) {
        ((ModelBasedListBoxItem<TModel>) (object) item).IsSelected = true;
    }

    public void Select(IEnumerable<TControl> items) {
        try {
            this.isBatching = true;
            foreach (TControl resource in items) {
                ((ModelBasedListBoxItem<TModel>) (object) resource).IsSelected = true;
            }
        }
        finally {
            this.isBatching = false;
        }

        try {
            this.RaiseSelectionChanged(GetList(this.batchResources_old), GetList(this.batchResources_new));
        }
        finally {
            this.batchResources_old?.Clear();
            this.batchResources_new?.Clear();
        }
    }

    public void Unselect(TControl item) {
        ModelBasedListBoxItem<TModel> control = (ModelBasedListBoxItem<TModel>) (object) item;
        control.IsSelected = false;
    }

    public void Unselect(IEnumerable<TControl> items) {
        try {
            this.isBatching = true;
            foreach (TControl resource in items) {
                ((ModelBasedListBoxItem<TModel>) (object) resource).IsSelected = false;
            }
        }
        finally {
            this.isBatching = false;
        }

        try {
            this.RaiseSelectionChanged(GetList(this.batchResources_old), GetList(this.batchResources_new));
        }
        finally {
            this.batchResources_old?.Clear();
            this.batchResources_new?.Clear();
        }
    }

    public void ToggleSelected(TControl item) {
        ModelBasedListBoxItem<TModel> control = (ModelBasedListBoxItem<TModel>) (object) item;
        control.IsSelected = !control.IsSelected;
    }

    public void Clear() {
        this.selectedControls.Clear();
    }

    public void SelectAll() {
        try {
            this.isBatching = true;
            foreach (ModelBasedListBoxItem<TModel> control in this.listBox.ItemMap.Controls) {
                control.IsSelected = true;
            }
        }
        finally {
            this.isBatching = false;
        }

        try {
            this.RaiseSelectionChanged(GetList(this.batchResources_old), GetList(this.batchResources_new));
        }
        finally {
            this.batchResources_old?.Clear();
            this.batchResources_new?.Clear();
        }
    }

    private void RaiseSelectionChanged(ReadOnlyCollection<TControl>? oldList, ReadOnlyCollection<TControl>? newList) {
        if (ReferenceEquals(oldList, newList) || (oldList?.Count < 1 && newList?.Count < 1)) {
            return;
        }

        this.SelectionChanged?.Invoke(this, new ValueChangedEventArgs<IList<TControl>?>(oldList, newList));
        this.LightSelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    public void RaiseSelectionCleared() {
        this.SelectionCleared?.Invoke(this, EventArgs.Empty);
        this.LightSelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    private static ReadOnlyCollection<TControl>? GetList(List<TControl>? list) => list == null || list.Count < 1 ? null : list.AsReadOnly();

    private class CastingList : IList<TControl> {
        private readonly AvaloniaList<object> selection;

        public int Count => this.selection.Count;
        public bool IsReadOnly => false;

        public TControl this[int index] {
            get => (TControl) this.selection[index]!;
            set => this.selection[index] = value;
        }

        public CastingList(AvaloniaList<object> items) {
            this.selection = items;
        }

        public IEnumerator<TControl> GetEnumerator() => this.selection.Cast<TControl>().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public void Add(TControl item) {
            if (!this.selection.Contains(item))
                this.selection.Add(item);
        }

        public void Clear() => this.selection.Clear();

        public bool Contains(TControl item) => this.selection.Contains(item);

        public void CopyTo(TControl[] array, int arrayIndex) {
            foreach (TControl item in this.selection)
                array[arrayIndex++] = item;
        }

        public bool Remove(TControl item) {
            if (!this.selection.Contains(item))
                return false;
            this.selection.Remove(item);
            return true;
        }

        public int IndexOf(TControl item) => this.selection.IndexOf(item);

        public void Insert(int index, TControl item) => this.selection.Insert(index, item);

        public void RemoveAt(int index) => this.selection.RemoveAt(index);
    }
}