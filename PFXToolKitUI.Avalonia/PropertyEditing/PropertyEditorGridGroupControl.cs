// 
// Copyright (c) 2023-2025 REghZy
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
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using PFXToolKitUI.Avalonia.Bindings;
using PFXToolKitUI.Avalonia.Utils;
using PFXToolKitUI.PropertyEditing;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.Avalonia.PropertyEditing;

public class PropertyEditorGridGroupControl : TemplatedControl {
    public static readonly StyledProperty<bool> IsExpandedProperty = AvaloniaProperty.Register<PropertyEditorGridGroupControl, bool>("IsExpanded");
    public static readonly StyledProperty<GroupType> GroupTypeProperty = AvaloniaProperty.Register<PropertyEditorGridGroupControl, GroupType>("GroupType");
    public static readonly StyledProperty<PropertyEditorControl?> PropertyEditorProperty = AvaloniaProperty.Register<PropertyEditorGridGroupControl, PropertyEditorControl?>("PropertyEditor");

    public bool IsExpanded {
        get => this.GetValue(IsExpandedProperty);
        set => this.SetValue(IsExpandedProperty, value);
    }

    public GroupType GroupType {
        get => this.GetValue(GroupTypeProperty);
        set => this.SetValue(GroupTypeProperty, value);
    }

    public PropertyEditorControl? PropertyEditor {
        get => this.GetValue(PropertyEditorProperty);
        set => this.SetValue(PropertyEditorProperty, value);
    }

    public PropertyEditorItemsPanel? Panel { get; private set; }

    public BasePropertyEditorGroup? Model { get; private set; }

    public Expander? TheExpander { get; private set; }

    private readonly IBinder<BasePropertyEditorGroup> displayNameBinder = new EventUpdateBinder<BasePropertyEditorGroup>(nameof(BasePropertyEditorGroup.DisplayNameChanged), UpdateControlDisplayName);
    private readonly IBinder<BasePropertyEditorGroup> isVisibleBinder = new EventUpdateBinder<BasePropertyEditorGroup>(nameof(BasePropertyEditorGroup.IsCurrentlyApplicableChanged), b => ((PropertyEditorGridGroupControl) b.Control).IsVisible = b.Model.IsRoot || b.Model.IsVisible);
    private readonly IBinder<BasePropertyEditorGroup> isExpandedBinder = new AvaloniaPropertyToEventPropertyGetSetBinder<BasePropertyEditorGroup>(IsExpandedProperty, nameof(BasePropertyEditorGroup.IsExpandedChanged), b => b.Model.IsExpanded.Box(), (b, v) => b.Model.IsExpanded = (bool) v!);

    public PropertyEditorGridGroupControl() {
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e) {
        base.OnPointerPressed(e);
        // WONKY CHANGE e.OriginalSource -> e.Source
        if (!e.Handled && e.Source is PropertyEditorItemsPanel) {
            e.Handled = true;
            this.PropertyEditor?.PropertyEditor?.ClearSelection();
            this.Focus();
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.Panel = e.NameScope.GetTemplateChild<PropertyEditorItemsPanel>("PART_Panel");
        // this.Panel.OwnerGroup = this;
        if (e.NameScope.TryGetTemplateChild("PART_Expander", out Expander? expander)) {
            this.TheExpander = expander;
        }
    }

    public void ConnectModel(PropertyEditorControl propertyEditor, BasePropertyEditorGroup group) {
        if (propertyEditor == null)
            throw new ArgumentNullException(nameof(propertyEditor));
        this.PropertyEditor = propertyEditor;
        this.Model = group;
        group.ItemAdded += this.ModelOnItemAdded;
        group.ItemRemoved += this.ModelOnItemRemoved;
        group.ItemMoved += this.ModelOnItemMoved;
        this.GroupType = group.GroupType;
        this.displayNameBinder.Attach(this, group);
        this.isVisibleBinder.Attach(this, group);
        this.isExpandedBinder.Attach(this, group);

        int i = 0;
        foreach (BasePropertyEditorObject obj in group.PropertyObjects) {
            this.Panel!.InsertItem(obj, i++);
        }
    }

    public void DisconnectModel() {
        for (int i = this.Panel!.Count - 1; i >= 0; i--) {
            this.Panel.RemoveItem(i);
        }

        this.displayNameBinder.Detach();
        this.isVisibleBinder.Detach();
        this.isExpandedBinder.Detach();
        this.Model!.ItemAdded -= this.ModelOnItemAdded;
        this.Model.ItemRemoved -= this.ModelOnItemRemoved;
        this.Model.ItemMoved -= this.ModelOnItemMoved;
        this.Model = null;
    }

    private void ModelOnItemAdded(object? sender, PropertyEditorObjectIndexEventArgs e) {
        this.Panel!.InsertItem(e.Item, e.Index);
    }

    private void ModelOnItemRemoved(object? sender, PropertyEditorObjectIndexEventArgs e) {
        this.Panel!.RemoveItem(e.Index);
    }

    private void ModelOnItemMoved(object? sender, PropertyEditorObjectMovedEventArgs e) {
        this.Panel!.MoveItem(e.OldIndex, e.NewIndex);
    }

    private static void UpdateControlDisplayName(IBinder<BasePropertyEditorGroup> obj) {
        PropertyEditorGridGroupControl ctrl = (PropertyEditorGridGroupControl) obj.Control;
        if (ctrl.TheExpander != null)
            ctrl.TheExpander.Header = obj.Model.DisplayName;
    }
}