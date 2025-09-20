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

using PFXToolKitUI.Avalonia.Interactivity.Windowing;
using PFXToolKitUI.Avalonia.Services.UserInputs;
using PFXToolKitUI.Interactivity.Windowing;
using PFXToolKitUI.Services.ColourPicking;
using SkiaSharp;

namespace PFXToolKitUI.Avalonia.Services.Colours;

public class ColourPickerDialogServiceImpl : IColourPickerDialogService {
    public async Task<SKColor?> PickColourAsync(SKColor? defaultColour, ITopLevel? parentTopLevel = null) {
        ColourUserInputInfo info = new ColourUserInputInfo() {
            Colour = defaultColour ?? SKColors.Black
        };

        Task<bool?> task = parentTopLevel != null
            ? UserInputDialogView.ShowDialogAsync(info, IWindow.FromTopLevel(parentTopLevel))
            : UserInputDialogView.ShowDialogAsync(info);

        return await task == true ? info.Colour : null;
    }
}