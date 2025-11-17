// 
// Copyright (c) 2025-2025 REghZy
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

using System.Numerics;

namespace PFXToolKitUI.Utils.Ranges;

/// <summary>
/// A read-only implementation of an integer set
/// </summary>
public interface IReadOnlyIntegerSet<T> : IEnumerable<IntegerRange<T>> where T : unmanaged, IBinaryInteger<T>, IMinMaxValue<T> {
    /// <summary>
    /// Gets the read-only list of ranges this union currently stores
    /// </summary>
    IReadOnlyList<IntegerRange<T>> Ranges { get; }

    /// <summary>
    /// Returns true when <see cref="Ranges"/> contains more than one range
    /// </summary>
    bool IsFragmented => this.Ranges.Count > 1;

    /// <summary>
    /// Gets the enclosing range of this union. This will contain the start index of the first range and span to the the end index of the last range
    /// </summary>
    IntegerRange<T> EnclosingRange { get; }

    /// <summary>
    /// Sums the value of <see cref="IntegerRange{T}.Length"/> for each range in <see cref="Ranges"/>
    /// </summary>
    T GrandTotal { get; }
}