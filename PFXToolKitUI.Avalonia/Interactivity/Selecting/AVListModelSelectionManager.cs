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

using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using PFXToolKitUI.Interactivity;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.Avalonia.Interactivity.Selecting;

/// <summary>
/// A selection manager that uses an avalonia list to store the selected list controls, and an item map to map between control and model
/// </summary>
public class AVListModelSelectionManager<TModel, TControl> : IListSelectionManager<TModel> where TModel : class where TControl : Control {
    private readonly AvaloniaList<object> mySelectionList;

    public ListBox ListBox { get; }

    public IEnumerable<TModel> SelectedItems => this.SelectedControls.Select(x => this.itemMap.GetModel(x));

    public IEnumerable<TControl> SelectedControls => this.mySelectionList.Cast<TControl>();

    public int Count => this.mySelectionList.Count;

    public event SelectionChangedEventHandler<TModel>? SelectionChanged;
    public event SelectionClearedEventHandler<TModel>? SelectionCleared;
    public event LightSelectionChangedEventHandler<TModel>? LightSelectionChanged;

    private bool isBatching;
    private List<TModel>? batchResources_old;
    private List<TModel>? batchResources_new;
    private readonly IModelControlDictionary<TModel,TControl> itemMap;
    private readonly AvaloniaProperty<bool> isSelectedProperty;

    public AVListModelSelectionManager(ListBox listBox, IModelControlDictionary<TModel, TControl> itemMap, AvaloniaProperty<bool> isSelectedProperty) {
        this.ListBox = listBox;
        this.ListBox.SelectedItems = this.mySelectionList = new AvaloniaList<object>();
        this.mySelectionList.CollectionChanged += this.OnSelectionCollectionChanged;
        this.itemMap = itemMap;
        this.isSelectedProperty = isSelectedProperty;
    }

    private void OnSelectionCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
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
        List<TModel>? oldList = oldItems?.Cast<TControl>().Select(x => this.itemMap.GetModel(x)).ToList();
        List<TModel>? newList = newItems?.Cast<TControl>().Select(x => this.itemMap.GetModel(x)).ToList();
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
        return this.itemMap.TryGetControl(item, out TControl? control) && (bool) (control.GetValue(this.isSelectedProperty) ?? BoolBox.False);
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
        if (this.itemMap.TryGetControl(item, out TControl? control)) {
            control.SetValue(this.isSelectedProperty, BoolBox.True);
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
        if (this.itemMap.TryGetControl(item, out TControl? control)) {
            control.SetValue(this.isSelectedProperty, BoolBox.False);
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
        if (this.itemMap.TryGetControl(item, out TControl? control)) {
            control.SetValue(this.isSelectedProperty, !(bool) (control.GetValue(this.isSelectedProperty) ?? BoolBox.False));
        }
    }

    public void Clear() {
        this.mySelectionList.Clear();
    }

    public void SelectAll() {
        try {
            this.isBatching = true;
            foreach (TControl control in this.itemMap.Controls) {
                control.SetValue(this.isSelectedProperty, BoolBox.True);
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
}