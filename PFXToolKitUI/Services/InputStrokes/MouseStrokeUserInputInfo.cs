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

using PFXToolKitUI.DataTransfer;
using PFXToolKitUI.Services.UserInputs;
using PFXToolKitUI.Shortcuts.Inputs;
using PFXToolKitUI.Utils.Accessing;

namespace PFXToolKitUI.Services.InputStrokes;

public class MouseStrokeUserInputInfo : UserInputInfo {
    public static readonly DataParameter<MouseStroke?> MouseStrokeParameter =
        DataParameter.Register(
            new DataParameter<MouseStroke?>(
                typeof(MouseStrokeUserInputInfo),
                nameof(MouseStroke), default(MouseStroke?),
                ValueAccessors.Reflective<MouseStroke?>(typeof(MouseStrokeUserInputInfo), nameof(mouseStroke))));

    private MouseStroke? mouseStroke;

    public MouseStroke? MouseStroke {
        get => this.mouseStroke;
        set => DataParameter.SetValueHelper(this, MouseStrokeParameter, ref this.mouseStroke, value);
    }

    public MouseStrokeUserInputInfo() {
        this.mouseStroke = MouseStrokeParameter.GetDefaultValue(this);
    }

    public override bool HasErrors() {
        return !this.mouseStroke.HasValue;
    }

    public override void UpdateAllErrors() {
    }
}