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

using System.Runtime.CompilerServices;

namespace PFXToolKitUI.Utils;

public readonly struct ULongRange : IEquatable<ULongRange> {
    /// <summary>
    /// Gets the start location of the range.
    /// </summary>
    public readonly ulong Start;

    /// <summary>
    /// Gets the exclusive end location of the range.
    /// </summary>
    public readonly ulong End;

    /// <summary>
    /// Gets the total number of bytes that the range spans.
    /// </summary>
    public ulong Length {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this.End - this.Start;
    }

    /// <summary>
    /// Gets a value indicating whether the range is empty or not.
    /// </summary>
    public bool IsEmpty {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this.Start == this.End;
    }

    /// <summary>
    /// Creates a new ulong range.
    /// </summary>
    /// <param name="start">The start location.</param>
    /// <param name="end">The (exclusive) end location.</param>
    private ULongRange(ulong start, ulong end) {
        this.Start = start;
        this.End = end;
        this.Validate();
    }

    /// <summary>
    /// Validates the state of this range.
    /// </summary>
    /// <exception cref="InvalidOperationException">The length is negative or start+length results in overflow</exception>
    public void Validate() {
        if (this.End < this.Start)
            throw new InvalidOperationException("Negative length");
    }

    public static ULongRange FromStartAndEnd(ulong start, ulong end) => new ULongRange(start, end);

    public static ULongRange FromStartAndLength(ulong start, ulong length) {
        if (Maths.WillAdditionOverflow(start, length))
            throw new InvalidOperationException("Overflow: start+length exceeds MaxValue");
        return new ULongRange(start, unchecked(start + length));
    }
    
    public bool Contains(ulong location) => location >= this.Start && location < this.End;

    public bool IsSuperSet(ULongRange other) => this.Contains(other.Start) && this.Contains(other.End == 0 ? 0 : (other.End - 1));

    public bool Overlaps(ULongRange other) {
        return this.Contains(other.Start) ||
               this.Contains(other.End == 0 ? 0 : (other.End - 1)) ||
               other.Contains(this.Start) ||
               other.Contains(this.End == 0 ? 0 : (this.End - 1));
    }

    public ULongRange Clamp(ULongRange range) {
        ulong start = Math.Max(range.Start, this.Start);
        ulong end = Math.Min(range.End, this.End);
        return start > end ? default : FromStartAndEnd(start, end);
    }

    public (ULongRange, ULongRange) Split(ulong location) {
        if (!this.Contains(location))
            throw new ArgumentOutOfRangeException(nameof(location));
        return (FromStartAndEnd(this.Start, location), FromStartAndEnd(location, this.End));
    }

    public bool Equals(ULongRange other) => this.Start.Equals(other.Start) && this.End.Equals(other.End);

    public override bool Equals(object? obj) => obj is ULongRange other && this.Equals(other);

    public override int GetHashCode() => (this.Start.GetHashCode() * 397) ^ this.End.GetHashCode();

    public override string ToString() => $"[{this.Start} -> {this.End - 1}]";

    public static bool operator ==(ULongRange left, ULongRange right) => left.Equals(right);

    public static bool operator !=(ULongRange left, ULongRange right) => !left.Equals(right);
}