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

using System.Collections.Specialized;
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

    public BaseMenuEntry? Entry { get; private set; }

    bool IAdvancedMenuOrItem.IsOpen => this.IsSubMenuOpen;

    public Dictionary<int, DynamicGroupPlaceholderMenuEntry>? DynamicInsertion { get; set; }

    public Dictionary<int, int>? DynamicInserted { get; set; }

    private readonly IconControl myIconControl;
    private bool previousEffectivelyEnabled;

    public AdvancedMenuItem() {
        this.Icon = this.myIconControl = new IconControl();
        this.Classes.CollectionChanged += this.ClassesOnCollectionChanged;
        this.previousEffectivelyEnabled = this.IsEffectivelyEnabled;
        
        ToolTipEx.SetTipType(this, typeof(ToolTips.ContextMenuToolTip));
        ToolTip.SetShowOnDisabled(this, true);
    }

    private void ClassesOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
        bool newIsEffectivelyEnabled = this.IsEffectivelyEnabled;
        if (newIsEffectivelyEnabled != this.previousEffectivelyEnabled) {
            this.previousEffectivelyEnabled = newIsEffectivelyEnabled;
            this.OnIsEffectivelyEnabledChanged();
        }
    }

    protected virtual void OnIsEffectivelyEnabledChanged() {
        this.UpdateIcon();
    }

    private void UpdateIcon() {
        BaseMenuEntry? entry = this.Entry;
        this.myIconControl.Icon = entry == null
            ? null
            : this.IsEffectivelyEnabled
                ? entry.Icon
                : entry.DisabledIcon ?? entry.Icon;
    }

    protected sealed override void OnLoaded(RoutedEventArgs e) {
        this.IsVisibilityChanging = true;
        base.OnLoaded(e);
        this.OnLoadedOverride(e);
        this.IsVisibilityChanging = false;

        this.OnIsCheckedFunctionChanged(this.Entry!);
        this.UpdateIsEnabled();
    }

    protected sealed override void OnUnloaded(RoutedEventArgs e) {
        this.IsVisibilityChanging = true;
        base.OnUnloaded(e);
        this.OnUnloadedOverride(e);
        this.IsVisibilityChanging = false;
    }

    protected virtual void OnLoadedOverride(RoutedEventArgs e) {
        BaseMenuEntry.InternalOnBecomeVisible(this.Entry!, this.OwnerMenu?.CapturedContext ?? EmptyContext.Instance);
        AdvancedMenuHelper.GenerateDynamicVisualItems(this);
        ApplicationPFX.Instance.Dispatcher.Post(() => AdvancedMenuHelper.NormaliseSeparators(this), DispatchPriority.Loaded);
    }

    protected virtual void OnUnloadedOverride(RoutedEventArgs e) {
        BaseMenuEntry.InternalOnBecomeHidden(this.Entry!);
        AdvancedMenuHelper.ClearDynamicVisualItems(this);
    }

    public virtual void OnAdding(IAdvancedMenu menu, IAdvancedMenuOrItem parent, BaseMenuEntry entry) {
        this.OwnerMenu = menu;
        this.ParentNode = parent;
        this.Entry = entry;
    }

    public virtual void OnAdded() {
        BaseMenuEntry entry = this.Entry!;
        DataManager.GetContextData(this).Set(BaseMenuEntry.DataKey, entry);

        this.UpdateHeader(entry);
        entry.DisplayNameChanged += this.OnEntryDisplayNameChanged;
        entry.IconChanged += this.OnAnyEntryIconChanged;
        entry.DisabledIconChanged += this.OnAnyEntryIconChanged;

        if (entry is MenuEntryGroup list) {
            AdvancedMenuHelper.OnLogicalItemsAdded(this, 0, list.Items);
            list.Items.ItemsAdded += this.ItemsOnItemsAdded;
            list.Items.ItemsRemoved += this.ItemsOnItemsRemoved;
            list.Items.ItemMoved += this.ItemsOnItemMoved;
            list.Items.ItemReplaced += this.ItemsOnItemReplaced;
        }

        entry.CanExecuteChanged += this.OnCanExecuteChanged;
        entry.IsCheckedFunctionChanged += this.OnIsCheckedFunctionChanged;
        entry.IsCheckedChanged += this.OnIsCheckedChanged;
        this.UpdateIsEnabled();
        this.UpdateIcon(); // force update icon, since entry changed
    }

    public virtual void OnRemoving() {
        BaseMenuEntry entry = this.Entry!;
        if (entry is MenuEntryGroup list) {
            list.Items.ItemsAdded -= this.ItemsOnItemsAdded;
            list.Items.ItemsRemoved -= this.ItemsOnItemsRemoved;
            list.Items.ItemMoved -= this.ItemsOnItemMoved;
            list.Items.ItemReplaced -= this.ItemsOnItemReplaced;
        }

        entry.DisplayNameChanged -= this.OnEntryDisplayNameChanged;
        entry.IconChanged -= this.OnAnyEntryIconChanged;
        entry.DisabledIconChanged -= this.OnAnyEntryIconChanged;
        entry.CanExecuteChanged -= this.OnCanExecuteChanged;
        entry.IsCheckedFunctionChanged -= this.OnIsCheckedFunctionChanged;
        entry.IsCheckedChanged -= this.OnIsCheckedChanged;

        this.myIconControl.Icon = null;
        
        AdvancedMenuHelper.ClearDynamicVisualItems(this);
        AdvancedMenuHelper.ClearVisualNodes(this);
        this.DynamicInsertion = null;
        this.DynamicInserted = null;

        DataManager.GetContextData(this).Remove(BaseMenuEntry.DataKey);
    }

    public virtual void OnRemoved() {
        this.OwnerMenu = null;
        this.ParentNode = null;
        this.Entry = null;
    }

    private void OnIsCheckedFunctionChanged(BaseMenuEntry sender) {
        this.ToggleType = sender.IsCheckedFunction != null ? MenuItemToggleType.CheckBox : MenuItemToggleType.None;
        // IsCheckedChanged is fired after IsCheckedFunctionChanged so no need to double call UpdateIsChecked
        // this.UpdateIsChecked(sender);
    }

    private void OnIsCheckedChanged(BaseMenuEntry sender) {
        this.IsChecked = sender.IsCheckedFunction != null && sender.IsCheckedFunction(sender);
    }

    private void OnCanExecuteChanged(BaseMenuEntry baseMenuEntry) {
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
            this.OnIsCheckedChanged(this.Entry);

        this.UpdateIsEnabled();
        (ToolTipEx.GetTip(this) as ToolTips.ContextMenuToolTip)?.OnCanExecuteChanged();

        foreach (object? item in this.Items) {
            if (item is AdvancedMenuItem menuItem && menuItem.IsLoaded) {
                menuItem.UpdateCanExecute();
            }
        }
    }

    private void UpdateIsEnabled() {
        if (this.Entry is MenuEntryGroup g) {
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

    protected virtual void UpdateHeader(BaseMenuEntry entry) {
        this.Header = entry.DisplayName;
    }

    private void ItemsOnItemsAdded(IObservableList<IMenuEntry> list, int index, IList<IMenuEntry> items) {
        this.OnItemAddedOrRemoved();
        AdvancedMenuHelper.OnLogicalItemsAdded(this, index, items);
    }

    private void ItemsOnItemsRemoved(IObservableList<IMenuEntry> list, int index, IList<IMenuEntry> items) {
        this.OnItemAddedOrRemoved();
        AdvancedMenuHelper.OnLogicalItemsRemoved(this, index, items);
    }

    private void ItemsOnItemMoved(IObservableList<IMenuEntry> list, int oldIndex, int newIndex, IMenuEntry item) {
        AdvancedMenuHelper.OnLogicalItemMoved(this, oldIndex, newIndex, item);
    }

    private void ItemsOnItemReplaced(IObservableList<IMenuEntry> list, int index, IMenuEntry oldItem, IMenuEntry newItem) {
        AdvancedMenuHelper.OnLogicalItemReplaced(this, index, oldItem, newItem);
    }

    private void OnAnyEntryIconChanged(BaseMenuEntry sender, Icon? oldIcon, Icon? newIcon) {
        this.UpdateIcon();
    }

    private void OnEntryDisplayNameChanged(BaseMenuEntry sender) {
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