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

using Avalonia;
using Avalonia.Controls;
using PFXToolKitUI.AdvancedMenuService;
using PFXToolKitUI.Interactivity.Contexts;

namespace PFXToolKitUI.Avalonia.AdvancedMenuService;

/// <summary>
/// The advanced menu service provides dynamic context and top-level menu item generation
/// </summary>
internal static class AdvancedMenuHelper {
    private static bool HasVisibleEntryAfter(ItemCollection items, int index) {
        for (int i = index + 1; i < items.Count; i++) {
            if (!(items[i] is Separator) && ((Visual) items[i]!).IsVisible)
                return true;
        }

        return false;
    }

    private static bool IsSeparatorNotNeeded(ItemCollection items, int index) {
        if (items[index] is Separator) {
            if (index > 0 && items[index - 1] is CaptionSeparator prevCaption && prevCaption.IsVisible)
                return true;
            return (index < items.Count - 1) && items[index + 1] is CaptionSeparator nextCaption && nextCaption.IsVisible;
        }

        return false;
    }

    // I think it needs to be longer
    private static void GoBackAndHideSeparatorsUntilNonSeparatorReached(ItemCollection items, int index) {
        for (int i = index - 1; i >= 0; i--) {
            if (items[i] is Separator separator) {
                separator.IsVisible = false;
            }
            else if (((Visual) items[i]!).IsVisible) {
                break;
            }
        }
    }

    /// <summary>
    /// Calculates which controls are visible. It is assumed the control's items are all
    /// fully loaded and their current visibility reflects their true underlying state 
    /// </summary>
    /// <param name="control">The control whose items should be processed</param>
    public static void NormaliseSeparators(ItemsControl control) {
        // # Caption (general)
        // Rename
        // Change Colour
        // # Separator
        // Group Items
        // # Separator
        // # Caption (Modify Online State)
        // Set Offline
        // # Separator
        // Delete

        ItemCollection items = control.Items;
        bool lastVisibleWasEntry = false;
        for (int i = 0; i < items.Count; i++) {
            object? current = items[i];
            if (current is Separator separator) {
                if (IsSeparatorNotNeeded(items, i)) {
                    separator.IsVisible = false;
                }
                else if (!lastVisibleWasEntry || !HasVisibleEntryAfter(items, i)) {
                    separator.IsVisible = false;
                }
                else {
                    separator.IsVisible = true;
                    lastVisibleWasEntry = false;
                }
            }
            else if (((Visual) current!).IsVisible) {
                lastVisibleWasEntry = true;
                if (current is CaptionSeparator && i > 0) {
                    GoBackAndHideSeparatorsUntilNonSeparatorReached(items, i);
                }
            }
        }
    }

    public static void InsertVisualOrDynamicNodes(IAdvancedMenuOrItem item, int index, IReadOnlyList<IContextObject> entries) {
        int i = index;
        IAdvancedMenu ownerMenu = item.OwnerMenu ?? throw new InvalidOperationException("No owner menu");
        foreach (IContextObject entry in entries) {
            if (entry is DynamicGroupPlaceholderContextObject dynamicItem) {
                Dictionary<int, DynamicGroupPlaceholderContextObject> dynamicMap = item.DynamicInsertion ??= new Dictionary<int, DynamicGroupPlaceholderContextObject>();
                dynamicMap[i] = dynamicItem;
            }
            else {
                Control element = CreateItem(ownerMenu, entry);
                if (element is AdvancedMenuItem menuItem) {
                    menuItem.OnAdding(ownerMenu, item, (BaseContextEntry) entry);
                    item.Items.Insert(i++, menuItem);
                    menuItem.OnAdded();
                }
                else if (element is IAdvancedEntryConnection connection) {
                    connection.OnAdding(ownerMenu, item, entry);
                    item.Items.Insert(i++, element);
                    connection.OnAdded();
                }
                else {
                    item.Items.Insert(i++, element);
                }
            }
        }
    }

    public static void ClearVisualNodes(IAdvancedMenuOrItem control) {
        ItemCollection list = control.Items;
        IAdvancedMenu container = control as IAdvancedMenu ?? control.OwnerMenu ?? throw new InvalidOperationException("No owner menu");
        for (int i = list.Count - 1; i >= 0; i--) {
            RemoveVisualNode(container, control, i);
        }
    }

    public static void RemoveVisualNodes(IAdvancedMenuOrItem control, int index, int count) {
        IAdvancedMenu? container = control as IAdvancedMenu ?? control.OwnerMenu;
        for (int i = index + count - 1; i >= index; i--) {
            RemoveVisualNode(container, control, i);
        }
    }

    public static void RemoveVisualNode(IAdvancedMenu? menu, IAdvancedMenuOrItem target, int index) {
        ItemCollection items = target.Items;
        Control element = (Control) items[index]!;
        if (element is AdvancedMenuItem menuItem) {
            Type type = menuItem.Entry!.GetType();
            menuItem.OnRemoving();
            items.RemoveAt(index);
            menuItem.OnRemoved();
            menu?.PushCachedItem(type, element);
        }
        else if (element is IAdvancedEntryConnection connection) {
            Type type = connection.Entry!.GetType();
            connection.OnRemoving();
            items.RemoveAt(index);
            connection.OnRemoved();
            menu?.PushCachedItem(type, element);
        }
        else {
            items.RemoveAt(index);
            if (menu != null) {
                if (element is Separator)
                    menu.PushCachedItem(typeof(SeparatorEntry), element);
            }
        }
    }

    /// <summary>
    /// Default implementation for <see cref="IAdvancedMenu.PushCachedItem"/>
    /// </summary>
    public static bool PushCachedItem(Dictionary<Type, Stack<Control>> itemCache, Type entryType, Control item, int maxCache = 16) {
        if (item == null)
            throw new ArgumentNullException(nameof(item));
        if (entryType == null)
            throw new ArgumentNullException(nameof(entryType));

        if (!itemCache.TryGetValue(entryType, out Stack<Control>? stack))
            itemCache[entryType] = stack = new Stack<Control>();
        else if (stack.Count == maxCache)
            return false;

        stack.Push(item);
        return true;
    }

    /// <summary>
    /// Default implementation for <see cref="IAdvancedMenu.PopCachedItem"/>
    /// </summary>
    public static Control? PopCachedItem(Dictionary<Type, Stack<Control>> itemCache, Type entryType) {
        if (entryType == null)
            throw new ArgumentNullException(nameof(entryType));

        if (itemCache.TryGetValue(entryType, out Stack<Control>? stack) && stack.Count > 0)
            return stack.Pop();

        return null;
    }

    public static Control CreateItem(IAdvancedMenu menu, IContextObject entry) {
        Control? element = menu.PopCachedItem(entry.GetType());
        if (element == null) {
            switch (entry) {
                case CommandContextEntry _: element = new AdvancedCommandMenuItem(); break;
                case CustomContextEntry _:  element = new AdvancedCustomMenuItem(); break;
                case BaseContextEntry _:    element = new AdvancedMenuItem(); break;
                case SeparatorEntry _:      element = new Separator() { Margin = new Thickness(5, 2) }; break;
                case CaptionEntry _:        element = new CaptionSeparator(); break;
                default:                    throw new Exception("Unknown item type: " + entry?.GetType());
            }
        }

        return element;
    }

    public static void ClearDynamicVisualItems(IAdvancedMenuOrItem element) {
        Dictionary<int, int>? dictionary = element.DynamicInserted;
        if (dictionary == null || dictionary.Count < 1) {
            return;
        }

        List<KeyValuePair<int, int>> items = dictionary.OrderBy(x => x.Key).Reverse().ToList();
        foreach (KeyValuePair<int, int> item in items) {
            RemoveVisualNodes(element, item.Key, item.Value);
        }

        dictionary.Clear();
    }

    public static void GenerateDynamicVisualItems(IAdvancedMenuOrItem element) {
        ClearDynamicVisualItems(element);
        Dictionary<int, DynamicGroupPlaceholderContextObject>? insertion = element.DynamicInsertion;
        if (insertion == null || insertion.Count < 1) {
            return;
        }

        IAdvancedMenu menu = element.OwnerMenu ?? throw new InvalidOperationException("No menu available from menu item");
        IContextData context = menu.CapturedContext ?? EmptyContext.Instance;

        // dynamicInserted maps the logical (non-offsetted) index of the DynamicGroupPlaceholderContextObject to the amount of items.
        Dictionary<int, int> inserted = element.DynamicInserted ??= new Dictionary<int, int>();

        int offset = 0;
        List<KeyValuePair<int, DynamicGroupPlaceholderContextObject>> items = insertion.OrderBy(x => x.Key).ToList();
        foreach (KeyValuePair<int, DynamicGroupPlaceholderContextObject> item in items) {
            // The key is a marker, we still need to post process the true index
            // This is also why we must insert from start to end
            int index = item.Key + offset;
            List<IContextObject> generated = item.Value.DynamicGroup.GenerateItems(context);
            if (generated.Any(x => x is DynamicGroupPlaceholderContextObject)) {
                throw new InvalidOperationException("Dynamic context entries cannot be provided by the generator");
            }
            
            InsertVisualOrDynamicNodes(element, index, generated);
            inserted[index] = generated.Count;
            offset += generated.Count;
        }
    }

    public static void OnLogicalItemsAdded(IAdvancedMenuOrItem target, int logicalIndex, IReadOnlyList<IContextObject> entries) {
        ClearDynamicVisualItems(target);
        InsertVisualOrDynamicNodes(target, logicalIndex, entries);
        if (target.IsOpen) {
            GenerateDynamicVisualItems(target);
        }
    }

    public static void OnLogicalItemsRemoved(IAdvancedMenuOrItem target, int logicalIndex, IReadOnlyList<IContextObject> entries) {
        IAdvancedMenu? container = target as IAdvancedMenu ?? target.OwnerMenu;
        ClearDynamicVisualItems(target);
        for (int i = entries.Count - 1; i >= 0; i--) {
            if (!(entries[i] is DynamicGroupPlaceholderContextObject))
                RemoveVisualNode(container, target, logicalIndex);
        }

        if (target.IsOpen) {
            GenerateDynamicVisualItems(target);
        }
    }
}