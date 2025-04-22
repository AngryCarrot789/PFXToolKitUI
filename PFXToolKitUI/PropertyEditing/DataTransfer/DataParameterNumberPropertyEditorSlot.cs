// 
// Copyright (c) 2023-2025 REghZy
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

using System.Numerics;
using System.Runtime.CompilerServices;
using PFXToolKitUI.DataTransfer;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.PropertyEditing.DataTransfer;

public abstract class BaseDataParameterNumberPropertyEditorSlot : DataParameterFormattablePropertyEditorSlot {
    private DragStepProfile stepProfile;
    
    public DragStepProfile StepProfile {
        get => this.stepProfile;
        set {
            this.stepProfile = value;
            this.StepProfileChanged?.Invoke(this);
        }
    }
    
    public event DataParameterPropertyEditorSlotEventHandler? StepProfileChanged;
    
    protected BaseDataParameterNumberPropertyEditorSlot(DataParameter parameter, Type applicableType, string? displayName = null) : base(parameter, applicableType, displayName) {
    }
} 

public class DataParameterNumberPropertyEditorSlot<T> : BaseDataParameterNumberPropertyEditorSlot where T : INumberBase<T>, IMinMaxValue<T>, IComparable<T> {
    private static readonly T TWO = T.CreateChecked(2);
    
    private T value;

    public new DataParameterNumber<T> Parameter => Unsafe.As<DataParameterNumber<T>>(base.Parameter);

    public T Value {
        get => this.value;
        set {
            T oldVal = this.value;
            this.value = value;
            bool useAddition = false; //this.IsMultiHandler; TODO: Fix with new NumberDragger
            T change = value - oldVal;
            DataParameterNumber<T> p = this.Parameter;
            for (int i = 0, c = this.Handlers.Count; i < c; i++) {
                ITransferableData obj = (ITransferableData) this.Handlers[i];
                T newValue = p.Clamp(useAddition ? (p.GetValue(obj) + change) : value);
                p.SetValue(obj, newValue);
            }

            this.OnValueChanged(this.lastQueryHasMultipleValues && useAddition, true);
        }
    }
    
    public DataParameterNumberPropertyEditorSlot(DataParameterNumber<T> parameter, Type applicableType, string? displayName = null) : this(parameter, null, applicableType, displayName) {
    }
    
    public DataParameterNumberPropertyEditorSlot(DataParameterNumber<T> parameter, Type applicableType, string? displayName, DragStepProfile stepProfile) : this(parameter, null, applicableType, displayName, stepProfile) {
    }
    
    public DataParameterNumberPropertyEditorSlot(DataParameterNumber<T> parameter, DataParameter<bool>? isEditable, Type applicableType, string? displayName = null) : this(parameter, isEditable, applicableType, displayName, DragStepProfile.Percentage) {
    }
    
    public DataParameterNumberPropertyEditorSlot(DataParameterNumber<T> parameter, DataParameter<bool>? isEditable, Type applicableType, string? displayName, DragStepProfile stepProfile) : base(parameter, applicableType, displayName) {
        this.StepProfile = stepProfile;
        this.IsEditableDataParameter = isEditable;
    }

    public override void QueryValueFromHandlers() {
        this.HasMultipleValues = !CollectionUtils.GetEqualValue(this.Handlers, (x) => this.Parameter.GetValue((ITransferableData) x), out this.value);
        if (this.HasMultipleValues) {
            this.value = T.Abs(this.Parameter.Maximum - this.Parameter.Minimum) / TWO;
        }
    }
}