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

namespace PFXToolKitUI.Utils;

public delegate void IntRangeUnionExIndicesChangedEventHandler(IList<IntRange> indices);

/// <summary>
/// A delegate around <see cref="IntRangeUnion"/> that fires an event when indices are added and removed
/// </summary>
public sealed class IntRangeUnionEx {
    private readonly IntRangeUnion myUnion;
    
    /// <summary>
    /// Returns a range of the smallest and largest integer value
    /// </summary>
    public IntRange EnclosingRange => this.myUnion.EnclosingRange;

    public bool IsFragmented => this.myUnion.IsFragmented;

    public int RangeCount => this.myUnion.RangeCount;
    
    public int GrandTotal => this.myUnion.GrandTotal;

    public event IntRangeUnionExIndicesChangedEventHandler? IndicesAdded;
    public event IntRangeUnionExIndicesChangedEventHandler? IndicesRemoved;

    /// <summary>
    /// Creates a new empty union.
    /// </summary>
    public IntRangeUnionEx() {
        this.myUnion = new IntRangeUnion();
    }

    /// <summary>
    /// Initializes a new union of int ranges.
    /// </summary>
    /// <param name="ranges">The ranges to unify.</param>
    public IntRangeUnionEx(IEnumerable<IntRange> ranges) {
        ArgumentNullException.ThrowIfNull(ranges);
        this.myUnion = new IntRangeUnion(ranges);
    }

    public static void Test() {
        IntRangeUnionEx list = new IntRangeUnionEx();
        list.IndicesAdded += indices => {
            Console.WriteLine("Add: " + string.Join(", ", indices));
        };

        list.IndicesRemoved += indices => {
            Console.WriteLine("Remove: " + string.Join(", ", indices));
        };

        list.Add(1);
        list.Add(4);
        list.Add(IntRange.FromLength(2, 2));
        list.Add(40);
        // list.Add(5);
        list.Add(9);
        list.Add(7);
        list.Add(6);
        list.Add(8);
        list.Add(42);
        list.Add(41);
        list.Add(new IntRange(4, 44));
        list.Remove(2);
        list.Remove(4);
        list.Remove(3);
    }
    
    public void Add(int value) {
        if (value < int.MaxValue)
            this.Add(new IntRange(value, value + 1));
    }

    public void Add(IntRange item) {
        if (item.Start < 0)
            throw new ArgumentOutOfRangeException(nameof(item), "item.Start cannot be negative");
        if (item.End < item.Start)
            throw new ArgumentOutOfRangeException(nameof(item), "item.End cannot be less than item.Start");
        if (item.Start == item.End)
            return; // we are adding literally nothing

        IntRangeUnion union_whatIsNotThere = this.myUnion.GetPresenceUnion(item, false);
        this.myUnion.Add(item);
        if (union_whatIsNotThere.RangeCount > 0)
            this.IndicesAdded?.Invoke(union_whatIsNotThere.ToList());
    }

    public void Clear() {
        IntRange range = this.EnclosingRange;
        this.myUnion.Clear();
        this.IndicesAdded?.Invoke([range]);
    }

    public bool Contains(IntRange item) => this.myUnion.Contains(item);

    public bool Contains(int location) => this.myUnion.Contains(location);

    public bool IsSuperSetOf(IntRange range) => this.myUnion.IsSuperSetOf(range);

    public bool IntersectsWith(IntRange range) => this.myUnion.IntersectsWith(range);

    public int GetOverlappingRanges(IntRange range, Span<IntRange> output) => this.myUnion.GetOverlappingRanges(range, output);

    public int GetIntersectingRanges(IntRange range, Span<IntRange> output) => this.myUnion.GetIntersectingRanges(range, output);

    public bool Remove(int value) {
        if (value != int.MaxValue)
            return this.Remove(new IntRange(value, value + 1));
        return false;
    }

    public bool Remove(IntRange item) {
        IntRangeUnion union_whatIsThere = this.myUnion.GetPresenceUnion(item, true);
        bool removed = this.myUnion.Remove(item);
        if (union_whatIsThere.RangeCount > 0)
            this.IndicesRemoved?.Invoke(union_whatIsThere.ToList());
        
        return union_whatIsThere.RangeCount > 0;
    }
}