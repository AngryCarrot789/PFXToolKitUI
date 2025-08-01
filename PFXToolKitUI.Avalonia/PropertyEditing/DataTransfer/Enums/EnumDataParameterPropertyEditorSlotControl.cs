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
using PFXToolKitUI.PropertyEditing.DataTransfer.Enums;

namespace PFXToolKitUI.Avalonia.PropertyEditing.DataTransfer.Enums;

public class EnumDataParameterPropertyEditorSlotControl<TEnum> : BaseEnumDataParameterPropertyEditorSlotControlEx where TEnum : unmanaged, Enum {
    // TODO: maybe add a property in the slot for the type of
    // control to use? combobox, radiobuttons, etc.

    public new DataParameterEnumPropertyEditorSlot<TEnum>? SlotModel => (DataParameterEnumPropertyEditorSlot<TEnum>?) base.SlotControl!.Model;

    public EnumDataParameterPropertyEditorSlotControl() {
    }

    protected override void UpdateControlValue() {
        DataParameterEnumPropertyEditorSlot<TEnum> slot = this.SlotModel!;
        if (slot.TranslationInfo != null && slot.TranslationInfo.EnumToText.TryGetValue(slot.Value, out string? text)) {
            this.comboBox!.SelectedItem = text;
        }
        else {
            this.comboBox!.SelectedItem = slot.Value.ToString();
        }
    }

    protected override void UpdateModelValue() {
        TEnum? value;
        DataParameterEnumPropertyEditorSlot<TEnum> slot = this.SlotModel!;
        if (slot.TranslationInfo != null) {
            if (this.comboBox!.SelectedItem is string selectedText) {
                value = slot.TranslationInfo.TextToEnum[selectedText];
            }
            else {
                value = slot.DefaultValue;
            }
        }
        else if (this.comboBox!.SelectedItem is TEnum selectedValue) {
            value = selectedValue;
        }
        else {
            value = slot.DefaultValue;
        }

        slot.Value = value ?? default;
    }

    protected override void OnCanEditValueChanged(bool canEdit) {
        this.comboBox!.IsEnabled = canEdit;
    }

    protected override void OnConnected() {
        // Initialise list first so that UpdateControlValue has something to work on when base.OnConnected invokes it eventually 

        ItemCollection list = this.comboBox!.Items;
        list.Clear();

        if (this.SlotModel!.TranslationInfo != null) {
            foreach (string value in this.SlotModel.TranslationInfo.TextList) {
                list.Add(value);
            }
        }
        else {
            foreach (TEnum value in this.SlotModel!.AllowedValues) {
                list.Add(value);
            }
        }

        base.OnConnected();
        if (this.SlotModel.OptionArrangement is EnumArrangementComboBox arrangement) {
            if (!arrangement.IsCompact) {
                this.comboBox!.Margin = new Thickness(2, 3);
                this.comboBox.Padding = new Thickness(3);
            }
        }
    }
}