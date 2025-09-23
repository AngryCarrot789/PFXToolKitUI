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
    
    public Dictionary<int, DynamicGroupPlaceholderContextObject>? DynamicInsertion { get; set; }
    
    public Dictionary<int, int>? DynamicInserted { get; set; }
    
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
        AdvancedMenuHelper.GenerateDynamicVisualItems(this);
        ApplicationPFX.Instance.Dispatcher.Post(() => AdvancedMenuHelper.NormaliseSeparators(this), DispatchPriority.Loaded);
    }

    protected virtual void OnUnloadedOverride(RoutedEventArgs e) {
        BaseContextEntry.InternalOnBecomeHidden(this.Entry!);
        AdvancedMenuHelper.ClearDynamicVisualItems(this);
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
            AdvancedMenuHelper.OnLogicalItemsAdded(this, 0, list.Items);
            list.Items.ItemsAdded += this.ItemsOnItemsAdded;
            list.Items.ItemsRemoved += this.ItemsOnItemsRemoved;
            list.Items.ItemMoved += this.ItemsOnItemMoved;
            list.Items.ItemReplaced += this.ItemsOnItemReplaced;
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
        AdvancedMenuHelper.ClearDynamicVisualItems(this);
        AdvancedMenuHelper.ClearVisualNodes(this);
        this.DynamicInsertion = null;
        this.DynamicInserted = null;
        
        DataManager.GetContextData(this).Remove(BaseContextEntry.DataKey);
    }

    public virtual void OnRemoved() {
        this.OwnerMenu = null;
        this.ParentNode = null;
        this.Entry = null;
    }

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
            AdvancedMenuHelper.GenerateDynamicVisualItems(this);
            ApplicationPFX.Instance.Dispatcher.Post(() => AdvancedMenuHelper.NormaliseSeparators(this), DispatchPriority.Loaded);
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change) {
        base.OnPropertyChanged(change);
        if (change.Property == IsSubMenuOpenProperty) {
            if (this.IsSubMenuOpen) {
                AdvancedMenuHelper.GenerateDynamicVisualItems(this);
                Debug.WriteLine($"Generated dynamic items for menu item shown: " + (this.DynamicInserted?.Count ?? 0));
            }
            else {
                Debug.WriteLine($"Cleared '{this.DynamicInserted?.Count ?? 0}' dynamic items for menu item hidden");
                AdvancedMenuHelper.ClearDynamicVisualItems(this);
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
    }

    private void ItemsOnItemsAdded(IObservableList<IContextObject> list, int index, IList<IContextObject> items) {
        this.OnItemAddedOrRemoved();
        AdvancedMenuHelper.OnLogicalItemsAdded(this, index, new List<IContextObject>(items));
    }

    private void ItemsOnItemsRemoved(IObservableList<IContextObject> list, int index, IList<IContextObject> items) {
        this.OnItemAddedOrRemoved();
        AdvancedMenuHelper.OnLogicalItemsRemoved(this, index, new List<IContextObject>(items));
    }

    private void ItemsOnItemMoved(IObservableList<IContextObject> list, int oldIndex, int newIndex, IContextObject item) {
        AdvancedMenuHelper.OnLogicalItemsRemoved(this, oldIndex, [item]);
        AdvancedMenuHelper.OnLogicalItemsAdded(this, newIndex, [item]);
    }

    private void ItemsOnItemReplaced(IObservableList<IContextObject> list, int index, IContextObject oldItem, IContextObject newItem) {
        AdvancedMenuHelper.OnLogicalItemsRemoved(this, index, [oldItem]);
        AdvancedMenuHelper.OnLogicalItemsAdded(this, index, [newItem]);
    }

    private void OnEntryIconChanged(BaseContextEntry sender, Icon? oldIcon, Icon? newIcon) {
        if (newIcon != null) {
            if (this.myIconControl == null) {
                this.myIconControl ??= new IconControl { Icon = newIcon };
                this.Icon = this.myIconControl;
            }
            else {
                this.myIconControl.Icon = newIcon;
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