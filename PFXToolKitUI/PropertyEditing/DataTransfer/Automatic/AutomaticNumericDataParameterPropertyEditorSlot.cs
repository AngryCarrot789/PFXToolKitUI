﻿// 
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
using System.Runtime.CompilerServices;
using PFXToolKitUI.DataTransfer;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.PropertyEditing.DataTransfer.Automatic;

/// <summary>
/// The base class for a numeric data parameter, but it supports an overridable "automatic value".
/// The presumption is that setting the automatic state to true causes the handlers' value parameter
/// to become automatically generated (as well as when their own internal values changes), and when
/// set to false, the value parameter becomes explicitly set and is not automatically generates anymore.
/// <para>
/// This is used for things like the scale and rotation origin points when the user wants them to be
/// relative to the center of the layer, because having to manually center it is annoying, so this slot
/// allows automatic calculation (center, right, bottom, bottom-right) or an explicit value (left, top, top-left)  
/// </para>
/// </summary>
/// <typeparam name="T">The parameter value type</typeparam>
public class AutomaticNumericDataParameterPropertyEditorSlot<T> : DataParameterFormattablePropertyEditorSlot where T : unmanaged, INumberBase<T>, IMinMaxValue<T>, IComparable<T> {
    private static readonly T TWO = T.CreateChecked(2);
    private DragStepProfile stepProfile = DragStepProfile.Percentage;
    protected T myLocalValue;

    /// <summary>
    /// Gets or sets the value. Setting this will update the value for all of our handlers,
    /// and it will also set the <see cref="IsAutomaticParameter"/> for all parameters to false
    /// </summary>
    public T Value {    
        get => this.myLocalValue;
        set {
            this.myLocalValue = value;
            DataParameterNumber<T> parameter = this.Parameter;
            for (int i = 0, c = this.Handlers.Count; i < c; i++) {
                ITransferableData obj = (ITransferableData) this.Handlers[i];
                // Set IsAutomatic to false so that the value is no longer auto-calculated by the handlers.
                // Though there ideally shouldn't be any issue setting it to false after setting the parameter
                this.IsAutomaticParameter.SetValue(obj, false);
                parameter.SetValue(obj, value);
            }

            this.OnValueChanged(false, true);
        }
    }

    /// <summary>
    /// Gets the parameter used to communicate the automatic state. When this parameter's value is set to true,
    /// ideally the handlers' value parameter should be re-calculated without this slot having to do anything else.
    /// </summary>
    public DataParameterBool IsAutomaticParameter { get; }

    public new DataParameterNumber<T> Parameter => Unsafe.As<DataParameterNumber<T>>(base.Parameter);

    public DragStepProfile StepProfile {
        get => this.stepProfile;
        set {
            this.stepProfile = value;
            this.StepProfileChanged?.Invoke(this);
        }
    }

    public event DataParameterPropertyEditorSlotEventHandler? StepProfileChanged;
    
    public AutomaticNumericDataParameterPropertyEditorSlot(DataParameter<T> parameter, DataParameterBool isAutomaticParameter, Type applicableType, string displayName, DragStepProfile stepProfile) : base(parameter, applicableType, displayName) {
        this.StepProfile = stepProfile;
        this.IsAutomaticParameter = isAutomaticParameter;
        this.myLocalValue = parameter.DefaultValue;
    }

    public override void QueryValueFromHandlers() {
        this.HasMultipleValues = !CollectionUtils.GetEqualValue(this.Handlers, (x) => this.Parameter.GetValue((ITransferableData) x), out this.myLocalValue);
        DataParameterNumber<T> p;
        if (this.HasMultipleValues && (p = this.Parameter).HasExplicitRangeLimit) {
            this.myLocalValue = T.Abs(p.Maximum - p.Minimum) / TWO;
        }
    }
}