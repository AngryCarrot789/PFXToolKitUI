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

using System.Runtime.CompilerServices;
using PFXToolKitUI.DataTransfer;
using PFXToolKitUI.Utils;
using SkiaSharp;

namespace PFXToolKitUI.PropertyEditing.DataTransfer;

public class DataParameterColourPropertyEditorSlot : DataParameterPropertyEditorSlot {
    private SKColor value;

    public SKColor Value {
        get => this.value;
        set {
            this.value = value;
            DataParameter<SKColor> parameter = this.Parameter;
            for (int i = 0, c = this.Handlers.Count; i < c; i++) {
                parameter.SetValue((ITransferableData) this.Handlers[i], value);
            }

            this.OnValueChanged(false, true);
        }
    }

    public new DataParameter<SKColor> Parameter => Unsafe.As<DataParameter<SKColor>>(base.Parameter);

    public DataParameterColourPropertyEditorSlot(DataParameter<SKColor> parameter, Type applicableType, string displayName) : base(parameter, applicableType, displayName) {
    }

    public DataParameterColourPropertyEditorSlot(DataParameter<SKColor> parameter, DataParameter<bool> isEditableParameter, bool invertIsEditable, Type applicableType, string displayName) : base(parameter, applicableType, displayName) {
        this.IsEditableDataParameter = isEditableParameter;
        this.InvertIsEditableForParameter = invertIsEditable;
    }

    public override void QueryValueFromHandlers() {
        this.HasMultipleValues = !CollectionUtils.GetEqualValue(this.Handlers, (x) => this.Parameter.GetValue((ITransferableData) x), out this.value);
    }
}