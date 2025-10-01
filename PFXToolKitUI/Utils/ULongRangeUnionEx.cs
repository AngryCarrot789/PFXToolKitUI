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

namespace PFXToolKitUI.Utils;

/// <summary>
/// A delegate around <see cref="ULongRangeUnion"/> that fires an event when indices are added and removed
/// </summary>
public sealed class ULongRangeUnionEx : IObservableULongRangeUnion {
    private readonly ULongRangeUnion myUnion;

    /// <summary>
    /// Returns a range of the smallest and largest integer value
    /// </summary>
    public ULongRange EnclosingRange => this.myUnion.EnclosingRange;

    public bool IsFragmented => this.myUnion.IsFragmented;

    public int RangeCount => this.myUnion.RangeCount;

    public ulong GrandTotal => this.myUnion.GrandTotal;

    public event IObservableULongRangeUnion.IndicesChangedEventHandler? IndicesChanged;

    /// <summary>
    /// Creates a new empty union.
    /// </summary>
    public ULongRangeUnionEx() {
        this.myUnion = new ULongRangeUnion();
    }

    /// <summary>
    /// Initializes a new union of ulong ranges.
    /// </summary>
    /// <param name="ranges">The ranges to unify.</param>
    public ULongRangeUnionEx(IEnumerable<ULongRange> ranges) {
        ArgumentNullException.ThrowIfNull(ranges);
        this.myUnion = new ULongRangeUnion(ranges);
    }

    public void Add(ulong value) {
        if (value < ulong.MaxValue)
            this.Add(new ULongRange(value, value + 1));
    }

    public void Add(ULongRange item) {
        if (item.End < item.Start)
            throw new ArgumentOutOfRangeException(nameof(item), "item.End cannot be less than item.Start");
        if (item.Start == item.End)
            return; // we are adding literally nothing

        ULongRangeUnion union_whatIsNotThere = this.myUnion.GetPresenceUnion(item, false);
        this.myUnion.Add(item);
        if (union_whatIsNotThere.RangeCount > 0)
            this.IndicesChanged?.Invoke(this, union_whatIsNotThere.ToList().AsReadOnly(), ReadOnlyCollection<ULongRange>.Empty);
    }

    public void Clear() {
        ULongRange range = this.EnclosingRange;
        this.myUnion.Clear();
        this.IndicesChanged?.Invoke(this, [range], ReadOnlyCollection<ULongRange>.Empty);
    }

    public bool IsSuperSet(ulong location) => this.myUnion.IsSuperSet(location);

    public bool IsSuperSet(ULongRange range) => this.myUnion.IsSuperSet(range);

    public bool Overlaps(ULongRange range) => this.myUnion.Overlaps(range);

    public int GetOverlappingRanges(ULongRange range, Span<ULongRange> output) => this.myUnion.GetOverlappingRanges(range, output);

    public int GetIntersectingRanges(ULongRange range, Span<ULongRange> output) => this.myUnion.GetIntersectingRanges(range, output);

    public bool Remove(ulong value) {
        if (value != ulong.MaxValue)
            return this.Remove(new ULongRange(value, value + 1));
        return false;
    }

    public bool Remove(ULongRange item) {
        ULongRangeUnion union_whatIsThere = this.myUnion.GetPresenceUnion(item, true);
        bool removed = this.myUnion.Remove(item);
        if (union_whatIsThere.RangeCount > 0)
            this.IndicesChanged?.Invoke(this, ReadOnlyCollection<ULongRange>.Empty, union_whatIsThere.ToList());

        return union_whatIsThere.RangeCount > 0;
    }

    public IEnumerator<ULongRange> GetEnumerator() {
        return this.myUnion.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return this.myUnion.GetEnumerator();
    }
}

public interface IObservableULongRangeUnion : IEnumerable<ULongRange> {
    delegate void IndicesChangedEventHandler(IObservableULongRangeUnion sender, IList<ULongRange> addedIndices, IList<ULongRange> removedIndices);

    /// <summary>
    /// Gets the enclosing range of this union
    /// </summary>
    ULongRange EnclosingRange { get; }

    /// <summary>
    /// Gets whether the union is fragmented or not
    /// </summary>
    bool IsFragmented { get; }

    /// <summary>
    /// Gets the total number of range entries
    /// </summary>
    int RangeCount { get; }

    /// <summary>
    /// Gets the grand total value, which is the summation of the values between the min and max values of each range
    /// </summary>
    ulong GrandTotal { get; }

    /// <summary>
    /// An event fired when indices are added or removed
    /// </summary>
    event IndicesChangedEventHandler? IndicesChanged;
}