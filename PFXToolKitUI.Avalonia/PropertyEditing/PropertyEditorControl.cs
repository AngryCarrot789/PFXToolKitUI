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
using PFXToolKitUI.Avalonia.Utils;
using PFXToolKitUI.PropertyEditing;

namespace PFXToolKitUI.Avalonia.PropertyEditing;

public class PropertyEditorControl : TemplatedControl {
    private static readonly GridLength Star = new GridLength(1, GridUnitType.Star);

    public static readonly StyledProperty<PropertyEditor?> PropertyEditorProperty = AvaloniaProperty.Register<PropertyEditorControl, PropertyEditor?>("PropertyEditor");
    public static readonly StyledProperty<GridLength> ColumnWidth0Property = AvaloniaProperty.Register<PropertyEditorControl, GridLength>("ColumnWidth0", new GridLength(85d));
    public static readonly StyledProperty<GridLength> ColumnWidth1Property = AvaloniaProperty.Register<PropertyEditorControl, GridLength>("ColumnWidth1", new GridLength(5));
    public static readonly StyledProperty<GridLength> ColumnWidth2Property = AvaloniaProperty.Register<PropertyEditorControl, GridLength>("ColumnWidth2", Star);

    // public static readonly AvaloniaProperty PropertyEditorProperty = AvaloniaProperty.Register("PropertyEditor", typeof(BasePropertyEditor), typeof(PropertyEditorControl), new PropertyMetadata(null, (d, e) => ((PropertyEditorControl) d).OnPropertyEditorChanged((BasePropertyEditor) e.OldValue, (BasePropertyEditor) e.NewValue)));

    public PropertyEditor? PropertyEditor {
        get => this.GetValue(PropertyEditorProperty);
        set => this.SetValue(PropertyEditorProperty, value);
    }

    public GridLength ColumnWidth0 { get => this.GetValue(ColumnWidth0Property); set => this.SetValue(ColumnWidth0Property, value); }
    public GridLength ColumnWidth1 { get => this.GetValue(ColumnWidth1Property); set => this.SetValue(ColumnWidth1Property, value); }
    public GridLength ColumnWidth2 { get => this.GetValue(ColumnWidth2Property); set => this.SetValue(ColumnWidth2Property, value); }

    public PropertyEditorGroupControl RootGroupControl { get; private set; }

    public PropertyEditorSlotContainerControl TouchedSlotContainer { get; set; }

    public PropertyEditorControl() {
    }

    static PropertyEditorControl() {
        PropertyEditorProperty.Changed.AddClassHandler<PropertyEditorControl, PropertyEditor?>((o, e) => o.OnPropertyEditorChanged(e.GetOldValue<PropertyEditor?>(), e.GetNewValue<PropertyEditor?>()));
    }

    // protected override void OnMouseDown(MouseButtonEventArgs e) {
    //     base.OnMouseDown(e);
    //     if (e.LeftButton != MouseButtonState.Pressed || e.Handled) {
    //         return;
    //     }
    // 
    //     if (this.TouchedSlot != null) {
    //         return;
    //     }
    // 
    //     this.PropertyEditor?.ClearSelection();
    // }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.RootGroupControl = e.NameScope.GetTemplateChild<PropertyEditorGroupControl>("PART_RootGroupControl");
        this.RootGroupControl.ApplyStyling();
        this.RootGroupControl.ApplyTemplate();
    }

    private void OnPropertyEditorChanged(PropertyEditor? oldEditor, PropertyEditor? newEditor) {
        if (oldEditor != null) {
            this.RootGroupControl.DisconnectModel();
        }

        if (newEditor != null) {
            this.RootGroupControl.ConnectModel(this, newEditor.Root);
        }
    }

    // TODO: cache items
    public bool PushCachedItem(BasePropertyEditorSlotControl slot) {
        return false;
    }
}

public class PropertyEditorWrapControl : PropertyEditorControl;