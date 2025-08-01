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

using PFXToolKitUI.Utils;
using PFXToolKitUI.Utils.Accessing;

namespace PFXToolKitUI.DataTransfer;

/// <summary>
/// A <see cref="DataParameter{T}"/> that manages a string value, and provides a character limit (both upper and lower limits)
/// </summary>
public sealed class DataParameterString : DataParameter<string?> {
    private readonly bool hasCharLimit;

    /// <summary>
    /// The smallest number of characters allowed. Default is 0
    /// </summary>
    public int MinimumChars { get; }

    /// <summary>
    /// The maximum number of characters allowed in the string. Default is <see cref="int.MaxValue"/>, meaning effectively unlimited
    /// </summary>
    public int MaximumChars { get; }

    /// <summary>
    /// Returns true if this string can be set to null.
    /// </summary>
    public bool IsNullable { get; }

    public DataParameterString(Type ownerType, string name, ValueAccessor<string?> accessor, bool isNullable = true) : this(ownerType, name, null, accessor, isNullable) {
    }

    public DataParameterString(Type ownerType, string name, string? defValue, ValueAccessor<string?> accessor, bool isNullable = true) : this(ownerType, name, defValue, 0, int.MaxValue, accessor, isNullable) {
    }

    public DataParameterString(Type ownerType, string name, string? defValue, int minChars, int maxChars, ValueAccessor<string?> accessor, bool isNullable = true) : base(ownerType, name, defValue, accessor) {
        if (minChars > maxChars)
            throw new ArgumentException($"Minimum value exceeds the maximum value: {minChars} > {maxChars}", nameof(minChars));
        this.IsNullable = isNullable;

        if (isNullable && defValue == null)
            defValue = "";

        this.hasCharLimit = minChars != 0 || maxChars != int.MaxValue;
        if (this.hasCharLimit && ((defValue ?? "").Length < minChars || (defValue ?? "").Length > maxChars))
            throw new ArgumentOutOfRangeException(nameof(defValue), $"Default value '{defValue ?? ""}' length of {defValue?.Length ?? 0} falls out of range of the min-max values ({minChars}-{maxChars})");

        this.MinimumChars = minChars;
        this.MaximumChars = maxChars;
    }

    public override void SetValue(ITransferableData owner, string? value) {
        base.SetValue(owner, this.CoerceValue(value));
    }

    public override void SetObjectValue(ITransferableData owner, object? value) {
        base.SetObjectValue(owner, this.CoerceValue((string?) value));
    }

    private string? CoerceValue(string? value) {
        if (this.hasCharLimit) {
            if (value == null || value.Length < this.MinimumChars) {
                if (value == null) {
                    value = this.DefaultValue ?? "";
                }
                else {
                    value += StringUtils.Repeat(' ', this.MinimumChars - value.Length);
                }
            }
            else if (value.Length > this.MaximumChars) {
                value = value.Substring(0, this.MaximumChars);
            }
        }

        return this.IsNullable ? value : (value ?? "");
    }
}