﻿// 
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

using System.Collections.Immutable;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using PFXToolKitUI.Avalonia.Bindings;
using PFXToolKitUI.Avalonia.Services.UserInputs;
using PFXToolKitUI.DataTransfer;
using PFXToolKitUI.Services.UserInputs;

namespace PFXToolKitUI.Avalonia.Services.Messages.Controls;

public partial class SingleUserInputControl : UserControl, IUserInputContent {
    private readonly AvaloniaPropertyToDataParameterBinder<SingleUserInputInfo> labelBinder = new AvaloniaPropertyToDataParameterBinder<SingleUserInputInfo>(TextBlock.TextProperty, SingleUserInputInfo.LabelParameter);
    private readonly AvaloniaPropertyToDataParameterBinder<SingleUserInputInfo> textBinder = new AvaloniaPropertyToDataParameterBinder<SingleUserInputInfo>(TextBox.TextProperty, SingleUserInputInfo.TextParameter);
    private readonly AvaloniaPropertyToDataParameterBinder<SingleUserInputInfo> footerBinder = new AvaloniaPropertyToDataParameterBinder<SingleUserInputInfo>(TextBlock.TextProperty, BaseTextUserInputInfo.FooterParameter);
    private UserInputDialogView? myDialog;
    private SingleUserInputInfo? myData;

    public SingleUserInputControl() {
        this.InitializeComponent();
        this.labelBinder.AttachControl(this.PART_Label);
        this.textBinder.AttachControl(this.PART_TextBox);
        this.footerBinder.AttachControl(this.PART_FooterTextBlock);

        this.PART_TextBox.KeyDown += this.OnTextFieldKeyDown;
    }

    private void OnTextFieldKeyDown(object? sender, KeyEventArgs e) {
        if ((e.Key == Key.Escape || e.Key == Key.Enter) && this.myDialog != null) {
            this.myDialog.TryCloseDialog(e.Key != Key.Escape);
        }
    }

    public void Connect(UserInputDialogView dialog, UserInputInfo info) {
        this.myDialog = dialog;
        this.myData = (SingleUserInputInfo) info;
        this.labelBinder.AttachModel(this.myData);
        this.textBinder.AttachModel(this.myData);
        this.footerBinder.AttachModel(this.myData);
        SingleUserInputInfo.LabelParameter.AddValueChangedHandler(info, this.OnLabelChanged);
        BaseTextUserInputInfo.FooterParameter.AddValueChangedHandler(this.myData!, this.OnFooterChanged);
        this.myData.TextErrorsChanged += this.UpdateTextErrors;
        this.UpdateLabelVisibility();
        this.UpdateFooterVisibility();
        this.UpdateTextErrors(this.myData);
    }

    public void Disconnect() {
        this.labelBinder.DetachModel();
        this.textBinder.DetachModel();
        this.footerBinder.DetachModel();
        SingleUserInputInfo.LabelParameter.RemoveValueChangedHandler(this.myData!, this.OnLabelChanged);
        BaseTextUserInputInfo.FooterParameter.RemoveValueChangedHandler(this.myData!, this.OnFooterChanged);
        this.myData!.TextErrorsChanged -= this.UpdateTextErrors;
        this.myDialog = null;
        this.myData = null;
    }

    public static void SetErrorsOrClear(AvaloniaObject target, IImmutableList<string>? errors) {
        target.SetValue(DataValidationErrors.ErrorsProperty, errors?.ToList() ?? AvaloniaProperty.UnsetValue);
    }

    private void UpdateTextErrors(SingleUserInputInfo info) {
        SetErrorsOrClear(this.PART_TextBox, info.TextErrors);
    }

    public bool FocusPrimaryInput() {
        this.PART_TextBox.Focus();
        this.PART_TextBox.SelectAll();
        return true;
    }

    public void OnWindowOpened() {
    }

    public void OnWindowClosed() {
    }

    private void UpdateLabelVisibility() => this.PART_Label.IsVisible = !string.IsNullOrWhiteSpace(this.myData!.Label);
    private void UpdateFooterVisibility() => this.PART_FooterTextBlock.IsVisible = !string.IsNullOrWhiteSpace(this.myData!.Footer);

    private void OnLabelChanged(DataParameter parameter, ITransferableData owner) => this.UpdateLabelVisibility();
    private void OnFooterChanged(DataParameter dataParameter, ITransferableData owner) => this.UpdateFooterVisibility();
}