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

using Avalonia.Controls.Primitives;
using PFXToolKitUI.Avalonia.PropertyEditing.Core;
using PFXToolKitUI.Avalonia.PropertyEditing.DataTransfer;
using PFXToolKitUI.Avalonia.PropertyEditing.DataTransfer.Automatic;
using PFXToolKitUI.Avalonia.PropertyEditing.DataTransfer.Enums;
using PFXToolKitUI.Avalonia.Utils;
using PFXToolKitUI.PropertyEditing;
using PFXToolKitUI.PropertyEditing.Core;
using PFXToolKitUI.PropertyEditing.DataTransfer;
using PFXToolKitUI.PropertyEditing.DataTransfer.Automatic;
using PFXToolKitUI.PropertyEditing.DataTransfer.Enums;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.Avalonia.PropertyEditing;

/// <summary>
/// The base class for a property editor slot control. Slot controls are parented to a slot container control (which implements selection and visibility).
/// This is the class which represents the visuals of an
/// </summary>
public abstract class BasePropertyEditorSlotControl : TemplatedControl {
    public static readonly ModelControlRegistry<PropertyEditorSlot, BasePropertyEditorSlotControl> Registry;

    public PropertyEditorSlotContainerControl? SlotControl { get; private set; }

    public PropertyEditorSlot? SlotModel => this.SlotControl?.Model;

    public bool IsConnected => this.SlotControl != null;

    protected BasePropertyEditorSlotControl() {
    }

    static BasePropertyEditorSlotControl() {
        Registry = new ModelControlRegistry<PropertyEditorSlot, BasePropertyEditorSlotControl>();
        
        // standard editors
        Registry.RegisterType<DataParameterNumberPropertyEditorSlot<sbyte>>(() => new DataParameterSBytePropertyEditorSlotControl());
        Registry.RegisterType<DataParameterNumberPropertyEditorSlot<byte>>(() => new DataParameterBytePropertyEditorSlotControl());
        Registry.RegisterType<DataParameterNumberPropertyEditorSlot<short>>(() => new DataParameterShortPropertyEditorSlotControl());
        Registry.RegisterType<DataParameterNumberPropertyEditorSlot<ushort>>(() => new DataParameterUShortPropertyEditorSlotControl());
        Registry.RegisterType<DataParameterNumberPropertyEditorSlot<int>>(() => new DataParameterIntPropertyEditorSlotControl());
        Registry.RegisterType<DataParameterNumberPropertyEditorSlot<uint>>(() => new DataParameterUIntPropertyEditorSlotControl());
        Registry.RegisterType<DataParameterNumberPropertyEditorSlot<long>>(() => new DataParameterLongPropertyEditorSlotControl());
        Registry.RegisterType<DataParameterNumberPropertyEditorSlot<ulong>>(() => new DataParameterULongPropertyEditorSlotControl());
        Registry.RegisterType<DataParameterNumberPropertyEditorSlot<float>>(() => new DataParameterFloatPropertyEditorSlotControl());
        Registry.RegisterType<DataParameterNumberPropertyEditorSlot<double>>(() => new DataParameterDoublePropertyEditorSlotControl());
        Registry.RegisterType<DataParameterNumberPropertyEditorSlot<decimal>>(() => new DataParameterDecimalPropertyEditorSlotControl());
        Registry.RegisterType<DataParameterBoolPropertyEditorSlot>(() => new DataParameterBoolPropertyEditorSlotControl());
        Registry.RegisterType<DataParameterStringPropertyEditorSlot>(() => new DataParameterStringPropertyEditorSlotControl());
        Registry.RegisterType<DataParameterVector2PropertyEditorSlot>(() => new DataParameterVector2PropertyEditorSlotControl());
        Registry.RegisterType<DataParameterColourPropertyEditorSlot>(() => new DataParameterColourPropertyEditorSlotControl());

        // automatic editors
        Registry.RegisterType<AutomaticNumericDataParameterPropertyEditorSlot<sbyte>>(() => new AutomaticDataParameterSBytePropertyEditorSlotControl());
        Registry.RegisterType<AutomaticNumericDataParameterPropertyEditorSlot<byte>>(() => new AutomaticDataParameterBytePropertyEditorSlotControl());
        Registry.RegisterType<AutomaticNumericDataParameterPropertyEditorSlot<short>>(() => new AutomaticDataParameterShortPropertyEditorSlotControl());
        Registry.RegisterType<AutomaticNumericDataParameterPropertyEditorSlot<ushort>>(() => new AutomaticDataParameterUShortPropertyEditorSlotControl());
        Registry.RegisterType<AutomaticNumericDataParameterPropertyEditorSlot<int>>(() => new AutomaticDataParameterIntPropertyEditorSlotControl());
        Registry.RegisterType<AutomaticNumericDataParameterPropertyEditorSlot<uint>>(() => new AutomaticDataParameterUIntPropertyEditorSlotControl());
        Registry.RegisterType<AutomaticNumericDataParameterPropertyEditorSlot<long>>(() => new AutomaticDataParameterLongPropertyEditorSlotControl());
        Registry.RegisterType<AutomaticNumericDataParameterPropertyEditorSlot<ulong>>(() => new AutomaticDataParameterULongPropertyEditorSlotControl());
        Registry.RegisterType<AutomaticNumericDataParameterPropertyEditorSlot<float>>(() => new AutomaticDataParameterFloatPropertyEditorSlotControl());
        Registry.RegisterType<AutomaticNumericDataParameterPropertyEditorSlot<double>>(() => new AutomaticDataParameterDoublePropertyEditorSlotControl());
        Registry.RegisterType<AutomaticNumericDataParameterPropertyEditorSlot<decimal>>(() => new AutomaticDataParameterDecimalPropertyEditorSlotControl());
        Registry.RegisterType<AutomaticDataParameterVector2PropertyEditorSlot>(() => new AutomaticDataParameterVector2PropertyEditorSlotControl());
        
        Registry.RegisterType<DisplayNamePropertyEditorSlot>(() => new DisplayNamePropertyEditorSlotControl());
    }

    public static void RegisterEnumControl<TEnum, TSlot>() where TEnum : struct, Enum where TSlot : DataParameterEnumPropertyEditorSlot<TEnum> {
        Registry.RegisterType<TSlot>(() => new EnumDataParameterPropertyEditorSlotControl<TEnum>());
    }

    public static BasePropertyEditorSlotControl NewOrCachedContentInstance(PropertyEditorSlot slot) {
        ArgumentNullException.ThrowIfNull(slot);
        return Registry.NewInstance(slot);
    }

    /// <summary>
    /// Connect this slot content to the given control
    /// </summary>
    public void Connect(PropertyEditorSlotContainerControl slotContainer) {
        this.SlotControl = slotContainer;
        this.OnConnected();
    }

    /// <summary>
    /// Disconnect this slot content from the slot control
    /// </summary>
    public void Disconnect() {
        this.OnDisconnected();
        this.SlotControl = null;
    }

    /// <summary>
    /// Invoked when this slot control is attached to a slot model
    /// </summary>
    protected abstract void OnConnected();

    /// <summary>
    /// Invoked when this slot control is detached from a slot model
    /// </summary>
    protected abstract void OnDisconnected();
}