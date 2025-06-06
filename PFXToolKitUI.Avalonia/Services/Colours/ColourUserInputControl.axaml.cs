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

using Avalonia.Controls;
using Avalonia.Media;
using PFXToolKitUI.Avalonia.Bindings;
using PFXToolKitUI.Avalonia.Services.UserInputs;
using PFXToolKitUI.Services.ColourPicking;
using PFXToolKitUI.Services.UserInputs;
using SkiaSharp;

namespace PFXToolKitUI.Avalonia.Services.Colours;

public partial class ColourUserInputControl : UserControl, IUserInputContent {
    private readonly AvaloniaPropertyToDataParameterBinder<ColourUserInputInfo> colourBinder;
    private UserInputDialogView? myDialog;
    private ColourUserInputInfo? myData;

    public ColourUserInputControl() {
        this.InitializeComponent();
        this.colourBinder = new AvaloniaPropertyToDataParameterBinder<ColourUserInputInfo>(ColorView.ColorProperty, ColourUserInputInfo.ColourParameter, arg => {
            SKColor c = (SKColor) arg!;
            return new Color(c.Alpha, c.Red, c.Green, c.Blue);
        }, arg => {
            Color c = (Color) arg!;
            return new SKColor(c.R, c.G, c.B, c.A);
        });

        this.colourBinder.AttachControl(this.PART_ColorView);
    }

    public void Connect(UserInputDialogView dialog, UserInputInfo info) {
        this.myDialog = dialog;
        this.myData = (ColourUserInputInfo) info;
        this.colourBinder.AttachModel(this.myData);
    }

    public void Disconnect() {
        this.colourBinder.DetachModel();
        this.myDialog = null;
        this.myData = null;
    }

    public bool FocusPrimaryInput() {
        return false;
    }

    public void OnWindowOpened() {
    }

    public void OnWindowClosed() {
    }
}