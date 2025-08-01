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

using System.Collections;

namespace PFXToolKitUI.Utils;

public class SingletonEnumerator<T> : IEnumerator<T> {
    private bool hasMovedNext;

    // should this throw if hasMovedNext is false?
    public T Current { get; }

    object IEnumerator.Current => this.Current!;

    public SingletonEnumerator(T value) {
        this.Current = value;
    }

    public bool MoveNext() {
        if (this.hasMovedNext) {
            return false;
        }
        else {
            this.hasMovedNext = true;
            return true;
        }
    }

    public void Reset() {
        this.hasMovedNext = false;
    }

    public void Dispose() { }
}