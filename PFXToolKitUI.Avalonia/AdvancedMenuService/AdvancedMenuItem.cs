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

using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using PFXToolKitUI.Avalonia.AvControls;
using PFXToolKitUI.AdvancedMenuService;
using PFXToolKitUI.Avalonia.Interactivity;
using PFXToolKitUI.Avalonia.ToolTips;
using PFXToolKitUI.Icons;
using PFXToolKitUI.Interactivity.Contexts;
using PFXToolKitUI.Utils.Collections.Observable;

namespace PFXToolKitUI.Avalonia.AdvancedMenuService;

/// <summary>
/// A menu item that participates in the advanced menu service
/// </summary>
public class AdvancedMenuItem : MenuItem, IAdvancedMenuOrItem {
    public static readonly StyledProperty<bool> IsVisibilityChangingProperty = AvaloniaProperty.Register<AdvancedMenuItem, bool>(nameof(IsVisibilityChanging), inherits: true);

    public bool IsVisibilityChanging {
        get => this.GetValue(IsVisibilityChangingProperty);
        set => this.SetValue(IsVisibilityChangingProperty, value);
    }

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

    public AdvancedMenuItem() {
    }

    protected sealed override void OnLoaded(RoutedEventArgs e) {
        this.IsVisibilityChanging = true;
        base.OnLoaded(e);
        this.OnLoadedOverride(e);
        this.IsVisibilityChanging = false;

        this.UpdateIsCheckedProperties(this.Entry!);
        this.UpdateIsEnabled();
    }

    protected sealed override void OnUnloaded(RoutedEventArgs e) {
        this.IsVisibilityChanging = true;
        base.OnUnloaded(e);
        this.OnUnloadedOverride(e);
        this.IsVisibilityChanging = false;
    }

    protected virtual void OnLoadedOverride(RoutedEventArgs e) {
        BaseContextEntry.InternalOnBecomeVisible(this.Entry!, this.OwnerMenu?.CapturedContext ?? EmptyContext.Instance);
        AdvancedMenuService.GenerateDynamicItems(this, ref this.dynamicInsertion, ref this.dynamicInserted);
        Dispatcher.UIThread.InvokeAsync(() => {
            AdvancedMenuService.NormaliseSeparators(this);
            // if (this.Entry is ContextEntryGroup list && list.ShowDummyItemWhenEmpty) {
            //     this.ShowDummyItem();
            // }
        }, DispatcherPriority.Loaded);
    }

    protected virtual void OnUnloadedOverride(RoutedEventArgs e) {
        BaseContextEntry.InternalOnBecomeHidden(this.Entry!);
        AdvancedMenuService.ClearDynamicItems(this, ref this.dynamicInserted);
        // if (this.Entry is ContextEntryGroup list && list.ShowDummyItemWhenEmpty) {
        //     this.HideDummyItem();
        // }
    }

    public virtual void OnAdding(IAdvancedMenu menu, IAdvancedMenuOrItem parent, BaseContextEntry entry) {
        this.OwnerMenu = menu;
        this.ParentNode = parent;
        this.Entry = entry;
    }

    public virtual void OnAdded() {
        BaseContextEntry entry = this.Entry!;
        DataManager.GetContextData(this).Set(BaseContextEntry.DataKey, entry);

        this.UpdateHeader(entry);
        if (entry.Description != null)
            ToolTipEx.SetTip(this, entry.Description ?? "");

        if (entry.Icon != null) {
            this.myIconControl = new IconControl() {
                Icon = entry.Icon
            };

            this.Icon = this.myIconControl;
        }

        entry.DisplayNameChanged += this.OnEntryDisplayNameChanged;
        entry.DescriptionChanged += this.OnEntryDescriptionChanged;
        entry.IconChanged += this.OnEntryIconChanged;

        if (entry is ContextEntryGroup list) {
            AdvancedMenuService.InsertItemNodes(this, list.Items);
            list.Items.ItemsAdded += this.ItemsOnItemsAdded;
            list.Items.ItemsRemoved += this.ItemsOnItemsRemoved;
            list.Items.ItemMoved += this.ItemsOnItemMoved;
            list.Items.ItemReplaced += this.ItemsOnItemReplaced;
            // list.ShowDummyItemWhenEmptyChanged += this.OnShowDummyItemWhenEmptyChanged;
            // if (list.ShowDummyItemWhenEmpty) {
            //     this.ShowDummyItem();
            // }
        }

        entry.CanExecuteChanged += this.OnCanExecuteChanged;
        entry.IsCheckedFunctionChanged += this.UpdateIsCheckedProperties;
        entry.IsCheckedChanged += this.UpdateIsChecked;
        this.UpdateIsEnabled();
    }

    public virtual void OnRemoving() {
        BaseContextEntry entry = this.Entry!;
        
        if (entry is ContextEntryGroup list) {
            list.Items.ItemsAdded -= this.ItemsOnItemsAdded;
            list.Items.ItemsRemoved -= this.ItemsOnItemsRemoved;
            list.Items.ItemMoved -= this.ItemsOnItemMoved;
            list.Items.ItemReplaced -= this.ItemsOnItemReplaced;
            // list.ShowDummyItemWhenEmptyChanged -= this.OnShowDummyItemWhenEmptyChanged;
            // if (list.ShowDummyItemWhenEmpty) {
            //     this.HideDummyItem();
            // }
        }

        entry.DisplayNameChanged -= this.OnEntryDisplayNameChanged;
        entry.DescriptionChanged -= this.OnEntryDescriptionChanged;
        entry.IconChanged -= this.OnEntryIconChanged;
        entry.CanExecuteChanged -= this.OnCanExecuteChanged;
        entry.IsCheckedFunctionChanged -= this.UpdateIsCheckedProperties;
        entry.IsCheckedChanged -= this.UpdateIsChecked;

        if (this.myIconControl != null) {
            this.myIconControl.Icon = null;
            this.myIconControl = null;
            this.Icon = null;
        }

        this.ClearValue(ToolTipEx.TipProperty);
        AdvancedMenuService.ClearDynamicItems(this, ref this.dynamicInserted);
        AdvancedMenuService.ClearItemNodes(this);
        
        DataManager.GetContextData(this).Set(BaseContextEntry.DataKey, null);
    }

    public virtual void OnRemoved() {
        this.OwnerMenu = null;
        this.ParentNode = null;
        this.Entry = null;
    }

    // private void OnShowDummyItemWhenEmptyChanged(BaseContextEntry sender) {
    //     if (((ContextEntryGroup) sender).ShowDummyItemWhenEmpty) {
    //         this.ShowDummyItem();
    //     }
    //     else {
    //         this.HideDummyItem();
    //     }
    // }
    // private bool isDummyItemShown;

    // private void ShowDummyItem() {
    //     if (!(this.Entry is ContextEntryGroup list) || this.isDummyItemShown) {
    //         return;
    //     }
    //
    //     if (this.Items.Count < 1 && list.ShowDummyItemWhenEmpty) {
    //         this.isDummyItemShown = true;
    //         // this.Items.Add(new CaptionSeparator() {
    //         //     Text = "No options available", Padding = new Thickness(24,3,4,3)
    //         // });
    //     }
    // }

    // private void HideDummyItem() {
    //     if (!(this.Entry is ContextEntryGroup list) || !this.isDummyItemShown) {
    //         return;
    //     }
    //
    //     if (this.Items.Count > 0 && list.ShowDummyItemWhenEmpty) {
    //         this.isDummyItemShown = false;
    //         // Debug.Assert(this.Items[0] is CaptionSeparator);
    //         // this.Items.RemoveAt(0);
    //     }
    // }

    private void UpdateIsCheckedProperties(BaseContextEntry sender) {
        this.ToggleType = sender.IsCheckedFunction != null ? MenuItemToggleType.CheckBox : MenuItemToggleType.None;
        // IsCheckedChanged is fired after IsCheckedFunctionChanged so no need to double call UpdateIsChecked
        // this.UpdateIsChecked(sender);
    }

    private void UpdateIsChecked(BaseContextEntry sender) {
        this.IsChecked = sender.IsCheckedFunction != null && sender.IsCheckedFunction(sender);
    }

    private void OnCanExecuteChanged(BaseContextEntry baseContextEntry) {
        this.UpdateCanExecute();

        if (!this.IsVisibilityChanging) {
            AdvancedMenuService.GenerateDynamicItems(this, ref this.dynamicInsertion, ref this.dynamicInserted);
            Dispatcher.UIThread.InvokeAsync(() => AdvancedMenuService.NormaliseSeparators(this), DispatcherPriority.Loaded);
        }
    }

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
        if (this.IsVisibilityChanging) {
            // True when processing OnLoaded/OnUnloaded, in which case, stuff is going crazy at the moment,
            // so don't do potentially useless calculations if an items will be cleared very soon, especially
            // since this method is recursive
            return;
        }

        if (this.Entry != null)
            this.UpdateIsChecked(this.Entry);

        this.UpdateIsEnabled();

        foreach (object? item in this.Items) {
            if (item is AdvancedMenuItem menuItem && menuItem.IsLoaded) {
                menuItem.UpdateCanExecute();
            }
        }
    }

    private void UpdateIsEnabled() {
        if (this.Entry is ContextEntryGroup g) {
            this.IsEnabled = g.Items.Count > 0;
        }
        else {
            this.IsEnabled = true;
        }
    }

    private void OnItemAddedOrRemoved() {
        this.UpdateHeader(this.Entry!);
        this.UpdateIsEnabled();
    }

    protected virtual void UpdateHeader(BaseContextEntry entry) {
        this.Header = entry.DisplayName;
        // if (entry is ContextEntryGroup g && g.Items.Count < 1) {
        //     this.Header = entry.DisplayName + " (empty)";
        // }
        // else {
        //     this.Header = entry.DisplayName;
        // }
    }

    public void StoreDynamicGroup(DynamicGroupPlaceholderContextObject groupPlaceholder, int index) {
        (this.dynamicInsertion ??= new Dictionary<int, DynamicGroupPlaceholderContextObject>())[index] = groupPlaceholder;
    }

    private void ItemsOnItemsAdded(IObservableList<IContextObject> list, int index, IList<IContextObject> items) {
        // this.HideDummyItem();
        this.OnItemAddedOrRemoved();
        AdvancedMenuService.InsertItemNodesWithDynamicSupport(this, index, new List<IContextObject>(items), ref this.dynamicInsertion, ref this.dynamicInserted);
    }

    private void ItemsOnItemsRemoved(IObservableList<IContextObject> list, int index, IList<IContextObject> items) {
        // this.ShowDummyItem();
        this.OnItemAddedOrRemoved();
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
            ToolTipEx.SetTip(this, sender.Description ?? "");
        }
        else {
            this.ClearValue(ToolTipEx.TipProperty);
        }
    }

    private void OnEntryDisplayNameChanged(BaseContextEntry sender) {
        this.UpdateHeader(sender);
    }

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey) {
        if (item is MenuItem || item is Separator || item is CaptionSeparator) {
            recycleKey = null;
            return false;
        }

        return base.NeedsContainerOverride(item, index, out recycleKey);
    }
}