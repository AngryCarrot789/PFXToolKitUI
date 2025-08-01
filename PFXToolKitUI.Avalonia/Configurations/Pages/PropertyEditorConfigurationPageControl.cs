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

using Avalonia.Controls.Primitives;
using PFXToolKitUI.Avalonia.PropertyEditing;
using PFXToolKitUI.Avalonia.Utils;
using PFXToolKitUI.Configurations;

namespace PFXToolKitUI.Avalonia.Configurations.Pages;

/// <summary>
/// A configuration page control that consists entirely of a property editor control
/// </summary>
public class PropertyEditorConfigurationPageControl : BaseConfigurationPageControl {
    private PropertyEditorControl? PART_PropertyEditor;

    public PropertyEditorConfigurationPageControl() { }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.PART_PropertyEditor = e.NameScope.GetTemplateChild<PropertyEditorControl>("PART_PropertyEditor");
        this.PART_PropertyEditor.ApplyStyling();
        this.PART_PropertyEditor.ApplyTemplate();
    }

    public override void OnConnected() {
        base.OnConnected();
        this.PART_PropertyEditor!.PropertyEditor = ((PropertyEditorConfigurationPage) this.Page!).PropertyEditor;
    }

    public override void OnDisconnected() {
        base.OnDisconnected();
        this.PART_PropertyEditor!.PropertyEditor = null;
    }
}