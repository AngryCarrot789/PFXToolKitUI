using System.Numerics;
using PFXToolKitUI.DataTransfer;
using PFXToolKitUI.PropertyEditing.DataTransfer;

namespace PFXToolKitUI.Avalonia.PropertyEditing.DataTransfer;

/// <summary>
/// The base control class for numeric data parameter slots
/// </summary>
/// <typeparam name="T">Value type</typeparam>
public abstract class DataParameterNumberPropertyEditorSlotControl<T> : BaseNumberDraggerDataParamPropEditorSlotControl where T : INumberBase<T>, IMinMaxValue<T>, IComparable<T>, IConvertible {
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