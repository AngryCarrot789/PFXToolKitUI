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
/// Represents a set of integers
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IIntegerSet<T> : IReadOnlyIntegerSet<T> where T : unmanaged, IBinaryInteger<T>, IMinMaxValue<T> {
    /// <summary>
    /// Adds a single integer to this set
    /// </summary>
    void Add(T range);
    
    /// <summary>
    /// Adds a range of integers to this set
    /// </summary>
    void Add(IntegerRange<T> range);
    
    /// <summary>
    /// Clears all ranges from this set
    /// </summary>
    void Clear();

    /// <summary>
    /// Returns true when this set contains the single location
    /// </summary>
    /// <param name="location"></param>
    /// <returns></returns>
    bool Contains(T location);

    /// <summary>
    /// Returns true when this set contains the entirety of the given range
    /// </summary>
    bool Contains(IntegerRange<T> range);

    /// <summary>
    /// Returns true when the range, in any way, overlaps this set 
    /// </summary>
    bool Overlaps(IntegerRange<T> range);

    int GetOverlappingRanges(IntegerRange<T> range, Span<IntegerRange<T>> output);
    
    int GetIntersectingRanges(IntegerRange<T> range, Span<IntegerRange<T>> output);
    
    IntegerSet<T> GetPresenceUnion(IntegerRange<T> range, bool complement);
    
    void GetPresenceUnion(IntegerSet<T> dstSet, IntegerRange<T> range, bool complement);
    
    void CopyTo(IntegerRange<T>[] array, int arrayIndex);

    /// <summary>
    /// Removes a single integer from this set
    /// </summary>
    bool Remove(T value);
    
    /// <summary>
    /// Removes a range of integers from this set
    /// </summary>
    bool Remove(IntegerRange<T> range);

    /// <summary>
    /// Creates a new list with the contents of <see cref="IntegerSet{T}.Ranges"/>
    /// </summary>
    /// <returns>A new list with our ranges</returns>
    List<IntegerRange<T>> ToList();

    /// <summary>
    /// Creates a clone of this set
    /// </summary>
    /// <returns>A new set</returns>
    IIntegerSet<T> Clone();
}