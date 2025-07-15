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

using System.Diagnostics;

namespace PFXToolKitUI.Utils.Events;

public class AutoValueHelper<T> where T : class {
    private readonly Action<T> update;
    private readonly Action<T, EventHandler> addHandler;
    private readonly Action<T, EventHandler> removeHandler;
    private T? value;

    /// <summary>
    /// Gets or sets the value
    /// </summary>
    public T? Value {
        get => this.value;
        set {
            if (!ReferenceEquals(this.value, value)) {
                if (this.value != null) {
                    this.removeHandler(this.value, this.OnEvent);
                }

                if ((this.value = value) != null) {
                    this.addHandler(value!, this.OnEvent);
                    this.update(value!);
                }
            }
        }
    }

    public AutoValueHelper(Action<T> update, Action<T, EventHandler> addHandler, Action<T, EventHandler> removeHandler) {
        this.update = update;
        this.addHandler = addHandler;
        this.removeHandler = removeHandler;
    }

    public void OnEvent(object? sender, EventArgs args) {
        Debug.Assert(sender == this.value && this.value != null);
        this.update(this.value!);
    }
}