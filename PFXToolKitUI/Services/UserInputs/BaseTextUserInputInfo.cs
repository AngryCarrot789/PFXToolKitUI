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

using PFXToolKitUI.Utils;

namespace PFXToolKitUI.Services.UserInputs;

public delegate void BaseTextUserInputInfoEventHandler(BaseTextUserInputInfo sender);

public abstract class BaseTextUserInputInfo : UserInputInfo {
    private string? footer;

    public string? Footer {
        get => this.footer;
        set => PropertyHelper.SetAndRaiseINE(ref this.footer, value, this, static t => t.FooterChanged?.Invoke(t));
    }

    public event BaseTextUserInputInfoEventHandler? FooterChanged;

    protected BaseTextUserInputInfo() {
    }

    protected BaseTextUserInputInfo(string? caption, string? message) : base(caption, message) {
    }
}