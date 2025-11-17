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

using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace PFXToolKitUI.Utils.Ranges;

public readonly struct IntegerRange<T> : IEquatable<IntegerRange<T>> where T : unmanaged, IBinaryInteger<T>, IMinMaxValue<T> {
    public static IntegerRange<T> Empty => default;

    /// <summary>
    /// Gets the start location of the range.
    /// </summary>
    public T Start { get; }

    /// <summary>
    /// Gets the exclusive end location of the range.
    /// </summary>
    public T End { get; }

    /// <summary>
    /// Gets the total number of bytes that the range spans.
    /// </summary>
    public T Length {
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
    /// Creates a new T range.
    /// </summary>
    /// <param name="start">The start location.</param>
    /// <param name="end">The (exclusive) end location.</param>
    public IntegerRange(T start, T end) {
        this.Start = start;
        this.End = end;
    }

    public bool Contains(T location) => location >= this.Start && location < this.End;

    /// <summary>
    /// Returns true when the current instance contains the entirety of the other range
    /// </summary>
    /// <param name="other">The other range</param>
    /// <returns>True if the other is entirely within the current instance</returns>
    public bool Contains(IntegerRange<T> other) => other.IsEmpty || (this.Start <= other.Start && other.End <= this.End);

    /// <summary>
    /// Returns true when the other range overlaps this range. Returns false when the current instance or <see cref="other"/> is empty 
    /// </summary>
    /// <param name="other">The other range</param>
    /// <returns>True if the other overlaps the current instance</returns>
    public bool Overlaps(IntegerRange<T> other) {
        return this.Start < other.End && other.Start < this.End && !this.IsEmpty && !other.IsEmpty;
    }

    public IntegerRange<T> Clamp(IntegerRange<T> range) {
        T start = T.Max(range.Start, this.Start);
        T end = T.Min(range.End, this.End);
        return start > end ? default : new IntegerRange<T>(start, end);
    }

    /// <summary>
    /// Splits this range into two, with no gap inbetween
    /// </summary>
    /// <param name="location">The splitting location</param>
    /// <param name="left">The left split</param>
    /// <param name="right">The right split</param>
    /// <exception cref="ArgumentOutOfRangeException">The location is not within the bounds of the current instance</exception>
    public void Split(T location, out IntegerRange<T> left, out IntegerRange<T> right) {
        if (!this.Contains(location))
            throw new ArgumentOutOfRangeException(nameof(location));

        left = new IntegerRange<T>(this.Start, location);
        right = new IntegerRange<T>(location, this.End);
    }

    public bool Equals(IntegerRange<T> other) => this.Start == other.Start && this.End == other.End;

    public override bool Equals(object? obj) => obj is IntegerRange<T> other && this.Equals(other);

    public override int GetHashCode() => HashCode.Combine(this.Start, this.End);

    public override string ToString() => this.IsEmpty ? $"[{this.Start} (empty)]" : $"[{this.Start} -> {unchecked(this.End - T.One)}]";

    public static bool operator ==(IntegerRange<T> left, IntegerRange<T> right) => left.Equals(right);

    public static bool operator !=(IntegerRange<T> left, IntegerRange<T> right) => !left.Equals(right);
}

/// <summary>
/// A helper class for creating <see cref="IntegerRange{T}"/> instances with prechecks
/// </summary>
public static class IntegerRange {
    public static IntegerRange<T> FromStartAndEnd<T>(T start, T end) where T : unmanaged, IBinaryInteger<T>, IMinMaxValue<T> {
        ArgumentOutOfRangeException.ThrowIfLessThan(end, start);
        return new IntegerRange<T>(start, end);
    }

    public static IntegerRange<T> FromStartAndLength<T>(T start, T length) where T : unmanaged, IBinaryInteger<T>, IMinMaxValue<T> {
        ArgumentOutOfRangeException.ThrowIfNegative(length);
        if (start > T.MaxValue - length)
            ThrowAdditionOverflows();

        return new IntegerRange<T>(start, unchecked(start + length));
    }

    public static IntegerRange<T> FromStartAndLengthClamped<T>(T start, T length) where T : unmanaged, IBinaryInteger<T>, IMinMaxValue<T> {
        ArgumentOutOfRangeException.ThrowIfNegative(length);
        return new IntegerRange<T>(start, start > T.MaxValue - length ? T.MaxValue : unchecked(start + length));
    }

    [DoesNotReturn]
    private static void ThrowAdditionOverflows() {
        throw new ArgumentException($"start+length overflows");
    }
}