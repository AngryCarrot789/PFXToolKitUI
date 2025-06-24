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

using PFXToolKitUI.DataTransfer;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.PropertyEditing.DataTransfer;

public delegate void SlotAnticipatedLineCountChangedEventHandler(DataParameterStringPropertyEditorSlot sender);

public class DataParameterStringPropertyEditorSlot : DataParameterPropertyEditorSlot {
    private string? value;
    private int anticipatedLineCount = -1;

    public string? Value {
        get => this.value;
        set {
            this.value = value;
            DataParameterString parameter = this.Parameter;
            for (int i = 0, c = this.Handlers.Count; i < c; i++) {
                parameter.SetValue((ITransferableData) this.Handlers[i], value);
            }

            this.OnValueChanged(false, true);
        }
    }

    /// <summary>
    /// Gets or sets the number of lines that will probably be taken up by this property. Default is -1, which means ignored. Value must be -1 or greater than 0
    /// </summary>
    public int AnticipatedLineCount {
        get => this.anticipatedLineCount;
        set {
            if (value < 0 && value != -1)
                throw new ArgumentOutOfRangeException(nameof(value), "Value must be -1 or greater than zero");

            PropertyHelper.SetAndRaiseINE(ref this.anticipatedLineCount, value, this, static t => t.AnticipatedLineCountChanged?.Invoke(t));
        }
    }

    public event SlotAnticipatedLineCountChangedEventHandler? AnticipatedLineCountChanged;

    public new DataParameterString Parameter => (DataParameterString) base.Parameter;

    public DataParameterStringPropertyEditorSlot(DataParameterString parameter, Type applicableType, string displayName) : base(parameter, applicableType, displayName) {
    }

    public override void QueryValueFromHandlers() {
        this.HasMultipleValues = !CollectionUtils.GetEqualValue(this.Handlers, (x) => this.Parameter.GetValue((ITransferableData) x), out this.value);
    }
}