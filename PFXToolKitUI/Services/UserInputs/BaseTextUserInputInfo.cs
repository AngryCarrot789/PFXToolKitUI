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

using PFXToolKitUI.DataTransfer;
using PFXToolKitUI.Utils.Accessing;

namespace PFXToolKitUI.Services.UserInputs;

public abstract class BaseTextUserInputInfo : UserInputInfo {
    public static readonly DataParameterString FooterParameter = DataParameter.Register(new DataParameterString(typeof(BaseTextUserInputInfo), nameof(Footer), null, ValueAccessors.Reflective<string?>(typeof(BaseTextUserInputInfo), nameof(footer))));

    private string? footer;

    public string? Footer {
        get => this.footer;
        set => DataParameter.SetValueHelper(this, FooterParameter, ref this.footer, value);
    }

    protected BaseTextUserInputInfo() {
    }

    protected BaseTextUserInputInfo(string? caption, string? message) : base(caption, message) {
    }
}