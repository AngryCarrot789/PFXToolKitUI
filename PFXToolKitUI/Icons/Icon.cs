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

using System.Runtime.CompilerServices;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.Icons;

/// <summary>
/// A registered icon.
/// </summary>
public abstract class Icon {
    public string Name { get; }

    protected Icon(string name) {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        this.Name = name;
    }

    public sealed override bool Equals(object? obj) => ReferenceEquals(this, obj);

    public sealed override int GetHashCode() => RuntimeHelpers.GetHashCode(this);
}