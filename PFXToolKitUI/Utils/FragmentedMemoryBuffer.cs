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

using System.Diagnostics;

namespace PFXToolKitUI.Utils;

/// <summary>
/// An array of elements stored in fragments, indexable by <see cref="ulong"/>. Automatically de-fragments and splits fragments when writing/reading data
/// </summary>
public class FragmentedMemoryBuffer {
    private List<Fragment> myFragments;

    public FragmentedMemoryBuffer() {
        this.myFragments = new List<Fragment>();
    }

    /// <summary>
    /// Writes the buffer into this memory buffer
    /// </summary>
    /// <param name="address"></param>
    /// <param name="buffer"></param>
    public void Write(ulong address, Span<byte> buffer) {
        ulong length = (ulong) buffer.Length;
        if (length == 0)
            return;
        if (Maths.WillAdditionOverflow(address, length))
            throw new InvalidOperationException($"Writing the buffer results in the address overflowing ({address} + {length}))");

        List<Fragment> overlapping = new List<Fragment>();
        List<Fragment> keepList = new List<Fragment>();
        foreach (Fragment fragment in this.myFragments) {
            if ((address + length) >= fragment.Address && fragment.End >= address) {
                overlapping.Add(fragment);
            }
            else {
                keepList.Add(fragment);
            }
        }

        Debug.Assert(overlapping.Count == 0 || overlapping.OrderBy(f => f.Address).SequenceEqual(overlapping));
        // overlapping = overlapping.OrderBy(f => f.Address).ToList();

        ulong start = overlapping.Count != 0 ? Math.Min(address, overlapping[0].Address) : address;
        ulong end = overlapping.Count != 0 ? Math.Max(address + length, overlapping[overlapping.Count - 1].End) : address + length;
        byte[] mergedData = new byte[end - start];

        foreach (Fragment f in overlapping) {
            f.Data.CopyTo(mergedData.AsSpan((int) (f.Address - start)));
        }

        buffer.CopyTo(mergedData.AsSpan((int) (address - start)));
        keepList.Add(new Fragment(start, mergedData));
        this.myFragments = keepList.OrderBy(f => f.Address).ToList();
    }

    public void Clear() {
        this.myFragments.Clear();
    }

    public void Clear(ulong offset, ulong count) {
        if (count == 0)
            return;
        if (Maths.WillAdditionOverflow(offset, count))
            count = ulong.MaxValue - offset;

        ulong end = offset + count;
        List<Fragment> keepFrags = new List<Fragment>();
        foreach (Fragment f in this.myFragments) {
            ulong fragmentEnd = f.End;
            if (fragmentEnd <= offset || f.Address >= end) {
                keepFrags.Add(f);
                continue;
            }

            ulong leftStart = f.Address;
            ulong leftEnd = Math.Min(offset, fragmentEnd);
            ulong rightStart = Math.Max(end, f.Address);
            ulong rightEnd = fragmentEnd;

            if (leftEnd > leftStart) {
                int leftLen = (int) (leftEnd - leftStart);
                byte[] leftData = new byte[leftLen];
                f.Data.AsSpan(0, leftLen).CopyTo(leftData.AsSpan(0, leftLen));
                keepFrags.Add(new Fragment(leftStart, leftData));
            }

            if (rightEnd > rightStart) {
                int rightOffset = (int) (rightStart - f.Address);
                int rightLen = (int) (rightEnd - rightStart);
                byte[] rightData = new byte[rightLen];
                f.Data.AsSpan(rightOffset, rightLen).CopyTo(rightData.AsSpan(0, rightLen));
                keepFrags.Add(new Fragment(rightStart, rightData));
            }
        }

        this.myFragments = keepFrags.OrderBy(f => f.Address).ToList();
    }

    public int Read(ulong offset, Span<byte> buffer, List<(ulong, ulong)>? affectedRanges = null) {
        if (buffer.Length == 0)
            return 0;
        if (Maths.WillAdditionOverflow(offset, buffer.Length))
            throw new InvalidOperationException("Reading the buffer results in the address overflowing");

        ulong offsetEnd = offset + (ulong) buffer.Length;
        int cbTotalRead = 0;
        foreach (Fragment frag in this.myFragments) {
            ulong fragEnd = frag.End;
            if (fragEnd > offset && frag.Address < offsetEnd) {
                ulong start = Math.Max(offset, frag.Address);
                ulong end = Math.Min(offsetEnd, fragEnd);
                int length = (int) (end - start);
                int srcIndex = (int) (start - frag.Address);
                int destIndex = (int) (start - offset);

                frag.Data.AsSpan(srcIndex, length).CopyTo(buffer.Slice(destIndex, length));
                affectedRanges?.Add((offset + (ulong) destIndex, (ulong) length));
                cbTotalRead += length;
            }
        }

        return cbTotalRead;
    }

    public override string ToString() {
        return $"Fragments: {string.Join(", ", this.myFragments.Select(f => $"[{f.Address}-{f.End})"))}";
    }

    [DebuggerDisplay("{Address} -> {End} ({Data.Length})")]
    private readonly struct Fragment(ulong address, byte[] data) : IEquatable<Fragment>, IComparable<Fragment> {
        public readonly ulong Address = address;
        public readonly byte[] Data = data;
        public ulong End => this.Address + (ulong) this.Data.Length;

        public bool Equals(Fragment other) => this.Address == other.Address && this.Data.Equals(other.Data);

        public override bool Equals(object? obj) => obj is Fragment other && this.Equals(other);

        public override int GetHashCode() => HashCode.Combine(this.Address, this.Data);

        public int CompareTo(Fragment other) => this.Address.CompareTo(other.Address);
    }
}