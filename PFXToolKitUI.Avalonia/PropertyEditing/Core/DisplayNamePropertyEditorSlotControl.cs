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

using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using PFXToolKitUI.Avalonia.Bindings;
using PFXToolKitUI.Avalonia.Utils;
using PFXToolKitUI.PropertyEditing.Core;

namespace PFXToolKitUI.Avalonia.PropertyEditing.Core;

public class DisplayNamePropertyEditorSlotControl : BasePropertyEditorSlotControl {
    public new DisplayNamePropertyEditorSlot? SlotModel => (DisplayNamePropertyEditorSlot?) base.SlotControl.Model;

    private TextBox displayNameBox;

    private readonly AvaloniaPropertyToEventPropertyGetSetBinder<DisplayNamePropertyEditorSlot> displayNameBinder = new AvaloniaPropertyToEventPropertyGetSetBinder<DisplayNamePropertyEditorSlot>(TextBox.TextProperty, nameof(DisplayNamePropertyEditorSlot.DisplayNameChanged), binder => binder.Model.DisplayName, (binder, v) => binder.Model.SetValue((string) v));

    public DisplayNamePropertyEditorSlotControl() {
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.displayNameBox = e.NameScope.GetTemplateChild<TextBox>("PART_TextBox");
    }

    protected override void OnConnected() {
        this.displayNameBinder.Attach(this.displayNameBox, this.SlotModel!);
    }

    protected override void OnDisconnected() {
        this.displayNameBinder.Detach();
    }
}