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

using System.Collections.ObjectModel;

namespace PFXToolKitUI.Utils.Collections;

/// <summary>
/// An extension to
/// </summary>
/// <typeparam name="T"></typeparam>
public class CollectionEx<T> : Collection<T> {
    public CollectionEx() {
    }

    public CollectionEx(IList<T> list) : base(list) {
    }

    public int FindIndex(Predicate<T> match) {
        for (int i = 0; i < this.Items.Count; i++) {
            if (match(this.Items[i])) {
                return i;
            }
        }

        return -1;
    }
    
    public int FindIndex<TState>(TState state, Func<T, TState, bool> match) {
        for (int i = 0; i < this.Items.Count; i++) {
            if (match(this.Items[i], state)) {
                return i;
            }
        }

        return -1;
    }
}