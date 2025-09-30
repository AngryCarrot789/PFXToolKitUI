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

using PFXToolKitUI.Avalonia.Services.UserInputs;
using PFXToolKitUI.Interactivity.Windowing;
using PFXToolKitUI.Services.InputStrokes;
using PFXToolKitUI.Shortcuts.Inputs;

namespace PFXToolKitUI.Avalonia.Shortcuts.Dialogs;

public class InputStrokeQueryDialogImpl : IInputStrokeQueryService {
    public async Task<KeyStroke?> GetKeyStrokeInput(KeyStroke? initialKeyStroke, ITopLevel? parentTopLevel = null) {
        KeyStrokeUserInputInfo info = new KeyStrokeUserInputInfo() {
            KeyStroke = initialKeyStroke, Caption = "Key Input Stroke"
        };

        Task<bool?> task = parentTopLevel != null 
            ? UserInputDialogView.ShowDialogWindowOrPopup(info, parentTopLevel) 
            : UserInputDialogView.ShowDialogWindowOrPopup(info);
        
        return await task == true ? info.KeyStroke : null;
    }

    public async Task<MouseStroke?> GetMouseStroke(MouseStroke? initialMouseStroke, ITopLevel? parentTopLevel = null) {
        MouseStrokeUserInputInfo info = new MouseStrokeUserInputInfo() {
            MouseStroke = initialMouseStroke, Caption = "Mouse Input Stroke"
        };

        Task<bool?> task = parentTopLevel != null 
            ? UserInputDialogView.ShowDialogWindowOrPopup(info, parentTopLevel) 
            : UserInputDialogView.ShowDialogWindowOrPopup(info);
        
        return await task == true ? info.MouseStroke : null;
    }
}