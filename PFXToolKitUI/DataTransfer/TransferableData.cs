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

using System.Diagnostics.CodeAnalysis;

namespace PFXToolKitUI.DataTransfer;

/// <summary>
/// A class which helps manage data properties relative to a specific instance of an owner
/// object. This class manages firing value changed events and processing data property flags,
/// which is why it is important that the setter methods of the data properties are not called directly
/// </summary>
public sealed class TransferableData {
    private class ParameterData {
        public bool isValueChanging;
        public event EventHandler<DataParameter, DataParameterValueChangedEventArgs>? ValueChanged;
        public object? storageValue; // only used when no value accessor is used

        public void RaiseValueChanged(DataParameter parameter, ITransferableData owner) {
            this.ValueChanged?.Invoke(parameter, new DataParameterValueChangedEventArgs(owner));
        }
    }

    private Dictionary<int, ParameterData>? paramData;

    public ITransferableData Owner { get; }

    /// <summary>
    /// An event fired when any parameter's value changes relative to our owner
    /// </summary>
    public event EventHandler<DataParameter, DataParameterValueChangedEventArgs>? ValueChanged;

    public TransferableData(ITransferableData owner) {
        this.Owner = owner ?? throw new ArgumentNullException(nameof(owner));
    }

    internal static void InternalAddHandler(DataParameter parameter, TransferableData owner, EventHandler<DataParameter, DataParameterValueChangedEventArgs> handler) {
        owner.GetOrCreateParamData(parameter).ValueChanged += handler;
    }

    internal static void InternalRemoveHandler(DataParameter parameter, TransferableData owner, EventHandler<DataParameter, DataParameterValueChangedEventArgs> handler) {
        if (owner.TryGetParameterData(parameter, out ParameterData? data)) {
            data.ValueChanged -= handler;
        }
    }

    public bool IsValueChanging(DataParameter parameter) {
        return this.TryGetParameterData(parameter, out ParameterData? data) && data.isValueChanging;
    }

    public bool IsParameterValid(DataParameter parameter) {
        // parameter.OwnerType.IsAssignableFrom(this.Owner.GetType());
        return parameter.OwnerType.IsInstanceOfType(this.Owner);
    }

    public void ValidateParameter(DataParameter parameter) {
        if (!this.IsParameterValid(parameter))
            throw new ArgumentException($"Parameter '{parameter.GlobalKey}' is incompatible for our owner. {this.Owner.GetType().Name} is not assignable to {parameter.OwnerType.Name}");
    }
    
    internal object? InternalGetStorageValue(DataParameter parameter) {
        return this.TryGetParameterData(parameter, out ParameterData? data) ? data.storageValue : null;
    }

    internal void InternalSetStorageValue(DataParameter parameter, object? storageValue) {
        this.GetOrCreateParamData(parameter).storageValue = storageValue;
    }

    private bool TryGetParameterData(DataParameter parameter, [NotNullWhen(true)] out ParameterData? data) {
        if (parameter == null)
            throw new ArgumentNullException(nameof(parameter), "Parameter cannot be null");
        if (this.paramData != null && this.paramData.TryGetValue(parameter.RuntimeId, out data))
            return true;
        this.ValidateParameter(parameter);
        data = null;
        return false;
    }

    private ParameterData GetOrCreateParamData(DataParameter parameter) {
        if (parameter == null)
            throw new ArgumentNullException(nameof(parameter), "Parameter cannot be null");

        ParameterData? data;
        if (this.paramData == null)
            this.paramData = new Dictionary<int, ParameterData>();
        else if (this.paramData.TryGetValue(parameter.RuntimeId, out data))
            return data;
        this.ValidateParameter(parameter);
        this.paramData[parameter.RuntimeId] = data = new ParameterData();
        return data;
    }

    internal static void InternalBeginValueChange(DataParameter parameter, ITransferableData owner) {
        ParameterData internalData = owner.TransferableData.GetOrCreateParamData(parameter);
        if (internalData.isValueChanging) {
            throw new InvalidOperationException("Value is already changing. This exception is thrown, as the alternative is most likely a stack overflow exception");
        }

        internalData.isValueChanging = true;
    }

    internal static void InternalEndValueChange(DataParameter parameter, ITransferableData owner) {
        TransferableData data = owner.TransferableData;
        ParameterData internalData = data.GetOrCreateParamData(parameter);
        try {
            DataParameter.InternalOnParameterValueChanged(parameter, owner, true);
            internalData.RaiseValueChanged(parameter, owner);
            data.ValueChanged?.Invoke(parameter, new DataParameterValueChangedEventArgs(owner));
            DataParameter.InternalOnParameterValueChanged(parameter, owner, false);
        }
        finally {
            internalData.isValueChanging = false;
        }
    }
}