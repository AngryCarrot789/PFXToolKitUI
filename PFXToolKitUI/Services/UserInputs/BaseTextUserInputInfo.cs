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

using PFXToolKitUI.Utils.Events;
using PFXToolKitUI.Utils.Reactive;

namespace PFXToolKitUI.Services.UserInputs;

public abstract class BaseTextUserInputInfo : UserInputInfo {
    public static IEventObservable<SingleUserInputInfo> FooterObservable => field ??= Observable.ForEvent<SingleUserInputInfo>((s, e) => s.FooterChanged += e, (s, e) => s.FooterChanged -= e);
    
    public string? Footer {
        get => field;
        set => PropertyHelper.SetAndRaiseINE(ref field, value, this, this.FooterChanged);
    }

    public event EventHandler? FooterChanged;

    protected BaseTextUserInputInfo() {
    }

    protected BaseTextUserInputInfo(string? caption, string? message) : base(caption, message) {
    }
}