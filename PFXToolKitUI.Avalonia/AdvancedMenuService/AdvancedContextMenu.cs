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

using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
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
    public static readonly StyledProperty<string?> ObjectNameProperty = AvaloniaProperty.Register<AdvancedContextMenu, string?>(nameof(ObjectName));

    private readonly IBinder<ContextRegistry> captionBinder, objectNameBinder;
    private readonly Dictionary<Type, Stack<Control>> itemCache;
    private InputElement? currentTarget;
    private readonly EventHandler<ContextRequestedEventArgs> requestContextHandler;

    public Dictionary<int, DynamicGroupPlaceholderMenuEntry>? DynamicInsertion { get; set; }

    public Dictionary<int, int>? DynamicInserted { get; set; }

    public string? ContextCaption {
        get => this.GetValue(ContextCaptionProperty);
        set => this.SetValue(ContextCaptionProperty, value);
    }

    public string? ObjectName {
        get => this.GetValue(ObjectNameProperty);
        set => this.SetValue(ObjectNameProperty, value);
    }

    public IContextData? CapturedContext { get; private set; }

    public ContextRegistry MyContextRegistry { get; }

    public AdvancedContextMenu(ContextRegistry registry) {
        this.MyContextRegistry = registry ?? throw new ArgumentNullException(nameof(registry));
        this.MyContextRegistry.RequestClose += this.ContextRegistryOnRequestClose;
        this.captionBinder = new EventUpdateBinder<ContextRegistry>(nameof(ContextRegistry.CaptionChanged), (b) => b.Control.SetValue(ContextCaptionProperty, b.Model.Caption));
        this.objectNameBinder = new EventUpdateBinder<ContextRegistry>(nameof(ContextRegistry.ObjectNameChanged), (b) => b.Control.SetValue(ObjectNameProperty, b.Model.ObjectName));
        this.itemCache = new Dictionary<Type, Stack<Control>>();
        this.Opening += this.OnMenuOpening;
        this.Closed += this.OnMenuClosed;
        this.requestContextHandler = (sender, e) => {
            this.currentTarget = sender as InputElement;
        };
    }

    private void ContextRegistryOnRequestClose(ContextRegistry registry) {
        this.Close();
    }

    static AdvancedContextMenu() {
        contextMenus = new Dictionary<ContextRegistry, AdvancedContextMenu>();
        ContextRegistryProperty.Changed.AddClassHandler<Control, ContextRegistry?>((d, e) => OnContextRegistryChanged(d, e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
    }

    #region Opening and Closing

    // These methods are defined in the order they're called

    private void OnMenuOpening(object? sender, CancelEventArgs e) {
        if (this.currentTarget == null) {
            e.Cancel = true;
            return;
        }

        this.CaptureContextFromObject(this.currentTarget);
        AdvancedMenuHelper.GenerateDynamicVisualItems(this);
        AdvancedMenuHelper.NormaliseSeparators(this);
    }

    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
        this.MyContextRegistry.OnOpened(this.CapturedContext ?? EmptyContext.Instance);
        this.captionBinder.Attach(this, this.MyContextRegistry);
        this.objectNameBinder.Attach(this, this.MyContextRegistry);
    }

    protected override void OnUnloaded(RoutedEventArgs e) {
        base.OnUnloaded(e);
        this.captionBinder.Detach();
        this.objectNameBinder.Detach();
        this.MyContextRegistry.OnClosed();
    }

    private void OnMenuClosed(object? sender, RoutedEventArgs e) {
        AdvancedMenuHelper.ClearDynamicVisualItems(this);
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

    private void OnOwnerAdded(Control target) {
        // ContextRequested is fired when the user wants to open the context menu.
        // Usually originates from handling a right click mouse event
        target.ContextRequested += this.requestContextHandler;
    }

    private void OnOwnerRemoved(Control target) {
        target.ContextRequested -= this.requestContextHandler;
    }

    public bool PushCachedItem(Type entryType, Control item) => AdvancedMenuHelper.PushCachedItem(this.itemCache, entryType, item);

    public Control? PopCachedItem(Type entryType) => AdvancedMenuHelper.PopCachedItem(this.itemCache, entryType);

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
            oldMenu.OnOwnerRemoved(target);
        }

        if (newValue != null) {
            // Generate context menu, if required
            if (!contextMenus.TryGetValue(newValue, out AdvancedContextMenu? menu)) {
                contextMenus[newValue] = menu = new AdvancedContextMenu(newValue);
                List<IMenuEntry> contextObjects = new List<IMenuEntry>();

                int i = 0;
                foreach (KeyValuePair<string, IWeightedMenuEntryGroup> entry in newValue.Groups) {
                    if (i++ != 0)
                        contextObjects.Add(new SeparatorEntry());

                    switch (entry.Value) {
                        case FixedWeightedMenuEntryGroup fixedGroup:     contextObjects.AddRange(fixedGroup.Items); break;
                        case DynamicWeightedMenuEntryGroup dynamicGroup: contextObjects.Add(new DynamicGroupPlaceholderMenuEntry(dynamicGroup)); break;
                    }
                }

                AdvancedMenuHelper.OnLogicalItemsAdded(menu, 0, contextObjects);
            }

            // Slide in and add ContextRequested handler before the base ContextMenu
            // class does, so that we can update the target trying to open the menu
            menu.OnOwnerAdded(target);
            target.ContextMenu = menu;
        }
        else {
            target.ContextMenu = null;
        }
    }
}