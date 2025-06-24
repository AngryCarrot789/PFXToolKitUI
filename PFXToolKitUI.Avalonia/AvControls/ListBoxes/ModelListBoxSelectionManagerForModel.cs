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

namespace PFXToolKitUI.Avalonia.AvControls.ListBoxes;

public class ModelListBoxSelectionManagerForModel<TModel> : IListSelectionManager<TModel> where TModel : class {
    private readonly ModelBasedListBox<TModel> listBox;
    private readonly AvaloniaList<object> selectedControls;

    public IEnumerable<TModel> SelectedItems => this.SelectedItemList;

    public IList<TModel> SelectedItemList { get; }
    
    private bool isBatching;
    private List<TModel>? batchResources_old;
    private List<TModel>? batchResources_new;

    public event SelectionChangedEventHandler<TModel>? SelectionChanged;
    public event SelectionClearedEventHandler<TModel>? SelectionCleared;
    public event LightSelectionChangedEventHandler<TModel>? LightSelectionChanged;

    public ModelListBoxSelectionManagerForModel(ModelBasedListBox<TModel> listBox) {
        this.selectedControls = new AvaloniaList<object>();
        this.selectedControls.CollectionChanged += this.OnSelectedItemsChanged;

        this.SelectedItemList = new CastingList(this);
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
        List<TModel>? oldList = oldItems?.Cast<ModelBasedListBoxItem<TModel>>().Select(x => x.Model!).ToList();
        List<TModel>? newList = newItems?.Cast<ModelBasedListBoxItem<TModel>>().Select(x => x.Model!).ToList();
        if (this.isBatching) {
            // Batch them into one final event that will get called after isBatching is set to false
            if (newList != null && newList.Count > 0)
                (this.batchResources_new ??= new List<TModel>()).AddRange(newList);
            if (oldList != null && oldList.Count > 0)
                (this.batchResources_old ??= new List<TModel>()).AddRange(oldList);
        }
        else if (oldList?.Count > 0 || newList?.Count > 0) {
            this.RaiseSelectionChanged(GetList(oldList), GetList(newList));
        }
    }

    public bool IsSelected(TModel item) {
        return this.listBox.ItemMap.TryGetControl(item, out ModelBasedListBoxItem<TModel>? control) && control.IsSelected;
    }

    public void SetSelection(TModel item) {
        this.Clear();
        this.Select(item);
    }

    public void SetSelection(IEnumerable<TModel> items) {
        this.Clear();
        this.Select(items);
    }

    public void Select(TModel item) {
        if (this.listBox.ItemMap.TryGetControl(item, out ModelBasedListBoxItem<TModel>? control)) {
            control.IsSelected = true;
        }
    }

    public void Select(IEnumerable<TModel> items) {
        try {
            this.isBatching = true;
            foreach (TModel resource in items) {
                this.Select(resource);
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

    public void Unselect(TModel item) {
        if (this.listBox.ItemMap.TryGetControl(item, out ModelBasedListBoxItem<TModel>? control)) {
            control.IsSelected = false;
        }
    }

    public void Unselect(IEnumerable<TModel> items) {
        try {
            this.isBatching = true;
            foreach (TModel resource in items) {
                this.Unselect(resource);
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

    public void ToggleSelected(TModel item) {
        if (this.listBox.ItemMap.TryGetControl(item, out ModelBasedListBoxItem<TModel>? control)) {
            control.IsSelected = !control.IsSelected;
        }
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

    private void RaiseSelectionChanged(ReadOnlyCollection<TModel>? oldList, ReadOnlyCollection<TModel>? newList) {
        if (ReferenceEquals(oldList, newList) || (oldList?.Count < 1 && newList?.Count < 1)) {
            return;
        }

        this.SelectionChanged?.Invoke(this, oldList, newList);
        this.LightSelectionChanged?.Invoke(this);
    }

    public void RaiseSelectionCleared() {
        this.SelectionCleared?.Invoke(this);
        this.LightSelectionChanged?.Invoke(this);
    }

    private static ReadOnlyCollection<TModel>? GetList(List<TModel>? list) => list == null || list.Count < 1 ? null : list.AsReadOnly();

    private class CastingList : IList<TModel> {
        private readonly ModelListBoxSelectionManagerForModel<TModel> owner;

        public int Count => ((IListSelectionManager<TModel>) this.owner).Count;
        public bool IsReadOnly => false;

        public TModel this[int index] {
            get => this.owner.listBox.ItemMap.GetModel((ModelBasedListBoxItem<TModel>) this.owner.selectedControls[index]!);
            set => this.owner.selectedControls[index] = this.owner.listBox.ItemMap.GetControl(value);
        }
        
        public CastingList(ModelListBoxSelectionManagerForModel<TModel> owner) {
            this.owner = owner;
        }

        public IEnumerator<TModel> GetEnumerator() => this.owner.selectedControls.Select(x => this.owner.listBox.ItemMap.GetModel((ModelBasedListBoxItem<TModel>) x)).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public void Add(TModel item) => this.owner.Select(item);

        public void Clear() => this.owner.Clear();

        public bool Contains(TModel item) => this.owner.IsSelected(item);

        public void CopyTo(TModel[] array, int arrayIndex) {
            foreach (TModel item in this)
                array[arrayIndex++] = item;
        }

        public bool Remove(TModel item) {
            if (!this.owner.IsSelected(item))
                return false;
            this.owner.Unselect(item);
            return true;
        }

        public int IndexOf(TModel item) => this.owner.selectedControls.IndexOf(this.owner.listBox.ItemMap.GetControl(item));

        public void Insert(int index, TModel item) => this.owner.selectedControls.Insert(index, this.owner.listBox.ItemMap.GetControl(item));

        public void RemoveAt(int index) => this.owner.selectedControls.RemoveAt(index);
    }
}