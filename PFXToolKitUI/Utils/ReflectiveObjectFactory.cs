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

namespace PFXToolKitUI.Utils;

/// <summary>
/// An object factory that allows reflective creation of objects that use a default constructor
/// </summary>
public class ReflectiveObjectFactory<T> : ObjectFactory where T : class {
    public ReflectiveObjectFactory() {
    }

    protected override bool IsTypeValid(Type type) {
        return typeof(T).IsAssignableFrom(type);
    }

    protected T NewInstance(string id) {
        Type type = this.GetType(id);
        return (T) Activator.CreateInstance(type)!;
    }
}