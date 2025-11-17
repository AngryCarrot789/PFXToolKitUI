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
using System.Collections.ObjectModel;
using System.Numerics;

namespace PFXToolKitUI.Utils.Ranges;

/// <summary>
/// A delegate around <see cref="IntegerSet{T}"/> that fires an event when indices are added and removed
/// </summary>
public sealed class ObservableIntegerSet<T> : IObservableIntegerSet<T> where T : unmanaged, IBinaryInteger<T>, IMinMaxValue<T> {
    private readonly IntegerSet<T> myUnion;
    
    public IReadOnlyList<IntegerRange<T>> Ranges => this.myUnion.Ranges;

    public bool IsFragmented => this.myUnion.IsFragmented;
    
    public IntegerRange<T> EnclosingRange => this.myUnion.EnclosingRange;

    public T GrandTotal => this.myUnion.GrandTotal;

    public event IObservableIntegerSet<T>.IndicesChangedEventHandler? IndicesChanged;

    /// <summary>
    /// Creates a new empty union.
    /// </summary>
    public ObservableIntegerSet() {
        this.myUnion = new IntegerSet<T>();
    }

    /// <summary>
    /// Initializes a new union of T ranges.
    /// </summary>
    /// <param name="ranges">The ranges to unify.</param>
    public ObservableIntegerSet(IEnumerable<IntegerRange<T>> ranges) {
        ArgumentNullException.ThrowIfNull(ranges);
        this.myUnion = new IntegerSet<T>(ranges);
    }
    
    public ObservableIntegerSet(IReadOnlyIntegerSet<T> ranges) : this(ranges.Ranges) {
    }

    public void Add(T range) {
        if (range < T.MaxValue)
            this.Add(IntegerRange.FromStartAndEnd(range, range + T.One));
    }

    public void Add(IntegerRange<T> range) {
        if (range.End < range.Start)
            throw new ArgumentOutOfRangeException(nameof(range), "item.End cannot be less than item.Start");
        if (range.Start == range.End)
            return; // we are adding literally nothing

        IntegerSet<T> union_whatIsNotThere = this.myUnion.GetPresenceUnion(range, false);
        this.myUnion.Add(range);
        if (union_whatIsNotThere.Ranges.Count > 0)
            this.IndicesChanged?.Invoke(this, union_whatIsNotThere.Ranges, ReadOnlyCollection<IntegerRange<T>>.Empty);
    }

    public void Clear() {
        IntegerRange<T> range = this.EnclosingRange;
        this.myUnion.Clear();
        this.IndicesChanged?.Invoke(this, [range], ReadOnlyCollection<IntegerRange<T>>.Empty);
    }

    public bool Contains(T location) => this.myUnion.Contains(location);

    public bool Contains(IntegerRange<T> range) => this.myUnion.Contains(range);

    public bool Overlaps(IntegerRange<T> range) => this.myUnion.Overlaps(range);

    public int GetOverlappingRanges(IntegerRange<T> range, Span<IntegerRange<T>> output) => this.myUnion.GetOverlappingRanges(range, output);

    public int GetIntersectingRanges(IntegerRange<T> range, Span<IntegerRange<T>> output) => this.myUnion.GetIntersectingRanges(range, output);
    
    public IntegerSet<T> GetPresenceUnion(IntegerRange<T> range, bool complement) {
        return this.myUnion.GetPresenceUnion(range, complement);
    }

    public void GetPresenceUnion(IntegerSet<T> dstSet, IntegerRange<T> range, bool complement) {
        this.myUnion.GetPresenceUnion(dstSet, range, complement);
    }

    public void CopyTo(IntegerRange<T>[] array, int arrayIndex) {
        this.myUnion.CopyTo(array, arrayIndex);
    }

    public bool Remove(T value) {
        if (value != T.MaxValue)
            return this.Remove(IntegerRange.FromStartAndEnd(value, value + T.One));
        return false;
    }

    public bool Remove(IntegerRange<T> range) {
        IntegerSet<T> union_whatIsThere = this.myUnion.GetPresenceUnion(range, true);
        bool removed = this.myUnion.Remove(range);
        if (union_whatIsThere.Ranges.Count > 0)
            this.IndicesChanged?.Invoke(this, ReadOnlyCollection<IntegerRange<T>>.Empty, union_whatIsThere.Ranges);
        return union_whatIsThere.Ranges.Count > 0;
    }

    public List<IntegerRange<T>> ToList() {
        return this.myUnion.ToList();
    }

    IIntegerSet<T> IIntegerSet<T>.Clone() => this.Clone();

    public ObservableIntegerSet<T> Clone() {
        return new ObservableIntegerSet<T>(this.myUnion);
    }

    public IEnumerator<IntegerRange<T>> GetEnumerator() {
        return this.myUnion.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return this.myUnion.GetEnumerator();
    }
}