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
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using PFXToolKitUI.Avalonia.AvControls.Dragger;
using PFXToolKitUI.Avalonia.Bindings;
using PFXToolKitUI.Avalonia.Utils;
using PFXToolKitUI.DataTransfer;
using PFXToolKitUI.PropertyEditing.DataTransfer;
using PFXToolKitUI.PropertyEditing.DataTransfer.Automatic;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.Avalonia.PropertyEditing.DataTransfer.Automatic;

public class AutomaticDataParameterVector2PropertyEditorSlotControl : BaseDataParameterPropertyEditorSlotControl {
    internal static readonly IImmutableBrush MultipleValuesBrush = BaseNumberDraggerDataParamPropEditorSlotControl.MultipleValuesBrush;

    public new AutomaticDataParameterVector2PropertyEditorSlot? SlotModel => (AutomaticDataParameterVector2PropertyEditorSlot?) base.SlotControl?.Model;

    protected NumberDragger draggerX;
    protected NumberDragger draggerY;
    protected Button resetButton;

    private readonly IBinder<DataParameterFormattablePropertyEditorSlot> valueFormatterBinder;

    public AutomaticDataParameterVector2PropertyEditorSlotControl() {
        this.valueFormatterBinder = new EventUpdateBinder<DataParameterFormattablePropertyEditorSlot>(nameof(DataParameterFormattablePropertyEditorSlot.ValueFormatterChanged), (x) => {
            AutomaticDataParameterVector2PropertyEditorSlotControl editor = (AutomaticDataParameterVector2PropertyEditorSlotControl) x.Control;
            editor.draggerX.ValueFormatter = x.Model.ValueFormatter;
            editor.draggerY.ValueFormatter = x.Model.ValueFormatter;
        });
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.draggerX = e.NameScope.GetTemplateChild<NumberDragger>("PART_DraggerX");
        this.draggerY = e.NameScope.GetTemplateChild<NumberDragger>("PART_DraggerY");
        this.resetButton = e.NameScope.GetTemplateChild<Button>("PART_ResetButton");
        this.draggerX.ValueChanged += (sender, args) => this.OnControlValueChanged();
        this.draggerY.ValueChanged += (sender, args) => this.OnControlValueChanged();
        this.resetButton.Click += this.ResetButtonOnClick;
        this.valueFormatterBinder.AttachControl(this);
        this.UpdateDraggerMultiValueState();

        this.draggerX.InvalidInputEntered += this.PartDraggerOnInvalidInputEntered;
        this.draggerY.InvalidInputEntered += this.PartDraggerOnInvalidInputEntered;
    }

    private void PartDraggerOnInvalidInputEntered(object? sender, InvalidInputEnteredEventArgs e) {
        AutomaticDataParameterVector2PropertyEditorSlot? model = this.SlotModel;
        if (model == null || !model.IsCurrentlyApplicable) {
            return;
        }

        if (("auto".EqualsIgnoreCase(e.Input) || "automatic".EqualsIgnoreCase(e.Input) || "\"auto\"".EqualsIgnoreCase(e.Input))) {
            foreach (object handler in model.Handlers) {
                model.IsAutomaticParameter.SetValue((ITransferableData) handler, true);
            }
        }
    }

    private void ResetButtonOnClick(object? sender, RoutedEventArgs e) {
        AutomaticDataParameterVector2PropertyEditorSlot? slot = this.SlotModel;
        if (slot != null && slot.HasHandlers) {
            foreach (ITransferableData handler in slot.Handlers) {
                slot.IsAutomaticParameter.SetValue(handler, true);
            }
        }
    }

    private void UpdateDraggerMultiValueState() {
        if (!this.IsConnected) {
            return;
        }

        bool flag = this.SlotModel!.HasMultipleValues, flag2 = this.SlotModel!.HasProcessedMultipleValuesSinceSetup;
        BaseNumberDraggerDataParamPropEditorSlotControl.UpdateNumberDragger(this.draggerX, flag, flag2);
        BaseNumberDraggerDataParamPropEditorSlotControl.UpdateNumberDragger(this.draggerY, flag, flag2);
    }

    protected override void OnCanEditValueChanged(bool canEdit) {
        this.draggerX.IsEnabled = canEdit;
        this.draggerY.IsEnabled = canEdit;
    }

    protected override void OnConnected() {
        this.valueFormatterBinder.AttachModel(this.SlotModel!);
        base.OnConnected();

        AutomaticDataParameterVector2PropertyEditorSlot slot = this.SlotModel!;
        DataParameterVector2 param = slot.Parameter;
        this.draggerX.Minimum = param.Minimum.X;
        this.draggerY.Minimum = param.Minimum.Y;
        this.draggerX.Maximum = param.Maximum.X;
        this.draggerY.Maximum = param.Maximum.Y;

        DragStepProfile profile = slot.StepProfile;
        this.draggerX.TinyChange = profile.TinyStep;
        this.draggerX.SmallChange = profile.SmallStep;
        this.draggerX.NormalChange = profile.NormalStep;
        this.draggerX.LargeChange = profile.LargeStep;

        this.draggerY.TinyChange = profile.TinyStep;
        this.draggerY.SmallChange = profile.SmallStep;
        this.draggerY.NormalChange = profile.NormalStep;
        this.draggerY.LargeChange = profile.LargeStep;

        this.SlotModel!.HasMultipleValuesChanged += this.OnHasMultipleValuesChanged;
        this.UpdateDraggerMultiValueState();
    }

    protected override void OnDisconnected() {
        this.valueFormatterBinder.DetachModel();
        base.OnDisconnected();

        this.SlotModel!.HasMultipleValuesChanged -= this.OnHasMultipleValuesChanged;
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
        if (this.singleHandler != null && this.SlotModel!.IsAutomaticParameter.GetValue(this.singleHandler)) {
            this.draggerX.FinalPreviewStringFormat = this.draggerY.FinalPreviewStringFormat = "{0} (Auto)";
        }
        else {
            this.draggerX.FinalPreviewStringFormat = this.draggerY.FinalPreviewStringFormat = null;
        }
    }

    private void OnHasMultipleValuesChanged(DataParameterPropertyEditorSlot sender) {
        this.UpdateDraggerMultiValueState();
    }

    protected override void UpdateControlValue() {
        Vector2 value = this.SlotModel!.Value;
        this.draggerX.Value = value.X;
        this.draggerY.Value = value.Y;
        this.UpdateTextPreview();
    }

    protected override void UpdateModelValue() {
        this.SlotModel!.Value = new Vector2((float) this.draggerX.Value, (float) this.draggerY.Value);
    }
}