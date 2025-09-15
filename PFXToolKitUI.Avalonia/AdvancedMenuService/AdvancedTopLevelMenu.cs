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
using Avalonia.Input;
using Avalonia.Interactivity;
using PFXToolKitUI.Avalonia.Interactivity;
using PFXToolKitUI.Avalonia.Shortcuts.Avalonia;
using PFXToolKitUI.AdvancedMenuService;
using PFXToolKitUI.Interactivity.Contexts;
using PFXToolKitUI.Utils.Collections.Observable;

namespace PFXToolKitUI.Avalonia.AdvancedMenuService;

public sealed class AdvancedTopLevelMenu : Menu, IAdvancedMenu {
    // We maintain a map of the registries to the context menu. This is to
    // save memory, since we don't have to create a context menu for each handler
    public static readonly StyledProperty<TopLevelMenuRegistry?> TopLevelMenuRegistryProperty = AvaloniaProperty.Register<AdvancedTopLevelMenu, TopLevelMenuRegistry?>(nameof(TopLevelMenuRegistry));

    public TopLevelMenuRegistry? TopLevelMenuRegistry {
        get => this.GetValue(TopLevelMenuRegistryProperty);
        set => this.SetValue(TopLevelMenuRegistryProperty, value);
    }

    public IContextData CapturedContext => DataManager.GetFullContextData(this);

    IAdvancedMenu IAdvancedMenuOrItem.OwnerMenu => this;

    public Dictionary<int, DynamicGroupPlaceholderContextObject>? DynamicInsertion {
        get => throw new InvalidOperationException();
        set => throw new InvalidOperationException();
    }

    public Dictionary<int, int>? DynamicInserted {
        get => throw new InvalidOperationException();
        set => throw new InvalidOperationException();
    }

    protected override Type StyleKeyOverride => typeof(Menu);

    private readonly Dictionary<Type, Stack<Control>> itemCache;
    private InputElement? lastFocus;
    private ObservableItemProcessorIndexing<ContextEntryGroup>? processor;

    public AdvancedTopLevelMenu() {
        this.itemCache = new Dictionary<Type, Stack<Control>>();
    }

    static AdvancedTopLevelMenu() {
        TopLevelMenuRegistryProperty.Changed.AddClassHandler<AdvancedTopLevelMenu, TopLevelMenuRegistry?>((d, e) => d.OnTopLevelMenuRegistryChanged(e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
    }

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey) => new AdvancedMenuItem();

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey) {
        if (item is MenuItem || item is Separator || item is CaptionSeparator) {
            recycleKey = null;
            return false;
        }

        recycleKey = DefaultRecycleKey;
        return true;
    }

    private void OnTopLevelMenuRegistryChanged(TopLevelMenuRegistry? oldValue, TopLevelMenuRegistry? newValue) {
        if (ReferenceEquals(oldValue, newValue)) {
            return; // should be impossible... but just in case let's check
        }

        this.processor?.RemoveExistingItems();
        this.processor?.Dispose();
        this.processor = null;

        if (newValue != null) {
            this.processor = ObservableItemProcessor.MakeIndexable(newValue.Items, this.OnItemAdded, this.OnItemRemoved, this.OnItemMoved).AddExistingItems();
        }
    }

    private void OnItemAdded(object sender, int index, ContextEntryGroup item) {
        AdvancedMenuItem menuItem = (AdvancedMenuItem) AdvancedMenuHelper.CreateItem(this, item);
        menuItem.OnAdding(this, this, item);
        this.Items.Insert(index, menuItem);
        menuItem.OnAdded();
    }

    private void OnItemRemoved(object sender, int index, ContextEntryGroup item) {
        ItemCollection list = this.Items;
        this.OnItemRemoved(list, index, (AdvancedMenuItem) list[index]!);
    }

    private void OnItemRemoved(ItemCollection items, int index, AdvancedMenuItem item) {
        Type type = item.Entry!.GetType();
        item.OnRemoving();
        items.RemoveAt(index);
        item.OnRemoved();
        this.PushCachedItem(type, item);
    }

    private void OnItemMoved(object sender, int oldindex, int newindex, ContextEntryGroup item) {
        ItemCollection list = this.Items;
        if (newindex < 0 || newindex >= list.Count)
            throw new IndexOutOfRangeException($"{nameof(newindex)} is not within range: {(newindex < 0 ? "less than zero" : "greater than list length")} ({newindex})");

        object? removedItem = list[oldindex];
        list.RemoveAt(oldindex);
        list.Insert(newindex, removedItem);
    }

    protected override void OnSubmenuOpened(RoutedEventArgs e) {
        if (e.Source is AdvancedMenuItem menuItem) {
            AdvancedMenuHelper.NormaliseSeparators(menuItem);
        }

        base.OnSubmenuOpened(e);
    }

    public override void Close() {
        bool wasOpen = this.IsOpen;
        base.Close();
        if (wasOpen && this.lastFocus != null) {
            DataManager.ClearDelegateContextData(this);
            Debug.WriteLine("[TopLevelMenu] Cleared captured context from: " + this.lastFocus);
            if (this.lastFocus != null) {
                this.lastFocus.Focus();
                this.lastFocus = null;
            }
        }
    }

    protected override void OnGotFocus(GotFocusEventArgs e) {
        if (e.Handled || this.lastFocus != null) {
            return;
        }

        if (TopLevel.GetTopLevel(this) is TopLevel topLevel) {
            this.lastFocus = UIInputManager.GetLastFocusedElement(topLevel);
        }

        base.OnGotFocus(e);
        if (this.lastFocus != null) {
            this.CaptureContextFromObject(this.lastFocus);
        }
    }

    private void CaptureContextFromObject(InputElement inputElement) {
        IContextData capturedContext = DataManager.GetFullContextData(inputElement);

        Debug.WriteLine($"[TopLevelMenu] Captured context from: {inputElement} ({string.Join(", ", capturedContext.Entries.Select(x => x.Key + "=" + x.Value))})");
        DataManager.SetDelegateContextData(this, capturedContext);
    }

    public bool PushCachedItem(Type entryType, Control item) => AdvancedMenuHelper.PushCachedItem(this.itemCache, entryType, item);

    public Control? PopCachedItem(Type entryType) => AdvancedMenuHelper.PopCachedItem(this.itemCache, entryType);
}