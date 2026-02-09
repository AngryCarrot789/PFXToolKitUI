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
using PFXToolKitUI.Utils.Ranges;

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
        if (length < 1)
            return;
        if (Maths.WillAdditionOverflow(address, length))
            throw new InvalidOperationException("Writing the buffer results in address overflow");

        ulong endAddress = address + length;
        int startIndex = 0, high = this.myFragments.Count - 1;
        while (startIndex <= high) {
            int mid = (startIndex + high) >> 1;
            if (this.myFragments[mid].End < address)
                startIndex = mid + 1;
            else
                high = mid - 1;
        }

        int endIndex = startIndex;
        while (endIndex < this.myFragments.Count && this.myFragments[endIndex].Address <= endAddress) {
            endIndex++;
        }

        if (startIndex == endIndex) {
            this.myFragments.Insert(startIndex, new Fragment(address, buffer.ToArray()));
            return;
        }

        ulong start = Math.Min(address, this.myFragments[startIndex].Address);
        ulong end = Math.Max(endAddress, this.myFragments[endIndex - 1].End);
        byte[] merged = new byte[end - start];
        for (int i = startIndex; i < endIndex; i++) {
            Fragment f = this.myFragments[i];
            f.Data.CopyTo(merged.AsSpan((int) (f.Address - start)));
        }

        buffer.CopyTo(merged.AsSpan((int) (address - start)));
        this.myFragments.RemoveRange(startIndex, endIndex - startIndex);
        this.myFragments.Insert(startIndex, new Fragment(start, merged));
    }

    public void Clear() {
        this.myFragments.Clear();
    }

    public void Clear(ulong address, ulong count) {
        if (count == 0)
            return;
        if (Maths.WillAdditionOverflow(address, count))
            count = ulong.MaxValue - address;

        ulong endAddress = address + count;
        List<Fragment> keepFrags = new List<Fragment>();
        foreach (Fragment f in this.myFragments) {
            ulong fragmentEnd = f.End;
            if (fragmentEnd <= address || f.Address >= endAddress) {
                keepFrags.Add(f);
                continue;
            }

            ulong leftStart = f.Address;
            ulong leftEnd = Math.Min(address, fragmentEnd);
            ulong rightStart = Math.Max(endAddress, f.Address);
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

    public int Read(ulong address, Span<byte> buffer, List<IntegerRange<ulong>>? affectedRanges = null) {
        int bufferLength = buffer.Length;
        if (bufferLength == 0)
            return 0;

        if (Maths.WillAdditionOverflow(address, bufferLength))
            throw new InvalidOperationException("Reading the buffer results in the address overflowing");

        ulong endAddress = address + (ulong) bufferLength;
        int read = 0;
        int startAddress = 0, idxB = this.myFragments.Count - 1;
        while (startAddress <= idxB) {
            int mid = (startAddress + idxB) >> 1;
            if (this.myFragments[mid].End <= address)
                startAddress = mid + 1;
            else
                idxB = mid - 1;
        }

        for (int i = startAddress; i < this.myFragments.Count; i++) {
            Fragment frag = this.myFragments[i];
            if (frag.Address >= endAddress) {
                break;
            }

            ulong start = address > frag.Address ? address : frag.Address;
            ulong end = endAddress < frag.End ? endAddress : frag.End;
            int length = (int) (end - start);
            if (length > 0) {
                int srcIndex = (int) (start - frag.Address);
                int destIndex = (int) (start - address);

                frag.Data.AsSpan(srcIndex, length).CopyTo(buffer.Slice(destIndex, length));
                affectedRanges?.Add(IntegerRange.FromStartAndLength(address + (ulong) destIndex, (ulong) length));
                read += length;
            }
        }

        return read;
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