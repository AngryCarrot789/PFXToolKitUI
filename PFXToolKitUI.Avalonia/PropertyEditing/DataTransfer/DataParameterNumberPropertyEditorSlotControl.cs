// 
// Copyright (c) 2024-2025 REghZy
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
using PFXToolKitUI.DataTransfer;
using PFXToolKitUI.PropertyEditing.DataTransfer;

namespace PFXToolKitUI.Avalonia.PropertyEditing.DataTransfer;

/// <summary>
/// The base control class for numeric data parameter slots
/// </summary>
/// <typeparam name="T">Value type</typeparam>
public abstract class DataParameterNumberPropertyEditorSlotControl<T> : BaseNumberDraggerDataParamPropEditorSlotControl where T : unmanaged, INumberBase<T>, IMinMaxValue<T>, IComparable<T>, IConvertible {
    public new DataParameterNumberPropertyEditorSlot<T>? SlotModel => (DataParameterNumberPropertyEditorSlot<T>?) base.SlotControl?.Model;
    
    public override double SlotValue {
        get => this.SlotModel!.Value.ToDouble(null);
        set => this.SlotModel!.Value = T.CreateChecked(value);
    }
    
    protected override void OnConnected() {
        base.OnConnected();
        DataParameterNumberPropertyEditorSlot<T>? slot = this.SlotModel!;
        DataParameterNumber<T> param = slot.Parameter;
        this.dragger!.Minimum = param.Minimum.ToDouble(null);
        this.dragger!.Maximum = param.Maximum.ToDouble(null);

        DragStepProfile profile = slot.StepProfile;
        this.dragger!.TinyChange = profile.TinyStep;
        this.dragger!.SmallChange = profile.SmallStep;
        this.dragger!.NormalChange = profile.NormalStep;
        this.dragger!.LargeChange = profile.LargeStep;
    }

    protected override void ResetValue() => this.SlotModel!.Value = this.SlotModel!.Parameter.DefaultValue;
}

public class DataParameterSBytePropertyEditorSlotControl : DataParameterNumberPropertyEditorSlotControl<sbyte>;
public class DataParameterBytePropertyEditorSlotControl : DataParameterNumberPropertyEditorSlotControl<byte>;
public class DataParameterShortPropertyEditorSlotControl : DataParameterNumberPropertyEditorSlotControl<short>;
public class DataParameterUShortPropertyEditorSlotControl : DataParameterNumberPropertyEditorSlotControl<ushort>;
public class DataParameterIntPropertyEditorSlotControl : DataParameterNumberPropertyEditorSlotControl<int>;
public class DataParameterUIntPropertyEditorSlotControl : DataParameterNumberPropertyEditorSlotControl<uint>;
public class DataParameterLongPropertyEditorSlotControl : DataParameterNumberPropertyEditorSlotControl<long>;
public class DataParameterULongPropertyEditorSlotControl : DataParameterNumberPropertyEditorSlotControl<ulong>;
public class DataParameterFloatPropertyEditorSlotControl : DataParameterNumberPropertyEditorSlotControl<float>;
public class DataParameterDoublePropertyEditorSlotControl : DataParameterNumberPropertyEditorSlotControl<double>;
public class DataParameterDecimalPropertyEditorSlotControl : DataParameterNumberPropertyEditorSlotControl<decimal>;