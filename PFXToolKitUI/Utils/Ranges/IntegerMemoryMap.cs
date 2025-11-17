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

using System.Diagnostics;
using System.Numerics;

namespace PFXToolKitUI.Utils.Ranges;

/// <summary>
/// An array of elements stored in fragments, indexable by <see cref="T"/>. Automatically de-fragments and splits fragments when writing/reading data
/// </summary>
public class IntegerMemoryMap<T, TValue> where T : unmanaged, IBinaryInteger<T>, IMinMaxValue<T> {
    private static readonly T MaximumLength = T.CreateChecked(int.MaxValue);

    private List<Fragment> myFragments;

    public IntegerMemoryMap() {
        this.myFragments = new List<Fragment>();
    }

    private static int CreateInt32(T value, string tooSmall, string tooLarge) {
        if (value < T.Zero)
            throw new ArgumentException(tooSmall);
        if (value > MaximumLength)
            throw new ArgumentException(tooLarge);
        return int.CreateChecked(value);
    }

    /// <summary>
    /// Writes the buffer into this memory buffer
    /// </summary>
    /// <param name="address"></param>
    /// <param name="buffer"></param>
    public void Write(T address, Span<TValue> buffer) {
        T length = T.CreateChecked(buffer.Length);
        if (length == T.Zero)
            return;
        if (Maths.WillAdditionOverflow(address, length))
            throw new InvalidOperationException($"Writing the buffer results in the address overflowing ({address} + {length}))");

        List<Fragment> overlapping = new List<Fragment>();
        List<Fragment> keepList = new List<Fragment>();
        foreach (Fragment fragment in this.myFragments) {
            if ((address + length) >= fragment.Range.Start && fragment.Range.End >= address) {
                overlapping.Add(fragment);
            }
            else {
                keepList.Add(fragment);
            }
        }

        Debug.Assert(overlapping.Count == 0 || overlapping.OrderBy(f => f.Range.Start).SequenceEqual(overlapping));
        // overlapping = overlapping.OrderBy(f => f.Address).ToList();

        T start = overlapping.Count != 0 ? T.Min(address, overlapping[0].Range.Start) : address;
        T end = overlapping.Count != 0 ? T.Max(address + length, overlapping[overlapping.Count - 1].Range.End) : address + length;
        TValue[] mergedData = new TValue[CreateInt32(end - start, "Error out of range", "Attempt to merge ranges such that it creates a buffer with an unsupported size (>2.147 billion elements)")];

        foreach (Fragment f in overlapping) {
            f.Data.CopyTo(mergedData.AsSpan(int.CreateChecked(f.Range.Start - start)));
        }

        buffer.CopyTo(mergedData.AsSpan(int.CreateChecked(address - start)));
        keepList.Add(new Fragment(IntegerRange.FromStartAndEnd(start, end), mergedData));
        this.myFragments = keepList.OrderBy(f => f.Range.Start).ToList();
    }

    public void Clear(T offset, T count) {
        const string msg = "Attempt to clear ranges such that it creates a buffer with an unsupported size (>2.147 billion elements)";

        if (count == T.Zero)
            return;
        if (Maths.WillAdditionOverflow(offset, count))
            count = T.MaxValue - offset;

        T end = offset + count;
        List<Fragment> keepFrags = new List<Fragment>();
        foreach (Fragment f in this.myFragments) {
            T fragmentEnd = f.Range.End;
            if (fragmentEnd <= offset || f.Range.Start >= end) {
                keepFrags.Add(f);
                continue;
            }

            T leftStart = f.Range.Start;
            T leftEnd = T.Min(offset, fragmentEnd);
            T rightStart = T.Max(end, f.Range.Start);
            T rightEnd = fragmentEnd;

            if (leftEnd > leftStart) {
                int leftLen = CreateInt32(leftEnd - leftStart, "Error out of range", msg);
                TValue[] leftData = new TValue[leftLen];
                f.Data.AsSpan(0, leftLen).CopyTo(leftData.AsSpan(0, leftLen));
                keepFrags.Add(new Fragment(IntegerRange.FromStartAndEnd(leftStart, leftEnd), leftData));
            }

            if (rightEnd > rightStart) {
                int rightOffset = CreateInt32(rightStart - f.Range.Start, "Error out of range", msg);
                int rightLen = CreateInt32(rightEnd - rightStart, "Error out of range", msg);
                TValue[] rightData = new TValue[rightLen];
                f.Data.AsSpan(rightOffset, rightLen).CopyTo(rightData.AsSpan(0, rightLen));
                keepFrags.Add(new Fragment(IntegerRange.FromStartAndEnd(rightStart, rightEnd), rightData));
            }
        }

        this.myFragments = keepFrags.OrderBy(f => f.Range.Start).ToList();
    }

    public T Read(T offset, Span<TValue> buffer, List<(T, T)>? affectedRanges = null) {
        if (buffer.Length == 0)
            return T.Zero;

        T len = T.CreateChecked(buffer.Length);
        if (Maths.WillAdditionOverflow(offset, len))
            throw new InvalidOperationException("Reading the buffer results in the address overflowing");

        T offsetEnd = offset + len;
        T cbTotalRead = T.Zero;
        foreach (Fragment frag in this.myFragments) {
            T fragEnd = frag.Range.End;
            if (fragEnd > offset && frag.Range.Start < offsetEnd) {
                T start = T.Max(offset, frag.Range.Start);
                T end = T.Min(offsetEnd, fragEnd);
                T length = (end - start);
                T srcIndex = (start - frag.Range.Start);
                T destIndex = (start - offset);

                frag.Data.AsSpan(int.CreateChecked(srcIndex), int.CreateChecked(length)).CopyTo(buffer.Slice(int.CreateChecked(destIndex), int.CreateChecked(length)));
                affectedRanges?.Add((offset + destIndex, length));
                cbTotalRead += length;
            }
        }

        return cbTotalRead;
    }

    public override string ToString() {
        return $"Fragments: {string.Join(", ", this.myFragments.Select(f => $"[{f.Range}]"))}";
    }

    private readonly struct Fragment(IntegerRange<T> range, TValue[] data) : IEquatable<Fragment> {
        public readonly IntegerRange<T> Range = range;
        public readonly TValue[] Data = data;

        public bool Equals(Fragment other) => this.Range == other.Range && this.Data.Equals(other.Data);

        public override bool Equals(object? obj) => obj is Fragment other && this.Equals(other);

        public override int GetHashCode() => HashCode.Combine(this.Range, this.Data);
    }
}