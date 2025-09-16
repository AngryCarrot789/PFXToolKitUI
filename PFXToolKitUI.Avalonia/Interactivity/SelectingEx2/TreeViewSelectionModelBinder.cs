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
using System.Diagnostics;
using Avalonia.Collections;
using Avalonia.Controls;
using PFXToolKitUI.Interactivity.Selections;

namespace PFXToolKitUI.Avalonia.Interactivity.SelectingEx2;

public sealed class TreeViewSelectionModelBinder<T> where T : class {
    private readonly Func<TreeViewItem, T> tviToModel;
    private readonly Func<T, TreeViewItem> modelToTvi;
    private bool isUpdatingModel, isUpdatingControl;
    private INotifyCollectionChanged myNcc;

    public TreeView TreeView { get; }

    public TreeSelectionModel<T> Selection { get; }

    public TreeViewSelectionModelBinder(TreeView treeView, TreeSelectionModel<T> selection, Func<TreeViewItem, T> tviToModel, Func<T, TreeViewItem> modelToTvi) {
        this.tviToModel = tviToModel;
        this.modelToTvi = modelToTvi;
        this.TreeView = treeView;
        this.Selection = selection;

        if (!(this.TreeView.SelectedItems is INotifyCollectionChanged ncc)) {
            throw new InvalidOperationException($"TreeView's selected items list does not inherit from {nameof(INotifyCollectionChanged)}");
        }

        this.myNcc = ncc;
        this.myNcc.CollectionChanged += this.OnTreeViewSelectedItemsChanged;
        if (selection.Count > 0) {
            this.OnModelSelectionChanged(selection, new TreeSelectionModel<T>.ChangedEventArgs(selection.SelectedItems.ToList(), ReadOnlyCollection<T>.Empty));
        }

        treeView.SelectionChanged += this.OnTreeViewSelectionChanged;
        selection.SelectionChanged += this.OnModelSelectionChanged;
    }

    private void OnModelSelectionChanged(object? o, TreeSelectionModel<T>.ChangedEventArgs e) {
        Debug.WriteLine($"[TVI Selection]{(this.isUpdatingModel ? " [Reentry]" : "")} Model Changed | {e.AddedItems.Count} added | {e.RemovedItems.Count} removed");
        
        if (!this.isUpdatingModel) {
            Debug.Assert(!this.isUpdatingControl);
            this.isUpdatingControl = true;

            if (this.TreeView.SelectedItems is AvaloniaList<object> avList) {
                avList.RemoveAll(e.RemovedItems.Select(this.modelToTvi));
                avList.AddRange(e.AddedItems.Select(this.modelToTvi));
            }
            else {
                IList list = this.TreeView.SelectedItems;
                foreach (TreeViewItem tvi in e.RemovedItems.Select(this.modelToTvi))
                    list.Remove(tvi);
                foreach (TreeViewItem tvi in e.AddedItems.Select(this.modelToTvi))
                    list.Add(tvi);
            }
            
            this.isUpdatingControl = false;
        }
    }

    private void ProcessTreeSelection(IList removedItems, IList addedItems) {
        if (removedItems.Count > 0)
            this.Selection.DeselectItems(removedItems.Cast<TreeViewItem>().Select(this.tviToModel));
        if (addedItems.Count > 0)
            this.Selection.SelectItems(addedItems.Cast<TreeViewItem>().Select(this.tviToModel));
    }
    
    // Handle the INotifyPropertyChanged.CollectionChanged for the SelectedItems list
    private void OnTreeViewSelectedItemsChanged(object? sender, NotifyCollectionChangedEventArgs e) {
        if (e.Action == NotifyCollectionChangedAction.Reset) {
            Debug.WriteLine($"[TVI Selection]{(this.isUpdatingControl ? " [Reentry]" : "")} Tree View Reset");
        }
        else {
            Debug.WriteLine($"[TVI Selection]{(this.isUpdatingControl ? " [Reentry]" : "")} Tree View Changed | {e.NewItems?.Count ?? 0} added | {e.OldItems?.Count ?? 0} removed");
        }
        
        if (!this.isUpdatingControl) {
            Debug.Assert(!this.isUpdatingModel);
            this.isUpdatingModel = true;

            IList oldList = e.OldItems ?? ReadOnlyCollection<object>.Empty; 
            IList newList = e.NewItems ?? ReadOnlyCollection<object>.Empty; 
            switch (e.Action) {
                case NotifyCollectionChangedAction.Add:
                    Debug.Assert(oldList.Count < 1 && newList.Count > 0);
                    this.ProcessTreeSelection(ReadOnlyCollection<object>.Empty, newList);
                    break;
                case NotifyCollectionChangedAction.Remove:  
                    Debug.Assert(newList.Count < 1 && oldList.Count > 0);
                    this.ProcessTreeSelection(oldList, ReadOnlyCollection<object>.Empty); break;
                case NotifyCollectionChangedAction.Replace:
                    Debug.Assert(newList.Count > 0 && oldList.Count > 0);
                    this.ProcessTreeSelection(oldList, newList); 
                    break;
                case NotifyCollectionChangedAction.Reset:   this.Selection.Clear(); break;
                case NotifyCollectionChangedAction.Move:    break;
                default:                                    throw new ArgumentOutOfRangeException();
            }

            this.isUpdatingModel = false;
        }
    }

    // We handle this to be notified when the SelectedItems list reference changes.
    private void OnTreeViewSelectionChanged(object? sender, SelectionChangedEventArgs e) {
        if (!this.isUpdatingControl) {
            IList newList = this.TreeView.SelectedItems;
            if (!ReferenceEquals(newList, this.myNcc)) {
                if (!(newList is INotifyCollectionChanged ncc)) {
                    throw new InvalidOperationException($"TreeView's selected items list does not inherit from {nameof(INotifyCollectionChanged)}");
                }

                this.myNcc.CollectionChanged -= this.OnTreeViewSelectedItemsChanged;
                this.myNcc = ncc;
                this.myNcc.CollectionChanged += this.OnTreeViewSelectedItemsChanged;
            }
        }
    }

    public void Dispose() {
        this.myNcc.CollectionChanged -= this.OnTreeViewSelectedItemsChanged;
        this.TreeView.SelectionChanged -= this.OnTreeViewSelectionChanged;
        this.Selection.SelectionChanged -= this.OnModelSelectionChanged;
    }
}