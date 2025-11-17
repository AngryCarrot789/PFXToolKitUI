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
using System.Numerics;

namespace PFXToolKitUI.Utils.Ranges;

/// <summary>
/// Stores a list of integers as ranges. This data structure cannot store <see cref="IMinMaxValue{T}.MaxValue"/>
/// </summary>
public sealed class IntegerSet<T> : IIntegerSet<T> where T : unmanaged, IBinaryInteger<T>, IMinMaxValue<T> {
    private readonly List<IntegerRange<T>> myRanges = new();
    private Optional<T> cachedGrandTotal;

    public IReadOnlyList<IntegerRange<T>> Ranges => this.myRanges;

    public bool IsFragmented => this.myRanges.Count > 1;

    public IntegerRange<T> EnclosingRange {
        get {
            return this.myRanges.Count < 1
                ? default
                : new IntegerRange<T>(this.myRanges[0].Start, this.myRanges[this.myRanges.Count - 1].End);
        }
    }

    public T GrandTotal {
        get {
            if (this.cachedGrandTotal.HasValue)
                return this.cachedGrandTotal.Value;

            T total = default;
            foreach (IntegerRange<T> range in this.myRanges)
                total += range.Length;
            this.cachedGrandTotal = total;
            return total;
        }
    }

    /// <summary>
    /// Creates a new empty union.
    /// </summary>
    public IntegerSet() {
    }

    /// <summary>
    /// Initializes a new union of int ranges.
    /// </summary>
    /// <param name="ranges">The ranges to unify.</param>
    public IntegerSet(IEnumerable<IntegerRange<T>> ranges) {
        ArgumentNullException.ThrowIfNull(ranges);
        foreach (IntegerRange<T> range in ranges) {
            this.Add(range);
        }
    }

    public IntegerSet(IReadOnlyIntegerSet<T> ranges) {
        ArgumentNullException.ThrowIfNull(ranges);
        if (ranges is IntegerSet<T> union) {
            this.myRanges = new List<IntegerRange<T>>(union.myRanges);
        }
        else {
            foreach (IntegerRange<T> range in ranges.Ranges) {
                this.Add(range);
            }
        }
    }

    private IntegerSet(List<IntegerRange<T>> ranges) {
        this.myRanges = ranges;
    }

    public void Add(T range) {
        if (range != T.MaxValue) {
            this.Add(new IntegerRange<T>(range, range + T.One));
        }
    }

    public void Add(IntegerRange<T> range) {
        if (range.Start == T.MaxValue)
            return;
        if (range.End < range.Start)
            throw new ArgumentOutOfRangeException(nameof(range), "item.End cannot be less than item.Start");
        if (range.Start == range.End)
            return; // we are adding literally nothing

        bool found = FindFirstOverlappingRange(range, this.myRanges, out bool isAfterIndex, out int index);
        this.myRanges.Insert((!found || isAfterIndex) ? index : (index + 1), range);

        // Merge ranges
        int i = Math.Max(0, index - 1);
        while (i < this.myRanges.Count - 1) {
            IntegerRange<T> current = this.myRanges[i];
            IntegerRange<T> next = this.myRanges[i + 1];
            if (current.End < next.Start) {
                i++;
            }
            else {
                T start = T.Min(current.Start, next.Start);
                T end = T.Max(current.End, next.End);
                this.myRanges[i] = new IntegerRange<T>(start, end);
                this.myRanges.RemoveAt(i + 1);
            }
        }

        this.cachedGrandTotal = default;
    }

    public void Clear() {
        this.myRanges.Clear();
        this.cachedGrandTotal = default;
    }

    /// <summary>
    /// Returns true when this union contains the single location
    /// </summary>
    /// <param name="location"></param>
    /// <returns></returns>
    public bool Contains(T location) {
        return location != T.MaxValue && this.Contains(new IntegerRange<T>(location, location + T.One));
    }

    /// <summary>
    /// Returns true when this union contains the entirety of the given range
    /// </summary>
    public bool Contains(IntegerRange<T> range) => Contains(range, this.myRanges);

    /// <summary>
    /// Returns true when the range, in any way, overlaps this union 
    /// </summary>
    public bool Overlaps(IntegerRange<T> range) => Overlaps(range, this.myRanges);

    public int GetOverlappingRanges(IntegerRange<T> range, Span<IntegerRange<T>> output) {
        if (!FindFirstOverlappingRange(range, this.myRanges, out bool isAfterIndex, out int index)) {
            return 0;
        }

        int count = 0;
        for (int i = index; i < this.myRanges.Count && count < output.Length; i++) {
            IntegerRange<T> current = this.myRanges[i];
            if (current.Start >= range.End)
                break;

            if (current.Overlaps(range))
                output[count++] = current;
        }

        return count;
    }

    public int GetIntersectingRanges(IntegerRange<T> range, Span<IntegerRange<T>> output) {
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

    public IntegerSet<T> GetPresenceUnion(IntegerRange<T> range, bool complement) {
        IntegerSet<T> union = new IntegerSet<T>();
        GetPresenceUnion(union, range, complement, this.myRanges);
        return union;
    }

    public void GetPresenceUnion(IntegerSet<T> dstSet, IntegerRange<T> range, bool complement) {
        GetPresenceUnion(dstSet, range, complement, this.myRanges);
    }

    private static void GetPresenceUnion(IntegerSet<T> dstSet, IntegerRange<T> range, bool complement, List<IntegerRange<T>> list) {
        T pos = range.Start;
        foreach (IntegerRange<T> r in list) {
            if (r.End <= range.Start)
                continue;
            if (r.Start >= range.End)
                break;

            if (complement) {
                T start = T.Max(pos, r.Start);
                T end = T.Min(range.End, r.End);
                if (start < end) {
                    dstSet.Add(new IntegerRange<T>(start, end));
                }
            }
            else if (pos < r.Start) {
                dstSet.Add(IntegerRange.FromStartAndEnd(pos, T.Min(r.Start, range.End)));
            }

            if ((pos = T.Max(pos, r.End)) >= range.End) {
                break;
            }
        }

        if (!complement && pos < range.End) {
            dstSet.Add(new IntegerRange<T>(pos, range.End));
        }
    }
    
    public void CopyTo(IntegerRange<T>[] array, int arrayIndex) => this.myRanges.CopyTo(array, arrayIndex);

    public bool Remove(T value) {
        if (value != T.MaxValue)
            return this.Remove(IntegerRange.FromStartAndEnd(value, value + T.One));
        return false;
    }

    public bool Remove(IntegerRange<T> range) {
        if (range.Start == T.MaxValue)
            return false;
        if (!FindFirstOverlappingRange(range, this.myRanges, out bool isAfterIndex, out int index))
            return false;

        IntegerRange<T> rangePlusOne = IntegerRange.FromStartAndEnd(range.Start, range.End == T.MaxValue ? T.MaxValue : (range.End + T.One));
        for (int i = index; i < this.myRanges.Count; i++) {
            // Is this an overlapping range?
            if (!this.myRanges[i].Overlaps(range))
                break;

            if (this.myRanges[i].Contains(rangePlusOne)) {
                // The range contains the entire range-to-remove, split up the range.
                (IntegerRange<T> a, IntegerRange<T> rest) = this.myRanges[i].Split(range.Start);
                (IntegerRange<T> b, IntegerRange<T> c) = rest.Split(range.End);

                if (a.IsEmpty)
                    this.myRanges.RemoveAt(i--);
                else
                    this.myRanges[i] = a;

                if (!c.IsEmpty)
                    this.myRanges.Insert(i + 1, c);
                break;
            }

            if (range.Contains(this.myRanges[i])) {
                this.myRanges.RemoveAt(i--);
            }
            else if (range.Start < this.myRanges[i].Start) {
                this.myRanges[i] = this.myRanges[i].Clamp(new IntegerRange<T>(range.End, T.MaxValue));
            }
            else if (range.End >= this.myRanges[i].End) {
                this.myRanges[i] = this.myRanges[i].Clamp(new IntegerRange<T>(T.MinValue, range.Start));
            }
        }

        this.cachedGrandTotal = default;
        return true;
    }

    /// <summary>
    /// Creates a new list with the contents of <see cref="Ranges"/>
    /// </summary>
    /// <returns>A new list with our ranges</returns>
    public List<IntegerRange<T>> ToList() => new List<IntegerRange<T>>(this.myRanges);

    IIntegerSet<T> IIntegerSet<T>.Clone() => this.Clone();

    public IntegerSet<T> Clone() {
        return new IntegerSet<T>(this.myRanges.ToList());
    }

    public static bool Contains(IntegerRange<T> range, List<IntegerRange<T>> list) {
        if (range.Start == T.MaxValue || !FindFirstOverlappingRange(range, list, out bool isAfterIndex, out int index))
            return false;
        return list[index].Contains(range);
    }

    public static bool Overlaps(IntegerRange<T> range, List<IntegerRange<T>> list) {
        if (range.Start == T.MaxValue || !FindFirstOverlappingRange(range, list, out bool isAfterIndex, out int index))
            return false;
        return list[index].Overlaps(range);
    }

    internal static bool FindFirstOverlappingRange(IntegerRange<T> range, List<IntegerRange<T>> list, out bool isAfterIndex, out int index) {
        int start = 0, end = list.Count - 1;
        while (start <= end) {
            int mid = start + (end - start) / 2;
            IntegerRange<T> check = list[mid];
            if (check.End < range.Start) {
                start = mid + 1;
            }
            else if (check.Start > range.End) {
                end = mid - 1;
            }
            else {
                isAfterIndex = check.Start >= range.Start;
                index = mid;
                return true;
            }
        }

        index = start;
        isAfterIndex = false;
        return false;
    }

    public Enumerator GetEnumerator() => new(this);

    IEnumerator<IntegerRange<T>> IEnumerable<IntegerRange<T>>.GetEnumerator() => this.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    public struct Enumerator : IEnumerator<IntegerRange<T>> {
        private readonly IntegerSet<T> ranges;
        private int index;
        
        public IntegerRange<T> Current => this.index < this.ranges.myRanges.Count ? this.ranges.myRanges[this.index] : default;

        object IEnumerator.Current => this.Current;

        public Enumerator(IntegerSet<T> ranges) {
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