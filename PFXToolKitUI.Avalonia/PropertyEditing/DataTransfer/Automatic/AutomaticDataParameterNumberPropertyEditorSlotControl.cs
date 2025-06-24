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

using System.Numerics;
using PFXToolKitUI.Avalonia.AvControls.Dragger;
using PFXToolKitUI.DataTransfer;
using PFXToolKitUI.PropertyEditing.DataTransfer;
using PFXToolKitUI.PropertyEditing.DataTransfer.Automatic;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.Avalonia.PropertyEditing.DataTransfer.Automatic;

public abstract class AutomaticDataParameterNumberPropertyEditorSlotControl<T> : BaseNumberDraggerDataParamPropEditorSlotControl where T : unmanaged, INumberBase<T>, IMinMaxValue<T>, IComparable<T>, IConvertible {
    public new AutomaticNumericDataParameterPropertyEditorSlot<T>? SlotModel => (AutomaticNumericDataParameterPropertyEditorSlot<T>?) base.SlotControl?.Model;

    public override double SlotValue {
        get => this.SlotModel!.Value.ToDouble(null);
        set => this.SlotModel!.Value = T.CreateChecked(value);
    }
    
    public AutomaticDataParameterNumberPropertyEditorSlotControl() {
    }

    protected override void UpdateControlValue() {
        base.UpdateControlValue();
        this.UpdateTextPreview();
    }
    
    protected override void OnConnected() {
        base.OnConnected();
        AutomaticNumericDataParameterPropertyEditorSlot<T> slot = this.SlotModel!;
        DataParameterNumber<T> param = slot.Parameter;
        this.dragger!.Minimum = param.Minimum.ToDouble(null);
        this.dragger!.Maximum = param.Maximum.ToDouble(null);

        DragStepProfile profile = slot.StepProfile;
        this.dragger!.TinyChange = profile.TinyStep;
        this.dragger!.SmallChange = profile.SmallStep;
        this.dragger!.NormalChange = profile.NormalStep;
        this.dragger!.LargeChange = profile.LargeStep;
        
        this.dragger!.InvalidInputEntered += this.PartDraggerOnInvalidInputEntered;
    }

    protected override void OnDisconnected() {
        base.OnDisconnected();
        this.dragger!.InvalidInputEntered -= this.PartDraggerOnInvalidInputEntered;
    }

    protected override void ResetValue() {
        AutomaticNumericDataParameterPropertyEditorSlot<T>? slot = this.SlotModel;
        if (slot == null) {
            return;
        }

        foreach (ITransferableData handler in slot.Handlers) {
            slot.IsAutomaticParameter.SetValue(handler, true);
        }
    }
    
    protected override void OnHandlersLoadedOverride(bool isLoaded) {
        base.OnHandlersLoadedOverride(isLoaded);
        if (isLoaded) {
            if (this.singleHandler != null)
                this.SlotModel!.IsAutomaticParameter.AddValueChangedHandler(this.singleHandler, this.OnIsAutomaticChanged);
        }
        else if (this.singleHandler != null) {
            this.SlotModel!.IsAutomaticParameter.RemoveValueChangedHandler(this.singleHandler, this.OnIsAutomaticChanged);
        }
    }

    private void OnIsAutomaticChanged(DataParameter parameter, ITransferableData owner) {
        this.UpdateTextPreview();
    }

    private void UpdateTextPreview() {
        if (this.singleHandler != null && this.SlotModel!.IsAutomaticParameter.GetValue(this.singleHandler) && !this.SlotModel.HasMultipleValues) {
            this.dragger!.FinalPreviewStringFormat = "{0} (Auto)";
        }
        else {
            this.dragger!.FinalPreviewStringFormat = null;
        }
    }

    private void PartDraggerOnInvalidInputEntered(object? sender, InvalidInputEnteredEventArgs e) {
        AutomaticNumericDataParameterPropertyEditorSlot<T>? model = this.SlotModel;
        if (model == null || !model.IsCurrentlyApplicable) {
            return;
        }

        if (("auto".EqualsIgnoreCase(e.Input) || "automatic".EqualsIgnoreCase(e.Input) || "\"auto\"".EqualsIgnoreCase(e.Input))) {
            foreach (object handler in model.Handlers) {
                model.IsAutomaticParameter.SetValue((ITransferableData) handler, true);
            }
        }
    }

    protected override void OnHasMultipleValuesChanged(DataParameterPropertyEditorSlot sender) {
        base.OnHasMultipleValuesChanged(sender);
        this.UpdateTextPreview();
    }
}

public class AutomaticDataParameterSBytePropertyEditorSlotControl : AutomaticDataParameterNumberPropertyEditorSlotControl<sbyte>;
public class AutomaticDataParameterBytePropertyEditorSlotControl : AutomaticDataParameterNumberPropertyEditorSlotControl<byte>;
public class AutomaticDataParameterShortPropertyEditorSlotControl : AutomaticDataParameterNumberPropertyEditorSlotControl<short>;
public class AutomaticDataParameterUShortPropertyEditorSlotControl : AutomaticDataParameterNumberPropertyEditorSlotControl<ushort>;
public class AutomaticDataParameterIntPropertyEditorSlotControl : AutomaticDataParameterNumberPropertyEditorSlotControl<int>;
public class AutomaticDataParameterUIntPropertyEditorSlotControl : AutomaticDataParameterNumberPropertyEditorSlotControl<uint>;
public class AutomaticDataParameterLongPropertyEditorSlotControl : AutomaticDataParameterNumberPropertyEditorSlotControl<long>;
public class AutomaticDataParameterULongPropertyEditorSlotControl : AutomaticDataParameterNumberPropertyEditorSlotControl<ulong>;
public class AutomaticDataParameterFloatPropertyEditorSlotControl : AutomaticDataParameterNumberPropertyEditorSlotControl<float>;
public class AutomaticDataParameterDoublePropertyEditorSlotControl : AutomaticDataParameterNumberPropertyEditorSlotControl<double>;
public class AutomaticDataParameterDecimalPropertyEditorSlotControl : AutomaticDataParameterNumberPropertyEditorSlotControl<decimal>;