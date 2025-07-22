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

/// <summary>
/// A class used to store memory in fragments, with support for addressing with ulong
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
        if (Maths.WillOverflow(address, length))
            throw new InvalidOperationException("Writing the buffer results in the address overflowing");

        List<Fragment> overlapping = new List<Fragment>();
        foreach (Fragment fragment in this.myFragments) {
            if (Fragment.OverlapsOrTouches(address, length, fragment)) {
                overlapping.Add(fragment);
            }
        }

        overlapping = overlapping.OrderBy(f => f.Address).ToList();
        foreach (Fragment frag in overlapping) {
            this.myFragments.Remove(frag);
        }

        ulong start = overlapping.Count != 0 ? Math.Min(address, overlapping[0].Address) : address;
        ulong end = overlapping.Count != 0 ? Math.Max(address + length, overlapping[overlapping.Count - 1].End) : address + length;
        byte[] mergedData = new byte[end - start];

        foreach (Fragment f in overlapping) {
            f.Data.CopyTo(mergedData.AsSpan((int) (f.Address - start)));
        }

        buffer.CopyTo(mergedData.AsSpan((int) (address - start)));

        this.myFragments.Add(new Fragment(start, mergedData));
        this.myFragments = this.myFragments.OrderBy(f => f.Address).ToList();
    }

    public void Clear(ulong offset, ulong count) {
        if (count == 0)
            return;
        if (Maths.WillOverflow(offset, count))
            count = ulong.MaxValue - offset;
        
        ulong end = offset + count;
        List<Fragment> keepFrags = new List<Fragment>();
        foreach (Fragment f in this.myFragments) {
            ulong fragmentEnd = f.End;
            if (fragmentEnd <= offset || f.Address >= end) {
                // completely out of range, so keep the fragment
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
                f.Data.CopyTo(leftData, leftLen);
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

    public int Read(ulong offset, Span<byte> buffer) {
        // foreach ((ulong bufOffset, byte[] bufData) in this.cachedMemory) {
        //     ulong bufEnd = bufOffset + (ulong) bufData.Length;
        //
        //     // No overlap
        //     if (offset >= bufEnd || offset + (ulong) buffer.Length <= bufOffset)
        //         continue;
        //
        //     // Compute intersection
        //     ulong start = Math.Max(offset, bufOffset);
        //     ulong end = Math.Min(offset + (ulong) buffer.Length, bufEnd);
        //
        //     int copyStart = (int) (start - bufOffset);
        //     int destStart = (int) (start - offset);
        //     int length = (int) (end - start);
        //
        //     bufData.AsSpan(copyStart, length).CopyTo(buffer.Slice(destStart, length));
        //     bytesRead += length;
        // }


        if (buffer.Length == 0) {
            return 0;
        }

        ulong readStart = offset;
        ulong readEnd = offset + (ulong) buffer.Length;
        if (readEnd < offset)
            readEnd = ulong.MaxValue;

        int totalCopied = 0;
        foreach (Fragment frag in this.myFragments) {
            ulong fragStart = frag.Address;
            ulong bufEnd = frag.End;
            if (bufEnd > readStart && fragStart < readEnd) {
                ulong start = Math.Max(readStart, fragStart);
                ulong end = Math.Min(readEnd, bufEnd);
                int length = (int) (end - start);
                int srcIndex = (int) (start - fragStart);
                int destIndex = (int) (start - readStart);

                frag.Data.AsSpan(srcIndex, length).CopyTo(buffer.Slice(destIndex, length));
                totalCopied += length;
            }
        }

        return totalCopied;
    }

    public override string ToString() {
        return $"Fragments: {string.Join(", ", this.myFragments.Select(f => $"[{f.Address}-{f.End})"))}";
    }

    private class Fragment {
        public readonly ulong Address;
        public readonly byte[] Data;
        public ulong End => this.Address + (ulong) this.Data.Length;

        public Fragment(ulong address, byte[] data) {
            this.Address = address;
            this.Data = data;
        }

        public static bool OverlapsOrTouches(ulong address, ulong count, Fragment other) {
            return !((address + count) < other.Address || other.End < address);
        }
    }
}