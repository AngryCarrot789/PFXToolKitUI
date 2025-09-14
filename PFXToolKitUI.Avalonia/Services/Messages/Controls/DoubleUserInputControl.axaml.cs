// 
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

using Avalonia.Controls;
using Avalonia.Input;
using PFXToolKitUI.Avalonia.Bindings;
using PFXToolKitUI.Avalonia.Services.UserInputs;
using PFXToolKitUI.Services.UserInputs;

namespace PFXToolKitUI.Avalonia.Services.Messages.Controls;

public partial class DoubleUserInputControl : UserControl, IUserInputContent {
    private readonly IBinder<DoubleUserInputInfo> labelABinder = new EventUpdateBinder<DoubleUserInputInfo>(nameof(DoubleUserInputInfo.LabelAChanged), b => b.Control.SetValue(TextBlock.TextProperty, b.Model.LabelA));
    private readonly IBinder<DoubleUserInputInfo> labelBBinder = new EventUpdateBinder<DoubleUserInputInfo>(nameof(DoubleUserInputInfo.LabelBChanged), b => b.Control.SetValue(TextBlock.TextProperty, b.Model.LabelB));
    private readonly IBinder<DoubleUserInputInfo> textABinder = new AvaloniaPropertyToEventPropertyBinder<DoubleUserInputInfo>(TextBox.TextProperty, nameof(DoubleUserInputInfo.TextAChanged), b => b.Control.SetValue(TextBox.TextProperty, b.Model.TextA), b => b.Model.TextA = b.Control.GetValue(TextBox.TextProperty) ?? "");
    private readonly IBinder<DoubleUserInputInfo> textBBinder = new AvaloniaPropertyToEventPropertyBinder<DoubleUserInputInfo>(TextBox.TextProperty, nameof(DoubleUserInputInfo.TextBChanged), b => b.Control.SetValue(TextBox.TextProperty, b.Model.TextB), b => b.Model.TextB = b.Control.GetValue(TextBox.TextProperty) ?? "");

    private readonly IBinder<DoubleUserInputInfo> linesABinder = new EventUpdateBinder<DoubleUserInputInfo>(nameof(DoubleUserInputInfo.LineCountHintAChanged), b => {
        b.Control.SetValue(TextBox.MinLinesProperty, b.Model.LineCountHintA);
        b.Control.SetValue(TextBox.MaxLinesProperty, b.Model.LineCountHintA);
    });

    private readonly IBinder<DoubleUserInputInfo> linesBBinder = new EventUpdateBinder<DoubleUserInputInfo>(nameof(DoubleUserInputInfo.LineCountHintBChanged), b => {
        b.Control.SetValue(TextBox.MinLinesProperty, b.Model.LineCountHintB);
        b.Control.SetValue(TextBox.MaxLinesProperty, b.Model.LineCountHintB);
    });

    private readonly IBinder<DoubleUserInputInfo> footerBinder = new EventUpdateBinder<DoubleUserInputInfo>(nameof(BaseTextUserInputInfo.FooterChanged), b => b.Control.SetValue(TextBlock.TextProperty, b.Model.Footer));
    private UserInputDialogView? myDialog;
    private DoubleUserInputInfo? myData;

    public DoubleUserInputControl() {
        this.InitializeComponent();
        this.labelABinder.AttachControl(this.PART_LabelA);
        this.labelBBinder.AttachControl(this.PART_LabelB);
        this.textABinder.AttachControl(this.PART_TextBoxA);
        this.footerBinder.AttachControl(this.PART_FooterTextBlock);
        Binders.AttachControls(this.PART_TextBoxB, this.textBBinder, this.linesABinder, this.linesBBinder);

        this.PART_TextBoxA.KeyDown += this.OnAnyTextFieldKeyDown;
        this.PART_TextBoxB.KeyDown += this.OnAnyTextFieldKeyDown;
    }

    private void OnAnyTextFieldKeyDown(object? sender, KeyEventArgs e) {
        if ((e.Key == Key.Escape || e.Key == Key.Enter) && this.myDialog != null) {
            this.myDialog.TryCloseDialog(e.Key != Key.Escape);
        }
    }

    public void Connect(UserInputDialogView dialog, UserInputInfo info) {
        this.myDialog = dialog;
        this.myData = (DoubleUserInputInfo) info;
        Binders.AttachModels(this.myData, this.labelABinder, this.labelBBinder, this.textABinder, this.textBBinder, this.linesABinder, this.linesBBinder, this.footerBinder);
        this.myData.LabelAChanged += this.OnLabelAChanged;
        this.myData.LabelBChanged += this.OnLabelBChanged;
        this.myData.FooterChanged += this.OnFooterChanged;
        this.myData.TextErrorsAChanged += this.UpdateTextErrorsA;
        this.myData.TextErrorsBChanged += this.UpdateTextErrorsB;
        this.UpdateLabelAVisibility();
        this.UpdateLabelBVisibility();
        this.UpdateFooterVisibility();
        this.UpdateTextErrorsA(this.myData);
        this.UpdateTextErrorsB(this.myData);
    }

    public void Disconnect() {
        Binders.DetachModels(this.labelABinder, this.labelBBinder, this.textABinder, this.textBBinder, this.linesABinder, this.linesBBinder, this.footerBinder);
        this.myData!.LabelAChanged -= this.OnLabelAChanged;
        this.myData!.LabelBChanged -= this.OnLabelBChanged;
        this.myData!.FooterChanged -= this.OnFooterChanged;
        this.myData!.TextErrorsAChanged -= this.UpdateTextErrorsA;
        this.myData!.TextErrorsBChanged -= this.UpdateTextErrorsB;
        this.myDialog = null;
        this.myData = null;
    }

    private void UpdateTextErrorsA(DoubleUserInputInfo info) {
        SingleUserInputControl.SetErrorsOrClear(this.PART_TextBoxA, info.TextErrorsA);
    }

    private void UpdateTextErrorsB(DoubleUserInputInfo info) {
        SingleUserInputControl.SetErrorsOrClear(this.PART_TextBoxB, info.TextErrorsB);
    }

    public bool FocusPrimaryInput() {
        this.PART_TextBoxA.Focus();
        this.PART_TextBoxA.SelectAll();
        return true;
    }

    private void UpdateLabelAVisibility() => this.PART_LabelA.IsVisible = !string.IsNullOrWhiteSpace(this.myData!.LabelA);
    private void UpdateLabelBVisibility() => this.PART_LabelA.IsVisible = !string.IsNullOrWhiteSpace(this.myData!.LabelA);
    private void UpdateFooterVisibility() => this.PART_FooterTextBlock.IsVisible = !string.IsNullOrWhiteSpace(this.myData!.Footer);

    private void OnLabelAChanged(DoubleUserInputInfo sender) => this.UpdateLabelAVisibility();
    private void OnLabelBChanged(DoubleUserInputInfo sender) => this.UpdateLabelBVisibility();
    private void OnFooterChanged(BaseTextUserInputInfo sender) => this.UpdateFooterVisibility();
}