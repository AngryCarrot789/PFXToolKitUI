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

using PFXToolKitUI.Utils;
using PFXToolKitUI.Utils.Accessing;

namespace PFXToolKitUI.DataTransfer;

public sealed class DataParameterBool : DataParameter<bool> {
    public DataParameterBool(Type ownerType, string name, ValueAccessor<bool> accessor) : this(ownerType, name, false, accessor) {
    }

    public DataParameterBool(Type ownerType, string name, bool defValue, ValueAccessor<bool> accessor) : base(ownerType, name, defValue, accessor) {
    }

    public override void SetValue(ITransferableData owner, bool value) {
        // Allow optimised boxing of boolean
        if (this.isObjectAccessPreferred) {
            base.SetObjectValue(owner, value.Box());
        }
        else {
            base.SetValue(owner, value);
        }
    }
}