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

using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using PFXToolKitUI.Avalonia.AvControls;
using PFXToolKitUI.AdvancedMenuService;
using PFXToolKitUI.Icons;
using PFXToolKitUI.Interactivity.Contexts;
using PFXToolKitUI.Utils.Collections.Observable;

namespace PFXToolKitUI.Avalonia.AdvancedMenuService;

/// <summary>
/// A menu item that participates in the advanced menu service
/// </summary>
public class AdvancedMenuItem : MenuItem, IAdvancedMenuOrItem {
    protected override Type StyleKeyOverride => typeof(MenuItem);

    /// <summary>
    /// Gets the container object, that being, the root object that stores the menu item tree that this instance is in
    /// </summary>
    public IAdvancedMenu? OwnerMenu { get; private set; }

    /// <summary>
    /// Gets the parent context menu item node. This MAY be different from the logical parent menu item
    /// </summary>
    public IAdvancedMenuOrItem? ParentNode { get; private set; }

    public BaseContextEntry? Entry { get; private set; }

    bool IAdvancedMenuOrItem.IsOpen => this.IsSubMenuOpen;

    protected Dictionary<int, DynamicGroupPlaceholderContextObject>? dynamicInsertion;
    protected Dictionary<int, int>? dynamicInserted;
    private IconControl? myIconControl;

    public AdvancedMenuItem() { }

    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
        BaseContextEntry.InternalOnBecomeVisible(this.Entry!, this.OwnerMenu?.CapturedContext ?? EmptyContext.Instance);
        AdvancedMenuService.GenerateDynamicItems(this, ref this.dynamicInsertion, ref this.dynamicInserted);
        Dispatcher.UIThread.InvokeAsync(() => AdvancedMenuService.NormaliseSeparators(this), DispatcherPriority.Loaded);
    }

    protected override void OnUnloaded(RoutedEventArgs e) {
        base.OnUnloaded(e);
        BaseContextEntry.InternalOnBecomeHidden(this.Entry!);
        AdvancedMenuService.ClearDynamicItems(this, ref this.dynamicInserted);
    }

    public virtual void OnAdding(IAdvancedMenu menu, IAdvancedMenuOrItem parent, BaseContextEntry entry) {
        this.OwnerMenu = menu;
        this.ParentNode = parent;
        this.Entry = entry;
    }

    public virtual void OnAdded() {
        this.Header = this.Entry!.DisplayName;
        if (this.Entry.Description != null)
            ToolTip.SetTip(this, this.Entry.Description ?? "");

        if (this.Entry.Icon != null) {
            this.myIconControl = new IconControl() {
                Icon = this.Entry.Icon
            };

            this.Icon = this.myIconControl;
        }

        this.Entry.DisplayNameChanged += this.OnEntryDisplayNameChanged;
        this.Entry.DescriptionChanged += this.OnEntryDescriptionChanged;
        this.Entry.IconChanged += this.OnEntryIconChanged;

        if (this.Entry is ContextEntryGroup list) {
            AdvancedMenuService.InsertItemNodes(this, list.Items);
            list.Items.ItemsAdded += this.ItemsOnItemsAdded;
            list.Items.ItemsRemoved += this.ItemsOnItemsRemoved;
            list.Items.ItemMoved += this.ItemsOnItemMoved;
            list.Items.ItemReplaced += this.ItemsOnItemReplaced;
        }

        this.Entry.CanExecuteChanged += this.OnCanExecuteChanged;
    }

    public virtual void OnRemoving() {
        if (this.Entry is ContextEntryGroup list) {
            list.Items.ItemsAdded -= this.ItemsOnItemsAdded;
            list.Items.ItemsRemoved -= this.ItemsOnItemsRemoved;
            list.Items.ItemMoved -= this.ItemsOnItemMoved;
            list.Items.ItemReplaced -= this.ItemsOnItemReplaced;
        }

        this.Entry!.DisplayNameChanged -= this.OnEntryDisplayNameChanged;
        this.Entry!.DescriptionChanged -= this.OnEntryDescriptionChanged;
        this.Entry!.IconChanged -= this.OnEntryIconChanged;
        this.Entry!.CanExecuteChanged -= this.OnCanExecuteChanged;

        if (this.myIconControl != null) {
            this.myIconControl.Icon = null;
            this.myIconControl = null;
            this.Icon = null;
        }

        this.ClearValue(ToolTip.TipProperty);
        AdvancedMenuService.ClearDynamicItems(this, ref this.dynamicInserted);
        AdvancedMenuService.ClearItemNodes(this);
    }

    public virtual void OnRemoved() {
        this.OwnerMenu = null;
        this.ParentNode = null;
        this.Entry = null;
    }

    private void OnCanExecuteChanged(BaseContextEntry baseContextEntry) => this.UpdateCanExecute();

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change) {
        base.OnPropertyChanged(change);
        if (change.Property == IsSubMenuOpenProperty) {
            if (this.IsSubMenuOpen) {
                AdvancedMenuService.GenerateDynamicItems(this, ref this.dynamicInsertion, ref this.dynamicInserted);
                Debug.WriteLine($"Generated dynamic items for menu item shown: " + (this.dynamicInserted?.Count ?? 0));
            }
            else {
                Debug.WriteLine($"Cleared '{this.dynamicInserted?.Count ?? 0}' dynamic items for menu item hidden");
                AdvancedMenuService.ClearDynamicItems(this, ref this.dynamicInserted);
            }
        }
    }

    public virtual void UpdateCanExecute() {
    }

    public void StoreDynamicGroup(DynamicGroupPlaceholderContextObject groupPlaceholder, int index) {
        (this.dynamicInsertion ??= new Dictionary<int, DynamicGroupPlaceholderContextObject>())[index] = groupPlaceholder;
    }

    private void ItemsOnItemsAdded(IObservableList<IContextObject> list, int index, IList<IContextObject> items) {
        AdvancedMenuService.InsertItemNodesWithDynamicSupport(this, index, new List<IContextObject>(items), ref this.dynamicInsertion, ref this.dynamicInserted);
    }

    private void ItemsOnItemsRemoved(IObservableList<IContextObject> list, int index, IList<IContextObject> items) {
        AdvancedMenuService.RemoveItemNodesWithDynamicSupport(this.OwnerMenu!, this, index, new List<IContextObject>(items), ref this.dynamicInsertion, ref this.dynamicInserted);
    }

    private void ItemsOnItemMoved(IObservableList<IContextObject> list, int oldIndex, int newIndex, IContextObject item) {
        AdvancedMenuService.RemoveItemNodesWithDynamicSupport(this.OwnerMenu!, this, oldIndex, [item], ref this.dynamicInsertion, ref this.dynamicInserted);
        AdvancedMenuService.InsertItemNodesWithDynamicSupport(this, newIndex, [item], ref this.dynamicInsertion, ref this.dynamicInserted);
    }

    private void ItemsOnItemReplaced(IObservableList<IContextObject> list, int index, IContextObject oldItem, IContextObject newItem) {
        AdvancedMenuService.RemoveItemNodesWithDynamicSupport(this.OwnerMenu!, this, index, [oldItem], ref this.dynamicInsertion, ref this.dynamicInserted);
        AdvancedMenuService.InsertItemNodesWithDynamicSupport(this, index, [newItem], ref this.dynamicInsertion, ref this.dynamicInserted);
    }

    private void OnEntryIconChanged(BaseContextEntry sender, Icon? oldicon, Icon? newicon) {
        if (newicon != null) {
            if (this.myIconControl == null) {
                this.myIconControl ??= new IconControl {
                    Icon = newicon
                };

                this.Icon = this.myIconControl;
            }
            else {
                this.myIconControl.Icon = newicon;
            }
        }
        else if (this.myIconControl != null) {
            this.myIconControl.Icon = null;
        }
    }

    private void OnEntryDescriptionChanged(BaseContextEntry sender) {
        if (sender.Description != null) {
            ToolTip.SetTip(this, sender.Description ?? "");
        }
        else {
            this.ClearValue(ToolTip.TipProperty);
        }
    }

    private void OnEntryDisplayNameChanged(BaseContextEntry sender) => this.Header = sender.DisplayName;
}