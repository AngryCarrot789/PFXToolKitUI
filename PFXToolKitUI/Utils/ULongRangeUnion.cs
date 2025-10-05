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

/// <summary>
/// Stores a list of integers as ranges
/// </summary>
public sealed class ULongRangeUnion : IEnumerable<ULongRange> {
    /// <summary>
    /// Returns a range of the smallest and largest integer value
    /// </summary>
    public ULongRange EnclosingRange => this.myRanges.Count == 0 ? default : ULongRange.FromStartAndEnd(this.myRanges[0].Start, this.myRanges[^1].End);

    public bool IsFragmented => this.myRanges.Count > 1;

    /// <summary>
    /// Returns the number of ranges stored in this union
    /// </summary>
    public int RangeCount => this.myRanges.Count;

    /// <summary>
    /// Calculates the number of actual integers present in this union by iterating each range and summing its <see cref="ULongRange.Length"/>
    /// </summary>
    public ulong GrandTotal {
        get {
            if (this.cachedGrandTotal.HasValue)
                return this.cachedGrandTotal.Value;

            ulong total = 0;
            foreach (ULongRange range in this.myRanges)
                total += range.Length;
            this.cachedGrandTotal = total;
            return total;
        }
    }

    private readonly List<ULongRange> myRanges = new();
    private ulong? cachedGrandTotal;

    /// <summary>
    /// Creates a new empty union.
    /// </summary>
    public ULongRangeUnion() {
    }

    /// <summary>
    /// Initializes a new union of int ranges.
    /// </summary>
    /// <param name="ranges">The ranges to unify.</param>
    public ULongRangeUnion(IEnumerable<ULongRange> ranges) {
        ArgumentNullException.ThrowIfNull(ranges);
        foreach (ULongRange range in ranges) {
            this.Add(range);
        }
    }

    private ULongRangeUnion(List<ULongRange> ranges) {
        this.myRanges = ranges;
    }

    public void Add(ulong value) {
        if (value < ulong.MaxValue)
            this.Add(ULongRange.FromStartAndEnd(value, value + 1));
    }

    public void Add(ULongRange item) {
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

    /// <summary>
    /// Returns true when this union contains the single location
    /// </summary>
    /// <param name="location"></param>
    /// <returns></returns>
    public bool IsSuperSet(ulong location) => this.IsSuperSet(ULongRange.FromStartAndEnd(location, location == ulong.MaxValue ? ulong.MaxValue : (location + 1)));
    
    /// <summary>
    /// Returns true when this union contains the entirety of the given range
    /// </summary>
    public bool IsSuperSet(ULongRange range) => IsSuperSet(range, this.myRanges);

    /// <summary>
    /// Returns true when the range, in any way, overlaps this union 
    /// </summary>
    public bool Overlaps(ULongRange range) => Overlaps(range, this.myRanges);
    
    public int GetOverlappingRanges(ULongRange range, Span<ULongRange> output) {
        SearchResultPair result = this.FindFirstOverlappingRange(range);
        if (result.Result == SearchResult.NotPresent)
            return 0;

        int count = 0;
        for (int i = result.Index; i < this.myRanges.Count && count < output.Length; i++) {
            ULongRange current = this.myRanges[i];
            if (current.Start >= range.End)
                break;

            if (current.Overlaps(range))
                output[count++] = current;
        }

        return count;
    }

    public int GetIntersectingRanges(ULongRange range, Span<ULongRange> output) {
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

    public ULongRangeUnion GetPresenceUnion(ULongRange range, bool complement) {
        ULongRangeUnion union = new ULongRangeUnion();
        GetPresenceUnion(union, range, complement, this.myRanges);
        return union;
    }

    public void GetPresenceUnion(ULongRangeUnion dstUnion, ULongRange range, bool complement) {
        GetPresenceUnion(dstUnion, range, complement, this.myRanges);
    }

    // private static void GetPresenceUnion(ULongRangeUnion dstUnion, ULongRange range, bool complement, List<ULongRange> list) {
    //     for (ulong i = range.Start; i < range.End; i++) {
    //         if (IsSuperSetOf(ULongRange.FromStartAndEnd(i, i + 1), list) == complement) {
    //             dstUnion.Add(i);
    //         }
    //     }
    // }

    private static void GetPresenceUnion(ULongRangeUnion dstUnion, ULongRange range, bool complement, List<ULongRange> list) {
        ulong pos = range.Start;
        foreach (ULongRange r in list) {
            if (r.End <= range.Start)
                continue;
            if (r.Start >= range.End)
                break;

            if (complement) {
                ulong start = Math.Max(pos, r.Start);
                ulong end = Math.Min(range.End, r.End);
                if (start < end)
                    dstUnion.Add(ULongRange.FromStartAndEnd(start, end));
            }
            else if (pos < r.Start) {
                dstUnion.Add(ULongRange.FromStartAndEnd(pos, Math.Min(r.Start, range.End)));
            }

            if ((pos = Math.Max(pos, r.End)) >= range.End) {
                break;
            }
        }

        if (!complement && pos < range.End) {
            dstUnion.Add(ULongRange.FromStartAndEnd(pos, range.End));
        }
    }


    public void CopyTo(ULongRange[] array, int arrayIndex) => this.myRanges.CopyTo(array, arrayIndex);

    public bool Remove(ulong value) {
        if (value != ulong.MaxValue)
            return this.Remove(ULongRange.FromStartAndEnd(value, value + 1));
        return false;
    }

    public bool Remove(ULongRange item) {
        SearchResultPair result = this.FindFirstOverlappingRange(item);
        if (result.Result == SearchResult.NotPresent)
            return false;

        ulong itemMaxEnd = item.End == ulong.MaxValue ? ulong.MaxValue : (item.End + 1);
        for (int i = result.Index; i < this.myRanges.Count; i++) {
            // Is this an overlapping range?
            if (!this.myRanges[i].Overlaps(item))
                break;

            if (this.myRanges[i].IsSuperSet(ULongRange.FromStartAndEnd(item.Start, itemMaxEnd))) {
                // The range contains the entire range-to-remove, split up the range.
                (ULongRange a, ULongRange rest) = this.myRanges[i].Split(item.Start);
                (ULongRange b, ULongRange c) = rest.Split(item.End);

                if (a.IsEmpty)
                    this.myRanges.RemoveAt(i--);
                else
                    this.myRanges[i] = a;

                if (!c.IsEmpty)
                    this.myRanges.Insert(i + 1, c);
                break;
            }

            if (item.IsSuperSet(this.myRanges[i])) {
                this.myRanges.RemoveAt(i--);
            }
            else if (item.Start < this.myRanges[i].Start) {
                this.myRanges[i] = this.myRanges[i].Clamp(ULongRange.FromStartAndEnd(item.End, ulong.MaxValue));
            }
            else if (item.End >= this.myRanges[i].End) {
                this.myRanges[i] = this.myRanges[i].Clamp(ULongRange.FromStartAndEnd(ulong.MinValue, item.Start));
            }
        }

        this.cachedGrandTotal = null;
        return true;
    }

    private void MergeRanges(int startIndex) {
        int i = Math.Max(0, startIndex - 1);
        while (i < this.myRanges.Count - 1) {
            ULongRange current = this.myRanges[i];
            ULongRange next = this.myRanges[i + 1];
            if (current.End < next.Start) {
                i++;
            }
            else {
                this.myRanges[i] = ULongRange.FromStartAndEnd(Math.Min(current.Start, next.Start), Math.Max(current.End, next.End));
                this.myRanges.RemoveAt(i + 1);
            }
        }
    }

    private SearchResultPair FindFirstOverlappingRange(ULongRange range) {
        int start = 0, end = this.myRanges.Count - 1;
        while (start <= end) {
            int mid = start + (end - start) / 2;
            ULongRange check = this.myRanges[mid];
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

    public List<ULongRange> ToList() => new List<ULongRange>(this.myRanges);

    public static bool IsSuperSet(ULongRange range, List<ULongRange> list) {
        SearchResultPair result = FindFirstOverlappingRange(range, list);
        if (result.Result == SearchResult.NotPresent)
            return false;

        return list[result.Index].IsSuperSet(range);
    }

    public static bool Overlaps(ULongRange range, List<ULongRange> list) {
        SearchResultPair result = FindFirstOverlappingRange(range, list);
        if (result.Result == SearchResult.NotPresent)
            return false;
        return list[result.Index].Overlaps(range);
    }

    internal static SearchResultPair FindFirstOverlappingRange(ULongRange range, List<ULongRange> list) {
        int start = 0, end = list.Count - 1;
        while (start <= end) {
            int mid = start + (end - start) / 2;
            ULongRange check = list[mid];
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

    IEnumerator<ULongRange> IEnumerable<ULongRange>.GetEnumerator() => this.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    /// <summary>
    /// An implementation of an enumerator that enumerates all disjoint ranges within a bit range union.
    /// </summary>
    public struct Enumerator : IEnumerator<ULongRange> {
        public ULongRange Current => this.index < this.ranges.myRanges.Count ? this.ranges.myRanges[this.index] : default;

        object IEnumerator.Current => this.Current;

        private readonly ULongRangeUnion ranges;
        private int index;

        /// <summary>
        /// Creates a new disjoint bit range union enumerator.
        /// </summary>
        /// <param name="ranges">The disjoint union to enumerate.</param>
        public Enumerator(ULongRangeUnion ranges) {
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

    public ULongRangeUnion Clone() {
        return new ULongRangeUnion(this.myRanges.ToList());
    }
}