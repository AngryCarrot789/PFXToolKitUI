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

using Avalonia.Controls;
using Avalonia.Input;
using PFXToolKitUI.Avalonia.Bindings;
using PFXToolKitUI.Avalonia.Services.UserInputs;
using PFXToolKitUI.Avalonia.Shortcuts.Avalonia;
using PFXToolKitUI.Avalonia.Shortcuts.Converters;
using PFXToolKitUI.Services.InputStrokes;
using PFXToolKitUI.Services.UserInputs;
using PFXToolKitUI.Shortcuts.Inputs;

namespace PFXToolKitUI.Avalonia.Shortcuts.Dialogs;

public partial class MouseStrokeUserInputControl : UserControl, IUserInputContent {
    public MouseStrokeUserInputInfo? InputInfo { get; private set; }

    private readonly IBinder<MouseStrokeUserInputInfo> mouseStrokeBinder = new AvaloniaPropertyToDataParameterAutoBinder<MouseStrokeUserInputInfo>(TextBox.TextProperty, MouseStrokeUserInputInfo.MouseStrokeParameter, (p) => {
        MouseStroke s = (MouseStroke?) p ?? default;
        return MouseStrokeStringConverter.ToStringFunction(s.MouseButton, s.Modifiers, s.ClickCount);
    });

    public MouseStrokeUserInputControl() {
        this.InitializeComponent();
        this.mouseStrokeBinder.AttachControl(this.InputBox);
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e) {
        MouseStroke stroke = ShortcutUtils.GetMouseStrokeForEvent(e);
        this.InputInfo!.MouseStroke = stroke;
    }

    private void InputElement_OnPointerWheelChanged(object? sender, PointerWheelEventArgs e) {
        if (ShortcutUtils.GetMouseStrokeForEvent(e, out MouseStroke stroke)) {
            this.InputInfo!.MouseStroke = stroke;
        }
    }

    public void Connect(UserInputDialogView dialog, UserInputInfo info) {
        this.InputInfo = (MouseStrokeUserInputInfo) info;
        this.mouseStrokeBinder.AttachModel(this.InputInfo);
    }

    public void Disconnect() {
        this.InputInfo = null;
        this.mouseStrokeBinder.DetachModel();
    }

    public bool FocusPrimaryInput() {
        this.InputBox.Focus();
        return true;
    }

    public void OnWindowOpened() {
    }

    public void OnWindowClosed() {
    }
}