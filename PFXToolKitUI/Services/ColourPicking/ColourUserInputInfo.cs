// 
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

using PFXToolKitUI.Services.UserInputs;
using PFXToolKitUI.Utils;
using SkiaSharp;

namespace PFXToolKitUI.Services.ColourPicking;

public delegate void ColourUserInputInfoEventHandler(ColourUserInputInfo sender);

public class ColourUserInputInfo : UserInputInfo {
    private SKColor colour = SKColor.Empty;

    public SKColor Colour {
        get => this.colour;
        set => PropertyHelper.SetAndRaiseINE(ref this.colour, value, this, static t => t.ColourChanged?.Invoke(t));
    }

    public event ColourUserInputInfoEventHandler? ColourChanged;

    public ColourUserInputInfo() {
    }

    public override bool HasErrors() {
        return false;
    }

    public override void UpdateAllErrors() {
    }
}