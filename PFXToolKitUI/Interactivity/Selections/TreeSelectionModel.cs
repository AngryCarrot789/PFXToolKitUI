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

namespace PFXToolKitUI.Interactivity.Selections;

/// <summary>
/// Manages a hierarchy of selected indices
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class TreeSelectionModel<T> where T : class {
    // TODO
    
    
    private readonly Dictionary<T, IntRangeUnion> mySelectionMap = new Dictionary<T, IntRangeUnion>();
    
    public void Select(T item, int index) {
        if (!this.mySelectionMap.TryGetValue(item, out IntRangeUnion? union))
            this.mySelectionMap[item] = union = new IntRangeUnion();
        
        union.Add(index);
    }
}