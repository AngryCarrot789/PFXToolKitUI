using System.Numerics;
using PFXToolKitUI.Utils.Accessing;

namespace PFXToolKitUI.DataTransfer;

public class DataParameterNumber<T> : DataParameter<T>, IRangedParameter<T> where T : INumberBase<T>, IMinMaxValue<T>, IComparable<T> {
    public T Minimum { get; }
    
    public T Maximum { get; }
    
    public bool HasExplicitRangeLimit { get; }
    
    public DataParameterNumber(Type ownerType, string name, ValueAccessor<T> accessor) : this(ownerType, name, default(T), accessor) {
    }

    public DataParameterNumber(Type ownerType, string name, T? defValue, ValueAccessor<T> accessor) : this(ownerType, name, defValue, T.MinValue, T.MaxValue, accessor) {
    }

    public DataParameterNumber(Type ownerType, string name, T? defValue, T minValue, T maxValue, ValueAccessor<T> accessor) : base(ownerType, name, defValue, accessor) {
        if (minValue.CompareTo(maxValue) > 0)
            throw new ArgumentException($"Minimum value exceeds the maximum value: {minValue} > {maxValue}", nameof(minValue));
        if (defValue != null && (defValue.CompareTo(minValue) < 0 || defValue.CompareTo(maxValue) > 0))
            throw new ArgumentOutOfRangeException(nameof(defValue), $"Default value ({defValue}) falls out of range of the min/max values ({minValue}/{maxValue})");
        this.Minimum = minValue;
        this.Maximum = maxValue;
        this.HasExplicitRangeLimit = !minValue.Equals(T.MinValue) || !maxValue.Equals(T.MaxValue);
    }
    
    public T Clamp(T value) {
        if (this.HasExplicitRangeLimit) {
            T safe = value.CompareTo(this.Maximum) <= 0 ? value : this.Maximum;
            return safe.CompareTo(this.Minimum) >= 0 ? safe : this.Minimum;
        }
        else {
            return value;
        }
    }

    public bool IsValueOutOfRange(T value) => this.HasExplicitRangeLimit && (value.CompareTo(this.Minimum) < 0 || value.CompareTo(this.Maximum) > 0);

    public override void SetValue(ITransferableData owner, T value) {
        base.SetValue(owner, this.Clamp(value));
    }

    public override void SetObjectValue(ITransferableData owner, object? value) {
        if (this.HasExplicitRangeLimit) {
            T unboxed = (T) value!;
            T clamped = this.Clamp(unboxed);
            if (!unboxed.Equals(clamped)) {
                value = clamped;
            }
        }

        base.SetObjectValue(owner, value);
    }
}