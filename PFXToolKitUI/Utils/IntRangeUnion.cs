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
using System.Diagnostics;

namespace PFXToolKitUI.Utils;

/// <summary>
/// Stores a list of integers as ranges
/// </summary>
public sealed class IntRangeUnion : IEnumerable<IntRange> {
    /// <summary>
    /// Returns a range of the smallest and largest integer value
    /// </summary>
    public IntRange EnclosingRange => this.myRanges.Count == 0 ? default : new IntRange(this.myRanges[0].Start, this.myRanges[^1].End);

    public bool IsFragmented => this.myRanges.Count > 1;

    /// <summary>
    /// Returns the number of ranges stored in this union
    /// </summary>
    public int RangeCount => this.myRanges.Count;
    
    /// <summary>
    /// Calculates the number of actual integers present in this union by iterating each range and summing its <see cref="IntRange.Length"/>
    /// </summary>
    public int GrandTotal {
        get {
            if (this.cachedGrandTotal.HasValue)
                return this.cachedGrandTotal.Value;
            
            int total = 0;
            foreach (IntRange range in this.myRanges)
                total += range.Length;
            this.cachedGrandTotal = total;
            return total;
        }
    }

    private readonly List<IntRange> myRanges = new();
    private int? cachedGrandTotal;

    /// <summary>
    /// Creates a new empty union.
    /// </summary>
    public IntRangeUnion() {
    }

    /// <summary>
    /// Initializes a new union of int ranges.
    /// </summary>
    /// <param name="ranges">The ranges to unify.</param>
    public IntRangeUnion(IEnumerable<IntRange> ranges) {
        ArgumentNullException.ThrowIfNull(ranges);
        foreach (IntRange range in ranges) {
            this.Add(range);
        }
    }

    public static void Test() {
        List<IntRange> reflectionRanges = new List<IntRange>();

        IntRangeUnion list = new IntRangeUnion();
        list.Add(1);
        list.Add(4);
        list.Add(IntRange.FromLength(2, 2));
        list.Add(40);
        list.Add(5);
        list.Add(9);
        list.Add(7);
        list.Add(6);
        list.Add(8);
        list.Add(42);
        list.Add(41);
        Debug.Assert(list.myRanges.Count == 2);
        Debug.Assert(list.myRanges[0].Equals(new IntRange(1, 10)));
        Debug.Assert(list.myRanges[1].Equals(new IntRange(40, 43)));

        list.Remove(2);
        Debug.Assert(list.myRanges.Count == 3);
        Debug.Assert(list.myRanges[0].Equals(new IntRange(1, 2)));
        Debug.Assert(list.myRanges[1].Equals(new IntRange(3, 10)));
        Debug.Assert(list.myRanges[2].Equals(new IntRange(40, 43)));

        list.Remove(new IntRange(3, 5));
        Debug.Assert(list.myRanges.Count == 3);
        Debug.Assert(list.myRanges[0].Equals(new IntRange(1, 2)));
        Debug.Assert(list.myRanges[1].Equals(new IntRange(5, 10)));
        Debug.Assert(list.myRanges[2].Equals(new IntRange(40, 43)));
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

        SearchResultPair result = this.FindFirstOverlappingRange(item);
        switch (result.Result) {
            case SearchResult.BeforeIndex: this.myRanges.Insert(result.Index + 1, item); break;

            case SearchResult.AfterIndex:
            case SearchResult.NotPresent:
                this.myRanges.Insert(result.Index, item);
                break;

            default: throw new ArgumentOutOfRangeException();
        }

        this.MergeRanges(result.Index);
        this.cachedGrandTotal = null;
    }

    public void Clear() {
        this.myRanges.Clear();
        this.cachedGrandTotal = null;
    }

    public bool Contains(IntRange item) => this.myRanges.Contains(item);

    public bool Contains(int location) => this.IsSuperSetOf(new IntRange(location, location == int.MaxValue ? int.MaxValue : (location + 1)));

    public bool IsSuperSetOf(IntRange range) => IsSuperSetOf(range, this.myRanges);

    public bool IntersectsWith(IntRange range) => IntersectsWith(range, this.myRanges);

    public int GetOverlappingRanges(IntRange range, Span<IntRange> output) {
        SearchResultPair result = this.FindFirstOverlappingRange(range);
        if (result.Result == SearchResult.NotPresent)
            return 0;

        int count = 0;
        for (int i = result.Index; i < this.myRanges.Count && count < output.Length; i++) {
            IntRange current = this.myRanges[i];
            if (current.Start >= range.End)
                break;

            if (current.OverlapsWith(range))
                output[count++] = current;
        }

        return count;
    }

    public int GetIntersectingRanges(IntRange range, Span<IntRange> output) {
        // Get overlapping ranges.
        int count = this.GetOverlappingRanges(range, output);

        // Cut off first and last ranges.
        if (count > 0) {
            output[0] = output[0].Clamp(range);
            if (count > 1)
                output[count - 1] = output[count - 1].Clamp(range);
        }

        return count;
    }

    public IntRangeUnion GetPresenceUnion(IntRange range, bool complement) {
        IntRangeUnion union = new IntRangeUnion();
        GetPresenceUnion(union, range, complement, this.myRanges);
        return union;
    }
    
    public void GetPresenceUnion(IntRangeUnion dstUnion, IntRange range, bool complement) {
        GetPresenceUnion(dstUnion, range, complement, this.myRanges);
    }

    private static void GetPresenceUnion(IntRangeUnion dstUnion, IntRange range, bool complement, List<IntRange> list) {
        for (int i = range.Start; i < range.End; i++) {
            if (IsSuperSetOf(new IntRange(i, i + 1), list) == complement) {
                dstUnion.Add(i);
            }
        }
    }

    public void CopyTo(IntRange[] array, int arrayIndex) => this.myRanges.CopyTo(array, arrayIndex);

    public bool Remove(int value) {
        if (value != int.MaxValue)
            return this.Remove(new IntRange(value, value + 1));
        return false;
    }

    public bool Remove(IntRange item) {
        SearchResultPair result = this.FindFirstOverlappingRange(item);
        if (result.Result == SearchResult.NotPresent)
            return false;

        int itemMaxEnd = item.End == int.MaxValue ? int.MaxValue : (item.End + 1);
        for (int i = result.Index; i < this.myRanges.Count; i++) {
            // Is this an overlapping range?
            if (!this.myRanges[i].OverlapsWith(item))
                break;

            if (this.myRanges[i].Contains(new IntRange(item.Start, itemMaxEnd))) {
                // The range contains the entire range-to-remove, split up the range.
                (IntRange a, IntRange rest) = this.myRanges[i].Split(item.Start);
                (IntRange b, IntRange c) = rest.Split(item.End);

                if (a.IsEmpty)
                    this.myRanges.RemoveAt(i--);
                else
                    this.myRanges[i] = a;

                if (!c.IsEmpty)
                    this.myRanges.Insert(i + 1, c);
                break;
            }

            if (item.Contains(this.myRanges[i])) {
                this.myRanges.RemoveAt(i--);
            }
            else if (item.Start < this.myRanges[i].Start) {
                this.myRanges[i] = this.myRanges[i].Clamp(new IntRange(item.End, int.MaxValue));
            }
            else if (item.End >= this.myRanges[i].End) {
                this.myRanges[i] = this.myRanges[i].Clamp(new IntRange(int.MinValue, item.Start));
            }
        }

        this.cachedGrandTotal = null;
        return true;
    }

    private void MergeRanges(int startIndex) {
        int i = Math.Max(0, startIndex - 1);
        while (i < this.myRanges.Count - 1) {
            IntRange current = this.myRanges[i];
            IntRange next = this.myRanges[i + 1];
            if (current.End < next.Start) {
                i++;
            }
            else {
                this.myRanges[i] = new IntRange(Math.Min(current.Start, next.Start), Math.Max(current.End, next.End));
                this.myRanges.RemoveAt(i + 1);
            }
        }
    }

    private SearchResultPair FindFirstOverlappingRange(IntRange range) {
        int start = 0, end = this.myRanges.Count - 1;
        while (start <= end) {
            int mid = start + (end - start) / 2;
            IntRange check = this.myRanges[mid];
            if (check.End < range.Start) {
                start = mid + 1;
            }
            else if (check.Start > range.End) {
                end = mid - 1;
            }
            else {
                return new SearchResultPair(check.Start >= range.Start ? SearchResult.AfterIndex : SearchResult.BeforeIndex, mid);
            }
        }

        return new SearchResultPair(SearchResult.NotPresent, start);
    }

    public List<IntRange> ToList() => this.myRanges.ToList();

    public static bool IsSuperSetOf(IntRange range, List<IntRange> list) {
        SearchResultPair result = FindFirstOverlappingRange(range, list);
        if (result.Result == SearchResult.NotPresent)
            return false;

        return list[result.Index].Contains(range);
    }

    public static bool IntersectsWith(IntRange range, List<IntRange> list) {
        SearchResultPair result = FindFirstOverlappingRange(range, list);
        if (result.Result == SearchResult.NotPresent)
            return false;

        return list[result.Index].OverlapsWith(range);
    }

    internal static SearchResultPair FindFirstOverlappingRange(IntRange range, List<IntRange> list) {
        int start = 0, end = list.Count - 1;
        while (start <= end) {
            int mid = start + (end - start) / 2;
            IntRange check = list[mid];
            if (check.End < range.Start) {
                start = mid + 1;
            }
            else if (check.Start > range.End) {
                end = mid - 1;
            }
            else {
                return new SearchResultPair(check.Start >= range.Start ? SearchResult.AfterIndex : SearchResult.BeforeIndex, mid);
            }
        }

        return new SearchResultPair(SearchResult.NotPresent, start);
    }

    internal enum SearchResult { BeforeIndex, AfterIndex, NotPresent }

    internal readonly record struct SearchResultPair(SearchResult Result, int Index);

    public Enumerator GetEnumerator() => new(this);

    IEnumerator<IntRange> IEnumerable<IntRange>.GetEnumerator() => this.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    /// <summary>
    /// An implementation of an enumerator that enumerates all disjoint ranges within a bit range union.
    /// </summary>
    public struct Enumerator : IEnumerator<IntRange> {
        public IntRange Current => this.index < this.ranges.myRanges.Count ? this.ranges.myRanges[this.index] : default;

        object IEnumerator.Current => this.Current;

        private readonly IntRangeUnion ranges;
        private int index;

        /// <summary>
        /// Creates a new disjoint bit range union enumerator.
        /// </summary>
        /// <param name="ranges">The disjoint union to enumerate.</param>
        public Enumerator(IntRangeUnion ranges) {
            this.ranges = ranges;
            this.index = -1;
        }

        public bool MoveNext() {
            return ++this.index < this.ranges.myRanges.Count;
        }

        void IEnumerator.Reset() {
        }

        public void Dispose() {
        }
    }
}