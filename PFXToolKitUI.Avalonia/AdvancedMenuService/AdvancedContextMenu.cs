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

using System.ComponentModel;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using PFXToolKitUI.Avalonia.Bindings;
using PFXToolKitUI.Avalonia.Interactivity;
using PFXToolKitUI.AdvancedMenuService;
using PFXToolKitUI.Interactivity.Contexts;

namespace PFXToolKitUI.Avalonia.AdvancedMenuService;

public sealed class AdvancedContextMenu : ContextMenu, IAdvancedMenu {
    // We maintain a map of the registries to the context menu. This is to
    // save memory, since we don't have to create a context menu for each handler
    private static readonly Dictionary<ContextRegistry, AdvancedContextMenu> contextMenus;

    public static readonly AttachedProperty<ContextRegistry?> ContextRegistryProperty = AvaloniaProperty.RegisterAttached<AdvancedContextMenu, Control, ContextRegistry?>("ContextRegistry");
    public static readonly StyledProperty<string?> ContextCaptionProperty = AvaloniaProperty.Register<AdvancedContextMenu, string?>(nameof(ContextCaption));

    private readonly EventPropertyBinder<ContextRegistry> captionBinder;
    private readonly Dictionary<Type, Stack<Control>> itemCache;
    private readonly List<Control> owners;
    private InputElement? currentTarget;
    private Dictionary<int, DynamicGroupPlaceholderContextObject>? dynamicInsertion;
    private Dictionary<int, int>? dynamicInserted;

    IAdvancedMenu IAdvancedMenuOrItem.OwnerMenu => this;

    public string? ContextCaption {
        get => this.GetValue(ContextCaptionProperty);
        set => this.SetValue(ContextCaptionProperty, value);
    }

    public IContextData? CapturedContext { get; private set; }

    public ContextRegistry ContextRegistry { get; }

    public AdvancedContextMenu(ContextRegistry registry) {
        this.ContextRegistry = registry ?? throw new ArgumentNullException(nameof(registry));
        this.captionBinder = new EventPropertyBinder<ContextRegistry>(nameof(this.ContextRegistry.CaptionChanged), (b) => b.Control.SetValue(ContextCaptionProperty, b.Model.Caption));
        this.itemCache = new Dictionary<Type, Stack<Control>>();
        this.owners = new List<Control>();
        this.Opening += this.OnMenuOpening;
        this.Closed += this.OnMenuClosed;
    }

    static AdvancedContextMenu() {
        contextMenus = new Dictionary<ContextRegistry, AdvancedContextMenu>();
        ContextRegistryProperty.Changed.AddClassHandler<Control, ContextRegistry?>((d, e) => OnContextRegistryChanged(d, e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
    }

    #region Opening and Closing

    // These methods are defined in the order they're called

    private void OnRequestOpenContextMenu(object? sender, ContextRequestedEventArgs e) {
        this.currentTarget = sender as InputElement;
    }

    private void OnMenuOpening(object? sender, CancelEventArgs e) {
        if (this.currentTarget == null) {
            e.Cancel = true;
            return;
        }

        this.CaptureContextFromObject(this.currentTarget);
        AdvancedMenuService.GenerateDynamicItems(this, ref this.dynamicInsertion, ref this.dynamicInserted);
        AdvancedMenuService.NormaliseSeparators(this);
    }

    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
        if (this.ContextRegistry.IsOpened)
            this.ContextRegistry.OnClosed();

        this.ContextRegistry.OnOpened(this.CapturedContext ?? EmptyContext.Instance);
        this.captionBinder.Attach(this, this.ContextRegistry);
    }

    protected override void OnUnloaded(RoutedEventArgs e) {
        base.OnUnloaded(e);
        this.captionBinder.Detach();
        if (this.ContextRegistry.IsOpened) {
            this.ContextRegistry.OnClosed();
        }
    }

    private void OnMenuClosed(object? sender, RoutedEventArgs e) {
        AdvancedMenuService.ClearDynamicItems(this, ref this.dynamicInserted);
        this.ClearContext();
    }

    #endregion

    private void ClearContext() {
        DataManager.ClearDelegateContextData(this);
        this.CapturedContext = null;
        this.currentTarget = null;
    }

    private void CaptureContextFromObject(InputElement inputElement) {
        DataManager.SetDelegateContextData(this, this.CapturedContext = DataManager.GetFullContextData(inputElement));
    }

    private void AddOwner(Control target) {
        this.owners.Add(target);

        // ContextRequested is fired when the user wants to open the context menu.
        // Usually originates from handling a right click mouse event
        target.ContextRequested += this.OnRequestOpenContextMenu;
    }

    private bool RemoveOwnerAndShouldDestroy(Control target) {
        if (this.owners.Remove(target))
            target.ContextRequested -= this.OnRequestOpenContextMenu;
        return this.owners.Count == 0;
    }

    public bool PushCachedItem(Type entryType, Control item) => AdvancedMenuService.PushCachedItem(this.itemCache, entryType, item);

    public Control? PopCachedItem(Type entryType) => AdvancedMenuService.PopCachedItem(this.itemCache, entryType);

    public Control CreateItem(IContextObject entry) => AdvancedMenuService.CreateChildItem(this, entry);

    public void StoreDynamicGroup(DynamicGroupPlaceholderContextObject groupPlaceholder, int index) {
        (this.dynamicInsertion ??= new Dictionary<int, DynamicGroupPlaceholderContextObject>())[index] = groupPlaceholder;
    }

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey) {
        return new AdvancedMenuItem();
    }

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey) {
        if (item is MenuItem || item is Separator || item is CaptionSeparator) {
            recycleKey = null;
            return false;
        }

        recycleKey = DefaultRecycleKey;
        return true;
    }

    public static void SetContextRegistry(Control obj, ContextRegistry? value) => obj.SetValue(ContextRegistryProperty, value);

    private static void OnContextRegistryChanged(Control target, ContextRegistry? oldValue, ContextRegistry? newValue) {
        if (ReferenceEquals(oldValue, newValue)) {
            return; // should be impossible... but just in case let's check
        }

        if (oldValue != null && contextMenus.TryGetValue(oldValue, out AdvancedContextMenu? oldMenu)) {
            if (oldMenu.RemoveOwnerAndShouldDestroy(target)) {
                contextMenus.Remove(oldValue); // remove the menu to prevent a memory leak I guess?
            }
        }

        if (newValue != null) {
            // Generate context menu, if required
            if (!contextMenus.TryGetValue(newValue, out AdvancedContextMenu? menu)) {
                contextMenus[newValue] = menu = new AdvancedContextMenu(newValue);
                List<IContextObject> contextObjects = new List<IContextObject>();

                int i = 0;
                foreach (KeyValuePair<string, IContextGroup> entry in newValue.Groups) {
                    if (i++ != 0)
                        contextObjects.Add(new SeparatorEntry());

                    switch (entry.Value) {
                        case FixedContextGroup fixedGroup:     contextObjects.AddRange(fixedGroup.Items); break;
                        case DynamicContextGroup dynamicGroup: contextObjects.Add(new DynamicGroupPlaceholderContextObject(dynamicGroup)); break;
                    }
                }

                AdvancedMenuService.InsertItemNodes(menu, contextObjects);
            }

            // Slide in and add ContextRequested handler before the base ContextMenu
            // class does, so that we can update the target trying to open the menu
            menu.AddOwner(target);
            target.ContextMenu = menu;
        }
        else {
            target.ContextMenu = null;
        }
    }
}