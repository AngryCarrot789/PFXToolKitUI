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

using System.Collections.ObjectModel;
using PFXToolKitUI.DataTransfer;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.PropertyEditing.DataTransfer.Enums;

public class DataParameterEnumPropertyEditorSlot<TEnum> : DataParameterPropertyEditorSlot where TEnum : unmanaged, Enum {
    public readonly DataParameterEnumInfo<TEnum>? TranslationInfo;

    /// <summary>
    /// An enumerable which returns the allowed enum values
    /// </summary>
    public ReadOnlyCollection<TEnum> AllowedValues { get; }

    private TEnum value;

    public TEnum Value {
        get => this.value;
        set {
            if (!EnumInfo<TEnum>.EnumValuesSet.Contains(value))
                value = default;

            if (EqualityComparer<TEnum>.Default.Equals(value, this.value))
                return;

            this.value = value;
            DataParameter<TEnum> parameter = this.Parameter;
            for (int i = 0, c = this.Handlers.Count; i < c; i++) {
                parameter.SetValue((ITransferableData) this.Handlers[i], value);
            }

            this.OnValueChanged(false, true);
        }
    }

    public TEnum? DefaultValue => ReferenceEquals(this.AllowedValues, EnumInfo<TEnum>.EnumValues) ? default : this.AllowedValues.FirstOrDefault();

    public new DataParameter<TEnum> Parameter => (DataParameter<TEnum>) base.Parameter;

    public EnumOptionArrangement OptionArrangement { get; init; } = new EnumArrangementComboBox(false);

    public DataParameterEnumPropertyEditorSlot(DataParameter<TEnum> parameter, Type applicableType, string displayName, IEnumerable<TEnum>? values = null, DataParameterEnumInfo<TEnum>? translationInfo = null) : base(parameter, applicableType, displayName) {
        this.TranslationInfo = translationInfo;
        this.AllowedValues = values != null ? values.Distinct().ToList().AsReadOnly() : EnumInfo<TEnum>.EnumValues;
    }

    public override void QueryValueFromHandlers() {
        TEnum? val = CollectionUtils.GetEqualValue(this.Handlers, (x) => this.Parameter.GetValue((ITransferableData) x), out TEnum? d) ? d : null;
        this.value = val.HasValue && EnumInfo<TEnum>.EnumValuesSet.Contains(val.Value) ? val.Value : default;
    }
}