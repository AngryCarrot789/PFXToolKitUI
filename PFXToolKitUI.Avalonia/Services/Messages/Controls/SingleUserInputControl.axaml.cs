﻿// 
// Copyright (c) 2023-2025 REghZy
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

using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using PFXToolKitUI.Avalonia.Bindings;
using PFXToolKitUI.Avalonia.Services.UserInputs;
using PFXToolKitUI.Services.UserInputs;

namespace PFXToolKitUI.Avalonia.Services.Messages.Controls;

public partial class SingleUserInputControl : UserControl, IUserInputContent {
    private readonly IBinder<SingleUserInputInfo> labelBinder = new EventUpdateBinder<SingleUserInputInfo>(nameof(SingleUserInputInfo.LabelChanged), b => b.Control.SetValue(TextBlock.TextProperty, b.Model.Label));
    private readonly IBinder<SingleUserInputInfo> textBinder = new AvaloniaPropertyToEventPropertyBinder<SingleUserInputInfo>(TextBox.TextProperty, nameof(SingleUserInputInfo.TextChanged), b => b.Control.SetValue(TextBox.TextProperty, b.Model.Text), b => b.Model.Text = b.Control.GetValue(TextBox.TextProperty) ?? "");

    private readonly IBinder<SingleUserInputInfo> linesBinder = new EventUpdateBinder<SingleUserInputInfo>(nameof(SingleUserInputInfo.LineCountHintChanged), b => {
        b.Control.SetValue(TextBox.MinLinesProperty, b.Model.LineCountHint);
        b.Control.SetValue(TextBox.MaxLinesProperty, b.Model.LineCountHint);
    });

    private readonly IBinder<SingleUserInputInfo> footerBinder = new EventUpdateBinder<SingleUserInputInfo>(nameof(BaseTextUserInputInfo.FooterChanged), b => b.Control.SetValue(TextBlock.TextProperty, b.Model.Footer));
    private UserInputDialogView? myDialog;
    private SingleUserInputInfo? myData;

    public SingleUserInputControl() {
        this.InitializeComponent();
        this.labelBinder.AttachControl(this.PART_Label);
        this.textBinder.AttachControl(this.PART_TextBox);
        this.linesBinder.AttachControl(this.PART_TextBox);
        this.footerBinder.AttachControl(this.PART_FooterTextBlock);

        this.PART_TextBox.KeyDown += this.OnTextFieldKeyDown;
    }

    private void OnTextFieldKeyDown(object? sender, KeyEventArgs e) {
        if ((e.Key == Key.Escape || e.Key == Key.Enter) && this.myDialog != null) {
            if (e.Key != Key.Escape && this.myData!.LineCountHint > 1)
                return; // do not auto-close when multi-line since that's just annoying and unusable

            this.myDialog.TryCloseDialog(e.Key != Key.Escape);
        }
    }

    public void Connect(UserInputDialogView dialog, UserInputInfo info) {
        this.myDialog = dialog;
        this.myData = (SingleUserInputInfo) info;
        this.labelBinder.AttachModel(this.myData);
        this.textBinder.AttachModel(this.myData);
        this.linesBinder.AttachModel(this.myData);
        this.footerBinder.AttachModel(this.myData);
        this.PART_TextBox.AcceptsReturn = this.myData.LineCountHint > 1;
        this.myData.LabelChanged += this.OnLabelChanged;
        this.myData.FooterChanged += this.OnFooterChanged;
        this.myData.TextErrorsChanged += this.UpdateTextErrors;
        this.UpdateLabelVisibility();
        this.UpdateFooterVisibility();
        this.UpdateTextErrors(this.myData);
    }

    public void Disconnect() {
        this.labelBinder.DetachModel();
        this.textBinder.DetachModel();
        this.linesBinder.DetachModel();
        this.footerBinder.DetachModel();
        this.myData!.LabelChanged -= this.OnLabelChanged;
        this.myData!.FooterChanged -= this.OnFooterChanged;
        this.myData!.TextErrorsChanged -= this.UpdateTextErrors;
        this.myDialog = null;
        this.myData = null;
    }

    public static void SetErrorsOrClear(AvaloniaObject target, ReadOnlyCollection<string>? errors) {
        target.SetValue(DataValidationErrors.ErrorsProperty, errors ?? AvaloniaProperty.UnsetValue);
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

    public PixelSize GetMinimumBounds() {
        int minWidth = this.myData!.MinimumDialogWidthHint;
        if (minWidth >= 0) {
            return new PixelSize(minWidth, 0);
        }
        
        return PixelSize.Empty;
    }

    public void OnWindowClosed() {
    }

    private void UpdateLabelVisibility() => this.PART_Label.IsVisible = !string.IsNullOrWhiteSpace(this.myData!.Label);
    private void UpdateFooterVisibility() => this.PART_FooterTextBlock.IsVisible = !string.IsNullOrWhiteSpace(this.myData!.Footer);

    private void OnLabelChanged(SingleUserInputInfo sender) => this.UpdateLabelVisibility();
    private void OnFooterChanged(BaseTextUserInputInfo sender) => this.UpdateFooterVisibility();
}